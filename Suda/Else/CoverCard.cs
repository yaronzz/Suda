using AIGS.Common;
using AIGS.Helper;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static AIGS.Helper.HttpHelper;

namespace Suda.Else
{
    public class CoverCard : Screen
    {
        public string ImgUrl { get; set; }
        public string Title { get; set; }
        public string Platform { get; set; }
        public string Url { get; set; }

        public static async Task<ObservableCollection<CoverCard>> GetList()
        {
            try
            {
                Result result = await HttpHelper.GetOrPostAsync("https://cdn.jsdelivr.net/gh/yaronzz/CDN/app/suda/todayplaylist.json");
                if(result.sData.IsNotBlank())
                {
                    ObservableCollection<CoverCard> pList = JsonHelper.ConverStringToObject<ObservableCollection<CoverCard>>(result.sData);
                    return pList;
                }
            }
            catch { }
            return GetDefaultList();
        }

        private static ObservableCollection<CoverCard> GetDefaultList()
        {
            CoverCard card1 = new CoverCard()
            {
                ImgUrl = "https://cdn.jsdelivr.net/gh/yaronzz/CDN/app/suda/1.jpg",
                Title = "Today's Top Hits",
                Platform = "Spotify",
                Url = "https://open.spotify.com/playlist/37i9dQZF1DXcBWIGoYBM5M",
            };
            CoverCard card2 = new CoverCard()
            {
                ImgUrl = "https://cdn.jsdelivr.net/gh/yaronzz/CDN/app/suda/2.jpg",
                Title = "This Is back number",
                Platform = "Spotify",
                Url = "https://open.spotify.com/playlist/37i9dQZF1DZ06evO3NSzPI",
            };
            CoverCard card3 = new CoverCard()
            {
                ImgUrl = "https://cdn.jsdelivr.net/gh/yaronzz/CDN/app/suda/3.jpg",
                Title = "This Is 陳奕迅",
                Platform = "Spotify",
                Url = "https://open.spotify.com/playlist/37i9dQZF1DZ06evO1Dy2vS",
            };
            ObservableCollection<CoverCard> pCards = new ObservableCollection<CoverCard>();
            pCards.Add(card1);
            pCards.Add(card2);
            pCards.Add(card3);

            string sjson = JsonHelper.ConverObjectToString<ObservableCollection<CoverCard>>(pCards);
            FileHelper.Write(sjson, true, "./covercards.json");

            return pCards;
        }
    }
}
