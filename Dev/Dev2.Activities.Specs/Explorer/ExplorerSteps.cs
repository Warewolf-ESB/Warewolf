using System.Collections.ObjectModel;
using Dev2.AppResources.Repositories;
using Dev2.Data.ServiceModel;
using Dev2.Models;
using Dev2.Network;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Management;
using System.Linq;
using System.Linq.Expressions;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Explorer
{
    [Binding]
    public class ExplorerSteps
    {
        const string ServerProcessName = "Warewolf Server";

        [Given(@"I have a path '(.*)'")]
        public void GivenIHaveAPath(string path)
        {
            var paths = path.Split("\\".ToArray()).ToList();
            var root = paths.First();
            var serverProcessPath = GetServerProcessPath();
            if(serverProcessPath == null || !File.Exists(serverProcessPath))
            {
                Assert.Fail("Warewolf server not running or file not accessible '" + serverProcessPath + "'.");
            }
            var directoryName = Path.GetDirectoryName(serverProcessPath);
            if(directoryName != null)
            {
                var basePath = Path.Combine(directoryName, "Resources");
                var workingDirectory = Path.Combine(basePath, root);
                Console.WriteLine(workingDirectory);
                if(Directory.Exists(workingDirectory))
                {
                    Directory.Delete(workingDirectory, true);
                }

                Directory.CreateDirectory(workingDirectory);
                paths.Remove(root);

                foreach(var p in paths)
                {
                    Directory.CreateDirectory(workingDirectory + @"\" + p);
                    workingDirectory += @"\" + p;
                }

                ScenarioContext.Current.Add("path", path);
                ScenarioContext.Current.Add("basePath", basePath);
                ScenarioContext.Current.Add("workingDirectory", workingDirectory);
            }
        }

        private static string GetServerProcessPath()
        {
            var query = new SelectQuery(@"SELECT * FROM Win32_Process where Name LIKE '%" + ServerProcessName + ".exe'");
            ManagementObjectCollection processes;
            //initialize the searcher with the query it is
            //supposed to execute
            using(ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                //execute the query
                processes = searcher.Get();
                if(processes.Count <= 0)
                {
                    return null;
                }
            }
            if(processes.Count == 0)
            {
                return null;
            }
            return (from ManagementObject process in processes select (process.Properties["ExecutablePath"].Value ?? string.Empty).ToString()).FirstOrDefault();
        }

        [Given(@"the folder '(.*)' exists on the server '(.*)'")]
        public void GivenTheFolderExistsOnTheServer(string folderName, string exists)
        {
            if(bool.Parse(exists))
            {
                var workingDirectory = ScenarioContext.Current.Get<string>("workingDirectory");
                Directory.CreateDirectory(workingDirectory + @"\" + folderName);
            }
        }

        [When(@"I add a folder with a name  '(.*)'")]
        public void WhenIAddAFolderWithAName(string folderName)
        {
            var localhost = Guid.Empty;

            ExecuteService(() =>
                {
                    var repository = ScenarioContext.Current.Get<StudioResourceRepository>("repository");
                    repository.Load(localhost, new TestAsyncWorker());
                    var resourcePath = ScenarioContext.Current.Get<string>("path");
                    var displayName = resourcePath.Split("\\".ToArray()).Last();
                    var parent = repository.FindItem(i => i.DisplayName == displayName);
                    string errorMessage = "";
                    var hasError = false;

                    try
                    {
                        repository.AddItem(new ExplorerItemModel
                        {
                            DisplayName = folderName,
                            EnvironmentId = localhost,
                            ResourcePath = resourcePath + "\\" + folderName,
                            Permissions = Services.Security.Permissions.Contribute,
                            ResourceType = ResourceType.Folder,
                            Parent = parent
                        });
                    }
                    catch(Exception exception)
                    {
                        errorMessage = exception.Message;
                        hasError = true;
                    }

                    ScenarioContext.Current.Add("errorMessage", errorMessage);
                    ScenarioContext.Current.Add("hasError", hasError);
                });
        }

        static void ExecuteService(Action action)
        {
            var localhost = Guid.Empty;

            var environmentRepository = new Mock<IEnvironmentRepository>();
            var environmentModel = new Mock<IEnvironmentModel>();

            environmentModel.SetupGet(env => env.IsConnected)
                            .Returns(true);

            environmentRepository.Setup(m => m.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>()))
                                 .Returns(environmentModel.Object);

            var repository = new StudioResourceRepository(null, localhost, (a, b) => a())
                {
                    GetCurrentEnvironment = () => localhost,
                    GetExplorerProxy = id =>
                        {
                            var connection = new ServerProxy(new Uri(string.Format("http://{0}:3142", "localhost")));
                            connection.Connect();
                            return new ServerExplorerClientProxy(connection);
                        },
                    GetEnvironmentRepository = () => environmentRepository.Object
                };

            ScenarioContext.Current.Add("repository", repository);
            action();
        }

        [Then(@"the folder path will be '(.*)'")]
        public void ThenTheFolderPathWillBe(string resultPath)
        {
            var basePath = ScenarioContext.Current.Get<string>("basePath");
            var path = Path.Combine(basePath, resultPath);
            Assert.IsTrue(Directory.Exists(path), "Expected path '" + path + "' does not exist or is not accessible.");
        }

        [Given(@"the resource '(.*)' exists on the server '(.*)'")]
        public void GivenTheResourceExistsOnTheServer(string newName, string exists)
        {
            if(bool.Parse(exists))
            {
                var workingDirectory = ScenarioContext.Current.Get<string>("workingDirectory");
                var resourceName = workingDirectory + @"\" + newName;
                var extension = Path.GetExtension(resourceName);
                if(string.IsNullOrEmpty(extension))
                {
                    Directory.CreateDirectory(resourceName);
                }
                else
                {
                    File.Create(resourceName);
                }
            }
        }

        [When(@"I rename the resource '(.*)' to '(.*)'")]
        public void WhenIRenameTheResourceTo(string oldResourceName, string newResourceName)
        {
            var workingDirectory = ScenarioContext.Current.Get<string>("workingDirectory");
            var resourceName = workingDirectory + @"\" + oldResourceName;
            var extension = Path.GetExtension(resourceName);
            if(string.IsNullOrEmpty(extension))
            {
                Directory.CreateDirectory(resourceName);
            }
            else
            {
                File.Create(resourceName);
            }

            var localhost = Guid.Empty;

            ExecuteService(() =>
            {
                var repository = ScenarioContext.Current.Get<StudioResourceRepository>("repository");
                repository.Load(localhost, new TestAsyncWorker());
                var resourcePath = ScenarioContext.Current.Get<string>("path");
                var displayName = resourcePath.Split("\\".ToArray()).Last();
                var parent = repository.FindItem(i => i.DisplayName == displayName);
                var child = parent.Children.FirstOrDefault(c => c.DisplayName == oldResourceName);
                var hasError = false;

                if(child == null)
                {
                    child = new ExplorerItemModel
                    {
                        Parent = parent,
                        DisplayName = oldResourceName,
                        ResourcePath = resourcePath + "\\" + displayName,
                        Permissions = Services.Security.Permissions.Contribute
                    };
                    parent.Children.Add(child);
                }

                string errorMessage = "";

                try
                {
                    repository.RenameItem(child, resourcePath + "\\" + newResourceName);
                }
                catch(Exception exception)
                {
                    errorMessage = exception.Message;
                    hasError = true;
                }

                ScenarioContext.Current.Add("errorMessage", errorMessage);
                ScenarioContext.Current.Add("hasError", hasError);
            });
        }

        [When(@"I delete the resource '(.*)'")]
        public void WhenIDeleteTheResource(string resourceToDelete)
        {
            var localhost = Guid.Empty;

            ExecuteService(() =>
            {
                var repository = ScenarioContext.Current.Get<StudioResourceRepository>("repository");
                repository.Load(localhost, new TestAsyncWorker());
                var resourcePath = ScenarioContext.Current.Get<string>("path");
                var displayName = resourcePath.Split("\\".ToArray()).Last();
                var parent = repository.FindItem(i => i.DisplayName == displayName);
                var child = parent.Children.FirstOrDefault(c => c.DisplayName == resourceToDelete);

                if(child == null)
                {
                    child = new ExplorerItemModel
                        {
                            Parent = parent,
                            DisplayName = resourceToDelete,
                            ResourcePath = resourcePath + "\\" + resourceToDelete,
                            Permissions = Services.Security.Permissions.Contribute
                        };
                    parent.Children.Add(child);
                }

                string errorMessage = "";
                var hasError = false;

                try
                {
                    repository.DeleteFolder(child);
                }
                catch(Exception exception)
                {
                    errorMessage = exception.Message;
                    hasError = true;
                }

                ScenarioContext.Current.Add("errorMessage", errorMessage);
                ScenarioContext.Current.Add("hasError", hasError);
            });
        }

        [Then(@"the folder path '(.*)' will be deleted")]
        public void ThenTheFolderPathWillBeDeleted(string folderPath)
        {
            var basePath = ScenarioContext.Current.Get<string>("basePath");
            var fullPath = basePath + folderPath;
            Assert.IsFalse(Directory.Exists(fullPath), "Directory does exist after delete " + fullPath);
        }

        [Then(@"an error message will be '(.*)'")]
        public void ThenAnErrorMessageWillBe(string expectedMessage)
        {
            var actualMessage = ScenarioContext.Current.Get<string>("errorMessage");
            Assert.IsTrue(actualMessage.Contains(expectedMessage), actualMessage + " does not contain " + expectedMessage + ".");
        }

        [Given(@"I have string '(.*)'")]
        public void GivenIHaveString(string filterString)
        {
            ScenarioContext.Current.Add("filterString", filterString);
        }

        [When(@"I filter")]
        public void WhenIFilter()
        {
            var localhost = Guid.Empty;

            ExecuteService(() =>
            {
                var repository = ScenarioContext.Current.Get<StudioResourceRepository>("repository");
                repository.Load(localhost, new TestAsyncWorker());
                var filterString = ScenarioContext.Current.Get<string>("filterString");
                var filterResult = repository.Filter(s => s.DisplayName.Contains(filterString));
                ScenarioContext.Current.Add("filterResult", filterResult);
            });
        }

        [Then(@"the filtered results will be '(.*)'")]
        public void ThenTheFilteredResultsWillBe(string result)
        {
            var splitResult = result.Split(',');
            var filterResult = ScenarioContext.Current.Get<ObservableCollection<ExplorerItemModel>>("filterResult");

            foreach(var s in splitResult)
            {
                string s1 = s;
                Assert.IsNotNull(FindItem(filterResult, m => m.DisplayName.Equals(s1)));
            }
        }

        public ExplorerItemModel FindItem(ObservableCollection<ExplorerItemModel> models, Func<ExplorerItemModel, bool> searchCriteria)
        {
            var explorerItemModels = models.SelectMany(explorerItemModel => explorerItemModel.Descendants()).ToList();
            return searchCriteria == null ? null : explorerItemModels.FirstOrDefault(searchCriteria);
        }
    }
}
