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
        public string Type { get; set; } = "(BETA)";
        public string Version { get; set; } = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        
        public void GotoGithub() => NetHelper.OpenWeb(Global.URL_SUDA_GITHUB);
        public void Feedback() => NetHelper.OpenWeb(Global.URL_SUDA_ISSUES);
        public void Telegram() => NetHelper.OpenWeb(Global.URL_SUDA_GROUP);
        public void ClickPaypal() => NetHelper.OpenWeb(Global.URL_PAYPAL);
        public void WindowClose() => Global.VMMain.HidePage(Global.VMMain.VMAbout);
    }
}
