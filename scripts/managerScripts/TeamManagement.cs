using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using coolbeats.scripts.staticScriptsAndDataStructures;
using Godot;

namespace coolbeats.scripts.managerScripts
{
    public partial class TeamManagement : managerNode
    {
        public team[] teams;
        readonly Dictionary<Guid, int> unitTeams = new Dictionary<Guid, int>();

        public override void setup()
        {
			LoadGameTeams(EntityFrameworkManagement.DefaultGameId);
        }

		public void LoadGameTeams(Guid gameId)
		{
			unitTeams.Clear();
			List<StoredGameTeam> definitions = mAccess.entityFrameworkManager.GetGameTeams(gameId);
			if (definitions.Count == 0)
			{
				teams = Array.Empty<team>();
				return;
			}
			int size = definitions.Max(definition => definition.TeamIndex) + 1;
			teams = Enumerable.Range(0, size).Select(_ => new team()).ToArray();
			foreach (StoredGameTeam definition in definitions)
			{
				teams[definition.TeamIndex].name = definition.Name;
			}
			foreach (StoredGameTeam definition in definitions)
			{
				try
				{
					int[] enemies = JsonSerializer.Deserialize<int[]>(definition.EnemyTeamIndexesJson) ?? Array.Empty<int>();
					setEnemies(definition.TeamIndex, enemies.Where(isValidTeamIndex).ToArray());
				}
				catch
				{
					setEnemies(definition.TeamIndex);
				}
			}
		}

        void setEnemies(int teamIndex, params int[] enemyIndexes)
        {
            teams[teamIndex].enemies = enemyIndexes.Select(index => teams[index]).ToArray();
        }

        public team GetTeam(Guid ID)
        {
            if (unitTeams.TryGetValue(ID, out int teamIndex) && isValidTeamIndex(teamIndex) && teams[teamIndex].units.Contains(ID))
            {
                return teams[teamIndex];
            }

            for (int i = 0; i < teams.Length; i++)
            {
                if (teams[i].units.Contains(ID))
                {
                    unitTeams[ID] = i;
                    return teams[i];
                }
            }

            throw new Exception(ID.ToString() + " not assigned to team");
        }

		public int GetTeamIndex(Guid id)
		{
			if (unitTeams.TryGetValue(id, out int teamIndex))
			{
				return teamIndex;
			}
			for (int i = 0; i < teams.Length; i++)
			{
				if (teams[i].units.Contains(id)) return i;
			}
			return 0;
		}

        public void addUnit(Guid ID, int teamIndex)
        {
            if (!isValidTeamIndex(teamIndex))
            {
                throw new ArgumentOutOfRangeException(nameof(teamIndex), "Invalid team index " + teamIndex);
            }

            removeUnit(ID);
            teams[teamIndex].units.Add(ID);
            unitTeams[ID] = teamIndex;
        }

        public void removeUnit(Guid ID)
        {
            if (unitTeams.TryGetValue(ID, out int teamIndex) && isValidTeamIndex(teamIndex))
            {
                teams[teamIndex].units.Remove(ID);
            }
            else
            {
                foreach (team t in teams)
                {
                    t.units.Remove(ID);
                }
            }

            unitTeams.Remove(ID);
        }

        bool isValidTeamIndex(int teamIndex)
        {
            return teamIndex >= 0 && teamIndex < teams.Length;
        }

        public override void _Process(double delta)
        {
			if (mAccess.sceneManager != null && mAccess.sceneManager.HasGameCapability("gameActive"))
            {
                UpdateTeamVisions();
            }
        }

        public void UpdateTeamVisions()
        {
            for (int i = 0; i < teams.Length; i++)
            {
                team t = teams[i];
                Guid[] encodings = createEncodings(t.units);
                t.BVH = createBVH(encodings);
                t.detectorBVH = createBVH(encodings, true);
            }

            for (int i = 0; i < teams.Length; i++)
            {
                team t = teams[i];
                HashSet<Guid> visibleEnemies = new HashSet<Guid>();
                for (int j = 0; j < t.enemies.Length; j++)
                {
                    collectVisibleTargets(t.enemies[j].BVH, t.detectorBVH, visibleEnemies);
                }

                t.visibleEnemies = visibleEnemies.ToList();
                Guid[] encodings = createEncodings(t.visibleEnemies);
                t.targetBVH = createBVH(encodings);
            }
        }

        public void splitTwinTraversal(treeBinary<(Guid, (float, float, float, float))> tree1, treeBinary<(Guid, (float, float, float, float))> tree2, ref List<Guid> output)
        {
            HashSet<Guid> visibleTargets = new HashSet<Guid>(output);
            collectVisibleTargets(tree1, tree2, visibleTargets);
            output.Clear();
            output.AddRange(visibleTargets);
        }

        void collectVisibleTargets(treeBinary<(Guid, (float, float, float, float))> targetTree, treeBinary<(Guid, (float, float, float, float))> detectorTree, HashSet<Guid> output)
        {
            if (isEmptyTree(targetTree) || isEmptyTree(detectorTree) || !inBounds(targetTree.Value.Item2, detectorTree.Value.Item2))
            {
                return;
            }

            bool targetLeaf = isLeaf(targetTree);
            bool detectorLeaf = isLeaf(detectorTree);
            if (targetLeaf && detectorLeaf)
            {
                output.Add(targetTree.Value.Item1);
                return;
            }

            if (targetLeaf)
            {
                collectVisibleTargets(targetTree, detectorTree.left, output);
                collectVisibleTargets(targetTree, detectorTree.right, output);
                return;
            }

            if (detectorLeaf)
            {
                collectVisibleTargets(targetTree.left, detectorTree, output);
                collectVisibleTargets(targetTree.right, detectorTree, output);
                return;
            }

            collectVisibleTargets(targetTree.left, detectorTree.left, output);
            collectVisibleTargets(targetTree.left, detectorTree.right, output);
            collectVisibleTargets(targetTree.right, detectorTree.left, output);
            collectVisibleTargets(targetTree.right, detectorTree.right, output);
        }

        public List<Guid> searchTeams(team[] teamsToSearch, (float, float, float, float) minMax)
        {
            List<Guid> units = new List<Guid>();
            foreach (team t in teamsToSearch)
            {
                searchBVH(t.BVH, ref units, minMax);
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
            if (isEmptyTree(pair) || !inBounds(minMax, pair.Value.Item2))
            {
                return;
            }

            if (isLeaf(pair))
            {
                output.Add(pair.Value.Item1);
                return;
            }

            searchBVH(pair.left, ref output, minMax);
            searchBVH(pair.right, ref output, minMax);
        }

        public Guid[] createEncodings(IEnumerable<Guid> unitList)
        {
            List<(uint, Guid)> encodings = new List<(uint, Guid)>();
            foreach (Guid id in unitList.ToArray())
            {
                if (!mAccess.unitManager.units.TryGetValue(id, out unitControler unit) || !GodotObject.IsInstanceValid(unit))
                {
                    removeUnit(id);
                    continue;
                }

                encodings.Add((MortenEncoding.encode((uint)unit.Position.X, (uint)unit.Position.Y), unit.ID));
            }

            return encodings.OrderBy(encoding => encoding.Item1).Select(encoding => encoding.Item2).Distinct().ToArray();
        }

        public treeBinary<(Guid, (float, float, float, float))> createBVH(Guid[] list, bool detectors = false)
        {
            if (list.Length == 0)
            {
                return new treeBinary<(Guid, (float, float, float, float))>();
            }

            return recursiveBVH(list, 0, list.Length - 1, detectors);
        }

        public treeBinary<(Guid, (float, float, float, float))> recursiveBVH(Guid[] list, int start, int end, bool detectors)
        {
            treeBinary<(Guid, (float, float, float, float))> tree = new treeBinary<(Guid, (float, float, float, float))>();
            if (start == end)
            {
                unitControler target = mAccess.unitManager.units[list[start]];
                float radius = detectors ? target.detectionRadius : target.radius;
                tree.Value = (target.ID, getBounds(target.Position, radius));
                return tree;
            }

            int middle = start + ((end - start) / 2);
            tree.left = recursiveBVH(list, start, middle, detectors);
            tree.right = recursiveBVH(list, middle + 1, end, detectors);
            tree.Value = (Guid.Empty, mergeBounds(tree.left.Value.Item2, tree.right.Value.Item2));
            return tree;
        }

        (float, float, float, float) getBounds(Vector2 position, float radius)
        {
            return (position.X + radius, position.X - radius, position.Y + radius, position.Y - radius);
        }

        (float, float, float, float) mergeBounds((float, float, float, float) a, (float, float, float, float) b)
        {
            return
            (
                Math.Max(a.Item1, b.Item1),
                Math.Min(a.Item2, b.Item2),
                Math.Max(a.Item3, b.Item3),
                Math.Min(a.Item4, b.Item4)
            );
        }

        bool isEmptyTree(treeBinary<(Guid, (float, float, float, float))> tree)
        {
            return tree == null || (tree.Value.Item1 == Guid.Empty && tree.left == null && tree.right == null);
        }

        bool isLeaf(treeBinary<(Guid, (float, float, float, float))> tree)
        {
            return tree != null && tree.Value.Item1 != Guid.Empty;
        }

        public bool inBounds((float, float, float, float) a1, (float, float, float, float) a2)
        {
            return a1.Item1 >= a2.Item2 &&
                a1.Item2 <= a2.Item1 &&
                a1.Item3 >= a2.Item4 &&
                a1.Item4 <= a2.Item3;
        }

        public void printBVH(treeBinary<(Guid, (float, float, float, float))> input)
        {
            if (isEmptyTree(input))
            {
                GD.Print("BVH: empty");
                return;
            }

            List<string> output = new List<string>() { input.Value.ToString() };
            GD.Print("BVH:");
            output.Add(input.Value.Item2.ToString());
            if (!isLeaf(input))
            {
                output.AddRange(recursivePrint(input.left).Select(x => "-" + x));
                output.AddRange(recursivePrint(input.right).Select(x => "-" + x));
            }
            else
            {
                output.Add(input.Value.Item1.ToString());
            }

            foreach (string s in output)
            {
                GD.Print(s);
            }
        }

        public List<string> recursivePrint(treeBinary<(Guid, (float, float, float, float))> input)
        {
            List<string> output = new List<string>();
            if (isEmptyTree(input))
            {
                output.Add("empty");
                return output;
            }

            output.Add(input.Value.Item2.ToString());
            if (!isLeaf(input))
            {
                output.AddRange(recursivePrint(input.left).Select(x => "-" + x));
                output.AddRange(recursivePrint(input.right).Select(x => "-" + x));
            }
            else
            {
                output.Add(input.Value.Item1.ToString());
            }

            return output;
        }
    }
}
