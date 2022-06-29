using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoidSys.Modles;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VoidSys.Service
{
    public class PageSve
    {

        /// <summary>
        /// 查询列表
        /// </summary>
        public static Task<ResultPage<List<v_PageRes>>> getList(string vid, string aid, string tag, int page, string search, int pid, int mode, int pz, int passMode, Set set)
        {
            var task = Task.Run(() => {
                List<v_Page> result = new List<v_Page>();
                //筛选类型
                if (tag != null && tag != "")
                {
                    if (pid == 0)
                    {
                        List<string> array = tag.Split("%").ToList();
                        result = new List<v_Page>();
                        result = set.use_tag.ToList().Where(i => i.tagName == array[0]).Select(i => set.v_page.Find(i.pid)).ToList();
                        for (int i = 0; i < result.Count; i++)
                        {
                            List<string> tags = set.use_tag.Where(i2 => i2.pid == result[i].pid).Select(i2 => i2.tagName).ToList();
                            if (!tags.OnajiAll(array))
                            {
                                result.Remove(result[i]);
                                i--;
                            }
                        }
                    }
                    else
                    {
                        List<string> array = tag.Split("%").ToList();
                        result = new List<v_Page>();
                        array.ForEach(it =>
                        {
                            result = result.Union(set.use_tag.ToList().Where(i => i.tagName == it).Select(i => set.v_page.Find(i.pid)).ToList()).ToList();
                        });
                    }
                    result.RemoveAll(i => i == null);
                }
                else
                {
                    result = set.v_page.ToList();
                }
                //获取个人文章
                if (vid != null && vid != "")
                {
                    result = result.Where(i => i.vid == vid).ToList();
                }
                //筛选分区
                if (aid != "" && aid != null)
                {
                    result = result.Where(i => i.aid == aid).ToList();
                }
                //模糊查询
                if (search != "" && search != null)
                {
                    result = result.Where(i => i.title.Contains(search)).ToList();
                }
                //跳过某个文章
                if (pid != 0)
                {
                    result = result.Where(i => i.pid != pid).ToList();
                }
                //判断是否获取过审文章
                switch (passMode)
                {
                    case 0:
                        result = result.Where(i => i.pass).ToList();
                        break;
                    case 1:
                        result = result.Where(i => !i.pass).ToList();
                        break;
                }
                //搜寻点赞最多，推荐，热门以及最新
                switch (mode)
                {
                    case 0:
                        result = result.OrderByDescending(i => set.likedata.Where(it => i.pid == it.pid).Count()).ThenByDescending(i => i.readNum).ThenByDescending(i => i.time).ToList();
                        break;
                    case 1:
                        result = result.OrderByDescending(i => i.readNum).ThenByDescending(i => i.time).ToList();
                        break;
                    case 2:
                        result = result.OrderByDescending(i => set.likedata.Where(it=>i.pid == it.pid).Count()).ThenByDescending(i => i.time).ToList();
                        break;
                    case 3:
                        result = result.OrderByDescending(i => i.time).ToList();
                        break;
                }
                ResultPage<List<v_PageRes>> resList = new ResultPage<List<v_PageRes>>();
                if (page == 0)
                {
                    page = 1;
                }
                resList.total = result.Count();
                int index = (page - 1) * pz;
                resList.limt = (int)Math.Ceiling((double)result.Count / pz);
                resList.page = page;
                resList.data = result.Skip(index).Take(pz).Select(i => new v_PageRes()
                {
                    pid = i.pid,
                    vid = i.vid,
                    userName = set.v_usersData.Find(i.vid).userName,
                    aid = i.aid,
                    areaName = set.v_area.Find(i.aid).areaName,
                    tag = set.use_tag.Where(i2=>i2.pid == i.pid).Select(i2=>i2.tagName).ToList(),
                    title = i.title,
                    content = i.content,
                    sayNum = set.v_say.Where(i2=>i2.pid == i.pid).Count() + set.v_say.Where(i2=>i2.pid == i.pid).Count(),
                    saveNum = set.save.Where(i2=>i2.pid == i.pid).Count(),
                    likeNum = set.likedata.Where(i2=>i2.pid == i.pid).Count(),
                    readNum = i.readNum,
                    time = i.time,
                    pass = i.pass,
                    post = i.post
                }).ToList();
                resList.msg = "第" + (index + 1) + "至" + (index + pz) + "条";
                return resList;
            });
            return task;
        }

        /// <summary>
        /// 获取文章信息
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="set"></param>
        /// <returns></returns>
        public static Task<Result<v_PageRes>> get(int pid, Set set)
        {
            var task = Task.Run(() =>
            {
                Result<v_PageRes> result = new Result<v_PageRes>();
                v_PageRes page = set.v_page.Where(i=>i.pid==pid).ToList().Select(i=> new v_PageRes()
                {
                    pid = i.pid,
                    vid = i.vid,
                    userName = set.v_usersData.Find(i.vid).userName,
                    aid = i.aid,
                    areaName = set.v_area.Find(i.aid).areaName,
                    tag = set.use_tag.Where(i2 => i2.pid == i.pid).Select(i2 => i2.tagName).ToList(),
                    title = i.title,
                    content = i.content,
                    sayNum = set.v_say.Where(i2 => i2.pid == i.pid).Count() + set.v_resay.ToList().Where(i2 => {
                        List<v_Say> list = set.v_say.Where(i2 => i2.pid == i.pid).ToList();
                        bool res = false;
                            foreach (var item in list)
                            {
                                if(item.sayid == i2.sid)
                                {
                                    res = true;
                                }
                            }
                            return res;
                        }).Count(),
                    saveNum = set.save.Where(i2 => i2.pid == i.pid).Count(),
                    likeNum = set.likedata.Where(i2 => i2.pid == i.pid).Count(),
                    readNum = i.readNum,
                    time = i.time,
                    pass = i.pass,
                    post = i.post
                }).FirstOrDefault();
                result.data = page;
                return result;
            });
            return task;
        }

        /// <summary>
        /// 阅读文章
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="set"></param>
        /// <returns></returns>
        public static Task<Result<v_PageRes>> to(int pid, Set set)
        {
            var task = Task.Run(() =>
            {
                Result<v_PageRes> result = new Result<v_PageRes>();
                v_Page p = set.v_page.Find(pid);
                v_PageRes page = set.v_page.Where(i => i.pid == pid).ToList().Select(i => new v_PageRes()
                {
                    pid = i.pid,
                    vid = i.vid,
                    userName = set.v_usersData.Find(i.vid).userName,
                    aid = i.aid,
                    areaName = set.v_area.Find(i.aid).areaName,
                    tag = set.use_tag.Where(i2 => i2.pid == i.pid).Select(i2 => i2.tagName).ToList(),
                    title = i.title,
                    content = i.content,
                    sayNum = set.v_say.Where(i2 => i2.pid == i.pid).Count() + set.v_resay.ToList().Where(i2 => {
                        List<v_Say> list = set.v_say.Where(i2 => i2.pid == i.pid).ToList();
                        bool res = false;
                        foreach (var item in list)
                        {
                            if (item.sayid == i2.sid)
                            {
                                res = true;
                            }
                        }
                        return res;
                    }).Count(),
                    saveNum = set.save.Where(i2 => i2.pid == i.pid).Count(),
                    likeNum = set.likedata.Where(i2 => i2.pid == i.pid).Count(),
                    readNum = i.readNum,
                    time = i.time,
                    pass = i.pass,
                    post = i.post
                }).FirstOrDefault();
                p.readNum++;
                set.v_page.Update(p);
                set.SaveChanges();
                result.data = page;
                return result;
            });
            return task;
        }

        /// <summary>
        /// 获取热帖文章
        /// </summary>
        /// <param name="set"></param>
        /// <returns></returns>
        public static Task<Result<List<v_PageRes>>> getTop(Set set){
            var task = Task.Run(() =>
            {
                Result<List<v_PageRes>> result = new Result<List<v_PageRes>>();
                result.data =  set.v_top.OrderBy(i=> i.tid).ToList()
                .Select(i3=> set.v_page.Where(i2=>i2.pid == i3.pid)
                .ToList()
                .Select(i=> new v_PageRes()
                {
                    pid = i.pid,
                    vid = i.vid,
                    userName = set.v_usersData.Find(i.vid).userName,
                    aid = i.aid,
                    areaName = set.v_area.Find(i.aid).areaName,
                    tag = set.use_tag.Where(i2 => i2.pid == i.pid).Select(i2 => i2.tagName).ToList(),
                    title = i.title,
                    content = i.content,
                    sayNum = set.v_say.Where(i2 => i2.pid == i.pid).Count() + set.v_say.Where(i2 => i2.pid == i.pid).Count(),
                    saveNum = set.save.Where(i2 => i2.pid == i.pid).Count(),
                    likeNum = set.likedata.Where(i2 => i2.pid == i.pid).Count(),
                    readNum = i.readNum,
                    time = i.time,
                    pass = i.pass,
                    post = i.post
                }).FirstOrDefault()).ToList();
                result.data = result.data.Select(i=> {
                    if (i == null)
                    {
                        return new v_PageRes()
                        {
                            pid = 0,
                            title = "该文章已被删除",
                            content = "",
                            sayNum = 0,
                            saveNum = 0,
                            likeNum = 0,
                            readNum = 0,
                            time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            pass = false,
                            post = "http://120.76.177.46:5000/api/PublicApi/getImg?imgUrl="
                        };
                    }
                    else
                    {
                        return i;
                    }
                }).ToList();
                return result;
            });
            return task;
        }

        /// <summary>
        /// 举报文章
        /// </summary>
        /// <param name="set"></param>
        /// <param name="page"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public static Task<Result<bool>> falsePage(Set set,v_Tsumi_Page page, string header)
        {
            return Task.Run(() =>
            {
                Result<bool> res = new Result<bool>();
                page.vid = header.getVid();
                page.time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                set.v_tsumi_page.Add(page);
                if(set.SaveChanges() > 0)
                {
                    res.data = true;
                }
                else
                {
                    res.data = false;
                }
                return res;
            });
        }
    }
}
