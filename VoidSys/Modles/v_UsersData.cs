using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class v_UsersData
    {
        [Key]
        public string vid { get; set; }

        public string userName { get; set; }

        public bool gender { get; set; }

        public long birthday { get; set; }

        public string email { get; set; }

        public string phoneNum { get; set; }

        public int fanNum { get; set; }

        public int exc { get; set; }

        public string imgUrl { get; set; }

        public string resume { get; set; }

        public long coin { get; set; }

        public string cardNum { get; set; }
    }
}
