using AIGS.Common;
using AIGS.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SudaLib
{
    public class QQMusic
    {
        #region 类

        //用户信息
        public class UserInfo
        {
            public string Nick { get; set; } //昵称
            public string Headpic { get; set; } //头像
            public string Uin { get; set; } //qq号
            public string Encrypt_uin { get; set; } //加密qq号
        }

        //我喜欢
        public class MyMusic
        {
            public string ID { get; set; } //歌单ID
            public string Picurl { get; set; } //封面
            public string Title { get; set; } //名称
        }

        //歌单简介
        public class PlaylistBrief
        {
            public string DissID { get; set; } //歌单ID
            public string DirID { get; set; } //内部ID
            public string Picurl { get; set; } //封面
            public string Title { get; set; } //名称
            public bool MyFavorite { get; set; }
    }

        //歌单
        public class Playlist
        {
            public string DisstID { get; set; } //歌单ID
            public string DirID { get; set; }  //内部ID
            public string DissName { get; set; } //名称
            public string Logo { get; set; } //封面
            public string Nick { get; set; } //创建者昵称
            public string Desc { get; set; } //描述
            public int SongNum { get; set; } //歌曲数量

            public List<Song> SongList { get; set; } = new List<Song>();
        }

        //歌曲
        public class Song
        {
            public string SongName { set; get; } //歌名
            public string SongID { set; get; } //歌曲ID
            public string SongMid { set; get; } //歌曲MID
            public int Interval { set; get; } //时长

            public string AlbumID { set; get; } //专辑ID
            public string AlbumName { set; get; } //专辑名

            public List<Singer> Singer { set; get; }
        }

        //歌手
        public class Singer
        {
            public string Name { set; get; }
            public string ID { set; get; }
            public string Mid { set; get; }
        }

        //专辑
        public class Album
        {
            public string Name { set; get; }
            public string Title { set; get; }
            public string ID { set; get; }
            public string Mid { set; get; }
        }

        //搜索项
        public class SearchItem
        {
            public string Title { set; get; } //歌曲名
            public string ID { set; get; }
            public string Mid { set; get; }
            public int Interval { set; get; } //时长
            public Album Album { set; get; }
            public List<Singer> Singer { set; get; }
        }

        //登录钥匙
        public class LoginKey
        {
            public string Cookie { get; set; }
            public string QQNumber { get; set; }
            public string GTK { get; set; }
        }

        #endregion


        #region 登录

        /// <summary>
        /// 获取登录链接
        /// </summary>
        /// <returns></returns>
        public static string GetLoginUrl()
        {
            string Url = "https://xui.ptlogin2.qq.com/cgi-bin/xlogin?daid=384&pt_no_auth=1&style=40&hide_border=1&appid=1006102" +
                "&s_url=https%3A%2F%2Fy.qq.com%2Fn%2Fyqq%2Fsong%2F000edOaL1WZOWq.html%23stat%3Dy_new.top.pop.logout" +
                "&low_login=1&hln_css=&hln_title=&hln_acc=&hln_pwd=&hln_u_tips=&hln_p_tips=&hln_autologin=&hln_login=" +
                "&hln_otheracc=&hide_close_icon=1&hln_qloginacc=&hln_reg=&hln_vctitle=&hln_verifycode=&hln_vclogin=&hln_feedback=";
            return Url;
        }

        /// <summary>
        /// 获取登录Key
        /// </summary>
        /// <param name="sDocumentTitle"></param>
        /// <param name="sDocumentCookie"></param>
        /// <returns></returns>
        public static (string,LoginKey) GetLoginKey(string sDocumentTitle, string sDocumentCookie)
        {
            if (sDocumentTitle.Contains("QQ音乐"))
            {
                LoginKey pRet = new LoginKey();
                pRet.Cookie = sDocumentCookie;
                pRet.QQNumber = StringHelper.GetSubString(sDocumentCookie, "p_luin=o", ";");
                if (sDocumentCookie.Contains("p_skey="))
                {
                    string p_skey = StringHelper.GetSubString(sDocumentCookie + ";", "p_skey=", ";");
                    long hash = 5381;
                    for (int i = 0; i < p_skey.Length; i++)
                        hash += (hash << 5) + p_skey[i];
                    long g_tk = hash & 0x7fffffff;
                    pRet.GTK = g_tk.ToString();
                }
                return (null, pRet);
            }
            return (null, null);
        }

        public static async Task<(string, bool)> IsValidLoginKey(LoginKey oKey)
        {
            (string msg, Common.UserInfo info) = await GetUserInfo(oKey);
            if (info == null)
                return (msg,false);
            return (null,true);
        }
        #endregion



        #region 用户信息
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        public static async Task<(string, Common.UserInfo)> GetUserInfo(LoginKey oKey, List<PlaylistBrief> briefs = null)
        {
            string url = $"https://c.y.qq.com/rsc/fcgi-bin/fcg_get_profile_homepage.fcg?" +
                $"loginUin={oKey.QQNumber}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8" +
                $"&notice=0&platform=yqq&needNewCode=0&cid=205360838&ct=20&userid={oKey.QQNumber}" +
                $"&reqfrom=1&reqtype=0";

            var ret = await GetWebDatacAsync(url, oKey.Cookie, Encoding.UTF8);
            UserInfo obj = JsonHelper.ConverStringToObject<UserInfo>(ret, "data", "creator");
            if (obj == null)
                return ("Get user info failed!",null);

            Common.UserInfo user = new Common.UserInfo();
            user.NickName = obj.Nick;
            user.UserID = obj.Uin;
            user.AvatarUrl = obj.Headpic;

            if (briefs != null)
            {
                //我喜欢
                List<MyMusic> pMymusic = JsonHelper.ConverStringToObject<List<MyMusic>>(ret, "data", "mymusic");
                foreach (var item in pMymusic)
                {
                    if (item.ID != "0")
                    {
                        briefs.Add(new PlaylistBrief()
                        {
                            DissID = item.ID,
                            Title = Method.GetFavoritePlaylistName(),
                            Picurl = item.Picurl,
                            MyFavorite = true
                        });
                        break;
                    }
                }

                //创建的歌单
                List<PlaylistBrief> plist = JsonHelper.ConverStringToObject<List<PlaylistBrief>>(ret, "data", "mydiss", "list");
                foreach (var item in plist)
                {
                    briefs.Add(item);
                }
            }
            return (null, user);
        }

        /// <summary>
        /// 获取用户歌单列表
        /// </summary>
        /// <param name="oKey"></param>
        /// <returns></returns>
        public static async Task<(string, ObservableCollection<Common.Playlist>)> GetUserPlaylist(LoginKey oKey)
        {
            List<PlaylistBrief> briefs = new List<PlaylistBrief>();
            (string msg, Common.UserInfo user) = await GetUserInfo(oKey, briefs);
            if (user == null)
                return (msg,null);

            ObservableCollection<Common.Playlist> plists = new ObservableCollection<Common.Playlist>();
            for (int i = 0; i < briefs.Count(); i++)
            {
                (string msg2, Common.Playlist item) = await GetPlaylist(oKey, briefs[i].DissID);
                if (item != null)
                {
                    if(briefs[i].MyFavorite)
                    {
                        item.Title = Method.GetFavoritePlaylistName();
                        item.MyFavorite = true;
                    }
                    plists.Add(item);
                }
            }
            return (null,plists);
        }
        #endregion

        #region 歌单

        /// <summary>
        /// 获取歌单详情
        /// </summary>
        /// <param name="oKey"></param>
        /// <param name="sID"></param>
        /// <returns></returns>
        public static async Task<(string, Common.Playlist)> GetPlaylist(LoginKey oKey, string sID = "7516711891")
        {
            string url = $"https://c.y.qq.com/qzone/fcg-bin/fcg_ucc_getcdinfo_byids_cp.fcg?" +
                $"type=1&json=1&utf8=1&onlysong=0&disstid={sID}&format=json&g_tk={oKey.GTK}" +
                $"&loginUin={oKey.QQNumber}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8" +
                $"&notice=0&platform=yqq&needNewCode=0";

            var ret = await GetWebDatacAsync(url, oKey.Cookie, Encoding.UTF8);
            List<Playlist> obj = JsonHelper.ConverStringToObject<List<Playlist>>(ret, "cdlist");
            if (obj != null && obj.Count > 0)
                return (null,QQMusicPlaylistConvert(obj[0]));
            return ("Get playlist failed!", null);
        }

        /// <summary>
        /// 新建歌单
        /// </summary>
        /// <param name="sPlaylistName"></param>
        /// <param name="sPlaylistDesc"></param>
        /// <returns></returns>
        public static async Task<(string, Common.Playlist)> CreatPlaylist(LoginKey oKey, string sPlaylistName, string sPlaylistDesc)
        {
            string url = "https://c.y.qq.com/splcloud/fcgi-bin/create_playlist.fcg?g_tk=" + oKey.GTK;
            string data = $"loginUin={oKey.QQNumber}&hostUin=0&format=json&inCharset=utf8&outCharset=utf8&notice=0&platform=yqq&needNewCode=0" +
                $"&g_tk={oKey.GTK}&uin={oKey.QQNumber}&name={sPlaylistName}&description=&show=1" +
                $"&pic_url=&tags=&tagids=&formsender=1&utf8=1&qzreferrer=https%3A%2F%2Fy.qq.com%2Fportal%2Fprofile.html%23sub%3Dother%26tab%3Dcreate%26";
            string result = await PostWeb(url, data, GetWebHeader_YQQCOM(oKey.Cookie));
            if (JsonHelper.GetValue(result, "code") == "0")
            {
                List<PlaylistBrief> briefs = new List<PlaylistBrief>();
                (string msg, Common.UserInfo info) = await GetUserInfo(oKey, briefs);
                foreach (var item in briefs)
                {
                    if (item.Title != sPlaylistName)
                        continue;
                    return await GetPlaylist(oKey, item.DissID);
                }
            }

            string msg2 = JsonHelper.GetValue(result, "msg");
            return (msg2, null);
        }

        public static async Task<(string, bool)> DeletePlaylist(LoginKey oKey, string sPlaylistDirID)
        {
            string url = "https://c.y.qq.com/splcloud/fcgi-bin/fcg_fav_modsongdir.fcg?g_tk=" + oKey.GTK;
            string data = $"loginUin={oKey.QQNumber}&hostUin=0&format=fs&inCharset=GB2312&outCharset=gb2312&notice=0&platform=yqq&needNewCode=0" +
                $"&g_tk={oKey.GTK}&uin={oKey.QQNumber}&delnum=1&deldirids={sPlaylistDirID}&forcedel=1&formsender=1&source=103";
            string result = await PostWeb(url, data, GetWebHeader_YQQCOM(oKey.Cookie));
            string code = StringHelper.GetSubString(result, "\"code\":", ",");
            if (code == "0")
                return (null, true);
            return ("Unknow", false);
        }

        /// <summary>
        /// 批量往歌单中添加歌曲
        /// </summary>
        /// <param name="sSongMidArray">歌曲MID集合</param>
        /// <param name="sPlaylistDirID">歌单DirID</param>
        /// <returns></returns>
        public static async Task<(string, bool)> AddSongToPlaylist(LoginKey oKey, string[] sSongMidArray, string sPlaylistDirID)
        {
            string sSongMidList = String.Join(",", sSongMidArray);
            string sTypeList = "";
            for (int i = 0; i < sSongMidArray.Count(); i++)
                sTypeList += ",13";
            sTypeList = sTypeList.Substring(1);

            string url = $"https://c.y.qq.com/splcloud/fcgi-bin/fcg_music_add2songdir.fcg?g_tk={oKey.GTK}";
            string data = $"loginUin={oKey.QQNumber}&hostUin=0&format=json&inCharset=utf8" +
                $"&outCharset=utf-8&notice=0&platform=yqq.post&needNewCode=0&uin={oKey.QQNumber}" +
                $"&midlist={sSongMidList}&typelist={sTypeList}&dirid={sPlaylistDirID}&addtype=" +
                $"&formsender=4&source=153&r2=0&r3=1&utf8=1";

            string result = await PostWeb(url, data, GetWebHeader_YQQCOM(oKey.Cookie));
            if (JsonHelper.GetValue(result, "code") == "0")
                return (null,true);

            string msg = JsonHelper.GetValue(result, "msg");
            return (msg,false);
        }

        /// <summary>
        /// 批量删除歌单中歌曲
        /// </summary>
        /// <param name="sSongMidArray"></param>
        /// <param name="sPlaylistDirID"></param>
        /// <returns></returns>
        public static async Task<(string, bool)> DelSongFromPlaylist(LoginKey oKey, string[] sSongMidArray, string sPlaylistDirID)
        {
            string sSongMidList = String.Join(",", sSongMidArray);
            string sTypeList = "";
            for (int i = 0; i < sSongMidArray.Count(); i++)
                sTypeList += ",3";
            sTypeList = sTypeList.Substring(1);

            string url = "https://c.y.qq.com/qzone/fcg-bin/fcg_music_delbatchsong.fcg?g_tk=" + oKey.GTK;
            string data = $"loginUin={oKey.QQNumber}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8" +
                $"&notice=0&platform=yqq.post&needNewCode=0&uin={oKey.QQNumber}&dirid={sPlaylistDirID}" +
                $"&ids={sSongMidList}&source=103&types={sTypeList}&formsender=4&flag=2&from=3&utf8=1";

            string result = await PostWeb(url, data, GetWebHeader_YQQCOM(oKey.Cookie));
            if (JsonHelper.GetValue(result, "code") == "0")
                return (null,true);

            string msg = JsonHelper.GetValue(result, "msg");
            return (msg, false);
        }

        
        #endregion



        #region 搜索

        public static async Task<ObservableCollection<Common.Track>> Search(LoginKey oKey, string sSearchText)
        {
            try
            {
                string url = $"http://59.37.96.220/soso/fcgi-bin/client_search_cp?format=json&t=0" +
                    $"&outCharset=utf-8&qqmusic_ver=1302&catZhida=0&p={1}&n=20&w={ sSearchText.UrlEncode()}" +
                    $"&flag_qc=0&remoteplace=sizer.newclient.song&new_json=1&lossless=0&aggr=1&cr=1&sem=0&force_zonghe=0";
                AIGS.Helper.HttpHelper.Result result = await HttpHelper.GetOrPostAsync(url);
                if (JsonHelper.GetValue(result.sData, "code") == "0")
                {
                    ObservableCollection<SearchItem> items = JsonHelper.ConverStringToObject<ObservableCollection<SearchItem>>(result.sData, "data", "song", "list");
                    ObservableCollection<Common.Track> ret = new ObservableCollection<Common.Track>();
                    foreach (var item in items)
                    {
                        ret.Add(QQMusicSearchItemConvert(item));
                    }
                    return ret;
                }
                return null;
            }
            catch(Exception e)
            {
                string msg = e.Message;
                return null;
            }
        }

        #endregion



        #region 网络
        static async Task<string> GetWebDatacAsync(string url,string cookie, Encoding c = null)
        {
            if (c == null) c = Encoding.UTF8;
            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(url);
            hwr.KeepAlive = true;
            hwr.Headers.Add(HttpRequestHeader.CacheControl, "max-age=0");
            hwr.Headers.Add(HttpRequestHeader.Upgrade, "1");
            hwr.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.110 Safari/537.36";
            hwr.Accept = "*/*";
            hwr.Referer = "https://y.qq.com/portal/player.html";
            hwr.Host = "c.y.qq.com";
            hwr.Headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.8");
            hwr.Headers.Add(HttpRequestHeader.Cookie, cookie);
            WebResponse o = await hwr.GetResponseAsync();
            StreamReader sr = new StreamReader(o.GetResponseStream(), c);
            var st = await sr.ReadToEndAsync();
            sr.Dispose();
            return st;
        }

        public static async Task<string> GetWebAsync(string url, Encoding e = null)
        {
            if (e == null)
                e = Encoding.UTF8;
            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(url);
            WebResponse res = await hwr.GetResponseAsync();
            StreamReader sr = new StreamReader(res.GetResponseStream(), e);
            var st = await sr.ReadToEndAsync();
            return st;
        }

        static async Task<string> PostWeb(string url, string data, WebHeaderCollection Header = null)
        {
            byte[] postData = Encoding.UTF8.GetBytes(data);
            WebClient webClient = new WebClient();
            if (Header != null)
                webClient.Headers = Header;
            byte[] responseData = await webClient.UploadDataTaskAsync(new Uri(url), "POST", postData);

            return Encoding.UTF8.GetString(responseData);
        }

        static WebHeaderCollection GetWebHeader_YQQCOM(string cookie) => new WebHeaderCollection
        {
            { HttpRequestHeader.Accept, "*/*" },
            { HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9" },
            { HttpRequestHeader.ContentType, "application/x-www-form-urlencoded; charset=UTF-8" },
            { HttpRequestHeader.Cookie, cookie },
            { HttpRequestHeader.Referer, "https://y.qq.com/n/yqq/singer/0020PeOh4ZaCw1.html" },
            { HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36" },
            { HttpRequestHeader.Host, "c.y.qq.com" }
        };
        #endregion



        #region ConvertToCommon



        static Common.Playlist QQMusicPlaylistConvert(QQMusic.Playlist data)
        {
            Common.Playlist ret = new Common.Playlist();
            ret.Title = data.DissName;
            ret.ID = data.DisstID;
            ret.MID = data.DirID;
            ret.MidArray.QQMusic = data.DirID;
            ret.ImgUrl = Method.FormatImgUrl(data.Logo);
            ret.Desc = data.Desc;
            ret.CreatorName = data.Nick;
            ret.CreatorID = null;
            ret.Tracks = new ObservableCollection<Common.Track>();

            for (int i = 0; i < data.SongList.Count(); i++)
            {
                Common.Track track = new Common.Track();
                track.Title = data.SongList[i].SongName;
                track.ID = data.SongList[i].SongID;
                track.MID = data.SongList[i].SongMid;
                track.MidArray.QQMusic = data.SongList[i].SongMid;
                track.Duration = data.SongList[i].Interval;
                track.DurationStr = AIGS.Helper.TimeHelper.ConverIntToString(data.SongList[i].Interval);
                track.AlbumID = data.SongList[i].AlbumID;
                track.AlbumTitle = data.SongList[i].AlbumName;
                track.Artists = new ObservableCollection<Common.Artist>();

                for (int j = 0; j < data.SongList[i].Singer.Count; j++)
                {
                    Common.Artist artist = new Common.Artist();
                    artist.Name = data.SongList[i].Singer[j].Name;
                    artist.ID = data.SongList[i].Singer[j].ID;
                    artist.MID = data.SongList[i].Singer[j].Mid;
                    track.Artists.Add(artist);

                }
                track.ArtistsName = Method.MergeArtistsName(track.Artists);
                (track.Live, track.TitleBrief) = Method.RemoveTilteLiveFlag(track.Title);
                ret.Tracks.Add(track);
            }
            return ret;
        }

        static Common.Track QQMusicSearchItemConvert(QQMusic.SearchItem data)
        {
            Common.Track track = new Common.Track();
            track.Title = data.Title;
            track.ID = data.ID;
            track.MID = data.Mid;
            track.MidArray.QQMusic = data.Mid;
            track.Duration = data.Interval;
            track.DurationStr = AIGS.Helper.TimeHelper.ConverIntToString(data.Interval);
            track.AlbumID = data.Album.ID;
            track.AlbumTitle = data.Album.Title;
            track.Artists = new ObservableCollection<Common.Artist>();

            for (int j = 0; j < data.Singer.Count; j++)
            {
                Common.Artist artist = new Common.Artist();
                artist.Name = data.Singer[j].Name;
                artist.ID = data.Singer[j].ID;
                artist.MID = data.Singer[j].Mid;
                track.Artists.Add(artist);
            }

            track.ArtistsName = Method.MergeArtistsName(track.Artists);
            (track.Live, track.TitleBrief) = Method.RemoveTilteLiveFlag(track.Title);
            return track;
        }
        #endregion
    }
}
