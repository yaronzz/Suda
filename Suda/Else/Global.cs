using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Suda.Else
{
    public static class Global
    {
        public static Cache Cache { get; set; }
        public static Settings Settings { get; set; }

        //Path
        public static string PATH_CACHE = "./data/cache.json";
        public static string PATH_SETTINGS = "./data/settings.json";
        public static string PATH_SUDA_PLAYLIST = "./data/sudaplaylist.json";

        //Token
        public static string TOKEN_PLATFORM = "PlatformToken";
        public static string TOKEN_MAIN = "MainToken";
        public static string TOKEN_PLAYLIST = "PlaylistToken";
    }
}
