using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Service
{
    public static class Tool
    {
        public static string getVid(this string header)
        {
            try
            {
                string token = header.Replace("Bearer ", "");
                var handler = new JwtSecurityTokenHandler();
                return handler.ReadJwtToken(token).Payload.Sub;
            }catch
            {
                return "-1";
            }
        }

        public static bool OnajiAll(this List<string> l1,List<string> l2)
        {
            return l2.All(i => l1.Any(i2 => i2 == i));
        }
    }
}
