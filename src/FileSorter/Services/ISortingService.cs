using FileSorter.Models;

namespace FileSorter.Services;

public interface ISortingService
{
    void Sort(List<FileItem> lines);
}