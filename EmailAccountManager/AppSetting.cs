using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace EmailAccountManager
{
    public class AppSetting
    {
        public ObservableCollection<string> UserNames { get; set; } = new ObservableCollection<string>();
        public bool IsAutoLogin { get; set; }
        public string DefaultUser { get; set; }


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
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    return JsonConvert.DeserializeObject<AppSetting>(json);
                }
                else
                {
                    return new AppSetting
                    {
                        UserNames = new ObservableCollection<string>(),
                        IsAutoLogin = false,
                        DefaultUser = string.Empty
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to load appSetting file", ex);
                return null;
            }
        }
    }
}
