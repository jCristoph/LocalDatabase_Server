using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalDatabase_Server.Directory
{
  
    public sealed class SettingsManager
    {
        private int IdleTime;
        private long AvailableSpace;
        private string SavePath;

        private static readonly Lazy<SettingsManager> lazy = new Lazy<SettingsManager>(() => new SettingsManager());

        public static SettingsManager Instance { get { return lazy.Value; } }

        private SettingsManager()
        {
            IdleTime = ConvertToMinutes(15);
            DriveInfo dDrive = new DriveInfo("C");
            AvailableSpace = dDrive.AvailableFreeSpace;
            SavePath = (@"C:\Directory_test\");
        }

        public void SetAvailableSpace(long AvailableSpaceSpace)
        {
            this.AvailableSpace = AvailableSpaceSpace;
        }
        
        public long GetAvailableSpace()
        {
            return AvailableSpace;
        }

        public void SetIdleTime(int IdleTimeInMinutes)
        {
            this.IdleTime = ConvertToMinutes(IdleTimeInMinutes);
        }

        public int GetIdleTime()
        {
            return IdleTime;
        }

        public void SetSavePath(string SavePath)
        {
            this.SavePath = SavePath;
        }

        public string GetSavePath()
        {
            return SavePath;
        }

        private int ConvertToMinutes(int TimeInMinutes)
        {
            return TimeInMinutes * 60 * 1000;
        }
    }
}
