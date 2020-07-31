using AIGS.Common;
using HandyControl.Controls;
using HandyControl.Properties.Langs;
using Stylet;
using Suda.Else;
using SudaLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SudaLib.Common;

namespace Suda.Pages
{
    public class ImportViewModel : Suda.Else.ModelBase
    {
        public Playlist Playlist { get; set; }
        public bool ShowCards { get; set; } = true;
        public ObservableCollection<CoverCard> CoverCards { get; set; }

        protected override async void OnViewLoaded()
        {
            CoverCards = await CoverCard.GetList();
        }

        public async void Search(string text)
        {
            if(text.IsBlank())
            {
                Growl.Error(Language.Get("strmsgPleaseEnterTheLink"), Global.TOKEN_MAIN);
                return;
            }

            (ePlatform type,string id) = SudaLib.Method.GetPlaylistIDByUrl(text);
            if(type == ePlatform.None || id.IsBlank())
            {
                Growl.Error(Language.Get("strmsgUrlIsWrong"), Global.TOKEN_MAIN);
                return;
            }

            Platform plat = Global.VMMain.FindPlatforms(type);
            if (Global.VMMain.FindPlatforms(type) == null || plat.LoginKey == null)
            {
                Growl.Error(String.Format(Language.Get("strmsgPleaseLoginByFirst"), Platform.GetPlatformDisplayName(type)), Global.TOKEN_MAIN);
                return;
            }

            (string msg, Playlist plist) = await Method.GetPlaylist(plat.LoginKey, id);
            if(plist == null)
            {
                Growl.Error(Language.Get("strmsgGetPlaylistFailed") + " " + msg, Global.TOKEN_MAIN);
                return;
            }
            ShowCards = false;
            Playlist = plist;
        }

        public void GoLeft() => ShowCards = true;
        public void GoRight() => ShowCards = false;


        public void ImportTo()
        {
            if (Playlist == null)
                return;
            Global.VMMain.SudaPlaylistUpload(Playlist);
        }


        public void ToLocal()
        {
            if (Playlist == null)
                return;

            Global.VMMain.SudaPlaylistAdd(new ObservableCollection<Playlist>() { Playlist });
            return;
        }


       
    }
}
