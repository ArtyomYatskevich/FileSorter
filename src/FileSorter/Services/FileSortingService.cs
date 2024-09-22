using FileSorter.Constants;
using FileSorter.Models;

namespace FileSorter.Services;

public class FileSortingService(ISortingService sortingService) : IFileSortingService
{
    private const double AvailableMemoryUsagePercentage = 70;
    private const int EstimatedLineSizeBytes = 10000;
    private const int MinimumChunkSizeBytes = 100000;
    private const string OutputFile = "output.txt";

    private List<StreamReader> _readers = new();

    public async Task SortAsync(string inputFile)
    {
        ValidateFile(inputFile);
        
        var sortedFiles = new List<string>();

        try
        {
            using var reader = new StreamReader(inputFile);
            var lines = new List<FileItem>();
            var chunkSize = CalculateOptimalChunkSize();
            
            while (await reader.ReadLineAsync() is { } line)
            {
                lines.Add(SplitLine(line));

                if (lines.Count < chunkSize) continue;
                
                var sortedChunkFile = await SortAndSaveChunkAsync(lines);
                sortedFiles.Add(sortedChunkFile);
                lines.Clear();
            }

            if (lines.Count > 0)
            {
                var sortedChunkFile = await SortAndSaveChunkAsync(lines);
                sortedFiles.Add(sortedChunkFile);
            }

            await MergeSortedFiles(sortedFiles, OutputFile);
        }
        finally
        {
            await DeleteTempFilesAsync(sortedFiles);
        }
    }

    #region private methods

    private void ValidateFile(string inputFile)
    {
        if (!File.Exists(inputFile))
        {
            var message = $"File '{inputFile}' does not exist.";
            Console.WriteLine(message);
            throw new ArgumentException(message);
        }
    }
    
    private async Task<string> SortAndSaveChunkAsync(List<FileItem> lines)
    {
        sortingService.Sort(lines);
        
        var tempFile = Path.GetTempFileName();
        await using var writer = new StreamWriter(tempFile);
        foreach (var line in lines)
        {
            await writer.WriteLineAsync(line.ToString());
        }
        
        return tempFile;
    }

    private async Task MergeSortedFiles(List<string> sortedFiles, string outputFile)
    {
        try
        {
            InitializeReaders(sortedFiles);
            var data = new SortedSet<(string line, int fileIndex)>
                (Comparer<(string, int)>.Create((x, y) =>
                {
                    var firstItem = SplitLine(x.Item1);
                    var secondItem = SplitLine(y.Item1);

                    return firstItem.CompareTo(secondItem);
                }));

            await using var writer = new StreamWriter(outputFile);
            for (var fileIndex = 0; fileIndex < _readers.Count; fileIndex++)
            {
                await ReadNextFileLine(_readers, fileIndex, data);
            }

            while (data.Count > 0)
            {
                var (smallestLine, fileIndex) = data.Min;
                data.Remove(data.Min);
                
                await writer.WriteLineAsync(smallestLine);
                await ReadNextFileLine(_readers, fileIndex, data);
            }
        }
        finally
        {
            foreach (var reader in _readers)
            {
                reader.Dispose();
            }
        }
    }
    
    private void InitializeReaders(List<string> sortedFiles)
    {
        _readers = sortedFiles.Select(file => new StreamReader(file)).ToList();
    }

    private async Task ReadNextFileLine(List<StreamReader> readers, int fileIndex, SortedSet<(string line, int fileIndex)> heap)
    {
        if (readers[fileIndex].EndOfStream) return;
            
        var nextLine = await readers[fileIndex].ReadLineAsync();
        if (nextLine is not null) heap.Add((nextLine, fileIndex));
    }

    private FileItem SplitLine(string line)
    {
        var items = line.Split(FileConstants.FileItemSeparator, StringSplitOptions.RemoveEmptyEntries);
        if (items.Length != 2)
        {
            var message = $"Incorrect line format - '{line}'";
            Console.WriteLine(message);
            throw new ArgumentException(message);
        }

        if (!int.TryParse(items[0], out var number))
        {
            var message = $"Couldn't parse number - '{items[0]}'. Incorrect line format - '{line}'";
            Console.WriteLine(message);
            throw new ArgumentException(message);
        }
        
        return new FileItem(number, items[1]);
    }

    private async Task DeleteTempFilesAsync(List<string> files)
    {
        var deletionTasks = files.Select(file => Task.Run(() => File.Delete(file))).ToArray();
        await Task.WhenAll(deletionTasks);
    }

    private int CalculateOptimalChunkSize()
    {
        var memoryInfo = GC.GetGCMemoryInfo();
        var availableMemory = memoryInfo.TotalAvailableMemoryBytes;
        var maxChunkMemory = availableMemory * AvailableMemoryUsagePercentage / 100;

        var estimatedChunkSize = (int)(maxChunkMemory / EstimatedLineSizeBytes);
        return Math.Max(estimatedChunkSize, MinimumChunkSizeBytes);
    }
    
    #endregion
}