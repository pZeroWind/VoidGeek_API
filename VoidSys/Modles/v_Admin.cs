using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class v_Admin
    {
        [Key]
        public string vid { get; set; }

        public string password { get; set; }

        public string name { get; set; }

        public int role { get; set; }
    }

    public class v_AdminRes
    {

        public string name { get; set; }

        public int role { get; set; }

        public string token { get; set; }
    }
}
