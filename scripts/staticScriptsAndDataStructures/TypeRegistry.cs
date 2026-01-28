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