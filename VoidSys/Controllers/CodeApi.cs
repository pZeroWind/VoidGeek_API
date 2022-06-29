using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VoidSys.Modles;
using Newtonsoft.Json;
using VoidSys.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Cors;
using System.IO;
using Newtonsoft.Json.Linq;

namespace VoidSys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors]
    public class CodeApi : ControllerBase
    {
        private readonly Set _set;
        private readonly IConfiguration _configuration;

        public CodeApi(Set set,IConfiguration configuration)
        {
            _set = set;
            _configuration = configuration;
        }
        static List<CodeData> KeyList = new List<CodeData>();

        [HttpGet("getCode")]
        async public Task connect()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using WebSocket websocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await Echo(websocket);
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
        async private Task Echo(WebSocket socket)
        {
            var buffer = new byte[1024 * 4];
            var res = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            CodeData code = new CodeData()
            {
                CodeKey = Guid.NewGuid().ToString()
            };
            KeyList.Add(code);
            while (!res.CloseStatus.HasValue)
            {
                try
                {
                    var serverMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(code));
                    await socket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), res.MessageType, res.EndOfMessage, CancellationToken.None);
                    res = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    Thread.Sleep(500);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    break;
                }
            }
            await socket.CloseAsync(res.CloseStatus.Value, res.CloseStatusDescription, CancellationToken.None);
            KeyList.Remove(code);
        }


        [HttpPost("login")]
        async public Task<Result<bool>> login(CodeData users)
        {
            return await Task.Run(async () =>
            {
                Result<bool> result = new Result<bool>();
                var u = _set.v_users.Find(users.userData.vid);
                if(u.pass == false)
                {
                    result.msg = "账号已被封禁无法登录";
                    result.code = 401;
                }
                else
                {
                    var data = KeyList.Where(i => i.CodeKey == users.CodeKey).FirstOrDefault();
                    data.token = await UserSve.token(users.userData, _configuration);
                    KeyList.ForEach(i =>
                    {
                        if (i.CodeKey == data.CodeKey)
                        {
                            i = data;
                            i.userData = users.userData;
                            i.status = true;
                            string ip = HttpContext.Connection.RemoteIpAddress.ToString();
                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://restapi.amap.com/v3/ip?ip={ip}&key=4317380dfc896ba8789017710376d38c");
                            request.Method = "GET";
                            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                            Stream stream = response.GetResponseStream();
                            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                            JObject json = JObject.Parse(reader.ReadToEnd());
                            _set.loginData.Add(new LoginData()
                            {
                                vid = users.userData.vid,
                                loginTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                                loginIP = ip,
                                address = json.GetValue("province").ToString() + json.GetValue("city").ToString()
                            });
                            _set.SaveChanges();
                            return;
                        }
                    });
                    result.data = true;
                }
                return result;
            });
        }
    }
}
