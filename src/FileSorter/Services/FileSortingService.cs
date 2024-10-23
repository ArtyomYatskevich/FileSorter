using FileSorter.Constants;

namespace FileSorter.Services;

public class FileSortingService(ISortingService sortingService) : IFileSortingService
{
    private const int IntMaxLength = 10;
    private const int AvailableMemoryUsagePercentage = 70;
    private const int EstimatedLineSizeBytes = 5_000;
    private const int FileBufferSizeBytes = 50_000;
    private const string TempDirectory = "temp";
    private const string OutputFile = "output.txt";
    
    public async Task SortAsync(string inputFile)
    {
        ValidateFile(inputFile);
        
        try
        {
            var sortedFiles = await SplitAndSortFilesAsync(inputFile);
            await MergeSortedFilesAsync(sortedFiles);
        }
        finally
        {
            DeleteTempFiles();
        }
    }
    
    #region private methods

    private async Task<List<string>> SplitAndSortFilesAsync(string inputFile)
    {
        await using var fs = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.None, FileBufferSizeBytes, FileOptions.SequentialScan);
        await using var bs = new BufferedStream(fs, FileBufferSizeBytes);
        using var reader = new StreamReader(bs);
        var chunkSize = CalculateOptimalChunkSize();
        var sortedFiles = new List<string>();
        var sortAndSaveTasks = new List<Task<string>>();
        var iteration = 0;
        Directory.CreateDirectory(TempDirectory);
            
        while (!reader.EndOfStream)
        {
            var counter = 0;
            var array = new string[chunkSize];

            while (await reader.ReadLineAsync() is { } line && counter < chunkSize)
            {
                array[counter++] = ReconstructLine(line);
            }

            if (reader.EndOfStream)
            {
                Array.Resize(ref array, counter);
                if (iteration == 0)
                {
                    SortAndSaveChunk(array, iteration, true);
                    return sortedFiles;
                }
            }

            sortAndSaveTasks.Add(Task<string>.Factory.StartNew(() => SortAndSaveChunk(array, iteration++)));
        }

        if (iteration == 0) return sortedFiles;
            
        Task.WaitAll(sortAndSaveTasks.Where(x => !x.IsCompleted).ToArray<Task>());
        sortedFiles.AddRange(sortAndSaveTasks.Select(t => t.Result));
        
        return sortedFiles;
    }
    
    private void ValidateFile(string inputFile)
    {
        if (!File.Exists(inputFile))
        {
            var message = $"File '{inputFile}' does not exist.";
            Console.WriteLine(message);
            throw new ArgumentException(message);
        }
    }
    
    private string SortAndSaveChunk(string[] lines, int iteration, bool isSingleIteration = false)
    {
        sortingService.Sort(lines);

        if (isSingleIteration)
        {
            File.WriteAllLines(OutputFile, lines.Select(x => ReconstructLine(x, true)));
            return OutputFile;
        }
        
        var fileName = $"{TempDirectory}/temp_{iteration}.tmp";
        
        File.WriteAllLines(fileName, lines);

        return fileName;
    }

    private async Task MergeSortedFilesAsync(List<string> sortedFiles)
    {
        if (!sortedFiles.Any()) return;
        
        var bufferSize = FileBufferSizeBytes;
        var data = new PriorityQueue<int, string>(StringComparer.InvariantCultureIgnoreCase);
        var readers = sortedFiles.Select((fileName, fileIndex) =>
        {
            var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize, FileOptions.SequentialScan);
            var bufferStream = new BufferedStream(fileStream, bufferSize);
            var streamReader = new StreamReader(bufferStream);
            var line = streamReader.ReadLine();
            if (line is not null) data.Enqueue(fileIndex, line);
            return streamReader;
        }).ToList();
        
        await using var fileStream = new FileStream(OutputFile, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize);
        await using var bufferStream = new BufferedStream(fileStream, bufferSize);
        await using var streamWriter = new StreamWriter(bufferStream);
        while (data.TryDequeue(out var currentFileIndex, out var currentLine))
        {
            await streamWriter.WriteLineAsync(ReconstructLine(currentLine, true));
            var currentReader = readers[currentFileIndex];
            if (currentReader.EndOfStream)
            {
                currentReader.Dispose();
                continue;
            }
            
            var line = await currentReader.ReadLineAsync();
            if (line is not null) data.Enqueue(currentFileIndex, line);
        }
    }

    private void DeleteTempFiles()
    {
        Directory.Delete(TempDirectory, true);
    }

    private int CalculateOptimalChunkSize()
    {
        var memoryInfo = GC.GetGCMemoryInfo();
        var availableMemory = memoryInfo.TotalAvailableMemoryBytes;
        var maxChunkMemory = availableMemory * AvailableMemoryUsagePercentage / 100;

        var estimatedChunkSize = (int)(maxChunkMemory / EstimatedLineSizeBytes);
        return estimatedChunkSize / Environment.ProcessorCount;
    }
    
    private string ReconstructLine(string line, bool isReverse = false)
    {
        var items = line.Split(FileConstants.FileItemSeparator, StringSplitOptions.RemoveEmptyEntries);
        if (items.Length != 2)
        {
            var message = $"Incorrect line format - '{line}'";
            Console.WriteLine(message);
            throw new ArgumentException(message);
        }

        return isReverse
            ? $"{items[1].Trim(FileConstants.PaddingChar)}{FileConstants.FileItemSeparator}{items[0]}"
            : $"{items[1]}{FileConstants.FileItemSeparator}{items[0].PadLeft(IntMaxLength, FileConstants.PaddingChar)}";
    }
    
    #endregion
}