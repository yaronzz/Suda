using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Suda.Else
{
    public static class Global
    {
        public static Music.QQMusic.Record QQRecord { get; set; } = new Music.QQMusic.Record();
        public static Music.Cache MusicCache { get; set; } = Music.Cache.Read(true);

    }
}
