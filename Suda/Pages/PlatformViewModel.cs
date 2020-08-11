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
    public class PlatformViewModel : Suda.Else.ModelBase
    {
        public Platform Platform { get; set; }
        public Playlist Playlist { get; set; }
        public int PlSelectIndex { get; set; }
        public bool AllCheck { get; set; }
        public bool AlreadyLoad { get; set; }
        public Visibility LoadingVisibility { get; set; }

        public async void Load(Platform plat)
        {
            if (AlreadyLoad)
                return;

            Platform = plat;
            Playlist = null;
            AllCheck = false;
            PlSelectIndex = 0;
            Playlist = null;
            LoadingVisibility = Visibility.Hidden;

            if (Platform != null)
            {
                string msg;
                if (Platform.UserInfo == null || Platform.Playlists == null)
                {
                    LoadingVisibility = Visibility.Visible;
                    if (Platform.UserInfo == null)
                        (msg, Platform.UserInfo) = await Method.GetUserInfo(Platform.LoginKey);
                    if (Platform.Playlists == null)
                        (msg, Platform.Playlists) = await Method.GetUserPlaylists(Platform.LoginKey);
                    LoadingVisibility = Visibility.Hidden;
                }
                
                if (Platform.Playlists != null && Platform.Playlists.Count > 0)
                {
                    PlSelectIndex = 0;
                    Playlist = Platform.Playlists[0];
                }

                AlreadyLoad = true;
            }
        }

        public void PlaylistSelectChange()
        {
            if (PlSelectIndex < 0)
                return;
            Playlist = Platform.Playlists[PlSelectIndex];
        }


        #region Botton

        public void AskDeletePlaylist()
        {
            Dialog.Show(new MessageView(MessageBoxImage.Information, Language.Get("strmsgDeleteThisPlaylist"), true, (x) =>
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
                Growl.Error(Language.Get("strmsgDeletePlaylistFailed") + " " + msg, Global.TOKEN_MAIN);
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
                        Growl.Error(Language.Get("strmsgDeleteTrackFailed") + " " + msg, Global.TOKEN_MAIN);
                        return;
                    }

                    Playlist.Tracks.Remove(item);
                    return;
                }
            }
        }

        public void ImportTo(string PlaylistTitle = null)
        {
            Global.VMMain.SudaPlaylistUpload(Platform.Playlists[PlSelectIndex], Platform.Type);
        }

        public void ToLocal(string PlaylistTitle = null)
        {
            if (Platform == null || Platform.Playlists == null)
                return;

            //Only one
            if(PlaylistTitle.IsNotBlank())
            {
                if(PlSelectIndex >= 0)
                    Global.VMMain.SudaPlaylistAdd(new ObservableCollection<Playlist>() { Platform.Playlists[PlSelectIndex] });
                return;
            }

            //All
            Global.VMMain.SudaPlaylistAdd(Platform.Playlists);
            return;
        }

        public async void RefreshPlaylist()
        {
            (string msg, ObservableCollection<Playlist> plist) = await SudaLib.Method.GetUserPlaylists(Platform.LoginKey);
            if(plist == null)
                Growl.Error(Language.Get("strmsgeRefreshPlaylistsFailed") + " " + msg, Global.TOKEN_MAIN);
            else
            {
                Platform.Playlists = plist;
                PlaylistSelectChange();
                Growl.Success(Language.Get("strmsgeRefreshPlaylistsSuccess"), Global.TOKEN_MAIN);
            }
        }

        public void Logout()
        {
            Dialog.Show(new MessageView(MessageBoxImage.Information, Language.Get("strmsgLogout"), true, (x) =>
            {
                if (Platform != null)
                {
                    Platform.LoginKey = null;
                    Platform.UserInfo = null;
                    Platform.Playlists = null;
                }
                AlreadyLoad = false;

                Platform data = Platform;
                Load(null);
                Global.VMMain.MenuSelectPlatform(data);
            }));
        }
        #endregion
    }
}
