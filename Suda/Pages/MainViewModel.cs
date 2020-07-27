using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AIGS.Helper;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using Stylet;
using Suda.Else;
using static SudaLib.Common;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Suda.Pages
{
    public class MainViewModel : Screen
    {
        /// <summary>
        /// Menu list
        /// </summary>
        public ObservableCollection<Platform> Platforms { get; set; } = new ObservableCollection<Platform>();
        public ObservableCollection<Playlist> SudaPlaylists { get; set; } = new ObservableCollection<Playlist>();

        /// <summary>
        /// Page Visibility
        /// </summary>
        public Visibility UploadVisibility { get; set; } = Visibility.Hidden;
        public Visibility PlatformVisibility { get; set; } = Visibility.Hidden;
        public Visibility PlaylistVisibility { get; set; } = Visibility.Hidden;
        public Visibility LoginVisibility { get; set; } = Visibility.Hidden;
        public Visibility SettingsVisibility { get; set; } = Visibility.Hidden;
        public Visibility AboutVisibility { get; set; } = Visibility.Hidden;

        /// <summary>
        /// Page ViewModel
        /// </summary>
        public SelectPlatformView VMSelect { get; set; }
        public LoginViewModel VMLogin { get; set; }
        public PlatformViewModel VMPlatform { get; set; }
        public PlaylistViewModel VMPlaylist { get; set; }
        public UploadViewModel VMUpload { get; set; }
        public AboutViewModel VMAbout { get; set; }
        public SettingsViewModel VMSettings { get; set; }


        public MainViewModel(PlaylistViewModel playlist, LoginViewModel login, PlatformViewModel platform, UploadViewModel upload, AboutViewModel about, SettingsViewModel settings)
        {
            VMPlaylist = playlist;
            VMLogin = login;
            VMPlatform = platform;
            VMUpload = upload;
            VMAbout = about;
            VMSettings = settings;
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
            string msg;
            Platform QQMusic = new Platform();
            QQMusic.Logo = (Geometry)Application.Current.TryFindResource("QQGeometry");
            QQMusic.Name = "QQ音乐";
            QQMusic.Type = SudaLib.ePlatform.QQMusic;
            (msg, QQMusic.LoginKey) = await SudaLib.Method.RefreshLoginKey(Global.Cache.QQLoginkey);
            (msg, QQMusic.UserInfo) = await SudaLib.Method.GetUserInfo(QQMusic.LoginKey);
            (msg, QQMusic.Playlists) = await SudaLib.Method.GetUserPlaylists(QQMusic.LoginKey);
            Platforms.Add(QQMusic);

            Platform CloudMusic = new Platform();
            CloudMusic.Logo = (Geometry)Application.Current.TryFindResource("NeteaseGeometry");
            CloudMusic.Name = "网易云";
            CloudMusic.Type = SudaLib.ePlatform.CloudMusic;
            (msg, CloudMusic.LoginKey) = await SudaLib.Method.RefreshLoginKey(Global.Cache.CloudLoginkey);
            (msg, CloudMusic.UserInfo) = await SudaLib.Method.GetUserInfo(CloudMusic.LoginKey);
            (msg, CloudMusic.Playlists) = await SudaLib.Method.GetUserPlaylists(CloudMusic.LoginKey);
            Platforms.Add(CloudMusic);

            Platform Tidal = new Platform();
            Tidal.Logo = (Geometry)Application.Current.TryFindResource("TidalGeometry");
            Tidal.Name = "Tidal";
            Tidal.Type = SudaLib.ePlatform.Tidal;
            (msg, Tidal.LoginKey) = await SudaLib.Method.RefreshLoginKey(Global.Cache.TidalLoginkey);
            (msg, Tidal.UserInfo) = await SudaLib.Method.GetUserInfo(Tidal.LoginKey);
            (msg, Tidal.Playlists) = await SudaLib.Method.GetUserPlaylists(Tidal.LoginKey);
            Platforms.Add(Tidal);

            //Platform Spotify = new Platform();
            //Spotify.Logo = (Geometry)Application.Current.TryFindResource("SpotifyGeometry");
            //Spotify.Name = "Spotify";
            //Spotify.Type = SudaLib.ePlatform.Spotify;
            //(msg, Spotify.LoginKey) = await SudaLib.Method.RefreshLoginKey(Global.Cache.SpotifyLoginkey);
            //(msg, Spotify.UserInfo) = await SudaLib.Method.GetUserInfo(Spotify.LoginKey);
            //(msg, Spotify.Playlists) = await SudaLib.Method.GetUserPlaylists(Spotify.LoginKey);
            //Platforms.Add(Spotify);

            //Platform Apple = new Platform();
            //Apple.Logo = (Geometry)Application.Current.TryFindResource("AppleGeometry");
            //Apple.Name = "Apple Music";
            //Apple.Type = SudaLib.ePlatform.AppleMusic;
            //Platforms.Add(Apple);
        }
        #endregion


        #region Show page

        public bool IsPageShow(object viewmodel)
        {
            Type type = viewmodel.GetType();
            if (type == typeof(LoginViewModel))
                return LoginVisibility == Visibility.Visible;
            if (type == typeof(UploadViewModel))
                return UploadVisibility == Visibility.Visible;
            if (type == typeof(PlatformViewModel))
                return PlatformVisibility == Visibility.Visible;
            if (type == typeof(PlaylistViewModel))
                return PlaylistVisibility == Visibility.Visible;
            if (type == typeof(AboutViewModel))
                return AboutVisibility == Visibility.Visible;
            if (type == typeof(SettingsViewModel))
                return SettingsVisibility == Visibility.Visible;
            return false;}

        public void ShowPage(object viewmodel)
        {
            Type type = viewmodel.GetType();
            if (type == typeof(LoginViewModel))
            {
                LoginVisibility = Visibility.Visible;
                return;
            }
            if (type == typeof(UploadViewModel))
            {
                UploadVisibility = Visibility.Visible;
                return;
            }

            LoginVisibility = Visibility.Hidden;
            PlatformVisibility = Visibility.Hidden;
            PlaylistVisibility = Visibility.Hidden;
            UploadVisibility = Visibility.Hidden;
            AboutVisibility = Visibility.Hidden;
            SettingsVisibility = Visibility.Hidden;
            if (type == typeof(PlatformViewModel))
                PlatformVisibility = Visibility.Visible;
            if (type == typeof(PlaylistViewModel))
                PlaylistVisibility = Visibility.Visible;
            if (type == typeof(AboutViewModel))
                AboutVisibility = Visibility.Visible;
            if (type == typeof(SettingsViewModel))
                SettingsVisibility = Visibility.Visible;
        }

        public void HidePage(object viewmodel)
        {
            Type type = viewmodel.GetType();
            if (type == typeof(LoginViewModel))
                LoginVisibility = Visibility.Hidden;
            if (type == typeof(UploadViewModel))
                UploadVisibility = Visibility.Hidden;
            if (type == typeof(PlatformViewModel))
                PlatformVisibility = Visibility.Hidden;
            if (type == typeof(PlaylistViewModel))
                PlaylistVisibility = Visibility.Hidden;
            if (type == typeof(AboutViewModel))
                AboutVisibility = Visibility.Hidden;
            if (type == typeof(SettingsViewModel))
                SettingsVisibility = Visibility.Hidden;
        }

        #endregion


        #region Menu select

        public void MenuSelectChange(object sender, RoutedEventArgs e)
        {
            object data = ((RadioButton)sender).DataContext;
            //select platform
            if (data.GetType() == typeof(Platform))
                MenuSelectPlatform(data);
            //select playlist
            else
            {
                Playlist plist = (Playlist)data;
                VMPlaylist.Load(plist, this);
                ShowPage(VMPlaylist);
            }
            return;
        }

        public void MenuSelectPlatform(object data)
        {
            Platform plat = (Platform)data;
            Action<object> action = (x) =>
            {
                VMPlatform.Load(plat, this);
                ShowPage(VMPlatform);
            };

            if (plat.LoginKey == null)
            {
                VMLogin.Load(plat, action);
                ShowPage(VMLogin);
            }
            else
                action(null);
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
            Dialog.Show(new MessageView(MessageBoxImage.Information, "Delete all playlist?", true, (x) =>
            {
                SudaPlaylists.Clear();
                VMPlaylist.Playlist = null;
            }));
        }
        #endregion


        #region Suda playlist add

        public void SudaPlaylistAdd(ObservableCollection<Playlist> list)
        {
            if (list == null)
                return;

            int updataNum = 0;
            foreach (var data in list)
            {
                //Find playlist
                int index = FindPlaylist(SudaPlaylists, data);

                //If can't find, creat new one
                if (index < 0)
                {
                    Playlist additem = (Playlist)AIGS.Common.Convert.CloneObject(data);
                    additem.Tracks = new ObservableCollection<Track>(); //can't clone the list
                    for (int i = 0; i < data.Tracks.Count; i++)
                        additem.Tracks.Add(data.Tracks[i]);
                    updataNum += data.Tracks.Count;

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
            }

            if(list.Count == 1)
                Growl.Success($"\"{list[0].Title}\" to local success! Updata {updataNum} tracks.", Global.TOKEN_MAIN);
            else
                Growl.Success($"To local success! Updata {updataNum} tracks.", Global.TOKEN_MAIN);
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
                Dialog.Show(new MessageView(MessageBoxImage.Information, "Save success!", false));
            }
            catch (Exception e)
            {
                Dialog.Show(new MessageView(MessageBoxImage.Warning, "Save failed! "+e.Message, false));
            }
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
                Dialog.Show(new MessageView(MessageBoxImage.Warning, "Read playlists failed! " + e.Message, false));
            }
        }

        #endregion


        #region Suda playlist upload

        public void SudaPlaylistUpload(Playlist data)
        {
            Dialog.Show(new SelectPlatformView(Platforms, (type)=> {

                if (type != SudaLib.ePlatform.None)
                {
                    Platform to = Platforms.First(x => x.Type == type);
                    if (to != null)
                    {
                        VMUpload.Load(data, this, to);
                        ShowPage(VMUpload);
                    }
                }
            }));
        }

        public void SudaPlaylistUploadAll()
        {

        }

        #endregion

        #region Windows

        public void WindowMove()
        {
            try
            {
                ((MainView)this.View).DragMove();
            }
            catch { }
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

        public void WindowAbout()
        {
            ShowPage(VMAbout);
        }

        public void WindowSettings()
        {
            ShowPage(VMSettings);
        }

        public void WindowUpload()
        {
            if (IsPageShow(VMUpload))
                HidePage(VMUpload);
            else
                ShowPage(VMUpload);
        }
        #endregion
    }
}
