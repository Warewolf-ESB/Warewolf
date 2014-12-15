
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FindLogDirectoryTests
    {
        static string _testDir;
        readonly static object MonitorLock = new object();

        #region ClassInitialize

        [ClassInitialize]
        public static void MyClassInitialize(TestContext context)
        {
            _testDir = Path.Combine(context.DeploymentDirectory, "TestLogDirectory");
            Directory.CreateDirectory(_testDir);
        }

        #endregion

        #region TestInitialize/Cleanup

        [TestInitialize]
        public void MyTestInitialize()
        {
            Monitor.Enter(MonitorLock);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            Monitor.Exit(MonitorLock);
        }

        #endregion

        #region Execution

        [TestMethod]
        public void FindLogDirectoryWithNoWebServerUriReturnsError()
        {
            var workspace = new Mock<IWorkspace>();

            Dictionary<string, StringBuilder> values = new Dictionary<string, StringBuilder>();
            var esb = new FindLogDirectory();
            var result = esb.Execute(values, workspace.Object);
            Assert.IsTrue(result.Contains("Value cannot be null"));
        }

        #endregion


        #region HandlesType

        [TestMethod]
        public void FindLogDirectoryHandlesTypeExpectedReturnsDeleteLogService()
        {
            var esb = new FindLogDirectory();
            var result = esb.HandlesType();
            Assert.AreEqual("FindLogDirectoryService", result);
        }

        #endregion

        #region CreateServiceEntry

        [TestMethod]
        public void FindLogDirectoryCreateServiceEntryExpectedReturnsDynamicService()
        {
            var esb = new FindLogDirectory();
            var result = esb.CreateServiceEntry();
            Assert.AreEqual(esb.HandlesType(), result.Name);
            Assert.AreEqual("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", result.DataListSpecification.ToString());
            Assert.AreEqual(1, result.Actions.Count);

            var serviceAction = result.Actions[0];
            Assert.AreEqual(esb.HandlesType(), serviceAction.Name);
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, serviceAction.ActionType);
            Assert.AreEqual(esb.HandlesType(), serviceAction.SourceMethod);
        }

        #endregion
    }
}
