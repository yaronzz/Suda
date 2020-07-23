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
        public string PlatformTitle { get; set; }

        public Visibility ViewVisibility { get; set; } = Visibility.Hidden;
        public Visibility InputVisibility { get; set; } = Visibility.Hidden;
        public Visibility WebVisibility { get; set; } = Visibility.Hidden;

        public string Username { get; set; }
        public string Password { get; set; }
        public string Errlabel { get; set; }
        public bool BtnLoginEnable { get; set; } = true;

        public void Load(Platform data)
        {
            Plat = data;
            PlatformTitle = AIGS.Common.Convert.ConverEnumToString((int)Plat.Type, typeof(ePlatform));
            if (Plat.Type == ePlatform.QQMusic)
            {
                LoadWebBrowser(Plat.Type);
                ShowPage(false);
            }
            else if (Plat.Type == ePlatform.CloudMusic)
            {
                LoadInput();
                ShowPage(true);
            }
            ViewVisibility = Visibility.Visible;
        }

        /// <summary>
        /// Callback wehn login success
        /// </summary>
        public async void LoginSuccess()
        {
            Plat.UserInfo = await SudaLib.Method.GetUserInfo(Plat.LoginKey);
            Plat.Playlists = await SudaLib.Method.GetUserPlaylists(Plat.LoginKey);
            ViewVisibility = Visibility.Hidden;

            if(Plat.Type == ePlatform.CloudMusic)
                Global.Cache.CloudLoginkey = (CloudMusic.LoginKey)Plat.LoginKey;
            else if (Plat.Type == ePlatform.QQMusic)
                Global.Cache.QQLoginkey = (QQMusic.LoginKey)Plat.LoginKey;
            Global.Cache.Write();
        }

        /// <summary>
        /// Show input or web
        /// </summary>
        /// <param name="bInputVisib"></param>
        public void ShowPage(bool bInputVisib)
        {
            if(bInputVisib)
            {
                InputVisibility = Visibility.Visible;
                WebVisibility = Visibility.Hidden;
            }
            else
            {
                InputVisibility = Visibility.Hidden;
                WebVisibility = Visibility.Visible;
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
                Errlabel = "Username or password is error!";
                goto RETURN;
            }

            object key = await SudaLib.Method.GetLoginKey(Plat.Type, Username, Password);
            if (key != null)
            {
                Plat.LoginKey = key;
                LoginSuccess();
                goto RETURN;
            }

            Errlabel = "Login error!";

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
            object key = SudaLib.Method.GetLoginKey(Plat.Type, CtrlWebBrowser.DocumentTitle, CtrlWebBrowser.Document.Cookie).Result;
            if(key != null)
            {
                Plat.LoginKey = key;
                LoginSuccess();
            }
        }
        #endregion
    }
}
