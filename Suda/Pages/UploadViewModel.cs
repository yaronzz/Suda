using HandyControl.Controls;
using Suda.Else;
using Suda.Properties;
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

namespace Suda.Pages
{
    public class UploadViewModel : Suda.Else.ModelBase
    {
        public MainViewModel VMMain;
        public Platform Platform { get; set; }
        public Playlist PlaylistFrom { get; set; }
        public ObservableCollection<UploadItem> UploadItems { get; set; }
        public ObservableCollection<UploadItem> SuccessItems { get; set; }
        public ObservableCollection<UploadItem> ErrorItems { get; set; }

        public int NumWait { get; set; }
        public int NumSuccess { get; set; }
        public int NumUpdate { get; set; }
        public int NumError { get; set; }

        public bool IsCancel { get; set; }

        public class UploadItem : Stylet.Screen
        {
            public Track Track { get; set; }
            public string Status { get; set; } = Language.Get("strmsgWait");
            public SolidColorBrush StatusColor { get; set; } = Brushes.Gray;
            public string Platform { get; set; }
        }

        public void Load(Playlist playlist, MainViewModel main, Platform plat)
        {
            PlaylistFrom = playlist;
            Platform = plat;
            VMMain = main;
            IsCancel = false;
            SuccessItems = new ObservableCollection<UploadItem>();
            ErrorItems = new ObservableCollection<UploadItem>();
            UploadItems = new ObservableCollection<UploadItem>();

            for (int i = playlist.Tracks.Count - 1; i >= 0 ; i--)
            {
                Track item = playlist.Tracks[i];
                if (item.Check == false)
                    continue;

                UploadItems.Add(new UploadItem()
                {
                    Track = item,
                    Platform = plat.Name,
                });
            }

            NumWait = UploadItems.Count;
            NumSuccess = 0;
            NumUpdate = 0;
            NumError = 0;

            Upload();
        }

        public void Cancel()
        {
            IsCancel = true;
        }

        public async void Upload()
        {
            Global.InUploading = true;

            //get user playlist
            (string msg, ObservableCollection<Playlist> plAarray) = await SudaLib.Method.GetUserPlaylists(Platform.LoginKey);
            if(plAarray == null)
            {
                SetAllItemsError(Language.Get("strmsgGetUserPlaylistsFailed") + " " + msg);
                goto END_POINT;
            }

            //find playlist, if can't find, creat new one
            Playlist playlistTo = null;
            int index = VMMain.FindPlaylist(plAarray, PlaylistFrom);
            if (index >= 0)
                playlistTo = plAarray[index];
            else
            {
                (msg,playlistTo) = await SudaLib.Method.CreatPlaylist(Platform.LoginKey, PlaylistFrom.Title, PlaylistFrom.Desc);
                if (playlistTo == null)
                {
                    SetAllItemsError(Language.Get("strmsgCreatPlaylistFailed") + " " + msg);
                    goto END_POINT;
                }
            }

            //skip exist tracks
            for (int i = UploadItems.Count - 1; i >= 0; i--)
            {
                UploadItem item = UploadItems[i];
                if (VMMain.FindTrack(playlistTo.Tracks, item.Track) >= 0)
                {
                    item.Status = Language.Get("strmsgExist");
                    item.StatusColor = Brushes.Green;

                    NumSuccess++;
                    NumWait -= 1;

                    SuccessItems.Add(item);
                    UploadItems.RemoveAt(i);
                }
            }

            //upload
            while(UploadItems.Count > 0)
            {
                UploadItem item = UploadItems[0];
                Track track = await SudaLib.Method.SearchTrack(Platform.LoginKey, item.Track, Global.Settings.Compare);
                if (track == null)
                {
                    item.Status = Language.Get("strmsgCantFind");
                    item.StatusColor = Brushes.IndianRed;
                    NumError++;
                    ErrorItems.Add(item);
                    goto NEXT_POINT;
                }

                (string msg1,bool flag) = await SudaLib.Method.AddTracksToPlaylist(Platform.LoginKey, new string[] { track.MID }, playlistTo.MID);
                if (flag == false)
                {
                    item.Status = Language.Get("strmsgAddToPlaylistFailed") + " " + msg1;
                    item.StatusColor = Brushes.IndianRed;
                    NumError++;
                    ErrorItems.Add(item);
                    goto NEXT_POINT;
                }

                item.Status = Language.Get("strmsgSuccess");
                item.StatusColor = Brushes.Green;
                NumSuccess++;
                NumUpdate++;
                SuccessItems.Add(item);

            NEXT_POINT:
                NumWait -= 1;
                UploadItems.RemoveAt(0);

                //check cancel
                if (IsCancel)
                    SetAllItemsCancel();
            }

            //update user playlists
            (msg, plAarray) = await SudaLib.Method.GetUserPlaylists(Platform.LoginKey);
            if (plAarray != null)
                Platform.Playlists = plAarray;

        END_POINT:
            Global.InUploading = false;
        }


        public void SetAllItemsError(string errmsg)
        {
            while(UploadItems.Count > 0)
            {
                UploadItem item = UploadItems[0];

                item.Status = errmsg;
                item.StatusColor = Brushes.IndianRed;
                ErrorItems.Add(item);

                UploadItems.RemoveAt(0);
            }

            NumError = NumWait;
            NumWait = 0;
        }

        public void SetAllItemsCancel()
        {
            while (UploadItems.Count > 0)
            {
                UploadItem item = UploadItems[0];

                item.Status = Language.Get("strmsgCancel");
                item.StatusColor = Brushes.DarkOrange;
                ErrorItems.Add(item);

                UploadItems.RemoveAt(0);
            }

            NumError = NumWait;
            NumWait = 0;
        }
    }
}
