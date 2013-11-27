using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Test.XML;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Execution;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using Moq;

namespace Dev2.Tests.Runtime.ESB
{
    // BUG 9710 - 2013.06.20 - TWR - Created
    [TestClass][ExcludeFromCodeCoverage]
    public class DatabaseServiceContainerTests
    {
        static string _testDir;

        #region ClassInitialize

        [ClassInitialize]
        public static void MyClassInitialize(TestContext context)
        {
            _testDir = context.DeploymentDirectory;
        }

        #endregion

        #region Execute
        [TestMethod]
        public void DatabaseServiceContainer_UnitTest_ExecuteWhereHasDatabaseServiceExecution_Guid()
        {
            //------------Setup for test--------------------------
            var mockServiceExecution = new Mock<IServiceExecution>();
            ErrorResultTO errors;
            Guid expected = Guid.NewGuid();
            mockServiceExecution.Setup(execution => execution.Execute(out errors)).Returns(expected);
            DatabaseServiceContainer databaseServiceContainer = new DatabaseServiceContainer(mockServiceExecution.Object);
            //------------Execute Test---------------------------
            Guid actual = databaseServiceContainer.Execute(out errors);
            //------------Assert Results-------------------------
            Assert.AreEqual(expected, actual, "Execute should return the Guid from the service execution");
        }

        [TestMethod]
        [Ignore] // Use Times Execute Instead
        public void DatabaseServiceContainerExecuteWithValidServiceHavingInputsExpectedExecutesService()
        {
            var container = CreateDatabaseServiceContainer(true);

            ErrorResultTO errors;
            var dataListID = container.Execute(out errors);

            var compiler = DataListFactory.CreateDataListCompiler();
            var result = compiler.ConvertFrom(dataListID, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, container.GetXmlDataFromSqlServiceActionHitCount);

            var resultXml = XElement.Parse(result);

            foreach(var actualNode in resultXml.Elements())
            {
                var actualName = actualNode.Name.LocalName;
                if(!actualName.StartsWith("Dev2System"))
                {
                    switch(actualName)
                    {
                        case "Prefix":
                            Assert.AreEqual("an", actualNode.Value);
                            break;
                        case "Countries":
                            Assert.IsTrue(actualNode.Value == "4Andorra" || actualNode.Value == "5Angola");
                            break;
                        default:
                            Assert.Fail("Invalid result");
                            break;
                    }
                }
            }
        }

        [TestMethod]
        [Ignore] // Use Validate Number of Times Executed Instead
        public void DatabaseServiceContainerExecuteWithValidServiceHavingNoInputsExpectedExecutesService()
        {
            var container = CreateDatabaseServiceContainer(false);

            ErrorResultTO errors;
            var dataListID = container.Execute(out errors);

            var compiler = DataListFactory.CreateDataListCompiler();
            var result = compiler.ConvertFrom(dataListID, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, container.GetXmlDataFromSqlServiceActionHitCount);

            var resultXml = XElement.Parse(result);

            foreach(var actualNode in resultXml.Elements())
            {
                var actualName = actualNode.Name.LocalName;
                if(!actualName.StartsWith("Dev2System"))
                {
                    switch(actualName)
                    {
                        case "Countries":
                            Assert.IsTrue(actualNode.Value == "4Andorra" || actualNode.Value == "5Angola");
                            break;
                        default:
                            Assert.Fail("Invalid result");
                            break;
                    }
                }
            }
        }

        [TestMethod]
        [Ignore]  // probably should be an integration test?
        public void DatabaseServiceContainerExecuteWithValidServiceHavingInputsExpectedExecutesSqlService()
        {
            var container = CreateDatabaseServiceContainer(true, true);

            ErrorResultTO errors;
            var dataListID = container.Execute(out errors);

            var compiler = DataListFactory.CreateDataListCompiler();
            var result = compiler.ConvertFrom(dataListID, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, container.GetXmlDataFromSqlServiceActionHitCount);

            var resultXml = XElement.Parse(result);

            foreach(var actualNode in resultXml.Elements())
            {
                var actualName = actualNode.Name.LocalName;
                if(!actualName.StartsWith("Dev2System"))
                {
                    switch(actualName)
                    {
                        case "Prefix":
                            Assert.AreEqual("an", actualNode.Value);
                            break;
                        case "Countries":
                            Assert.IsTrue(actualNode.Value == "4Andorra" || actualNode.Value == "5Angola");
                            break;
                        default:
                            Assert.Fail("Invalid result");
                            break;
                    }
                }
            }
        }

        [TestMethod]
        [Ignore]  // need an test account 
        public void DatabaseServiceContainerExecuteWithValidServiceHavingInputsAndDomainUserExpectedExecutesSqlService()
        {
            var container = CreateDatabaseServiceContainer(true, true, "DEV2\\??", "pass123");

            ErrorResultTO errors;
            var dataListID = container.Execute(out errors);

            var compiler = DataListFactory.CreateDataListCompiler();
            var result = compiler.ConvertFrom(dataListID, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, container.GetXmlDataFromSqlServiceActionHitCount);

            var resultXml = XElement.Parse(result);

            foreach(var actualNode in resultXml.Elements())
            {
                var actualName = actualNode.Name.LocalName;
                if(!actualName.StartsWith("Dev2System"))
                {
                    switch(actualName)
                    {
                        case "Prefix":
                            Assert.AreEqual("an", actualNode.Value);
                            break;
                        case "Countries":
                            Assert.IsTrue(actualNode.Value == "4Andorra" || actualNode.Value == "5Angola");
                            break;
                        default:
                            Assert.Fail("Invalid result");
                            break;
                    }
                }
            }
        }

        #endregion

        #region CreateDatabaseServiceContainer

        static DatabaseServiceContainerMock CreateDatabaseServiceContainer(bool withInputs, bool shouldExecuteSql = false, string userID = "", string password = "")
        {
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            Guid dataListID;
            var responseXml = shouldExecuteSql ? string.Empty
                : "<NewDataSet><Table><CountryID>4</CountryID><Description>Andorra</Description></Table><Table><CountryID>5</CountryID><Description>Angola</Description></Table></NewDataSet>";

            if(withInputs)
            {
                dataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML),
                    "<DataList><Prefix>an</Prefix></DataList>",
                    "<ADL><Prefix></Prefix><Countries><CountryID></CountryID><CountryName></CountryName></Countries></ADL>", out errors);
            }
            else
            {
                dataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML),
                    "<DataList></DataList>",
                    "<ADL><Countries><CountryID></CountryID><CountryName></CountryName></Countries></ADL>", out errors);
            }

            var dataObj = new Mock<IDSFDataObject>();
            dataObj.Setup(d => d.DataListID).Returns(dataListID);

            var workspace = new Mock<IWorkspace>();
            var esbChannel = new Mock<IEsbChannel>();

            var sa = CreateServiceAction(withInputs, userID, password);
            var container = new DatabaseServiceContainerMock(sa, dataObj.Object, workspace.Object, esbChannel.Object)
            {
                DatabaseRespsonseXml = responseXml
            };
            return container;
        }

        #endregion

        #region CreateServiceAction

        static ServiceAction CreateServiceAction(bool withInputs, string userID = "", string password = "")
        {
            var source = new DbSource(XmlResource.Fetch("CitiesDatabase"))
            {
                ServerType = enSourceType.SqlDatabase
            };
            if(!string.IsNullOrEmpty(userID))
            {
                source.AuthenticationType = AuthenticationType.User;
                source.UserID = userID;
                source.Password = password;
            }
            var service = new DbService(XmlResource.Fetch("CatalogServiceCitiesDatabase"))
            {
                Source = source
            };

            if(!withInputs)
            {
                service.Method.Name = "XXXX";
                service.Method.Parameters.Clear();
            }

            var serviceXml = service.ToXml();
            var graph = new DynamicObjectHelper().GenerateObjectGraphFromString(serviceXml.ToString());

            var ds = (DynamicService)graph[0];
            var sa = ds.Actions[0];
            sa.Source = new Source
            {
                ResourceDefinition = service.Source.ToXml().ToString(),
                ConnectionString = ((DbSource)service.Source).ConnectionString
            };
            return sa;
        }

        #endregion

    }
}
