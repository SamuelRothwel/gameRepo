using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Collections;

namespace coolbeats.scripts.staticScriptsAndDataStructures
{
    public class compositeArray<T> : IEnumerable
    {
        T[][] arrays;
        int[] cumulativeIndex;
        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i< arrays.Length; i++)
            {
                for (int j = 0; j < arrays[i].Length; j++)
                {
                    yield return arrays[i][j];
                }
            }
        }

        public compositeArray(params T[][] parameters)
        {
            arrays = parameters;
            cumulativeIndex = new int[parameters.Length + 1];
            for (int i = 0; i < parameters.Count(); i++)
            {
                cumulativeIndex[i + 1] = cumulativeIndex[i] + parameters[i].Length;
            }
        }
        public int Length => cumulativeIndex[^1];

        public T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)Length)
                    throw new IndexOutOfRangeException();

                int i = Array.BinarySearch(cumulativeIndex, index);
                if (i < 0) i = ~i - 1;

                return arrays[i][index - cumulativeIndex[i]];
            }
        }
    }
}