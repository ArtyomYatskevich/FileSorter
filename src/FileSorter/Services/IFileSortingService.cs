namespace FileSorter.Services;

public interface IFileSortingService
{
    Task SortAsync(string inputFile);
}