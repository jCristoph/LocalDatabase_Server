using System;
using System.Configuration;

namespace LocalDatabase_Server.Directory
{
  
    public sealed class SettingsManager
    {

        private static readonly Lazy<SettingsManager> lazy = new Lazy<SettingsManager>(() => new SettingsManager());

        public static SettingsManager Instance { get { return lazy.Value; } }

        public void SetAvailableSpace(long AvailableSpace)
        {
            SetSetting(IdleTimeKey, AvailableSpace.ToString());
        }

        public long GetAvailableSpace()
        {
            return long.Parse(GetSetting(AvailableSpaceKey));
        }

        public void SetIdleTime(int IdleTimeInMinutes)
        {
            SetSetting(IdleTimeKey, ConvertToMinutes(IdleTimeInMinutes).ToString());
        }

        public int GetIdleTime()
        {
            return int.Parse(GetSetting(IdleTimeKey));
        }

        public void SetSavePath(string SavePath)
        {
            SetSetting(SavePathKey, SavePath);
        }

        public string GetSavePath()
        {
            return GetSetting(SavePathKey);
        }

        private int ConvertToMinutes(int TimeInMinutes)
        {
            return TimeInMinutes * 60 * 1000;
        }

        private const string IdleTimeKey = "IdleTimeKey";
        private const string AvailableSpaceKey = "AvailableSpaceKey";
        private const string SavePathKey = "SavePathKey";

        private static string GetSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        private static void SetSetting(string key, string value)
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings[key].Value = value;
            configuration.Save(ConfigurationSaveMode.Full, true);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
