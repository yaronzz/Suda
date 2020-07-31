using Stylet;
using Suda.Pages;
using SudaLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static SudaLib.Common;

namespace Suda.Else
{
    public class Platform  : Screen
    {
        public ePlatform Type { get;set; }
        public Geometry Logo { get; set; }
        public string Name { get; set; }

        public object LoginKey { get; set; }
        public UserInfo UserInfo { get; set; }
        public ObservableCollection<Playlist> Playlists { get; set; }

        public PlatformViewModel VMPlatform { get; set; }


        public static string GetPlatformDisplayName(ePlatform type)
        {
            if (type == ePlatform.CloudMusic)
                return Language.Get("strCloudMusic");
            else if (type == ePlatform.QQMusic)
                return Language.Get("strQQMusic");
            else if (type == ePlatform.Spotify)
                return Language.Get("strSpotify");
            else if (type == ePlatform.Tidal)
                return Language.Get("strTidal");
            else
                return "Unknow";
        }
    }
}
