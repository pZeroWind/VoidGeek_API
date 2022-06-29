using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class LoginData
    {
        [Key]
        public int id { get; set; }

        public string vid { get; set; }

        public long loginTime { get; set; }

        public string loginIP { get; set; }

        public string address { get; set; }
    }

    public class LoginDataRes : LoginData
    {
        public v_UsersData userData { get; set; }
    }
}
