using FileSorter.Constants;

namespace FileSorter.Models;

public class FileItem(int number, string text) : IComparable<FileItem>
{
    private int Number { get; } = number;
    private string Text { get; } = text;

    public int CompareTo(FileItem? other)
    {
        var compareResult = string.Compare(Text, other?.Text, StringComparison.InvariantCultureIgnoreCase);
        return compareResult == 0 ? Number.CompareTo(other?.Number) : compareResult;
    }

    public override string ToString()
    {
        return string.Concat(Number.ToString(), FileConstants.FileItemSeparator, Text);
    }
}