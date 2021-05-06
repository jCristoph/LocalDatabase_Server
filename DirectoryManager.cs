using System;
using System.Collections.Generic;
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
            DirectoryElement de = new DirectoryElement("\\Main_Folder", 0, "None", true);
            directoryElements.Add(de);
            ProcessDirectory(tokenDirectory);
            //foreach (var dirEl in directoryElements)
            //    PrintFolderContent(dirEl);
        }
        public DirectoryManager(List<DirectoryElement> directoryElements)
        {
            this.directoryElements = directoryElements;
        }

        public void ProcessDirectory(string targetDirectory)
        {

            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string path in fileEntries)
            {
                DirectoryElement de = new DirectoryElement(path.Replace(tokenDirectory, "Main_Folder"), new FileInfo(path).Length, File.GetLastWriteTime(path).ToString(), false);
                directoryElements.Add(de);
            }

            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
            {
                DirectoryElement de = new DirectoryElement(subdirectory.Replace(tokenDirectory, "Main_Folder"), 0, "None", true);
                directoryElements.Add(de);
                ProcessDirectory(subdirectory);
            }
        }
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
        public string DeleteElement(string path)
        {
            int IndexHome = path.LastIndexOf("\\") + "\\".Length;
            int IndexEnd = path.Length;
            string name = path.Substring(IndexHome, IndexEnd - IndexHome);
            path = path.Replace("Main_Folder", @"C:\Directory_test");
            directoryElements.Remove(directoryElements.Find(x => x.path == path && x.name == name));
            if (System.IO.File.Exists(path))
            {
                try
                {
                    System.IO.File.Delete(path);
                }
                catch (System.IO.IOException e)
                {
                    return e.Message;
                }
                return "GOOD";
            }
            else
                return "BAD";
        }
        private void PrintFolderContent(DirectoryElement dirEl)
        {
            if (dirEl.isFolder)
            {
                Console.WriteLine("Inside {0} are:", dirEl.name);
                foreach (var de in directoryElements)
                {
                    if (de.pathArray.Count > 1)
                    {
                        if (de.pathArray[de.pathArray.Count - 1].Equals(dirEl.name) && de.pathArray[de.pathArray.Count - 2].Equals(dirEl.pathArray[dirEl.pathArray.Count - 1]))
                            Console.WriteLine("\t" + de.name + " path of subfolder: " + dirEl.pathArray[dirEl.pathArray.Count - 1]);
                    }
                    else
                        if (de.pathArray[de.pathArray.Count - 1].Equals(dirEl.name))
                        Console.WriteLine("\t" + de.name);
                }
            }
        }
    }
}
