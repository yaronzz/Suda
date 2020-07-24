using HandyControl.Controls;
using Suda.Else;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static SudaLib.Common;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Suda.Pages
{
    public class PlatformViewModel : Stylet.Screen
    {
        public MainViewModel VMMain { get; set; }
        public Platform Platform { get; set; }
        public bool AllCheck { get; set; }

        
        public void Load(Platform plat, MainViewModel main)
        {
            VMMain = main;
            Platform = plat;
            AllCheck = false;
        }

        public void ClickAllCheck()
        {
            if (Platform == null || Platform.Playlists == null)
                return;
            foreach (var item in Platform.Playlists)
            {
                item.Check = AllCheck;
            }
        }

        public void ToLocal()
        {
            if (Platform == null || Platform.Playlists == null)
                return;

            int num = 0;
            foreach (var item in Platform.Playlists)
            {
                if (item.Check)
                {
                    VMMain.SudaPlaylistAdd(item);
                    num++;
                }
            }

            if(num <= 0)
                Growl.Info("Please select playlist!", Global.TOKEN_PLATFORM);
            return;
        }

        public async void RefreshPlaylist()
        {
            ObservableCollection<Playlist> plist = await SudaLib.Method.GetUserPlaylists(Platform.LoginKey);
            if(plist == null)
                Growl.Error("Refresh playlists err!", Global.TOKEN_PLATFORM);
            else
            {
                Platform.Playlists = plist;
                Growl.Success("Refresh playlists success!", Global.TOKEN_PLATFORM);
            }
        }

        public void Logout()
        {
            Dialog.Show(new MessageView(MessageBoxImage.Information, "Logout?", true, (x) =>
            {
                
            }));
        }
    }
}
