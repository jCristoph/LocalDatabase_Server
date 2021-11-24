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
            SetSetting(AvailableSpaceKey, (AvailableSpace).ToString());
        }

        /// <summary>
        /// Available space on disc in GB
        /// </summary>
        /// <returns>Space in GB</returns>
        public long GetAvailableSpace()
        {
            return long.Parse(GetSetting(AvailableSpaceKey));
        }

        public void SetIdleTime(int IdleTimeInMinutes)
        {
            SetSetting(IdleTimeKey, IdleTimeInMinutes.ToString());
        }

        /// <summary>
        ///  Idle time in Minutes
        /// </summary>
        /// <returns>Idle time</returns>
        public int GetIdleTime()
        {
            return int.Parse(GetSetting(IdleTimeKey));
        }

        /// <summary>
        /// Sets default save path
        /// </summary>
        /// <param name="SavePath"></param>
        public void SetSavePath(string SavePath)
        {
            SetSetting(SavePathKey, SavePath);
        }

        public string GetSavePath()
        {
            return GetSetting(SavePathKey);
        }

        /// <summary>
        /// Sets server Ip. Default is 127.0.0.1
        /// </summary>
        /// <param name="serverIp"></param>
        public void SetServerIp(string serverIp)
        {
            SetSetting(ServerIp, serverIp);
        }

        public string GetServerIp()
        {
            return GetSetting(ServerIp);
        }

        private const string IdleTimeKey = "IdleTimeKey";
        private const string AvailableSpaceKey = "AvailableSpaceKey";
        private const string SavePathKey = "SavePathKey";
        private const string ServerIp = "ServerIp";

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
