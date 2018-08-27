using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Warewolf.Launcher.TestRunners
{
    class VSTestRunner : ITestRunner
    {
        private string _path = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe";

        public string Path
        {
            get
            {
                if (!File.Exists(_path) && File.Exists(_path.Replace("Enterprise", "Professional")))
                {
                    _path = _path.Replace("Enterprise", "Professional");
                }
                if (!File.Exists(_path) && File.Exists(_path.Replace("Enterprise", "Community")))
                {
                    _path = _path.Replace("Enterprise", "Community");
                }
                if (!File.Exists(_path) && File.Exists(_path.Replace("Enterprise", "TestAgent")))
                {
                    _path = _path.Replace("Enterprise", "TestAgent");
                }
                if (!File.Exists(_path) && File.Exists(Environment.GetEnvironmentVariable("VSTESTEXE", EnvironmentVariableTarget.Machine)))
                {
                    _path = Environment.GetEnvironmentVariable("VSTESTEXE", EnvironmentVariableTarget.Machine);
                }
                return _path;
            }
            set => _path = value;
        }
        public string TestList { get; set; }
        public string TestsPath { get; set; } = Environment.CurrentDirectory;
        public string TestsResultsPath { get; set; } = System.IO.Path.Combine(Environment.CurrentDirectory, "TestResults");
        public string WriteTestRunner(string JobName, string ProjectSpec, string TestCategories, string TestAssembliesList, string TestSettingsFile, string TestResultsPath, bool RecordScreen, Dictionary<string, Tuple<string, string>> JobSpecs)
        {
            if (!string.IsNullOrEmpty(TestList) && !TestList.StartsWith(" /Tests:"))
            {
                TestList = $" /Tests:{TestList}";
            }
            string TestRunnerPath;
            Environment.CurrentDirectory = $"{TestsResultsPath}\\..";
            string FullArgsList;
            string TestSettings = "";
            if (RecordScreen)
            {
                TestSettings = $" /Settings:\"{TestSettingsFile}\"";
            }
            string ParallelizeSwitch = "";
            if (RecordScreen)
            {
                ParallelizeSwitch = " /Parallel";
            }
            if (string.IsNullOrEmpty(TestList))
            {
                FullArgsList = $"{TestAssembliesList}{TestSettings}{ParallelizeSwitch} /logger:trx{this.TestCategories(ProjectSpec, TestCategories, JobSpecs)}";
            }
            else
            {
                FullArgsList = $"{TestAssembliesList}{TestSettings}{ParallelizeSwitch} /logger:trx{TestList}";
            }

            // Write full command including full argument string.
            TestRunnerPath = $"{TestsResultsPath}\\..\\Run {JobName}.bat";
            TestCleanupUtils.CopyOnWrite(TestRunnerPath);
            File.WriteAllText(TestRunnerPath, $"\"{Path}\"{FullArgsList}");
            return TestRunnerPath;
        }

        public string AppendTestAssembly(string TestAssembliesList, string file) => TestAssembliesList + " \"" + file + "\"";

        public string TestCategories(string ProjectSpec, string TestCategories, Dictionary<string, Tuple<string, string>> JobSpecs)
        {
            if (string.IsNullOrEmpty(TestList))
            {
                if (!string.IsNullOrEmpty(TestCategories))
                {
                    TestCategories = $" /TestCaseFilter:\"(TestCategory={TestCategories})\"";
                }
                else
                {
                    var DefinedCategories = AllCategoriesDefinedForProject(JobSpecs, ProjectSpec);
                    if (DefinedCategories.Count() > 0)
                    {
                        TestCategories = String.Join(")&(TestCategory!=", DefinedCategories);
                        TestCategories = $" /TestCaseFilter:\"(TestCategory!={TestCategories})\"";
                    }
                }
            }
            else
            {
                TestCategories = "";
            }

            return TestCategories;
        }

        public void ReadPlaylist()
        {
            if (string.IsNullOrEmpty(TestList))
            {
                foreach (var playlistFile in Directory.GetFiles(TestsPath, "*.playlist"))
                {
                    XmlDocument playlistContent = new XmlDocument();
                    playlistContent.Load(playlistFile);
                    if (playlistContent.DocumentElement.SelectNodes("/Playlist/Add").Count > 0)
                    {
                        foreach (XmlNode TestName in playlistContent.DocumentElement.SelectNodes("/Playlist/Add"))
                        {
                            TestList += " /test:" + TestName.Attributes["Test"].InnerText.Substring(TestName.Attributes["Test"].InnerText.LastIndexOf(".") + 1);
                        }
                    }
                    else
                    {
                        if (playlistContent.SelectSingleNode("/Playlist/Add") != null && playlistContent.SelectSingleNode("/Playlist/Add").Attributes["Test"] != null)
                        {
                            TestList = " /test:" + playlistContent.SelectSingleNode("/Playlist/Add").Attributes["Test"].InnerText.Substring(playlistContent.SelectSingleNode("/Playlist/Add").Attributes["Test"].InnerText.LastIndexOf(".") + 1);
                        }
                        else
                        {
                            Console.WriteLine("Error parsing Playlist.Add from playlist file at " + playlistFile);
                        }
                    }
                }
            }
        }

        public string AppendProjectFolder(string projectFolder) => " \"" + projectFolder + "\\bin\\Debug\\" + System.IO.Path.GetFileName(projectFolder) + ".dll\"";

        List<string> AllCategoriesDefinedForProject(Dictionary<string, Tuple<string, string>> JobSpecs, string AssemblyNameToCheck)
        {
            var JobCategorySpecs = new List<string>();
            foreach (var Job in JobSpecs.Values)
            {
                if (!string.IsNullOrEmpty(Job.Item2) && AssemblyNameToCheck == Job.Item1)
                {
                    JobCategorySpecs.Add(Job.Item2);
                }
            }
            return JobCategorySpecs;
        }
    }
}
