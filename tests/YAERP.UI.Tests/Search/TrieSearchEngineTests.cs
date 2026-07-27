using System.Linq;
using Xunit;
using YAERP.Infrastructure.Search;

namespace YAERP.UI.Tests.Search;

public class TrieSearchEngineTests
{
    [Fact]
    public void AddAndSearch_ShouldReturnMatches()
    {
        var trie = new TrieSearchEngine<string>();
        trie.Add("Customer", "Command1");
        trie.Add("Custom", "Command2");
        trie.Add("Inventory", "Command3");

        var results = trie.Search("Cust").ToList();

        Assert.Equal(2, results.Count);
        Assert.Contains("Command1", results);
        Assert.Contains("Command2", results);
    }

    [Fact]
    public void Search_ShouldBeCaseInsensitive()
    {
        var trie = new TrieSearchEngine<string>();
        trie.Add("Invoice", "Command1");

        var results = trie.Search("inv").ToList();

        Assert.Single(results);
        Assert.Contains("Command1", results);
    }
}
