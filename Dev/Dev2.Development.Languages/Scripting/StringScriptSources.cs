using System.Collections.Generic;
using Jurassic;

public class StringScriptSources: IStringScriptSources
{    
    public IList<FileScriptSource> FileScriptSources { get; }

    public StringScriptSources()
    {
        FileScriptSources = new List<FileScriptSource>();
    }
    public void AddPaths(string paths)
    {
        var parts = paths.Split(';');
        foreach (var path in parts)
        {
            var fileScriptSource = new FileScriptSource(path);
            FileScriptSources.Add(fileScriptSource);
        }
    }
    public IList<FileScriptSource> GetFileScriptSources()
    {
        return FileScriptSources;
    }
}

public interface IStringScriptSources
{
    void AddPaths(string paths);
    IList<FileScriptSource> GetFileScriptSources();

    IList<FileScriptSource> FileScriptSources { get; }
}