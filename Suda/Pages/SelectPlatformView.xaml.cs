using HandyControl.Controls;
using Suda.Else;
using SudaLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static SudaLib.Common;

namespace Suda.Pages
{
    /// <summary>
    /// SelectPlatform.xaml 的交互逻辑
    /// </summary>
    public partial class SelectPlatformView : UserControl
    {
        public Dictionary<int, string> ComboxPlatforms { get; set; } = new Dictionary<int, string>();
        Action<SudaLib.ePlatform> Action { get; set; }

        public SelectPlatformView(ObservableCollection<Platform> Platforms, Action<SudaLib.ePlatform> action)
        {
            InitializeComponent();

            Action = action;
            foreach (var item in Platforms)
            {
                if (item.LoginKey != null)
                    ComboxPlatforms.Add((int)item.Type, Platform.GetPlatformDisplayName(item.Type));
            }
            CtlCombox.ItemsSource = ComboxPlatforms;
        }

        private void Confim(object sender, RoutedEventArgs e)
        {
            if(ComboxPlatforms.Count <= 0)
            {
                Growl.Error(Suda.Else.Language.Get("strmsgNoPlatform"), Global.TOKEN_MAIN);
                return;
            }

            if (CtlCombox.SelectedIndex >= 0)
            {
                ePlatform type = (ePlatform)ComboxPlatforms.ElementAt(CtlCombox.SelectedIndex).Key;
                if (Action != null)
                    Action(type);
            }
            BtnClose.Command.Execute(null);
        }
    }
}
