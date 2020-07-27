using AIGS.Common;
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
        public static string PATH_CACHE = "./data/cache.json";
        public static string PATH_SETTINGS = "./data/settings.json";
        public static string PATH_SUDA_PLAYLIST = "./data/sudaplaylist.json";

        //Token
        public static string TOKEN_PLATFORM = "PlatformToken";
        public static string TOKEN_MAIN = "MainToken";
        public static string TOKEN_PLAYLIST = "PlaylistToken";

        //Global DynamicResource
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        private static bool _InUploading = false;
        public static bool InUploading { get { return _InUploading; } set { _InUploading = value; OnPropertyChangedStatic(StaticPropertyChanged); } }
    }

}
