using DataUploader.Constants;
using DataUploader.Providers.Interfaces;
using Microsoft.Data.SqlClient;

namespace DataUploader.Providers;

public class DatabaseProvider : IDatabaseProvider
{
    private const string DatabaseName = "master";
    private const string ConnectionString = $"Data Source=localhost;Initial Catalog={DatabaseName};User ID=sa;Password=enterpassAsd123@;Encrypt=False;";

    public async Task<bool>IsDatabaseNotExistsAsync()
    {
        var sqlQuery = string.Format(SqlQueries.ExistDatabaseQuery, DatabaseName);
        await using var connection = new SqlConnection(ConnectionString);
        await using var command = new SqlCommand(sqlQuery, connection);
        await connection.OpenAsync();
        
        var count = (int)command.ExecuteScalar();
        
        await connection.CloseAsync();
        
        return count > 0;
    }

    public SqlConnection OpenDatabase()
    {
        return new SqlConnection(ConnectionString);
    }

    public async Task CreateDatabaseAsync()
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        connection.ChangeDatabase(DatabaseName);
        await using var command = new SqlCommand(SqlQueries.CreateTableQuery, connection);
        
        command.ExecuteNonQuery();
        await connection.CloseAsync();
    }
    
    public async Task TruncateAsync(string tableName)
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        var checkDatabaseQuery = string.Format(SqlQueries.TruncateQuery, tableName);
        await using var checkCommand = new SqlCommand(checkDatabaseQuery, connection);
        
        checkCommand.ExecuteNonQuery();
        await connection.CloseAsync();
    }
}