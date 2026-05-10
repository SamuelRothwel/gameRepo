using coolbeats.scripts.staticScriptsAndDataStructures;
using Xunit;

namespace coolbeats.scripts.staticScriptsAndDataStructures.test;

public class MortenEncodingTests
{
    [Theory]
    [InlineData(0u, 0u, 0u)]
    [InlineData(1u, 0u, 1u)]
    [InlineData(0u, 1u, 2u)]
    [InlineData(1u, 1u, 3u)]
    [InlineData(2u, 3u, 14u)]
    public void EncodeInterleavesLowBits(uint x, uint y, uint expected)
    {
        Assert.Equal(expected, MortenEncoding.encode(x, y));
    }

    [Fact]
    public void EncodeUsesSixteenBitsFromEachCoordinate()
    {
        Assert.Equal(0x55555555u, MortenEncoding.encode(0xFFFFu, 0u));
        Assert.Equal(0xAAAAAAAAu, MortenEncoding.encode(0u, 0xFFFFu));
        Assert.Equal(0xFFFFFFFFu, MortenEncoding.encode(0xFFFFu, 0xFFFFu));
    }

    [Fact]
    public void EncodeIgnoresCoordinateBitsThatDoNotFitInUintResult()
    {
        Assert.Equal(0u, MortenEncoding.encode(0x1_0000u, 0x1_0000u));
    }
}
