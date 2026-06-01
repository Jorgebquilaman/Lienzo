using FluentAssertions;
using Lienzo.Domain.ValueObjects;
using Xunit;

namespace Lienzo.Domain.Tests;

public class TimeRangeTests
{
    [Fact]
    public void Constructor_ValidTimes_CreatesInstance()
    {
        var range = new TimeRange(new TimeOnly(8, 0), new TimeOnly(10, 0));

        range.Start.Should().Be(new TimeOnly(8, 0));
        range.End.Should().Be(new TimeOnly(10, 0));
    }

    [Fact]
    public void Constructor_StartEqualsEnd_ThrowsException()
    {
        var act = () => new TimeRange(new TimeOnly(10, 0), new TimeOnly(10, 0));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_StartAfterEnd_ThrowsException()
    {
        var act = () => new TimeRange(new TimeOnly(10, 0), new TimeOnly(8, 0));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void OverlapsWith_OverlappingRanges_ReturnsTrue()
    {
        var range1 = new TimeRange(new TimeOnly(8, 0), new TimeOnly(10, 0));
        var range2 = new TimeRange(new TimeOnly(9, 0), new TimeOnly(11, 0));

        var result = range1.OverlapsWith(range2);

        result.Should().BeTrue();
    }

    [Fact]
    public void OverlapsWith_NonOverlappingRanges_ReturnsFalse()
    {
        var range1 = new TimeRange(new TimeOnly(8, 0), new TimeOnly(10, 0));
        var range2 = new TimeRange(new TimeOnly(10, 0), new TimeOnly(12, 0));

        var result = range1.OverlapsWith(range2);

        result.Should().BeFalse();
    }

    [Fact]
    public void OverlapsWith_ContainedRange_ReturnsTrue()
    {
        var range1 = new TimeRange(new TimeOnly(8, 0), new TimeOnly(12, 0));
        var range2 = new TimeRange(new TimeOnly(9, 0), new TimeOnly(11, 0));

        var result = range1.OverlapsWith(range2);

        result.Should().BeTrue();
    }

    [Fact]
    public void OverlapsWith_AdjacentRanges_ReturnsFalse()
    {
        var range1 = new TimeRange(new TimeOnly(8, 0), new TimeOnly(10, 0));
        var range2 = new TimeRange(new TimeOnly(10, 0), new TimeOnly(12, 0));

        var result = range1.OverlapsWith(range2);

        result.Should().BeFalse();
    }
}
