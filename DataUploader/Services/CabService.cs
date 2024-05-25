using System.Data;
using DataUploader.Constants;
using DataUploader.Helpers;
using DataUploader.Providers.Interfaces;
using DataUploader.Services.Interfaces;
using Microsoft.Data.SqlClient;

namespace DataUploader.Services;

public class CabService : ICabService
{
    private readonly IDatabaseProvider databaseProvider;

    public CabService(IDatabaseProvider databaseProvider)
    {
        this.databaseProvider = databaseProvider ?? throw new ArgumentNullException(nameof(databaseProvider));
    }
    
    public async Task<DataTable> CreateCabsAsync()
    {
        await using var connection = databaseProvider.OpenDatabase();
        await connection.OpenAsync();

        using var reader = GetStreamReader();
        
        await databaseProvider.TruncateAsync(CabUploader.Cab);
        await using var transaction = connection.BeginTransaction();

        var dataTable = new DataTable();
        
        try
        {
            using (var sqlBulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.KeepNulls, transaction))
            {
                sqlBulk.DestinationTableName = CabUploader.Cab;
                dataTable = await UploadDataHelper.GetDataTableAsync(reader.BaseStream);

                await sqlBulk.WriteToServerAsync(dataTable);
            }

            transaction.Commit();
            await connection.CloseAsync();

            return dataTable;

        }
        catch(Exception)
        {
            transaction.Rollback();
            await connection.CloseAsync();
            
            throw;
        }
    }

    private static StreamReader GetStreamReader()
    {
        var cabFilePath = GetCabFilePath();
        
        return new StreamReader(cabFilePath);
    }

    private static string GetCabFilePath()
    {
        var currentDirectory  = Environment.CurrentDirectory;
        var filePath = Path.Combine(currentDirectory, CabUploader.CabFileName);
        
        return filePath;
    }
}