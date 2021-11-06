using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace LocalDatabase_Server.Database
{
    public static class Generator
    {
        public static string GenerateRandomString()
        {
            string value = Membership.GeneratePassword(10, 3);
            return value;
        }

        public static string GenerateToken()
        {
            string randomString = GenerateRandomString();
            using (var sha256 = new SHA256Managed())
            {
                return BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(randomString))).Replace("-", "").Substring(0, 10);
            }
        }


        public static string GenerateLogin(string surname, string name)
        {
            return surname + '.' + name;
        }
    }
}
