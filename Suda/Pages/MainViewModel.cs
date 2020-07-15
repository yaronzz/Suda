using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Music;
using Music.QQMusic;
using Stylet;
using Suda.Else;

namespace Suda.Pages
{
    public class MainViewModel : Screen
    {
        public string UserName { get; set; } = "请登录";
        public string UserImg { get; set; } = "/Img/avatar.png";
        public ObservableCollection<Music.Playlist> UserPlaylists { get; set; }

        public Visibility LoginVisibility { get; set; } = Visibility.Hidden;

        public LoginViewModel VMLogin { get; set; }
        public PlaylistViewModel VMPlaylist { get; set; }

        public MainViewModel(LoginViewModel login, PlaylistViewModel playlist)
        {
            VMLogin = login;
            VMPlaylist = playlist;

            VMLogin.VMMain = this;
        }

        /// <summary>
        /// 程序打开前的检查工作
        /// </summary>
        protected override void OnViewLoaded()
        {
            if (Global.MusicCache.IsQQValid())
            {
                Global.QQRecord.QQNumber = Global.MusicCache.QQNumber;
                Global.QQRecord.GTK = Global.MusicCache.QQGTK;
                Global.QQRecord.Cookie = Global.MusicCache.QQCookie;
                RefreshUser();
                return;
            }

            LoginVisibility = Visibility.Visible;
            VMLogin.LoadWebBrowser();
        }

        /// <summary>
        /// 登录后的工作
        /// </summary>
        public void LoginComplete()
        {
            LoginVisibility = Visibility.Hidden;
            RefreshUser();
        }

        /// <summary>
        /// 刷新用户信息
        /// </summary>
        public async void RefreshUser()
        {
            var check = await Global.QQRecord.GetUser();
            if (!check)
                return; //print err

            UserName = Global.QQRecord.User.Nick;
            UserImg = Global.QQRecord.User.Headpic;
            UserPlaylists = Music.Method.LoadPlaylist(Global.QQRecord.User.Playlists.ToArray());
        }

        public void SelectedPlaylist(object sender, RoutedEventArgs e)
        {
            ListBox ctrl = (ListBox)sender;
            int index = ctrl.SelectedIndex;
            VMPlaylist.Playlist = UserPlaylists[index];
            return;
        }
        
        #region 窗口响应
        public void WindowMove()
        {
            ((MainView)this.View).DragMove();
        }

        public void WindowMin()
        {
            ((MainView)this.View).WindowState = WindowState.Minimized;
        }

        public void WindowMax()
        {
            AIGS.Helper.ScreenShotHelper.MaxWindow((MainView)this.View);
        }

        public void WindowClose()
        {
            RequestClose();
        }
        #endregion
    }
}
