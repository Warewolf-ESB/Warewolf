using System.Collections.Generic;
using Jurassic;

public class StringScriptSources : IStringScriptSources
{
    readonly IList<FileScriptSource> _fileScriptSources;

    public StringScriptSources()
    {
        _fileScriptSources = new List<FileScriptSource>();
    }
    public void AddPaths(string paths)
    {
        var parts = paths.Split(';');
        foreach (var path in parts)
        {
            var fileScriptSource = new FileScriptSource(path);
            _fileScriptSources.Add(fileScriptSource);
        }
    }
    
    public IList<FileScriptSource> GetFileScriptSources()
    {
        return _fileScriptSources;
    }
}

public interface IStringScriptSources
{
    void AddPaths(string paths);
    IList<FileScriptSource> GetFileScriptSources();

}