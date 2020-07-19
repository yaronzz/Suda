using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Music.CloudMusic
{
    public class Record
    {

        /// <summary>
        /// 获取歌单信息
        /// </summary>
        /// <param name="sID">歌单ID</param>
        /// <returns></returns>
        public static async Task<Playlist> GetPlaylist(string sID = "5054214050")
        {
            string data = await GetWebAsync($"http://music.163.com/api/playlist/detail?id={sID}");
            
            return null;
        }
        //http://music.163.com/api/playlist/detail?id=5054214050
        

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
    }
}
