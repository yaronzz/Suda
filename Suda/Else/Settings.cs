using AIGS.Helper;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SudaLib.Common;

namespace Suda.Else
{
    public class Settings : Stylet.Screen
    {
        public CompareArgs Compare { get; set; } = new CompareArgs();
        public Theme.Type ThemeType { get; set; } = Theme.Type.Light;
        public Language.Type LanguageType { get; set; } = Language.Type.Default;

        public bool EnableQQMusic { get; set; } = true;
        public bool EnableCloudMusic { get; set; } = true;
        public bool EnableTidal { get; set; } = true;
        public bool EnableSpotify { get; set; } = true;
        public bool EnableAppleMusic { get; set; } = true;

        public void Write()
        {
            string data = JsonHelper.ConverObjectToString<Settings>(this);
            FileHelper.Write(data, true, Global.PATH_SETTINGS);
        }

        public static Settings Read()
        {
            string data = FileHelper.Read(Global.PATH_SETTINGS);
            Settings ret = JsonHelper.ConverStringToObject<Settings>(data);
            if (ret == null)
                return new Settings();
            return ret;
        }

        public static void SetWebBrowserFeatures(int ieVersion)
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime)
                return;
            var appName = System.IO.Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            UInt32 ieMode = GeoEmulationModee(ieVersion);
            var featureControlRegKey = @"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Main\FeatureControl\";
            Registry.SetValue(featureControlRegKey + "FEATURE_BROWSER_EMULATION",
                appName, ieMode, RegistryValueKind.DWord);
            Registry.SetValue(featureControlRegKey + "FEATURE_ENABLE_CLIPCHILDREN_OPTIMIZATION",
                appName, 1, RegistryValueKind.DWord);
        }

        public static UInt32 GeoEmulationModee(int browserVersion)
        {
            UInt32 mode = 11000; // Internet Explorer 11. Webpages containing standards-based !DOCTYPE directives are displayed in IE11 Standards mode. 
            switch (browserVersion)
            {
                case 7:
                    mode = 7000; // Webpages containing standards-based !DOCTYPE directives are displayed in IE7 Standards mode. 
                    break;
                case 8:
                    mode = 8000; // Webpages containing standards-based !DOCTYPE directives are displayed in IE8 mode. 
                    break;
                case 9:
                    mode = 9000; // Internet Explorer 9. Webpages containing standards-based !DOCTYPE directives are displayed in IE9 mode.                    
                    break;
                case 10:
                    mode = 10000; // Internet Explorer 10.
                    break;
                case 11:
                    mode = 11000; // Internet Explorer 11
                    break;
            }
            return mode;
        }

    }
}
