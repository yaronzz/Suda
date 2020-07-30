using AIGS.Common;
using AIGS.Helper;
using Suda.Pages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Suda.Else
{
    public class Global : ViewMoudleBase
    {
        public static Cache Cache { get; set; }
        public static Settings Settings { get; set; }

        //Path
        public static string PATH_BASE = SystemHelper.GetUserFolders().PersonalPath + "/Suda/";
        public static string PATH_CACHE = PATH_BASE + "/data/cache.json";
        public static string PATH_SETTINGS = PATH_BASE + "/data/settings.json";
        public static string PATH_SUDA_PLAYLIST = PATH_BASE + "/data/sudaplaylist.json";

        //Token
        public static string TOKEN_PLATFORM = "PlatformToken";
        public static string TOKEN_MAIN = "MainToken";
        public static string TOKEN_PLAYLIST = "PlaylistToken";

        //KEY
        public static string KEY_CACHE = "38&*hjkfsau)(#";

        //Global DynamicResource
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        private static bool _InUploading = false;
        public static bool InUploading { get { return _InUploading; } set { _InUploading = value; OnPropertyChangedStatic(StaticPropertyChanged); } }
    }

}
