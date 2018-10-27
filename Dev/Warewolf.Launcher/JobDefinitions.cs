using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Warewolf.Launcher
{
    public static class Job_Definitions
    {
        public static Dictionary<string, Tuple<string, string>> GetJobDefinitions()
        {
            var JobDefinitions = new Dictionary<string, Tuple<string, string>>();
            try
            {
                using (var repo = new Repository(Properties.Settings.Default.BuildDefinitionsGitURL))
                {
                    var BuildDefintions = ReadBuildDefinitionsFromRepo(repo);
                    foreach (var BuildDefinition in BuildDefintions)
                    {
                        string currentJobName = "";
                        foreach (var CurrentLine in BuildDefinition.Split('\r', '\n'))
                        {
                            if (CurrentLine.Contains("new Job("))
                            {
                                currentJobName = CurrentLine.Trim()
                                    .Replace(".jobs(", "")
                                    .Replace("new Job(\"", "")
                                    .Replace("\",", "")
                                    .Replace("public static Job Job = ", "");
                            }
                            const string parseProjFrom = "--ProjectName \\\"";
                            const string parseCategoryFrom = "--Category \\\"";
                            if (CurrentLine.Contains(parseProjFrom) && !CurrentLine.Contains("#Error Correcting Download"))
                            {
                                var projStartIndex = CurrentLine.IndexOf(parseProjFrom) + parseProjFrom.Length;
                                string projectName = null;
                                while (CurrentLine[projStartIndex] != '\\')
                                {
                                    projectName += CurrentLine[projStartIndex++];
                                }
                                string categoryName = null;
                                int findCategoryStart = CurrentLine.IndexOf(parseCategoryFrom);
                                if (findCategoryStart > 0)
                                {
                                    var categoryStartIndex = findCategoryStart + parseCategoryFrom.Length;
                                    while (CurrentLine[categoryStartIndex] != '\\')
                                    {
                                        categoryName += CurrentLine[categoryStartIndex++];
                                    }
                                }
                                if (!JobDefinitions.ContainsKey(currentJobName))
                                {
                                    JobDefinitions.Add(currentJobName, new Tuple<string, string>(projectName, categoryName));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to get build config from {Properties.Settings.Default.BuildDefinitionsGitURL}\n{e.Message}");
            }
            if (JobDefinitions.Count > 0)
            {
                return JobDefinitions;
            }
            return new Dictionary<string, Tuple<string, string>>
            {
                //Unit Tests
                ["Other Unit Tests"] = new Tuple<string, string>("Dev2.*.Tests,Warewolf.*.Tests", null),
                ["Infrastructure Unit Tests"] = new Tuple<string, string>("Dev2.Infrastructure.Tests", null),
                ["Runtime Unit Tests"] = new Tuple<string, string>("Dev2.Runtime.Tests", null),
                ["MS SQL Server Unit Tests"] = new Tuple<string, string>("Dev2.Runtime.Tests", "MSSql"),
                ["Core Unit Tests"] = new Tuple<string, string>("Dev2.Core.Tests", null),
                ["Data Unit Tests"] = new Tuple<string, string>("Dev2.Data.Tests", null),
                ["Parsing Unit Tests"] = new Tuple<string, string>("Warewolf.Parsing.Tests", null),
                ["Storage Unit Tests"] = new Tuple<string, string>("Warewolf.Storage.Tests", null),
                ["Studio Core Unit Tests"] = new Tuple<string, string>("Dev2.Studio.Core.Tests", null),
                ["COMIPC Unit Tests"] = new Tuple<string, string>("Warewolf.COMIPC.Tests", null),
                ["Studio View Models Unit Tests"] = new Tuple<string, string>("Warewolf.Studio.ViewModels.Tests", null),
                ["Request Service Name View Models Unit Tests"] = new Tuple<string, string>("Warewolf.Studio.ViewModels.Tests", "RequestServiceNameViewModel"),
                ["Activity Designers Unit Tests"] = new Tuple<string, string>("Dev2.Activities.Designers.Tests", null),
                ["Activities Unit Tests"] = new Tuple<string, string>("Dev2.Activities.Tests", null),
                ["Other Tools Specs"] = new Tuple<string, string>("Warewolf.Tools.Specs", null),
                ["Scripting Tools Specs"] = new Tuple<string, string>("Warewolf.Tools.Specs", "Scripting"),
                ["Storage Tools Specs"] = new Tuple<string, string>("Warewolf.Tools.Specs", "Storage"),
                ["Utility Tools Specs"] = new Tuple<string, string>("Warewolf.Tools.Specs", "Utility"),
                ["Control Flow Tools Specs"] = new Tuple<string, string>("Warewolf.Tools.Specs", "ControlFlow"),
                ["Data Tools Specs"] = new Tuple<string, string>("Warewolf.Tools.Specs", "Data"),
                ["Email Tools Specs"] = new Tuple<string, string>("Warewolf.Tools.Specs", "Email"),
                ["File And Folder Copy Tool Specs"] = new Tuple<string, string>("Warewolf.Tools.Specs", "FileAndFolderCopy"),
                ["File And Folder Create Tool Specs"] = new Tuple<string, string>("Warewolf.Tools.Specs", "FileAndFolderCreate"),
                ["File And Folder Delete Tool Specs"] = new Tuple<string, string>("Warewolf.Tools.Specs", "FileAndFolderDelete"),
                ["File And Folder Move Tool Specs"] = new Tuple<string, string>("Warewolf.Tools.Specs", "FileAndFolderMove"),
                ["Folder Read Tool Specs"] = new Tuple<string, string>("Warewolf.Tools.Specs", "ReadFolder"),
                ["New Folder Read Tool Specs"] = new Tuple<string, string>("Warewolf.Tools.Specs", "NewReadFolder"),
                ["File Read Tool Specs"] = new Tuple<string, string>("Warewolf.Tools.Specs", "ReadFile"),
                ["File And Folder Rename Tool Specs"] = new Tuple<string, string>("Warewolf.Tools.Specs", "FileAndFolderRename"),
                ["Unzip Tool Specs"] = new Tuple<string, string>("Warewolf.Tools.Specs", "Unzip"),
                ["Write File Tool Specs"] = new Tuple<string, string>("Warewolf.Tools.Specs", "WriteFile"),
                ["Zip Tool Specs"] = new Tuple<string, string>("Warewolf.Tools.Specs", "Zip"),
                ["Loop Construct Tools Specs"] = new Tuple<string, string>("Warewolf.Tools.Specs", "LoopConstructs"),
                ["Recordset Tools Specs"] = new Tuple<string, string>("Warewolf.Tools.Specs", "Recordset"),
                ["Resource Tools Specs"] = new Tuple<string, string>("Warewolf.Tools.Specs", "Resources"),
                ["UI Binding Tests"] = new Tuple<string, string>("Warewolf.UIBindingTests.*", null),
                //Server Tests
                ["Database Tools Specs"] = new Tuple<string, string>("Warewolf.Tools.Specs", "Database"),
                ["SQL Bulk Insert Tool Specs"] = new Tuple<string, string>("Warewolf.Tools.Specs", "SqlBulkInsert"),
                ["Integration Tests"] = new Tuple<string, string>("Dev2.Integration.Tests", null),
                ["Load Tests"] = new Tuple<string, string>("Dev2.Integration.Tests", "Load Tests"),
                ["Other Specs"] = new Tuple<string, string>("Dev2.*.Specs,Warewolf.*.Specs", null),
                ["Other Activities Specs"] = new Tuple<string, string>("Dev2.Activities.Specs", null),
                ["Scheduler Specs"] = new Tuple<string, string>("Dev2.Activities.Specs", "Scheduler"),
                ["Remote Server Specs"] = new Tuple<string, string>("Dev2.Activities.Specs", "RemoteServer"),
                ["Workflow Merging Specs"] = new Tuple<string, string>("Dev2.Activities.Specs", "WorkflowMerging"),
                ["Subworkflow Execution Specs"] = new Tuple<string, string>("Dev2.Activities.Specs", "SubworkflowExecution"),
                ["Workflow Execution Specs"] = new Tuple<string, string>("Dev2.Activities.Specs", "WorkflowExecution"),
                ["MS SQL Server Specs"] = new Tuple<string, string>("Dev2.Activities.Specs", "MSSql"),
                ["Assign Workflow Execution Specs"] = new Tuple<string, string>("Dev2.Activities.Specs", "AssignWorkflowExecution"),
                ["Studio Test Framework Specs"] = new Tuple<string, string>("Dev2.Activities.Specs", "StudioTestFramework"),
                ["Studio Test Framework With Data Tools Specs"] = new Tuple<string, string>("Dev2.Activities.Specs", "StudioTestFrameworkWithDataTools"),
                ["Studio Test Framework With Database Tools Specs"] = new Tuple<string, string>("Dev2.Activities.Specs", "StudioTestFrameworkWithDatabaseTools"),
                ["Studio Test Framework With Deleted Resources Specs"] = new Tuple<string, string>("Dev2.Activities.Specs", "StudioTestFrameworkWithDeletedResources"),
                ["Studio Test Framework With Dropbox Tools Specs"] = new Tuple<string, string>("Dev2.Activities.Specs", "StudioTestFrameworkWithDropboxTools"),
                ["Studio Test Framework With File And Folder Tools Specs"] = new Tuple<string, string>("Dev2.Activities.Specs", "StudioTestFrameworkWithFileAndFolderTools"),
                ["Studio Test Framework With Hello World Workflow Specs"] = new Tuple<string, string>("Dev2.Activities.Specs", "StudioTestFrameworkWithHelloWorldWorkflow"),
                ["Studio Test Framework With HTTP Web Tools Specs"] = new Tuple<string, string>("Dev2.Activities.Specs", "StudioTestFrameworkWithHTTPWebTools"),
                ["Studio Test Framework With Scripting Tools Specs"] = new Tuple<string, string>("Dev2.Activities.Specs", "StudioTestFrameworkWithScriptingTools"),
                ["Studio Test Framework With Subworkflow Specs"] = new Tuple<string, string>("Dev2.Activities.Specs", "StudioTestFrameworkWithSubworkflow"),
                ["Studio Test Framework With Utility Tools Specs"] = new Tuple<string, string>("Dev2.Activities.Specs", "StudioTestFrameworkWithUtilityTools"),
                ["Other Security Specs"] = new Tuple<string, string>("Warewolf.Security.Specs", null),
                ["Conflicting Contribute View And Execute Permissions Security Specs"] = new Tuple<string, string>("Warewolf.Security.Specs", "ConflictingContributeViewExecutePermissionsSecurity"),
                ["Conflicting Execute Permissions Security Specs"] = new Tuple<string, string>("Warewolf.Security.Specs", "ConflictingExecutePermissionsSecurity"),
                ["Conflicting View And Execute Permissions Security Specs"] = new Tuple<string, string>("Warewolf.Security.Specs", "ConflictingViewExecutePermissionsSecurity"),
                ["Conflicting View Permissions Security Specs"] = new Tuple<string, string>("Warewolf.Security.Specs", "ConflictingViewPermissionsSecurity"),
                ["No Conflicting Permissions Security Specs"] = new Tuple<string, string>("Warewolf.Security.Specs", "NoConflictingPermissionsSecurity"),
                ["Overlapping User Groups Permissions Security Specs"] = new Tuple<string, string>("Warewolf.Security.Specs", "OverlappingUserGroupsPermissionsSecurity"),
                ["Resource Permissions Security Specs"] = new Tuple<string, string>("Warewolf.Security.Specs", "ResourcePermissionsSecurity"),
                ["Server Permissions Security Specs"] = new Tuple<string, string>("Warewolf.Security.Specs", "ServerPermissionsSecurity"),
                //Release Resource Tests
                ["Example Workflow Execution Specs"] = new Tuple<string, string>("Dev2.Activities.Specs", "ExampleWorkflowExecution"),
                //Desktop UI Tests
                ["Other UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", null),
                ["Other UI Specs"] = new Tuple<string, string>("Warewolf.UI.Specs", null),
                ["Assign Tool UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Assign Tool"),
                ["Control Flow Tools UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Control Flow Tools"),
                ["Database Sources UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Database Sources"),
                ["Database Tools UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Database Tools"),
                ["MS SQL Server UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "MSSql"),
                ["Data Tools UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Data Tools"),
                ["DB Connector UI Specs"] = new Tuple<string, string>("Warewolf.UI.Specs", "DBConnector"),
                ["Debug Input UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Debug Input"),
                ["Default Layout UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Default Layout"),
                ["Studio Shutdown UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Studio Shutdown"),
                ["Dependency Graph UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Dependency Graph"),
                ["Deploy UI Specs"] = new Tuple<string, string>("Warewolf.UI.Specs", "Deploy"),
                ["Deploy Security UI Specs"] = new Tuple<string, string>("Warewolf.UI.Specs", "DeploySecurity"),
                ["Deploy UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Deploy"),
                ["Deploy from Explorer UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Deploy from Explorer"),
                ["Deploy from Remote UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Deploy from Remote"),
                ["Deploy Filtering UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Deploy Filtering"),
                ["Deploy Hello World UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Deploy Hello World"),
                ["Deploy Select Dependencies UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Deploy Select Dependencies"),
                ["DotNet Connector Mocking UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "DotNet Connector Mocking Tests"),
                ["DotNet Connector Tool UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "DotNet Connector Tool"),
                ["Dropbox Tools UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Dropbox Tools"),
                ["Email Tools UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Email Tools"),
                ["Explorer UI Specs"] = new Tuple<string, string>("Warewolf.UI.Specs", "Explorer"),
                ["Explorer UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Explorer"),
                ["File Tools UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "File Tools"),
                ["Hello World Mocking UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Hello World Mocking Tests"),
                ["HTTP Tools UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "HTTP Tools"),
                ["Plugin Sources UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Plugin Sources"),
                ["Recordset Tools UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Recordset Tools"),
                ["Resource Tools UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Resource Tools"),
                ["Save Dialog UI Specs"] = new Tuple<string, string>("Warewolf.UI.Specs", "SaveDialog"),
                ["Save Dialog UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Save Dialog"),
                ["Server Sources UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Server Sources"),
                ["Settings UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Settings"),
                ["Sharepoint Tools UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Sharepoint Tools"),
                ["Shortcut Keys UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Shortcut Keys"),
                ["Source Wizards UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Source Wizards"),
                ["Tabs and Panes UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Tabs and Panes"),
                ["Tools UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Tools"),
                ["Utility Tools UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Utility Tools"),
                ["Variables UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Variables"),
                ["Web Connector UI Specs"] = new Tuple<string, string>("Warewolf.UI.Specs", "WebConnector"),
                ["Web Sources UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Web Sources"),
                ["Workflow Mocking Tests UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Workflow Mocking Tests"),
                ["Workflow Testing UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Workflow Testing"),
                ["Workflow Merge with All Tools Conflicting"] = new Tuple<string, string>("Warewolf.UI.Tests", "Merge All Tools Conflicts"),
                ["Workflow Merge with Assign Tools Conflicting"] = new Tuple<string, string>("Warewolf.UI.Tests", "Merge Assign Conflicts"),
                ["Workflow Merge with Decision Tools Conflicting"] = new Tuple<string, string>("Warewolf.UI.Tests", "Merge Decision Conflicts"),
                ["Workflow Merge with Foreach Tools Conflicting"] = new Tuple<string, string>("Warewolf.UI.Tests", "Merge Foreach"),
                ["Workflow Merge with Sequence Tools Conflicting"] = new Tuple<string, string>("Warewolf.UI.Tests", "Merge Sequence Conflicts"),
                ["Workflow Merge with Simple Tools Conflicting"] = new Tuple<string, string>("Warewolf.UI.Tests", "Merge Simple Tools Conflicts"),
                ["Workflow Merge with Switch Tools Conflicting"] = new Tuple<string, string>("Warewolf.UI.Tests", "Merge Switch Conflicts"),
                ["Workflow Merge with Variables Conflicting"] = new Tuple<string, string>("Warewolf.UI.Tests", "Merge Variable Conflicts"),
                ["Search UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Search"),
                ["Input Search UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Input Search"),
                ["Output Search UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Output Search"),
                ["Test Name Search UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Test Name Search"),
                ["Scalar Search UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Scalar Search"),
                ["Recordset Search UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Recordset Search"),
                ["Object Search UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Object Search"),
                ["Service Search UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Service Search"),
                ["Title Search UI Tests"] = new Tuple<string, string>("Warewolf.UI.Tests", "Title Search"),
                //Web UI Tests
                ["Other Web UI Tests"] = new Tuple<string, string>("Warewolf.Web.UI.Tests", null),
                ["Execution Logging Web UI Tests"] = new Tuple<string, string>("Warewolf.Web.UI.Tests", "ExecutionLogging"),
                ["No Warewolf Server Web UI Tests"] = new Tuple<string, string>("Warewolf.Web.UI.Tests", "NoWarewolfServer"),
                //Load Tests
                ["Composition Load Tests"] = new Tuple<string, string>("Dev2.Activities.Specs", "CompositionLoadTests"),
                ["UI Load Specs"] = new Tuple<string, string>("Warewolf.UI.Load.Specs", null)
            };
        }

        static List<string> ReadBuildDefinitionsFromRepo(Repository repo)
        {
            CheckoutBranch(ref repo);
            List<string> result = new List<string>();
            TreeEntry TreeWalker = null;
            foreach (var TreeEntry in ((Tree)((Tree)((Tree)((Tree)((Tree)((Tree)repo.Head.Tip.Tree[@"bamboo-specs"].Target)["src"].Target)["main"].Target)["java"].Target)["com"].Target)["CI"].Target))
            {
                if (TreeEntry.TargetType == TreeEntryTargetType.Blob)
                {
                    result.Add(ReadTreeEntry(TreeEntry));
                }
            }
            foreach (var TreeEntry in ((Tree)((Tree)((Tree)((Tree)((Tree)((Tree)repo.Head.Tip.Tree[@"bamboo-specs"].Target)["src"].Target)["main"].Target)["java"].Target)["com"].Target)["Nightlies"].Target))
            {
                if (TreeEntry.TargetType == TreeEntryTargetType.Blob)
                {
                    result.Add(ReadTreeEntry(TreeEntry));
                }
            }
            foreach (var TreeEntry in ((Tree)((Tree)((Tree)((Tree)((Tree)((Tree)repo.Head.Tip.Tree[@"bamboo-specs"].Target)["src"].Target)["main"].Target)["java"].Target)["com"].Target)["Single_Jobs"].Target))
            {
                if (TreeEntry.TargetType == TreeEntryTargetType.Blob)
                {
                    result.Add(ReadTreeEntry(TreeEntry));
                }
            }
            return result;
        }

        static string ReadFileFromRepo(Repository repo, string fileName)
        {
            CheckoutBranch(ref repo);
            var commit = repo.Head.Tip;
            var treeEntry = commit[fileName];
            return ReadTreeEntry(treeEntry);
        }

        private static string ReadTreeEntry(TreeEntry treeEntry)
        {
            var blob = (Blob)treeEntry.Target;
            var contentStream = blob.GetContentStream();
            string JobDefinitionsCSV;
            using (var tr = new StreamReader(contentStream, Encoding.UTF8))
            {
                JobDefinitionsCSV = tr.ReadToEnd();
            }

            return JobDefinitionsCSV;
        }

        static void CheckoutBranch(ref Repository repo)
        {
            if (repo.Branches[Properties.Settings.Default.BuildDefinitionGitBranch] != null)
            {
                if (Properties.Settings.Default.BuildDefinitionGitBranch != "master")
                {
                    repo.Refs.UpdateTarget(repo.Refs.Head, repo.Refs["refs/heads/" + Properties.Settings.Default.BuildDefinitionGitBranch]);
                }
            }
            else
            {
                throw new ArgumentException($"Unrecognized branch {Properties.Settings.Default.BuildDefinitionGitBranch} for repo {Properties.Settings.Default.BuildDefinitionsGitURL}");
            }
        }

        public static bool GetEnableDockerValue()
        {
            if (File.Exists("EnableDocker.txt"))
            {
                return File.ReadAllText("EnableDocker.txt") == "True";
            }
            else
            {
                string JobDefinitionsCSV = "";
                try
                { 
                    using (var repo = new Repository(Properties.Settings.Default.BuildDefinitionsGitURL))
                    {
                        JobDefinitionsCSV = ReadFileFromRepo(repo, "EnableDocker.txt");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to get Docker enable build config from {Properties.Settings.Default.BuildDefinitionsGitURL}\n{e.Message}");
                }
                return JobDefinitionsCSV == "True";
            }
        }
    }
}
