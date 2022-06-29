using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class v_Area
    {
        [Key]
        public string aid { get; set; }

        public string areaName { get; set; }

        public int pageNum { get; set; }
        
        public string iconUrl { get; set; }

    }
}
