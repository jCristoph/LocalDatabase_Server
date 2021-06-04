using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalDatabase_Server.Database
{
    public class User
    {
        public int id { set; get; }
        public string surname { set; get; }
        public string name { set; get; }
        public string login { set; get; }
        public string password { set; get; }
        public string token { set; get; }
        public long limit { set; get; }

        public User(int id, string surname, string name, string login, string password, string token, long limit)
        {
            this.id = id;
            this.surname = surname;
            this.name = name;
            this.login = login;
            this.password = password;
            this.token = token;
            this.limit = limit;
        }
    }
}
