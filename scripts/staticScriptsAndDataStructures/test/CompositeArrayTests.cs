using System;
using System.Linq;
using coolbeats.scripts.staticScriptsAndDataStructures;
using Xunit;

namespace coolbeats.scripts.staticScriptsAndDataStructures.test;

public class CompositeArrayTests
{
    [Fact]
    public void LengthSumsAllSourceArrays()
    {
        var array = new compositeArray<int>(new[] { 1, 2 }, Array.Empty<int>(), new[] { 3, 4, 5 });

        Assert.Equal(5, array.Length);
    }

    [Fact]
    public void IndexerReadsAcrossArrayBoundaries()
    {
        var array = new compositeArray<string>(new[] { "a", "b" }, new[] { "c" });

        Assert.Equal("a", array[0]);
        Assert.Equal("b", array[1]);
        Assert.Equal("c", array[2]);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void IndexerThrowsWhenIndexIsOutsideCompositeRange(int index)
    {
        var array = new compositeArray<int>(new[] { 1 }, new[] { 2, 3 });

        Assert.Throws<IndexOutOfRangeException>(() => array[index]);
    }

    [Fact]
    public void EnumerationPreservesSourceArrayOrder()
    {
        var array = new compositeArray<int>(new[] { 1, 2 }, Array.Empty<int>(), new[] { 3 });

        Assert.Equal(new[] { 1, 2, 3 }, array.Cast<int>());
    }
}
