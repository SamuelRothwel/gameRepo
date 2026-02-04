using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using coolbeats.scripts.staticScriptsAndDataStructures;

public class team
{
    public team( )
    {
        name = "";
        enemies = new team[0];
        allies = new List<team>();
    }
    public string name;
    public team[] enemies;
    public List<team> allies;
    public treeBinary<(Guid, (float, float, float, float))> BVH;
    public treeBinary<(Guid, (float, float, float, float))> detectorBVH;
    public treeBinary<(Guid, (float, float, float, float))> targetBVH;
    public HashSet<Guid> units = new HashSet<Guid>();
    public List<Guid> visibleEnemies;
}