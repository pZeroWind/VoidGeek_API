using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VoidSys.Modles;
using VoidSys.Service;

namespace VoidSys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableCors]
    public class UserApi : ControllerBase
    {
        private readonly Set _set;
        private readonly IConfiguration _configuration;
        public static List<WebSocket> webSockets = new List<WebSocket>();
        public UserApi(Set set,IConfiguration configuration)
        {
            _set = set;
            _configuration = configuration;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("connect")]
        async public Task connect()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                
                using WebSocket websocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                string vid = HttpContext.Connection.RemoteIpAddress.ToString();
                await Echo(websocket,vid);
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
        async private Task Echo(WebSocket socket,string vid)
        {
            var buffer = new byte[1024 * 4];
            var res = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            webSockets.Add(socket);
            Console.WriteLine(vid + "：连接成功" + "---" + DateTime.Now.ToString());
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
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            webSockets.Remove(socket);
            Console.WriteLine(vid + "：连接已断开" + "---" + DateTime.Now.ToString());
        }

        /// <summary>
        /// 获取个人信息
        /// </summary>
        [HttpGet("user")]
        async public Task<Result<v_UsersData>> getMyData()
        {
            string header = HttpContext.Request.Headers["Authorization"];
            return await UserSve.getData(header,_set);
        }

        /// <summary>
        /// 查找其他用户
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        [HttpGet("find")]
        [AllowAnonymous]
        async public Task<Result<v_UsersData>> getUserData(string vid)
        {
            return await UserSve.findUser(vid, _set);
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        [HttpPost("login")]
        [AllowAnonymous]
        async public Task<Result<string>> login(v_Users users)
        {
            return await Task.Run(async () =>
            {
                _set.Database.BeginTransaction();
                Result<string> result = new Result<string>();
                if (_set.v_users.Find(users.vid).pass)
                {
                    if (await UserSve.login(users, _set))
                    {
                        result.data = await UserSve.token(users, _configuration);
                        string ip = HttpContext.Connection.RemoteIpAddress.ToString();
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://restapi.amap.com/v3/ip?ip={ip}&key=4317380dfc896ba8789017710376d38c");
                        request.Method = "GET";
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        Stream stream = response.GetResponseStream();
                        StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                        JObject json = JObject.Parse(reader.ReadToEnd());
                        _set.loginData.Add(new LoginData()
                        {
                            vid = users.vid,
                            loginTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                            loginIP = ip,
                            address = json.GetValue("province").ToString()+json.GetValue("city").ToString()
                        });
                        _set.SaveChanges();
                        _set.Database.CommitTransaction();
                    }
                    else
                    {
                        result.code = 500;
                        result.msg = "VID或密码错误";
                        result.data = "";
                        _set.Database.RollbackTransaction();
                    }
                }
                else
                {
                    result.code = 401;
                    result.msg = "账号已被封禁";
                    result.data = "";
                    _set.Database.RollbackTransaction();
                }
                return result;
            });
            
        }

        /// <summary>
        /// 注册用户
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("register")]
        async public Task<Result<v_Users>> register(v_Rigster data)
        {
            Result<v_Users> result = await UserSve.register(data,_set);
            return result;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <returns></returns>
        [HttpPost("updateFile")]
        [AllowAnonymous]
        async public Task<Result<List<string>>> update()
        {
            IFormFileCollection files = Request.Form.Files;
            return await UserSve.updateFile(files);
        }

        /// <summary>
        /// 修改个人信息
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPut("updateData")]
        async public Task<Result<bool>> dataChange(v_UsersData data)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            return await UserSve.updateData(header, data, _set);
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPut("password")]
        async public Task<Result<bool>> password(Password data)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            return await UserSve.updatePWD(header, data, _set);
        }

        /// <summary>
        /// 忘记密码
        /// </summary>
        /// <param name="pwd"></param>
        /// <param name="vid"></param>
        /// <returns></returns>
        [HttpPut("forget")]
        [AllowAnonymous]
        async public Task<Result<bool>> changePwd(string pwd,string vid)
        {
            return await UserSve.cPwd(_set, pwd, vid);
        }

        /// <summary>
        /// 从vid中发送邮件
        /// </summary>
        /// <param name="vid"></param>
        /// <returns></returns>
        [HttpGet("getCodeForVid")]
        [AllowAnonymous]
        async public Task<Result<string>> getCodeForVid(string vid)
        {
            return await UserSve.toEmail(_set,vid);
        }

        /// <summary>
        /// 新增文章
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("addPage")]
        async public Task<Result<bool>> addPage(updatePage data)
        {
            string header = HttpContext.Request.Headers["Authorization"];
            return await UserSve.addPage(header, data, _set);
        }


        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="mode"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet("getUserList")]
        [AllowAnonymous]
        async public Task<ResultPage<List<v_UsersData>>> getUserList(int page, int mode, int size)
        {
            return await UserSve.getUserList(_set, page, mode, size);
        }

        /// <summary>
        /// 检测是否封禁
        /// </summary>
        /// <param name="vid"></param>
        /// <returns></returns>
        [HttpGet("checkUserFalse")]
        async public Task<Result<bool>> checkUserFalse(string vid)
        {
            return await Task.Run(() =>
            {
                return new Result<bool>()
                {
                    data = _set.v_users.Find(vid).pass
                };
            });
        }

        /// <summary>
        /// 记录足迹
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        [HttpPost("AddHistory")]
        async public Task<Result<bool>> historyPage(int pid)
        {
            return await UserSve.addHistory(_set,pid, HttpContext.Request.Headers["Authorization"]);
        }

        /// <summary>
        /// 收藏文章
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        [HttpPost("AddSave")]
        async public Task<Result<bool>> savePage(int pid,int sfid)
        {
            return await UserSve.addSave(_set, pid, sfid, HttpContext.Request.Headers["Authorization"]);
        }

        /// <summary>
        /// 获取用户的收藏夹
        /// </summary>
        /// <returns></returns>
        [HttpGet("getFolder")]
        async public Task<Result<List<SaveFolder>>> getFolder()
        {
            return await UserSve.getSaveFolder(_set, HttpContext.Request.Headers["Authorization"],true);
        }

        [HttpGet("getFolderFind")]
        [AllowAnonymous]
        async public Task<Result<List<SaveFolder>>> getFolder(string vid)
        {
            return await UserSve.getSaveFolder(_set, vid ,false);
        }

        /// <summary>
        /// 添加文件夹
        /// </summary>
        /// <param name="sf"></param>
        /// <returns></returns>
        [HttpPost("addFolder")]
        async public Task<Result<int>> addFolder(SaveFolder sf)
        {
            return await Task.Run(() =>
            {
                string header = HttpContext.Request.Headers["Authorization"];
                Result<int> result = new Result<int>();
                string token = header.Replace("Bearer ", "");
                var handler = new JwtSecurityTokenHandler();
                sf.vid = handler.ReadJwtToken(token).Payload.Sub;
                _set.savefolder.Add(sf);
                if (_set.SaveChanges() > 0)
                {
                    result.data = sf.fsId;
                }
                return result;
            });
        }

        /// <summary>
        /// 删除文件夹
        /// </summary>
        /// <param name="sfid"></param>
        /// <returns></returns>
        [HttpDelete("deleteFolder")]
        async public Task<Result<bool>> deleteFolder(int sfid)
        {
            return await Task.Run(() =>
            {
                Result<bool> result = new Result<bool>();   
                _set.savefolder.Remove(_set.savefolder.Find(sfid));
                if (_set.SaveChanges() > 0)
                {
                    result.data = true;
                }
                return result;
            });
        }

        /// <summary>
        /// 删除收藏
        /// </summary>
        /// <param name="sfid"></param>
        /// <returns></returns>
        [HttpDelete("deleteSave")]
        async public Task<Result<bool>> deleteSave(int pid,int sfid)
        {
            return await Task.Run(() =>
            {
                Result<bool> result = new Result<bool>();
                _set.save.Remove(_set.save.Where(i=>i.pid == pid&&i.folderId==sfid).FirstOrDefault());
                if (_set.SaveChanges() > 0)
                {
                    result.data = true;
                }
                return result;
            });
        }


        /// <summary>
        /// 获取该收藏夹中的文章
        /// </summary>
        /// <param name="sfid"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet("getSavePage")]
        [AllowAnonymous]
        async public Task<ResultPage<List<v_PageRes>>> getSavePage(int sfid, int page)
        {
            return await UserSve.getSavePage(_set,sfid,page);
        }

        /// <summary>
        /// 获取该用户历史记录的前50篇文章
        /// </summary>
        /// <param name="sfid"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet("getHistory")]
        async public Task<Result<List<HisRes>>> getHis()
        {
            return await UserSve.getHis(_set, HttpContext.Request.Headers["Authorization"]);
        }

        /// <summary>
        /// 测试接口,获取md5加密密码
        /// </summary>
        /// <param name="pwd"></param>
        /// <returns></returns>
        [HttpGet("md5")]
        [AllowAnonymous]
        public string getMd5(string pwd)
        {
            return UserSve.md5(pwd);
        }

        /// <summary>
        /// 检测是否已点赞
        /// </summary>
        /// <returns></returns>
        [HttpGet("checkLike")]
        async public Task<Result<bool>> checkLike(int pid)
        {
            return await Task.Run(() =>
            {
                string header = HttpContext.Request.Headers["Authorization"];
                string vid = header.getVid();
                Result<bool> result = new Result<bool>();
                int i = _set.likedata.Where(i => i.vid == vid && i.pid == pid).Count();
                if (i > 0)
                {
                    result.data = true;
                }
                return result;
            });
        }

        /// <summary>
        /// 关注用户
        /// </summary>
        /// <param name="vid"></param>
        /// <returns></returns>
        [HttpPut("likeUser")]
        async public Task<Result<bool>> likeUser(string vid)
        {
            return await UserSve.likeUser(_set, HttpContext.Request.Headers["Authorization"], vid);
        }

        /// <summary>
        /// 检测是否关注
        /// </summary>
        /// <param name="vid"></param>
        /// <returns></returns>
        [HttpGet("checkLikeUser")]
        async public Task<Result<bool>> checkLikeUser(string vid)
        {
            return await Task.Run(() =>
            {
                string header = HttpContext.Request.Headers["Authorization"];
                string me_vid = header.getVid();
                Result<bool> result = new Result<bool>();
                int i = _set.v_friends.Where(i => i.vid_me == me_vid && i.vid_friend == vid).Count();
                if (i > 0)
                {
                    result.data = true;
                }
                return result;
            });
        }

        [HttpGet("checkSave")]
        async public Task<Result<bool>> checkSave(int pid)
        {
            return await Task.Run(() =>
            {
                Result<bool> result = new Result<bool>();
                string header = HttpContext.Request.Headers["Authorization"];
                string me_vid = header.getVid();
                var list = _set.savefolder
                .Where(i => i.vid == me_vid).ToList();
                int con = 0;
                list.ForEach(i =>
                {
                    if(_set.save
                    .Where(it => it.pid == pid&&it.folderId == i.fsId)
                    .FirstOrDefault() != null)
                    {
                        con++;
                    }
                });
                result.data = con > 0;
                return result;
            });
        }
    }

    
}
