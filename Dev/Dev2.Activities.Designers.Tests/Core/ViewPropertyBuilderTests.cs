using System;
using System.Linq;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Core.ActionRegion;
using Dev2.Activities.Designers2.Core.Source;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Activities.Designers.Tests.Core
{
    [TestClass]
    public class ViewPropertyBuilderTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ViewPropertyBuilder_Constructor()
        {
            var act = new ViewPropertyBuilder() { };
            Assert.IsNotNull(act);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BuildProperties_GivenNullValues_ExpectNoError()
        {

            //------------Setup for test--------------------------
            var act = new ViewPropertyBuilder() { };
            //------------Execute Test---------------------------
            Assert.IsNotNull(act);
            var keyValuePairs = act.BuildProperties(null, null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, keyValuePairs.Count);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BuildProperties_GivenActionRegionNullValues_ExpectNoError()
        {

            //------------Setup for test--------------------------
            var act = new ViewPropertyBuilder() { };
            IDbActionToolRegion<IDbAction> actionRegion = new DbActionRegion();
            //------------Execute Test---------------------------
            Assert.IsNotNull(act);
            var keyValuePairs = act.BuildProperties(actionRegion, null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, keyValuePairs.Count);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BuildProperties_GivenActionAndSourceRegionRegionNullValues_ExpectNoError()
        {

            //------------Setup for test--------------------------
            var act = new ViewPropertyBuilder() { };
            IDbActionToolRegion<IDbAction> actionRegion = new DbActionRegion();
            var sourceRegion = new Mock<ISourceToolRegion<IDbSource>>();
            //------------Execute Test---------------------------
            Assert.IsNotNull(act);
            var keyValuePairs = act.BuildProperties(actionRegion, sourceRegion.Object, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, keyValuePairs.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BuildProperties_GivenActionSourceTypeRegionRegionNullValues_ExpectNoError()
        {

            //------------Setup for test--------------------------
            var act = new ViewPropertyBuilder() { };
            IDbActionToolRegion<IDbAction> actionRegion = new DbActionRegion();
            var sourceRegion = new Mock<ISourceToolRegion<IDbSource>>();
            //------------Execute Test---------------------------
            Assert.IsNotNull(act);
            var keyValuePairs = act.BuildProperties(actionRegion, sourceRegion.Object, string.Empty);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, keyValuePairs.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BuildProperties_GivenActionSourceTypeRegionRegionNullValues_ExpectNoError_DefinedValues()
        {

            //------------Setup for test--------------------------
            var act = new ViewPropertyBuilder() { };
            var serviceModel = new Mock<IDbServiceModel>();
            var dsfSqlServerDatabaseActivity = new DsfSqlServerDatabaseActivity()
            {
                ProcedureName = "proc a"
            };
            IDbActionToolRegion<IDbAction> actionRegion = new DbActionRegion(serviceModel.Object, ModelItemUtils.CreateModelItem(dsfSqlServerDatabaseActivity), new DatabaseSourceRegion(), new SynchronousAsyncWorker());

            var sourceRegion = new Mock<ISourceToolRegion<IDbSource>>();
            sourceRegion.Setup(region => region.SelectedSource).Returns(new DbSourceDefinition()
            {
                Name = "Sourcename"
            });
            //------------Execute Test---------------------------
            Assert.IsNotNull(act);
            var keyValuePairs = act.BuildProperties(actionRegion, sourceRegion.Object, "DsfSqlServerDatabaseActivity");
            //------------Assert Results-------------------------
            Assert.AreEqual(3, keyValuePairs.Count);
            var keySource = keyValuePairs.Single(pair => pair.Key == "Source :").Value;
            var keyType = keyValuePairs.Single(pair => pair.Key == "Type :").Value;
            var keyProcedure = keyValuePairs.Single(pair => pair.Key == "Procedure :").Value;
            Assert.AreEqual("Sourcename", keySource);
            Assert.AreEqual("DsfSqlServerDatabaseActivity", keyType);
            Assert.AreEqual("proc a", keyProcedure);
        }
    }
}
