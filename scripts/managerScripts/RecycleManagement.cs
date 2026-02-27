using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using coolbeats.scripts.logicScripts.Bases;

namespace coolbeats.scripts.managerScripts
{
    public partial class RecycleManagement : managerNode
    {
        public FetcherTypeRegistry recyclers;
        public GetterTypeRegistry typeGetters;
        public override void setup()
        {
            recyclers = new FetcherTypeRegistry(new Recycler<RecyclableDefault>(RecyclableDefault.get));
            typeGetters = new GetterTypeRegistry();
            typeGetters.Register(typeof(projectile), () => (projectile)mAccess.entityManager.getEntity("projectile"));
            recyclers.Register<projectile>();
        }
        public T Get<T>() where T : class
        {
            return recyclers.Get<T>();
        }
        public class Recycler<T> : Fetcher<T> where T : Recyclable
        {
            Stack<T> idle;
            List<T> active;
            Func<T> getter;
            public Recycler(Func<T> get) 
            {
                getter = get;
                idle = new Stack<T>(Enumerable.Range(0, 1025)
                .Select(x => getter()));
                active = new List<T>();
            }
            public T Get()
            {
                T activated;
                bool success = idle.TryPop(out activated);
                if (!success)
                {
                    activated = getter();
                }
                active.Add(activated);
                return activated;
            }
            public Fetcher<TNew> init<TNew>() where TNew : class
            {
                return ((dynamic)this).strictInit<TNew>();
            }
            public Fetcher<Ts> strictInit<Ts>() where Ts : class, Recyclable
            {
                return new Recycler<Ts>(mAccess.recycleManager.typeGetters.GetDelegate<Ts>());
            }
        }
        public class entityWrapper<T> : Recyclable
        {
            string name;
            public bool active { get; set; }
            public void setup()
            {
            }
            public void clean()
            {
            }
        }
    }
}