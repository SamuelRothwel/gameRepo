using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using coolbeats.scripts.logicScripts.Bases;

namespace coolbeats.scripts.logicScripts.BackgroundLogic
{
    public class timedFlag : timedObject
    {
        public bool flag;
        public timedFlag() {}
        public timedFlag(double t)
        {
            flag = false;
            mAccess.lifetimeManager.queueTimer(this, t);
        }
        public void reset(double t)
        {
            flag = false;
            mAccess.lifetimeManager.queueTimer(this, t);
        }
        public void QueueFree()
        {
            flag = true;
        }
    }
}