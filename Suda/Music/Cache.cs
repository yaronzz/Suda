using AIGS.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music
{
    public class Cache
    {
        public string QQCookie { get; set; }
        public string QQNumber { get; set; }
        public string QQGTK { get; set; }
        public static string Path { get; set; } = ".\\Cache.json";




        public static Cache Read()
        {
            string sText = FileHelper.Read(Path);
            Cache rObj = JsonHelper.ConverStringToObject<Cache>(sText);
            return rObj;
        }

        public static void Write(Cache wObj)
        {
            string sText = JsonHelper.ConverObjectToString<Cache>(wObj);
            FileHelper.Write(sText, true, Path);
        }
    }
}
