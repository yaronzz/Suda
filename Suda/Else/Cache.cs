using AIGS.Common;
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

        public void Save(object Logkey)
        {
            ePlatform type = Method.GetPlatform(Logkey);
            if (type == ePlatform.CloudMusic)
                CloudLoginkey = (CloudMusic.LoginKey)Logkey;
            else if (type == ePlatform.QQMusic)
                QQLoginkey = (QQMusic.LoginKey)Logkey;
            else if (type == ePlatform.Tidal)
                TidalLoginkey = (Tidal.LoginKey)Logkey;
            else if (type == ePlatform.Spotify)
                SpotifyLoginkey = (Spotify.LoginKey)Logkey;

            string data = JsonHelper.ConverObjectToString<Cache>(this);
            string edata = EncryptHelper.Encode(data, Global.KEY_CACHE);
            FileHelper.Write(edata, true, Global.PATH_CACHE);
        }

        public static Cache Read()
        {
            string edata = FileHelper.Read(Global.PATH_CACHE);
            string data = EncryptHelper.Decode(edata, Global.KEY_CACHE);
            Cache ret = JsonHelper.ConverStringToObject<Cache>(data);
            if (ret == null)
                return new Cache();
            return ret;
        }

    }
}
