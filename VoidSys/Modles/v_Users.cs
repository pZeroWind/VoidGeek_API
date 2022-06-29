using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class v_Users
    {
        [Key]
        public string vid { get; set; }

        public string password { get; set; }

        public bool pass { get; set; }
    }
}
