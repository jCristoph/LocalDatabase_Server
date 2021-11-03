using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalDatabase_Server.Data
{
    public static class ExceptionCatcher
    {
        private static string path = "Log.txt";

        public static void addExceptionToFile(string e)
        {
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(DateTime.Now + " " + e);
            }
        }
    }
}
