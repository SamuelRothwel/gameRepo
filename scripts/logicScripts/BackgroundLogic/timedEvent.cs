using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using coolbeats.scripts.logicScripts.Bases;

namespace coolbeats.scripts.logicScripts.BackgroundLogic
{
    public class timedEvent : timedObject
    {
        public Action function;
        public timedEvent() {}
        public timedEvent(Action func, double t)
        {
            function = func;
            mAccess.lifetimeManager.queueTimer(this, t);
        }
        public void setFunction(Action func)
        {
            function = func;
        }
        public void reset(double t)
        {
            mAccess.lifetimeManager.queueTimer(this, t);
        }
        public void QueueFree()
        {
            function.Invoke();
        }
    }
}