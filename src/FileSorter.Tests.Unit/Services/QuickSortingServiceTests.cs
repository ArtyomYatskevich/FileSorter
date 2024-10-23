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
        var lines = new[]
        {
            "415. Apple",
            "30432. Something something something",
            "1. Apple",
            "32. Cherry is the best",
            "2. Banana is yellow"
        };
        
        _quickSortingService.Sort(lines);

        // Assert
        Assert.Equal("1. Apple", lines[0]);
        Assert.Equal("415. Apple", lines[1]);
        Assert.Equal("2. Banana is yellow", lines[2]);
        Assert.Equal("32. Cherry is the best", lines[3]);
        Assert.Equal("30432. Something something something", lines[4]);
    }
    
    // TODO: Add more tests
}