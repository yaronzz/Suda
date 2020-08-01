using AIGS.Common;
using AIGS.Helper;
using ICSharpCode.SharpZipLib.Zip;
using Suda.Else;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Suda.Pages
{
    public class DownloadViewModel : Suda.Else.ModelBase
    {
        public string Type { get; set; } = "(BETA)";
        public string Version { get; set; } = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public void GotoGithub() => NetHelper.OpenWeb(Global.URL_SUDA_GITHUB);
        public void Feedback() => NetHelper.OpenWeb(Global.URL_SUDA_ISSUES);
        public void Telegram() => NetHelper.OpenWeb(Global.URL_SUDA_GROUP);

        public int ProgressValue { get; set; }
        public string TotalSize { get; set; } 
        public string DownloadSize { get; set; } 
        public Action<object> Action { get; set; }
        public Visibility VisibilityErr { get; set; }

        public bool CheckRequire()
        {
            string file = "./libcef.dll";
            return File.Exists(file);
        }

        public bool DownloadRequire(Action<object> action)
        {
            ProgressValue = 0;
            TotalSize = null;
            DownloadSize = null;
            Action = action;
            VisibilityErr = Visibility.Collapsed;

            if (UnzipRequire())
            {
                Action((true, "Unzip require success!"));
                return false;
            }

            DownloadFileHepler.StartAsync(Global.PATH_REQUIRE, Global.PATH_REQUIRE, null, UpdateDownloadNotify, CompleteDownloadNotify, ErrDownloadNotify, 3);
            return true;
        }

        public bool UnzipRequire()
        {
            try
            {
                FastZip fz = new FastZip();
                if (File.Exists(Global.PATH_REQUIRE))
                {
                    fz.ExtractZip(Global.PATH_REQUIRE, Path.GetFullPath("./"), null);
                    if (CheckRequire())
                        return true;
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
            }
            return false;
        }

        public bool UpdateDownloadNotify(long lTotalSize, long lAlreadyDownloadSize, long lIncreSize, object data)
        {
            int progress = (int)(lAlreadyDownloadSize * 100 / lTotalSize);
            if (progress > ProgressValue)
                ProgressValue = progress;

            float Size;
            if (TotalSize.IsBlank())
            {
                Size = (float)lTotalSize / 1048576;
                TotalSize = Size.ToString("#0.00");
            }

            Size = (float)lAlreadyDownloadSize / 1048576;
            DownloadSize = Size.ToString("#0.00");
            return true;
        }

        public void CompleteDownloadNotify(long lTotalSize, object data)
        {
            ProgressValue = 100;

            if(UnzipRequire())
                Action((true, "Download success!"));
            else
                ShowErr();
        }

        public void ErrDownloadNotify(long lTotalSize, long lAlreadyDownloadSize, string sErrMsg, object data)
        {
            ShowErr();
        }

        public void OpenRequireUrl()
        {
            NetHelper.OpenWeb(Global.PATH_REQUIRE);
        }

        public void ShowErr()
        {
            VisibilityErr = Visibility.Visible;
        }
    }
}
