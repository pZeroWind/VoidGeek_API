using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VoidSys.Modles;
using VoidSys.Service;

namespace VoidSys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors]
    public class PageApi : ControllerBase
    {
        private readonly Set _set;

        public PageApi(Set set)
        {
            _set = set;
        }

        /// <summary>
        /// 查询page
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        [HttpGet("getList")]
        [HttpPost("getList")]
        async public Task<ResultPage<List<v_PageRes>>> GetList(string vid, string aid, [FromForm]string tag, int page, string search, int invId, int mode, int passMode, int size)
        {
            return await PageSve.getList(vid, aid, tag, page, search, invId, mode, size, passMode, _set);
        }

        /// <summary>
        /// 获取详情信息
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        [HttpGet("get")]
        async public Task<Result<v_PageRes>> Get(int pid)
        {
            return await PageSve.get(pid, _set);
        }

        /// <summary>
        /// 进入文章
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        [HttpPut("to")]
        async public Task<Result<v_PageRes>> to(int pid)
        {

            return await PageSve.to(pid, _set); ;
        }

        /// <summary>
        /// 文章点赞
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="sayid"></param>
        /// <returns></returns>
        [HttpPut("like")]
        [Authorize]
        async public Task<Result<int>> like(int pid)
        {
            var task = Task.Run(() =>
            {
                string header = HttpContext.Request.Headers["Authorization"];
                string vid = header.getVid();
                Result<int> result = new Result<int>();
                
                v_Page p = _set.v_page.Find(pid);
                if(_set.likedata.Where(i=>i.pid == pid && i.vid == vid).Count() == 0)
                {
                    _set.likedata.Add(new LikeData()
                    {
                        pid = pid,
                        vid = vid
                    });
                    _set.SaveChanges();
                }
                else
                {
                    result.code = 500;
                    result.msg = "你已经点过赞了";
                }
                
                result.data = p.likeNum;
                return result;
            });
            return await task;
        }

        /// <summary>
        /// 取消点赞
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        [HttpPut("unlike")]
        [Authorize]
        async public Task<Result<int>> unlike(int pid)
        {
            var task = Task.Run(() =>
            {
                string header = HttpContext.Request.Headers["Authorization"];
                string vid = header.getVid();
                Result<int> result = new Result<int>();
                v_Page p = _set.v_page.Find(pid);
                if (_set.likedata.Where(i => i.pid == pid && i.vid == vid).Count() > 0)
                {
                    _set.likedata.Remove(_set.likedata.Where(i=>i.vid == vid).FirstOrDefault());
                    _set.v_page.Update(p);
                    _set.SaveChanges();
                }
                else
                {
                    result.code = 500;
                    result.msg = "你还未点过赞";
                }
                result.data = p.likeNum;
                return result;
            });
            return await task;
        }

        /// <summary>
        /// 获取评论
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        [HttpGet("getSayList")]
        async public Task<ResultPage<List<sayListObj>>> GetSay(int pid, int page, int size)
        {
            var task = Task.Run(() =>
            {
                ResultPage<List<sayListObj>> result = new ResultPage<List<sayListObj>>();
                var list = _set.v_say
                .OrderByDescending(i => i.time)
                .Where(say => say.pid == pid)
                .ToList();
                result.limt = (int)Math.Ceiling((double)list.Count / size);
                if (page == 0)
                {
                    page = 1;
                }
                result.page = page;
                result.data = list.Skip((page - 1) * size).Take(size).Select(i =>
                    new sayListObj()
                    {
                        vid = i.vid,
                        pid = i.pid,
                        content = i.content,
                        readed = i.readed,
                        sayid = i.sayid,
                        time = i.time,
                        usersData = _set.v_usersData.Where(i2 => i2.vid == i.vid).FirstOrDefault()
                    }
                ).ToList();
                return result;
            });
            return await task;
        }

        /// <summary>
        /// 获取回复
        /// </summary>
        /// <param name="sayid"></param>
        /// <returns></returns>
        [HttpGet("getReSayList")]
        async public Task<ResultPage<List<ReSayListObj>>> GetReSay(int sayid, int page, int size)
        {
            var task = Task.Run(() =>
            {
                ResultPage<List<ReSayListObj>> result = new ResultPage<List<ReSayListObj>>();
                var list = _set.v_resay.OrderByDescending(i => i.time).Where(item => item.sid == sayid).ToList();
                result.limt = (int)Math.Ceiling((double)list.Count / size);
                if (page == 0)
                {
                    page = 1;
                }
                result.page = page;
                result.data = list.Skip((page - 1) * size).Take(size).Select(i=> new ReSayListObj()
                {
                    vid = i.vid,
                    content = i.content,
                    readed = i.readed,
                    sayid = i.sayid,
                    sid = i.sid,
                    userData = _set.v_usersData.Where(i2 => i2.vid == i.vid).FirstOrDefault(),
                    toid = i.toid,
                    time = i.time,
                    toUserData = _set.v_usersData.Where(i2 => i2.vid == i.toid).FirstOrDefault()
                }).ToList();
                return result;
            });
            return await task;
        }

        /// <summary>
        /// 发表评论
        /// </summary>
        /// <param name="say"></param>
        /// <returns></returns>
        [HttpPost("AddSay")]
        [Authorize]
        async public Task<Result<int>> AddSay(v_Say say)
        {
            var task = Task.Run(() =>
            {
                Result<int> result = new Result<int>();
                string header = HttpContext.Request.Headers["Authorization"];
                string token = header.Replace("Bearer ", "");
                var handler = new JwtSecurityTokenHandler();
                string vid = handler.ReadJwtToken(token).Payload.Sub;
                if (!_set.v_users.Find(vid).pass)
                {
                    result.code = 401;
                    result.msg = "当前账号已被封禁，无法评论!";
                    return result;
                }
                say.vid = vid;
                List<string> chars = _set.falseChars.Select(i => i.chars).ToList();
                chars.ForEach(i =>
                {
                    if (say.content.Contains(i))
                    {
                        say.content = say.content.Replace(i, "***");
                    }
                });
                _set.v_say.Add(say);
                if (_set.SaveChanges() > 0)
                {
                    result.data = say.sayid;
                    _set.v_page.Find(say.pid).sayNum++;
                    _set.SaveChanges();
                }
                else
                {
                    result.msg = "评论发表失败";
                }
                return result;
            });
            return await task;
        }

        /// <summary>
        /// 回复评论
        /// </summary>
        /// <param name="say"></param>
        /// <returns></returns>
        [HttpPost("AddReSay")]
        [Authorize]
        async public Task<Result<int>> AddReSay(v_Resay say)
        {
            var task = Task.Run(() =>
            {
                Result<int> result = new Result<int>();
                string header = HttpContext.Request.Headers["Authorization"];
                string vid = header.getVid();
                if (!_set.v_users.Find(vid).pass)
                {
                    result.code = 401;
                    result.msg = "当前账号已被封禁，无法评论!";
                    return result;
                }
                say.vid = vid;
                v_UsersData data = _set.v_usersData.Find(say.toid);
                say.content = say.content;
                List<string> chars = _set.falseChars.Select(i => i.chars).ToList();
                chars.ForEach(i =>
                {
                    if (say.content.Contains(i))
                    {
                        say.content = say.content.Replace(i, "***");
                    }
                });
                _set.v_resay.Add(say);
                if (_set.SaveChanges() > 0)
                {
                    result.data = say.sayid;
                    _set.v_page.Find(_set.v_say.Find(say.sid).pid).sayNum++;
                    _set.SaveChanges();
                }
                else
                {
                    result.msg = "评论发表失败";
                }
                return result;
            });
            return await task;
        }

        /// <summary>
        /// 获取热帖
        /// </summary>
        /// <returns></returns>
        [HttpGet("getTop")]
        async public Task<Result<List<v_PageRes>>> getTop()
        {
            return await PageSve.getTop(_set);
        }

        /// <summary>
        /// 获取常用标签
        /// </summary>
        [HttpGet("getTag")]
        async public Task<ResultPage<List<v_Tag>>> getTag(int page, int size)
        {
            var task = Task.Run(() =>
            {
                ResultPage<List<v_Tag>> result = new ResultPage<List<v_Tag>>();
                result.data = _set.v_tag.ToList();
                if (page == 0)
                {
                    page = 1;
                }
                result.page = page;
                result.limt = (int)((double)result.data.Count / size);
                result.data = result.data.Skip((page - 1) * size).Take(size).ToList();
                return result;
            });
            return await task;
        }

        [HttpGet("getHotTag")]
        async public Task<Result<List<string>>> getHotTag()
        {
            return await Task.Run(() =>
            {
                Result<List<string>> data = new Result<List<string>>();
                data.data = _set.use_tag.Where(i => i.pid != 0)
                .GroupBy(i => i.tagName)
                .Select(i => new { name = i.Key, count = i.Count() })
                .OrderByDescending(i => i.count)
                .Select(i => i.name).Take(30).ToList();
                return data;
            });
        }

        [HttpPost("getHotTag")]
        async public Task<Result<List<Tag_Data>>> postHotTag()
        {
            return await Task.Run(() =>
            {
                Result<List<Tag_Data>> data = new Result<List<Tag_Data>>();
                data.data = _set.use_tag.Where(i => i.pid != 0)
                .GroupBy(i => i.tagName)
                .Select(i => new Tag_Data { name = i.Key, count = i.Count() })
                .OrderByDescending(i => i.count).Take(30).ToList();
                return data;
            });
        }
        

        [HttpGet("getUseTag")]
        async public Task<ResultPage<List<string>>> getUseTag(int page,int size)
        {
            return await Task.Run(() =>
            {
                ResultPage<List<string>> data = new ResultPage<List<string>>();
                data.data = _set.use_tag.Where(i=>_set.v_page.Where(it=>it.pid == i.pid).FirstOrDefault()!=null)
                .GroupBy(i => i.tagName)
                .Select(i => new { name = i.Key, conut = i.Count() })
                .OrderByDescending(i => i.conut)
                .Select(i => i.name).ToList();
                data.total = data.data.Count();
                data.limt = (int)Math.Ceiling(data.total * 1.0 / size);
                data.page = page;
                data.data = data.data.Skip((page - 1) * size).Take(size).ToList();
                return data;
            });
        }

        /// <summary>
        /// 添加常用标签
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        [HttpPost("addTag")]
        async public Task<Result<bool>> addTag(v_Tag tag)
        {
            return await Task.Run(() =>
            {
                _set.v_tag.Add(tag);
                _set.SaveChanges();
                return new Result<bool>()
                {
                    data = true
                };
            });
        }

        [HttpDelete("deleteTag")]
        async public Task<Result<bool>> deleteTag(int tagid)
        {
            return await Task.Run(() =>
            {
                _set.v_tag.Remove(_set.v_tag.Where(i=>i.tagid == tagid).FirstOrDefault());
                _set.SaveChanges();
                return new Result<bool>()
                {
                    data = true
                };
            });
        }

        /// <summary>
        /// 搜索框列表
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        [HttpGet("getContains")]
        async public Task<Result<List<string>>> getContains(string text)
        {
            return await Task.Run(() =>
            {
                Result<List<string>> result = new Result<List<string>>();
                result.data = _set.v_page.Where(i => i.title.Contains(text))
                .OrderByDescending(i => i.time)
                .OrderByDescending(i => i.likeNum)
                .OrderByDescending(i => i.readNum)
                .Select(i => i.title).Take(10).ToList();
                return result;
            });
        }

        /// <summary>
        /// 举报文章
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpPost("falsePage")]
        [Authorize]
        async public Task<Result<bool>> falsePage(v_Tsumi_Page page)
        {
            return await PageSve.falsePage(_set, page, HttpContext.Request.Headers["Authorization"]);
        }

        

        /// <summary>
        /// 获取举报列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet("getTsumi")]
        async public Task<ResultPage<List<ReTsumi>>> getTsumi(int page,int size)
        {
            return await Task.Run(() =>
            {
                ResultPage<List<ReTsumi>> result = new ResultPage<List<ReTsumi>>();
                result.total = _set.v_tsumi_page.Count();
                result.page = page;
                result.limt = (int)Math.Ceiling(result.total * 1.0 /size);
                result.data = _set.v_tsumi_page.OrderByDescending(i => i.time).Skip((page - 1) * size).Take(size).ToList()
                .Select(i=>new ReTsumi()
                {
                    tsumiId = i.tsumiId,
                    content = i.content,
                    pid = i.pid,
                    time = i.time,
                    usersData = _set.v_usersData.Where(it=>it.vid == i.vid).FirstOrDefault(),
                    pageData = _set.v_page.Where(it=>it.pid == i.pid).FirstOrDefault(),
                    vid = i.vid,
                    readed =i.readed
                }).ToList();
                return result;
            });
        }

        [HttpGet("getReturns")]
        async public Task<ResultPage<List<ReturnData>>> getReturn(int page, int size)
        {
            return await Task.Run(() =>
            {
                ResultPage<List<ReturnData>> result = new ResultPage<List<ReturnData>>();
                result.total = _set.v_return.Count();
                result.page = page;
                result.limt = (int)Math.Ceiling(result.total * 1.0 / size);
                result.data = _set.v_return.OrderByDescending(i => i.time).Skip((page - 1) * size).Take(size).ToList()
                .Select(i=>new ReturnData()
                {
                    returnId=i.returnId,
                    content = i.content,
                    time =i.time,
                    title = i.title,
                    usersData = _set.v_usersData.Where(it => it.vid == i.vid).FirstOrDefault(),
                    vid = i.vid,
                    readed = i.readed
                }).ToList();
                return result;
            });
        }
    }
}
