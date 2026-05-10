using coolbeats.scripts.staticScriptsAndDataStructures;
using Xunit;

namespace coolbeats.scripts.staticScriptsAndDataStructures.test;

public class CircularEnumeratorTests
{
    [Fact]
    public void CurrentUsesStartIndex()
    {
        var values = new[] { "first", "second", "third" };
        var enumerator = new CircularEnumerator<string>(ref values, 1);

        Assert.Equal("second", enumerator.Current);
    }

    [Fact]
    public void MoveNextWrapsToBeginning()
    {
        var values = new[] { 10, 20 };
        var enumerator = new CircularEnumerator<int>(ref values, 1);

        enumerator.MoveNext();

        Assert.Equal(10, enumerator.Current);
    }

    [Fact]
    public void LoopReturnsEachItemOnceStartingAtCurrentPosition()
    {
        var values = new[] { "north", "east", "south", "west" };
        var enumerator = new CircularEnumerator<string>(ref values, 2);

        Assert.Equal(new[] { "south", "west", "north", "east" }, enumerator.loop());
        Assert.Equal("south", enumerator.Current);
    }
}
