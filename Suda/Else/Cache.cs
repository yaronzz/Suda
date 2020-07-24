using AIGS.Helper;
using SudaLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Suda.Else
{
    public class Cache
    {
        public QQMusic.LoginKey QQLoginkey { get; set; }
        public CloudMusic.LoginKey CloudLoginkey { get; set; }
        public Tidal.LoginKey TidalLoginkey { get; set; }
        public Spotify.LoginKey SpotifyLoginkey { get; set; }

        public void Write()
        {
            string data = JsonHelper.ConverObjectToString<Cache>(this);
            FileHelper.Write(data, true, Global.PATH_CACHE);
        }

        public static Cache Read()
        {
            string data = FileHelper.Read(Global.PATH_CACHE);
            Cache ret = JsonHelper.ConverStringToObject<Cache>(data);
            if (ret == null)
                return new Cache();
            return ret;
        }

    }
}
