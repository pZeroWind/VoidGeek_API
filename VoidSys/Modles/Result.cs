using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class Result<T>
    {
        public int code { set; get; } = 200;
        public string msg { set; get; } = "操作成功";
        public T data { set; get; }
    }

    public class ResultImg<T> : Result<T>
    {
        public int errno { set; get; } = 0;
    }

    public class ResultPage<T> : Result<T>
    {
        public int limt { set; get; }
        public int page { set; get; }
        public int total { get; set; }
    }

    public class ResData {
        public int newSay { get; set; }

        public int newReSay { get; set; }

        public int newLetter { get; set; }

        public int newPost { get; set; }

        public int allConut { get; set; }
    }

    public class SendData
    {
        public string fid { get; set; }

        public string content { get; set; }
    }
}
