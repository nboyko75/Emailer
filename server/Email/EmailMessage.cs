using MailKit;
using MimeKit;

namespace MailServer
{
    public class EmailMessage
    {
        public EmailMessage(MimeMessage message, UniqueId uid, EmailAcct account)
        {
            Message = message;
            Uid = uid;
            Account = account;
        }

        public MimeMessage Message { get; }
        public UniqueId Uid { get; }
        public EmailAcct Account { get; }
        public MessageFlags? Flags { get; set; }
    }
}