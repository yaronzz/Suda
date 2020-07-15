using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Music;
using Music.QQMusic;
using Stylet;

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
            CtrlWebBrowser.Navigate(VMMain.Account.URL_LOGIN);
            CtrlWebBrowser.DocumentCompleted += DocumentCompleted;
        }

        private void DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (VMMain.Account.LoginDocumentCompleted(CtrlWebBrowser.DocumentTitle, CtrlWebBrowser.Document.Cookie))
            {
                Cache obj = new Cache();
                obj.QQCookie = VMMain.Account.Cookie;
                obj.QQGTK = VMMain.Account.GTK;
                obj.QQNumber = VMMain.Account.QQNumber;
                Cache.Write(obj);
            }
        }
    }
}
