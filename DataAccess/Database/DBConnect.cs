using System;
using System.Data.SqlClient;

namespace Backend_Development.DataAccess
{
    public class DBConnect
    {
        private string _connectionString = "Data Source=MINECRAFT;Initial Catalog=Test;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Application Name=\"Microsoft SQL Server Management Studio - Query\";Command Timeout=0";

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public bool TestConnection()
        {
            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    connection.Open();
                    Console.WriteLine("Connection successful!");
                    return true;
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
                return false;
            }
        }
    }
}