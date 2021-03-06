using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class History
    {
        [Key]
        public int hId { get; set; }

        public string vid { get; set; }

        public int pid { get; set; }

        public long time { get; set; }
    }

    public class HisRes : History
    {
        public v_Page pageData { get; set; }
    }
}
