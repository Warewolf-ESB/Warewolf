using System;
using System.Collections.Generic;

namespace Warewolf.Launcher
{
    public interface ITestRunner
    {
        string Path { get; set; }
        string TestList { get; set; }
        string TestsPath { get; set; }
        string TestsResultsPath { get; set; }
        string WriteTestRunner(string JobName, string ProjectSpec, string TestCategories, string TestAssembliesList, string TestSettingsFile, string TestsResultsPath, bool RecordScreen, bool Parallelize, Dictionary<string, Tuple<string, string>> JobSpecs);
        string AppendTestAssembly(string TestAssembliesList, string file);
        string TestCategories(string ProjectSpec, string TestCategories, Dictionary<string, Tuple<string, string>> JobSpecs);
        List<string> ReadPlaylist();
        void WritePlaylist(string jobName);
        string AppendProjectFolder(string projectFolder);
    }
}
