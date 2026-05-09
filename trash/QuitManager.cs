/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coolbeats.scripts.managerScripts
{
    public partial class QuitManager : managerNode
    {
        public List<Action> closingActions;
        public override void setup()
        {
            closingActions = new List<Action>();
            GetTree().AutoAcceptQuit = false;
        }
        public override void _Notification(int what)
        {
            if (what == NotificationWMCloseRequest)
            {
                for (int i = 0; i < closingActions.Count(); i ++)
                {
                    closingActions[i].Invoke();
                }
                GetTree().Quit(); 
            }
        }
    }
}*/