using System.Collections.Generic;
using System.Linq;
using YAERP.Infrastructure.Search;

namespace YAERP.UI.Services;

public class CommandPaletteService : ICommandPaletteService
{
    private readonly TrieSearchEngine<PaletteCommandItem> _trie = new();
    private readonly List<PaletteCommandItem> _allItems = new();

    public void RegisterCommand(PaletteCommandItem item)
    {
        _allItems.Add(item);
        IndexItem(item);
    }

    public void UnregisterCommand(PaletteCommandItem item)
    {
        _allItems.Remove(item);
        RemoveItemIndex(item);
    }

    private void IndexItem(PaletteCommandItem item)
    {
        // Index by title words
        var titleWords = item.Title.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in titleWords)
        {
            _trie.Add(word, item);
        }

        // Index by category
        _trie.Add(item.Category, item);
    }

    private void RemoveItemIndex(PaletteCommandItem item)
    {
        var titleWords = item.Title.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in titleWords)
        {
            _trie.Remove(word, item);
        }

        _trie.Remove(item.Category, item);
    }

    public IReadOnlyList<PaletteCommandItem> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return GetAllCommands();

        var words = query.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

        if (words.Length == 0)
            return GetAllCommands();

        // Start with the set from the first word
        var resultSet = new HashSet<PaletteCommandItem>(_trie.Search(words[0]));

        // Intersect with sets from subsequent words (AND logic)
        for (int i = 1; i < words.Length; i++)
        {
            var wordResults = _trie.Search(words[i]);
            resultSet.IntersectWith(wordResults);
        }

        return resultSet.ToList();
    }

    public IReadOnlyList<PaletteCommandItem> GetAllCommands()
    {
        return _allItems.ToList();
    }
}
