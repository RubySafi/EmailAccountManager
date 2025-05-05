using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;

namespace EmailAccountManager
{
    public class AppSetting
    {
        public int Version { get; set; } = AsmUtility.AppSettingVersion;
        public ObservableCollection<string> UserNames { get; set; } = new ObservableCollection<string>();
        public bool IsAutoLogin { get; set; }
        public string DefaultUser { get; set; }

        public bool CanAutoLogin => IsAutoLogin && UserNames.Contains(DefaultUser);

        private static readonly string SettingsFilePath = "appsettings.json";

        public static void Save(AppSetting settings)
        {
            try
            {
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to save appSetting file", ex);
            }
        }

        public static AppSetting Load()
        {
            int version = 0;
            try
            {
                if (!File.Exists(SettingsFilePath))
                {
                    return CreateDefault();
                }

                string json = File.ReadAllText(SettingsFilePath);

                var jObject = JObject.Parse(json);
                version = jObject["Version"]?.Value<int>() ?? 1;


                if (version == AsmUtility.AppSettingVersion)
                {
                    return JsonConvert.DeserializeObject<AppSetting>(json);
                }
                else
                {
                    // Reserved for future version compatibility handling
                    switch (version)
                    {
                        default:
                            Logger.LogError($"Unsupported AppSetting version: {version}");
                            return CreateDefault();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to load appSetting file. AppSetting version: {version}", ex);
                return null;
            }
        }

        public static AppSetting CreateDefault()
        {
            return new AppSetting
            {
                Version = AsmUtility.AppSettingVersion,
                UserNames = new ObservableCollection<string>(),
                IsAutoLogin = false,
                DefaultUser = string.Empty
            };
        }
    }
}
