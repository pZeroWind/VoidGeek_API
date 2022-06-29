using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VoidSys.Modles;
using VoidSys.Service;

namespace VoidSys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors]
    public class PublicApi : ControllerBase
    {
        private readonly Set _set;

        public PublicApi(Set set)
        {
            _set = set;
        }
        /// <summary>
        /// 获取图片
        /// </summary>
        /// <param name="imgUrl"></param>
        /// <returns></returns>
        [HttpGet("getImg")]
        public IActionResult GetImg(string imgUrl)
        {
            if(imgUrl == null|| imgUrl == "" ||imgUrl == "undefined" || imgUrl == "null")
            {
                imgUrl = "imgs/udf.jpg";
            }
            FileInfo fi = new FileInfo(imgUrl);
            FileStream fs = fi.OpenRead();
            byte[] buffer = new byte[fi.Length];
            //读取图片字节流
            fs.Read(buffer, 0, Convert.ToInt32(fi.Length));
            var response = File(buffer, "image/png");
            return response;
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        [HttpGet("download")]
        public IActionResult GetFile(string url)
        {
            FileInfo fi = new FileInfo(url);
            FileStream fs = fi.OpenRead();
            byte[] buffer = new byte[fi.Length];
            //读取文件字节流
            fs.Read(buffer, 0, Convert.ToInt32(fi.Length));
            var response = File(buffer, "application/octet-stream", fi.Name);
            return response;
        }

        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost("getEmailCode")]
        async public Task<Result<string>> GetCode([FromForm]string email)
        {
            return await EmailSend.toEmil(email);
        }

        /// <summary>
        /// 获取轮播图
        /// </summary>
        /// <returns></returns>
        [HttpGet("getBanner")]
        async public Task<Result<List<v_Page>>> GetBanner()
        {
            var task = Task.Run(() =>
            {
                Result<List<v_Page>> result = new Result<List<v_Page>>();
                result.data = _set.v_roll.Select(i => _set.v_page.Where(it => it.pid == i.pid).FirstOrDefault()).ToList();
                return result;
            });
            return await task;
        }

        /// <summary>
        /// 富文本编辑框的上传文件
        /// </summary>
        /// <returns></returns>
        [HttpPost("update")]
        [AllowAnonymous]
        async public Task<ResultImg<List<string>>> update()
        {
            IFormFileCollection files = Request.Form.Files;
            return await UserSve.updateFile2(files);
        }

        /// <summary>
        /// 登录成功
        /// </summary>
        [HttpPost("inner")]
        async public Task<Result<bool>> inner()
        {
            return await Task.Run(() =>
            {
                Result<bool> result = new Result<bool>();
                result.data = true;
                return result;
            });
        }

        /// <summary>
        /// 提交反馈
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        [HttpPost("sendReturn")]
        [AllowAnonymous]
        async public Task<Result<bool>> sendReturn(v_Return r)
        {
            return await Task.Run(() =>
            {
                Result<bool> result = new Result<bool>();
                string header = HttpContext.Request.Headers["Authorization"];
                string vid = header.getVid();
                _set.v_return.Add(r);
                if (_set.SaveChanges() > 0)
                {
                    result.data = true;
                }                
                return result;
            });
        }

        /// <summary>
        /// 检测安卓版本是否与最新一致
        /// </summary>
        /// <param name="vision"></param>
        /// <returns></returns>
        [HttpGet("AndroidUpdateCheck")]
        async public Task<bool> AndroidUpdateCheck(string vision)
        {
            return await Task.Run(() =>
            {
                return vision != _set.visionupdate.OrderByDescending(i => i.time).FirstOrDefault().vision;
            });
        }

        /// <summary>
        /// 更新安卓端版本
        /// </summary>
        /// <param name="vision"></param>
        /// <returns></returns>
        [HttpPost("AndroidUpdate")]
        [DisableRequestSizeLimit]
        async public Task<bool> AndroidUpdate()
        {
            return await Task.Run(async () =>
            {
                Result <List<string>> filesStr = await UserSve.updateFile(Request.Form.Files);
                string[] vision = _set.visionupdate.OrderByDescending(i => i.time).FirstOrDefault().vision.Split(".");
                vision[2] = (int.Parse(vision[2]) + 1).ToString();
                if (int.Parse(vision[2]) == 10)
                {
                    vision[2] = "0";
                    vision[1] = (int.Parse(vision[1]) + 1).ToString();
                }
                if (int.Parse(vision[1]) == 10)
                {
                    vision[1] = "0";
                    vision[0] = (int.Parse(vision[0]) + 1).ToString();
                }
                _set.visionupdate.Add(new Visionupdate()
                {
                    time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    url = filesStr.data[0],
                    vision = vision[0]+"."+vision[1]+"."+vision[2]
                }) ;

                return _set.SaveChanges() > 0;
            });
        }

        /// <summary>
        /// 获取安卓端下载路径
        /// </summary>
        /// <returns></returns>
        [HttpGet("AndroidDownload")]
        async public Task<string> AndroiDownLoad()
        {
            return await Task.Run(() =>
            {
                return "http://120.76.177.46:5000/api/PublicApi/download?url=" + _set.visionupdate.OrderByDescending(i => i.time).FirstOrDefault().url;
            });
        }

        /// <summary>
        /// 获取违禁词
        /// </summary>
        /// <returns></returns>
        [HttpGet("getChars")]
        async public Task<Result<List<string>>> getChars()
        {
            return await Task.Run(() =>
            {
                return new Result<List<string>>()
                {
                    data = _set.falseChars.Select(i => i.chars).ToList()
                };
            });
        }

        /// <summary>
        /// 添加违禁词
        /// </summary>
        /// <returns></returns>
        [HttpPost("addChars")]
        async public Task<Result<bool>> addChars(string chars)
        {
            return await Task.Run(() =>
            {
                _set.falseChars.Add(new FalseChars()
                {
                    chars = chars
                });
                return new Result<bool>()
                {
                    data = _set.SaveChanges() > 0
                };
            });
        }

        /// <summary>
        /// 删除违禁词
        /// </summary>
        /// <param name="chars"></param>
        /// <returns></returns>
        [HttpDelete("deleteChars")]
        async public Task<Result<bool>> deleteChars(string chars)
        {
            return await Task.Run(() =>
            {
                _set.falseChars.Remove(_set.falseChars.Where(i=>i.chars == chars).FirstOrDefault());
                return new Result<bool>()
                {
                    data = _set.SaveChanges() > 0
                };
            });
        }
    }
}
