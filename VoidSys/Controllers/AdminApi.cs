using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VoidSys.Modles;
using VoidSys.Service;

namespace VoidSys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors]
    [Authorize]
    public class AdminApi : ControllerBase
    {
        private readonly Set _set;
        private readonly IConfiguration _configuration;

        public AdminApi(Set set, IConfiguration configuration)
        {
            _set = set;
            _configuration = configuration;
        }

        /// <summary>
        /// 管理员登录
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("login")]
        [AllowAnonymous]
        async public Task<Result<v_AdminRes>> loginAd(v_Admin data)
        {
            return await AdminSve.login(data,_set,_configuration);
        }

        /// <summary>
        /// 注册账户
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("register")]
        async public Task<Result<bool>> register(v_Admin data)
        {
            string header = Request.Headers["Authorization"];
            return await AdminSve.register(header, _set, data);
        }

        /// <summary>
        /// 修改轮播图
        /// </summary>
        [HttpPost("changeBanner")]
        async public Task<Result<bool>> changeBanner(List<int> data)
        {
            string header = Request.Headers["Authorization"];
            return await AdminSve.changeRoll(header, _set, data);
        }

        /// <summary>
        /// 修改博客
        /// </summary>
        [HttpPut("changePage")]
        async public Task<Result<bool>> changePage(v_Page data)
        {
            return await AdminSve.changePage(_set, data);
        }

        [HttpDelete("deletePage")]
        async public Task<Result<bool>> deletePage(int pid)
        {
            return await Task.Run(() =>
            {
                _set.v_page.Remove(_set.v_page.Where(i=>i.pid==pid).FirstOrDefault());
                _set.SaveChanges();
                return new Result<bool>() { data = true };
            });
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        [HttpGet("connect")]
        [AllowAnonymous]
        async public Task connect()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using WebSocket websocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                //Console.WriteLine("One User connecting");
                await Echo(websocket);
            }
            else
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }

        /// <summary>
        /// 获取用户数量
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        async private Task Echo(WebSocket socket)
        {
            var buffer = new byte[1024 * 4];
            var res = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            //Console.WriteLine("Log::One Admin connect");
            while (!res.CloseStatus.HasValue)
            {
                var msg = new JObject(
                    new JProperty("num", UserApi.webSockets.Count.ToString())
                    );
                try
                {
                    DateTime now = DateTime.Now;
                    string nowStr = now.Year + "-" + now.Month + "-" + now.Day;
                    v_DayHeight d = _set.v_dayheight.Where(i => i.dateTime == nowStr).FirstOrDefault();
                    if (d == null)
                    {
                        d = new v_DayHeight();
                        d.dateTime = nowStr;
                        d.num = UserApi.webSockets.Count;
                        _set.v_dayheight.Add(d);
                    }
                    else
                    {
                        if (UserApi.webSockets.Count > d.num)
                        {
                            d.num = UserApi.webSockets.Count;
                            _set.v_dayheight.Update(d);
                        }
                    }
                    _set.SaveChanges();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                var serverMsg = Encoding.UTF8.GetBytes(msg.ToString());
                await socket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), res.MessageType, res.EndOfMessage, CancellationToken.None);
                res = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                Thread.Sleep(2500);
            }
            await socket.CloseAsync(res.CloseStatus.Value, res.CloseStatusDescription, CancellationToken.None);
            //Console.WriteLine("Log::One Admin out now");
        }

        /// <summary>
        /// 获取员工数量
        /// </summary>
        /// <returns></returns>
        [HttpGet("getAdminList")]
        async public Task<ResultPage<List<v_Admin>>> getAdminList(int page, int size ,string name, string vid)
        {
            return await Task.Run(() =>
            {
                List<v_Admin> list = null;
                if (vid == ""||vid == null)
                {
                    if(name == null)
                    {
                        name = "";
                    }
                     list = _set.v_admins
                    .Where(i => i.name.Contains(name))
                    .ToList();
                }
                else
                {
                    list = _set.v_admins
                    .Where(i => i.vid == vid)
                    .ToList();
                }
                return new ResultPage<List<v_Admin>>()
                {
                    page = page,
                    limt = (int)Math.Ceiling(list.Count()*1.0/size),
                    data = list.Skip((page - 1) * size).Take(size).ToList()
                };
            });
        }

        /// <summary>
        /// 获取主页信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("MainData")]
        [AllowAnonymous]
        async public Task<Result<MainData>> MonthData()
        {
            var task = Task.Run(() =>
            {
                
                Result<MainData> result = new Result<MainData>();
                result.data = new MainData();
                result.data.userNum = _set.v_users.Count();
                result.data.adminNum = _set.v_admins.Count();
                result.data.wordNum = _set.v_page.Count();
                result.data.dayHeight = _set.v_dayheight.OrderByDescending(i=>i.dhid).Take(7).ToList();
                result.data.dayHeight.Reverse();
                return result;
            });
            return await task;
        }

        /// <summary>
        /// 修改置顶文章列表
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        [HttpPut("changeTop")]
        async public Task<Result<bool>> ChangeData(List<int> list)
        {
            return await Task.Run(() =>
            {
                List<v_Top> tops = new List<v_Top>();
                for (int i = 0; i < list.Count; i++)
                {
                    v_Top top = _set.v_top.Find(i + 1);
                    top.pid = list[i];
                    _set.v_top.Update(top);
                }
                _set.SaveChanges();
                return new Result<bool>
                {
                    data = true
                };
            });
        }

        /// <summary>
        /// 修改用户信息
        /// </summary>
        /// <param name="usersData"></param>
        /// <returns></returns>
        [HttpPut("changeUser")]
        async public Task<Result<bool>> changeUser(v_UsersData usersData)
        {
            return await Task.Run(() =>
            {
                Result<bool> result = new Result<bool>();
                _set.v_usersData.Update(usersData);
                if (_set.SaveChanges() > 0)
                {
                    result.data = true;
                }
                return result; 
            });
        }

        /// <summary>
        /// 将用户封禁
        /// </summary>
        /// <param name="vid"></param>
        /// <returns></returns>
        [HttpPut("falseUser")]
        async public Task<Result<bool>> falseUser(string vid)
        {
            return await Task.Run(() =>
            {
                Result<bool> res = new Result<bool>();
                _set.v_users.Find(vid).pass = !_set.v_users.Find(vid).pass;
                if (_set.SaveChanges() > 0)
                {
                    res.data = true;
                }
                return res;
            });
        }

        /// <summary>
        /// 发送系统通知
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        [HttpPost("addSysPost")]
        async public Task<Result<bool>> addSysPost(v_Post post)
        {
            return await Task.Run(() =>
            {
                Result<bool> result = new Result<bool>();
                string header = HttpContext.Request.Headers["Authorization"];
                string vid = header.getVid();
                _set.v_post.Add(post);
                if (_set.SaveChanges() > 0)
                {
                    result.data = true;
                }
                return result;
            });
        }

        [HttpPut("readedRe")]
        async public Task<Result<bool>> readedRe(int tid,int rid)
        {
            return await Task.Run(() =>
            {
                if (tid == 0)
                {
                    var data = _set.v_return.Find(rid);
                    data.readed = true;
                    _set.v_return.Update(data);
                }
                else
                {
                    var data = _set.v_tsumi_page.Find(tid);
                    data.readed = true;
                    _set.v_tsumi_page.Update(data);
                }
                _set.SaveChanges();
                return new Result<bool>()
                {
                    data = true
                };
            });
        }

        /// <summary>
        /// 删除管理员
        /// </summary>
        /// <param name="vid"></param>
        /// <returns></returns>
        [HttpDelete("deleteAdmin")]
        async public Task<Result<bool>> deleteAdmin(string vid)
        {
            return await Task.Run(() =>
            {
                _set.v_admins.Remove(_set.v_admins.Find(vid));
                _set.SaveChanges();
                return new Result<bool>()
                {
                    data = true
                };
            });
        }

        [HttpGet("getLoginData")]
        async public Task<ResultPage<List<LoginDataRes>>> getLoginData(int page,int size,int mode,string vid)
        {
            return await Task.Run(() =>
            {
                ResultPage<List<LoginDataRes>> res = new ResultPage<List<LoginDataRes>>();
                List<LoginData> list = new List<LoginData>();
                if (vid != null)
                {
                    list = _set.loginData.Where(i => i.vid == vid).OrderByDescending(i=>i.loginTime).ToList();
                }
                else
                {
                    list = _set.loginData.OrderByDescending(i => i.loginTime).ToList();
                }
                switch (mode)
                {
                    case 1:
                        list = list.Where(i => DateTimeOffset.FromUnixTimeMilliseconds(i.loginTime) > DateTime.Today).ToList();
                        break;
                    case 2:
                        list = list.Where(i => DateTimeOffset.FromUnixTimeMilliseconds(i.loginTime) > DateTime.Today.AddDays(-3)).ToList();
                        break;
                    case 3:
                        list = list.Where(i => DateTimeOffset.FromUnixTimeMilliseconds(i.loginTime) > DateTime.Today.AddDays(-7)).ToList();
                        break;
                    case 4:
                        list = list.Where(i => DateTimeOffset.FromUnixTimeMilliseconds(i.loginTime) > DateTime.Today.AddDays(-30)).ToList();
                        break;
                }
                res.total = list.Count;
                res.limt = (int)Math.Ceiling(list.Count * 1.0 / size);
                res.data = list.Skip((page - 1) * size).Take(size).Select(i=>
                    new LoginDataRes()
                    {
                        id = i.id,
                        loginIP = i.loginIP,
                        loginTime = i.loginTime,
                        userData = _set.v_usersData.Where(i2=>i2.vid == i.vid ).FirstOrDefault(),
                        vid = i.vid,
                        address = i.address
                    }
                ).ToList();
                return res;
            });
        }
    }
}
