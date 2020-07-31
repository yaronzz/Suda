using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIGS.Common;
using OpenTidl;
using OpenTidl.Methods;
using OpenTidl.Models;
using OpenTidl.Models.Base;
using Stylet.Xaml;
using static SudaLib.Common;

namespace SudaLib
{
    public class Tidal
    {
        public class LoginKey
        {
            public string UserID { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }

            private string playlistID { get; set; }
            public string PlaylistID(string ID = null)
            {
                if (ID != null)
                    playlistID = ID;
                return playlistID;
            }

            private string etag { get; set; }
            public string ETag(string ID = null)
            {
                if (ID != null)
                    etag = ID;
                return etag;
            }

            private OpenTidlClient client = new OpenTidlClient(ClientConfiguration.Default);
            public OpenTidlClient Client() { return client; }

            private OpenTidlSession session;
            public OpenTidlSession Session(OpenTidlSession set = null) 
            {
                if (set != null)
                    session = set;
                return session; 
            }
        }

        #region login
        public async static Task<(string,LoginKey)> GetLoginKey(string sUserName, string sPassword)
        {
            try
            {
                LoginKey ret = new LoginKey();
                OpenTidlSession session = await ret.Client().LoginWithUsername(sUserName, sPassword);
                ret.Session(session);
                ret.Username = sUserName;
                ret.Password = sPassword;
                ret.UserID = session.LoginResult.UserId.ToString();
                return (null,ret);
            }
            catch(Exception e)
            {
                string msg = e.Message;
                return (msg,null);
            }
        }
        #endregion

        #region user info

        public static async Task<(string, UserInfo)> GetUserInfo(LoginKey oKey)
        {
            try
            {
                UserModel data = await oKey.Session().GetUser();
                UserInfo ret = new UserInfo();
                ret.AvatarUrl = GetCoverUrl(data.Picture,"100");
                ret.UserID = data.Id.ToString();
                ret.NickName = data.FirstName + " " + data.LastName;
                if (ret.NickName.IsBlank())
                    ret.NickName = oKey.Username;

                return (null,ret);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                return (msg,null);
            }
        }

        public static async Task<(string, ObservableCollection<Playlist>)> GetUserPlaylist(LoginKey oKey)
        {
            try
            {
                ObservableCollection<Playlist> ret = new ObservableCollection<Playlist>();

                //Playlist-Favorite
                (string msg, Playlist data) = await GetFavorite(oKey);
                if(data!= null)
                {
                    ret.Add(data);
                }

                //Playlist-Creat
                JsonList<PlaylistModel> plists = await oKey.Session().GetUserPlaylists();
                foreach (var item in plists.Items)
                {
                    (msg, data) = await GetPlaylist(oKey, item.Uuid);
                    if(data != null)
                        ret.Add(data);
                }
                return (null, ret);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                return (msg, null);
            }
        }

        #endregion

        #region favorite
        public static string MY_FAVORITE_PLAYLIST_ID = "@!#&^%156139FDSA#*()";
        public static string MY_FAVORITE_PLAYLIST_IMGURL = "https://i.loli.net/2020/07/28/cJvtXmIjDEfRbsq.jpg";

        public static async Task<(string, Playlist)> GetFavorite(LoginKey oKey)
        {
            try
            {
                JsonList<JsonListItem<TrackModel>> plist = await oKey.Session().GetFavoriteTracks();
                Playlist playlist = new Playlist()
                {
                    CreatorID = oKey.UserID,
                    CreatorName = oKey.Username,
                    ImgUrl = MY_FAVORITE_PLAYLIST_IMGURL,
                    Title = Method.GetFavoritePlaylistName(),
                    ID = MY_FAVORITE_PLAYLIST_ID,
                    MID = MY_FAVORITE_PLAYLIST_ID,
                    Tracks = new ObservableCollection<Track>(),
                    MyFavorite = true,
                    MidArray = new MIDArray() { Tidal = MY_FAVORITE_PLAYLIST_ID },
                };

                foreach (var item in plist.Items)
                {
                    Track track = ConvertTrack(item.Item);
                    playlist.Tracks.Add(track);
                }
                return (null, playlist);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                return (msg, null);
            }
        }

        public static async Task<(string, bool)> AddFavoriteTracks(LoginKey oKey, string sID)
        {
            try
            {
                EmptyModel ret = await oKey.Session().AddFavoriteTrack(int.Parse(sID));
                return (null, true);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                return (msg, false);
            }
        }

        public static async Task<(string, bool)> DelFavoriteTracks(LoginKey oKey, string sID)
        {
            try
            {
                EmptyModel ret = await oKey.Session().RemoveFavoriteTrack(int.Parse(sID));
                return (null, true);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                return (msg, false);
            }
        }

        #endregion


        #region playlist

        public static async Task<(string, Playlist)> GetPlaylist(LoginKey oKey, string sID = "7516711891")
        {
            try
            {
                PlaylistModel playlist = await oKey.Session().GetPlaylist(sID);
                JsonList<TrackModel> array = await oKey.Session().GetPlaylistTracks(sID);
                Playlist ret = new Playlist();
                ret.ID = playlist.Uuid;
                ret.MID = playlist.Uuid;
                ret.Title = playlist.Title;
                ret.CreatorID = playlist.Creator.Id.ToString();
                ret.CreatorName = playlist.Creator.Name;
                ret.ImgUrl = GetCoverUrl(playlist.SquareImage);
                ret.Desc = playlist.Description;
                ret.MidArray.Tidal = ret.MID;
                ret.Tracks = new System.Collections.ObjectModel.ObservableCollection<Track>();

                for (int i = 0; array != null &&  i < array.TotalNumberOfItems && i < array.Items.Count(); i++)
                {
                    Track item = ConvertTrack(array.Items[i]);
                    ret.Tracks.Add(item);
                }

                return (null, ret);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                return (msg, null);
            }
        }

        public static async Task<(string, Playlist)> CreatPlaylist(LoginKey oKey, string sPlaylistName, string sPlaylistDesc)
        {
            try
            {
                PlaylistModel playlist = await oKey.Session().CreateUserPlaylist(sPlaylistName);
                Playlist ret = new Playlist();
                ret.ID = playlist.Uuid;
                ret.Title = playlist.Title;
                ret.ID = playlist.Uuid;
                ret.MID = playlist.Uuid;
                ret.MidArray.Tidal = playlist.Uuid;
                ret.CreatorName = playlist.Creator.Name;
                ret.ImgUrl = GetCoverUrl(playlist.Image);
                ret.Desc = playlist.Description;
                ret.Tracks = new System.Collections.ObjectModel.ObservableCollection<Track>();
                return (null, ret);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                return (msg, null);
            }
        }

        public static async Task<(string, bool)> DeletePlaylist(LoginKey oKey, string sPlaylistID)
        {
            try
            {
                if(sPlaylistID == MY_FAVORITE_PLAYLIST_ID)
                    return (null, true);

                PlaylistModel playlist = await oKey.Session().GetPlaylist(sPlaylistID);
                EmptyModel ret = await oKey.Session().DeletePlaylist(sPlaylistID, playlist.ETag);
                return (null, true);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                return (msg, false);
            }
        }

        public static async Task<(string, bool)> AddSongToPlaylist(LoginKey oKey, string[] sSongidArray, string sPlaylistID)
        {
            try
            {
                if (sPlaylistID == MY_FAVORITE_PLAYLIST_ID)
                {
                    string msg = null;
                    bool retBool = false;
                    for (int i = 0; i < sSongidArray.Count(); i++)
                    {
                        (msg, retBool) = await AddFavoriteTracks(oKey, sSongidArray[i]);
                        if (retBool == false)
                            return (msg, false);
                    }
                    return (null, true);
                }

                List<int> ids = new List<int>();
                for (int i = 0; i < sSongidArray.Count(); i++)
                    ids.Add(int.Parse(sSongidArray[i]));
                
                bool flag = await RefreshETg(oKey, sPlaylistID);
                EmptyModel ret = await oKey.Session().AddPlaylistTracks(sPlaylistID, oKey.ETag(), ids.ToArray());
                oKey.ETag(ret.ETag);
                return (null, true);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                return (msg, false);
            }
        }

        public static async Task<(string, bool)> DelSongFromPlaylist(LoginKey oKey, string[] sSongidArray, string sPlaylistID)
        {
            try
            {
                if (sPlaylistID == MY_FAVORITE_PLAYLIST_ID)
                {
                    string msg = null;
                    bool retBool = false;
                    for (int i = 0; i < sSongidArray.Count(); i++)
                    {
                        (msg, retBool) = await DelFavoriteTracks(oKey, sSongidArray[i]);
                        if (retBool == false)
                            return (msg, false);
                    }
                    return (null, true);
                }

                JsonList<TrackModel> array = await oKey.Session().GetPlaylistTracks(sPlaylistID);
                List<int> ids = new List<int>();
                for (int i = 0; i < sSongidArray.Count(); i++)
                {
                    int find = int.Parse(sSongidArray[i]);
                    int index = -1;
                    for (int j = 0; j < array.Items.Count(); j++)
                    {
                        if(find == array.Items[j].Id)
                        {
                            index = j;
                            break;
                        }
                    }

                    if(index < 0)
                        return ("TrackId err!", false);

                    ids.Add(index);
                }

                bool flag = await RefreshETg(oKey, sPlaylistID);
                EmptyModel ret = await oKey.Session().DeletePlaylistTracks(sPlaylistID, oKey.ETag(), ids.ToArray());
                oKey.ETag(ret.ETag);
                return (null, true);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                return (msg, false);
            }
        }

        

        #endregion

        #region search

        public static async Task<ObservableCollection<Common.Track>> Search(LoginKey oKey, string sSearchText)
        {
            try
            {
                ObservableCollection<Common.Track> ret = new ObservableCollection<Track>();
                JsonList<TrackModel> array = await oKey.Client().SearchTracks(sSearchText, 0, 30);
                for (int i = 0; array != null && i < array.TotalNumberOfItems && i < array.Items.Count(); i++)
                {
                    Track item = ConvertTrack(array.Items[i]);
                    ret.Add(item);
                }
                return ret;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                return (null);
            }
        }

        #endregion


        private static string GetCoverUrl(string Name, string Size = "320")
        {
            if (Name.IsBlank())
                return Name;
            return $"https://resources.tidal.com/images/{Name.Replace('-', '/')}/{Size}x{Size}.jpg";
        }

        private static async Task<bool> RefreshETg(LoginKey oKey, string PlaylistID = null)
        {
            bool bRefresh = true;

            //if exist etg, check if refresh
            if(oKey.ETag().IsNotBlank())
            {
                if (oKey.PlaylistID() == PlaylistID)
                    bRefresh = false;
            }

            //refresh
            if(bRefresh)
            {
                PlaylistModel playlist = await oKey.Session().GetPlaylist(PlaylistID);
                oKey.ETag(playlist.ETag);
                oKey.PlaylistID(playlist.Uuid);
            }
            return bRefresh;
        }

        private static Track ConvertTrack(TrackModel data)
        {
            Track item = new Track();
            item.Title = data.Title;
            item.ID = data.Id.ToString();
            item.MID = data.Id.ToString();
            item.MidArray.Tidal = item.MID;
            item.Duration = data.Duration;
            item.DurationStr = AIGS.Helper.TimeHelper.ConverIntToString(data.Duration);
            item.AlbumID = data.Album.Id.ToString();
            item.AlbumTitle = data.Album.Title;
            item.Artists = new System.Collections.ObjectModel.ObservableCollection<Artist>();

            for (int j = 0; j < data.Artists.Count(); j++)
            {
                item.Artists.Add(new Artist()
                {
                    ID = data.Artists[j].Id.ToString(),
                    MID = data.Artists[j].Id.ToString(),
                    Name = data.Artists[j].Name,
                });
            }

            item.ArtistsName = Method.MergeArtistsName(item.Artists);
            (item.Live, item.TitleBrief) = Method.RemoveTilteLiveFlag(item.Title);

            return item;
        }
    }
}
