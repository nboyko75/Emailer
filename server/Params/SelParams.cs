using MailServer.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MailServer.Params
{
    public class SelParams
    {
        public string AccountName { get; set; }
        public string ids { get; set; }
        public uint[] uids { get { return StringUtils.GetIntArrayFromString(ids); } }

        public static string CheckParams(SelParams prms)
        {
            if (prms == null)
            {
                return "Host name and ids are unknown";
            }
            else if (string.IsNullOrEmpty(prms.AccountName))
            {
                return "Host name is unknown";
            }
            else if (string.IsNullOrEmpty(prms.ids))
            {
                return "Message ids is not set";
            }
            return null;
        }
    }
}
