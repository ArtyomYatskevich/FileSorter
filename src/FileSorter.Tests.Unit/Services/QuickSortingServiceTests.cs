using FileSorter.Models;
using FileSorter.Services;
using Xunit;

namespace FileSorter.Tests.Unit.Services;

public class QuickSortingServiceTests
{
    private readonly QuickSortingService _quickSortingService = new();
    
    [Fact]
    public void Sort_SortsCorrectly()
    {
        // Arrange
        var lines = new List<FileItem>
        {
            new(415, "Apple"),
            new(30432, "Something something something"),
            new(1, "Apple"),
            new(32, "Cherry is the best"),
            new(2, "Banana is yellow")
        };
        
        _quickSortingService.Sort(lines);

        // Assert
        Assert.Equal("1. Apple", lines[0].ToString());
        Assert.Equal("415. Apple", lines[1].ToString());
        Assert.Equal("2. Banana is yellow", lines[2].ToString());
        Assert.Equal("32. Cherry is the best", lines[3].ToString());
        Assert.Equal("30432. Something something something", lines[4].ToString());
    }
    
    // TODO: Add more tests
}