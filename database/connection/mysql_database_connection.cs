using MySql.Data.MySqlClient;
namespace oop_project.Database
{

    public class MySqlDatabaseConnection : IDatabaseConnection
    {
        private const string Server = "localhost";

   
        private const int Port = 3309;
        private const string UserId = "root";
        private const string Password = "";
        private const string DatabaseName = "libsys_database";
        public MySqlConnection CreateConnection()
        {
            string connectionString = "server=" + Server + ";port=" + Port + ";user id=" + UserId + ";password=" + Password + ";database=" + DatabaseName + ";";
            return new MySqlConnection(connectionString);
        }
    }
}
