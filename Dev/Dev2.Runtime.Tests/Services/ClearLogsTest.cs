
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ClearLogsTest
    {
        private static readonly Guid _workspaceID = Guid.Parse("34c0ce48-1f02-4a47-ad51-19ee3789ed4c");
        readonly static object SyncRoot = new object();

        static string _testDir;

        #region ClassInitialize

        [ClassInitialize]
        public static void MyClassInitialize(TestContext context)
        {
            _testDir = Path.Combine(context.DeploymentDirectory, "ClearLogs");
            Directory.CreateDirectory(_testDir);
        }

        #endregion

        #region Execute

        [TestMethod]
        public void ClearLogNonValidDirectoryExpectsNoDelete()
        {
            //setup
            const string dir = "c:/no_suc_directory_will_Ever_Exist_unless_you_Are_a_cake";

            //execute
            var clearLog = new ClearLog();
            var dict = new Dictionary<string, StringBuilder> { { "Directory", new StringBuilder(dir) } };
            var result = clearLog.Execute(dict, GetWorkspace().Object);

            //assert
            Assert.IsTrue(result.Contains("No such directory exists on the server."));
        }

        [TestMethod]
        public void ClearLogNoDirectoryPassedExpectsNoDelete()
        {
            //execute
            var clearLog = new ClearLog();
            var dict = new Dictionary<string, StringBuilder>();
            var result = clearLog.Execute(dict, GetWorkspace().Object);

            //assert
            Assert.IsTrue(result.Contains("Cant delete a file if no directory is passed."));
        }

        [TestMethod]
        public void ClearLogDirectoryExpectsFilesDeleted()
        {
            lock(SyncRoot)
            {
                //setup
                var fileName1 = Guid.NewGuid().ToString() + "_Test.log";
                var fileName2 = Guid.NewGuid().ToString() + "_Test.log";

                var path1 = Path.Combine(_testDir, fileName1);
                File.WriteAllText(path1, @"hello test");
                Assert.IsTrue(File.Exists(path1));

                var path2 = Path.Combine(_testDir, fileName2);
                File.WriteAllText(path2, @"hello test");
                Assert.IsTrue(File.Exists(path2));

                //execute
                var values = new Dictionary<string, StringBuilder> { { "Directory", new StringBuilder(_testDir) } };
                var esb = new ClearLog();
                var result = esb.Execute(values, GetWorkspace().Object);

                //assert
                Assert.IsTrue(result.Contains("Success"));
                Assert.IsFalse(File.Exists(path1));
                Assert.IsFalse(File.Exists(path2));
            }
        }

        [TestMethod]
        public void ClearLogExecuteWithValidPathAndLockedExpectedReturnsError()
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            lock(SyncRoot)
            {
                //setup
                var fileName1 = Guid.NewGuid().ToString() + "_Test.log";
                var fileName2 = Guid.NewGuid().ToString() + "_Test.log";

                var path1 = Path.Combine(_testDir, fileName1);
                File.WriteAllText(path1, @"hello test");
                Assert.IsTrue(File.Exists(path1));

                var path2 = Path.Combine(_testDir, fileName2);
                File.WriteAllText(path2, @"hello test");
                Assert.IsTrue(File.Exists(path2));

                var fs = File.OpenRead(path2);

                //execute
                StringBuilder result;
                try
                {
                    var values = new Dictionary<string, StringBuilder> { { "Directory", new StringBuilder(_testDir) } };
                    var esb = new ClearLog();
                    result = esb.Execute(values, GetWorkspace().Object);
                }
                finally
                {
                    fs.Close();
                }

                var msg = serializer.Deserialize<ExecuteMessage>(result.ToString());

                //assert
                Assert.IsTrue(msg.Message.ToString().StartsWith("Error clearing "));
            }
        }

        #endregion Exeute

        #region HandlesType

        [TestMethod]
        public void DeleteLogHandlesTypeExpectedReturnsDeleteLogService()
        {
            var esb = new ClearLog();
            var result = esb.HandlesType();
            Assert.AreEqual("ClearLogService", result);
        }

        #endregion

        #region CreateServiceEntry

        [TestMethod]
        public void DeleteLogCreateServiceEntryExpectedReturnsDynamicService()
        {
            var esb = new ClearLog();
            var result = esb.CreateServiceEntry();
            Assert.AreEqual(esb.HandlesType(), result.Name);
            Assert.AreEqual("<DataList><Directory ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", result.DataListSpecification.ToString());
            Assert.AreEqual(1, result.Actions.Count);

            var serviceAction = result.Actions[0];
            Assert.AreEqual(esb.HandlesType(), serviceAction.Name);
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, serviceAction.ActionType);
            Assert.AreEqual(esb.HandlesType(), serviceAction.SourceMethod);
        }

        #endregion

        #region Helpers

        private Mock<IWorkspace> GetWorkspace()
        {
            var mock = new Mock<IWorkspace>();
            mock.Setup(w => w.ID).Returns(_workspaceID);
            return mock;
        }

        #endregion
    }
}
