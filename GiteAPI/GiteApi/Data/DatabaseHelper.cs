using System.Data;
using Microsoft.Data.SqlClient;

namespace GiteApi.Data
{
    public class DatabaseHelper
    {
        private readonly string? _connectionString;

        public DatabaseHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("GiteDb");
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<DataTable> ExecuteQueryAsync(string query, List<SqlParameter>? parameters = null)
        {
            using var connection = GetConnection();
            using var command = new SqlCommand(query, connection);

            if (parameters != null)
            {
                command.Parameters.AddRange(parameters.ToArray());
            }

            var dataTable = new DataTable();
            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();
            dataTable.Load(reader);

            return dataTable;
        }

        public async Task<int> ExecuteNonQueryAsync(string query, List<SqlParameter>? parameters = null)
        {
            using var connection = GetConnection();
            using var command = new SqlCommand(query, connection);

            if (parameters != null)
            {
                command.Parameters.AddRange(parameters.ToArray());
            }

            await connection.OpenAsync();
            return await command.ExecuteNonQueryAsync();
        }

        public async Task<int> ExecuteScalarAsync(string query, List<SqlParameter>? parameters = null)
        {
            using var connection = GetConnection();
            using var command = new SqlCommand(query, connection);

            if (parameters != null)
            {
                command.Parameters.AddRange(parameters.ToArray());
            }

            await connection.OpenAsync();
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }
    }
}
