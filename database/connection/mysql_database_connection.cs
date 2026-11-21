using System;
using MySql.Data.MySqlClient;

namespace oop_project.Database
{
    public class MySqlDatabaseConnection : IDatabaseConnection
    {
        // DEPLOYMENT: the constants below are the values this project has always
        // used on the developer machine. A deployment can override any of them
        // with an environment variable, so the same executable can point at a
        // different server without being rebuilt.
        //
        //   LIBRARY_DB_SERVER      server name or IP        (default localhost)
        //   LIBRARY_DB_PORT        TCP port                 (default 3306)
        //   LIBRARY_DB_USER        MySQL user               (default root)
        //   LIBRARY_DB_PASSWORD    MySQL password           (default empty)
        //   LIBRARY_DB_NAME        database name            (default libsys_database)
        //   LIBRARY_DB_CONNECTION  complete MySQL connection string; wins over all
        //                          of the values above and can add options such as
        //                          SslMode=Required for a remote server.
        private const string ServerVariable = "LIBRARY_DB_SERVER";
        private const string PortVariable = "LIBRARY_DB_PORT";
        private const string UserVariable = "LIBRARY_DB_USER";
        private const string PasswordVariable = "LIBRARY_DB_PASSWORD";
        private const string DatabaseVariable = "LIBRARY_DB_NAME";
        private const string ConnectionVariable = "LIBRARY_DB_CONNECTION";

        private const string DefaultServer = "localhost";
        private const int DefaultPort = 3306;
        private const string DefaultUser = "root";
        private const string DefaultPassword = "";
        private const string DefaultDatabase = "libsys_database";

        public MySqlConnection CreateConnection()
        {
            string? configured = Environment.GetEnvironmentVariable(ConnectionVariable);

            if (!string.IsNullOrWhiteSpace(configured))
            {
                return new MySqlConnection(configured.Trim());
            }

            // The builder escapes the values, so a password that contains a
            // semicolon or an equal sign still produces a valid connection string.
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
            {
                Server = Setting(ServerVariable, DefaultServer),
                Port = PortSetting(),
                UserID = Setting(UserVariable, DefaultUser),
                Password = SecretSetting(PasswordVariable, DefaultPassword),
                Database = Setting(DatabaseVariable, DefaultDatabase)
            };

            return new MySqlConnection(builder.ConnectionString);
        }

        private static string Setting(string variable, string fallback)
        {
            string? value = Environment.GetEnvironmentVariable(variable);
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        // A password is taken exactly as written, spaces included; only an unset
        // variable falls back to the default.
        private static string SecretSetting(string variable, string fallback)
        {
            string? value = Environment.GetEnvironmentVariable(variable);
            return value ?? fallback;
        }

        private static uint PortSetting()
        {
            string? value = Environment.GetEnvironmentVariable(PortVariable);

            if (uint.TryParse(value, out uint port) && port > 0 && port <= 65535)
            {
                return port;
            }

            return DefaultPort;
        }
    }
}
