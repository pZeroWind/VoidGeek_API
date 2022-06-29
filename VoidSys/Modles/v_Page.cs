using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class v_Page
    {
        [Key]
        public int pid { get; set; }
        public string vid { get; set; }
        public string aid { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public int sayNum { get; set; }
        public int saveNum { get; set; }
        public int likeNum { get; set; }
        public int readNum { get; set; }
        public long time { get; set; }
        public bool pass { get; set; }
        public string post { get; set; }
    }

    public class v_PageRes:v_Page
    {
        public string userName { get; set; }
        public string areaName { get; set; }
        public List<string> tag { get; set; }
    }

    public class updatePage:v_Page
    {
        public List<string> tag { get; set; }
    }
}
