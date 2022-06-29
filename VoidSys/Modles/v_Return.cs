using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class v_Return
    {
        [Key]
        public int returnId { get; set; }

        public string vid { get; set; }

        public string title { get; set; }

        public string content { get; set; }

        public long time { get; set; }

        public bool readed { get; set; }
    }

    public class ReturnData : v_Return
    {
        public v_UsersData usersData { get; set; }
    }
}
