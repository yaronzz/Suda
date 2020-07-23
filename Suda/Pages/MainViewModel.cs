using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AIGS.Helper;
using HandyControl.Controls;
using Stylet;
using Suda.Else;
using static SudaLib.Common;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Suda.Pages
{
    public class MainViewModel : Screen
    {
        public ObservableCollection<Platform> Platforms { get; set; } = new ObservableCollection<Platform>();
        public ObservableCollection<Playlist> SudaPlaylists { get; set; } = new ObservableCollection<Playlist>();
        public Visibility PlaylistVisibility { get; set; } = Visibility.Hidden;

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
        protected override async void OnViewLoaded()
        {
            //read global
            Global.Cache = Cache.Read();
            Global.Settings = Settings.Read();
            Global.Settings.Compare.AlbumTitle = false;

            //read suda playlist
            SudaPlaylistRead();

            //platforms login
            await PreparePlatforms();
        }

        async Task PreparePlatforms()
        {
            Platform QQMusic = new Platform();
            QQMusic.Logo = (Geometry)Application.Current.TryFindResource("QQGeometry");
            QQMusic.Name = "QQ Music";
            QQMusic.Type = SudaLib.ePlatform.QQMusic;
            QQMusic.LoginKey = await SudaLib.Method.RefreshLoginKey(Global.Cache.QQLoginkey);
            QQMusic.UserInfo = await SudaLib.Method.GetUserInfo(QQMusic.LoginKey);
            QQMusic.Playlists = await SudaLib.Method.GetUserPlaylists(QQMusic.LoginKey);
            Platforms.Add(QQMusic);

            Platform CloudMusic = new Platform();
            CloudMusic.Logo = (Geometry)Application.Current.TryFindResource("NeteaseGeometry");
            CloudMusic.Name = "Cloud Music";
            CloudMusic.Type = SudaLib.ePlatform.CloudMusic;
            CloudMusic.LoginKey = await SudaLib.Method.RefreshLoginKey(Global.Cache.CloudLoginkey);
            CloudMusic.UserInfo = await SudaLib.Method.GetUserInfo(CloudMusic.LoginKey);
            CloudMusic.Playlists = await SudaLib.Method.GetUserPlaylists(CloudMusic.LoginKey);
            Platforms.Add(CloudMusic);

            Platform Tidal = new Platform();
            Tidal.Logo = (Geometry)Application.Current.TryFindResource("TidalGeometry");
            Tidal.Name = "Tidal";
            Tidal.Type = SudaLib.ePlatform.Tidal;
            Tidal.LoginKey = await SudaLib.Method.RefreshLoginKey(Global.Cache.TidalLoginkey);
            Tidal.UserInfo = await SudaLib.Method.GetUserInfo(Tidal.LoginKey);
            Tidal.Playlists = await SudaLib.Method.GetUserPlaylists(Tidal.LoginKey);
            Platforms.Add(Tidal);

            Platform Spotify = new Platform();
            Spotify.Logo = (Geometry)Application.Current.TryFindResource("SpotifyGeometry");
            Spotify.Name = "Spotify";
            Spotify.Type = SudaLib.ePlatform.Spotify;
            Spotify.LoginKey = await SudaLib.Method.RefreshLoginKey(Global.Cache.SpotifyLoginkey);
            Spotify.UserInfo = await SudaLib.Method.GetUserInfo(Spotify.LoginKey);
            Spotify.Playlists = await SudaLib.Method.GetUserPlaylists(Spotify.LoginKey);
            Platforms.Add(Spotify);

            Platform Apple = new Platform();
            Apple.Logo = (Geometry)Application.Current.TryFindResource("AppleGeometry");
            Apple.Name = "Apple Music";
            Apple.Type = SudaLib.ePlatform.AppleMusic;
            Platforms.Add(Apple);
        }
        #endregion


        #region Menu select

        public void MenuSelectChange(object sender, RoutedEventArgs e)
        {
            PlaylistVisibility = Visibility.Hidden;

            object data = ((RadioButton)sender).DataContext;
            //select platform
            if (data.GetType() == typeof(Platform))
            {
                Platform plat = (Platform)data;
                //if (plat.LoginKey == null)
                //{
                //    VMLogin.Load(plat);
                //}
                VMPlatform.Load(plat, this);
            }

            //select playlist
            else
            {
                Playlist plist = (Playlist)data;
                VMPlaylist.Load(plist, this);
                PlaylistVisibility = Visibility.Visible;
            }
            return;
        }
        #endregion

        #region Suda playlist del

        public void SudaPlaylistDel(Playlist data)
        {
            if (data == null)
                return;

            //Find playlist
            int index = FindPlaylist(SudaPlaylists, data);
            if(index >= 0)
                SudaPlaylists.RemoveAt(index);
        }

        public void SudaPlaylistDelAll()
        {
            if (MessageBox.Show("Delete all playlist?", "Info", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                SudaPlaylists.Clear();
                VMPlaylist.Playlist = null;
            }
        }
        #endregion


        #region Suda playlist add

        public void SudaPlaylistAdd(Playlist data)
        {
            if (data == null)
                return;

            //Find playlist
            int index = FindPlaylist(SudaPlaylists, data);

            //If can't find, creat new one
            int updataNum = 0;
            if (index < 0)
            {
                Playlist additem = (Playlist)AIGS.Common.Convert.CloneObject(data);
                additem.Tracks = new ObservableCollection<Track>(); //can't clone the list
                for (int i = 0; i < data.Tracks.Count; i++)
                    additem.Tracks.Add(data.Tracks[i]);
                updataNum = data.Tracks.Count;

                SudaPlaylists.Add(additem);
            }
            //If exist, add tracks
            else
            {
                Playlist existitem = SudaPlaylists[index];
                for (int i = 0; i < data.Tracks.Count; i++)
                {
                    if (FindTrack(existitem.Tracks, data.Tracks[i]) >= 0)
                        continue;

                    updataNum++;
                    existitem.Tracks.Add(data.Tracks[i]);
                }
            }

            Growl.Success($"\"{data.Title}\" to local success! Updata {updataNum} tracks.", Global.TOKEN_PLATFORM);
        }
        
        public int FindPlaylist(ObservableCollection<Playlist> Array, Playlist item)
        {
            int index = -1;
            string title = item.Title.Trim();
            for (int i = 0; i < Array.Count; i++)
            {
                if (Array[i].Title.Trim() == title)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        public int FindTrack(ObservableCollection<Track> Array, Track item)
        {
            int index = -1;
            for (int i = 0; i < Array.Count; i++)
            {
                if(SudaLib.Method.IsSameTrack(Array[i], item, Global.Settings.Compare))
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        #endregion


        #region Suda playlist save/read

        public void SudaPlaylistSave()
        {
            bool flag = false;
            try
            {
                string sTxt = JsonHelper.ConverObjectToString<ObservableCollection<Playlist>>(SudaPlaylists);
                flag = FileHelper.Write(sTxt, true, Global.PATH_SUDA_PLAYLIST);
            }
            catch(Exception e)
            {
                
            }

            if (!flag)
                MessageBox.Show("Save failed!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                MessageBox.Show("Save success!", "INFO", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void SudaPlaylistRead()
        {
            try
            {
                string sTxt = FileHelper.Read(Global.PATH_SUDA_PLAYLIST);
                ObservableCollection<Playlist> pLists = JsonHelper.ConverStringToObject<ObservableCollection<Playlist>>(sTxt);
                if (pLists != null)
                    SudaPlaylists = pLists;
            }
            catch (Exception e)
            {

            }
        }

        #endregion

        #region Suda playlist upload

        public void SudaPlaylistUpload()
        {

        }

        public void SudaPlaylistUploadAll()
        {

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
