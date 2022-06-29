using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class LikeData
    {
        [Key]
        public int id { get; set; }
        public int pid { get; set; }
        public string vid { get; set; }

    }
}
