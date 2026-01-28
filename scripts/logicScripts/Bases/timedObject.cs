using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace coolbeats.scripts.logicScripts.Bases
{
    public interface timedObject
    {
        public virtual void timeout()
        {
            QueueFree();
        }
        public void QueueFree();
    }
}