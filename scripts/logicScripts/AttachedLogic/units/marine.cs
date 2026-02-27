using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;
using coolbeats.scripts.logicScripts.AttachedLogic.Components;
using coolbeats.scripts.managerScripts;
using Godot;

namespace coolbeats.scripts.logicScripts.AttachedLogic.units
{
    public partial class marine : unitControler
    {
        bool attackTarget;
        bool chaseTarget;
        bool scanForChase;
        bool scanForAttack;
        float attackRange;
        float speed;
        public marine()
        {
            type = "attacker";
            radius = 30;
            detectionRadius = 150;
            speed = 1;
            maxHP = 50;
            sendCommand(new command("idle"));
            QueueRedraw();
        }
        public override void _Process(double delta)
        {
            if (scanForAttack)
            {
                bool stationaryAttack = false;
                foreach (componentGun gun in components.Get<componentGun>())
                {
                    Guid? target = gun.scan();
                    if (target != null)
                    {
                        stationaryAttack = true;
                        Rotate(GetAngleTo(mAccess.unitManager.units[(Guid)target].Position)+math.PI/2);
                    }
                }
                if (stationaryAttack)
                {
                    goto end;
                }
            }
            if (scanForChase)
            {
                Guid? target = scanTargets(attackRange);
                if (target != null)
                {
                    move(mAccess.unitManager.units[(Guid)target].Position);
                    goto end;
                }
            }
            if (attackTarget)
            {
                if (Position.DistanceTo(mAccess.unitManager.units[(Guid)activeCommand.unit].Position) < attackRange)
                {
                    attack((Guid)activeCommand.unit);
                    goto end;
                }
            }
            if (chaseTarget)
            {
                if (activeCommand.unit != Guid.Empty)
                {
                    move(mAccess.unitManager.units[(Guid)activeCommand.unit].Position);
                }
                else
                {
                    move(activeCommand.coordinates);
                }
            }
            end:;
        }
        public override void Next()
        {
            if (commandList.Any())
            {
                activateCommand(commandList.Dequeue());
            }
            else
            {
                activateCommand(new command("idle"));
            }
        }
        public override void queueCommand(command com)
        {
            if (activeCommand.state == "idle")
            {
                activateCommand(com);
            } else
            {
                commandList.Enqueue(com);
            }
        }
        
        public override void sendCommand(command com)
        {
            commandList.Clear();
            activateCommand(com);
        }
        public override void activateCommand(command com)
        {
            activeCommand = com;
            switch(com.state) {
                case "move":
                    attackTarget = false;
                    chaseTarget = true;
                    scanForChase = false;
                    scanForAttack = false;
                    break;
                case "attack":
                    attackTarget = true;
                    chaseTarget = true;
                    scanForChase = false;
                    scanForAttack = false;
                    break;
                case "idle":
                    attackTarget = false;
                    chaseTarget = false;
                    scanForChase = true;
                    scanForAttack = true;
                    break;
                case "holdPosition":
                    attackTarget = true;
                    chaseTarget = false;
                    scanForChase = false;
                    scanForAttack = true;
                    break;
                case "attackMove":
                    attackTarget = false;
                    chaseTarget = true;
                    scanForChase = true;
                    scanForAttack = true;
                    break;
                default:
                    GD.Print("Invalid State: ", com.state);
                    break;
            }
        }
        public void chase()
        {
            
        }
        public void move(Vector2 targetPosition)
        {
            Rotate(GetAngleTo(targetPosition)+math.PI/2);
            Position += (targetPosition - Position).Normalized()*speed*2;
            if (Position.DistanceTo(targetPosition) < 10)
            {
                Next();
            }
            foreach(componentGun weapon in components.Get<componentGun>())
            {
                weapon.subComponents.Get<Gun>().shooting = false;
            }
        }
        public void attack(Guid target) {
            foreach(componentGun weapon in components.Get<componentGun>())
            {
                weapon.target(target);
            }
        }
        public Guid? scanTargets(float range)
        {
            Guid? target = null;
            List<Guid> potentialTargets = mAccess.teamManager.searchTeams(mAccess.teamManager.GetTeam(ID).enemies, math.getMinMax(Position, range));
            float smallest = range;
            foreach (Guid potentialTarget in potentialTargets)
            {
                unitControler unit = mAccess.unitManager.units[potentialTarget];
                float distance = Position.DistanceTo(unit.Position);
                if (distance < smallest)
                {
                    smallest = distance;
                    target = potentialTarget;
                }
            }
            return target;
        }
    }
}