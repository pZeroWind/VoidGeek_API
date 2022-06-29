using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VoidSys.Modles;

namespace VoidSys.Service
{
    public class UserSve
    {
        /// <summary>
        /// 登录
        /// </summary>
        public static Task<bool> login(v_Users users,Set set)
        {
            var task = Task.Run(()=> {
                bool res = false;
                v_Users login = set.v_users.Find(users.vid);
                if (login != null&&login.password == md5(users.password + users.vid))
                {
                    res = true;
                }
                return res;
            });
            return task;
        }

        /// <summary>
        /// 获取token
        /// </summary>
        public static Task<string> token(v_Users users,IConfiguration configuration)
        {
            var task = Task.Run(() =>
            {
                var claims = new List<Claim>
                {
                    new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Sub,users.vid)
                };
                var token = configuration.GetSection("Security:Token");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(token["Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var resToken = new JwtSecurityToken(
                    issuer: token["Issuer"],
                    audience: token["Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddDays(7),
                    signingCredentials: creds
                    );
                return new JwtSecurityTokenHandler().WriteToken(resToken);
            });
            return task;
        }

        /// <summary>
        /// md5加密
        /// </summary>
        public static string md5(string pwd)
        {
            var m = MD5.Create();
            var res = m.ComputeHash(Encoding.UTF8.GetBytes(pwd));
            string bitRes = BitConverter.ToString(res).Replace("-", "");
            return bitRes;
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        public static Task<Result<v_UsersData>> getData(string header,Set set)
        {
            var task = Task.Run(() =>
            {
                string vid = header.getVid();
                v_UsersData u = set.v_usersData.Find(vid);
                Result<v_UsersData> result = new Result<v_UsersData>();
                if (u == null)
                {
                    result.msg = $"未找到VID为{vid}的用户";
                }
                result.data = u;
                return result;
            });
            return task;
        }
        
        /// <summary>
        /// 修改个人信息
        /// </summary>
        /// <param name="header"></param>
        /// <param name="set"></param>
        /// <returns></returns>
        public static Task<Result<bool>> updateData(string header,v_UsersData data, Set set)
        {
            var task = Task.Run(() =>
            {
                Result<bool> result = new Result<bool>();
                string vid = header.getVid();
                set.v_usersData.Update(data);
                if (set.SaveChanges() > 0)
                {
                    result.data = true;
                }
                else
                {
                    result.code = 500;
                    result.msg = "修改失败,修改过程中出现未知错误";
                    result.data = false;
                }
                return result;
            });
            return task;
        }


        /// <summary>
        /// 注册用户
        /// </summary>
        /// <param name="rigster"></param>
        /// <param name="set"></param>
        /// <returns></returns>
        public static Task<Result<v_Users>> register(v_Rigster rigster,Set set)
        {
            var task = Task.Run(() =>
            {
                set.Database.BeginTransaction();
                Result<v_Users> result = new Result<v_Users>();
                try
                {
                    v_Users u = new v_Users();
                    v_UsersData usersData = new v_UsersData();
                    SaveFolder folder = new SaveFolder();
                    string vid = GetVid(set);
                    u.password = md5(rigster.password + vid);
                    u.vid = vid;
                    u.pass = true;
                    set.v_users.Add(u);
                    if (!(set.SaveChanges() > 0))
                    {
                        result.code = 500;
                        result.msg = "注册失败，保存账户信息时出错";
                        result.data = null;
                        set.Database.RollbackTransaction();
                        return result;
                    }
                    usersData.vid = vid;
                    usersData.userName = rigster.userName;
                    usersData.birthday = rigster.birthday;
                    usersData.email = rigster.email;
                    usersData.phoneNum = rigster.phoneNum;
                    usersData.gender = rigster.gender;
                    usersData.resume = "暂无";
                    set.v_usersData.Add(usersData);
                    if (!(set.SaveChanges() > 0))
                    {
                        result.code = 500;
                        result.msg = "注册失败，保存账户信息时出错";
                        result.data = null;
                        set.Database.RollbackTransaction();
                        return result;
                    }
                    folder.folderName = "默认收藏夹";
                    folder.vid = vid;
                    set.savefolder.Add(folder);
                    result.data = u;
                    if (!(set.SaveChanges() > 0))
                    {
                        result.code = 500;
                        result.msg = "注册失败，保存账户信息时出错";
                        result.data = null;
                        set.Database.RollbackTransaction();
                        return result;
                    }
                    set.v_post.Add(new v_Post()
                    {
                        vid = vid,
                        time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        content = $"{rigster.userName}，欢迎来到Void极客之家"
                    });
                    set.SaveChanges();
                    set.Database.CommitTransaction();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                    set.Database.RollbackTransaction();
                }
                return result;
            });
            return task;
        }

        /// <summary>
        /// 创建新的vid
        /// </summary>
        /// <param name="set"></param>
        /// <returns></returns>
        public static string GetVid(Set set)
        {
            string a = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            string vid = null;
            for(int i = 0; i <12 ; i++)
            {
                vid += a[i];
            }
            if (set.v_users.Where(i => i.vid == vid).Count() > 0)
            {
                GetVid(set);
            }
            return vid;
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="header"></param>
        /// <param name="set"></param>
        /// <returns></returns>
        public static Task<Result<bool>> updatePWD(string header,Password data, Set set)
        {
            var task = Task.Run(() =>
            {
                string token = header.Replace("Bearer ", "");
                var handler = new JwtSecurityTokenHandler();
                string vid = handler.ReadJwtToken(token).Payload.Sub;
                v_Users u = set.v_users.Find(vid);
                Result<bool> result = new Result<bool>();
                if (u == null)
                {
                    result.code = 500;
                    result.msg = $"未找到VID为{vid}的用户";
                }
                if(u.password == md5(data.oldPassword + u.vid))
                {
                    u.password = md5(data.newPassword + u.vid);
                    set.v_users.Update(u);
                    if (set.SaveChanges() > 0)
                    {
                        result.data = true;
                    }
                    else
                    {
                        result.code = 500;
                        result.msg = "修改失败,修改过程中出现未知错误";
                        result.data = false;
                    }
                }
                else
                {
                    result.code = 500;
                    result.msg = "修改失败,旧密码错误";
                    result.data = false;
                }
                return result;
            });
            return task;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public static Task<Result<List<string>>> updateFile(IFormFileCollection files)
        {
            var task = Task.Run(() =>
            {
                Result<List<string>> res = new Result<List<string>>();
                res.data = new List<string>();
                try
                {
                    foreach (FormFile item in files)
                    {
                        string exen = Path.GetExtension(item.FileName).ToLower();
                        string path = "";
                        if (exen != ".jpg" && exen != ".png" && exen != ".jpeg" && exen != ".gif")
                        {
                            path = "Files/" + Guid.NewGuid().ToString("N") + exen;
                        }
                        else
                        {
                            Directory.CreateDirectory("imgs/" + DateTime.Now.ToString("d"));
                            path = "imgs/"+DateTime.Now.ToString("d")+"/" + Guid.NewGuid().ToString("N") + exen;
                        }
                        using (FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write))
                        {
                            item.CopyTo(file);
                            file.Flush();
                        }
                        res.data.Add(path);
                    }
                }
                catch(Exception e)
                {
                    res.code = 500;
                    res.msg = "文件上传时出错：" + e.Message;
                }
                return res;
            });
            return task;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public static Task<ResultImg<List<string>>> updateFile2(IFormFileCollection files)
        {
            var task = Task.Run(() =>
            {
                ResultImg<List<string>> res = new ResultImg<List<string>>();
                res.data = new List<string>();
                try
                {
                    foreach (FormFile item in files)
                    {
                        string exen = Path.GetExtension(item.FileName).ToLower();
                        string path = "";
                        if (exen != ".jpg" && exen != ".png" && exen != ".jpeg" && exen != ".gif")
                        {
                            path = "Files/" + Guid.NewGuid().ToString("N") + exen;
                        }
                        else
                        {
                            Directory.CreateDirectory("imgs/" + DateTime.Now.ToString("d"));
                            path = "imgs/" + DateTime.Now.ToString("d") + "/" + Guid.NewGuid().ToString("N") + exen;
                        }
                        using (FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write))
                        {
                            item.CopyTo(file);
                            file.Flush();
                        }
                        res.data.Add("http://120.76.177.46:5000/api/PublicApi/getImg?imgUrl=" + path);
                    }
                }
                catch (Exception e)
                {
                    res.errno = 1;
                    res.msg = "文件上传时出错：" + e.Message;
                }
                return res;
            });
            return task;
        }

        /// <summary>
        /// 上传博客
        /// </summary>
        /// <param name="header"></param>
        /// <param name="data"></param>
        /// <param name="set"></param>
        /// <returns></returns>
        public static Task<Result<bool>> addPage(string header, updatePage data, Set set)
        {
            return Task.Run(() =>
            {
                set.Database.BeginTransaction();
                Result<bool> result = new Result<bool>();
                try
                {
                    string vid = header.getVid();
                    v_Users u = set.v_users.Find(vid);
                    if (u == null)
                    {
                        result.msg = $"未找到VID为{vid}的用户";
                    }
                    else
                    {
                        data.vid = vid;
                        data.pass = false;
                    }
                    if (!u.pass)
                    {
                        result.code = 401;
                        result.msg = "当前账号已被封禁，无法上传!";
                        set.Database.RollbackTransaction();
                        return result;
                    }
                    set.v_page.Add(data);
                    set.SaveChanges();
                    List<Use_Tag> tags = new List<Use_Tag>();
                    data.tag.ForEach(i =>
                    {
                        tags.Add(new Use_Tag()
                        {
                            pid = data.pid,
                            tagName = i
                        });
                    });
                    set.use_tag.AddRange(tags);
                    set.v_area.Find(data.aid).pageNum++;
                    set.v_usersData.Find(data.vid).exc += 50;
                    if (set.SaveChanges() > 0)
                    {
                        List<string> chars = set.falseChars.Select(i => i.chars).ToList();
                        int len = 0;
                        chars.ForEach(i =>
                        {
                            if (data.content.IndexOf(i) != -1)
                            {
                                len += Regex.Matches(data.content, i).Count;
                                data.content = data.content.Replace(i, "***");
                            }
                        });
                        if (len < 10)
                        {
                            data.pass = true;
                        }
                        set.v_page.Update(data);
                        set.SaveChanges();
                        set.Database.CommitTransaction();
                        result.data = true;
                    }
                    else
                    {
                        result.code = 500;
                        result.msg = "上传博客失败，请重新尝试";
                        set.Database.RollbackTransaction();
                    }
                }
                catch (Exception e)
                {
                    result.code = 500;
                    result.msg = "上传博客失败："+e.Message;
                    set.Database.RollbackTransaction();
                }
                return result;
            });
        }

        /// <summary>
        /// 查寻用户
        /// </summary>
        /// <param name="vid"></param>
        /// <param name="set"></param>
        /// <returns></returns>
        public static Task<Result<v_UsersData>> findUser(string vid,Set set)
        {
            var task = Task.Run(() =>
            {
                Result<v_UsersData> result = new Result<v_UsersData>();
                result.data = set.v_usersData.Find(vid);
                return result;
            });
            return task;
        }

        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <param name="set"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static Task<ResultPage<List<v_UsersData>>> getUserList(Set set, int page, int mode, int size)
        {
            var task = Task.Run(() =>
            {
                ResultPage<List<v_UsersData>> res = new ResultPage<List<v_UsersData>>();
                if (page == 0)
                {
                    page = 1;
                }
                res.page = page;
                switch (mode)
                {
                    //正常使用的账号
                    case 1:
                        res.data = set.v_usersData.Where(i => set.v_users.Where(i2=>i2.vid==i.vid).FirstOrDefault().pass)
                        .OrderByDescending(i => i.vid)
                        .Skip((page - 1) * size).Take(size).ToList();
                        res.limt = (int)Math.Ceiling((double)set.v_usersData.Where(i => set.v_users.Where(i2 => i2.vid == i.vid).FirstOrDefault().pass).Count() / size);
                        break;
                    //已被封禁的账号
                    case 2:
                        res.data = set.v_usersData.Where(i => !set.v_users.Where(i2=>i2.vid==i.vid).FirstOrDefault().pass)
                        .OrderByDescending(i => i.vid)
                        .Skip((page - 1) * size).Take(size).ToList();
                        res.limt = (int)Math.Ceiling((double)set.v_usersData.Where(i => !set.v_users.Where(i2 => i2.vid == i.vid).FirstOrDefault().pass).Count() / size);
                        break;
                    //粉丝最多十个的账号
                    case 3:
                        res.data = set.v_usersData
                        .OrderByDescending(i => i.fanNum)
                        .Take(10).ToList();
                        break;
                    default:
                        res.data = set.v_usersData.OrderByDescending(i => i.vid)
                        .Skip((page - 1) * size).Take(size).ToList();
                        res.limt = (int)Math.Ceiling((double)set.v_usersData.Count() / size);
                        break;
                }
                
                return res;
            });
            return task;
        }
        
        /// <summary>
        /// 记录历史
        /// </summary>
        /// <param name="set"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static Task<Result<bool>> addHistory(Set set,int pid,string header)
        {
            var task = Task.Run(() =>
            {
                string vid = header.getVid();
                Result<bool> result = new Result<bool>();
                History history = new History();
                history.vid = vid;
                history.pid = pid;
                history.time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                set.Database.BeginTransaction();
                try
                {
                    var HisData = set.history.Where(i => i.pid == pid && i.vid == vid).FirstOrDefault();
                    if (HisData != null)
                    {
                        set.history.Remove(HisData);
                        set.SaveChanges();
                    }
                    set.history.Add(history);
                    set.SaveChanges();
                    set.Database.CommitTransaction();
                    result.data = true;
                }
                catch(Exception e)
                {
                    result.code = 500;
                    result.data = false;
                    result.msg = e.Message;
                    set.Database.RollbackTransaction();
                }
                return result;
            });
            return task;
        }

        /// <summary>
        /// 获取历史记录
        /// </summary>
        public static Task<Result<List<HisRes>>> getHis(Set set,string header)
        {
            var task = Task.Run(() =>
            {
                string vid = header.getVid();
                Result<List<HisRes>> result = new Result<List<HisRes>>();
                result.data = set.history.Where(i => i.vid == vid).OrderByDescending(i=>i.time).Take(50).Select(i =>
                new HisRes() {
                    pageData = set.v_page.Where(it=>it.pid==i.pid).FirstOrDefault(),
                    pid = i.pid,
                    vid = i.vid,
                    time = i.time,
                    hId = i.hId
                }).ToList();
                return result;
            });
            return task;
        }

        /// <summary>
        /// 收藏文章
        /// </summary>
        /// <param name="set"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static Task<Result<bool>> addSave(Set set, int pid,int sfId, string header)
        {
            var task = Task.Run(() =>
            {
                string vid = header.getVid();
                Result<bool> result = new Result<bool>();
                int i = set.save.Where(i => i.pid == pid && i.folderId == sfId).Count();
                if (i > 0)
                {
                    result.msg = "当前收藏夹已添加了该文章";
                    result.data = false;
                }
                else
                {
                    Save save = new Save();
                    save.folderId = sfId;
                    save.pid = pid;
                    set.save.Add(save);
                    try
                    {
                        if (set.SaveChanges() < 0)
                        {
                            result.code = 500;
                            result.data = false;
                            result.msg = "添加失败";
                        }
                        else
                        {
                            result.data = true;
                        }

                    }
                    catch (Exception e)
                    {
                        result.code = 500;
                        result.data = false;
                        result.msg = e.Message;
                    }
                }
                return result;
            });
            return task;
        }

        /// <summary>
        /// 获取收藏文件夹
        /// </summary>
        /// <param name="set"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public static Task<Result<List<SaveFolder>>> getSaveFolder(Set set,string vid,bool token)
        {
            var task = Task.Run(() =>
            {
                if (token)
                {
                    vid = vid.getVid();
                }
                Result<List<SaveFolder>> result = new Result<List<SaveFolder>>();
                result.data = set.savefolder.Where(i => i.vid == vid).ToList();
                return result;
            });
            return task;
        }

        /// <summary>
        /// 获取文件夹中的文章
        /// </summary>
        /// <param name="set"></param>
        /// <param name="sfid"></param>
        /// <returns></returns>
        public static Task<ResultPage<List<v_PageRes>>> getSavePage(Set set,int sfid,int page)
        {
            var task = Task.Run(() =>
            {
                ResultPage<List<v_PageRes>> result = new ResultPage<List<v_PageRes>>();
                result.data = new List<v_PageRes>();
                set.save.Where(i => i.folderId == sfid).ToList().ForEach(i =>
                {
                   
                    if (i.pid != 0)
                    {
                        v_Page item = set.v_page.Find(i.pid);
                        v_PageRes page = new v_PageRes();
                        page.pid = item.pid;
                        page.vid = item.vid;
                        page.userName = set.v_usersData.Find(item.vid).userName;
                        page.aid = item.aid;
                        page.areaName = set.v_area.Find(item.aid).areaName;
                        page.tag = set.use_tag.Where(i2 => i2.pid == i.pid).Select(i2 => i2.tagName).ToList();
                        page.title = item.title;
                        page.content = item.content;
                        page.sayNum = item.sayNum;
                        page.saveNum = item.saveNum;
                        page.likeNum = item.likeNum;
                        page.readNum = item.readNum;
                        page.time = item.time;
                        page.pass = item.pass;
                        page.post = item.post;
                        result.data.Add(page);
                    }
                });
                result.data.Reverse();
                if (page == 0)
                {
                    page = 1;
                }
                int index = (page - 1) * 5;
                result.limt = (int)Math.Ceiling((double)result.data.Count / 5);
                result.page = page;
                result.data = result.data.Skip(index).Take(5).ToList();
                return result;
            });
            return task;
        }

        /// <summary>
        /// 关注/取消关注用户
        /// </summary>
        /// <param name="set"></param>
        /// <param name="header"></param>
        /// <param name="vid"></param>
        /// <returns></returns>
        public static Task<Result<bool>> likeUser(Set set, string header, string fid)
        {
            return Task.Run(() =>
            {
                string vid = header.getVid();
                Result<bool> result = new Result<bool>();
                if (vid == fid)
                {
                    result.code = 500;
                    result.msg = "您无法关注自己";
                    return result;
                }
                set.Database.BeginTransaction();
                var data = set.v_friends.Where(i => i.vid_me == vid && i.vid_friend == fid).FirstOrDefault();
                try
                {
                    if (data != null)
                    {
                        set.v_friends.Remove(data);
                        set.v_usersData.Find(data.vid_friend).fanNum--;
                        result.data = true;
                        result.msg = "取消关注成功";
                    }
                    else
                    {
                        set.v_friends.Add(new v_Friends()
                        {
                            vid_me = vid,
                            vid_friend = fid
                        });
                        set.v_usersData.Find(fid).fanNum++;
                        result.data = true;
                        result.msg = "关注成功";
                    }
                    set.SaveChanges();
                    set.Database.CommitTransaction();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    set.Database.RollbackTransaction();
                }
                
                return result;
            });
        }

        /// <summary>
        /// 忘记密码
        /// </summary>
        /// <param name="set"></param>
        /// <param name="pwd"></param>
        /// <param name="vid"></param>
        /// <returns></returns>
        public static Task<Result<bool>> cPwd(Set set,string pwd,string vid)
        {
            return Task.Run(() =>
            {

                Result<bool> res = new Result<bool>();
                set.v_users.Find(vid).password = md5(pwd+vid);
                if (set.SaveChanges() > 0)
                {
                    res.data = true;
                }
                else
                {
                    res.code = 500;
                    res.msg = "密码修改失败";
                    res.data = false;
                }
                return res;
            });
        }

        /// <summary>
        /// 通过vid发送邮箱验证码
        /// </summary>
        /// <param name="set"></param>
        /// <param name="pwd"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public static Task<Result<string>> toEmail(Set set, string vid)
        {
            return Task.Run(async () =>
            {
                string em = set.v_usersData.Find(vid).email; 
                return await EmailSend.toEmil(em);
            });
        }
    }
}
