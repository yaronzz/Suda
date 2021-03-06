﻿using AIGS.Common;
using AIGS.Helper;
using Suda.Pages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Suda.Else
{
    public class Global : ViewMoudleBase
    {
        public static MainViewModel VMMain { get; set; }
        public static Cache Cache { get; set; }
        public static Settings Settings { get; set; }

        //Path
        public static string PATH_BASE = SystemHelper.GetUserFolders().PersonalPath + "\\Suda\\data\\";
        public static string PATH_CACHE = PATH_BASE + "cache.json";
        public static string PATH_SETTINGS = PATH_BASE + "settings.json";
        public static string PATH_SUDA_PLAYLIST = PATH_BASE + "sudaplaylist.json";
        public static string PATH_REQUIRE = Path.GetFullPath("./") + "require.zip";

        //url
        public static string URL_REQUIRE = "https://github.com/yaronzz/CDN/raw/master/app/suda/require.zip";
        public static string URL_SUDA_GROUP = "https://t.me/suda_group";
        public static string URL_SUDA_GITHUB = "https://github.com/yaronzz/Suda";
        public static string URL_SUDA_ISSUES = "https://github.com/yaronzz/Suda/issues";
        public static string URL_PAYPAL = "https://www.paypal.com/paypalme/yaronzz";

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
