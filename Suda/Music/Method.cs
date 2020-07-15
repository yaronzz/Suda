using AIGS.Common;
using AIGS.Helper;
using Music.QQMusic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music
{
    public class Method
    {
        public static bool IsLive(string sSongTitle)
        {
            if (sSongTitle.IsBlank())
                return false;
            if (sSongTitle.ToLower().Contains("(live)"))
                return true;
            return false;
        }

        public static string RemoveLiveFlag(string sSongTitle)
        {
            string flag = "(live)";
            int index = sSongTitle.ToLower().IndexOf(flag);
            if (index < 0)
                return sSongTitle;

            string ret = sSongTitle.Substring(0, index);
            return ret.Trim();
        }

        public static Playlist LoadPlaylist(QQMusic.Playlist playlist)
        {
            Playlist ret = new Playlist()
            {
                Title = playlist.DissName,
                ImgUrl = playlist.Logo,
                Author = playlist.Nick,
                Desc = playlist.Desc,
                Tracks = new ObservableCollection<Track>(),
            };
            foreach (var item in playlist.SongList)
            {
                Track obj = new Track();
                obj.Title = item.SongName;
                obj.AlbumTitle = item.AlbumName;
                obj.Duration = TimeHelper.ConverIntToString(item.Interval);
                for (int i = 0; i < item.Singer.Count(); i++)
                    obj.Artists += "\\" + item.Singer[i].Name;
                obj.Artists = obj.Artists.Substring(1);
                ret.Tracks.Add(obj);
            }
            return ret;
        }

        public static ObservableCollection<Playlist> LoadPlaylist(QQMusic.Playlist[] playlist)
        {
            ObservableCollection<Playlist> ret = new ObservableCollection<Playlist>();
            for (int i = 0; i < playlist.Count(); i++)
            {
                ret.Add(LoadPlaylist(playlist[i]));
            }
            return ret;
        }
    }
}
