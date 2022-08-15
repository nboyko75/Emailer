using System.Collections.Generic;

namespace MailServer.Utils
{
    public class StringUtils
    {
        public static uint[] GetIntArrayFromString(string str) 
        {
            List<uint> lint = new List<uint>();
            if (!string.IsNullOrEmpty(str)) 
            {
                string[] sids = str.Split();
                foreach (string sid in sids) 
                {
                    if (uint.TryParse(sid, out uint id)) 
                    {
                        lint.Add(id);
                    }
                }
            }
            return lint.ToArray();
        }
    }
}
