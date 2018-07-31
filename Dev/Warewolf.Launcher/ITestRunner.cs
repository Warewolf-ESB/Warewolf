using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warewolf.Launcher
{
    public interface ITestRunner
    {
        string Path { get; set; }
        string TestList { get; set; }
        string TestsPath { get; set; }
        string TestsResultsPath { get; set; }
        string WriteTestRunner(string JobName, string ProjectSpec, string TestCategories, string TestAssembliesList, string TestSettingsFile, string TestsResultsPath, bool RecordScreen, Dictionary<string, Tuple<string, string>> JobSpecs);
        string AppendTestAssembly(string TestAssembliesList, string file);
        string TestCategories(string ProjectSpec, string TestCategories, Dictionary<string, Tuple<string, string>> JobSpecs);
        void ReadPlaylist();
        string AppendProjectFolder(string projectFolder);
    }
}
