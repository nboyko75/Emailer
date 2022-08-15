namespace MailServer.Params
{
    public class GetParams
    {
        public string AccountName { get; set; }

        public static string CheckParams(GetParams prms)
        {
            if (prms == null || string.IsNullOrEmpty(prms.AccountName))
            {
                return "Host name is unknown";
            }
            return null;
        }
    }
}
