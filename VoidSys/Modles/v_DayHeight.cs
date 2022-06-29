using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class v_DayHeight
    {
        [Key]
        public int dhid { get; set; }

        public string dateTime { get; set; }

        public int num { get; set; }
    }
}
