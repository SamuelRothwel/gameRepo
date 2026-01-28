using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class tree<K, V> : Dictionary<K, tree<K, V>>
{
    public V Value { get; set; }
}