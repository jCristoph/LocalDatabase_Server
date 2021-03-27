﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace LocalDatabase_Server
{
    public class DirectoryElement
    {
        public string path { get; set; }
        public List<string> pathArray { get; set; }
        public string name { get; set; }
        public long size { get; set; }
        public string lwr { get; set; }
        public bool isFolder { get; set; }


        public DirectoryElement(string path, long size, string lwr, bool isFolder)
        {
            //pelna sciezka
            this.path = path;
            //sciezka zostaje podzielona na osobne stringi o nazwach podfolderów w skrócie : C:\Directory_test\Folder2\Folder6 -> C:,Directory_test,Folder2,Folder6,(pusty element)
            pathArray = path.Split('\\').ToList<String>();
            //nazwe pliku/folderu wrzucamy do zmiennej name i usuwamy ta czesc ze sciezki
            this.name = pathArray[pathArray.Count - 1];
            pathArray.RemoveAt(pathArray.Count - 1);
            this.path = this.path.Substring(0, path.LastIndexOf(name));
            this.size = size;
            this.lwr = lwr;
            this.isFolder = isFolder;
        }

        public DirectoryElement(string pathWithoutName, string name, long size, string lwr, string isFolder)
        {
            this.path = pathWithoutName.Replace(" ", "");
            //sciezka zostaje podzielona na osobne stringi o nazwach podfolderów w skrócie : C:\Directory_test\Folder2\Folder6 -> C:,Directory_test,Folder2,Folder6,(pusty element)
            pathArray = path.Split('\\').ToList<String>();
            pathArray.RemoveAt(pathArray.Count - 1);
            this.name = name;
            this.size = size;
            this.lwr = lwr;
            if (isFolder.Equals("True"))
                this.isFolder = true;
            else
                this.isFolder = false;
        }

        public override string ToString()
        {
            return "Path: " + path + ", name: " + name + ", size: " + size + ", lwr: " + lwr + ", isFolder: " + isFolder.ToString();
        }
    }
}

