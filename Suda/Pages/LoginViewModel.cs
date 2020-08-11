using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using AIGS.Common;
using AIGS.Helper;
//using CefSharp;
//using CefSharp.Wpf;
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
        public Visibility PCWebVisibility { get; set; } = Visibility.Hidden;

        public string Username { get; set; }
        public string Password { get; set; }
        public string Errlabel { get; set; }
        public bool   BtnLoginEnable { get; set; } = true;

        public string WebUrl { get; set; }

        public Action<object> Action { get; set; }
        WebBrowser Browser { get; set; }

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
                        Browser = ((LoginView)this.View).Browser;
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
            PCWebVisibility = Visibility.Hidden;
            WebVisibility = Visibility.Hidden;
            InputVisibility = Visibility.Hidden;

            if (type == ePlatform.CloudMusic || type == ePlatform.Tidal)
                InputVisibility = Visibility.Visible;
            else if (type == ePlatform.QQMusic)
                WebVisibility = Visibility.Visible;
            else
                PCWebVisibility = Visibility.Visible;
        }

        /// <summary>
        /// Callback wehn login success
        /// </summary>
        public void LoginSuccess()
        {
            if (Action != null)
                Action(null);

            Global.Cache.Save(Plat.LoginKey);
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
                WebUrl = QQMusic.GetLoginUrl();
                Browser.DocumentCompleted += Browser_DocumentCompleted;
                Browser.Navigate(WebUrl);
            }
            else if (type == ePlatform.Spotify)
            {
                WebUrl = Spotify.GetLoginUrl();
                await Spotify.WorkBeforeLogin((x) => { TryLogin(x); });
            }

            return;
        }

        public void OpenWeb()
        {
            NetHelper.OpenWeb(WebUrl);
        }

        private void Browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            string html = Browser.DocumentText;
            string cookie = Browser.Document.Cookie;
            TryLogin((html, cookie));
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

        #endregion
    }


}
