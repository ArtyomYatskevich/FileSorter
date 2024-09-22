using FileSorter.Models;

namespace FileSorter.Services;

public class QuickSortingService : ISortingService
{
    public void Sort(List<FileItem> lines)
    {
        Sort(lines, 0, lines.Count - 1);
    }

    #region private methods
    
    private void Sort(List<FileItem> lines, int low, int high)
    {
        if (low >= high) return;

        var pivotIndex = MedianOfThree(lines, low, high);

        pivotIndex = Partition(lines, low, high, pivotIndex);
        Sort(lines, low, pivotIndex - 1);
        Sort(lines, pivotIndex + 1, high);
    }

    private int Partition(List<FileItem> lines, int low, int high, int pivotIndex)
    {
        var pivotValue = lines[pivotIndex];
        Swap(lines, pivotIndex, high);
        var storeIndex = low;

        for (var i = low; i < high; i++)
        {
            if (lines[i].CompareTo(pivotValue) >= 0) continue;
            
            Swap(lines, i, storeIndex);
            storeIndex++;
        }

        Swap(lines, storeIndex, high);
        return storeIndex;
    }

    private int MedianOfThree(List<FileItem> lines, int low, int high)
    {
        var mid = low + (high - low) / 2;

        if (lines[low].CompareTo(lines[mid]) > 0) Swap(lines, low, mid);
        if (lines[low].CompareTo(lines[high]) > 0) Swap(lines, low, high);
        if (lines[mid].CompareTo(lines[high]) > 0) Swap(lines, mid, high);

        return mid;
    }

    private void Swap(List<FileItem> lines, int i, int j)
    {
        (lines[i], lines[j]) = (lines[j], lines[i]);
    }
    
    #endregion
}