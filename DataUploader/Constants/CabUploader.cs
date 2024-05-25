namespace DataUploader.Constants;

public static class CabUploader
{
    public const string Cab = nameof(Cab);
    
    public const string CabFileName = "sample-cab-data.csv";
    
    public const string DuplicatesFileName = "duplicates.csv";

    public const string FlagColumnName = "store_and_fwd_flag";
    
    public static readonly string[] AllowedColumns = 
    {
        "tpep_pickup_datetime",
        "tpep_dropoff_datetime",
        "passenger_count",
        "trip_distance",
        "store_and_fwd_flag",
        "PULocationID",
        "DOLocationID",
        "fare_amount",
        "tip_amount"
    };
}