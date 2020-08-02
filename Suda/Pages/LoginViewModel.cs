using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using AIGS.Common;
using CefSharp;
using CefSharp.Wpf;
using Stylet;
using Suda.Else;
using SudaLib;

namespace Suda.Pages
{
    public class LoginViewModel : Suda.Else.ModelBase
    {
        public Platform Plat { get; set; }
        public Visibility InputVisibility { get; set; } = Visibility.Hidden;
        public Visibility WebVisibility { get; set; } = Visibility.Hidden;

        public string Username { get; set; }
        public string Password { get; set; }
        public string Errlabel { get; set; }
        public bool   BtnLoginEnable { get; set; } = true;

        public Action<object> Action { get; set; }
        CollapsableChromiumWebBrowser Browser { get; set; }

        public void Load(Platform data, Action<object> action = null)
        {
            Plat = data;
            Action = action;
            ShowPage(Plat.Type);
            switch (Plat.Type)
            {
                case ePlatform.QQMusic:
                case ePlatform.Spotify:
                    //creat Browser
                    if (Browser == null)
                    {
                        Browser = new CollapsableChromiumWebBrowser();
                        ((LoginView)this.View).ctrlBrowerGrid.Children.Add(Browser);
                    }
                    LoadWebBrowser(Plat.Type);
                    break;
                case ePlatform.CloudMusic:
                case ePlatform.Tidal:
                    LoadInput();
                    break;
            }

        }

        public void ShowPage(ePlatform type)
        {
            WebVisibility = Visibility.Hidden;
            InputVisibility = Visibility.Hidden;

            if (type == ePlatform.CloudMusic ||
                type == ePlatform.Tidal)
                InputVisibility = Visibility.Visible;
            else
                WebVisibility = Visibility.Visible;
        }

        /// <summary>
        /// Callback wehn login success
        /// </summary>
        public void LoginSuccess()
        {
            if (Action != null)
                Action(null);

            Global.Cache.Save(Plat.LoginKey);

            //remove Browser (consume big memory)
            if (Browser != null)
            {
                ((LoginView)this.View).ctrlBrowerGrid.Children.Remove(Browser);
                Browser = null;
            }
        }

        #region Login by input username and password

        /// <summary>
        /// Init control
        /// </summary>
        public void LoadInput()
        {
            Username = null;
            Password = null;
            Errlabel = null;
            BtnLoginEnable = true;
        }

        /// <summary>
        /// Login
        /// </summary>
        public async void Login()
        {
            BtnLoginEnable = false;
            Errlabel = null;

            if(Username.IsBlank() || Password.IsBlank())
            {
                Errlabel = Language.Get("strmsgUsernamePasswordError");
                goto RETURN;
            }

            (string msg,object key) = await SudaLib.Method.GetLoginKey(Plat.Type, Username, Password);
            if (key != null)
            {
                Plat.LoginKey = key;
                LoginSuccess();
                goto RETURN;
            }

            Errlabel = Language.Get("strmsgLoginFailed") + " " + msg;

        RETURN:
            BtnLoginEnable = true;
        }

        #endregion

        #region Login by web

        /// <summary>
        /// Load url
        /// </summary>
        /// <param name="type"></param>
        async void LoadWebBrowser(ePlatform type)
        {
            //load
            if (type == ePlatform.QQMusic)
            {
                Browser.FrameLoadEnd += Browser_FrameLoadEnd;
                Browser.Address = QQMusic.GetLoginUrl();
            }
            else if (type == ePlatform.Spotify)
            {
                await Spotify.WorkBeforeLogin((x) => { TryLogin(x); });
                Browser.Address = Spotify.GetLoginUrl();
            }

            return;
        }

        public void TryLogin(object data)
        {
            (string html,string cookies) = ((string,string))data;
            (string msg, object key) = SudaLib.Method.GetLoginKey(Plat.Type, html, cookies).Result;
            if (key != null)
            {
                Plat.LoginKey = key;
                LoginSuccess();
            }
        }

        private async void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            CookieVisitor.Html = await Browser.GetSourceAsync();
            CookieVisitor visitor = new CookieVisitor();
            visitor.Action += TryLogin;
            Cef.GetGlobalCookieManager().VisitAllCookies(visitor);
            return;
        }

        public class CookieVisitor : ICookieVisitor
        {
            public static string Cookies = null;
            public static string Html = null;
            public event Action<object> Action;
            public bool Visit(CefSharp.Cookie cookie, int count, int total, ref bool deleteCookie)
            {
                if(count == 0)
                    Cookies = null;
                
                Cookies += cookie.Name + "=" + cookie.Value + ";";
                deleteCookie = false;
                return true;
            }

            public void Dispose() 
            {
                if (Action != null)
                    Action((Html, Cookies));
                return;
            }
        }

        #endregion
    }

    internal sealed class CollapsableChromiumWebBrowser : ChromiumWebBrowser
    {
        public CollapsableChromiumWebBrowser()
        {
            this.Loaded += this.BrowserLoaded;
        }

        private void BrowserLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Avoid loading CEF in designer
            if (DesignerProperties.GetIsInDesignMode(this))
                return;
            // Avoid NRE in AbstractRenderHandler.OnPaint
            ApplyTemplate();
            CreateOffscreenBrowser(new Size(400, 400));
        }
    }
}
