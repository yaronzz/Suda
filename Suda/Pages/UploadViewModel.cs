using HandyControl.Controls;
using Suda.Else;
using SudaLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static SudaLib.Common;

namespace Suda.Pages
{
    public class UploadViewModel : Stylet.Screen
    {
        public MainViewModel VMMain;
        public Platform Platform { get; set; }
        public Playlist PlaylistFrom { get; set; }
        public ObservableCollection<UploadItem> UploadItems { get; set; }
        public ObservableCollection<UploadItem> SuccessItems { get; set; }
        public ObservableCollection<UploadItem> ErrorItems { get; set; }

        public int NumWait { get; set; }
        public int NumSuccess { get; set; }
        public int NumError { get; set; }

        public bool IsCancel { get; set; }

        public class UploadItem : Stylet.Screen
        {
            public Track Track { get; set; }
            public string Status { get; set; } = "Wait!";
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
            foreach (var item in playlist.Tracks)
            {
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
                SetAllItemsError("Get user playlists failed! " + msg);
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
                    SetAllItemsError("Creat playlist failed! " + msg);
                    goto END_POINT;
                }
            }

            //upload
            while(UploadItems.Count > 0)
            {
                UploadItem item = UploadItems[0];
                if (VMMain.FindTrack(playlistTo.Tracks, item.Track) >= 0)
                {
                    item.Status = "Exist!";
                    item.StatusColor = Brushes.Green;
                    NumSuccess++;
                    SuccessItems.Add(item);
                    goto NEXT_POINT;
                }

#if DEBUG
                if (item.Track.Title.Contains("Uptown"))
                    NumSuccess = NumSuccess;
#endif

                Track track = await SudaLib.Method.SearchTrack(Platform.LoginKey, item.Track, Global.Settings.Compare);
                if (track == null)
                {
                    item.Status = "Can't find!";
                    item.StatusColor = Brushes.Red;
                    NumError++;
                    ErrorItems.Add(item);
                    goto NEXT_POINT;
                }

                (string msg1,bool flag) = await SudaLib.Method.AddTracksToPlaylist(Platform.LoginKey, new string[] { track.MID }, playlistTo.MID);
                if (flag == false)
                {
                    item.Status = "Can't add to playlist! "+ msg1;
                    item.StatusColor = Brushes.Red;
                    NumError++;
                    ErrorItems.Add(item);
                    goto NEXT_POINT;
                }

                item.Status = "Success!";
                item.StatusColor = Brushes.Green;
                NumSuccess++;
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
                item.StatusColor = Brushes.Red;
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

                item.Status = "Cancel!";
                item.StatusColor = Brushes.DarkOrange;
                ErrorItems.Add(item);

                UploadItems.RemoveAt(0);
            }

            NumError = NumWait;
            NumWait = 0;
        }
    }
}
