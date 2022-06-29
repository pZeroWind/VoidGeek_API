using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class v_Say
    {
        [Key]
        public int sayid { get; set; }

        public string vid { get; set; }

        public int pid { get; set; }

        public string content { get; set; }

        public long time { get; set; }

        public bool readed { get; set; }
    }

    public class sayListObj : v_Say
    {
        public v_UsersData usersData { get; set; }
    }

    public class reSay:v_Say
    {
        public string userName { get; set; }

        public v_Page pageData { get; set; }
    }
}
