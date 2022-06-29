using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VoidSys.Modles;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Cors;
using Newtonsoft.Json.Linq;
using VoidSys.Service;

namespace VoidSys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableCors]
    public class LinkApi : ControllerBase
    {
        private readonly Set _set;

        public static Dictionary<string, WebSocket> sokets = new Dictionary<string, WebSocket>();

        public LinkApi(Set set)
        {
            _set = set;
        }

        /// <summary>
        /// 连接聊天服务器
        /// </summary>
        /// <param name="vid"></param>
        /// <returns></returns>
        [HttpGet("connect")]
        [AllowAnonymous]
        async public Task connect(string vid)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using WebSocket websocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await Echo(websocket, vid);
            }
            else
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }

        /// <summary>
        /// 用户连接
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        async private Task Echo(WebSocket socket, string vid)
        {
            var buffer = new byte[1024 * 4];
            var res = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (sokets.ContainsKey(vid))
            {
                Console.WriteLine(vid + "：正在更新当前聊天服务连接" + "---" + DateTime.Now.ToString("G"));
                sokets[vid] = socket;
                Console.WriteLine(vid + "：聊天服务连接已更新" + "---" + DateTime.Now.ToString());
            }
            else
            {
                sokets.Add(vid, socket);
                Console.WriteLine(vid + "：聊天服务连接成功" + "---" + DateTime.Now.ToString());
            }
            while (!res.CloseStatus.HasValue)
            {
                try
                {
                    var serverMsg = Encoding.UTF8.GetBytes("1");
                    await socket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), res.MessageType, res.EndOfMessage, CancellationToken.None);
                    res = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    Thread.Sleep(2500);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                    break;
                }
            }
            try
            {
                await socket.CloseAsync(res.CloseStatus.Value, res.CloseStatusDescription, CancellationToken.None);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// 向目标发送信息
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        [HttpPost("toSend")]
        async public Task<Result<bool>> toSend(SendData data)
        {
            return await Task.Run(async () =>
            {
                string header = HttpContext.Request.Headers["Authorization"];
                string vid = header.getVid();
                Result<bool> result = new Result<bool>();
                long time = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                _set.v_letter.Add(new v_Letter()
                {
                    time = time,
                    content = data.content,
                    fid = data.fid,
                    vid = vid,
                    readed = false
                });
                if (_set.SaveChanges() > 0)
                {
                    result.data = true;
                }
                if (result.data&&sokets.ContainsKey(data.fid))
                {
                    Console.WriteLine("正在发送信息给" + data.fid);
                    try
                    {
                        var msg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new
                        {
                            time = time,
                            content = data.content,
                            vid = vid
                        }));
                        await sokets[data.fid].SendAsync(new ArraySegment<byte>(msg, 0, msg.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                        Console.WriteLine("已发送信息给" + data.fid);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                return result;
            });
        }

        /// <summary>
        /// 将自己与对方的未读消息全部转为已读
        /// </summary>
        /// <param name="vid"></param>
        /// <returns></returns>
        [HttpPut("readedLetter")]
        async public Task<Result<bool>> readed(string vid)
        {
            return await Task.Run(() =>
            {
                Result<bool> result = new Result<bool>();
                string header = HttpContext.Request.Headers["Authorization"];
                string fid = header.getVid();
                //系统消息
                if (vid == "-1")
                {
                    var list_post = _set.v_post.Where(i => i.vid == fid&& !i.readed).ToList();
                    list_post.ForEach(i =>
                    {
                        i.readed = true;
                    });
                    _set.v_post.UpdateRange(list_post);
                    _set.SaveChanges();
                    return result;
                }
                //回复
                else if(vid == "-2")
                {
                    var list_re = _set.v_resay.Where(i => i.toid == fid&&!i.readed).ToList();
                    list_re.ForEach(i =>
                    {
                        i.readed = true;
                    });
                    _set.v_resay.UpdateRange(list_re);
                    _set.SaveChanges();
                    return result;
                }
                //评论
                else if(vid == "-3")
                {
                    var list_say = _set.v_say.Where(i =>_set.v_page.Where(i2=>i2.pid == i.pid).FirstOrDefault().vid == fid&& !i.readed).ToList();
                    list_say.ForEach(i =>
                    {
                        i.readed = true;
                    });
                    _set.v_say.UpdateRange(list_say);
                    _set.SaveChanges();
                    return result;
                }
                
                var list = _set.v_letter.Where(i => i.vid == vid && i.fid == fid && !i.readed).ToList();
                list.ForEach(i =>
                {
                    i.readed = true;
                });
                _set.UpdateRange(list);
                _set.SaveChanges();
                result.data = true;
                return result;
            });
        }

        /// <summary>
        /// 获取与目标的私信列表
        /// </summary>
        /// <param name="vid"></param>
        /// <returns></returns>
        [HttpGet("getLetter")]
        async public Task<ResultPage<List<v_Letter>>> toData(string vid,int page,int size)
        {
            return await Task.Run(() =>
            {
                string header = HttpContext.Request.Headers["Authorization"];
                string fid = header.getVid();
                ResultPage<List<v_Letter>> result = new ResultPage<List<v_Letter>>();
                result.page = page;
                result.data = _set.v_letter.Where(i => i.vid == fid && i.fid == vid).ToList();
                result.data.AddRange(_set.v_letter.Where(i => i.vid == vid && i.fid == fid).ToList());
                result.limt = (int)Math.Ceiling(result.data.Count() * 1.0 / size);
                result.data = result.data.OrderByDescending(i => i.time).ToList().Skip((page - 1) * size).Take(size).ToList();
                result.data.Reverse();
                return result;
            });
        }

        /// <summary>
        /// 获取关注用户列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("getLikeUsers")]
        async public Task<ResultPage<List<v_UsersData>>> getLikeUsers(int page,int size)
        {
            return await Task.Run(() =>
            {
                string header = HttpContext.Request.Headers["Authorization"];
                string vid = header.getVid();
                ResultPage<List<v_UsersData>> result = new ResultPage<List<v_UsersData>>();
                result.total = _set.v_friends.Where(i => i.vid_me == vid).Count();
                result.page = page;
                result.limt = (int)Math.Ceiling(result.total * 1.0 / size);
                result.data = _set.v_friends.Where(i => i.vid_me == vid)
                .OrderByDescending(i => i.fid).ToList()
                .Skip((page - 1) * size)
                .Take(size)
                .Select(i=> _set.v_usersData.Where(a => a.vid == i.vid_friend).FirstOrDefault())
                .ToList();
                return result;
            });
        }

        /// <summary>
        /// 按vid查询关注列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="vid"></param>
        /// <returns></returns>
        [HttpGet("getLikeUsersFind")]
        [AllowAnonymous]
        async public Task<ResultPage<List<v_UsersData>>> getLikeUsers(int page, int size,string vid)
        {
            return await Task.Run(() =>
            {
                ResultPage<List<v_UsersData>> result = new ResultPage<List<v_UsersData>>();
                result.page = page;
                result.total = _set.v_friends.Where(i => i.vid_me == vid).Count();
                result.limt = (int)Math.Ceiling(result.total * 1.0 / size);
                result.data = _set.v_friends.Where(i => i.vid_me == vid)
                .OrderByDescending(i => i.fid).ToList()
                .Skip((page - 1) * size)
                .Take(size)
                .Select(i => _set.v_usersData.Where(a => a.vid == i.vid_friend).FirstOrDefault())
                .ToList();
                return result;
            });
        }

        /// <summary>
        /// 获取私信我的用户
        /// </summary>
        /// <returns></returns>
        [HttpGet("getLetterMe")]
        async public Task<ResultPage<List<v_UsersData>>> getLetterMe(int page,int size)
        {
            return await Task.Run(() =>
            {
                string header = HttpContext.Request.Headers["Authorization"];
                string vid = header.getVid();
                ResultPage<List<v_UsersData>> result = new ResultPage<List<v_UsersData>>();
                result.total = _set.v_letter.Where(i => i.fid == vid).GroupBy(i => i.vid).Count();
                result.page = page;
                result.limt = (int)Math.Ceiling(result.total * 1.0 / size);
                result.data = _set.v_letter
                .Where(i => i.fid == vid &&
                    _set.v_friends
                    .Where(it => vid == it.vid_me && i.vid == it.vid_friend)
                    .Count() == 0)
                .OrderByDescending(i => i.time).ToList()
                .GroupBy(i => i.vid)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(i => _set.v_usersData.Where(a => a.vid == i.Key).FirstOrDefault())
                .ToList();
                return result;
            });
        }

        /// <summary>
        /// 获取最后一条数据
        /// </summary>
        /// <param name="vid"></param>
        /// <returns></returns>
        [HttpGet("getLastLetter")]
        async public Task<Result<v_Letter>> getLastLetter(string vid)
        {
            return await Task.Run(() =>
            {
                string header = HttpContext.Request.Headers["Authorization"];
                string fid = header.getVid();
                Result<v_Letter> result = new Result<v_Letter>();
                var list = _set.v_letter.Where(i => i.vid == fid && i.fid == vid).ToList();
                list.AddRange(_set.v_letter.Where(i => i.vid == vid && i.fid == fid).ToList());
                result.data = list.OrderByDescending(i => i.time).FirstOrDefault();
                return result;
            });
        }

        /// <summary>
        /// 获取目标与我的未读消息数量
        /// </summary>
        /// <param name="vid"></param>
        /// <returns></returns>
        [HttpGet("getLetterNum")]
        async public Task<Result<int>> getLetterNum(string vid)
        {
            return await Task.Run(() =>
            {
                string header = HttpContext.Request.Headers["Authorization"];
                string vid_me = header.getVid();
                Result<int> result = new Result<int>();
                result.data = _set.v_letter.Where(i => i.fid == vid_me && i.vid == vid&&!i.readed).Count();
                return result;
            });
        }

        /// <summary>
        /// 获取未读消息数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("getMsgData")]
        async public Task<Result<ResData>> getMsg()
        {
            var task = Task.Run(() =>
            {
                int sayNum = 0;
                int resayNum = 0;
                int letterNum = 0;
                int postNum = 0;
                string header = HttpContext.Request.Headers["Authorization"];
                string vid = header.getVid();
                //获取用户发布的文章有多少评论
                sayNum = _set.v_page.Where(i => i.vid == vid).Select(i=>_set.v_say
                        .Where(it => it.pid == i.pid && !it.readed)
                        .Count()).Sum();
                
                //获取用户有多少未读回复
                resayNum += _set.v_resay
                    .Where(it => it.toid == vid&& !it.readed)
                    .Count();
                //获取是否有人发送私信
                letterNum = _set.v_letter.Where(i => i.fid == vid && !i.readed).Count();
                //获取未读系统消息数
                postNum = _set.v_post.Where(i => i.vid == vid && !i.readed).Count();
                var msg = new ResData()
                {
                    newSay = sayNum,
                    newReSay = resayNum,
                    newLetter = letterNum,
                    newPost = postNum,
                    allConut = sayNum+resayNum+letterNum+postNum
                };
                Result<ResData> result = new Result<ResData>();
                result.data = msg;
                return result;
            });
            return await task;
        }

        /// <summary>
        /// 获取系统通知
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet("getPostMe")]
        async public Task<ResultPage<List<v_Post>>> getPostMe(int page, int size)
        {
            return await Task.Run(() =>
            {
                string header = HttpContext.Request.Headers["Authorization"];
                string vid = header.getVid();
                ResultPage<List<v_Post>> result = new ResultPage<List<v_Post>>();
                result.total = _set.v_post.Where(i => i.vid == vid).Count();
                result.page = page;
                result.limt = (int)Math.Ceiling(result.total * 1.0 / size);
                result.data = _set.v_post
                .Where(i => i.vid == vid)
                .OrderByDescending(i => i.time)
                .Skip((page - 1) * size)
                .Take(size)
                .ToList();
                result.data.Reverse();
                return result;
            });
        }

        /// <summary>
        /// 获取回复
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet("getReMe")]
        async public Task<ResultPage<List<resResay>>> getReMe(int page, int size)
        {
            return await Task.Run(() =>
            {
                string header = HttpContext.Request.Headers["Authorization"];
                string vid = header.getVid();
                ResultPage<List<resResay>> result = new ResultPage<List<resResay>>();
                result.total = _set.v_resay.Where(i => i.toid == vid).Count();
                result.page = page;
                result.limt = (int)Math.Ceiling(result.total * 1.0 / size);
                result.data = _set.v_resay
                .Where(i => i.toid == vid)
                .OrderByDescending(i => i.time).ToList()
                .Skip((page - 1) * size)
                .Take(size).ToList()
                .Select(i=> new resResay() {
                    sayid = i.sayid,
                    sid = i.sid,
                    content = i.content,
                    reContent = i.reContent,
                    pageData = _set.v_page.Find(_set.v_say.Where(it=>it.sayid==i.sid).FirstOrDefault().pid),
                    sayData = _set.v_say.Find(i.sid),
                    readed = i.readed,
                    time = i.time,
                    toid = i.toid,
                    vid = i.vid,
                    userName = _set.v_usersData.Find(i.vid).userName
                }).ToList();
                result.data.Reverse();
                return result;
            });
        }

        /// <summary>
        /// 获取评论
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet("getSayMe")]
        async public Task<ResultPage<List<reSay>>> getSayMe(int page, int size)
        {
            return await Task.Run(() =>
            {
                string header = HttpContext.Request.Headers["Authorization"];
                string vid = header.getVid();
                ResultPage<List<reSay>> result = new ResultPage<List<reSay>>();
                result.total = _set.v_say.Where(i => _set.v_page.Where(i2 => i2.pid == i.pid).FirstOrDefault().vid == vid).Count();
                result.page = page;
                result.limt = (int)Math.Ceiling(result.total * 1.0 / size);
                result.data = _set.v_say
                .Where(i => _set.v_page.Where(i2 => i2.pid == i.pid).FirstOrDefault().vid == vid)
                .OrderByDescending(i => i.time).ToList()
                .Skip((page - 1) * size)
                .Take(size)
                .ToList().Select(i => new reSay()
                {
                    sayid = i.sayid,
                    content = i.content,
                    pid = i.pid,
                    pageData = _set.v_page.Find(i.pid),
                    time = i.time,
                    userName = _set.v_usersData.Find(i.vid).userName,
                    readed = i.readed,
                    vid = i.vid
                }).ToList();
                result.data.Reverse();
                return result;
            });
        }
    }
}
