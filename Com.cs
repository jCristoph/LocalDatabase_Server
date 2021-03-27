using System.Collections.Generic;

namespace LocalDatabase_Server
{
    public static class Com
    {
        public static string Login(string login, string password)
        {
            return "<Task=Login><Login>" + login + "</Login><Pass>" + password + "</Pass></Task>";
        }
        public static string CheckLogin(bool isLogged)
        {
            if (isLogged)
                return "<Task=CheckLogin><isLogged>Yes</isLogged><Login>";
            else
                return "<Task=CheckLogin><isLogged>No</isLogged><Login>";
        }
        public static string Logout()
        {
            return "<Task=Logout></Task>";
        }
        public static string Download(string path)
        {
            return "<Task=Download><Path>" + path + "</Path></Task>";
        }
        public static string Send(string path)
        {
            return "<Task=Send><Path>" + path + "</Path></Task>";
        }
        public static string[] SendDirectory(List<DirectoryElement> directory)
        {
            string[] dirMessage = new string[directory.Count + 1];
            int i = 1;
            dirMessage[0] = "<Task=SendingDir></Task>";
            foreach (var de in directory)
            {
                dirMessage[i] = "<Task=SendingDir><Folder>" + de.isFolder + "</Folder>" +
                                "<Path>" + de.path + " </Path>" +
                                "<Name>" + de.name + "</Name>" +
                                "<Size>" + de.size + "</Size>" +
                                "<Last Write>" + de.lwr + "</Last Write></Task>";
                i++;
            }
            return dirMessage;
        }
        public static string SendDirectoryOrder()
        {
            return "<Task=SendDir></Task>";
        }
        public static string UpdateDirectoryOrder()
        {
            return "<Task=DownloadDir></Task>";
        }
        public static string Delete(string path)
        {
            return "<Task=Delete><Path>" + path + "</Path></Task>";
        }
        public static string response(bool goesWrong, string content)
        {
            return "<Task=Response><Content>" + content + "</Content></Task>";
        }
    }
}
