using Dev2.Activities;
using Dev2.Activities.Designers2.Core.ActionRegion;
using Dev2.Activities.Designers2.Core.InputRegion;
using Dev2.Activities.Designers2.Core.Source;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase.Database;
using Dev2.Controller;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Warewolf.Core;
using Warewolf.Studio.ViewModels;

namespace Dev2.Integration.Tests.Database_Tools_Refresh
{
    [TestClass]
    public class DatabaseServiceInputChangeTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SqlDb_Add_A_New_InputOnSqlProcedure_Expect_New_IS_InputAdded()
        {
            var newName = Guid.NewGuid().ToString();
            var cleanProcName = newName.Replace("-", "").Replace(" ", "");
            var dropProcedure = "USE [Dev2TestingDB]  DROP PROCEDURE [dbo].[" + cleanProcName + "]";
            var createProcedure = "CREATE procedure [dbo].[" + cleanProcName + "](@ProductId int) as Begin select * from Country select * from City end";
            var result = SqlHelper.RunSqlCommand(createProcedure);
            Assert.AreEqual(-1, result);
            List<IServiceInput> inputs = new List<IServiceInput>()
                {
                    new ServiceInput("ProductId","[[ProductId]]")
                };
            var sqlActivity = new DsfSqlServerDatabaseActivity()
            {
                Inputs = inputs,
                ActionName = "dbo." + cleanProcName,
                ProcedureName = "dbo." + cleanProcName,
                SourceId = new Guid("b9184f70-64ea-4dc5-b23b-02fcd5f91082")
            };
            var modelItem = ModelItemUtils.CreateModelItem(sqlActivity);
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();
            var environmentConnection = environmentModel.Connection;
            var controllerFactory = new CommunicationControllerFactory();
            var _proxyLayer = new StudioServerProxy(controllerFactory, environmentConnection);
            var mock = new Mock<IShellViewModel>();
            ManageDbServiceModel dbServiceModel = new ManageDbServiceModel(new StudioResourceUpdateManager(controllerFactory, environmentConnection)
                                                                                    , _proxyLayer.QueryManagerProxy
                                                                                    , mock.Object
                                                                                    , environmentModel);
            var source = new DatabaseSourceRegion(dbServiceModel, modelItem, Common.Interfaces.Core.DynamicServices.enSourceType.SqlDatabase);
            var selectedSource = source.Sources.Single(a => a.Id == sqlActivity.SourceId);
            source.SelectedSource = selectedSource;
            var actionRegion = new DbActionRegion(dbServiceModel, modelItem, source, new SynchronousAsyncWorker());


            var diffAction = actionRegion.Actions.First(p => p.Name != sqlActivity.ProcedureName);
            IDatabaseInputRegion databaseInputRegion = new DatabaseInputRegion(modelItem, actionRegion);
            Assert.AreEqual(1, databaseInputRegion.Inputs.Count);
            Assert.AreEqual("ProductId", databaseInputRegion.Inputs.Single().Name);
            Assert.AreEqual("[[ProductId]]", databaseInputRegion.Inputs.Single().Value);
            //add testing here
            var alterProcedure = "ALTER procedure [dbo].[" + cleanProcName + "](@ProductId int,@ProductId1 int,@ProductId2 int) as Begin select * from Country select * from City end";
            actionRegion.SelectedAction = diffAction;//trigger action changes
            var alterTableResults = SqlHelper.RunSqlCommand(alterProcedure);
            actionRegion.RefreshActionsCommand.Execute(null);
            var underTest = actionRegion.Actions.Single(p => p.Name.EndsWith(cleanProcName));
            actionRegion.SelectedAction = underTest;//trigger action changes
            Assert.AreEqual(3, databaseInputRegion.Inputs.Count);
            Assert.AreEqual("ProductId", databaseInputRegion.Inputs.ToList()[0].Name);
            Assert.AreEqual("[[ProductId]]", databaseInputRegion.Inputs.ToList()[0].Value);

            Assert.AreEqual("ProductId1", databaseInputRegion.Inputs.ToList()[1].Name);
            Assert.AreEqual("[[ProductId1]]", databaseInputRegion.Inputs.ToList()[1].Value);

            Assert.AreEqual("ProductId2", databaseInputRegion.Inputs.ToList()[2].Name);
            Assert.AreEqual("[[ProductId2]]", databaseInputRegion.Inputs.ToList()[2].Value);
            Assert.AreEqual(-1, alterTableResults);
            var resultForDrop = SqlHelper.RunSqlCommand(dropProcedure);
            Assert.AreEqual(-1, resultForDrop);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SqlDb_Add_A_New_InputOnSqlProcedure_Expect_New_IS_InputAdded_ExistingInputsAreNotChanged()
        {
            var newName = Guid.NewGuid().ToString();
            var cleanProcName = newName.Replace("-", "").Replace(" ", "");
            var dropProcedure = "USE [Dev2TestingDB]  DROP PROCEDURE [dbo].[" + cleanProcName + "]";
            var createProcedure = "CREATE procedure [dbo].[" + cleanProcName + "](@ProductId int) as Begin select * from Country select * from City end";
            var result = SqlHelper.RunSqlCommand(createProcedure);
            Assert.AreEqual(-1, result);
            List<IServiceInput> inputs = new List<IServiceInput>()
                {
                    new ServiceInput("ProductId","[[ProductId]]")
                };
            var sqlActivity = new DsfSqlServerDatabaseActivity()
            {
                Inputs = inputs,
                ActionName = "dbo." + cleanProcName,
                ProcedureName = "dbo." + cleanProcName,
                SourceId = new Guid("b9184f70-64ea-4dc5-b23b-02fcd5f91082")
            };
            var modelItem = ModelItemUtils.CreateModelItem(sqlActivity);
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();
            var environmentConnection = environmentModel.Connection;
            var controllerFactory = new CommunicationControllerFactory();
            var _proxyLayer = new StudioServerProxy(controllerFactory, environmentConnection);
            var mock = new Mock<IShellViewModel>();
            ManageDbServiceModel dbServiceModel = new ManageDbServiceModel(new StudioResourceUpdateManager(controllerFactory, environmentConnection)
                                                                                    , _proxyLayer.QueryManagerProxy
                                                                                    , mock.Object
                                                                                    , environmentModel);
            var source = new DatabaseSourceRegion(dbServiceModel, modelItem, Common.Interfaces.Core.DynamicServices.enSourceType.SqlDatabase);
            var selectedSource = source.Sources.Single(a => a.Id == sqlActivity.SourceId);
            source.SelectedSource = selectedSource;
            var actionRegion = new DbActionRegion(dbServiceModel, modelItem, source, new SynchronousAsyncWorker());


            var diffAction = actionRegion.Actions.First(p => p.Name != sqlActivity.ProcedureName);
            IDatabaseInputRegion databaseInputRegion = new DatabaseInputRegion(modelItem, actionRegion);
            Assert.AreEqual(1, databaseInputRegion.Inputs.Count);
            Assert.AreEqual("ProductId", databaseInputRegion.Inputs.Single().Name);
            Assert.AreEqual("[[ProductId]]", databaseInputRegion.Inputs.Single().Value);
            //add testing here
            var alterProcedure = "ALTER procedure [dbo].[" + cleanProcName + "](@ProductId int,@ProductId1 int,@ProductId2 int) as Begin select * from Country select * from City end";
            actionRegion.SelectedAction = diffAction;//trigger action changes
            var alterTableResults = SqlHelper.RunSqlCommand(alterProcedure);
            actionRegion.RefreshActionsCommand.Execute(null);
            var underTest = actionRegion.Actions.Single(p => p.Name.EndsWith(cleanProcName));
            actionRegion.SelectedAction = underTest;//trigger action changes
            Assert.AreEqual(3, databaseInputRegion.Inputs.Count);
            Assert.AreEqual("ProductId", databaseInputRegion.Inputs.ToList()[0].Name);
            Assert.AreEqual("[[ProductId]]", databaseInputRegion.Inputs.ToList()[0].Value);

            Assert.AreEqual("ProductId1", databaseInputRegion.Inputs.ToList()[1].Name);
            Assert.AreEqual("[[ProductId1]]", databaseInputRegion.Inputs.ToList()[1].Value);

            Assert.AreEqual("ProductId2", databaseInputRegion.Inputs.ToList()[2].Name);
            Assert.AreEqual("[[ProductId2]]", databaseInputRegion.Inputs.ToList()[2].Value);
            Assert.AreEqual(-1, alterTableResults);
            var resultForDrop = SqlHelper.RunSqlCommand(dropProcedure);
            Assert.AreEqual(-1, resultForDrop);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OracleDb_Add_A_New_InputOnSqlProcedure_Expect_New_IS_InputAdded()
        {
            var newName = Guid.NewGuid().ToString();
            var cleanProcName = newName.Replace("-", "").Replace(" ", "");
            var dropProcedure = "USE [Dev2TestingDB]  DROP PROCEDURE [dbo].[" + cleanProcName + "]";
            var createProcedure = "CREATE procedure [dbo].[" + cleanProcName + "](@ProductId int) as Begin select * from Country select * from City end";
            var result = SqlHelper.RunSqlCommand(createProcedure);
            Assert.AreEqual(-1, result);
            List<IServiceInput> inputs = new List<IServiceInput>()
                {
                    new ServiceInput("P_DEPTNO_1","[[ProductId]]")
                };
            var oracleActivity = new DsfOracleDatabaseActivity()
            {
                Inputs = inputs,
                ActionName = "dbo." + cleanProcName,
                ProcedureName = "dbo." + cleanProcName,
                SourceId = new Guid("b1c12282-1712-419c-9929-5dfe42c90210")
            };
            var modelItem = ModelItemUtils.CreateModelItem(oracleActivity);
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();
            var environmentConnection = environmentModel.Connection;
            var controllerFactory = new CommunicationControllerFactory();
            var _proxyLayer = new StudioServerProxy(controllerFactory, environmentConnection);
            var mock = new Mock<IShellViewModel>();
            ManageDbServiceModel dbServiceModel = new ManageDbServiceModel(new StudioResourceUpdateManager(controllerFactory, environmentConnection)
                                                                                    , _proxyLayer.QueryManagerProxy
                                                                                    , mock.Object
                                                                                    , environmentModel);
            var source = new DatabaseSourceRegion(dbServiceModel, modelItem, Common.Interfaces.Core.DynamicServices.enSourceType.SqlDatabase);
            var selectedSource = source.Sources.Single(a => a.Id == oracleActivity.SourceId);
            source.SelectedSource = selectedSource;
            var actionRegion = new DbActionRegion(dbServiceModel, modelItem, source, new SynchronousAsyncWorker());


            var diffAction = actionRegion.Actions.First(p => p.Name != oracleActivity.ProcedureName);
            IDatabaseInputRegion databaseInputRegion = new DatabaseInputRegion(modelItem, actionRegion);
            Assert.AreEqual(1, databaseInputRegion.Inputs.Count);
            Assert.AreEqual("ProductId", databaseInputRegion.Inputs.Single().Name);
            Assert.AreEqual("[[ProductId]]", databaseInputRegion.Inputs.Single().Value);
            //add testing here
            var alterProcedure = "ALTER procedure [dbo].[" + cleanProcName + "](@ProductId int,@ProductId1 int,@ProductId2 int) as Begin select * from Country select * from City end";
            actionRegion.SelectedAction = diffAction;//trigger action changes
            var alterTableResults = SqlHelper.RunSqlCommand(alterProcedure);
            actionRegion.RefreshActionsCommand.Execute(null);
            var underTest = actionRegion.Actions.Single(p => p.Name.EndsWith(cleanProcName));
            actionRegion.SelectedAction = underTest;//trigger action changes
            Assert.AreEqual(3, databaseInputRegion.Inputs.Count);
            Assert.AreEqual("ProductId", databaseInputRegion.Inputs.ToList()[0].Name);
            Assert.AreEqual("[[ProductId]]", databaseInputRegion.Inputs.ToList()[0].Value);

            Assert.AreEqual("ProductId1", databaseInputRegion.Inputs.ToList()[1].Name);
            Assert.AreEqual("[[ProductId1]]", databaseInputRegion.Inputs.ToList()[1].Value);

            Assert.AreEqual("ProductId2", databaseInputRegion.Inputs.ToList()[2].Name);
            Assert.AreEqual("[[ProductId2]]", databaseInputRegion.Inputs.ToList()[2].Value);
            Assert.AreEqual(-1, alterTableResults);
            var resultForDrop = SqlHelper.RunSqlCommand(dropProcedure);
            Assert.AreEqual(-1, resultForDrop);
        }
    }
}
