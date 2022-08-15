using MailKit;
using System;

namespace MailServer.Dtos
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
        public DateTime ReceiveDate { get; set; }
        public bool Seen { get; set; }
        public bool IsSelected { get; set; } = false;

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
            res.ReceiveDate = msg.Message.Date.LocalDateTime;
            res.Seen = msg.Flags.Value.HasFlag(MessageFlags.Seen);
            return res;
        }
    }
}
