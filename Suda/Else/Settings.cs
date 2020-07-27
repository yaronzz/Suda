using AIGS.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SudaLib.Common;

namespace Suda.Else
{
    public class Settings : Stylet.Screen
    {
        public CompareArgs Compare { get; set; } = new CompareArgs();

        public void Write()
        {
            string data = JsonHelper.ConverObjectToString<Settings>(this);
            FileHelper.Write(data, true, Global.PATH_SETTINGS);
        }

        public static Settings Read()
        {
            string data = FileHelper.Read(Global.PATH_SETTINGS);
            Settings ret = JsonHelper.ConverStringToObject<Settings>(data);
            if (ret == null)
                return new Settings();
            return ret;
        }
    }
}
