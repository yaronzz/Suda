using AIGS.Common;
using AIGS.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static SudaLib.Common;

namespace SudaLib
{
    public class Method
    {

        #region Network
        /// <summary>
        /// Ger user access key
        /// </summary>
        /// <param name="type"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<(string,object)> GetLoginKey(ePlatform type, params string[] paras)
        {
            string msg = null;
            object data = null;
            if (type == ePlatform.QQMusic)
            {
                (msg, data) = QQMusic.GetLoginKey(paras[0], paras[1]);
            }
            else if (type == ePlatform.CloudMusic)
            {
                (msg, data) = await CloudMusic.GetLoginKey(paras[0], paras[1]);
            }
            else if (type == ePlatform.Tidal)
            {
                (msg, data) = await Tidal.GetLoginKey(paras[0], paras[1]);
            }
            else if (type == ePlatform.Spotify)
            {
                (msg, data) = await Spotify.GetLoginKey(paras[0], paras[1]);
            }

            return (msg, data);
        }

        public static async Task<(string, object)> RefreshLoginKey(object loginKey)
        {
            try
            {
                if (loginKey == null)
                    return ("Loginkey is null!",null);

                ePlatform type = GetPlatform(loginKey);
                if (type == ePlatform.QQMusic)
                {
                    (string msg, bool data) = await QQMusic.IsValidLoginKey((QQMusic.LoginKey)loginKey);
                    return data ? ("",loginKey) : (msg,null);
                }
                else if (type == ePlatform.CloudMusic)
                {
                    (string msg, bool data) = await CloudMusic.IsValidLoginKey((CloudMusic.LoginKey)loginKey);
                    return data ? ("", loginKey) : (msg, null);
                }
                else if (type == ePlatform.Tidal)
                {
                    (string msg, Tidal.LoginKey data) = await Tidal.GetLoginKey(((Tidal.LoginKey)loginKey).Username, ((Tidal.LoginKey)loginKey).Password);
                    return (msg, data);
                }
                else if (type == ePlatform.Spotify)
                {
                    (string msg, Spotify.LoginKey data) = await Spotify.GetLoginKey(((Spotify.LoginKey)loginKey).AccessToken, ((Spotify.LoginKey)loginKey).RefreshToken);
                    return (msg, data);
                }
            }
            catch(Exception e)
            {
                return (e.Message, null);
            }
            return ("Unknow!", null);
        }

        /// <summary>
        /// Get user information
        /// </summary>
        /// <param name="loginKey"></param>
        /// <returns></returns>
        public static async Task<(string, UserInfo)> GetUserInfo(object loginKey)
        {
            if (loginKey == null)
                return ("Loginkey is null!", null);

            ePlatform type = GetPlatform(loginKey);
            string msg = "Unknow!";
            UserInfo data = null;
            if (type == ePlatform.QQMusic)
            {
                (msg, data) = await QQMusic.GetUserInfo((QQMusic.LoginKey)loginKey);
            }
            else if (type == ePlatform.CloudMusic)
            {
                (msg, data) = await CloudMusic.GetUserInfo((CloudMusic.LoginKey)loginKey);
            }
            else if (type == ePlatform.Tidal)
            {
                (msg, data) = await Tidal.GetUserInfo((Tidal.LoginKey)loginKey);
            }
            else if (type == ePlatform.Spotify)
            {
                (msg, data) = await Spotify.GetUserInfo((Spotify.LoginKey)loginKey);
            }

            return (msg, data);
        }

        /// <summary>
        /// Get playlist by id
        /// </summary>
        /// <param name="loginKey"></param>
        /// <param name="sID"></param>
        /// <returns></returns>
        public static async Task<(string, Playlist)> GetPlaylist(object loginKey, string sID)
        {
            if (loginKey == null)
                return ("Loginkey is null!", null);

            ePlatform type = GetPlatform(loginKey);
            (string msg, Playlist data) = ("Unknow!", null);

            if (type == ePlatform.QQMusic)
            {
                (msg, data) = await QQMusic.GetPlaylist((QQMusic.LoginKey)loginKey, sID);
            }
            else if (type == ePlatform.CloudMusic)
            {
                (msg, data) = await CloudMusic.GetPlaylist((CloudMusic.LoginKey)loginKey, sID);
            }
            else if (type == ePlatform.Tidal)
            {
                (msg, data) = await Tidal.GetPlaylist((Tidal.LoginKey)loginKey, sID);
            }
            else if (type == ePlatform.Spotify)
            {
                (msg, data) = await Spotify.GetPlaylist((Spotify.LoginKey)loginKey, sID);
            }

            return (msg, data);
        }

        /// <summary>
        /// Get playlist array
        /// </summary>
        /// <param name="loginKey"></param>
        /// <param name="sIDArray"></param>
        /// <returns></returns>
        public static async Task<(string, ObservableCollection<Playlist>)> GetPlaylists(object loginKey, string[] sIDArray)
        {
            if (loginKey == null)
                return ("Loginkey is null!", null);

            ObservableCollection<Playlist> ret = new ObservableCollection<Playlist>();
            for (int i = 0; i < sIDArray.Count(); i++)
            {
                (string msg, Playlist data) = await GetPlaylist(loginKey, sIDArray[i]);
                if (data == null)
                    continue;
                ret.Add(data);
            }
            return (null,ret);
        }

        /// <summary>
        /// Get user playlist array
        /// </summary>
        /// <param name="loginKey"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public static async Task<(string, ObservableCollection<Playlist>)> GetUserPlaylists(object loginKey)
        {
            if (loginKey == null)
                return ("Loginkey is null!", null);

            ePlatform type = GetPlatform(loginKey);
            (string msg, ObservableCollection<Playlist> data) = ("Unknow!", null);

            if (type == ePlatform.QQMusic)
            {
                (msg, data) = await QQMusic.GetUserPlaylist((QQMusic.LoginKey)loginKey);
            }
            else if (type == ePlatform.CloudMusic)
            {
                (msg, data) = await CloudMusic.GetUserPlaylist((CloudMusic.LoginKey)loginKey);
            }
            else if (type == ePlatform.Tidal)
            {
                (msg, data) = await Tidal.GetUserPlaylist((Tidal.LoginKey)loginKey);
            }
            else if (type == ePlatform.Spotify)
            {
                (msg, data) = await Spotify.GetUserPlaylist((Spotify.LoginKey)loginKey);
            }

            return (msg, data);
        }

        /// <summary>
        /// Add track array to playlist
        /// </summary>
        /// <param name="loginKey"></param>
        /// <param name="trackIDs"></param>
        /// <param name="playlistID"></param>
        /// <returns></returns>
        public static async Task<(string, bool)> AddTracksToPlaylist(object loginKey, string[] trackIDs, string playlistID, bool isMyfavorite = false)
        {
            if (loginKey == null)
                return ("Loginkey is null!", false);

            ePlatform type = GetPlatform(loginKey);

            if (type == ePlatform.QQMusic)
            {
                return await QQMusic.AddSongToPlaylist((QQMusic.LoginKey)loginKey, trackIDs, playlistID);
            }
            else if(type == ePlatform.CloudMusic)
            {
                return await CloudMusic.AddSongToPlaylist((CloudMusic.LoginKey)loginKey, trackIDs, playlistID);
            }
            else if (type == ePlatform.Tidal)
            {
                if (isMyfavorite)
                    playlistID = Tidal.MY_FAVORITE_PLAYLIST_ID;
                return await Tidal.AddSongToPlaylist((Tidal.LoginKey)loginKey, trackIDs, playlistID);
            }
            else if (type == ePlatform.Spotify)
            {
                return await Spotify.AddSongToPlaylist((Spotify.LoginKey)loginKey, trackIDs, playlistID);
            }
            return ("Unknow!", false);
        }

        /// <summary>
        /// Delete tracks from playlist
        /// </summary>
        /// <param name="loginKey"></param>
        /// <param name="trackIDs"></param>
        /// <param name="playlistID"></param>
        /// <returns></returns>
        public static async Task<(string, bool)> DelTracksFromPlaylist(object loginKey, string[] trackIDs, string playlistID, bool isMyfavorite = false)
        {
            if (loginKey == null)
                return ("Loginkey is null!", false);

            ePlatform type = GetPlatform(loginKey);

            if (type == ePlatform.QQMusic)
            {
                return await QQMusic.DelSongFromPlaylist((QQMusic.LoginKey)loginKey, trackIDs, playlistID);
            }
            else if (type == ePlatform.CloudMusic)
            {
                return await CloudMusic.DelSongFromPlaylist((CloudMusic.LoginKey)loginKey, trackIDs, playlistID);
            }
            else if (type == ePlatform.Tidal)
            {
                if (isMyfavorite)
                    playlistID = Tidal.MY_FAVORITE_PLAYLIST_ID;
                return await Tidal.DelSongFromPlaylist((Tidal.LoginKey)loginKey, trackIDs, playlistID);
            }
            else if (type == ePlatform.Spotify)
            {
                return await Spotify.DelSongFromPlaylist((Spotify.LoginKey)loginKey, trackIDs, playlistID);
            }
            return ("Unknow!", false);
        }

        /// <summary>
        /// Creat playlist
        /// </summary>
        /// <param name="loginKey"></param>
        /// <param name="playlistName"></param>
        /// <param name="playlistDesc"></param>
        /// <returns></returns>
        public static async Task<(string, Playlist)> CreatPlaylist(object loginKey, string playlistName, string playlistDesc)
        {
            if (loginKey == null)
                return ("Loginkey is null!", null);

            ePlatform type = GetPlatform(loginKey);
            (string msg, Playlist data) = ("Unknow!", null);

            if (playlistDesc.IsBlank())
                playlistDesc = "CREAT BY SUDA.";

            if (type == ePlatform.QQMusic)
            {
                (msg,data) = await QQMusic.CreatPlaylist((QQMusic.LoginKey)loginKey, playlistName, playlistDesc);
            }
            else if (type == ePlatform.CloudMusic)
            {
                (msg, data) = await CloudMusic.CreatPlaylist((CloudMusic.LoginKey)loginKey, playlistName, playlistDesc);
            }
            else if (type == ePlatform.Tidal)
            {
                (msg, data) = await Tidal.CreatPlaylist((Tidal.LoginKey)loginKey, playlistName, playlistDesc);
            }
            else if (type == ePlatform.Spotify)
            {
                (msg, data) = await Spotify.CreatPlaylist((Spotify.LoginKey)loginKey, playlistName, playlistDesc);
            }

            return (msg, data);
        }

        /// <summary>
        /// Delete playlist
        /// </summary>
        /// <param name="loginKey"></param>
        /// <param name="playlistID"></param>
        /// <returns></returns>
        public static async Task<(string, bool)> DeletePlaylist(object loginKey, string playlistID)
        {
            if (loginKey == null)
                return ("Loginkey is null!", false);

            ePlatform type = GetPlatform(loginKey);

            if (type == ePlatform.QQMusic)
            {
                return await QQMusic.DeletePlaylist((QQMusic.LoginKey)loginKey, playlistID);
            }
            else if (type == ePlatform.CloudMusic)
            {
                return await CloudMusic.DeletePlaylist((CloudMusic.LoginKey)loginKey, playlistID);
            }
            else if (type == ePlatform.Tidal)
            {
                return await Tidal.DeletePlaylist((Tidal.LoginKey)loginKey, playlistID);
            }
            else if (type == ePlatform.Spotify)
            {
                return await Spotify.DeletePlaylist((Spotify.LoginKey)loginKey, playlistID);
            }
            return ("Unknow!", false);
        }

        #endregion


        #region Search

        public static (ePlatform,string) GetPlaylistIDByUrl(string url)
        {
            if (url.IsBlank())
                return (ePlatform.None, null);

            /*QQ
             * [??????]
             * https://c.y.qq.com/base/fcgi-bin/u?__=V3m82wi
             * [playlist/xxxxxx]
             * https://y.qq.com/n/yqq/playlist/7650485105.html
             * [id=xxxxx]
             * https://y.qq.com/n/m/detail/taoge/index.html?id=7650485105
             * https://i.y.qq.com/n2/m/share/details/taoge.html?hosteuin=oiEAoinqow-5&id=4126894351&appversion=100201&ADTAG=wxfshare&appshare=iphone_wx
            */
            string ret;
            if (url.Contains("y.qq.com"))
            {
                if((ret = StringHelper.GetSubString(url, "id=", "&")).IsNotBlank())
                    return (ePlatform.QQMusic, ret);
                if((ret = StringHelper.GetSubString(url, "playlist/", ".html")).IsNotBlank())
                    return (ePlatform.QQMusic, ret);
            }

            /*Cloud
             * http://music.163.com/playlist?id=624669757&userid=43010093
             * http://music.163.com/playlist/8058083/583471/?userid=43010093
            */
            if (url.Contains("music.163.com"))
            {
                if ((ret = StringHelper.GetSubString(url, "id=", "&")).IsNotBlank())
                    return (ePlatform.CloudMusic, ret);
                if ((ret = StringHelper.GetSubString(url, "playlist/", "/")).IsNotBlank())
                    return (ePlatform.CloudMusic, ret);
            }

            /*Tidal
             * https://listen.tidal.com/playlist/90e77c1f-dba0-497b-ae52-bd778ee9625e
             * https://tidal.com/browse/playlist/90e77c1f-dba0-497b-ae52-bd778ee9625e
            */
            if (url.Contains("tidal.com"))
            {
                if ((ret = StringHelper.GetSubString(url, "playlist/", "/")).IsNotBlank())
                    return (ePlatform.Tidal, ret);
            }

            /*Spotify
             * https://open.spotify.com/playlist/37i9dQZF1DX6GwdWRQMQpq?si=M8Tlo1CPRFqyK2rxH9Z6Tw
             * https://open.spotify.com/playlist/37i9dQZF1E36zuFBPlUFKB
            */
            if (url.Contains("spotify.com"))
            {
                if ((ret = StringHelper.GetSubString(url, "playlist/", "?")).IsNotBlank())
                    return (ePlatform.Spotify, ret);
            }

            return (ePlatform.None, null);
        }

        public static async Task<Track> SearchTrack(object loginKey, Track track, CompareArgs compare)
        {
            if (loginKey == null || track == null)
                return null;

            //get search text
            string sSearchTxt = track.TitleBrief + " - " + track.Artists[0].Name;

            //search by different platform
            ObservableCollection<Common.Track> SearchItems = null;
            ePlatform type = GetPlatform(loginKey);
            if (type == ePlatform.QQMusic)
            {
                SearchItems = await QQMusic.Search((QQMusic.LoginKey)loginKey, sSearchTxt);
            }
            else if (type == ePlatform.CloudMusic)
            {
                SearchItems = await CloudMusic.Search((CloudMusic.LoginKey)loginKey, sSearchTxt);
            }
            else if (type == ePlatform.Tidal)
            {
                SearchItems = await Tidal.Search((Tidal.LoginKey)loginKey, sSearchTxt);
            }
            else if (type == ePlatform.Spotify)
            {
                SearchItems = await Spotify.Search((Spotify.LoginKey)loginKey, sSearchTxt);
            }

            //compare and return correct track
            if (SearchItems == null)
                return null;

            int index = MatchTrack(SearchItems, track, compare);
            if (index < 0)
                return null;
            return SearchItems[index];
        }
        #endregion



        #region Match

        private static int CalcTrackWeight(Track item1, Track item2, CompareArgs compare = null)
        {
            int value = 50;
            if (MatchMidArray(item1.MidArray, item2.MidArray))
                return value;

            //title
            if (item1.TitleBrief != item2.TitleBrief)
                value = 0;

            //artist
            if (item1.ArtistsName != item1.ArtistsName)
                value = 0;

            //live
            if (item1.Live != item2.Live)
            {
                if (compare == null || compare.AlbumTitle == false)
                    value -= 20;
                else
                    value = 0;
            }

            //album
            if (item1.AlbumTitle != item2.AlbumTitle)
            {
                if (compare == null || compare.AlbumTitle == false)
                    value -= 10;
                else
                    value = 0;
            }

            return value;
        }

        public static int MatchTrack(ObservableCollection<Common.Track> items, Track track, CompareArgs compare = null)
        {
            int retIndx = -1;
            int retValue = -1;

            for (int i = 0; i < items.Count; i++)
            {
                int value = CalcTrackWeight(items[i], track, compare);
                if (value <= 0)
                    continue;

                if(value > retValue)
                {
                    retValue = value;
                    retIndx = i;
                }
            }

            return retIndx;
        }

        #endregion


        #region Tool

        public static ePlatform GetPlatform(object loginKey)
        {
            if (loginKey == null)
                return ePlatform.None;

            Type type = loginKey.GetType();
            if (type == typeof(QQMusic.LoginKey))
                return ePlatform.QQMusic;
            else if (type == typeof(CloudMusic.LoginKey))
                return ePlatform.CloudMusic;
            else if (type == typeof(Tidal.LoginKey))
                return ePlatform.Tidal;
            else if (type == typeof(Spotify.LoginKey))
                return ePlatform.Spotify;
            return ePlatform.None;
        }

        public static string MergeArtistsName(ObservableCollection<Artist> Artists, string sDiv = "/")
        {
            string ret = null;
            for (int i = 0; Artists != null && i < Artists.Count(); i++)
            {
                string name = Artists[i].Name;
                if (name.IsBlank())
                    continue;

                ret += name.Trim();
                if (i < Artists.Count() - 1)
                    ret += sDiv;
            }
            return ret;
        }


        public static (bool, string) RemoveTilteLiveFlag(string title)
        {
            string flag = "(live)";
            string lowTitle = title.ToLower();
            bool   isLive = lowTitle.Contains(flag);

            if (isLive)
            {
                int index = lowTitle.IndexOf(flag);

                string str = title.Substring(index, flag.Length);
                title = title.Replace(str, "").Trim();
            }

            return (isLive, title);
        }

        public static bool MatchMidArray(MIDArray item1, MIDArray item2)
        {
            if (item1.QQMusic != null && item1.QQMusic == item2.QQMusic)
                return true;
            if (item1.CloudMusic != null && item1.CloudMusic == item2.CloudMusic)
                return true;
            if (item1.Tidal != null && item1.Tidal == item2.Tidal)
                return true;
            if (item1.Spotify != null && item1.Spotify == item2.Spotify)
                return true;
            if (item1.AppleMusic != null && item1.AppleMusic == item2.AppleMusic)
                return true;
            return false;
        }

        //public static string GetPlatformDisplayName(ePlatform type)
        //{
        //    if (type == ePlatform.AppleMusic)
        //        return "Apple Music";
        //    else if (type == ePlatform.CloudMusic)
        //        return "网易云";
        //    else if (type == ePlatform.QQMusic)
        //        return "QQ音乐";
        //    else if (type == ePlatform.Spotify)
        //        return "Spotify";
        //    else if (type == ePlatform.Tidal)
        //        return "Tidal";
        //    else
        //        return "Unknow";
        //}

        public static string FormatImgUrl(string url)
        {
            if (url.IsBlank())
                return url;

            int index = url.IndexOf("?");
            if(index > 0)
                url = url.Substring(0, index);
            return url;
        }

        public static string GetFavoritePlaylistName()
        {
            return "My Favorite";
        }

        public static object GetImageObj(string url)
        {
            try
            {
                if (url.IsBlank())
                    return null;

                object data = AIGS.Common.Convert.ConverByteArrayToBitmapImage(NetHelper.DownloadData(url));
                if (data == null)
                    return "https://i.loli.net/2020/07/29/hgXxGae51fDVHQJ.jpg";
                return data;
            }
            catch
            {
                return url;
            }
        }
#endregion
    }
}
