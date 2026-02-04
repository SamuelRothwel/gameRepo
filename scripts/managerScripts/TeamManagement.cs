using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Headers;
using System.Numerics;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using coolbeats.scripts.logicScripts.Bases;
using coolbeats.scripts.staticScriptsAndDataStructures;
using Godot;

namespace coolbeats.scripts.managerScripts
{
    public partial class TeamManagement : managerNode
    {
        public team[] teams;
        public override void setup()
        {
            teams = new team[] {new team(), new team()};
            teams[0].enemies = new team[] {teams[1]};
            teams[1].enemies = new team[] {teams[0]};
        }
        public team GetTeam(Guid ID)
        {
            for (int i = 0; i < teams.Count(); i++)
            {
                if (teams[i].units.Contains(ID))
                {
                    return teams[i];
                }
            }        
            throw new Exception(ID.ToString() + " not assigned to team");
        }
        public void addUnit(Guid ID, int team)
        {
            teams[team].units.Add(ID);
        }
        public void UpdateTeamVisions()
        {
            for (int i = 0; i < teams.Length; i ++)
            {
                team t = teams[i];
                Guid[] encodings = createEncodings(t.units);
                t.BVH = createBVH(ref encodings);
                printBVH(t.BVH);
                t.detectorBVH = createBVH(ref encodings, true);
            }
            for (int i = 0; i < teams.Length; i ++)
            {
                team t = teams[i];
                t.visibleEnemies = new List<Guid>();
                for (int j = 0; j < teams[i].enemies.Length; j++)
                {
                    List<Guid> output = new List<Guid>();
                    splitTwinTraversal(
                        t.enemies[j].BVH, 
                        t.detectorBVH, ref output);
                    t.visibleEnemies.AddRange(output);
                }
                Guid[] encodings = createEncodings(t.visibleEnemies);
                foreach (Guid encoding in encodings)
                {
                GD.Print(encoding);
                }
                GD.Print(encodings);
                t.targetBVH = createBVH(ref encodings);
                GD.Print("haaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaah");
            }
        }
        public void splitTwinTraversal(treeBinary<(Guid, (float, float, float, float))> Tree1, treeBinary<(Guid, (float, float, float, float))> Tree2, ref List<Guid> output)
        {
            Dictionary<string, treeBinary<(Guid, (float, float, float, float))>> lookUp = new Dictionary<string, treeBinary<(Guid, (float, float, float, float))>>();
            Queue<(treeBinary<(Guid, (float, float, float, float))>, List<string>)> current = new Queue<(treeBinary<(Guid, (float, float, float, float))>, List<string>)>();
            while (true)
            {
                var cur = current.Dequeue();
                List<string> newKeys = new List<string>();
                bool end = false;
                foreach (string s in cur.Item2)
                {
                    treeBinary<(Guid, (float, float, float, float))> branch = lookUp[s];
                    if (inBounds(branch.Value.Item2, cur.Item1.Value.Item2))
                    {
                        if (branch.Value.Item1 == Guid.Empty)
                        {
                            string key = s + "1";
                            newKeys.Add(key);
                            lookUp[key] = branch.left;
                            key = s + "2";
                            newKeys.Add(key);
                            lookUp[key] = branch.right;
                        }
                        else
                        {
                            end = true;
                            newKeys.Add(s);
                        }
                    }
                }
                if (newKeys.Any())
                {
                    if (cur.Item1.Value.Item1 == Guid.Empty)
                    {
                        current.Enqueue((cur.Item1.left, newKeys));
                        current.Enqueue((cur.Item1.right, newKeys));
                    }
                    else
                    {
                        if (end)
                        {
                            output.Add(cur.Item1.Value.Item1);
                        }
                        else
                        {
                            
                        current.Enqueue((cur.Item1, newKeys));
                        }
                    }
                }
            }
        }
        public List<Guid> searchTeams(team[] t, (float, float, float, float) minMax)
        {
            List<Guid> units = new List<Guid>();
            foreach (team enemy in t)
            {
                searchBVH(enemy.BVH, ref units, minMax);
            }
            return units;
        }
        public List<Guid> searchBVH(treeBinary<(Guid, (float, float, float, float))> BVH, (float, float, float, float) minMax)
        {
            List<Guid> output = new List<Guid>();
            searchBVH(BVH, ref output, minMax);
            return output;
        }
        public void searchBVH(treeBinary<(Guid, (float, float, float, float))> pair, ref List<Guid> output, (float, float, float, float) minMax)
        {
            if (inBounds(minMax, pair.Value.Item2))
            {
                if (pair.Value.Item1 != Guid.Empty)
                {
                    output.Add(pair.Value.Item1);
                }  else
                {
                    searchBVH(pair.left, ref output, minMax);
                    searchBVH(pair.right, ref output, minMax);
                }
            }
        }
        public Guid[] createEncodings(IEnumerable<Guid> unitList)
        {
            List<(uint, Guid)> encodings = new List<(uint, Guid)>();
            foreach (Guid u in unitList)
            {
                unitControler unit = mAccess.unitManager.units[u];
                encodings.Add((MortenEncoding.encode((uint)unit.Position.X, (uint)unit.Position.Y), unit.ID));
            }
            return encodings.OrderBy(x => x.Item1).Select(x => x.Item2).Distinct().ToArray();
        }
        
        public treeBinary<(Guid, (float, float, float, float))> createBVH(ref Guid[] list, bool detectors = false)
        {
            if (list.Count() != 0)
            {
                return recursiveBVH(ref list, 0, list.Count() - 1, detectors);
            }
            return new treeBinary<(Guid, (float, float, float, float))>();
        }
        public treeBinary<(Guid, (float, float, float, float))> recursiveBVH(ref Guid[] list, int start, int end, bool detectors)
        {
            treeBinary<(Guid, (float, float, float, float))> Tree = new treeBinary<(Guid, (float, float, float, float))>();
            if (start == end)
            {
                unitControler target = mAccess.unitManager.units[list[start]];
                if (detectors)
                {
                    Tree.Value = (target.ID, (target.Position.X + target.detectionRadius, target.Position.X - target.detectionRadius, target.Position.Y + target.detectionRadius, target.Position.Y - target.detectionRadius));
                } else
                {
                    Tree.Value = (target.ID, (target.Position.X + target.radius, target.Position.X - target.radius, target.Position.Y + target.radius, target.Position.Y - target.radius));
                }
                return Tree;
            }
            int middle = (int)Math.Round((double)start/end);
            Tree.left = recursiveBVH(ref list, start, middle, detectors);
            Tree.right = recursiveBVH(ref list, middle+1, end, detectors);
            Tree.Value = (Guid.Empty, (Math.Max(Tree.left.Value.Item2.Item1, Tree.left.Value.Item2.Item1), 
                Math.Min(Tree.left.Value.Item2.Item2, Tree.left.Value.Item2.Item2),
                Math.Max(Tree.left.Value.Item2.Item3, Tree.left.Value.Item2.Item3),
                Math.Min(Tree.left.Value.Item2.Item4, Tree.left.Value.Item2.Item4)));
            return Tree;
        }
        public bool inBounds((float, float, float, float) a1, (float, float, float, float) a2)
        {
            return (!(a1.Item1 < a2.Item1 && a1.Item1 < a2.Item2) && 
            !(a1.Item2 > a2.Item1 && a1.Item2 > a2.Item2)) && 
            (!(a1.Item3 < a2.Item3 && a1.Item3 < a2.Item4) && 
            !(a1.Item4 > a2.Item3 && a1.Item4 > a2.Item4));
        }
        public void printBVH(treeBinary<(Guid, (float, float, float, float))> input)
        {
            GD.Print("start");
            List<string> output = new List<string>() {input.Value.ToString()};
            
            output.AddRange(recursivePrint(input.left).Select(x => "-" + x));
            output.AddRange(recursivePrint(input.right).Select(x => "-" + x));
            foreach (string s in output)
            {
                GD.Print(s);
            }
            GD.Print("end");
        }
        public List<string> recursivePrint(treeBinary<(Guid, (float, float, float, float))> input)
        {
            List<string> output = new List<string>();
            
            output.AddRange(recursivePrint(input.left).Select(x => "-" + x));
            output.AddRange(recursivePrint(input.right).Select(x => "-" + x));
            return output;
        }
    }
}