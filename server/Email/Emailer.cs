using MailKit;
using netServer.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace netServer
{
    public class Emailer
    {
        private int Timeout = 5000;
        private const string OutboxFolder = "Sent Items";
        private List<EmailMessage> messages;
        private bool _IsReceiveCompleted;
        private MailKit.Net.Imap.ImapClient _CurrentClient;
        private IMailFolder _CurrentFolder;
        private EmailAcct _CurrentAccount;
        private CancellationTokenSource _CurrentTokenSource;
        private int _ReceivedMsgCount;
        private Dictionary<string, string> _ErrorMessages;

        public MailKit.Net.Imap.ImapClient CurrentClient { get { return _CurrentClient; } }
        public IMailFolder CurrentFolder { get { return _CurrentFolder; } }
        public EmailAcct CurrentAccount { get { return _CurrentAccount; } }
        public CancellationTokenSource CurrentTokenSource { get { return _CurrentTokenSource; } }
        public int ReceivedMsgCount { get { return _ReceivedMsgCount; } }
        public bool IsReceiveCompleted { get { return _IsReceiveCompleted; } }

        public Emailer(AppSettings config)
        {
            Timeout = config.EmailTimeout;
            _CurrentClient = new MailKit.Net.Imap.ImapClient();
            _CurrentClient.Timeout = Timeout;
            _IsReceiveCompleted = true;
            messages = new List<EmailMessage>();
            _ErrorMessages = new Dictionary<string, string>();
        }

        public async Task<IEnumerable<EmailMessage>> ConnectAndRetrieve(List<EmailAcct> acctList, bool isOutbox)
        {
            messages.Clear();
            ClearErrors();
            _IsReceiveCompleted = false;
            foreach (EmailAcct acct in acctList)
            {
                await connectAndRetrieve(acct, isOutbox);
            }
            _IsReceiveCompleted = true;
            return messages;
        }

        public async Task<IEnumerable<EmailMessage>> ConnectAndRetrieve(EmailAcct account, bool isOutbox)
        {
            messages.Clear();
            ClearErrors();
            _IsReceiveCompleted = false;
            await connectAndRetrieve(account, isOutbox);
            _IsReceiveCompleted = true;
            return messages;
        }

        private async Task connectAndRetrieve(EmailAcct account, bool isOutbox)
        {
            try
            {
                _ReceivedMsgCount = 0;
                _CurrentTokenSource = new CancellationTokenSource();
                _CurrentAccount = account;

                await _CurrentClient.ConnectAsync(_CurrentAccount.ImapServer, _CurrentAccount.ImapPort, _CurrentAccount.Secure, _CurrentTokenSource.Token);
                await _CurrentClient.AuthenticateAsync(_CurrentAccount.Username, _CurrentAccount.Password, _CurrentTokenSource.Token);

                _CurrentFolder = isOutbox ? _CurrentClient.GetFolder(OutboxFolder) : _CurrentClient.Inbox;
                _CurrentFolder.CountChanged += OnCountChanged;

                await _CurrentFolder.OpenAsync(FolderAccess.ReadOnly, _CurrentTokenSource.Token);

                var msgs = _GetMessages(_CurrentFolder, _CurrentTokenSource, _CurrentAccount);
                messages.AddRange(msgs);

                _ReceivedMsgCount = CurrentFolder.Count;
                await _CurrentClient.DisconnectAsync(true, _CurrentTokenSource.Token);
            }
            catch (Exception ex)
            {
                SetError(account.Name, ex.Message);
            }
            finally
            {
                if (_CurrentClient.IsConnected)
                {
                    await _CurrentClient.DisconnectAsync(true, _CurrentTokenSource.Token);
                }
            }
        }

        public void DeleteMessage(EmailMessage msg, bool isOutbox)
        {
            string _ErrorMessage = string.Empty;
            try
            {
                var client = new MailKit.Net.Imap.ImapClient();
                var cancel = new CancellationTokenSource();
                client.Timeout = Timeout;
                client.Connect(msg.Account.ImapServer, msg.Account.ImapPort, msg.Account.Secure, cancel.Token);
                client.Authenticate(msg.Account.Username, msg.Account.Password, cancel.Token);
                var folder = isOutbox ? client.GetFolder(SpecialFolder.Sent) : client.Inbox;
                folder.Open(FolderAccess.ReadWrite, cancel.Token);
                folder.AddFlags(msg.Uid, MessageFlags.Deleted, true);
                client.Inbox.Expunge(new UniqueId[] { msg.Uid });
                client.Disconnect(true, cancel.Token);
            }
            catch (Exception ex)
            {
                SetError(msg.Account.Name, ex.Message);
            }
        }

        public void MarkMessageAsSeen(EmailMessage msg, bool isOutbox)
        {
            string _ErrorMessage = string.Empty;
            try
            {
                var client = new MailKit.Net.Imap.ImapClient();
                var cancel = new CancellationTokenSource();
                client.Timeout = Timeout;
                client.Connect(msg.Account.ImapServer, msg.Account.ImapPort, msg.Account.Secure, cancel.Token);
                client.Authenticate(msg.Account.Username, msg.Account.Password, cancel.Token);
                var folder = isOutbox ? client.GetFolder(SpecialFolder.Sent) : client.Inbox;
                folder.Open(FolderAccess.ReadWrite, cancel.Token);
                folder.AddFlags(msg.Uid, MessageFlags.Seen, true);
                client.Disconnect(true, cancel.Token);
            }
            catch (Exception ex)
            {
                SetError(msg.Account.Name, ex.Message);
            }
        }

        public string Send(EmailAcct account, System.Net.Mail.MailMessage mailMsg)
        {
            string _ErrorMessage = string.Empty;
            var smtpClient = new System.Net.Mail.SmtpClient(account.SmtpServer)
            {
                Port = account.SmtpPort,
                Credentials = new NetworkCredential(account.Username, account.Password),
                EnableSsl = true,
                Timeout = Timeout
            };

            try
            {
                smtpClient.Send(mailMsg);
            }
            catch (Exception ex)
            {
                SetError(account.Name, ex.Message);
            }
            return _ErrorMessage;
        }

        public string GetErrorMessage(string accountName)
        {
            string msg = string.Empty;
            if (_ErrorMessages.ContainsKey(accountName))
            {
                msg = _ErrorMessages[accountName];
                if (msg.Contains("timeout"))
                {
                    msg = string.Empty;
                }
            }
            return msg;
        }

        public static string AddrListToStr(MimeKit.InternetAddressList list, bool showAddress)
        {
            StringBuilder sRes = new StringBuilder();
            if (list.Count > 0)
            {
                bool isFirst = true;
                foreach (var ma in list)
                {
                    if (!isFirst)
                    {
                        sRes.Append(", ");
                    }
                    else
                    {
                        isFirst = false;
                    }
                    string addr = ma.Name;
                    if (showAddress)
                    {
                        MimeKit.MailboxAddress mba = ma as MimeKit.MailboxAddress;
                        if (mba != null)
                        {
                            addr = mba.Address;
                        }
                    }
                    sRes.Append(addr);
                }
            }
            return sRes.ToString();
        }

        public List<EmailMessage> GetMessages()
        {
            return messages;
        }

        private List<EmailMessage> _GetMessages(IMailFolder folder, CancellationTokenSource cancel, EmailAcct account)
        {
            List<EmailMessage> mm = new List<EmailMessage>();
            // fetch some useful metadata about each message in the folder...
            var items = folder.Fetch(0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.Size |
                MessageSummaryItems.Flags, cancel.Token);

            // iterate over all of the messages and fetch them by UID
            foreach (var item in items)
            {
                var msg = folder.GetMessage(item.UniqueId, cancel.Token);
                EmailMessage em = new EmailMessage(msg, item.UniqueId, account)
                {
                    Flags = item.Flags
                };
                mm.Add(em);
            }
            return mm;
        }

        private void OnCountChanged(object sender, EventArgs e)
        {
            _ReceivedMsgCount++;
        }

        private void ClearErrors()
        {
            _ErrorMessages.Keys.ToList().ForEach(x => _ErrorMessages[x] = string.Empty);
        }

        private void SetError(string accountName, string msg)
        {
            _ErrorMessages[accountName] = msg;
        }
    }
}