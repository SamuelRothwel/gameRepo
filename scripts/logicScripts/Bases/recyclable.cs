using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace coolbeats.scripts.logicScripts.Bases
{
    public class RecyclableDefault : Recyclable
    {
        public static Func<RecyclableDefault> get = () => new RecyclableDefault();
        public bool active { get; set; }
        public void clean() {}
        public void setup() {}
    }
    public interface Recyclable
    {
        bool active {get; set;}
        public void clean();
    }
    public interface Fetcher<out T>
    {
        public Fetcher<TNew> init<TNew>() where TNew : class;
        public T Get();
    }
}