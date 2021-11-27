using System.IO;
using System.Linq;

namespace LocalDatabase_Server.Data.Utils
{
    public static class GetFileSumFromDirectory
    {
            /// <summary>
            /// Counts directory size in bytes
            /// </summary>
            /// <param name="searchDirectory"></param>
            /// <returns></returns>
            public static long count(string searchDirectory)
            {
                var files = System.IO.Directory.EnumerateFiles(searchDirectory);

                // get the sizeof all files in the current directory
                var currentSize = (from file in files let fileInfo = new FileInfo(file) select fileInfo.Length).Sum();

                var directories = System.IO.Directory.EnumerateDirectories(searchDirectory);

                // get the size of all files in all subdirectories
                var subDirSize = (from directory in directories select count(directory)).Sum();

                return currentSize + subDirSize;
            }
    }
}