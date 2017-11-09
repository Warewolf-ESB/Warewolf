using Caliburn.Micro;
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
using Dev2.Studio.ViewModels.DataList;
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
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            var aggr = new Mock<IEventAggregator>();
            DataListSingleton.SetDataList(new DataListViewModel(aggr.Object));
        }

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
                    new ServiceInput("ProductId","[[ProductId]]"){ActionName = "dbo." + cleanProcName}
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
            //actionRegion.SelectedAction = diffAction;//trigger action changes
            var alterTableResults = SqlHelper.RunSqlCommand(alterProcedure);
            actionRegion.RefreshActionsCommand.Execute(null);
            var underTest = actionRegion.Actions.Single(p => p.Name.EndsWith(cleanProcName));
            Assert.AreEqual(3, databaseInputRegion.Inputs.Count);
            Assert.AreEqual("ProductId", underTest.Inputs.ToList()[0].Name);
            Assert.AreEqual("ProductId1", underTest.Inputs.ToList()[1].Name);
            Assert.AreEqual("ProductId2", underTest.Inputs.ToList()[2].Name);

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
        public void Oracle_Add_A_New_InputOnSqlProcedure_Expect_New_IS_InputAdded()
        {
            var newName = Guid.NewGuid().ToString();
            var cleanProcName = newName.Replace("-", "").Replace(" ", "").Substring(0,8);
            var dropProcedure = "drop procedure \"HR\"." + cleanProcName + "";
            var createProcedure = "create or replace PROCEDURE HR." + cleanProcName + "(P_DEPTNO_1 IN employees.department_id%TYPE,p_recordset OUT SYS_REFCURSOR) AS BEGIN OPEN p_recordset FOR SELECT first_name,last_name,employee_id,email FROM employees WHERE department_id = P_DEPTNO_1   ORDER BY last_name, first_name ASC;END " + cleanProcName + ";";
            var result = SqlHelper.RunOracleSqlCommand(createProcedure);
            Assert.AreEqual(-1, result);
            List<IServiceInput> inputs = new List<IServiceInput>()
                {
                    new ServiceInput("P_DEPTNO_1","[[P_DEPTNO_1]]"){ActionName = "HR." + cleanProcName}
                };

            var sqlActivity = new DsfOracleDatabaseActivity()
            {
                Inputs = inputs,
                ActionName = "HR." + cleanProcName,
                ProcedureName = "HR." + cleanProcName,
                SourceId = new Guid("b1c12282-1712-419c-9929-5dfe42c90210")
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
            var source = new DatabaseSourceRegion(dbServiceModel, modelItem, Common.Interfaces.Core.DynamicServices.enSourceType.Oracle);
            var selectedSource = source.Sources.Single(a => a.Id == sqlActivity.SourceId);
            source.SelectedSource = selectedSource;
            var actionRegion = new DbActionRegion(dbServiceModel, modelItem, source, new SynchronousAsyncWorker());


            var diffAction = actionRegion.Actions.First(p => p.Name != sqlActivity.ProcedureName);
            IDatabaseInputRegion databaseInputRegion = new DatabaseInputRegion(modelItem, actionRegion);
            Assert.AreEqual(1, databaseInputRegion.Inputs.Count);
            Assert.AreEqual("P_DEPTNO_1", databaseInputRegion.Inputs.Single().Name);
            Assert.AreEqual("[[P_DEPTNO_1]]", databaseInputRegion.Inputs.Single().Value);
            //add testing here
            var alterProcedure = "create or replace PROCEDURE hr." + cleanProcName + "(P_DEPTNO_1 IN employees.department_id%TYPE,P_DEPTNO_2 IN employees.department_id%TYPE,P_DEPTNO_3 IN employees.department_id%TYPE,p_recordset OUT SYS_REFCURSOR) AS BEGIN OPEN p_recordset FOR SELECT first_name,last_name,employee_id,email FROM employees WHERE department_id = P_DEPTNO_1   ORDER BY last_name, first_name ASC;END " + cleanProcName + ";";
            //actionRegion.SelectedAction = diffAction;//trigger action changes
            var alterTableResults = SqlHelper.RunOracleSqlCommand(alterProcedure);
            actionRegion.RefreshActionsCommand.Execute(null);
            var underTest = actionRegion.Actions.Single(p => p.Name.EndsWith(cleanProcName));
            Assert.AreEqual(3, databaseInputRegion.Inputs.Count);
            Assert.AreEqual("P_DEPTNO_1", underTest.Inputs.ToList()[0].Name);
            Assert.AreEqual("P_DEPTNO_2", underTest.Inputs.ToList()[1].Name);
            Assert.AreEqual("P_DEPTNO_3", underTest.Inputs.ToList()[2].Name);

            Assert.AreEqual("P_DEPTNO_1", databaseInputRegion.Inputs.ToList()[0].Name);
            Assert.AreEqual("[[P_DEPTNO_1]]", databaseInputRegion.Inputs.ToList()[0].Value);

            Assert.AreEqual("P_DEPTNO_2", databaseInputRegion.Inputs.ToList()[1].Name);
            Assert.AreEqual("[[P_DEPTNO_2]]", databaseInputRegion.Inputs.ToList()[1].Value);

            Assert.AreEqual("P_DEPTNO_3", databaseInputRegion.Inputs.ToList()[2].Name);
            Assert.AreEqual("[[P_DEPTNO_3]]", databaseInputRegion.Inputs.ToList()[2].Value);
            Assert.AreEqual(-1, alterTableResults);
            var resultForDrop = SqlHelper.RunOracleSqlCommand(dropProcedure);
            Assert.AreEqual(-1, resultForDrop);
        }


    }
}
