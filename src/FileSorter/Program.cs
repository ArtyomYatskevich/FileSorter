using System.Diagnostics;
using FileSorter.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FileSorter;

public static class FileSorter
{
    private static async Task Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: <input_file>");
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var inputFile = args[0];

        var serviceProvider = ConfigureServices();
        var sortingService = serviceProvider.GetRequiredService<IFileSortingService>();
        await sortingService.SortAsync(inputFile);
        
        stopwatch.Stop();
        Console.WriteLine($"Sorting completed in {stopwatch.Elapsed.TotalSeconds:F2} seconds");
    }
    
    #region private methods

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddScoped<ISortingService, QuickSortingService>();
        services.AddScoped<IFileSortingService, FileSortingService>();

        return services.BuildServiceProvider();
    }
    
    #endregion
}