using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace LocalDatabase_Server
{
    class DirectoryManager
    {
        public List<DirectoryElement> directoryElements { get; set; }
        private string tokenDirectory;

        public DirectoryManager(string targetDirectory)
        {
            tokenDirectory = targetDirectory.Remove(targetDirectory.Length - 1);
            directoryElements = new List<DirectoryElement>();
            DirectoryElement directoryElement = new DirectoryElement("\\Main_Folder", 0, "None", true);
            directoryElements.Add(directoryElement);
            ProcessDirectory(tokenDirectory);
        }
       
        public DirectoryManager(List<DirectoryElement> directoryElements)
        {
            this.directoryElements = directoryElements;
        }

        //method that finds every element in folder. It look for in every subfolder so this method is recurrent.
        public void ProcessDirectory(string targetDirectory)
        {
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string path in fileEntries)
            {
                DirectoryElement directoryElement = new DirectoryElement(path.Replace(tokenDirectory, "Main_Folder"), new FileInfo(path).Length, File.GetLastWriteTime(path).ToString(), false);
                directoryElements.Add(directoryElement);
            }

            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
            {
                DirectoryElement directoryElement = new DirectoryElement(subdirectory.Replace(tokenDirectory, "Main_Folder"), 0, "None", true);
                directoryElements.Add(directoryElement);
                ProcessDirectory(subdirectory);
            }
        }

        /*If client sends message with path, it has to be translate. The message is in kind of xml message.
         * This method is used to translate it. 
         */
        public void ProcessPath(string s)
        {
            try
            {
                int isFolderIndexHome = s.IndexOf("<Folder>") + "<Folder>".Length;
                int isFolderIndexEnd = s.LastIndexOf("</Folder>");
                string isFolder = s.Substring(isFolderIndexHome, isFolderIndexEnd - isFolderIndexHome);

                int pathIndexHome = s.IndexOf("<Path>") + "<Path>".Length;
                int pathIndexEnd = s.LastIndexOf("</Path>");
                string path = s.Substring(pathIndexHome, pathIndexEnd - pathIndexHome);

                int nameIndexHome = s.IndexOf("<Name>") + "<Name>".Length;
                int nameIndexEnd = s.LastIndexOf("</Name>");
                string name = s.Substring(nameIndexHome, nameIndexEnd - nameIndexHome);

                int sizeIndexHome = s.IndexOf("<Size>") + "<Size>".Length;
                int sizeIndexEnd = s.LastIndexOf("</Size>");
                string size = s.Substring(sizeIndexHome, sizeIndexEnd - sizeIndexHome);

                int lwrIndexHome = s.IndexOf("<Last Write>") + "<Last Write>".Length;
                int lwrIndexEnd = s.LastIndexOf("</Last Write>");
                string lwr = s.Substring(lwrIndexHome, lwrIndexEnd - lwrIndexHome);

                DirectoryElement de = new DirectoryElement(path, name, long.Parse(size), lwr, isFolder);
                directoryElements.Add(de);
            }
            catch (Exception e)
            {

            }
        }

        //method that deletes file or folder from directoryElements container and also from disk.
        public string DeleteElement(string path, string isFolder)
        {
            int IndexHome = path.LastIndexOf("\\") + "\\".Length;
            int IndexEnd = path.Length;
            string name = path.Substring(IndexHome, IndexEnd - IndexHome);
            path = path.Replace("Main_Folder", @"C:\Directory_test");
            directoryElements.Remove(directoryElements.Find(x => x.path == path && x.name == name));
            if (isFolder.Equals("False"))
            {
                if (File.Exists(path))
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch (IOException e)
                    {
                        return e.Message;
                    }
                    return "Usunięto element";
                }
                else
                    return "Nie udało się usunąć elementu";
            }
            else
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                    return "Usunięto element";
                }
                return "Nie udało się usunąć elementu";
            }
        }

        //this method is used for create new folder in server disk.
        public void CreateFolder(string path)
        {
            path = path.Replace("Main_Folder", @"C:\Directory_test");
            Directory.CreateDirectory(path);
        }
    }
}
