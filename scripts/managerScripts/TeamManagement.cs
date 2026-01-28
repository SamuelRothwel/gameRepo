using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
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
            teams = new team[] {new team()};
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
                t.targetBVH = createBVH(ref encodings);
            }
        }
        public void twinTraversal(KeyValuePair<Guid, tree<Guid, (float, float, float, float)>> pair1, int depth1, KeyValuePair<Guid, tree<Guid, (float, float, float, float)>> pair2, int depth2, ref List<Guid> output)
        {
            tree<Guid, (float, float, float, float)> Tree1 = pair1.Value;
            tree<Guid, (float, float, float, float)> Tree2 = pair2.Value;
            if ((!(Tree1.Value.Item1 < Tree2.Value.Item1 && Tree1.Value.Item1 < Tree2.Value.Item1)  || !(Tree1.Value.Item1 < Tree2.Value.Item1 && Tree1.Value.Item1 < Tree2.Value.Item1)) && (!(Tree1.Value.Item1 < Tree2.Value.Item1 && Tree1.Value.Item1 < Tree2.Value.Item1)  || !(Tree1.Value.Item1 < Tree2.Value.Item1 && Tree1.Value.Item1 < Tree2.Value.Item1)))
            {
                if (!Tree1.Any() && !Tree2.Any())
                {
                    output.Add(pair1.Key);
                }
                else
                {
                    if (depth1 > depth2)
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
        }
        public void searchBVH(tree<Guid, (float, float, float, float)> BVH, ref List<Guid> output, Godot.Vector2 coords, float radius)
        {
            float minX = coords.X - radius;
            float maxX = coords.X + radius;
            float minY = coords.Y - radius;
            float maxY = coords.Y + radius;
            if ((!(BVH.Value.Item1 < minX && BVH.Value.Item1 < maxX) || 
            !(BVH.Value.Item2 > minX && BVH.Value.Item2 > maxX)) && 
            (!(BVH.Value.Item3 < minY && BVH.Value.Item3 < maxY) || 
            !(BVH.Value.Item4 > minY && BVH.Value.Item4 > maxY))) {
                foreach (KeyValuePair<Guid, tree<Guid, (float, float, float, float)>> branch in BVH)
                {
                    searchBVH(branch, ref output, minX, minY, maxX, maxY);
                }
            }
        }
        public void searchBVH(tree<Guid, (float, float, float, float)> BVH, ref List<Guid> output, float minX, float minY, float maxX, float maxY)
        {
            foreach (KeyValuePair<Guid, tree<Guid, (float, float, float, float)>> branch in BVH)
            {
                searchBVH(branch, ref output, minX, minY, maxX, maxY);
            }
        }
        public void searchBVH(KeyValuePair<Guid, tree<Guid, (float, float, float, float)>> pair, ref List<Guid> output, float minX, float minY, float maxX, float maxY)
        {
            tree<Guid, (float, float, float, float)> Tree = pair.Value;
            if ((!(Tree.Value.Item1 < minX && Tree.Value.Item1 < maxX) && 
            !(Tree.Value.Item2 > minX && Tree.Value.Item2 > maxX)) && 
            (!(Tree.Value.Item3 < minY && Tree.Value.Item3 < maxY) && 
            !(Tree.Value.Item4 > minY && Tree.Value.Item4 > maxY)))
            {
                if (!Tree.Any())
                {
                    output.Add(pair.Key);
                }  else
                {
                    foreach (KeyValuePair<Guid, tree<Guid, (float, float, float, float)>> branch in Tree)
                    {
                        searchBVH(branch, ref output, minX, minY, maxX, maxY);
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
            return encodings.OrderBy(x => x.Item1).Select(x => x.Item2).ToArray();
        }
        public tree<Guid, (float, float, float, float)> createBVH(ref Guid[] list, bool detectors = false)
        {
            tree<Guid, (float, float, float, float)> Tree = new tree<Guid, (float, float, float, float)>();
            if (list.Count() != 0)
            {
                Tree[list[0]] = recursiveBVH(ref list, 0, list.Count() - 1, detectors);
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
            int middle = Convert.ToInt16(start/end);
            tree<Guid, (float, float, float, float)> left = recursiveBVH(ref list, start, middle-1, detectors);
            tree<Guid, (float, float, float, float)> right = recursiveBVH(ref list, middle, end, detectors);
            Tree.Value = ( Math.Max(left.Value.Item1, right.Value.Item1), 
                Math.Min(left.Value.Item2, right.Value.Item2),
                Math.Max(left.Value.Item3, right.Value.Item3),
                Math.Min(left.Value.Item4, right.Value.Item4));
            Tree[list[start]] = left;
            Tree[list[middle]] = right;
            return Tree;
        }
    }
}