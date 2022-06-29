using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class Use_Tag
    {
        [Key]
        public int id { get; set; }
        public string tagName { get; set; }
        public int pid { get; set; }

    }
}
