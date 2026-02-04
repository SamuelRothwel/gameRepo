using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
                    twinTraversal(
                        new KeyValuePair<Guid, tree<Guid, (float, float, float, float)>>(new Guid(), t.enemies[j].BVH), 0, 
                        new KeyValuePair<Guid, tree<Guid, (float, float, float, float)>>(new Guid(), t.detectorBVH), 0, ref output);
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
        public void splitTwinTraversal(tree<Guid, (float, float, float, float)> Tree1, tree<Guid, (float, float, float, float)> Tree2, ref List<Guid> output)
        {
            Dictionary<string, List<tree<Guid, (float, float, float, float)>>> lookUp = new Dictionary<string, List<tree<Guid, (float, float, float, float)>>>();
            Queue<(tree<Guid, (float, float, float, float)>, List<string>)> current = new Queue<(tree<Guid, (float, float, float, float)>, List<string>)>();
            while (true)
            {
                var cur = current.Dequeue();
                foreach (string s in cur.Item2)
                {
                    foreach (tree<Guid, (float, float, float, float)> area in lookUp[s]) {
                        if (inBounds(cur.Item1.Value, area.Value))
                        {
                            if (area.Any())
                            {
                                if ()
                            }
                        }
                    }
                }
            }
            //iterate through all elements in 
            for ()
        }
        /*public void twinTraversal(KeyValuePair<Guid, tree<Guid, (float, float, float, float)>> pair1, int depth1, KeyValuePair<Guid, tree<Guid, (float, float, float, float)>> pair2, int depth2, ref List<Guid> output)
        {
            tree<Guid, (float, float, float, float)> Tree1 = pair1.Value;
            tree<Guid, (float, float, float, float)> Tree2 = pair2.Value;
            GD.Print("eepy");
            if ((!(Tree1.Value.Item1 < Tree2.Value.Item1 && Tree1.Value.Item1 < Tree2.Value.Item2) &&
            !(Tree1.Value.Item2 > Tree2.Value.Item1 && Tree1.Value.Item2 > Tree2.Value.Item2)) && 
            (!(Tree1.Value.Item3 < Tree2.Value.Item3 && Tree1.Value.Item3 < Tree2.Value.Item4) && 
            !(Tree1.Value.Item4 > Tree2.Value.Item3 && Tree1.Value.Item4 > Tree2.Value.Item4)))
            {
                if (!Tree1.Any() && !Tree2.Any())
                {
                    output.Add(pair1.Key);
                }
                else
                {
                    if (!Tree1.Any() || depth1 < depth2)
                    {
                        foreach (KeyValuePair<Guid, tree<Guid, (float, float, float, float)>> branch in Tree2)
                        {
                            twinTraversal(pair1, depth1, branch, depth2 + 1, ref output);
                        } 
                    } else
                    {
                        foreach (KeyValuePair<Guid, tree<Guid, (float, float, float, float)>> branch in Tree1)
                        {
                            twinTraversal(branch, depth1 + 1, pair2, depth2, ref output);
                        } 
                    }
                }
            }
        }*/
        public List<Guid> searchTeams(team[] t, (float, float, float, float) minMax)
        {
            List<Guid> units = new List<Guid>();
            foreach (team enemy in t)
            {
                searchBVH(enemy.BVH, ref units, minMax);
            }
            return units;
        }
        public List<Guid> searchBVH(tree<Guid, (float, float, float, float)> BVH, (float, float, float, float) minMax)
        {
            List<Guid> output = new List<Guid>();
            searchBVH(BVH, ref output, minMax);
            return output;
        }
        public void searchBVH(tree<Guid, (float, float, float, float)> tree, ref List<Guid> output, (float, float, float, float) minMax)
        {
            if ((!(tree.Value.Item1 < minMax.Item1 && tree.Value.Item1 < minMax.Item2) && 
            !(tree.Value.Item2 > minMax.Item1 && tree.Value.Item2 > minMax.Item2)) && 
            (!(tree.Value.Item3 < minMax.Item3 && tree.Value.Item3 < minMax.Item4) && 
            !(tree.Value.Item4 > minMax.Item3 && tree.Value.Item4 > minMax.Item4))) {
                foreach (KeyValuePair<Guid, tree<Guid, (float, float, float, float)>> branch in tree)
                {
                    searchBVH(branch, ref output, minMax);
                }
            }
        }
        public void searchBVH(KeyValuePair<Guid, tree<Guid, (float, float, float, float)>> pair, ref List<Guid> output, (float, float, float, float) minMax)
        {
            tree<Guid, (float, float, float, float)> BVH = pair.Value;
            if ((!(BVH.Value.Item1 < minMax.Item1 && BVH.Value.Item1 < minMax.Item2) && 
            !(BVH.Value.Item2 > minMax.Item1 && BVH.Value.Item2 > minMax.Item2)) && 
            (!(BVH.Value.Item3 < minMax.Item3 && BVH.Value.Item3 < minMax.Item4) && 
            !(BVH.Value.Item4 > minMax.Item3 && BVH.Value.Item4 > minMax.Item4)))
            {
                if (!BVH.Any())
                {
                    output.Add(pair.Key);
                }  else
                {
                    foreach (KeyValuePair<Guid, tree<Guid, (float, float, float, float)>> branch in BVH)
                    {
                        searchBVH(branch, ref output, minMax);
                    }
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
        
        public tree<Guid, (float, float, float, float)> createBVH(ref Guid[] list, bool detectors = false)
        {
            tree<Guid, (float, float, float, float)> Tree = new tree<Guid, (float, float, float, float)>();
            if (list.Count() != 0)
            {
                Tree[list[0]] = recursiveBVH(ref list, 0, list.Count() - 1, detectors);
                Tree.Value = Tree[list[0]].Value;
            }
            return Tree;
        }
        public tree<Guid, (float, float, float, float)> recursiveBVH(ref Guid[] list, int start, int end, bool detectors)
        {
            tree<Guid, (float, float, float, float)> Tree = new tree<Guid, (float, float, float, float)>();
            if (start == end)
            {
                unitControler target = mAccess.unitManager.units[list[start]];
                if (detectors)
                {
                    Tree.Value = (target.Position.X + target.detectionRadius, target.Position.X - target.detectionRadius, target.Position.Y + target.detectionRadius, target.Position.Y - target.detectionRadius);
                } else
                {
                    Tree.Value = (target.Position.X + target.radius, target.Position.X - target.radius, target.Position.Y + target.radius, target.Position.Y - target.radius);
                }
                return Tree;
            }
            int middle = (int)Math.Round((double)start/end);
            tree<Guid, (float, float, float, float)> left = recursiveBVH(ref list, start, middle, detectors);
            tree<Guid, (float, float, float, float)> right = recursiveBVH(ref list, middle+1, end, detectors);
            Tree.Value = ( Math.Max(left.Value.Item1, right.Value.Item1), 
                Math.Min(left.Value.Item2, right.Value.Item2),
                Math.Max(left.Value.Item3, right.Value.Item3),
                Math.Min(left.Value.Item4, right.Value.Item4));
            Tree[list[start]] = left;
            Tree[list[middle+1]] = right;
            return Tree;
        }
        public bool inBounds((float, float, float, float) a1, (float, float, float, float) a2)
        {
            return (!(a1.Item1 < a2.Item1 && a1.Item1 < a2.Item2) && 
            !(a1.Item2 > a2.Item1 && a1.Item2 > a2.Item2)) && 
            (!(a1.Item3 < a2.Item3 && a1.Item3 < a2.Item4) && 
            !(a1.Item4 > a2.Item3 && a1.Item4 > a2.Item4));
        }
        public void printBVH(tree<Guid, (float, float, float, float)> input)
        {
            GD.Print("start");
            List<string> output = new List<string>() {input.Value.ToString()};
            foreach (KeyValuePair<Guid, tree<Guid, (float, float, float, float)>> pair in input)
            {
                output.AddRange(recursivePrint(pair).Select(x => "-" + x));
            }
            foreach (string s in output)
            {
                GD.Print(s);
            }
            GD.Print("end");
        }
        public List<string> recursivePrint(KeyValuePair<Guid, tree<Guid, (float, float, float, float)>> input)
        {
            List<string> output = new List<string>();
            
            foreach (KeyValuePair<Guid, tree<Guid, (float, float, float, float)>> pair in input.Value)
            {
                output.AddRange(recursivePrint(pair).Select(x => "-" + x));
            }
            return output;
        }
    }
}