using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music.QQMusic
{
    //用户信息
    public class User
    {
        public string Nick { get; set; } //昵称
        public string Headpic { get; set; } //头像
        public string Uin { get; set; } //qq号
        public List<Diss> DissList { get; set; } //歌单列表
    }

    //歌单简介
    public class Diss
    {
        public string DissID { get; set; } //歌单ID
        public string DirID { get; set; } //歌单ID
        public string Picurl { get; set; } //歌单封面
        public string Title { get; set; } //歌单名称
    }

    //歌单
    public class Playlist
    {
        public string DisstID { get; set; } //歌单ID
        public string DissName { get; set; } //歌单名称
        public string DirID { get; set; } 
        public string Logo { get; set; } //歌单封面
        public string Nick { get; set; } //创建者昵称
        public int SongNum { get; set; } //歌曲数量

        public List<Song> SongList { get; set; } = new List<Song>();
    }

    //歌曲
    public class Song
    {
        public string SongName { set; get; } //歌名
        public string SongID { set; get; } //歌曲ID
        public string SongMid { set; get; } //歌曲MID

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

    public class SearchItem
    {
        public string Name { set; get; } //歌曲名
        public string Mid { set; get; } 
        public string Time_public { set; get; }
        public Album Album { set; get; }
        public List<Singer> Singer { set; get; }

    }

}
