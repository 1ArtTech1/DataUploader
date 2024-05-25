using System.Globalization;
using DataUploader.Providers;
using DataUploader.Providers.Interfaces;
using DataUploader.Services;
using DataUploader.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DataUploader;

public static class Program
{
    public static async Task Main()
    {
        var serviceProvider = SetDependencyInjection();
        
        var databaseProvider = serviceProvider.GetService<IDatabaseProvider>();
        var cabService = serviceProvider.GetService<ICabService>();
        
        if (await databaseProvider!.IsDatabaseNotExistsAsync())
        {
            await databaseProvider.CreateDatabaseAsync();
        }

        var createdDataTable = await cabService!.CreateCabsAsync();

        Console.WriteLine(createdDataTable.Rows.Count > (int)default!
            ? $"Successfully created Cabs data. Count of the unique records: {createdDataTable.Rows.Count}."
            : "Something went wrong.");
    }

    private static ServiceProvider SetDependencyInjection()
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IDatabaseProvider>(_ => new DatabaseProvider())
            .AddScoped<ICabService, CabService>()
            .BuildServiceProvider();
        
        var culture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        return serviceProvider;
    }
}