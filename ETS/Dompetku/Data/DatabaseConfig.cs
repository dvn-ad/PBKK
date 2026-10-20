using System;
using System.Configuration;
using Microsoft.Data.SqlClient;

namespace Dompetku.Data
{
    public static class DatabaseConfig
    {
        private static string? _connectionString;

        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    try
                    {
                        var configStr = ConfigurationManager.ConnectionStrings["DompetkuConnection"]?.ConnectionString;
                        if (!string.IsNullOrWhiteSpace(configStr))
                        {
                            _connectionString = configStr;
                        }
                    }
                    catch
                    {
                        // Fallback below
                    }

                    if (string.IsNullOrEmpty(_connectionString))
                    {
                        _connectionString = "Server=.\\SQLEXPRESS;Database=DompetkuDB;Integrated Security=True;TrustServerCertificate=True;";
                    }
                }
                return _connectionString;
            }
            set => _connectionString = value;
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static bool TestConnection(out string errorMessage)
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT 1";
                cmd.ExecuteScalar();
                errorMessage = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
    }
}
