using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music
{
    public class Track : Screen
    {
        public string Title { get; set; }
        public string AlbumTitle { get; set; }
        public string Artists { get; set; }
        public string Duration { get; set; }
    }

    public class Playlist : Screen
    {
        public string Title { get; set; }
        public string ImgUrl { get; set; }
        public string Author { get; set; }
        public string Desc { get; set; }
        public ObservableCollection<Track> Tracks { get; set; }
    }

}
