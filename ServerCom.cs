using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

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
        public static string CheckLoginMessage(string isLogged)
        {
            if (!isLogged.Equals("ERROR"))
                return "<Task=CheckLogin><isLogged>" + isLogged +"</isLogged><Login>";
            else
                return "<Task=CheckLogin><isLogged>ERROR</isLogged><Login>";
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
        public static string LoginRecognizer(string s)
        {
            int IndexHome = s.IndexOf("<Login>") + "<Login>".Length;
            int IndexEnd = s.LastIndexOf("</Login>");
            string login = s.Substring(IndexHome, IndexEnd - IndexHome);
            IndexHome = s.IndexOf("<Pass>") + "<Pass>".Length;
            IndexEnd = s.LastIndexOf("</Pass>");
            string passowrd = s.Substring(IndexHome, IndexEnd - IndexHome);
            //tutaj funkcja sprawdzajaca haslo
            SqlConnection polaczenie = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\PC\source\repos\LocalDatabase_Server\PZ_BD.mdf;Integrated Security=True;Connect Timeout=30");
            SqlCommand zapytanie = new SqlCommand();
            zapytanie.Connection = polaczenie;
            zapytanie.CommandText = "SELECT * FROM [User] WHERE Login = '" + login + "' and Password = '" + passowrd + "'";
            SqlDataAdapter adapter = new SqlDataAdapter(zapytanie);
            DataTable tabela = new DataTable();
            adapter.Fill(tabela);
            if (tabela.Rows.Count == 1)
                return tabela.Rows[0].ItemArray.GetValue(5).ToString();
            else
                return "ERROR";
        }

        /// <summary>
        /// For Client and Server usage. This message tells client/server to launch file downloader method.
        /// param s is message sent from other like "<Task=Download></Task><#>"
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string DownloadRecognizer(string s)
        {
            int IndexHome = s.IndexOf("<Path>") + "<Path>".Length;
            int IndexEnd = s.LastIndexOf("</Path>");
            string path = s.Substring(IndexHome, IndexEnd - IndexHome);
            return path;
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
        public static string SendDirectoryOrderRecognizer(string s)
        {
            int IndexHome = s.IndexOf("<Token>") + "<Token>".Length;
            int IndexEnd = s.LastIndexOf("</Token>");
            string token = s.Substring(IndexHome, IndexEnd - IndexHome);
            return token;
        }

        /// <summary>
        /// For Server usage. Client sends this message and Server now has to recognize path and delete file.
        /// </summary>
        /// <param name="path"></param>
        public static string[] DeleteRecognizer(string s)
        {
            string[] data = new string[2];
            int IndexHomePath = s.IndexOf("<Path>") + "<Path>".Length;
            int IndexEndPath = s.LastIndexOf("</Path>");
            int IndexHomeFolder = s.IndexOf("<isFolder>") + "<isFolder>".Length;
            int IndexEndFolder = s.LastIndexOf("</isFolder>");
            data[0] = s.Substring(IndexHomePath, IndexEndPath - IndexHomePath);
            data[1] = s.Substring(IndexHomeFolder, IndexEndFolder - IndexHomeFolder);
            return data;
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
