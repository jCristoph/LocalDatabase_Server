
namespace LocalDatabase_Server.Data.Utils
{
    public static class UnitsConverter
    {

       public static long ConvertBytesToGigabytes(long bytes)
        {
            long resultGigabytes = bytes / 1024 / 1024 / 1024;
            return resultGigabytes;
        }

        public static long ConvertGigabytesToBytes(long gigabytes)
        {
            long resultBytes = gigabytes * 1024 * 1024 * 1024;
            return resultBytes;
        }
    }
}
