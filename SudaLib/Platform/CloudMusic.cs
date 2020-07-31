using AIGS.Common;
using AIGS.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static AIGS.Helper.HttpHelper;

namespace SudaLib
{
    public class CloudMusic
    {
        #region model

        public class LoginKey
        {
            public string UserID { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Cookie { get; set; }
            public string Csrf { get; set; }
        }

        private class UserInfo
        {
            public string NickName { get; set; }    
            public string AvatarUrl { get; set; } 
            public string UserID { get; set; }  
        }

        private class PlaylistBrief
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string UserId { get; set; }
            public string CoverImgUrl { get; set; }
        }

        private class Playlist
        {
            public string ID { get; set; } 
            public string Name { get; set; }

            public string UserId { get; set; }
            public string NickName { get; set; }

            public string CoverImgUrl { get; set; } 
            public string Description { get; set; } 
            public List<Track> Tracks { get; set; }
        }

        private class Track
        {
            public string Name { set; get; } 
            public string ID { set; get; } 
            public int Dt { set; get; } //Duration(ms)
            public Album Al { get; set; } 
            public List<Artist> Ar { get; set; } 
        }

        private class Artist
        {
            public string ID { set; get; }
            public string Name { set; get; }
        }

        private class Album
        {
            public string ID { set; get; } 
            public string Name { set; get; } 
            public string PicUrl { set; get; } 
        }

        private class SearchItem
        {
            public string Name { set; get; }
            public string ID { set; get; }
            public int Duration { set; get; } //Duration(ms)
            public Album Album { get; set; }
            public List<Artist> Artists { get; set; }
        }

        #endregion

        #region Login

        public async static Task<(string,LoginKey)> GetLoginKey(string sUserName, string sPassword)
        {
            string url = "http://music.163.com/weapi/login";
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "username" , sUserName},
                { "password" , Encrypt_Music163.MD5(sPassword)},
                { "rememberLogin" , "true"}
            };
            CookieContainer cookie = new CookieContainer();
            (string errmsg, string sjson) = await HttpRequest(url, data, cookie);
            if (errmsg.IsNotBlank() || sjson.IsBlank() || JsonHelper.GetValue(sjson, "code") != "200")
                return (errmsg,null);

            LoginKey oKey = new LoginKey();
            oKey.Username = sUserName;
            oKey.Password = sPassword;
            oKey.UserID = JsonHelper.GetValue(sjson, "profile", "userId");
            oKey.Cookie = AIGS.Common.Convert.ConverCookieCollectionToString(cookie, "http://music.163.com");
            oKey.Csrf = cookie.GetCookies(new Uri("http://music.163.com"))["__csrf"].ToString().Replace("__csrf=","");
            return (null,oKey);
        }

        public static async Task<(string, bool)> IsValidLoginKey(LoginKey oKey)
        {
            try
            {
                string url = $"https://music.163.com/weapi/nmusician/userinfo/get?csrf_token={oKey.Csrf}";
                Dictionary<string, string> data = new Dictionary<string, string>()
                {
                    { "csrf_token" , oKey.Csrf},
                };
                (string errmsg, string sjson) = await HttpRequest(url, data, null, oKey.Cookie);
                if (errmsg.IsNotBlank() || sjson.IsBlank() || JsonHelper.GetValue(sjson, "code") != "200")
                    return (errmsg, false);
            }
            catch(Exception e)
            {
                string msg = e.Message;
            }

            return (null, true);
        }
        #endregion


        #region User info
        public static async Task<(string, Common.UserInfo)> GetUserInfo(LoginKey oKey)
        {
            string url = $"https://music.163.com/weapi/v1/user/detail/{oKey.UserID}";
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "csrf_token" , oKey.Csrf},
            };
            (string errmsg, string json) = await HttpRequest(url, data, null, oKey.Cookie);
            if (errmsg.IsNotBlank() || json.IsBlank() || JsonHelper.GetValue(json, "code") != "200")
                return (errmsg, null);

            UserInfo ret = JsonHelper.ConverStringToObject<UserInfo>(json, "profile");
            return (null,CloudMusicUserInfoConvert(ret));
        }

        public static async Task<(string, ObservableCollection<Common.Playlist>)> GetUserPlaylist(LoginKey oKey)
        {
            string url = $"https://music.163.com/weapi/user/playlist";
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "offset" , "0"},
                { "limit" , "50"},
                { "uid" , oKey.UserID},
            };
            (string errmsg, string json) = await HttpRequest(url, data, null, oKey.Cookie);
            if (errmsg.IsNotBlank() || json.IsBlank() || JsonHelper.GetValue(json, "code") != "200")
                return (errmsg,null);

            List<PlaylistBrief> briefs = JsonHelper.ConverStringToObject<List<PlaylistBrief>>(json, "playlist");
            for (int i = briefs.Count() - 1; i >= 0; i--)
            {
                if (briefs[i].UserId != oKey.UserID)
                    briefs.RemoveAt(i);
            }

            //Get playlist by id
            ObservableCollection<Common.Playlist> plists = new ObservableCollection<Common.Playlist>();
            for (int i = 0; i < briefs.Count(); i++)
            {
                (string errmsg2, Common.Playlist item) = await GetPlaylist(oKey, briefs[i].ID);
                if (item != null)
                    plists.Add(item);
            }

            //第一个歌单为”我喜欢的音乐“，需要做标记
            if(plists.Count() > 0 && plists[0].Title.Contains("喜欢的音乐"))
            {
                plists[0].Title = Method.GetFavoritePlaylistName();
                plists[0].MyFavorite = true;
            }

            return (null, plists);
        }

        #endregion


        #region Playlist

        public static async Task<(string, Common.Playlist)> GetPlaylist(LoginKey oKey, string sID = "7516711891")
        {
            string url = $"https://music.163.com/weapi/v3/playlist/detail";
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "id" , sID},
                { "n" , "100000"},
            };
            //Http request
            (string errmsg, string json) = await HttpRequest(url, data, null, oKey.Cookie);
            if (errmsg.IsNotBlank() || json.IsBlank() || JsonHelper.GetValue(json, "code") != "200")
                return (errmsg,null);

            //Parsing json
            Playlist ret = JsonHelper.ConverStringToObject<Playlist>(json, "playlist");
            ret.NickName = JsonHelper.GetValue(json, "playlist", "creator", "nickname");
            return (null,CloudMusicPlaylistConvert(ret));
        }

        public static async Task<(string, Common.Playlist)> CreatPlaylist(LoginKey oKey, string sPlaylistName, string sPlaylistDesc)
        {
            string url = $"https://music.163.com/weapi/playlist/create";
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "name" , sPlaylistName},
                { "privacy" , "0"},
            };
            //Http request
            (string errmsg, string json) = await HttpRequest(url, data, null, oKey.Cookie + ";os=pc");
            if (errmsg.IsNotBlank() || json.IsBlank() || JsonHelper.GetValue(json, "code") != "200")
                return (errmsg, null);

            //Parsing json
            Playlist ret = JsonHelper.ConverStringToObject<Playlist>(json, "playlist");
            ret.NickName = JsonHelper.GetValue(json, "playlist", "creator", "nickname");
            return (null, CloudMusicPlaylistConvert(ret));
        }

        public static async Task<(string, bool)> DeletePlaylist(LoginKey oKey, string sPlaylistID)
        {
            string url = $"https://music.163.com/weapi/playlist/remove";
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "ids" , '[' + sPlaylistID + ']'},
            };
            //Http request
            (string errmsg, string json) = await HttpRequest(url, data, null, oKey.Cookie + ";os=pc");
            if (errmsg.IsNotBlank() || json.IsBlank() || JsonHelper.GetValue(json, "code") != "200")
                return (errmsg, false);

            return (null, true);
        }

        public static async Task<(string, bool)> AddSongToPlaylist(LoginKey oKey, string[] sSongidArray, string sPlaylistID)
        {
            string url = $"https://music.163.com/weapi/playlist/manipulate/tracks";
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "op" , "add"},
                { "pid" , sPlaylistID},
                { "trackIds" , '[' + string.Join(",",sSongidArray) + ']'},
            };
            (string errmsg, string json) = await HttpRequest(url, data, null, oKey.Cookie + ";os=pc");
            if (errmsg.IsNotBlank() || json.IsBlank() || JsonHelper.GetValue(json, "code") != "200")
            {
                if (errmsg.IsBlank())
                    errmsg = JsonHelper.GetValue(json, "message");
                return (errmsg, false);
            }
            return (null, true);
        }

        public static async Task<(string, bool)> DelSongFromPlaylist(LoginKey oKey, string[] sSongidArray, string sPlaylistID)
        {
            string url = $"https://music.163.com/weapi/playlist/manipulate/tracks";
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "op" , "del"},
                { "pid" , sPlaylistID},
                { "trackIds" , '[' + string.Join(",",sSongidArray)+ ']'},
            };
            (string errmsg, string json) = await HttpRequest(url, data, null, oKey.Cookie);
            if (errmsg.IsNotBlank() || json.IsBlank() || JsonHelper.GetValue(json, "code") != "200")
                return (errmsg,false);
            return (null,true);
        }
       
        #endregion

        #region Search

        public static async Task<ObservableCollection<Common.Track>> Search(LoginKey oKey, string sSearchText)
        {
            string url = $"https://music.163.com/weapi/search/get";
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "s" , sSearchText},
                { "limit" , "30"},
                { "type" , "1"},
                { "offset" , "0"},
            };
            (string errmsg, string json) = await HttpRequest(url, data, null, oKey.Cookie);
            if (errmsg.IsNotBlank() || json.IsBlank())
                return null;

            ObservableCollection<SearchItem> items = JsonHelper.ConverStringToObject<ObservableCollection<SearchItem>>(json, "result", "songs");
            ObservableCollection<Common.Track> ret = new ObservableCollection<Common.Track>();
            if (items != null)
            {
                foreach (var item in items)
                {
                    ret.Add(CloudMusicSearchItemConvert(item));
                }
            }
            return ret;
        }

        #endregion


        #region Http request
        async static Task<(string, string)> HttpRequest(string url, Dictionary<string, string> ldata, CookieContainer container = null, string cookieStr = null)
        {
            try
            {
                string data = ldata == null ? null : Encrypt_Music163.EncryptedRequest(JsonHelper.ConverObjectToString<Dictionary<string, string>>(ldata));
                Dictionary<HttpRequestHeader, string> Headers = new Dictionary<HttpRequestHeader, string>();
                Headers.Add(HttpRequestHeader.AcceptLanguage, "h-CN,zh;q=0.8,gl;q=0.6,zh-TW;q=0.4");
                if (cookieStr.IsNotBlank())
                    Headers.Add(HttpRequestHeader.Cookie, cookieStr);

                Result result = await AIGS.Helper.HttpHelper.GetOrPostAsync(url,
                    Accept: "*/*",
                    Referer: "https://music.163.com/",
                    Host: "music.163.com",
                    ContentType: "application/x-www-form-urlencoded",
                    UserAgent: "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.97 Safari/537.36",
                    Cookie:container,
                    PostJson: data,
                    Headers: Headers);
                return (result.Errresponse, result.sData);
            }
            catch (Exception e)
            {
                return (e.Message, null);
            }
        }
        #endregion


        #region ConvertToCommon

        static Common.UserInfo CloudMusicUserInfoConvert(CloudMusic.UserInfo data)
        {
            Common.UserInfo ret = new Common.UserInfo();
            ret.NickName = data.NickName;
            ret.UserID = data.UserID;
            ret.AvatarUrl = data.AvatarUrl;
            return ret;
        }

        static Common.Playlist CloudMusicPlaylistConvert(CloudMusic.Playlist data)
        {
            Common.Playlist ret = new Common.Playlist();
            ret.Title = data.Name;
            ret.ID = data.ID;
            ret.MID = data.ID;
            ret.MidArray.CloudMusic = data.ID;
            //https://p1.music.126.net/SsPvUEo8WSIwAiBuWEGFXg==/18448705602716641.jpg?param=200y200
            ret.ImgUrl = data.CoverImgUrl + "?param=200y200";  //不加这个参数，Image控件加载的时候，内存会飙升到你想象不到的地步，凸(艹皿艹 )
            ret.Desc = data.Description;
            ret.CreatorName = data.NickName;
            ret.CreatorID = data.UserId;
            ret.Tracks = new ObservableCollection<Common.Track>();

            for (int i = 0; data.Tracks != null && i < data.Tracks.Count(); i++)
            {
                Common.Track track = new Common.Track();
                track.Title = data.Tracks[i].Name;
                track.ID = data.Tracks[i].ID;
                track.MID = data.Tracks[i].ID;
                track.MidArray.CloudMusic = data.Tracks[i].ID;
                track.Duration = data.Tracks[i].Dt/1000;
                track.DurationStr = AIGS.Helper.TimeHelper.ConverIntToString(track.Duration);
                track.AlbumID = data.Tracks[i].Al.ID;
                track.AlbumTitle = data.Tracks[i].Al.Name;
                track.Artists = new ObservableCollection<Common.Artist>();

                for (int j = 0; j < data.Tracks[i].Ar.Count; j++)
                {
                    Common.Artist artist = new Common.Artist();
                    artist.Name = data.Tracks[i].Ar[j].Name;
                    artist.ID = data.Tracks[i].Ar[j].ID;
                    artist.MID = data.Tracks[i].Ar[j].ID;
                    track.Artists.Add(artist);

                }
                track.ArtistsName = Method.MergeArtistsName(track.Artists);
                (track.Live, track.TitleBrief) = Method.RemoveTilteLiveFlag(track.Title);
                ret.Tracks.Add(track);
            }
            return ret;
        }

        static Common.Track CloudMusicSearchItemConvert(CloudMusic.SearchItem data)
        {
            Common.Track track = new Common.Track();
            track.Title = data.Name;
            track.ID = data.ID;
            track.MID = data.ID;
            track.MidArray.CloudMusic = data.ID;
            track.Duration = data.Duration / 1000;
            track.DurationStr = AIGS.Helper.TimeHelper.ConverIntToString(track.Duration);
            track.AlbumID = data.Album.ID;
            track.AlbumTitle = data.Album.Name;
            track.Artists = new ObservableCollection<Common.Artist>();

            for (int j = 0; j < data.Artists.Count; j++)
            {
                Common.Artist artist = new Common.Artist();
                artist.Name = data.Artists[j].Name;
                artist.ID = data.Artists[j].ID;
                artist.MID = data.Artists[j].ID;
                track.Artists.Add(artist);

            }
            track.ArtistsName = Method.MergeArtistsName(track.Artists);
            (track.Live, track.TitleBrief) = Method.RemoveTilteLiveFlag(track.Title);
            return track;
        }

        #endregion

       


        #region 
        class Encrypt_Music163
        {
            // 固定值
            private const String nonce = "0CoJUm6Qyw8W8jud";
            private const String iv = "0102030405060708";
            private const String pubKey = "010001";
            private const String modulus = "00e0b509f6259df8642dbc35662901477df22677ec152b5ff68ace615bb7" +
                                           "b725152b3ab17a876aea8a5aa76d2e417629ec4ee341f56135fccf695280" +
                                           "104e0312ecbda92557c93870114af6c9d05c4f7f0c3685b7a46bee255932" +
                                           "575cce10b424d813cfe4875d3e82047b97ddef52741d546b8e289dc6935b" +
                                           "3ece0462db0a22b8e7";

            public static string MD5(string str)
            {
                try
                {
                    MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                    byte[] bytValue, bytHash;
                    bytValue = System.Text.Encoding.UTF8.GetBytes(str);
                    bytHash = md5.ComputeHash(bytValue);
                    md5.Clear();
                    string sTemp = "";
                    for (int i = 0; i < bytHash.Length; i++)
                    {
                        sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
                    }
                    str = sTemp.ToLower();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                return str;
            }

            /// <summary>
            /// 对明文数据进行加密
            /// </summary>
            /// <param name="text">明文数据</param>
            /// <returns>加密后的数据</returns>
            public static String EncryptedRequest(String text)
            {
                // key
                var secKey = CreateSecretKey(16);
                //Console.WriteLine($"secKey:{secKey}");
                //有时候会失败  所以直接固定密钥了
                secKey = "oUCb6k473aBQFUgv";
                // aes
                var encTextFisrt = AesEncrypt(text, nonce, iv);
                var encText = AesEncrypt(encTextFisrt, secKey, iv);
                // rsa
                var encSecKey = RsaEncrypt(secKey, pubKey, modulus);
                Console.WriteLine(encSecKey);
                var data = $"params={encText.UrlEncode()}&encSecKey={encSecKey}";
                return data;
            }

            /// <summary>
            /// 生成随机SecKey
            /// </summary>
            /// <param name="size">key长度</param>
            /// <returns>SecKey</returns>
            private static String CreateSecretKey(Int32 size)
            {
                // String keys = "0123456789ABCDEF";
                var keys = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                var key = String.Empty;
                var rd = new Random();

                for (Int32 i = 0; i < size; i++)
                {
                    key += keys[rd.Next(keys.Length)].ToString();
                }

                return key;
            }

            /// <summary>
            /// AES加密
            /// </summary>
            /// <param name="data">数据</param>
            /// <param name="secKey">私钥</param>
            /// <param name="iv">iv</param>
            /// <returns>加密后的数据</returns>
            public static String AesEncrypt(String data, String secKey, String iv)
            {
                var rijndaelCipher = new RijndaelManaged();
                rijndaelCipher.Mode = CipherMode.CBC;
                rijndaelCipher.Padding = PaddingMode.PKCS7;
                rijndaelCipher.KeySize = 128;

                var keyBytes = Encoding.UTF8.GetBytes(secKey);
                rijndaelCipher.Key = keyBytes;

                var ivBytes = Encoding.UTF8.GetBytes(iv);
                rijndaelCipher.IV = ivBytes;

                var dataBytes = Encoding.UTF8.GetBytes(data);
                ICryptoTransform transform = rijndaelCipher.CreateEncryptor();
                var encryptedData = transform.TransformFinalBlock(dataBytes, 0, dataBytes.Length);

                return System.Convert.ToBase64String(encryptedData);

            }

            /// <summary>
            /// 生成EncSecKey
            /// </summary>
            /// <param name="text">第一个数据</param>
            /// <param name="pubKey">第二个数据</param>
            /// <param name="modulus">第三个数据</param>
            /// <returns>EncSecKey</returns>
            public static String RsaEncrypt(String text, String pubKey, String modulus)
            {
                var rs = String.Empty;

                // 反转字符串
                text = ReverseText(text);
                // 转为HEX码
                var bt = Encoding.Default.GetBytes(text);
                text = BitConverter.ToString(bt).Replace("-", "");

                // 输入为16进制
                var t = BigInteger.Parse("0" + text, System.Globalization.NumberStyles.HexNumber);
                var p = BigInteger.Parse("0" + pubKey, System.Globalization.NumberStyles.HexNumber);
                var m = BigInteger.Parse("0" + modulus, System.Globalization.NumberStyles.HexNumber);

                // 幂取模，转为10进制
                rs = BigInteger.ModPow(t, p, m).ToString("x");

                // 使用后导零填充到特定的长度
                rs = ZeroFill(rs, 256);

                return rs;
            }

            /// <summary>
            /// 反转字符串
            /// </summary>
            /// <param name="str">字符串</param>
            /// <returns>反转后的字符串</returns>
            private static String ReverseText(String str)
            {
                var arr = str.ToCharArray();
                Array.Reverse(arr);
                return new String(arr);
            }

            /// <summary>
            /// 使用前导0填充字符串
            /// </summary>
            /// <param name="str">字符串</param>
            /// <param name="size">要填充到的长度</param>
            /// <returns>处理后的字符串</returns>
            private static String ZeroFill(String str, Int32 size)
            {
                //去掉前面的0
                while (str.StartsWith("0"))
                {
                    str = str.Substring(1, str.Length - 1);
                }
                //右填充0
                if (str.Length < size)
                {
                    str = str.PadRight(size, '0');
                }
                return str;
            }

        }

        #endregion
    }
}
