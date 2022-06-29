using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class v_Letter
    {
        [Key]
        public int letterId { get; set; }
        public string fid { get; set; }
        public string vid { get; set; }
        public long time { get; set; }
        public string content { get; set; }
        public bool readed { get; set; }

    }
}
