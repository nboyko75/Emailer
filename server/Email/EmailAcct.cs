namespace netServer
{
    public class EmailAcct
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ImapServer { get; set; }
        public string SmtpServer { get; set; }
        public int ImapPort { get; set; }
        public int SmtpPort { get; set; }
        public bool Secure { get; set; }

        public string Email
        { 
            get
            {
                return $"{Username}@{Host}";
            }
        }
    }
}