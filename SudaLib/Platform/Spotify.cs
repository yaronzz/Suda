using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIGS.Common;
using SpotifyAPI;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using static SudaLib.Common;
using static SpotifyAPI.Web.Scopes;
using AIGS.Helper;

namespace SudaLib
{
    public class Spotify
    {
        public class LoginKey
        {
            public string UserID { get; set; }
            public string UserName { get; set; }
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }

            SpotifyClient spotify { get; set; }
            public SpotifyClient API(SpotifyClient api = null) 
            {
                if (api != null)
                    spotify = api;
                return spotify; 
            }
        }

        #region Login

        private static string CLIENT = "ddcfe87f7ded4cec843769b882905d89";
        private static string BASE_URL = "http://localhost:5000/callback";
        private static EmbedIOAuthServer SERVER = new EmbedIOAuthServer(new Uri(BASE_URL), 5000);
        private static Action<object> ACTION = null;

        private static async Task OnAuthorizationCodeReceived(object sender, AuthorizationCodeResponse response)
        {
            await SERVER.Stop();
            AuthorizationCodeTokenResponse token = await new OAuthClient().RequestToken( new AuthorizationCodeTokenRequest(CLIENT, EncryptHelper.Decode("S9TRJK2xov+CK84ztwfeabCVzq9jlzSzwkMlx2Uim1hHFwDKxK4+1A==", CLIENT), response.Code, SERVER.BaseUri) );
            ACTION((token.AccessToken, token.RefreshToken));
        }

        public static async Task WorkBeforeLogin(Action<object> action)
        {
            ACTION = action;
            await SERVER.Start();
            SERVER.AuthorizationCodeReceived += OnAuthorizationCodeReceived;
        }

        public static string GetLoginUrl()
        {
            var request = new LoginRequest(new Uri(BASE_URL), CLIENT, LoginRequest.ResponseType.Code)
            {
                Scope = new List<string> { 
                    UserReadEmail, 
                    UserReadPrivate,
                    PlaylistReadPrivate,
                    PlaylistModifyPrivate,
                    PlaylistModifyPublic,
                    UserLibraryRead, 
                    UserLibraryModify,
                    }
            };

            string url = request.ToUri().ToString();
            return url;
        }

        private static async Task<(string, string)> RefreshToken(string rtoken)
        {
            try
            {
                AuthorizationCodeRefreshResponse newResponse = await new OAuthClient().RequestToken(new AuthorizationCodeRefreshRequest(CLIENT, EncryptHelper.Decode("S9TRJK2xov+CK84ztwfeabCVzq9jlzSzwkMlx2Uim1hHFwDKxK4+1A==", CLIENT), rtoken));
                string newAccessToken = newResponse.AccessToken;
                return (null, newAccessToken);
            }
            catch(Exception e)
            {
                string msg = e.Message;
                return (msg, null);
            }
        }

        public static async Task<(string,LoginKey)> GetLoginKey(string sAccessToken, string sRefreshToken, bool tryRefresh = true)
        {
            try
            {
                if (sAccessToken.IsBlank())
                    return ("accsstoken is null!", null);

                LoginKey ret = new LoginKey();
                ret.API(new SpotifyClient(sAccessToken)); ;

                PrivateUser user = await ret.API().UserProfile.Current();
                if(user == null)
                    return (null, null);

                ret.AccessToken = sAccessToken;
                ret.RefreshToken = sRefreshToken;
                ret.UserID = user.Id;
                ret.UserName = user.DisplayName;
                return (null, ret);
            }
            catch (Exception e)
            {
                if (sRefreshToken.IsNotBlank())
                {
                    (string msg, string newToke) = await RefreshToken(sRefreshToken);
                    if(newToke.IsNotBlank())
                    {
                        sAccessToken = newToke;
                        return await GetLoginKey(sAccessToken, sRefreshToken, false);
                    }
                }
                return (e.Message, null);
            }
        }
        #endregion

        #region user info

        public static async Task<(string, UserInfo)> GetUserInfo(LoginKey oKey)
        {
            try
            {
                PrivateUser user = await oKey.API().UserProfile.Current();
                UserInfo ret = new UserInfo();
                ret.UserID = user.Id;
                ret.NickName = user.DisplayName;
                if (ret.NickName.IsBlank())
                    ret.NickName = user.Email;
                if (user.Images != null && user.Images.Count > 0)
                    ret.AvatarUrl = user.Images[0].Url;
                return (null, ret);
            }
            catch(Exception e)
            {
                return (e.Message, null);
            }
        }

        public static async Task<(string, ObservableCollection<Playlist>)> GetUserPlaylist(LoginKey oKey)
        {
            ObservableCollection<Playlist> ret = new ObservableCollection<Playlist>();

            //Playlist-Favorite
            (string msg1, Playlist fav) = await GetFavorite(oKey);
            if(fav != null)
                ret.Add(fav);

            //Playlist-Creat
            Paging<SimplePlaylist> plists = await oKey.API().Playlists.GetUsers(oKey.UserID);
            foreach (var item in plists.Items)
            {
                (string msg, Playlist data) = await GetPlaylist(oKey, item.Id);
                if (data != null)
                    ret.Add(data);
            }
            return (null,ret);
        }

        #endregion

        #region favorite
        public static string MY_FAVORITE_PLAYLIST_ID = "@!#&^%156139FDSA#*()";
        public static string MY_FAVORITE_PLAYLIST_IMGURL = "https://i.loli.net/2020/07/28/cJvtXmIjDEfRbsq.jpg";

        public static async Task<(string, Playlist)> GetFavorite(LoginKey oKey)
        {
            try
            {
                ObservableCollection<Track> Tracks = new ObservableCollection<Track>();
                LibraryTracksRequest request = new LibraryTracksRequest();
                request.Offset = 0;
                request.Limit = 50;

                while(true)
                {
                    Paging<SavedTrack> plist = await oKey.API().Library.GetTracks(request);
                    for (int i = 0; plist != null && plist.Items != null && i < plist.Items.Count(); i++)
                    {
                        Track item = ConvertTrack((FullTrack)plist.Items[i].Track);
                        Tracks.Add(item);
                    }

                    if (plist == null || plist.Items.Count() < request.Limit)
                        break;

                    request.Offset += request.Limit;
                }

                Playlist playlist = new Playlist()
                {
                    CreatorID = oKey.UserID,
                    CreatorName = oKey.UserID,
                    ImgUrl = MY_FAVORITE_PLAYLIST_IMGURL,
                    Title = Method.GetFavoritePlaylistName(),
                    ID = MY_FAVORITE_PLAYLIST_ID,
                    MID = MY_FAVORITE_PLAYLIST_ID,
                    Tracks = Tracks,
                    MyFavorite = true,
                    MidArray = new MIDArray() { Tidal = MY_FAVORITE_PLAYLIST_ID },
                };
                return (null, playlist);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                return (msg, null);
            }
        }

        public static async Task<(string, bool)> AddFavoriteTracks(LoginKey oKey, string[] sID)
        {
            try
            {
                LibrarySaveTracksRequest request = new LibrarySaveTracksRequest(sID.ToList());
                bool ret = await oKey.API().Library.SaveTracks(request);
                return (null, ret);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                return (msg, false);
            }
        }

        public static async Task<(string, bool)> DelFavoriteTracks(LoginKey oKey, string[] sID)
        {
            try
            {
                LibraryRemoveTracksRequest request = new LibraryRemoveTracksRequest(sID.ToList());
                bool ret = await oKey.API().Library.RemoveTracks(request);
                return (null, ret);
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
                FullPlaylist data = await oKey.API().Playlists.Get(sID);
                Playlist ret = new Playlist();
                ret.ID = data.Id;
                ret.MID = data.Id;
                ret.MidArray.Spotify = data.Id;
                ret.Title = data.Name;
                ret.CreatorName = data.Owner.DisplayName;
                ret.CreatorID = data.Owner.Id;
                ret.Desc = data.Description;
                if (data.Images != null && data.Images.Count() > 0)
                    ret.ImgUrl = data.Images[0].Url;
                ret.Tracks = new System.Collections.ObjectModel.ObservableCollection<Track>();

                int offset = 0;
                int limit = 100;
                while (true)
                {
                    Paging<PlaylistTrack<IPlayableItem>> plist = await oKey.API().Playlists.GetItems(sID, new PlaylistGetItemsRequest(PlaylistGetItemsRequest.AdditionalTypes.Track) { Offset = offset, Limit = limit });
                    for (int i = 0; i < plist.Items.Count(); i++)
                    {
                        Track item = ConvertTrack((FullTrack)plist.Items[i].Track);
                        ret.Tracks.Add(item);
                    }

                    if (plist == null || plist.Items == null || plist.Items.Count() < limit)
                        break;
                    offset += limit;
                }
                return (null,ret);
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
                FullPlaylist data = await oKey.API().Playlists.Create(oKey.UserID, new PlaylistCreateRequest(sPlaylistName) { Description = sPlaylistDesc });
                Playlist ret = new Playlist();
                ret.ID = data.Id;
                ret.MID = data.Id;
                ret.MidArray.Spotify = data.Id;
                ret.Title = data.Name;
                ret.CreatorName = data.Owner.DisplayName;
                ret.CreatorID = data.Owner.Id;
                ret.Desc = data.Description;
                ret.Tracks = new System.Collections.ObjectModel.ObservableCollection<Track>();
                if (data.Images != null && data.Images.Count() > 0)
                    ret.ImgUrl = data.Images[0].Url;
                return (null, ret);

            }
            catch (Exception e)
            {
                string msg = e.Message;
                return (msg, null);
            }
        }

        public static async Task<(string, bool)> DeletePlaylist(LoginKey oKey, string sPlaylistDirID)
        {
            try
            {
                if (sPlaylistDirID == MY_FAVORITE_PLAYLIST_ID)
                    return (null, true);

                bool ret = await oKey.API().Follow.UnfollowPlaylist(sPlaylistDirID);
                return (null, ret);
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
                    return await AddFavoriteTracks(oKey, sSongidArray);

                PlaylistAddItemsRequest data = new PlaylistAddItemsRequest(sSongidArray.ToList()) ;
                SnapshotResponse ret = await oKey.API().Playlists.AddItems(sPlaylistID, data);
                return (null,true);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                return (msg,false);
            }
        }

        public static async Task<(string, bool)> DelSongFromPlaylist(LoginKey oKey, string[] sSongidArray, string sPlaylistID)
        {
            try
            {
                if (sPlaylistID == MY_FAVORITE_PLAYLIST_ID)
                    return await DelFavoriteTracks(oKey, sSongidArray);

                List<PlaylistRemoveItemsRequest.Item> items = new List<PlaylistRemoveItemsRequest.Item>();
                foreach (var item in sSongidArray)
                    items.Add(new PlaylistRemoveItemsRequest.Item() { Uri = item });

                PlaylistRemoveItemsRequest data = new PlaylistRemoveItemsRequest(items);
                SnapshotResponse ret = await oKey.API().Playlists.RemoveItems(sPlaylistID, data);
                return (null, true);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                return (msg, false);
            }
        }



        #endregion


        public static async Task<ObservableCollection<Common.Track>> Search(LoginKey oKey, string sSearchText)
        {
            SearchRequest request = new SearchRequest(SearchRequest.Types.Track, sSearchText) { Limit = 30 };
            SearchResponse response = await oKey.API().Search.Item(request);

            Paging<FullTrack, SearchResponse> items = response.Tracks;
            ObservableCollection<Common.Track> ret = new ObservableCollection<Common.Track>();
            if (response != null && response.Tracks != null)
            {
                foreach (var item in response.Tracks.Items)
                    ret.Add(ConvertTrack(item));
            }
            return ret;
        }


        private static Common.Track ConvertTrack(FullTrack from)
        {
            Track item = new Track();
            item.Title = from.Name;
            item.ID = from.Id;
            item.MID = from.Uri;
            item.MidArray.Spotify = from.Id;
            item.Duration = from.DurationMs / 1000;
            item.DurationStr = AIGS.Helper.TimeHelper.ConverIntToString(item.Duration);
            item.AlbumID = from.Album.Id;
            item.AlbumTitle = from.Album.Name;
            item.Artists = new System.Collections.ObjectModel.ObservableCollection<Artist>();

            for (int j = 0; j < from.Artists.Count(); j++)
            {
                item.Artists.Add(new Artist()
                {
                    ID = from.Artists[j].Id,
                    MID = from.Artists[j].Id,
                    Name = from.Artists[j].Name,
                });

            }
            item.ArtistsName = Method.MergeArtistsName(item.Artists);
            (item.Live, item.TitleBrief) = Method.RemoveTilteLiveFlag(item.Title);

            return item;
        }
    }
}
