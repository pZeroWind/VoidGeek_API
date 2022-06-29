using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class v_Resay
    {
        [Key]
        public int sayid { get; set; }

        public string vid { get; set; }

        public int sid { get; set; }

        public string toid { get; set; }

        public string reContent { get; set; }

        public string content { get; set; }

        public long time { get; set; }

        public bool readed { get; set; }
    }

    public class ReSayListObj : v_Resay
    {
        public v_UsersData userData { get; set; }


        public v_UsersData toUserData { get; set; }
    }


    public class resResay : v_Resay
    {
        public string userName { get; set; }

        public v_Say sayData { get; set; }

        public v_Page pageData { get; set; }
    }
}
