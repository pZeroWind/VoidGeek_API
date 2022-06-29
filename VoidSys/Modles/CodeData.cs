using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class CodeData
    {
        [Key]
        public string CodeKey { get; set; }

        public v_Users userData { get; set; } = new v_Users();

        public string token { get; set; } = "";

        public bool status { get; set; } = false;
    }
}
