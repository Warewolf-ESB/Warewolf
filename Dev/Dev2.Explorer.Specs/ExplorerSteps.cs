
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Security;
using Dev2.Models;
using Dev2.Network;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Dev2.Explorer.Specs
{
    [Binding]
    public class ExplorerSteps
    {
        [BeforeFeature("explorer")]
        public static void BeforeFeature()
        {
            AppSettings.LocalHost = "http://localhost:3142";
            IEnvironmentModel environmentModel = EnvironmentRepository.Instance.Source;
            environmentModel.Connect();
        }

        [Given(@"I have a server ""(.*)""")]
        public void GivenIHaveAServer(string serverName)
        {
            if(serverName != "localhost")
            {
                var environmentModel = new EnvironmentModel(Guid.NewGuid(), new ServerProxy(new Uri(string.Format("http://{0}:3142", serverName))));
                EnvironmentRepository.Instance.Save(environmentModel);
                environmentModel.Connect();
            }
        }

        [When(@"resources are loaded for ""(.*)""")]
        public void WhenResourcesAreLoadedFor(string serverName)
        {
            var environmentModel = EnvironmentRepository.Instance.FindSingle(model => model.Name == serverName);
            ServerExplorerClientProxy repository = new ServerExplorerClientProxy(environmentModel.Connection);
            var explorerItemModel = repository.Load(Guid.Empty);
            ScenarioContext.Current.Add(serverName, explorerItemModel);
        }


        [When(@"I rename the folder FolderToRename to ""(.*)""")]
        public void WhenIRenameTheFolderFolderToRenameTo(string p0)
        {
            var environmentModel = EnvironmentRepository.Instance.FindSingle(model => model.Name == "localhost");
            ServerExplorerClientProxy repository = new ServerExplorerClientProxy(environmentModel.Connection);
            var result = repository.RenameItem(new ServerExplorerItem("FolderToRename", Guid.NewGuid(), Common.Interfaces.Data.ResourceType.Folder, null, Permissions.Administrator, "FolderToRename"), p0, Guid.Empty);

            Assert.AreEqual(result.Status, ExecStatus.Success);
            var explorerItemModel = repository.Load(Guid.Empty);
            ScenarioContext.Current.Add("localhost", explorerItemModel);
            ScenarioContext.Current.Add("newName", explorerItemModel);
        }

        [Then(@"the explorer for ""(.*)"" will have")]
        public void ThenTheExplorerWillHave(string serverName, Table table)
        {
            var allItems = new List<ExplorerItemModel>();
            foreach(var tableRow in table.Rows)
            {
                var parentName = tableRow["Parent"];
                var childName = tableRow["Child"];
                var type = tableRow["Type"];

                Common.Interfaces.Data.ResourceType resourceType;
                Enum.TryParse(type, out resourceType);
                var eim = new ExplorerItemModel
                {
                    DisplayName = childName,
                    Children = new ObservableCollection<IExplorerItemModel>(),
                    ResourceType = resourceType
                };

                var explorerItemModel = allItems.FirstOrDefault(model => model.DisplayName == parentName);
                if(explorerItemModel != null)
                {
                    explorerItemModel.Children.Add(eim);
                }
                allItems.Add(eim);
            }

            var itemModel = ScenarioContext.Current.Get<IExplorerItem>(serverName);
            Assert.AreEqual(itemModel.DisplayName, Environment.MachineName);
            Assert.AreEqual(itemModel.Children[0].DisplayName, allItems[0].Children[0].DisplayName);
            Assert.AreEqual(itemModel.Children[0].Children.Count, allItems[0].Children[0].Children.Count);
            // assert that the test directory exists
            foreach(var explorerItemModel in itemModel.Children[0].Children)
            {
                Assert.IsTrue(allItems[0].Children[0].Children.Count(a => a.ResourceType == explorerItemModel.ResourceType && a.DisplayName == explorerItemModel.DisplayName) == 1);
            }

        }

        [Then(@"the explorer tree for ""(.*)"" will have")]
        public void ThenTheExplorerTreeForWillHave(string p0, Table table)
        {
            var allItems = new List<ExplorerItemModel>();
            foreach(var tableRow in table.Rows)
            {
                var parentName = tableRow["Parent"];
                var childName = tableRow["Child"];
                var type = tableRow["Type"];

                Common.Interfaces.Data.ResourceType resourceType;
                Enum.TryParse(type, out resourceType);
                var eim = new ExplorerItemModel
                {
                    DisplayName = childName,
                    Children = new ObservableCollection<IExplorerItemModel>(),
                    ResourceType = resourceType
                };

                var explorerItemModel = allItems.FirstOrDefault(model => model.DisplayName == parentName);
                if(explorerItemModel != null)
                {
                    explorerItemModel.Children.Add(eim);
                }
                allItems.Add(eim);
            }
            var folderName = ScenarioContext.Current.Get<string>("folderName");


            var itemModel = ScenarioContext.Current.Get<IExplorerItem>("localhost");
            var folder = itemModel.Children.FirstOrDefault(a => a.DisplayName == folderName);
            Assert.IsNotNull(folder);
            Assert.AreEqual(itemModel.DisplayName, Environment.MachineName);
            Assert.AreEqual(folder.DisplayName, allItems[0].Children[0].DisplayName);
            Assert.AreEqual(folder.Children.Count, allItems[0].Children[0].Children.Count);
            // assert that the test directory exists
            foreach(var explorerItemModel in folder.Children)
            {
                Assert.IsTrue(allItems[0].Children[0].Children.Count(a => a.ResourceType == explorerItemModel.ResourceType && a.DisplayName == explorerItemModel.DisplayName) == 1);
            }

            var environmentModel = EnvironmentRepository.Instance.FindSingle(model => model.Name == "localhost");
            ServerExplorerClientProxy repository = new ServerExplorerClientProxy(environmentModel.Connection);
            repository.RenameItem(new ServerExplorerItem("FolderToRename", Guid.NewGuid(), Common.Interfaces.Data.ResourceType.Folder, null, Permissions.Administrator, "FolderToRename"), p0, Guid.Empty);
        }


        [When(@"I rename the resource PrimitiveReturnTypeTest to ""(.*)""")]
        public void WhenIRenameTheResourcePrimitiveReturnTypeTestTo(string p0)
        {
            var environmentModel = EnvironmentRepository.Instance.FindSingle(model => model.Name == "localhost");
            ServerExplorerClientProxy repository = new ServerExplorerClientProxy(environmentModel.Connection);
            var item = repository.Load(Guid.Empty)
                                .Children.First(a => a.DisplayName == "ExplorerSpecsRenameItem")
                      .Children.First();


            var result = repository.RenameItem(item, "BobAndDora", Guid.Empty);
            Assert.AreEqual(result.Status, ExecStatus.Success);
            var explorerItemModel = repository.Load(Guid.Empty);
            ScenarioContext.Current.Add("localhost", explorerItemModel);
            ScenarioContext.Current.Add("newName", explorerItemModel);
        }

        [When(@"I delete the resource ""(.*)""")]
        public void WhenIDeleteTheResource(string p0)
        {
            var environmentModel = EnvironmentRepository.Instance.FindSingle(model => model.Name == "localhost");
            ServerExplorerClientProxy repository = new ServerExplorerClientProxy(environmentModel.Connection);
            var item = repository.Load(Guid.Empty).Children.First(a => a.DisplayName == "ExplorerSpecsDeleteItem")
                      .Children.First();


            var result = repository.DeleteItem(item, Guid.Empty);
            Assert.AreEqual(result.Status, ExecStatus.Success);
            var explorerItemModel = repository.Load(Guid.Empty);
            ScenarioContext.Current.Add("localhost", explorerItemModel);
            ScenarioContext.Current.Add("newName", explorerItemModel);
            ScenarioContext.Current.Add("folderName", "ExplorerSpecsDeleteItem");
        }

        [When(@"I delete the Folder FolderToDelete")]
        public void WhenIDeleteTheFolderFolderToDelete()
        {
            var environmentModel = EnvironmentRepository.Instance.FindSingle(model => model.Name == "localhost");
            ServerExplorerClientProxy repository = new ServerExplorerClientProxy(environmentModel.Connection);


            var result = repository.DeleteItem(new ServerExplorerItem("FolderToDelete", Guid.NewGuid(), Common.Interfaces.Data.ResourceType.Folder, null, Permissions.Administrator, "FolderToDelete"), Guid.Empty);
            Assert.AreEqual(result.Status, ExecStatus.Success);
            var explorerItemModel = repository.Load(Guid.Empty);
            ScenarioContext.Current.Add("localhost", explorerItemModel);
            ScenarioContext.Current.Add("newName", explorerItemModel);
        }

        [When(@"I create the Folder ""(.*)""")]
        public void WhenICreateTheFolder(string folderName)
        {
            var environmentModel = EnvironmentRepository.Instance.FindSingle(model => model.Name == "localhost");
            var repository = new ServerExplorerClientProxy(environmentModel.Connection);

            Assert.AreEqual(0, repository.Load(Guid.Empty).Children.Count(a => a.DisplayName == folderName));

            var result = repository.AddItem(new ServerExplorerItem(folderName, Guid.NewGuid(), Common.Interfaces.Data.ResourceType.Folder, null, Permissions.Administrator, ""), Guid.Empty);
            Assert.AreEqual(result.Status, ExecStatus.Success);
            var explorerItemModel = repository.Load(Guid.Empty);

            ScenarioContext.Current.Add("explorerItemModel", explorerItemModel);
            ScenarioContext.Current.Add("folderName", folderName);
        }

        [Then(@"the reloaded explorer tree for ""(.*)"" will have the created folder as a child")]
        public void ThenTheReloadedExplorerTreeForWillHaveTheCreatedFolderAsAChild(string p0, Table table)
        {
            IExplorerItem item;
            ScenarioContext.Current.TryGetValue("explorerItemModel", out item);
            string folderName;
            ScenarioContext.Current.TryGetValue("folderName", out folderName);
            var folder = item.Children.FirstOrDefault(a => a.DisplayName == folderName);
            Assert.IsNotNull(folder);
            Assert.AreEqual(folder.ResourceType, Common.Interfaces.Data.ResourceType.Folder);
            Assert.AreEqual(folder.Children.Count, 0);

        }


        [When(@"I delete the Folder ""(.*)""")]
        public void WhenIDeleteTheFolder(string folderToDelete)
        {
            var environmentModel = EnvironmentRepository.Instance.FindSingle(model => model.Name == "localhost");
            ServerExplorerClientProxy repository = new ServerExplorerClientProxy(environmentModel.Connection);


            var result = repository.DeleteItem(new ServerExplorerItem(folderToDelete, Guid.NewGuid(), Common.Interfaces.Data.ResourceType.Folder, null, Permissions.Administrator, ""), Guid.Empty);
            Assert.AreEqual(result.Status, ExecStatus.Success);
            var explorerItemModel = repository.Load(Guid.Empty);
            ScenarioContext.Current.Add("localhost", explorerItemModel);
            ScenarioContext.Current.Add("folderName", folderToDelete);
        }

        [When(@"I delete the Folder without recursive delete flag ""(.*)""")]
        public void WhenIDeleteTheFolderWithoutRecursiveDeleteFlag(string folderToDelete)
        {
            var environmentModel = EnvironmentRepository.Instance.FindSingle(model => model.Name == "localhost");
            ServerExplorerClientProxy repository = new ServerExplorerClientProxy(environmentModel.Connection);


            var result = repository.DeleteItem(new ServerExplorerItem(folderToDelete, Guid.NewGuid(), Common.Interfaces.Data.ResourceType.Folder, null, Permissions.Administrator, ""), Guid.Empty);
            Assert.AreEqual(result.Status, ExecStatus.Success);
            var explorerItemModel = repository.Load(Guid.Empty);
            Assert.IsFalse(0 == explorerItemModel.Children.Count(a => a.DisplayName == folderToDelete));
            ScenarioContext.Current.Add("localhost", explorerItemModel);
            ScenarioContext.Current.Add("newName", explorerItemModel);
        }
        [When(@"I rename the resource ""(.*)"" to ""(.*)""")]
        public void WhenIRenameTheResourceTo(string itemToRename, string p1)
        {
            var environmentModel = EnvironmentRepository.Instance.FindSingle(model => model.Name == "localhost");
            ServerExplorerClientProxy repository = new ServerExplorerClientProxy(environmentModel.Connection);
            var item = repository.Load(Guid.Empty)
                                .Children.First(a => a.DisplayName == "ExplorerSpecsRenameItem")
                      .Children.FirstOrDefault(a => a.DisplayName == itemToRename);
            item.DisplayName = p1;

            var result = repository.RenameItem(item, p1, Guid.Empty);
            Assert.AreEqual(result.Status, ExecStatus.Success);
            var explorerItemModel = repository.Load(Guid.Empty);
            ScenarioContext.Current.Add("localhost", explorerItemModel);
            ScenarioContext.Current.Add("newName", explorerItemModel);
            ScenarioContext.Current.Add("folderName", "ExplorerSpecsRenameItem");
        }

        [When(@"I rename the folder ""(.*)"" to ""(.*)""")]
        public void WhenIRenameTheFolderTo(string folderToRename, string newName)
        {
            var environmentModel = EnvironmentRepository.Instance.FindSingle(model => model.Name == "localhost");
            ServerExplorerClientProxy repository = new ServerExplorerClientProxy(environmentModel.Connection);
            var result = repository.RenameItem(new ServerExplorerItem(folderToRename, Guid.NewGuid(), Common.Interfaces.Data.ResourceType.Folder, null, Permissions.Administrator, ""), newName, Guid.Empty);

            Assert.AreEqual(ExecStatus.Success, result.Status, "Rename failed: " + result.Message);
            var explorerItemModel = repository.Load(Guid.Empty);
            ScenarioContext.Current.Add("localhost", explorerItemModel);
            ScenarioContext.Current.Add("newName", explorerItemModel);
            ScenarioContext.Current.Add("folderName", newName);
        }

        [Then(@"the explorer tree for ""(.*)"" will not have """"(.*)""""")]
        public void ThenTheExplorerTreeForWillNotHave(string p0, string p1, Table table)
        {
            var folderName = ScenarioContext.Current.Get<string>("folderName");
            var itemModel = ScenarioContext.Current.Get<IExplorerItem>("localhost");
            Assert.AreEqual(0, itemModel.Children.Count(a => a.DisplayName == folderName));
        }

    }
}
