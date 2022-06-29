using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class v_Tag
    {
        [Key]
        public int tagid { get; set; }

        public string title { get; set; }
    }

    public class Tag_Data
    {
        public string name { get; set; }

        public int count { get; set; }
    }
}
