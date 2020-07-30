using HandyControl.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Suda.Else
{
    public class Theme
    {
        public enum Type
        {
            Light,
            Dark,
        }

        public static void Change(Type type = Type.Light)
        {
            SharedResourceDictionary.SharedDictionaries.Clear();
            if (type == Type.Light)
                System.Windows.Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/HandyControl;component/Themes/SkinDefault.xaml", UriKind.RelativeOrAbsolute) });
            else
                System.Windows.Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/HandyControl;component/Themes/SkinDark.xaml", UriKind.RelativeOrAbsolute) });

            System.Windows.Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/HandyControl;component/Themes/Theme.xaml") });
            System.Windows.Application.Current.MainWindow?.OnApplyTemplate();
        }
    }
}
