using Microsoft.Data.SqlClient;

namespace KaraokeManagement.DAL
{
    public class DatabaseAccess
    {
        protected string connectionString = "Server=localhost;Database=KaraokeDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
