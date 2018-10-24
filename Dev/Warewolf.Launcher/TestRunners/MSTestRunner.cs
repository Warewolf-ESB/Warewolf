using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Warewolf.Launcher.TestRunners
{
    class MSTestRunner : ITestRunner
    {
        private string _path = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\MSTest.exe";

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
                if (!File.Exists(_path) && File.Exists(Environment.GetEnvironmentVariable("MSTESTEXE", EnvironmentVariableTarget.Machine)))
                {
                    _path = Environment.GetEnvironmentVariable("MSTESTEXE", EnvironmentVariableTarget.Machine);
                }
                return _path;
            }
            set => _path = value;
        }
        public string TestList { get; set; }
        public string TestsPath { get; set; } = Environment.CurrentDirectory;
        public string TestsResultsPath { get; set; } = System.IO.Path.Combine(Environment.CurrentDirectory, "TestResults");
        public string WriteTestRunner(string JobName, string ProjectSpec, string TestCategories, string TestAssembliesList, string TestSettingsFile, string TestsResultsPath, bool RecordScreen, bool Parallelize, Dictionary<string, Tuple<string, string>> JobSpecs)
        {
            if (!string.IsNullOrEmpty(TestList) && !TestList.StartsWith(" /test:"))
            {
                TestList = $" /test:{TestList.Replace(",", " /test:")}";
            }
            // Resolve test results file name
            var TestResultsFile = TestsResultsPath + $"\"{JobName} Results.trx";
            TestCleanupUtils.CopyOnWrite(TestResultsFile);

            // Create full MSTest argument string.
            string categories = this.TestCategories(ProjectSpec, TestCategories, JobSpecs);
            string FullArgsList;
            string TestSettings = "";
            if (RecordScreen)
            {
                TestSettings = $" /Settings:\"{TestSettingsFile}\"";
            }
            if (TestList == "")
            {
                FullArgsList = $"{TestAssembliesList} /resultsfile:\"{TestResultsFile}\"{TestSettings}{categories}";
            }
            else
            {
                FullArgsList = $"{TestAssembliesList} /resultsfile:\"{TestResultsFile}\"{TestSettings}{TestList}";
            }

            // Write full command including full argument string.
            var TestRunnerPath = $"{TestsResultsPath}\\..\\Run {JobName}.bat";
            TestCleanupUtils.CopyOnWrite(TestRunnerPath);
            File.WriteAllText(TestRunnerPath, $"\"{Path}\"{FullArgsList}");
            return TestRunnerPath;
        }

        public string AppendTestAssembly(string TestAssembliesList, string file)
        {
            TestAssembliesList += " /testcontainer:\"" + file + "\"";
            return TestAssembliesList;
        }

        public string TestCategories(string ProjectSpec, string TestCategories, Dictionary<string, Tuple<string, string>> JobSpecs)
        {
            if (String.IsNullOrEmpty(TestList))
            {
                if (!String.IsNullOrEmpty(TestCategories))
                {
                    TestCategories = $" /category:\"{TestCategories}\"";
                }
                else
                {
                    var DefinedCategories = AllCategoriesDefinedForProject(JobSpecs, ProjectSpec);
                    if (DefinedCategories.Any())
                    {
                        TestCategories = string.Join("&!", DefinedCategories);
                        TestCategories = $" /category:\"!{TestCategories}\"";
                    }
                }
            }
            else
            {
                TestCategories = "";
            }

            return TestCategories;
        }

        public List<string> ReadPlaylist()
        {
            var testList = new List<string>();
            foreach (var playlistFile in Directory.GetFiles(TestsPath, "*.playlist"))
            {
                XmlDocument playlistContent = new XmlDocument();
                playlistContent.Load(playlistFile);
                if (playlistContent.DocumentElement.SelectNodes("/Playlist/Add").Count > 0)
                {
                    TestList = " /Tests:";
                    foreach (XmlNode TestName in playlistContent.DocumentElement.SelectNodes("/Playlist/Add"))
                    {
                        string fullTestName = TestName.Attributes["Test"].InnerText;
                        string testName = fullTestName.Substring(fullTestName.LastIndexOf(".") + 1);
                        testList.Add(fullTestName);
                        TestList += "," + testName;
                    }
                }
                else
                {
                    if (playlistContent.SelectSingleNode("/Playlist/Add") != null && playlistContent.SelectSingleNode("/Playlist/Add").Attributes["Test"] != null)
                    {
                        string fullTestName = playlistContent.SelectSingleNode("/Playlist/Add").Attributes["Test"].InnerText;
                        string testName = fullTestName.Substring(fullTestName.LastIndexOf(".") + 1);
                        testList.Add(fullTestName);
                        TestList = " /Tests:" + testName;
                    }
                    else
                    {
                        Console.WriteLine("Error parsing Playlist.Add from playlist file at " + playlistFile);
                    }
                }
            }
            return testList;
        }

        public void WritePlaylist(string jobName)
        {
            if (Directory.Exists(TestsResultsPath))
            {
                foreach (var FullTRXFilePath in Directory.GetFiles(TestsResultsPath, "*.trx"))
                {
                    XmlDocument trxContent = new XmlDocument();
                    trxContent.Load(FullTRXFilePath);
                    var namespaceManager = new XmlNamespaceManager(trxContent.NameTable);
                    namespaceManager.AddNamespace("a", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010");
                    if (trxContent.DocumentElement.SelectSingleNode("/a:TestRun/a:ResultSummary", namespaceManager).Attributes["outcome"].Value != "Completed")
                    {
                        WriteFailingTestPlaylist($"{TestsResultsPath}\\{jobName} Failures.playlist", FullTRXFilePath, trxContent, namespaceManager);
                    }
                }
            }
        }

        void WriteFailingTestPlaylist(string OutPlaylistPath, string FullTRXFilePath, XmlDocument trxContent, XmlNamespaceManager namespaceManager)
        {
            Console.WriteLine($"Writing all test failures in \"{FullTRXFilePath}\" to a playlist file \"{OutPlaylistPath}\".");
            var playlist = "<Playlist Version=\"1.0\">";
            if (File.Exists(OutPlaylistPath))
            {
                playlist += "<Add Test=\"" + string.Join("\" /><Add Test=\"", ReadPlaylist()) + "\" />";
                File.Delete(OutPlaylistPath);
            }
            if (trxContent.DocumentElement.SelectNodes("/a:TestRun/a:Results/a:UnitTestResult", namespaceManager).Count > 0)
            {
                foreach (XmlNode TestResult in trxContent.DocumentElement.SelectNodes("/a:TestRun/a:Results/a:UnitTestResult", namespaceManager))
                {
                    if (TestResult.Attributes["outcome"] == null || TestResult.Attributes["outcome"].InnerText == "Failed")
                    {
                        if (trxContent.DocumentElement.SelectNodes("/a:TestRun/a:TestDefinitions/a:UnitTest/a:TestMethod", namespaceManager).Count > 0)
                        {
                            foreach (XmlNode TestDefinition in trxContent.DocumentElement.SelectNodes("/a:TestRun/a:TestDefinitions/a:UnitTest/a:TestMethod", namespaceManager))
                            {
                                if (TestResult.Attributes["testName"] != null && TestDefinition.Attributes["name"].InnerText == TestResult.Attributes["testName"].InnerText)
                                {
                                    var fullTestName = TestDefinition.Attributes["className"].InnerText + "." + TestDefinition.Attributes["name"].InnerText;
                                    if (!playlist.Contains(fullTestName))
                                    {
                                        playlist += "<Add Test=\"" + fullTestName + "\" />";
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error parsing /TestRun/TestDefinitions/UnitTest/TestMethod from trx file at trxFile");
                        }
                    }
                }
            }
            else
            {
                if (trxContent.DocumentElement.SelectSingleNode("/a:TestRun/a:Results/a:UnitTestResult", namespaceManager).Attributes["outcome"].InnerText == "Failed")
                {
                    playlist += "<Add Test=\"" + trxContent.DocumentElement.SelectSingleNode("/a:TestRun/a:TestDefinitions/a:UnitTest/a:TestMethod", namespaceManager).Attributes["className"].InnerText + "." + trxContent.DocumentElement.SelectSingleNode("/a:TestRun/a:TestDefinitions/a:UnitTest/a:TestMethod", namespaceManager).Attributes["name"].InnerText + "\" />";
                }
                else
                {
                    if (trxContent.DocumentElement.SelectSingleNode("/a:TestRun/a:Results/a:UnitTestResult", namespaceManager) == null)
                    {
                        Console.WriteLine("Error parsing /TestRun/Results/UnitTestResult from trx file at " + FullTRXFilePath);
                    }
                }
            }
            playlist += "</Playlist>";
            File.WriteAllText(OutPlaylistPath, playlist);
            Console.WriteLine($"Playlist file written to \"{OutPlaylistPath}\".");
        }

        public string AppendProjectFolder(string projectFolder) => " /testcontainer:\"" + projectFolder + "\\bin\\Debug\\" + System.IO.Path.GetFileName(projectFolder) + ".dll\"";

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
