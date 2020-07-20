using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Stylet;
using Suda.Else;

namespace Suda.Pages
{
    public class MainViewModel : Screen
    {
        public int PlatformsSelectIndex { get; set; } = -1;
        public ObservableCollection<Platform> Platforms { get; set; } = new ObservableCollection<Platform>();
        public ObservableCollection<SudaPlaylist> SudaPlaylists { get; set; } = new ObservableCollection<SudaPlaylist>();

        public LoginViewModel VMLogin { get; set; }
        public PlatformViewModel VMPlatform { get; set; }
        public PlaylistViewModel VMPlaylist { get; set; }
        public MainViewModel(PlaylistViewModel playlist, LoginViewModel login, PlatformViewModel platform)
        {
            VMPlaylist = playlist;
            VMLogin = login;
            VMPlatform = platform;
        }

        #region Prepare work
        protected override void OnViewLoaded()
        {
            PreparePlatforms();
        }

        void PreparePlatforms()
        {
            Platform QQMusic = new Platform();
            QQMusic.Logo = (Geometry)Application.Current.TryFindResource("QQGeometry");
            QQMusic.Name = "Login";
            QQMusic.Type = SudaLib.ePlatform.QQMusic;
            QQMusic.LoginKey = Global.Cache.QQLoginkey;

            Platform CloudMusic = new Platform();
            CloudMusic.Logo = (Geometry)Application.Current.TryFindResource("NeteaseGeometry");
            CloudMusic.Name = "Login";
            CloudMusic.Type = SudaLib.ePlatform.CloudMusic;
            CloudMusic.LoginKey = Global.Cache.CloudLoginkey;

            Platforms.Add(QQMusic);
            Platforms.Add(CloudMusic);
        }
        #endregion


        #region Platform select change

        public void PlatformSelectChange(object sender, RoutedEventArgs e)
        {
            if (PlatformsSelectIndex < 0)
                return;
            //Show login view
            if (Platforms[PlatformsSelectIndex].LoginKey == null)
            {
                VMLogin.Load(Platforms[PlatformsSelectIndex]);
            }
            //Show platform view
            VMPlatform.Platform = Platforms[PlatformsSelectIndex];
            return;
        }

        #endregion


        #region Suda playlist select change
        public void SudaPlaylistSelectChange(object sender, RoutedEventArgs e)
        {
            return;
        }
        #endregion


        #region Windows
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
