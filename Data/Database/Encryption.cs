using System;
using System.Security.Cryptography;
using System.Text;

namespace LocalDatabase_Server.Data.Database
{
    public static class Encryption
    {
        //password encryption method SHA256
        public static string encryption256(string password)
        {
            Byte[] passBytes = Encoding.UTF8.GetBytes(password);
            Byte[] hashBytes = new SHA256CryptoServiceProvider().ComputeHash(passBytes);
            return BitConverter.ToString(hashBytes);
        }
    }
}
