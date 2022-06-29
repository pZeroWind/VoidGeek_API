using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class SaveFolder
    {
        [Key]
        public int fsId { get; set; }

        public string vid { get; set; }

        public string folderName { get; set; }
    }
}
