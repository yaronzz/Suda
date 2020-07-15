using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Music;
using Music.QQMusic;
using Stylet;
using Suda.Else;

namespace Suda.Pages
{
    public class LoginViewModel : Stylet.Screen
    {
        public MainViewModel VMMain;
        public WebBrowser CtrlWebBrowser;

        public void LoadWebBrowser()
        {
            if(CtrlWebBrowser == null)
                CtrlWebBrowser = ((LoginView)this.View).ctrlWebBrowser;
            CtrlWebBrowser.Navigate(Global.QQRecord.URL_LOGIN);
            CtrlWebBrowser.DocumentCompleted += DocumentCompleted;
        }

        private void DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (Global.QQRecord.LoginDocumentCompleted(CtrlWebBrowser.DocumentTitle, CtrlWebBrowser.Document.Cookie))
            {
                Cache obj = new Cache()
                {
                    QQCookie = Global.QQRecord.Cookie,
                    QQGTK = Global.QQRecord.GTK,
                    QQNumber = Global.QQRecord.QQNumber,
                };
                Cache.Write(obj);
                VMMain.LoginComplete();
            }
        }
    }
}
