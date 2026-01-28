using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coolbeats.scripts.staticScriptsAndDataStructures
{
    public class CircularEnumerator<T>
    {
        public T[] array;
        public int index;
        public T Current => array[index];
        public CircularEnumerator(ref T[] values, int start = 0)
        {
            array = values;
            index = start;
        }
        public void MoveNext()
        {
            index = (index + 1) % array.Count();
        }
        public IEnumerable<T> loop()
        {
            int start = index;
            do 
            {
                yield return Current;
                MoveNext();
            } while (start != index);
        }
    }
}