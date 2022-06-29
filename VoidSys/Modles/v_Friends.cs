using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class v_Friends
    {
        [Key]
        public int fid { get; set; }

        public string vid_me { get; set; }

        public string vid_friend { get; set; }

    }
}
