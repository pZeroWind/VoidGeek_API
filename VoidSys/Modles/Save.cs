using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class Save
    {
        [Key]
        public int saveId { get; set; }

        public int pid { get; set; }

        public int folderId { get; set; }
    }
}
