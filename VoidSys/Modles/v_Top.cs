using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class v_Top
    {
        [Key]
        public int tid { get; set; }

        public int pid { get; set; }
    }
}
