using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace LocalDatabase_Server.Database
{

   class ConnectionString
    {
        private readonly SqlConnection _connectionString;

        public ConnectionString()
        {
            string _projectDirectory = System.IO.Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
            string _dbFilePath = $"{_projectDirectory}\\Data\\Database\\PZ_BD.mdf";
            _connectionString = new SqlConnection($@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={_dbFilePath};Integrated Security=True;Connect Timeout=30");
        }
        
        public SqlConnection GetConnectionString()
        {
            return _connectionString;
        }
    }
}
