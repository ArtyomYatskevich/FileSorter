namespace FileSorter.Services;

public class QuickSortingService : ISortingService
{
    private const StringComparison DefaultStringComparison = StringComparison.InvariantCultureIgnoreCase;
    
    public void Sort(string[] lines)
    {
        Sort(lines, 0, lines.Length - 1);
    }

    #region private methods
    
    private void Sort(string[] lines, int low, int high)
    {
        if (low >= high) return;

        var pivotIndex = MedianOfThree(lines, low, high);

        pivotIndex = Partition(lines, low, high, pivotIndex);
        Sort(lines, low, pivotIndex - 1);
        Sort(lines, pivotIndex + 1, high);
    }

    private int Partition(string[] lines, int low, int high, int pivotIndex)
    {
        var pivotValue = lines[pivotIndex];
        Swap(lines, pivotIndex, high);
        var storeIndex = low;

        for (var i = low; i < high; i++)
        {
            if (string.Compare(lines[i], pivotValue, DefaultStringComparison) >= 0) continue;
            
            Swap(lines, i, storeIndex);
            storeIndex++;
        }

        Swap(lines, storeIndex, high);
        return storeIndex;
    }

    private int MedianOfThree(string[] lines, int low, int high)
    {
        var mid = low + (high - low) / 2;

        if (string.Compare(lines[low], lines[mid], DefaultStringComparison) > 0) Swap(lines, low, mid);
        if (string.Compare(lines[low], lines[high], DefaultStringComparison) > 0) Swap(lines, low, high);
        if (string.Compare(lines[mid], lines[high], DefaultStringComparison) > 0) Swap(lines, mid, high);

        return mid;
    }

    private void Swap(string[] lines, int i, int j)
    {
        (lines[i], lines[j]) = (lines[j], lines[i]);
    }
    
    #endregion
}