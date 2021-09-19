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

        // ObservableCollection container doesn't have a lambda "Contains" version of method so I have to create new user and 
        //compare them. To do it simply I compare users only by token - which is unique so to create new user (only for compare operation) I use this
        //kind of consturctor.
        public User(string token)
        {
            this.id = 0;
            this.surname = "";
            this.name = "";
            this.login = "";
            this.password = "";
            this.token = token;
            this.limit = 0;
        }

        //ObservableCollection container doesn't have a lambda "Contains" version of method so I have to create new user and 
        //compare them. To do it simply I compare users only by token - which is unique
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            User u = obj as User;
            if (u == null) return false;
            return u.token.Equals(token);
        }
    }
}
