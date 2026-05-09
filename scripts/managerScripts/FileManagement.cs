using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

public partial class FileManagement : managerNode
{
    public Dictionary<string, string> filePaths;

    public override void setup()
    {
        filePaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        filePaths["sets"] = @"scripts/scriptSets";
        filePaths["shaders"] = filePaths["sets"] + @"\ShaderSpripts";

        foreach(string dir in Directory.EnumerateDirectories(filePaths["sets"]))
        {
            filePaths[dir.Split(@"\").Last()] = dir;
        }
    }

    public string[] getFiles(string directory)
    {
        return getFiles(filePaths[directory]);
    }
}