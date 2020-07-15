using System;
using System.Windows;
using Music.QQMusic;
using Stylet;

namespace Suda.Pages
{
    public class MainViewModel : Screen
    {
        public Record Account { get; set; } = new Record();
        public string UserName { get; set; } = "请登录";



        public async void RefreshUser()
        {
            var check = await Account.GetUser();
            if (!check)
                return; //print err

        }

        #region Windows
        public void WindowMove()
        {
            ((MainView)this.View).DragMove();
        }

        public void WindowMin()
        {
            ((MainView)this.View).WindowState = WindowState.Minimized;
        }

        public void WindowMax()
        {
            AIGS.Helper.ScreenShotHelper.MaxWindow((MainView)this.View);
        }

        public void WindowClose()
        {
            RequestClose();
        }
        #endregion
    }
}
