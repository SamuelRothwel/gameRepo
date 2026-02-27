using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class FallbackDictionary<TKey, TValue> : FallbackDictionary<TKey, TKey, TValue> {}
public class FallbackDictionary<TAccessor, TKey, TValue>
{
    TAccessor currentDictionary;
    TAccessor fallback;
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
    public void SetDefault(TAccessor accessor)
    {
        fallback = accessor;
        currentDictionary = fallback;
    }
    public void Switch(TAccessor accessor)
    {
        currentDictionary = accessor;
    }
    public void Add(TAccessor accessor, Dictionary<TKey, TValue> newDict)
    {
        dictionarySet[accessor] = newDict;
    }
    public TValue this[TKey key]
    {
        get { if (dictionarySet[currentDictionary].ContainsKey(key)) 
            return dictionarySet[currentDictionary][key];
            else return dictionarySet[fallback][key]; }
        set {dictionarySet[currentDictionary][key] = value;}
    }
}