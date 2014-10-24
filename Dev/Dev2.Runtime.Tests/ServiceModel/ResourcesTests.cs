
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Runtime.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.ServiceModel
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ResourcesTests
    {

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            Directory.SetCurrentDirectory(testContext.TestDir);
        }

        [TestMethod]
        public void Paths_Expected_JSONSources()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var servicesPath = Path.Combine(workspacePath, "Services");
            var sourcesPath = Path.Combine(workspacePath, "Sources");
            var pluginsPath = Path.Combine(workspacePath, "Plugins");
            try
            {
                Directory.CreateDirectory(servicesPath);
                Directory.CreateDirectory(sourcesPath);
                Directory.CreateDirectory(pluginsPath);

                var xml = XmlResource.Fetch("Calculate_RecordSet_Subtract");
                xml.Save(Path.Combine(servicesPath, "Calculate_RecordSet_Subtract.xml"));

                xml = XmlResource.Fetch("HostSecurityProviderServerSigned");
                xml.Save(Path.Combine(sourcesPath, "HostSecurityProviderServerSigned.xml"));

                var testResources = new Dev2.Runtime.ServiceModel.Resources();
                var actual = testResources.Paths("", workspaceID, Guid.Empty);
                Assert.AreEqual("[\"Integration Test Services\\\\Calculate_RecordSet_Subtract\"]", actual);
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    DirectoryHelper.CleanUp(workspacePath);
                }
            }
        }

        #region private test methods

        #endregion

        #region Sources

        [TestMethod]
        public void SourcesWithNullArgsExpectedReturnsEmptyList()
        {
            var resources = new Dev2.Runtime.ServiceModel.Resources();
            var result = resources.Sources(null, Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void SourcesWithInvalidArgsExpectedReturnsEmptyList()
        {
            var resources = new Dev2.Runtime.ServiceModel.Resources();
            var result = resources.Sources("xxxx", Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void SourcesWithValidArgsExpectedReturnsList()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            try
            {
                const int Modulo = 2;
                const int ExpectedCount = 6;
                for(var i = 0; i < ExpectedCount; i++)
                {
                    var resource = new Resource
                    {
                        ResourceID = Guid.NewGuid(),
                        ResourceName = string.Format("My Name {0}", i),
                        ResourcePath = string.Format("My Path {0}", i),
                        ResourceType = (i % Modulo == 0) ? ResourceType.DbSource : ResourceType.Unknown
                    };
                    ResourceCatalog.Instance.SaveResource(workspaceID, resource);
                }
                var resources = new Dev2.Runtime.ServiceModel.Resources();
                var result = resources.Sources("{\"resourceType\":\"" + ResourceType.DbSource + "\"}", workspaceID, Guid.Empty);

                Assert.AreEqual(ExpectedCount / Modulo, result.Count);
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    DirectoryHelper.CleanUp(workspacePath);
                }
            }
        }
        #endregion

        #region Services

        [TestMethod]
        public void ServicesWithValidArgsExpectedReturnsList()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            try
            {
                const int Modulo = 2;
                const int ExpectedCount = 6;
                for(var i = 0; i < ExpectedCount; i++)
                {
                    var resource = new Resource
                    {
                        ResourceID = Guid.NewGuid(),
                        ResourcePath = (i % Modulo == 0) ? ResourceType.WorkflowService + "\\" + string.Format("My Name {0}", i) : string.Format("My Name {0}", i),
                        ResourceName = string.Format("My Name {0}", i),
                        ResourceType = (i % Modulo == 0) ? ResourceType.WorkflowService : ResourceType.Unknown
                    };
                    ResourceCatalog.Instance.SaveResource(workspaceID, resource);
                }
                var resources = new Dev2.Runtime.ServiceModel.Resources();
                var result = resources.Services(ResourceType.WorkflowService.ToString(), workspaceID, Guid.Empty);

                Assert.AreEqual(ExpectedCount / Modulo, result.Count);
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    DirectoryHelper.CleanUp(workspacePath);
                }
            }
        }

        [TestMethod]
        public void ServicesWithNullArgsExpectedReturnsEmptyList()
        {
            var resources = new Dev2.Runtime.ServiceModel.Resources();
            var result = resources.Services(null, Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void ServicesWithInvalidArgsExpectedReturnsEmptyList()
        {
            var resources = new Dev2.Runtime.ServiceModel.Resources();
            var result = resources.Services("xxxx", Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }
        #endregion

        #region DataListInputVariables

        [TestMethod]
        public void DataListInputVariablesWhereNullArgsExpectEmptyString()
        {
            //------------Setup for test--------------------------
            var resources = new Dev2.Runtime.ServiceModel.Resources();
            //------------Execute Test---------------------------
            var result = resources.DataListInputVariables(null, Guid.Empty, Guid.Empty);
            //------------Assert Results-------------------------
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void DataListInputVariablesWhereInvalidArgsExpectEmptyString()
        {
            //------------Setup for test--------------------------
            var resources = new Dev2.Runtime.ServiceModel.Resources();
            //------------Execute Test---------------------------
            var result = resources.DataListInputVariables("xxxx", Guid.Empty, Guid.Empty);
            //------------Assert Results-------------------------
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void DataListInputWhereValidArgsDataListHasInputsScalarsAndRecSetExpectCorrectString()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            try
            {
                string completePath = Path.Combine(workspacePath, "Mo");
                if(Directory.Exists(completePath))
                {
                    Directory.Delete(completePath);
                }
                var xml = XmlResource.Fetch("TestForEachOutput");
                var resourcePath = xml.Element("Category");
                Directory.CreateDirectory(completePath);
                Assert.IsNotNull(resourcePath);
                xml.Save(Path.Combine(workspacePath, resourcePath.Value + ".xml"));
                var resources = new Dev2.Runtime.ServiceModel.Resources();
                //---------------Assert Preconditions------------------------------
                Assert.IsNotNull(resourcePath);
                var resource = ResourceCatalog.Instance.GetResource(workspaceID, resourcePath.Value);
                Assert.IsNotNull(resource);
                //------------Execute Test---------------------------
                var dataListInputVariables = resources.DataListInputVariables(resource.ResourceID.ToString(), workspaceID, Guid.Empty);
                //------------Assert Results-------------------------
                StringAssert.Contains(dataListInputVariables, "inputScalar");
                StringAssert.Contains(dataListInputVariables, "bothScalar");
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    DirectoryHelper.CleanUp(workspacePath);
                }
            }
        }

        [TestMethod]
        public void DataListInputWhereValidArgsDataListHasNoInputsExpectEmptyString()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();

            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            try
            {
                string completePath = Path.Combine(workspacePath, "Bugs");
                if(Directory.Exists(completePath))
                {
                    Directory.Delete(completePath);
                }
                var xml = XmlResource.Fetch("Bug6619");
                var resourcePath = xml.Element("Category");
                Directory.CreateDirectory(completePath);
                Assert.IsNotNull(resourcePath);
                xml.Save(Path.Combine(workspacePath, resourcePath.Value + ".xml"));
                var resources = new Dev2.Runtime.ServiceModel.Resources();
                //-----------------Assert Preconditions-----------------------------
                Assert.IsNotNull(resourcePath);
                var resource = ResourceCatalog.Instance.GetResource(workspaceID, resourcePath.Value);
                Assert.IsNotNull(resource);
                //------------Execute Test---------------------------
                var dataListInputVariables = resources.DataListInputVariables(resource.ResourceID.ToString(), workspaceID, Guid.Empty);
                //------------Assert Results-------------------------
                Assert.IsTrue(string.IsNullOrEmpty(dataListInputVariables));
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    DirectoryHelper.CleanUp(workspacePath);
                }
            }
        }
        #endregion

        #region Paths And Names

        [TestMethod]
        [TestCategory("Resources_PathsAndNames")]
        [Description("Correct list of folder names returned for workflow type service")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void Resources_UnitTest_PluginPathsAndNames_AllServicePathsExeptWorkflows()
        // ReSharper restore InconsistentNaming
        {
            //Isolate PathsAndNames for workflows as a functional unit
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            try
            {
                const int Modulo = 3;
                const int TotalResourceCount = 9;
                for(var i = 0; i < TotalResourceCount; i++)
                {
                    var resource = new Resource
                    {
                        ResourceID = Guid.NewGuid(),
                        ResourceName = string.Format("My Name {0}", i),
                        ResourcePath = string.Format("My Path {0}\\{1}", i, string.Format("My Name {0}", i))
                    };

                    switch(i % Modulo)
                    {
                        case 0:
                            resource.ResourcePath = ResourceType.WorkflowService + "\\" + resource.ResourceName;
                            resource.ResourceType = ResourceType.WorkflowService;
                            break;
                        case 1:
                            resource.ResourcePath = ResourceType.DbService + "\\" + resource.ResourceName;
                            resource.ResourceType = ResourceType.DbService;
                            break;
                        case 2:
                            resource.ResourcePath = ResourceType.PluginService + "\\" + resource.ResourceName;
                            resource.ResourceType = ResourceType.PluginService;
                            break;
                    }
                    ResourceCatalog.Instance.SaveResource(workspaceID, resource);
                }
                var resources = new Dev2.Runtime.ServiceModel.Resources();
                const string ExpectedJson = "{\"Names\":[\"My Name 1\",\"My Name 4\",\"My Name 7\"],\"Paths\":[\"DbService\"]}";

                //Run PathsAndNames
                var actualJson = resources.PathsAndNames("DbService", workspaceID, Guid.Empty);

                //Assert CorrectListReturned
                Assert.AreEqual(ExpectedJson, actualJson, "Incorrect list of names and paths for workflow services");
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    DirectoryHelper.CleanUp(workspacePath);
                }
            }
        }


        #endregion
    }
}
