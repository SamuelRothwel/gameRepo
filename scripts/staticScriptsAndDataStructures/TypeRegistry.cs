using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using coolbeats.scripts.logicScripts.Bases;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


public class TypeRegistry
{
    public readonly Dictionary<Type, object> _instances = new();
    public void Register(object instance, Type T)  => _instances[T] = instance;
    public T Get<T>() where T : class => Unsafe.As<T>(_instances[typeof(T)]);
}

public class multiTypeRegistry
{
    public readonly Dictionary<Type, List<object>> _instances = new();
    public void Register(object instance, Type T)
    {
        if (!_instances.ContainsKey(T))
        {
            _instances[T] = new List<object>();
        }
        _instances[T].Add(instance);
    }
    public List<T> Get<T>() => Unsafe.As<List<T>>(_instances[typeof(T)]);
}

public class FetcherTypeRegistry
{
    Fetcher<object> _default;
    public readonly Dictionary<Type, Fetcher<object>> _instances = new();
    public FetcherTypeRegistry(Fetcher<object> example) 
    {
        _default = example;
    }
    public void Register<T>() where T : class
    {
        if (!_instances.ContainsKey(typeof(T)))
        {
            _instances[typeof(T)] = (Fetcher<object>)_default.init<T>();
        }
    }
    public T Get<T>() where T : class => Unsafe.As<T>(_instances[typeof(T)].Get());
}

public class GetterTypeRegistry
{
    Dictionary<Type, Func<object>> _instances = new();
    public void Register(Type T, Func<object> getter)
    {
        _instances[T] = getter;
    }
    public Func<T> GetDelegate<T>() where T : class => Unsafe.As<Func<T>>(_instances[typeof(T)]);
    public T Get<T>() where T : class => Unsafe.As<T>(_instances[typeof(T)].Invoke());
}