using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class Coinget
    {
        [Key]
        public int id { get; set; }

        public string vid { get; set; }

        public string toid { get; set; }

        public long coin { get; set; }
        public long time { get; set; }

    }

    public class CoinRes : Coinget
    {
        public v_UsersData userData { get; set; }
    }
}
