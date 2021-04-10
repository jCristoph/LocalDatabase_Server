using System.Collections.Generic;

namespace LocalDatabase_Server
{
    public static class ServerCom
    {
        #region type of messages
        /// <summary>
        /// For Server usage. This message is sent to client to confirm or reject login
        /// </summary>
        /// <param name="isLogged"></param>
        /// <returns></returns>
        public static string CheckLoginMessage(bool isLogged)
        {
            if (isLogged)
                return "<Task=CheckLogin><isLogged>Yes</isLogged><Login>";
            else
                return "<Task=CheckLogin><isLogged>No</isLogged><Login>";
        }

        /// <summary>
        /// For both. Send to other order that he has to listen for file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string DownloadMessage()
        {
            return "<Task=Download></Task><#>";
        }

        /// <summary>
        /// For both. Send to other order to send file of this path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string SendMessge(string path)
        {
            return "<Task=Send><Path>" + path + "</Path></Task><#>";
        }

        /// <summary>
        /// For server side. Sends directory.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static string[] SendDirectoryMessage(List<DirectoryElement> directory)
        {
            string[] dirMessage = new string[directory.Count];
            int i = 0;
            foreach (var de in directory)
            {
                if (i < directory.Count - 1)
                    dirMessage[i] = "<Task=SendingDir><Folder>" + de.isFolder + "</Folder>" +
                                "<Path>" + de.path + " </Path>" +
                                "<Name>" + de.name + "</Name>" +
                                "<Size>" + de.size + "</Size>" +
                                "<Last Write>" + de.lwr + "</Last Write></Task>";
                else
                    dirMessage[i] = "<Task=SendingDir><Folder>" + de.isFolder + "</Folder>" +
                                "<Path>" + de.path + " </Path>" +
                                "<Name>" + de.name + "</Name>" +
                                "<Size>" + de.size + "</Size>" +
                                "<Last Write>" + de.lwr + "</Last Write></Task><#>";
                i++;
            }
            return dirMessage;
        }

        /// <summary>
        /// For Server usage. Is order for client to download directory one more time because of changes.
        /// </summary>
        /// <returns></returns>
        public static string UpdateDirectoryOrderMessage()
        {
            return "<Task=DownloadDir></Task><#>";
        }

        /// <summary>
        /// For client and server usage. If something goes wrong or needs only confirmations then this method
        /// send what to show in other side MessageBox.
        /// </summary>
        /// <param name="goesWrong"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string responseMessage(string content)
        {
            return "<Task=Response><Content>" + content + "</Content></Task><#>";
        }
        #endregion

        #region message Recognizer methods
        /// <summary>
        /// Only for Server. Client sends login parameters and now server by this Method checks if login parameters are invalid
        /// param s is a message sent from Client like <Task=Login><Login>login</Login><Pass>password</Pass></Task><#>"
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool LoginRecognizer(string s)
        {
            int IndexHome = s.IndexOf("<Login>") + "<Login>".Length;
            int IndexEnd = s.LastIndexOf("</Login>");
            string login = s.Substring(IndexHome, IndexEnd - IndexHome);
            IndexHome = s.IndexOf("<Pass>") + "<Pass>".Length;
            IndexEnd = s.LastIndexOf("</Pass>");
            string passowrd = s.Substring(IndexHome, IndexEnd - IndexHome);
            //tutaj funkcja sprawdzajaca haslo
            if (login.Equals("login") && passowrd.Equals("password"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// For Client and Server usage. This message tells client/server to launch file downloader method.
        /// param s is message sent from other like "<Task=Download></Task><#>"
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string DownloadRecognizer(string s)
        {
            return "";
        }

        /// <summary>
        /// For Client and server usage. This message tells server/client what file has to be sent
        /// param s is message from other ie. <Task=Send><Path>path</Path></Task><#>
        /// Right here we are launching sendFile method.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string SendRecognizer(string s)
        {
            int IndexHome = s.IndexOf("<Path>") + "<Path>".Length;
            int IndexEnd = s.LastIndexOf("</Path>");
            string path = s.Substring(IndexHome, IndexEnd - IndexHome);
            return path;
        }

        /// <summary>
        /// For Server usage. Client sent send order and right there we have to launch Com.SendDirectory(directory)
        /// </summary>
        /// <returns></returns>
        public static string SendDirectoryOrderRecognizer()
        {
            return "<Task=SendDir></Task><#>";
        }

        /// <summary>
        /// For Server usage. Client sends this message and Server now has to recognize path and delete file.
        /// </summary>
        /// <param name="path"></param>
        public static string DeleteRecognizer(string s)
        {
            int IndexHome = s.IndexOf("<Path>") + "<Path>".Length;
            int IndexEnd = s.LastIndexOf("</Path>");
            return s.Substring(IndexHome, IndexEnd - IndexHome);
        }

        /// <summary>
        /// For client and server usage. If something goes wrong or needs only confirmations then this method 
        /// read what to show in MessageBox.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string responseRecognizer(string s)
        {
            int IndexHome = s.IndexOf("<Content>") + "<Content>".Length;
            int IndexEnd = s.LastIndexOf("</Content>");
            return s.Substring(IndexHome, IndexEnd - IndexHome); ;
        }
        #endregion
    }
}
