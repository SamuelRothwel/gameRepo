using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coolbeats.scripts.staticScriptsAndDataStructures
{
    public class CircularEnumerator<T> : IEnumerable<T>
    {
        public T[] array;
        public int index;
        public int Count { get; }
        public T Current => array[index];
        public CircularEnumerator(ref T[] values, int start = 0)
        {
            array = values;
            index = start;
            Count = values.Count();
        }
        public void MoveNext()
        {
            if (Count == 0)
            {
                return;
            }
            index = (index + 1) % Count;
        }
        public T GetLoopItem(int offset)
        {
            if (Count == 0)
            {
                return default;
            }
            return array[(index + offset) % Count];
        }
        public IEnumerable<T> loop()
        {
            return this;
        }
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return GetLoopItem(i);
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
