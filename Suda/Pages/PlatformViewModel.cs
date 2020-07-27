using AIGS.Common;
using HandyControl.Controls;
using Suda.Else;
using SudaLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using static SudaLib.Common;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Suda.Pages
{
    public class PlatformViewModel : Stylet.Screen
    {
        public MainViewModel VMMain { get; set; }
        public Platform Platform { get; set; }
        public Playlist Playlist { get; set; }
        public int PlSelectIndex { get; set; }
        public bool AllCheck { get; set; }

        public void Load(Platform plat, MainViewModel main)
        {
            VMMain = main;
            Platform = plat;
            Playlist = null;
            AllCheck = false;

            if (Platform.Playlists.Count > 0)
            {
                PlSelectIndex = 0;
                Playlist = Platform.Playlists[0];
            }
            else
            {
                PlSelectIndex = -1;
                Playlist = null;
            }
        }

        //public void ClickAllCheck()
        //{
        //    if (Platform == null || Platform.Playlists == null)
        //        return;
        //    foreach (var item in Platform.Playlists)
        //    {
        //        item.Check = AllCheck;
        //    }
        //}

        public void PlaylistSelectChange()
        {
            if (PlSelectIndex < 0)
                return;
            Playlist = Platform.Playlists[PlSelectIndex];
        }


        #region Botton

        public void AskDeletePlaylist()
        {
            Dialog.Show(new MessageView(MessageBoxImage.Information, "Delete this playlist?", true, (x) =>
            {
                //Remove track from platform
                DeletePlaylist();
            }));
        }

        public async void DeletePlaylist()
        {
            (string msg, bool flag) = await Method.DeletePlaylist(Platform.LoginKey, Playlist.MID);
            if (flag == false)
            {
                Growl.Error("Delete playlist failed! " + msg, Global.TOKEN_MAIN);
                return;
            }
            Platform.Playlists.RemoveAt(PlSelectIndex);
            PlSelectIndex = 0;
        }

        public async void DeleteTrack(object MidArray)
        {
            foreach (var item in Playlist.Tracks)
            {
                if (Method.MatchMidArray((MIDArray)MidArray, item.MidArray))
                {
                    //Remove track from platform
                    (string msg, bool flag) = await Method.DelTracksFromPlaylist(Platform.LoginKey, new string[] { item.MID }, Playlist.MID);
                    if(flag == false)
                    {
                        Growl.Error("Delete track failed! " + msg, Global.TOKEN_MAIN);
                        return;
                    }

                    Playlist.Tracks.Remove(item);
                    return;
                }
            }
        }


        public void ToLocal(string PlaylistTitle = null)
        {
            if (Platform == null || Platform.Playlists == null)
                return;

            //Only one
            if(PlaylistTitle.IsNotBlank())
            {
                if(PlSelectIndex >= 0)
                    VMMain.SudaPlaylistAdd(new ObservableCollection<Playlist>() { Platform.Playlists[PlSelectIndex] });
                return;
            }

            //All
            VMMain.SudaPlaylistAdd(Platform.Playlists);
            return;
        }

        public async void RefreshPlaylist()
        {
            (string msg, ObservableCollection<Playlist> plist) = await SudaLib.Method.GetUserPlaylists(Platform.LoginKey);
            if(plist == null)
                Growl.Error("Refresh playlists err! " + msg, Global.TOKEN_MAIN);
            else
            {
                Platform.Playlists = plist;
                PlaylistSelectChange();
                Growl.Success("Refresh playlists success! ", Global.TOKEN_MAIN);
            }
        }

        public void Logout()
        {
            Dialog.Show(new MessageView(MessageBoxImage.Information, "Logout?", true, (x) =>
            {
                Platform.LoginKey = null;
                Platform.UserInfo = null;
                Platform.Playlists = null;

                Platform data = Platform;
                Load(null, VMMain);
                VMMain.MenuSelectPlatform(data);
            }));
        }
        #endregion
    }
}
