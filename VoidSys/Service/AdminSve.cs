using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using VoidSys.Modles;

namespace VoidSys.Service
{
    public class AdminSve
    {

        /// <summary>
        /// 登录
        /// </summary>
        public static Task<Result<v_AdminRes>> login(v_Admin users, Set set,IConfiguration configuration)
        {
            var task = Task.Run(async () =>
            {
                Result<v_AdminRes> res = new Result<v_AdminRes>();
                v_Admin u = set.v_admins.Where(i => i.vid == users.vid && i.password == UserSve.md5(users.password + users.vid)).First();
                res.data = new v_AdminRes();
                if (u != null)
                {
                    res.data.name = u.name;
                    res.data.role = u.role;
                    res.data.token = await token(u, configuration);
                }
                return res;
            });
            return task;
        }

        /// <summary>
        /// 获取token
        /// </summary>
        /// <param name="users"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static Task<string> token(v_Admin users, IConfiguration configuration)
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
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: creds
                    );
                return new JwtSecurityTokenHandler().WriteToken(resToken);
            });
            return task;
        }

        /// <summary>
        /// 注册新管理员
        /// </summary>
        /// <param name="header"></param>
        /// <param name="set"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Task<Result<bool>> register(string header, Set set, v_Admin data)
        {
            var task = Task.Run(() =>
            {
                Result<bool> result = new Result<bool>();
                string token = header.Replace("Bearer ", "");
                var handler = new JwtSecurityTokenHandler();
                string vid = handler.ReadJwtToken(token).Payload.Sub;
                v_Admin a = set.v_admins.Find(vid);
                if (a != null && a.role == 0)
                {
                    data.password = UserSve.md5(data.password + data.vid);
                    set.v_admins.Add(data);
                    if (set.SaveChanges() > 0)
                    {
                        result.data = true;
                    }
                    else
                    {
                        result.code = 500;
                        result.msg = "注册时出现未知错误！";
                        result.data = false;
                    }
                }
                else
                {
                    result.code = 401;
                    result.msg = "操作失败，权限不足！";
                    result.data = false;
                }
                return result;
            });
            return task;
        }

        /// <summary>
        /// 轮播图修改
        /// </summary>
        /// <param name="header"></param>
        /// <param name="set"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Task<Result<bool>> changeRoll(string header, Set set, List<int> data)
        {
            var task = Task.Run(() =>
            {
                string token = header.Replace("Bearer ", "");
                var handler = new JwtSecurityTokenHandler();
                string vid = handler.ReadJwtToken(token).Payload.Sub;
                v_Admin a = set.v_admins.Find(vid);
                Result<bool> result = new Result<bool>();
                result.data = false;
                if (a != null)
                {
                    var d = set.v_roll.ToList();
                    for (int i = 0; i < d.Count; i++)
                    {
                        d[i].pid = data[i];
                    }
                    set.v_roll.UpdateRange(d);
                    if (set.SaveChanges() > 0)
                    {
                        result.data = true;

                    }
                    else
                    {
                        result.code = 500;
                        result.msg = "操作失败";
                    }
                }
                else
                {
                    result.code = 401;
                    result.msg = "操作失败，权限不足";
                }
                return result;
            });
            return task;
        }

        /// <summary>
        /// 修改博客内容
        /// </summary>
        /// <param name="header"></param>
        /// <param name="set"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Task<Result<bool>> changePage(Set set, v_Page data)
        {
            var task = Task.Run(() =>
            {
                Result<bool> result = new Result<bool>();
                set.v_page.Update(data);
                if (set.SaveChanges() > 0)
                {
                    result.data = true;

                }
                else
                {
                    result.code = 500;
                    result.msg = "操作失败";
                }
                return result;
            });
            return task;
        }

    }       
}
