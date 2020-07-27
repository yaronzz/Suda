using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using AIGS.Common;
using Stylet;
using Suda.Else;
using SudaLib;

namespace Suda.Pages
{
    public class LoginViewModel : Stylet.Screen
    {
        public Platform Plat { get; set; }
        public Visibility InputVisibility { get; set; } = Visibility.Hidden;
        public Visibility WebVisibility { get; set; } = Visibility.Hidden;

        public string Username { get; set; }
        public string Password { get; set; }
        public string Errlabel { get; set; }
        public bool   BtnLoginEnable { get; set; } = true;

        public Action<object> Action { get; set; }

        public void Load(Platform data, Action<object> action = null)
        {
            Plat = data;
            Action = action;

            switch(Plat.Type)
            {
                case ePlatform.QQMusic:
                    LoadWebBrowser(Plat.Type);
                    break;
                case ePlatform.CloudMusic:
                case ePlatform.Tidal:
                    LoadInput();
                    break;
            }

            ShowPage(Plat.Type);
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
        public async Task LoginSuccess()
        {
            string msg, msg1;
            (msg, Plat.UserInfo) = await SudaLib.Method.GetUserInfo(Plat.LoginKey);
            (msg1, Plat.Playlists) = await SudaLib.Method.GetUserPlaylists(Plat.LoginKey);

            if(Plat.Type == ePlatform.CloudMusic)
                Global.Cache.CloudLoginkey = (CloudMusic.LoginKey)Plat.LoginKey;
            else if (Plat.Type == ePlatform.QQMusic)
                Global.Cache.QQLoginkey = (QQMusic.LoginKey)Plat.LoginKey;
            else if (Plat.Type == ePlatform.Tidal)
                Global.Cache.TidalLoginkey = (Tidal.LoginKey)Plat.LoginKey;

            Global.Cache.Write();
            if (Action != null)
                Action(null);
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
                Errlabel = "Username or password is error!";
                goto RETURN;
            }

            (string msg,object key) = await SudaLib.Method.GetLoginKey(Plat.Type, Username, Password);
            if (key != null)
            {
                Plat.LoginKey = key;
                await LoginSuccess();
                goto RETURN;
            }

            Errlabel = "Login err! " + msg;

        RETURN:
            BtnLoginEnable = true;
        }

        #endregion

        #region Login by web

        /// <summary>
        /// Load url
        /// </summary>
        /// <param name="type"></param>
        void LoadWebBrowser(ePlatform type)
        {
            WebBrowser CtrlWebBrowser = ((LoginView)this.View).ctrlWebBrowser;
            if(type == ePlatform.QQMusic)
                CtrlWebBrowser.Navigate(QQMusic.GetLoginUrl());

            CtrlWebBrowser.DocumentCompleted += DocumentCompleted;
        }

        /// <summary>
        /// Web Document Completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            WebBrowser CtrlWebBrowser = (WebBrowser)sender;
            (string msg, object key) = SudaLib.Method.GetLoginKey(Plat.Type, CtrlWebBrowser.DocumentTitle, CtrlWebBrowser.Document.Cookie).Result;
            if(key != null)
            {
                Plat.LoginKey = key;
                LoginSuccess();
            }
        }
        #endregion
    }
}
