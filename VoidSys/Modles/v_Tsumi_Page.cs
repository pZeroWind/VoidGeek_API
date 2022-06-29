using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class v_Tsumi_Page
    {
        [Key]
        public int tsumiId { get; set; }

        public int pid { get; set; }

        public string vid { get; set; }

        public string content { get; set; }

        public long time { get; set; }

        public bool readed { get; set; }
    }

    public class ReTsumi : v_Tsumi_Page
    {
        public v_UsersData usersData { get; set; }

        public v_Page pageData { get; set; }
    }
}
