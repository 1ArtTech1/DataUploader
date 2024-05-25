namespace DataUploader.Constants;

public static class SqlQueries
{
    public const string CreateTableQuery = @"
    CREATE TABLE Cab
    (
        tpep_pickup_datetime DATETIME,
        tpep_dropoff_datetime DATETIME,
        passenger_count INT,
        trip_distance DECIMAL(10, 2),
        store_and_fwd_flag VARCHAR(3),
        PULocationID INT,
        DOLocationID INT,
        fare_amount DECIMAL(10, 2),
        tip_amount DECIMAL(10, 2),
        travel_time_seconds AS DATEDIFF(SECOND, tpep_pickup_datetime, tpep_dropoff_datetime)
    );

    CREATE CLUSTERED INDEX IX_PULocationID ON Cab (PULocationID);
    CREATE INDEX idx_tip_amount ON Cab (tip_amount);
    CREATE INDEX idx_trip_distance ON Cab (trip_distance);
    CREATE INDEX idx_pickup_datetime ON Cab (tpep_pickup_datetime);
    CREATE INDEX idx_dropoff_datetime ON Cab (tpep_dropoff_datetime);
";

    public static string ExistDatabaseQuery = "SELECT COUNT(*) FROM master.sys.tables WHERE name = '{0}' AND schema_id = SCHEMA_ID('dbo')";
    
    public const string TruncateQuery = "TRUNCATE TABLE {0}";
}