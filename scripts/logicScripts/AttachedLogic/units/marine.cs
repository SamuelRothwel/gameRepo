using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using coolbeats.scripts.managerScripts;
using Godot;

namespace coolbeats.scripts.logicScripts.AttachedLogic.units
{
    public partial class marine : unitControler
    {
        public marine()
        {
            state = "idle";
            type = "test";
            radius = 1;
            detectionRadius = 8;
        }
        public override void sendCommand(command com)
        {
            state = com.state;
            switch(state) {
                case "idle":
                    idle();
                    break;
                case "move":
                    move(com.coordinates?? new Vector2());
                    break;
                case "attack":
                    attack(com.unit?? new Guid());
                    break;
                default:
                    GD.Print("Invalid State: ", com.state);
                    break;
            }
        }
        public void idle()
        {
            foreach(componentController weapon in components["weapon"])
            {
                weapon.state = "scan";
            }
        }
        public void move(Vector2 coords)
        {
            foreach(componentController weapon in components["weapon"])
            {
                weapon.state = "idle";
            }
        }
        public void attack(Guid target) {
            foreach(componentController weapon in components["weapon"])
            {
                weapon.state = "target";
            }
        }
    }
}