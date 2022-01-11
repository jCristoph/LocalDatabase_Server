
namespace LocalDatabase_Server.Data.Utils
{
    public static class UnitsConverter
    {

       public static double ConvertBytesToGigabytes(long bytes)
        {
            double resultGigabytes = bytes / 1024.0 / 1024.0 / 1024.0;
            return resultGigabytes;
        }

        public static long ConvertGigabytesToBytes(long gigabytes)
        {
            long resultBytes = gigabytes * 1024 * 1024 * 1024;
            return resultBytes;
        }
    }
}
