using Microsoft.Data.SqlClient;

namespace DataUploader.Providers.Interfaces;

public interface IDatabaseProvider
{
    Task<bool> IsDatabaseNotExistsAsync();

    Task CreateDatabaseAsync();

    SqlConnection OpenDatabase();

    Task TruncateAsync(string tableName);
}