using AIGS.Helper;
using Suda.Else;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Suda.Pages
{
    public class AboutViewModel : Suda.Else.ModelBase
    {
#if DEBUG
        public string Type { get; set; } = "(DEBUG)";
#else
        public string Type { get; set; } = "(BETA)";
#endif
        public string Version { get; set; } = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public void GotoGithub() => NetHelper.OpenWeb("https://github.com/yaronzz/Suda");
        public void Feedback() => NetHelper.OpenWeb("https://github.com/yaronzz/Suda/issues");
        public void Telegram() => NetHelper.OpenWeb("https://t.me/suda_group");
        public void ClickPaypal() => NetHelper.OpenWeb("https://www.paypal.com/paypalme/yaronzz");
        public void ClickPatreon() => NetHelper.OpenWeb("https://www.patreon.com/yaronzz");
        public void CopyBTC() => SystemHelper.SetClipBoardData("1KQVpksXqXVNx2BM1q1bng5qwPKEBfMM2s");

        public void WindowClose()
        {
            Global.VMMain.HidePage(Global.VMMain.VMAbout);
        }
    }
}
