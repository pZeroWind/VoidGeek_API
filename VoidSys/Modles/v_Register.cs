using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class v_Rigster
    {
        public string password { get; set; }

        public string userName { get; set; }

        public bool gender { get; set; }

        public long birthday { get; set; }

        public string email { get; set; }

        public string phoneNum { get; set; }
    }
}
