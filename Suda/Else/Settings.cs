using AIGS.Helper;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
        [JsonProperty("Compare")]
        public CompareArgs Compare { get; set; } = new CompareArgs();
        [JsonProperty("ThemeType")]
        public Theme.Type ThemeType { get; set; } = Theme.Type.Default;
        [JsonProperty("LanguageType")]
        public Language.Type LanguageType { get; set; } = Language.Type.Default;

        [JsonProperty("EnableQQMusic")]
        public bool EnableQQMusic { get; set; } = true;
        [JsonProperty("EnableCloudMusic")]
        public bool EnableCloudMusic { get; set; } = true;
        [JsonProperty("EnableTidal")]
        public bool EnableTidal { get; set; } = true;
        [JsonProperty("EnableSpotify")]
        public bool EnableSpotify { get; set; } = true;
        [JsonProperty("EnableAppleMusic")]
        public bool EnableAppleMusic { get; set; } = true;

        public bool Save()
        {
            string data = JsonHelper.ConverObjectToString<Settings>(this,true);
            return FileHelper.Write(data, true, Global.PATH_SETTINGS);
        }

        public static Settings Read()
        {
            string data = FileHelper.Read(Global.PATH_SETTINGS);
            Settings ret = JsonHelper.ConverStringToObject<Settings>(data);
            if (ret == null)
                return new Settings();
            return ret;
        }
    }
}

