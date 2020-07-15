using AIGS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music
{
    public class Method
    {
        public static bool IsLive(string sSongTitle)
        {
            if (sSongTitle.IsBlank())
                return false;
            if (sSongTitle.ToLower().Contains("(live)"))
                return true;
            return false;
        }

        public static string RemoveLiveFlag(string sSongTitle)
        {
            string flag = "(live)";
            int index = sSongTitle.ToLower().IndexOf(flag);
            if (index < 0)
                return sSongTitle;

            string ret = sSongTitle.Substring(0, index);
            return ret.Trim();
        }
    }
}
