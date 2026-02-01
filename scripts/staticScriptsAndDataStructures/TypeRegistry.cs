using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


public class TypeRegistry<baseT> where baseT : class
{
    public readonly Dictionary<Type, object> _instances = new();
    public void Register(baseT instance, Type T)  => _instances[T] = instance;
    public T Get<T>() where T : class, baseT => Unsafe.As<T>(_instances[typeof(T)]);
}

public class multiTypeRegistry<baseT> where baseT : class
{
    public readonly Dictionary<Type, List<object>> _instances = new();
    public void Register(baseT instance, Type T)
    {
        if (!_instances.ContainsKey(T))
        {
            _instances[T] = new List<object>();
        }
        _instances[T].Add(instance);
    }
    public List<T> Get<T>() where T : class, baseT => Unsafe.As<List<T>>(_instances[typeof(T)]);
}