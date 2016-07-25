using System.Collections.Generic;
using Jurassic;

public class StringScriptSources
{
    public static IList<FileScriptSource> FileScriptSources = new List<FileScriptSource>();

    public static void AddPaths(string paths)
    {
        var parts = paths.Split(';');
        foreach (var path in parts)
        {
            var fileScriptSource = new FileScriptSource(path);
            FileScriptSources.Add(fileScriptSource);
        }
    }
    public static IList<FileScriptSource> GetFileScriptSources()
    {
        return FileScriptSources;
    }
}