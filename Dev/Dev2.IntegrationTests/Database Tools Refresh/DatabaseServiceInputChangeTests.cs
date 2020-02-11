using Caliburn.Micro;
using Dev2.Activities;
using Dev2.Activities.Designers2.Core.ActionRegion;
using Dev2.Activities.Designers2.Core.InputRegion;
using Dev2.Activities.Designers2.Core.Source;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
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
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using Warewolf.Core;
using Warewolf.Test.Agent;
using Warewolf.Studio.ViewModels;
using Warewolf.UnitTestAttributes;

namespace Dev2.Integration.Tests.Database_Tools_Refresh
{
    [TestClass]
    public class DatabaseServiceInputChangeTests
    {
        public static Depends _containerOps;
        private ManageDbServiceModel _dbServiceModel;
        private DsfSqlServerDatabaseActivity _sqlActivity;
        private DbActionRegion _dbActionRegion;
        private ModelItem _modelItem;
        private DatabaseSourceRegion _source;
        private IDbSource _selectedSource;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            var aggr = new Mock<IEventAggregator>();
            DataListSingleton.SetDataList(new DataListViewModel(aggr.Object));
            _containerOps = new Depends(Depends.ContainerType.MSSQL);
        }

        [ClassCleanup]
        public static void CleanupContainer() => _containerOps?.Dispose();

        private void CreateDbServiceModel()
        {
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.ConnectAsync().Wait(60000);
            var environmentConnection = environmentModel.Connection;
            var controllerFactory = new CommunicationControllerFactory();

            var _proxyLayer = new StudioServerProxy(controllerFactory, environmentConnection);
            var mock = new Mock<IShellViewModel>();

            _dbServiceModel = new ManageDbServiceModel(new StudioResourceUpdateManager(controllerFactory, environmentConnection)
                                                                                                , _proxyLayer.QueryManagerProxy
                                                                                                , mock.Object
                                                                                                , environmentModel);
        }

        private void CreateSqlServerActivity(string procName)
        {
            var inputs = new List<IServiceInput>
            {
                new ServiceInput("ProductId","[[ProductId]]"){ActionName = "dbo." + procName}
            };

            _sqlActivity = new DsfSqlServerDatabaseActivity
            {
                Inputs = inputs,
                ActionName = "dbo." + procName,
                ProcedureName = "dbo." + procName,
                SourceId = new Guid("b9184f70-64ea-4dc5-b23b-02fcd5f91082")
            };
        }

        private void CreateDatabaseSourceRegion()
        {
            _source = new DatabaseSourceRegion(_dbServiceModel, _modelItem, Common.Interfaces.Core.DynamicServices.enSourceType.SqlDatabase);
        }

        private void CreateDbActionRegion()
        {
            _selectedSource = _source.Sources.Single(a => a.Id == _sqlActivity.SourceId);
            _source.SelectedSource = _selectedSource;
            _dbActionRegion = new DbActionRegion(_dbServiceModel, _modelItem, _source, new SynchronousAsyncWorker());
        }

        private void CreateModelItem()
        {
            _modelItem = ModelItemUtils.CreateModelItem(_sqlActivity);
        }

        private void Setup(string cleanProcName)
        {
            CreateDbServiceModel();
            CreateSqlServerActivity(cleanProcName);
            CreateModelItem();
            CreateDatabaseSourceRegion();
            CreateDbActionRegion();
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Change_sql_source_verify_Empty_Inputs()
        {
            _containerOps = new Depends(Depends.ContainerType.MSSQL);
            var newName = Guid.NewGuid().ToString();
            var cleanProcName = newName.Replace("-", "").Replace(" ", "");
            try
            {
                var createProcedure = "CREATE procedure [dbo].[" + cleanProcName + "](@ProductId int) as Begin select * from Country select * from City end";
                var result = SqlHelper.RunSqlCommand(_containerOps.Container.IP,
                    _containerOps.Container.Port, createProcedure);
                Assert.AreEqual(-1, result);

                Setup(cleanProcName);

                var mockSource = new Mock<IDbSource>();

                IDatabaseInputRegion databaseInputRegion = new DatabaseInputRegion(_modelItem, _dbActionRegion);
                Assert.AreEqual(1, databaseInputRegion.Inputs.Count);
                Assert.AreEqual("ProductId", databaseInputRegion.Inputs.Single().Name);
                Assert.AreEqual("[[ProductId]]", databaseInputRegion.Inputs.Single().Value);
                //add testing here

                _source.SelectedSource = mockSource.Object;
                Assert.AreEqual(0, databaseInputRegion.Inputs.Count);
            }
            finally
            {
                var dropResult = DropProcedure(cleanProcName);
                Assert.AreEqual(-1, dropResult);
            }
        }

        int DropProcedure(string cleanProcName)
        {
            var dropProcedure = "IF ( OBJECT_ID('" + cleanProcName + "') IS NOT NULL ) DROP PROCEDURE [dbo].[" + cleanProcName + "]";
            var dropResult = SqlHelper.RunSqlCommand(_containerOps.Container.IP,
                _containerOps.Container.Port, dropProcedure);
            return dropResult;
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void Add_A_New_InputOnSqlProcedure_Expect_New_IS_InputAdded()
        {
            const string procName = "TestingAddingANewInput";

            Setup(procName);

            IDatabaseInputRegion databaseInputRegion = new DatabaseInputRegion(_modelItem, _dbActionRegion);
            Assert.AreEqual(1, databaseInputRegion.Inputs.Count);
            Assert.AreEqual("ProductId", databaseInputRegion.Inputs.Single().Name);
            Assert.AreEqual("[[ProductId]]", databaseInputRegion.Inputs.Single().Value);
            //testing here
            const string alterProcedure = "ALTER procedure [dbo].[" + procName + "](@ProductId int,@ProductId1 int,@ProductId2 int) as Begin select * from Country select * from City end";
            var alterTableResults = SqlHelper.RunSqlCommand(_containerOps.Container.IP,
                _containerOps.Container.Port, alterProcedure);
            Assert.AreEqual(-1, alterTableResults);

            _dbActionRegion.RefreshActionsCommand.Execute(null);
            Assert.IsNotNull(_dbActionRegion.Actions, "No Actions were generated for source: " + _selectedSource);

            var procActionsToInputs = _dbActionRegion.Actions.Single(p => p.Name.EndsWith(procName));

            Assert.AreEqual("ProductId", procActionsToInputs.Inputs.ToList()[0].Name);
            Assert.AreEqual("[[ProductId]]", procActionsToInputs.Inputs.ToList()[0].Value);

            Assert.AreEqual("ProductId1", procActionsToInputs.Inputs.ToList()[1].Name);
            Assert.AreEqual("[[ProductId1]]", procActionsToInputs.Inputs.ToList()[1].Value);

            Assert.AreEqual("ProductId2", procActionsToInputs.Inputs.ToList()[2].Name);
            Assert.AreEqual("[[ProductId2]]", procActionsToInputs.Inputs.ToList()[2].Value);
        }
    }
}
