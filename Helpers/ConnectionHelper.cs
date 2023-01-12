using Npgsql;

namespace Contact_Manager_Pro.Helpers
{
    public static class ConnectionHelper
    {
        public static string GetConnectionString(IConfiguration configuration)
        {
            var connectionString = configuration.GetSection("pgSettings")["pgConnection"];
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            return string.IsNullOrEmpty(databaseUrl) ? connectionString : BuildConnectionString(databaseUrl);
        }


        // build a connection string from the environment. Postgres
        private static string BuildConnectionString(string databaseUrl)
        {
            var dataBaseUri = new Uri(databaseUrl);
            var userInfo = dataBaseUri.UserInfo.Split(':');
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = dataBaseUri.Host,
                Port = dataBaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = dataBaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Require,
                TrustServerCertificate = true
            };
            return builder.ToString();
        }
    }
}
