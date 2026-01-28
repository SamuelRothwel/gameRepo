using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Microsoft.VisualBasic;


public static class fileSearch
{
	public static string animationsFilePath = "visualAssets/pixelAssets/Animations/";
    public static string audioFilePath = "AudioAssets/";


	public static Dictionary<string, AudioStream> getAudio(string audioSet)
	{
		return getDirectory(audioFilePath + audioSet, false, false).ToDictionary(x => x, x => GD.Load<AudioStream>(audioFilePath + x));
	}
	public static Dictionary<string, Dictionary<string, Image[]>> getImages()
    {
		return getDirectory(animationsFilePath, true).ToDictionary(x => x, x => getSpriteImages(x));
    }
	public static Dictionary<string, Image[]> getSpriteImages(string spriteSet)
	{
		return getSpritePaths(spriteSet).ToDictionary(x => x.Key, x => x.Value.Select(y => { return Image.LoadFromFile(y); }).ToArray());
	}
	public static Dictionary<string, string[]> getSpritePaths(string spriteSet)
	{
		string basePath = animationsFilePath + spriteSet;
		Dictionary<string, string[]> spritePaths = new Dictionary<string, string[]>();
		List<string> animationPaths = getDirectory(basePath, true);
		foreach (string animationPath in animationPaths)
		{
			spritePaths.Add(animationPath, getDirectory(animationsFilePath + spriteSet + "/" + animationPath, false, true).OrderBy(x => x).ToArray());
		}
		return spritePaths;
	}
	public static List<string> getDirectory(string basePath, bool folders = false, bool includePath = false)
	{
		List<string> directory = new List<string>();
		DirAccess libraryFolder = DirAccess.Open(basePath);
		try
        {
			libraryFolder.ListDirBegin();
        } catch
        {
			GD.Print("invalid filepath: " + basePath);
        }
		while (true)
		{
			string name = libraryFolder.GetNext();
			if (name == "")
			{
				if (directory.Count == 0)
                {
					GD.Print("empty directory: " + basePath);
                }
				libraryFolder.ListDirEnd();
				return directory;
			}
			if (folders == libraryFolder.CurrentIsDir() && name.Split(".").Last() != "import")
			{
				directory.Add((includePath ? basePath + "/" : "") + name);
			}
		}
	}
}