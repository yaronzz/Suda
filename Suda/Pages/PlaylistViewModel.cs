using HandyControl.Controls;
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
    public class PlaylistViewModel : Stylet.Screen
    {
        public MainViewModel VMMain { get; set; }
        public Playlist Playlist { get; set; }
        public bool AllCheck { get; set; }

        public void Load(Playlist data, MainViewModel main)
        {
            Playlist = data;
            AllCheck = false;
            VMMain = main;
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
            VMMain.SudaPlaylistUpload(Playlist);
            return;
        }

        public void DeletePlaylist()
        {
            Dialog.Show(new MessageView(MessageBoxImage.Information, "Delete this playlist?", true, (x) =>
            {
                VMMain.SudaPlaylistDel(Playlist);
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
