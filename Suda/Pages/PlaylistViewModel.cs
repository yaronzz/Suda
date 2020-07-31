using HandyControl.Controls;
using HandyControl.Tools.Extension;
using Suda.Else;
using SudaLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static SudaLib.Common;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Suda.Pages
{
    public class PlaylistViewModel : Suda.Else.ModelBase
    {
        public Playlist Playlist { get; set; }
        public bool AllCheck { get; set; }

        public void Load(Playlist data)
        {
            Playlist = data;
            AllCheck = false;
        }

        public void ClickAllCheck()
        {
            if (Playlist == null || Playlist.Tracks == null)
                return;
            foreach (var item in Playlist.Tracks)
            {
                item.Check = AllCheck;
            }
        }

        public void Upload()
        {
            int iNum = 0;
            foreach (var item in Playlist.Tracks)
            {
                if (item.Check)
                    iNum++;
            }
            if (iNum <= 0)
                Growl.Error(Language.Get("strmsgPleaseselectTracks"), Global.TOKEN_PLAYLIST);
            else
                Global.VMMain.SudaPlaylistUpload(Playlist);
            return;
        }

        public void DeletePlaylist()
        {
            Dialog.Show(new MessageView(MessageBoxImage.Information, Language.Get("strmsgDeleteThisPlaylist"), true, (x) =>
            {
                Global.VMMain.SudaPlaylistDel(Playlist);
                Playlist = null;
            }));
        }


        public void Delete(object MidArray)
        {
            foreach (var item in Playlist.Tracks)
            {
                if(Method.MatchMidArray((MIDArray)MidArray, item.MidArray))
                {
                    Playlist.Tracks.Remove(item);
                    return;
                }
            }
        }
    }
}
