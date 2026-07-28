using System;
using System.Collections.Generic;
using System.Linq;

namespace YAERP.Infrastructure.Search;

public class TrieSearchEngine<T>
{
    private class TrieNode
    {
        public Dictionary<char, TrieNode> Children { get; } = new();
        public HashSet<T> Items { get; } = new();
    }

    private readonly TrieNode _root = new();

    public void Add(string key, T item)
    {
        if (string.IsNullOrWhiteSpace(key)) return;

        var node = _root;
        foreach (var c in key.ToLowerInvariant())
        {
            if (!node.Children.TryGetValue(c, out var child))
            {
                child = new TrieNode();
                node.Children[c] = child;
            }
            node = child;
            node.Items.Add(item); // Store item at every prefix node
        }
    }

    public void Remove(string key, T item)
    {
        if (string.IsNullOrWhiteSpace(key)) return;

        var node = _root;
        foreach (var c in key.ToLowerInvariant())
        {
            if (!node.Children.TryGetValue(c, out node))
            {
                return;
            }
            node.Items.Remove(item);
        }
    }

    public void Clear()
    {
        _root.Children.Clear();
        _root.Items.Clear();
    }

    public IEnumerable<T> Search(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix)) return Array.Empty<T>();

        var node = _root;
        foreach (var c in prefix.ToLowerInvariant())
        {
            if (!node.Children.TryGetValue(c, out node))
            {
                return Array.Empty<T>(); // Prefix not found
            }
        }

        return node.Items;
    }
}
