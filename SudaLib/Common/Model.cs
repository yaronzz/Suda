using AIGS.Common;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudaLib
{
    public class Common
    {
        public class UserInfo 
        {
            public string NickName { get; set; }
            public string UserID { get; set; }
            public string AvatarUrl { get; set; }

        }

        public class Playlist : ViewMoudleBase
        {
            private bool check;
            public bool Check { get { return check; } set { check = value; OnPropertyChanged(); } } 

            public string Title { get; set; }
            public string ID { get; set; }
            public string MID { get; set; }
            public object ImgUrl { get; set; }
            public string Desc { get; set; }

            public string CreatorName { get; set; }
            public string CreatorID { get; set; }

            public ObservableCollection<Track> Tracks { get; set; }

            public MIDArray MidArray { get; set; } = new MIDArray();
            public bool MyFavorite { get; set; }

            private object vmmodel { get; set; }
            public object VMModel(object data = null)
            {
                if (data != null) vmmodel = data;
                return vmmodel;
            }
        }


        public class Track : ViewMoudleBase
        {
            private bool check = true;
            public bool Check { get { return check; } set { check = value; OnPropertyChanged(); } }

            public string Title { get; set; }
            public string TitleBrief { get; set; }
            public bool Live { get; set; }

            public string ID { get; set; }
            public string MID { get; set; }
            public int Duration { get; set; }
            public string DurationStr { get; set; }

            public string AlbumID { set; get; }
            public string AlbumTitle { set; get; }

            public ObservableCollection<Artist> Artists { get; set; }
            public string ArtistsName { get; set; } 

            public MIDArray MidArray { get; set; } = new MIDArray();
        }


        public class Artist 
        {
            public string Name { get; set; }
            public string ID { get; set; }
            public string MID { get; set; }
        }


        public class MIDArray 
        {
            public string Tidal { get; set; }
            public string Spotify { get; set; }
            public string QQMusic { get; set; }
            public string CloudMusic { get; set; }
            public string AppleMusic { get; set; }
        }


        public class CompareArgs
        {
            public bool AlbumTitle { get; set; } = true;
            public bool Live { get; set; }
            
        }
    }
}
