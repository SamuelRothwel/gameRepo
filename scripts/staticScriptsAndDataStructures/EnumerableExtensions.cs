using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

static class EnumerableExtensions
{
    public static T[,] To2DArray<T>(this IEnumerable<IEnumerable<T>> source)
    {
        var data = source
            .Select(x => x.ToArray())
            .ToArray();

        var res = new T[data.Length, data.Max(x => x.Length)];
        for (var i = 0; i < data.Length; ++i)
        {
            for (var j = 0; j < data[i].Length; ++j)
            {
                res[i,j] = data[i][j];
            }
        }

        return res;
    }
    public static TResult[,] Select<TSource, TResult>(this TSource[,] source,  Func<TSource, TResult> selector)
    {
        int height = source.GetLength(0);
        int width = source.GetLength(1);
        TResult[,] output = new TResult[height, width];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                output[i, j] = selector.Invoke(source[i, j]);
            }
        }
        return output;
    }
}