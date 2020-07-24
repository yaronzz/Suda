using Stylet;
using SudaLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
