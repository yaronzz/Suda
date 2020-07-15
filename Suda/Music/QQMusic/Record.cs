using AIGS.Common;
using AIGS.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Music.QQMusic
{
    public class Record
    {
        public string QQNumber = null;
        public string GTK = null;
        public string Cookie = null;
        public User User = null;

        #region 登录
        /// <summary>
        /// 登录URL
        /// </summary>
        public string URL_LOGIN = "https://xui.ptlogin2.qq.com/cgi-bin/xlogin?daid=384&pt_no_auth=1&style=40&hide_border=1&appid=1006102&s_url=https%3A%2F%2Fy.qq.com%2Fn%2Fyqq%2Fsong%2F000edOaL1WZOWq.html%23stat%3Dy_new.top.pop.logout&low_login=1&hln_css=&hln_title=&hln_acc=&hln_pwd=&hln_u_tips=&hln_p_tips=&hln_autologin=&hln_login=&hln_otheracc=&hide_close_icon=1&hln_qloginacc=&hln_reg=&hln_vctitle=&hln_verifycode=&hln_vclogin=&hln_feedback=";
        
        /// <summary>
        /// 登录成功回调接口
        /// </summary>
        /// <param name="sDocumentTitle">文档名</param>
        /// <param name="sDocumentCookie">文档Cookie</param>
        /// <returns></returns>
        public bool LoginDocumentCompleted(string sDocumentTitle, string sDocumentCookie)
        {
            if (sDocumentTitle.Contains("QQ音乐"))
            {
                Cookie = sDocumentCookie;
                QQNumber = StringHelper.GetSubString(sDocumentCookie, "p_luin=o", ";");
                if (sDocumentCookie.Contains("p_skey="))
                {
                    string p_skey = StringHelper.GetSubString(sDocumentCookie + ";", "p_skey=", ";");
                    long hash = 5381;
                    for (int i = 0; i < p_skey.Length; i++)
                        hash += (hash << 5) + p_skey[i];
                    long g_tk = hash & 0x7fffffff;
                    GTK = g_tk.ToString();
                }
                return true;
            }
            return false;
        }
        #endregion


        #region 用户信息
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        public async Task<bool> GetUser()
        {
            if (QQNumber.IsBlank() || Cookie.IsBlank() || GTK.IsBlank())
                return false;

            var ret = await GetWebDatacAsync($"https://c.y.qq.com/rsc/fcgi-bin/fcg_get_profile_homepage.fcg?loginUin={QQNumber}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&cid=205360838&ct=20&userid={QQNumber}&reqfrom=1&reqtype=0", Encoding.UTF8);
            User obj = JsonHelper.ConverStringToObject<User>(ret, "data", "creator");
            if (obj == null)
                return false;

            obj.DissList = JsonHelper.ConverStringToObject<List<Diss>>(ret, "data", "mydiss", "list");
            obj.Playlists = new List<Playlist>();
            for (int i = 0; i < obj.DissList.Count(); i++)
            {
                var item = await GetPlaylist(obj.DissList[i].DissID);
                if (item != null)
                    obj.Playlists.Add(item);
            }

            User = obj;
            return true;
        }
        #endregion


        #region 歌单
        /// <summary>
        /// 获取歌单信息
        /// </summary>
        /// <param name="sID">歌单ID</param>
        /// <returns></returns>
        public async Task<Playlist> GetPlaylist(string sID = "7516711891")
        {
            if (QQNumber.IsBlank() || Cookie.IsBlank() || GTK.IsBlank())
                return null;

            var ret = await GetWebDatacAsync($"https://c.y.qq.com/qzone/fcg-bin/fcg_ucc_getcdinfo_byids_cp.fcg?type=1&json=1&utf8=1&onlysong=0&disstid={sID}&format=json&g_tk={GTK}&loginUin={QQNumber}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0", Encoding.UTF8);
            List<Playlist> obj = JsonHelper.ConverStringToObject<List<Playlist>>(ret, "cdlist");
            if(obj != null && obj.Count > 0)
                return obj[0];
            return null;
        }

        /// <summary>
        /// 往歌单中添加歌曲
        /// </summary>
        /// <param name="sSongMid">歌曲MID</param>
        /// <param name="sPlaylistDirID">歌单DirID</param>
        /// <returns></returns>
        public async Task<bool> AddSongToPlaylist(string sSongMid, string sPlaylistDirID)
        {
            string result = await PostWeb("https://c.y.qq.com/splcloud/fcgi-bin/fcg_music_add2songdir.fcg?g_tk=" + GTK,
               $"loginUin={QQNumber}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.post&needNewCode=0" +
               $"&uin={QQNumber}&midlist={sSongMid}&typelist=13&dirid={sPlaylistDirID}&addtype=&formsender=4&source=153&r2=0&r3=1&utf8=1", GetWebHeader_YQQCOM());
            if (JsonHelper.GetValue(result, "code") == "0")
                return true;
            return false;
        }

        /// <summary>
        /// 批量往歌单中添加歌曲
        /// </summary>
        /// <param name="sSongMidArray">歌曲MID集合</param>
        /// <param name="sPlaylistDirID">歌单DirID</param>
        /// <returns></returns>
        public async Task<bool> AddSongToPlaylist(string[] sSongMidArray, string sPlaylistDirID)
        {
            string sSongMidList = String.Join(",", sSongMidArray);
            string sTypeList = "";
            for (int i = 0; i < sSongMidArray.Count(); i++)
                sTypeList += ",13";
            sTypeList = sTypeList.Substring(1);

            string result = await PostWeb("https://c.y.qq.com/splcloud/fcgi-bin/fcg_music_add2songdir.fcg?g_tk=" + GTK,
               $"loginUin={QQNumber}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.post&needNewCode=0" +
               $"&uin={QQNumber}&midlist={sSongMidList}&typelist={sTypeList}&dirid={sPlaylistDirID}&addtype=&formsender=4&source=153&r2=0&r3=1&utf8=1", GetWebHeader_YQQCOM());
            if (JsonHelper.GetValue(result, "code") == "0")
                return true;
            return false;
        }

        /// <summary>
        /// 批量删除歌单中歌曲
        /// </summary>
        /// <param name="sSongMidArray"></param>
        /// <param name="sPlaylistDirID"></param>
        /// <returns></returns>
        public async Task<bool> DelSongFromPlaylist(string[] sSongMidArray, string sPlaylistDirID)
        {
            string sSongMidList = String.Join(",", sSongMidArray);
            string sTypeList = "";
            for (int i = 0; i < sSongMidArray.Count(); i++)
                sTypeList += ",3";
            sTypeList = sTypeList.Substring(1);
            
            string result = await PostWeb("https://c.y.qq.com/qzone/fcg-bin/fcg_music_delbatchsong.fcg?g_tk=" + GTK,
                $"loginUin={QQNumber}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.post&needNewCode=0&uin={QQNumber}&dirid={sPlaylistDirID}" +
                $"&ids={sSongMidList}&source=103&types={sTypeList}&formsender=4&flag=2&from=3&utf8=1", GetWebHeader_YQQCOM());
            if (JsonHelper.GetValue(result, "code") == "0")
                return true;
            return false;
        }
        #endregion

        #region 搜索

        public static async Task<SearchItem> Search(string sSongTitle, string sSingerName, string sAlbumName = null)
        {
            bool bLive = Music.Method.IsLive(sSongTitle);
            if(bLive)
                sSongTitle = Music.Method.RemoveLiveFlag(sSongTitle);

            string sSearchContent = sSongTitle + '-' + sSingerName;
            string result = await GetWebAsync($"http://59.37.96.220/soso/fcgi-bin/client_search_cp?format=json&t=0&inCharset=GB2312&outCharset=utf-8" +
                $"&qqmusic_ver=1302&catZhida=0&p={1}&n=20&w={HttpUtility.UrlDecode(sSearchContent)}&flag_qc=0&remoteplace=sizer.newclient.song&new_json=1" +
                $"&lossless=0&aggr=1&cr=1&sem=0&force_zonghe=0");

            List<SearchItem> pList = JsonHelper.ConverStringToObject<List<SearchItem>>(result, "data", "song", "list");
            for (int i = 0; i < pList.Count(); i++)
            {
                if (sAlbumName.IsNotBlank() && sAlbumName != pList[i].Album.Name)
                    continue;
                if (sSingerName != pList[i].Singer[0].Name)
                    continue;

                string sItemName = pList[i].Name;
                bool bItemLive = Music.Method.IsLive(pList[i].Name);
                if (bItemLive)
                    sItemName = Music.Method.RemoveLiveFlag(pList[i].Name);

                if(bItemLive != bLive)
                    continue;
                if (sSongTitle != sItemName)
                    continue;
                return pList[i];
            }
            return null;
        }

        #endregion



        #region 其他
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

        public async Task<string> GetWebDatacAsync(string url, Encoding c = null)
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
            hwr.Headers.Add(HttpRequestHeader.Cookie, Cookie);
            WebResponse o = await hwr.GetResponseAsync();
            StreamReader sr = new StreamReader(o.GetResponseStream(), c);
            var st = await sr.ReadToEndAsync();
            sr.Dispose();
            return st;
        }

        public static async Task<string> PostWeb(string url, string data, WebHeaderCollection Header = null)
        {
            byte[] postData = Encoding.UTF8.GetBytes(data);
            WebClient webClient = new WebClient();
            if (Header != null)
                webClient.Headers = Header;
            byte[] responseData = await webClient.UploadDataTaskAsync(new Uri(url), "POST", postData);

            return Encoding.UTF8.GetString(responseData);
        }

        public WebHeaderCollection GetWebHeader_YQQCOM() => new WebHeaderCollection
        {
            { HttpRequestHeader.Accept, "*/*" },
            { HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9" },
            { HttpRequestHeader.ContentType, "application/x-www-form-urlencoded; charset=UTF-8" },
            { HttpRequestHeader.Cookie, Cookie },
            { HttpRequestHeader.Referer, "https://y.qq.com/n/yqq/singer/0020PeOh4ZaCw1.html" },
            { HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36" },
            { HttpRequestHeader.Host, "c.y.qq.com" }
        };
        #endregion
    }
}
