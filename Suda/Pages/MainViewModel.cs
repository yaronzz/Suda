using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using AIGS.Helper;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using Stylet;
using Suda.Else;
using SudaLib;
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
        /// Page ViewModel
        /// </summary>
        public LoginViewModel VMLogin { get; set; } = new LoginViewModel();
        public UploadViewModel VMUpload { get; set; } = new UploadViewModel();
        public AboutViewModel VMAbout { get; set; } = new AboutViewModel();
        public SettingsViewModel VMSettings { get; set; } = new SettingsViewModel();

        public PlatformViewModel VMQQPlatform { get; set; } = new PlatformViewModel();
        public PlatformViewModel VMCloudPlatform { get; set; } = new PlatformViewModel();
        public PlatformViewModel VMTidalPlatform { get; set; } = new PlatformViewModel();
        public PlatformViewModel VMSpotifyPlatform { get; set; } = new PlatformViewModel();
        public List<object> VMList { get; set; } = new List<object>();

        public ViewManager VMManager { get; set; }

        public MainViewModel(ViewManager manager)
        {
            VMManager = manager;

            VMList.Add(VMLogin);
            VMList.Add(VMUpload);
            VMList.Add(VMAbout);
            VMList.Add(VMSettings);
            VMList.Add(VMQQPlatform);
            VMList.Add(VMCloudPlatform);
            VMList.Add(VMTidalPlatform);
            VMList.Add(VMSpotifyPlatform);
        }

        #region Prepare work
        protected override async void OnViewLoaded()
        {
            //read global
            Global.Cache = Cache.Read();
            Global.Settings = Settings.Read();
            
            //settings 
            Language.Change(Global.Settings.LanguageType);
            Theme.Change(Global.Settings.ThemeType);
            Settings.SetWebBrowserFeatures(11);

            //read suda playlist
            SudaPlaylistRead();

            //platforms login
            await PreparePlatforms();
        }

        async Task PreparePlatforms()
        {
            string msg;
            if (Global.Settings.EnableSpotify)
            {
                Platform Spotify = new Platform();
                Spotify.Logo = (Geometry)Application.Current.TryFindResource("SpotifyGeometry");
                Spotify.Name = "Spotify";
                Spotify.Type = SudaLib.ePlatform.Spotify;
                Spotify.VMPlatform = VMSpotifyPlatform;
                (msg, Spotify.LoginKey) = await SudaLib.Method.RefreshLoginKey(Global.Cache.SpotifyLoginkey);
                Platforms.Add(Spotify);
            }

            if (Global.Settings.EnableTidal)
            {
                Platform Tidal = new Platform();
                Tidal.Logo = (Geometry)Application.Current.TryFindResource("TidalGeometry");
                Tidal.Type = SudaLib.ePlatform.Tidal;
                Tidal.Name = SudaLib.Method.GetPlatformDisplayName(Tidal.Type);
                Tidal.VMPlatform = VMTidalPlatform;
                (msg, Tidal.LoginKey) = await SudaLib.Method.RefreshLoginKey(Global.Cache.TidalLoginkey);
                Platforms.Add(Tidal);
            }

            if (Global.Settings.EnableQQMusic)
            {
                Platform QQMusic = new Platform();
                QQMusic.Logo = (Geometry)Application.Current.TryFindResource("QQGeometry");
                QQMusic.Type = SudaLib.ePlatform.QQMusic;
                QQMusic.Name = SudaLib.Method.GetPlatformDisplayName(QQMusic.Type);
                QQMusic.VMPlatform = VMQQPlatform;
                (msg, QQMusic.LoginKey) = await SudaLib.Method.RefreshLoginKey(Global.Cache.QQLoginkey);
                Platforms.Add(QQMusic);
            }

            if (Global.Settings.EnableCloudMusic)
            {
                Platform CloudMusic = new Platform();
                CloudMusic.Logo = (Geometry)Application.Current.TryFindResource("NeteaseGeometry");
                CloudMusic.Type = SudaLib.ePlatform.CloudMusic;
                CloudMusic.Name = SudaLib.Method.GetPlatformDisplayName(CloudMusic.Type);
                CloudMusic.VMPlatform = VMCloudPlatform;
                (msg, CloudMusic.LoginKey) = await SudaLib.Method.RefreshLoginKey(Global.Cache.CloudLoginkey);
                Platforms.Add(CloudMusic);
            }
        }
        #endregion


        #region Show page

        public bool IsPageShow(object viewmodel)
        {
            return ((ModelBase)viewmodel).ViewVisibility == Visibility.Visible;
        }

        public void ShowPage(object viewmodel, bool hideElse = true)
        {
            if (hideElse)
            {
                foreach (var item in VMList)
                    ((ModelBase)item).ViewVisibility = Visibility.Hidden;
            }
            ((ModelBase)viewmodel).ViewVisibility = Visibility.Visible;
        }

        public void HidePage(object viewmodel)
        {
            ((ModelBase)viewmodel).ViewVisibility = Visibility.Hidden;
        }

        #endregion

        #region 
        public PlaylistViewModel AddControlPlaylistView()
        {
            //creat view
            PlaylistView view = new PlaylistView();
            PlaylistViewModel model = new PlaylistViewModel();
            VMManager.BindViewToModel(view, model);

            //add to vmlist
            VMList.Add(model);

            //add to the view
            ((MainView)this.View).ctrlPLGrid.Children.Add(view);

            return model;
        }

        public void DelControlPlaylistView(object viewmodel)
        {
            PlaylistViewModel model = (PlaylistViewModel)viewmodel;
            VMList.Remove(model);
            ((MainView)this.View).ctrlPLGrid.Children.Remove(model.View);
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
                ((PlaylistViewModel)plist.VMModel()).Load(plist, this);
                ShowPage(plist.VMModel());
            }
            return;
        }

        public void MenuSelectPlatform(object data)
        {
            Platform plat = (Platform)data;
            Action<object> action = (x) =>
            {
                plat.VMPlatform.Load(plat, this);
                ShowPage(plat.VMPlatform);
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
            if (index >= 0)
            {
                DelControlPlaylistView(SudaPlaylists[index].VMModel());
                SudaPlaylists.RemoveAt(index);
            }
        }

        public void SudaPlaylistDelAll()
        {
            Dialog.Show(new MessageView(MessageBoxImage.Information, Language.Get("strmsgDeleteAllPlaylists"), true, (x) =>
            {
                for (int i = 0; i < SudaPlaylists.Count(); i++)
                {
                    DelControlPlaylistView(SudaPlaylists[i].VMModel());
                }

                SudaPlaylists.Clear();
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

                    if(additem.MyFavorite)
                        SudaPlaylists.Insert(0,additem);
                    else
                        SudaPlaylists.Add(additem);

                    additem.VMModel(AddControlPlaylistView());
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
                Growl.Success(string.Format(Language.Get("strmsgOneToLocalSuccess"), list[0].Title, updataNum), Global.TOKEN_MAIN);
            else
                Growl.Success(string.Format(Language.Get("strmsgMoreToLocalSuccess"), updataNum), Global.TOKEN_MAIN);
        }

        public int FindPlaylist(ObservableCollection<Playlist> Array, Playlist item)
        {
            int index = Array.IndexOf(item);
            if (index >= 0)
                return index;

            string title = item.Title.Trim();
            for (int i = 0; i < Array.Count; i++)
            {
                if (Array[i].Title.Trim() == title && Array[i].MyFavorite == item.MyFavorite)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        public int FindTrack(ObservableCollection<Track> Array, Track item)
        {
            int index = SudaLib.Method.MatchTrack(Array, item, Global.Settings.Compare);
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
                Dialog.Show(new MessageView(MessageBoxImage.Information, Language.Get("strmsgSaveSuccess"), false));
            }
            catch (Exception e)
            {
                Dialog.Show(new MessageView(MessageBoxImage.Warning, Language.Get("strmsgSaveFailed") + " " +e.Message, false));
            }
        }

        public void SudaPlaylistRead()
        {
            try
            {
                string sTxt = FileHelper.Read(Global.PATH_SUDA_PLAYLIST);
                ObservableCollection<Playlist> pLists = JsonHelper.ConverStringToObject<ObservableCollection<Playlist>>(sTxt);
                if (pLists != null)
                {
                    for (int i = 0; i < pLists.Count(); i++)
                    {
                        pLists[i].VMModel(AddControlPlaylistView());
                    }
                    SudaPlaylists = pLists;
                }
            }
            catch (Exception e)
            {
                Dialog.Show(new MessageView(MessageBoxImage.Warning, Language.Get("strmsgeReadhPlaylistsFailed") + " " + e.Message, false));
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
                        ShowPage(VMUpload,false);
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
            VMAbout.VMMain = this;
            ShowPage(VMAbout,false);
        }

        public void WindowSettings()
        {
            if (IsPageShow(VMSettings))
                HidePage(VMSettings);
            else
                ShowPage(VMSettings, false);
        }

        public void WindowUpload()
        {
            if (IsPageShow(VMUpload))
                HidePage(VMUpload);
            else
                ShowPage(VMUpload, false);
        }
        #endregion
    }
}
