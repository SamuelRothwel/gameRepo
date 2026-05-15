using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class FallbackDictionary<TKey, TValue> : FallbackDictionary<TKey, TKey, TValue> {}
public class FallbackDictionary<TAccessor, TKey, TValue>
{
    TAccessor currentDictionary;
    TAccessor fallback;
    readonly Dictionary<TAccessor, TAccessor> fallbacks = new();
    public Dictionary<TAccessor, Dictionary<TKey, TValue>> dictionarySet;
    public FallbackDictionary(Dictionary<TAccessor, Dictionary<TKey, TValue>> set)
    {
        dictionarySet = set;
        fallback = set.Keys.First();
        currentDictionary = fallback;
    }
    public FallbackDictionary()
    {
        dictionarySet = new Dictionary<TAccessor, Dictionary<TKey, TValue>>();
    }
    public void Add(TAccessor accessor, Dictionary<TKey, TValue> newDict)
    {
        dictionarySet[accessor] = newDict;
    }
    public void Add(TAccessor accessor, Dictionary<TKey, TValue> newDict, TAccessor fallbackAccessor)
    {
        Add(accessor, newDict);
        fallbacks[accessor] = fallbackAccessor;
    }
    public void SetDefault(TAccessor accessor)
    {
        fallback = accessor;
        currentDictionary = fallback;
    }
    public void Switch(TAccessor accessor)
    {
        currentDictionary = accessor;
    }
    public TAccessor GetState()
    {
        return currentDictionary;
    }
    public TValue this[TKey key]
    {
        get { return GetValue(currentDictionary, key, new HashSet<TAccessor>()); }
        set {dictionarySet[currentDictionary][key] = value;}
    }
    TValue GetValue(TAccessor accessor, TKey key, HashSet<TAccessor> visited)
    {
        if (!visited.Add(accessor))
        {
            throw new InvalidOperationException("Cyclic fallback state: " + accessor);
        }
        if (dictionarySet[accessor].TryGetValue(key, out TValue value))
        {
            return value;
        }
        if (fallbacks.TryGetValue(accessor, out TAccessor parent))
        {
            return GetValue(parent, key, visited);
        }
        if (!EqualityComparer<TAccessor>.Default.Equals(accessor, fallback))
        {
            return GetValue(fallback, key, visited);
        }
        throw new KeyNotFoundException(key.ToString());
    }
}
