using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace coolbeats.scripts.staticScriptsAndDataStructures
{
    public class multiDictionary<TKey, TValue> where TKey : IComparable
    {
        public List<(SortedSet<TKey>, TValue)> keysValuePairs;
        public multiDictionary (List<(SortedSet<TKey>, TValue)> input)
        {
            keysValuePairs = input;
        }
        public IEnumerable<TValue> get(IEnumerable<TKey> keys)
        {
            List<TValue> output = new List<TValue>();
            foreach ((SortedSet<TKey>, TValue) pair in keysValuePairs)
            {
                IEnumerator<TKey> subset = pair.Item1.GetEnumerator();
                foreach (TKey key in keys)
                {
                    if (key.CompareTo(subset.Current) > 0)
                    {
                        break;
                    }
                    if (key.Equals(subset.Current))
                    {
                        if (!subset.MoveNext())
                        {
                            output.Add(pair.Item2);
                        }
                    }
                }
            }
            return output;
        }
    }
}