using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Microsoft.VisualBasic;
namespace coolbeats.scripts.logicScripts.Bases
{
    public abstract class Polymorph<T> : Polymorph where T : Polymorph
    {
        static T Default;
        List<string> changedActions;
        List<string> changedValues;
        public virtual void morph(Polymorph<T> newObject)
        {
            //changedActions = 
        }
        public virtual void revert()
        {
            foreach (string action in changedActions)
            {
                actions[action] = Default.actions[action];
            }
            foreach (string value in changedValues)
            {
                values[value] = Default.values[value];
            }
        }
    }
}


