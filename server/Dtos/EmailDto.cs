using System;

namespace netServer.Dtos
{
    public class EmailDto
    {
        public int Id { get; set; }
        public string AccountName { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string CC { get; set; }
        public string Subject { get; set; }
        public string HtmlBody { get; set; }

        public static EmailDto FromMessage(EmailMessage msg) 
        {
            EmailDto res = new EmailDto();
            res.Id = Convert.ToInt32(msg.Uid.Id);
            res.AccountName = msg.Account.Name;
            res.From = Emailer.AddrListToStr(msg.Message.From, true);
            res.To = Emailer.AddrListToStr(msg.Message.To, true);
            res.CC = Emailer.AddrListToStr(msg.Message.From, true);
            res.Subject = msg.Message.Subject;
            res.HtmlBody = msg.Message.HtmlBody;
            return res;
        }
    }
}
