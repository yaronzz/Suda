using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Suda.Pages
{
    public class ImportViewModel : Screen
    {
        public async void Search(string SearchStr)
        {
            await Music.CloudMusic.Record.GetPlaylist();
        }
    }
}
