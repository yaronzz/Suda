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
        public DownloadViewModel VMDownload { get; set; } = new DownloadViewModel();
        public SettingsViewModel VMSettings { get; set; } = new SettingsViewModel();
        public ImportViewModel VMImport { get; set; } = new ImportViewModel();


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
            VMList.Add(VMDownload);
            VMList.Add(VMSettings);
            VMList.Add(VMImport);
            VMList.Add(VMQQPlatform);
            VMList.Add(VMCloudPlatform);
            VMList.Add(VMTidalPlatform);
            VMList.Add(VMSpotifyPlatform);
        }

        protected override async void OnViewLoaded()
        {
            //read global
            Global.VMMain = this;
            Global.Cache = Cache.Read();
            Global.Settings = Settings.Read();

            //ShowImport
            ShowPage(VMImport);

            //read suda playlist
            SudaPlaylistRead();

            //check require
            if (VMDownload.CheckRequire() == false)
            {
                bool bNeed = VMDownload.DownloadRequire((x) =>
                {
                    (bool flag, string msg) = ((bool, string))x;
                    if (flag)
                        Growl.Success(msg, Global.TOKEN_MAIN);
                    else
                        Growl.Error(msg, Global.TOKEN_MAIN);
                    HidePage(VMDownload);
                });

                if(bNeed)
                    ShowPage(VMDownload);
            }

            //settings
            await ChangeSettings(Global.Settings);
        }


        #region Settings

        public async Task ChangeSettings(Settings newItem, Settings oldItems = null)
        {
            if(oldItems == null || oldItems.ThemeType != newItem.ThemeType)
                Theme.Change(newItem.ThemeType);
            if (oldItems == null || oldItems.LanguageType != newItem.LanguageType)
                Language.Change(newItem.LanguageType);

            if (oldItems == null || oldItems.EnableQQMusic != newItem.EnableQQMusic)
            {
                if(newItem.EnableQQMusic)
                    await AddPlatform(ePlatform.QQMusic);
                else
                    DelPlatform(ePlatform.QQMusic);
            }
            if (oldItems == null || oldItems.EnableCloudMusic != newItem.EnableCloudMusic)
            {
                if (newItem.EnableCloudMusic)
                    await AddPlatform(ePlatform.CloudMusic);
                else
                    DelPlatform(ePlatform.CloudMusic);
            }
            if (oldItems == null || oldItems.EnableTidal != newItem.EnableTidal)
            {
                if (newItem.EnableTidal)
                    await AddPlatform(ePlatform.Tidal);
                else
                    DelPlatform(ePlatform.Tidal);
            }
            if (oldItems == null || oldItems.EnableSpotify != newItem.EnableSpotify)
            {
                if (newItem.EnableSpotify)
                    await AddPlatform(ePlatform.Spotify);
                else
                    DelPlatform(ePlatform.Spotify);
            }
        }
        #endregion


        #region Platform

        public async Task AddPlatform(ePlatform eType)
        {
            //exist
            if (FindPlatforms(eType) != null)
                return;

            string msg = null;
            Platform plat = new Platform()
            {
                Type = eType,
                Name = Platform.GetPlatformDisplayName(eType),
            };

            if (eType == ePlatform.Spotify)
            {
                plat.Logo = (Geometry)Application.Current.TryFindResource("SpotifyGeometry");
                plat.VMPlatform = VMSpotifyPlatform;
                (msg, plat.LoginKey) = await SudaLib.Method.RefreshLoginKey(Global.Cache.SpotifyLoginkey);
            }
            else if (eType == ePlatform.Tidal)
            {
                plat.Logo = (Geometry)Application.Current.TryFindResource("TidalGeometry");
                plat.VMPlatform = VMTidalPlatform;
                (msg, plat.LoginKey) = await SudaLib.Method.RefreshLoginKey(Global.Cache.TidalLoginkey);
            }
            else if (eType == ePlatform.QQMusic)
            {
                plat.Logo = (Geometry)Application.Current.TryFindResource("QQGeometry");
                plat.VMPlatform = VMQQPlatform;
                (msg, plat.LoginKey) = await SudaLib.Method.RefreshLoginKey(Global.Cache.QQLoginkey);
            }
            else if (eType == ePlatform.CloudMusic)
            {
                plat.Logo = (Geometry)Application.Current.TryFindResource("NeteaseGeometry");
                plat.VMPlatform = VMCloudPlatform;
                (msg, plat.LoginKey) = await SudaLib.Method.RefreshLoginKey(Global.Cache.CloudLoginkey);
            }
            Platforms.Add(plat);
        }

        public Platform FindPlatforms(ePlatform eType)
        {
            foreach (var item in Platforms)
            {
                if (item.Type == eType)
                    return item;
            }
            return null;
        }

        public void DelPlatform(ePlatform eType)
        {
            Platform item = FindPlatforms(eType);
            if (item != null)
                Platforms.Remove(item);
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
            if (data.GetType() == typeof(Platform))
            {
                //select platform
                MenuSelectPlatform(data);
            }
            else
            {
                //select playlist
                Playlist plist = (Playlist)data;
                ((PlaylistViewModel)plist.VMModel()).Load(plist);
                ShowPage(plist.VMModel());
            }
            return;
        }

        public void MenuSelectPlatform(object data)
        {
            Platform plat = (Platform)data;
            Action<object> action = (x) =>
            {
                plat.VMPlatform.Load(plat);
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

        public void SudaPlaylistUpload(Playlist data, ePlatform ignore = ePlatform.None)
        {
            Dialog.Show(new SelectPlatformView(Platforms, ignore, (type)=> {

                if (type != SudaLib.ePlatform.None)
                {
                    Platform to = Platforms.First(x => x.Type == type);
                    if (to != null)
                    {
                        VMUpload.Load(data, to);
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
            ShowPage(VMAbout,false);
        }

        public void WindowSettings()
        {
            if (IsPageShow(VMSettings))
                HidePage(VMSettings);
            else
            {
                VMSettings.Load();
                ShowPage(VMSettings, false);
            }
        }

        public void WindowUpload()
        {
            if (IsPageShow(VMUpload))
                HidePage(VMUpload);
            else
                ShowPage(VMUpload, false);
        }

        public void WindowImport()
        {
            ShowPage(VMImport);
        }
        #endregion
    }
}
