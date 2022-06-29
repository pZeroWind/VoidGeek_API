using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VoidSys.Modles;

namespace VoidSys.Service
{
    public class EmailSend
    {

        static string host = "smtp.qq.com";
        static string emil = "prozerowind@foxmail.com";
        static string pwd = "gstndmuslnaxggfg";
        /// <summary>
        /// 发送验证短信给目标邮件地址
        /// </summary>
        /// <param name="Address"></param>
        /// <returns></returns>
        public static Task<Result<string>> toEmil(string Address)
        {
            var task = Task.Run(() =>
            {   
                SmtpClient sct = new SmtpClient();
                Result<string> result = new Result<string>();
                //设置email的基本信息
                sct.Host = host;
                sct.Port = 587;//端口号
                MailAddress me = new MailAddress(emil);
                MailAddress you = new MailAddress(Address);
                MailMessage mm = new MailMessage(me, you);
                //邮件标题
                mm.Subject = "Void密码验证";
                //编码方式
                mm.SubjectEncoding = Encoding.UTF8;
                //邮件内容
                string pass = GetPass();
                mm.Body = "您的验证码为：" + pass;
                mm.BodyEncoding = Encoding.UTF8;
                result.data = pass;
                sct.DeliveryMethod = SmtpDeliveryMethod.Network;
                //发送
                try
                {
                    sct.EnableSsl = true;
                    sct.UseDefaultCredentials = false;
                    NetworkCredential nc = new NetworkCredential(emil, pwd);
                    sct.Credentials = nc;
                    sct.Send(mm);
                }
                catch (Exception e)
                {
                    result.msg = e.Message;
                }
                return result;
            });
            return task;
        }

        public static Task<Result<string>> toEmil(string Address,double coin)
        {
            var task = Task.Run(() =>
            {
                SmtpClient sct = new SmtpClient();
                Result<string> result = new Result<string>();
                //设置email的基本信息
                sct.Host = host;
                sct.Port = 587;//端口号
                MailAddress me = new MailAddress(emil);
                MailAddress you = new MailAddress(Address);
                MailMessage mm = new MailMessage(me, you);
                //邮件标题
                mm.Subject = "Void提现提醒";
                //编码方式
                mm.SubjectEncoding = Encoding.UTF8;
                //邮件内容
                string pass = GetPass();
                mm.Body = "提现成功！提现金额：" + coin + "元，请注意查收。";
                mm.BodyEncoding = Encoding.UTF8;
                result.data = pass;
                sct.DeliveryMethod = SmtpDeliveryMethod.Network;
                //发送
                try
                {
                    sct.EnableSsl = true;
                    //sct.UseDefaultCredentials = false;
                    NetworkCredential nc = new NetworkCredential(emil, pwd);
                    sct.Credentials = nc;
                    sct.Send(mm);
                }
                catch (Exception e)
                {
                    result.msg = e.Message;
                }
                return result;
            });
            return task;
        }

        //构建一个随机的四位字符作为验证码
        private static string GetPass()
        {
            string[] Is = new string[4];
            for (int i = 0; i < Is.Length; i++)
            {
                int num = new Random().Next(10);
                if (num > 7 || num == 0)
                {
                    Is[i] = new Random().Next(10).ToString();
                }
                else if (num < 3)
                {
                    Is[i] = ((char)('A' + new Random().Next(26))).ToString();
                }
                else
                {
                    Is[i] = ((char)('a' + new Random().Next(26))).ToString();
                }
                Thread.Sleep(100);
            }
            string result = "";
            foreach (string item in Is)
            {
                result += item;
            }
            return result;
        }
    }
}
