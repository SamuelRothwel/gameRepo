using System.Collections.Generic;
using Xunit;

namespace coolbeats.scripts.staticScriptsAndDataStructures.test;

public class FallbackDictionaryTests
{
    [Fact]
    public void IndexerReturnsCurrentDictionaryValueWhenKeyExists()
    {
        var dictionary = CreateDictionary();

        dictionary.Switch("combat");

        Assert.True(dictionary["canAttack"]);
    }

    [Fact]
    public void IndexerFallsBackToDefaultDictionaryWhenCurrentStateOmitsKey()
    {
        var dictionary = CreateDictionary();

        dictionary.Switch("combat");

        Assert.False(dictionary["isPaused"]);
    }

    [Fact]
    public void SetterWritesOnlyToCurrentDictionary()
    {
        var dictionary = CreateDictionary();

        dictionary.Switch("combat");
        dictionary["isPaused"] = true;

        Assert.True(dictionary["isPaused"]);

        dictionary.Switch("menu");
        Assert.False(dictionary["isPaused"]);
    }

    [Fact]
    public void IndexerThrowsWhenKeyIsMissingFromCurrentAndFallbackDictionaries()
    {
        var dictionary = CreateDictionary();

        dictionary.Switch("combat");

        Assert.Throws<KeyNotFoundException>(() => dictionary["missing"]);
    }

    [Fact]
    public void IndexerFallsBackThroughNamedParentState()
    {
        var dictionary = CreateDictionary();
        dictionary.Add("activeCombat", new Dictionary<string, bool>
        {
            ["gameActive"] = true,
        }, "combat");

        dictionary.Switch("activeCombat");

        Assert.True(dictionary["gameActive"]);
        Assert.True(dictionary["canAttack"]);
        Assert.False(dictionary["isPaused"]);
    }

    private static FallbackDictionary<string, bool> CreateDictionary()
    {
        var dictionary = new FallbackDictionary<string, bool>();
        dictionary.Add("menu", new Dictionary<string, bool>
        {
            ["canAttack"] = false,
            ["isPaused"] = false,
        });
        dictionary.Add("combat", new Dictionary<string, bool>
        {
            ["canAttack"] = true,
        });
        dictionary.SetDefault("menu");
        return dictionary;
    }
}
