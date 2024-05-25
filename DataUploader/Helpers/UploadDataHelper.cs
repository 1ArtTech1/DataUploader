using System.Data;
using System.Globalization;
using CsvHelper;
using DataUploader.Constants;

namespace DataUploader.Helpers;

public static class UploadDataHelper
{
    public static async Task<DataTable> GetDataTableAsync(Stream fileStream)
    {
        using var reader = new StreamReader(fileStream);
        using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
        using var csvDataReader = new CsvDataReader(csvReader);
        
        var dataTable = new DataTable();
        dataTable.Load(csvDataReader);
        
        foreach (DataColumn column in dataTable.Columns)
        {
            column.ReadOnly = false;
        }
        
        RemoveColumns(dataTable);
        RemoveEmptyValues(dataTable);
        await GetDuplicatesDataAsync(dataTable);
        HandleData(dataTable);
        
        return dataTable;
    }
    
    private static async Task GetDuplicatesDataAsync(DataTable dataTable)
    {
        var duplicateRows = GetDuplicates(dataTable).ToArray();
        var columnNames = GetNames(dataTable).ToArray();
        
        await SaveDuplicatesDataAsync(columnNames, duplicateRows, CabUploader.DuplicatesFileName);
        RemoveDuplicateRows(dataTable, duplicateRows);
    }

    private static void RemoveColumns(DataTable dataTable)
    {
        var unnecessaryColumns = new List<DataColumn>();
        
        foreach (DataColumn column in dataTable.Columns)
        {
            if (!CabUploader.AllowedColumns.Any(allowedColumn => allowedColumn.Equals(column.ColumnName)))
            {
                unnecessaryColumns.Add(column);
            }
        }

        foreach (var column in unnecessaryColumns)
        {
            dataTable.Columns.Remove(column);
        }
    }

    private static void RemoveEmptyValues(DataTable dataTable)
    {
        var incorrectRows = new List<DataRow>();
        
        foreach (DataRow row in dataTable.Rows)
        {
            if (row.ItemArray.Any(item => string.IsNullOrEmpty(item?.ToString())))
            {
                incorrectRows.Add(row);
            }
        }

        foreach (var incorrectRow in incorrectRows)
        {
            dataTable.Rows.Remove(incorrectRow);
        }
    }

    private static IEnumerable<DataRow> GetDuplicates(DataTable dataTable)
    {
        var duplicateData = new List<DataRow>();
        var uniqueData = new HashSet<string>();
        
        foreach (DataRow row in dataTable.Rows)
        {
            var record = $"{row["tpep_pickup_datetime"]}/{row["tpep_dropoff_datetime"]}/{row["passenger_count"]}";
            var isRecordUnique = uniqueData.Add(record);
            
            if (!isRecordUnique)
            {
                duplicateData.Add(row);
            }
        }
        
        return duplicateData;
    }

    private static void HandleData(DataTable dataTable)
    {
        foreach (DataRow row in dataTable.Rows)
        {
            TrimData(row);
            HandleFlagValue(row);
            ConvertDateTimeFieldsToUtc(row);
        }
    }

    private static void ConvertDateTimeFieldsToUtc(DataRow row)
    {
        foreach (DataColumn column in row.Table.Columns)
        {
            if (DateTime.TryParseExact(row[column].ToString(), "MM/dd/yyyy hh:mm:ss tt",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
            {
                var utcDateTime = dateTime.ToUniversalTime();
                row[column] = utcDateTime;
            }
        }
    }
    
    private static IEnumerable<string> GetNames(DataTable dataTable)
    {
        foreach (DataColumn dataColumn in dataTable.Columns)
        {
            yield return dataColumn.ColumnName;
        }
    }

    private static void RemoveDuplicateRows(DataTable dataTable, IEnumerable<DataRow> incorrectRows)
    {
        foreach (var incorrectRow in incorrectRows)
        {
            dataTable.Rows.Remove(incorrectRow);
        }
    }
    
    private static async Task SaveDuplicatesDataAsync(
        IEnumerable<string> columnNames,
        IEnumerable<DataRow> incorrectRows,
        string filePath)
    {
        await using var streamWriter = new StreamWriter(filePath);
        await streamWriter.WriteLineAsync(string.Join(",", columnNames));

        foreach (var incorrectRow in incorrectRows)
        {
            await streamWriter.WriteLineAsync(string.Join(",", incorrectRow.ItemArray));
        }
    }

    private static void TrimData(DataRow dataRow)
    {
        foreach (DataColumn column in dataRow.Table.Columns)
        {
            dataRow[column] = dataRow[column].ToString()?.Trim();
        }
    }

    private static void HandleFlagValue(DataRow row)
    {
        row[CabUploader.FlagColumnName] = row[CabUploader.FlagColumnName].ToString() switch
        {
            "N" => "No",
            "Y" => "Yes",
            _ => throw new ArgumentOutOfRangeException(nameof(CabUploader.FlagColumnName))
        };
    }
}