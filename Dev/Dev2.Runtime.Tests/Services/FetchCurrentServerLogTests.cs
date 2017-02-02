/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class FetchCurrentServerLogTests
    {
        #region Static Class Init

        static string _testDir;

        [ClassInitialize]
        public static void MyClassInit(TestContext context)
        {
            _testDir = context.DeploymentDirectory;
        }

        #endregion


        ExecuteMessage ConvertToMsg(StringBuilder msg)
        {
            var serialier = new Dev2JsonSerializer();
            var result = serialier.Deserialize<ExecuteMessage>(msg);
            return result;
        }

        #region CTOR

        [TestMethod]
        public void FetchCurrentServerLogConstructorWithDefaultExpectedInitializesServerLogPath()
        {
            var serverLogPath = EnvironmentVariables.ServerLogFile;

            var esb = new FetchCurrentServerLog();
            Assert.AreEqual(serverLogPath, esb.ServerLogPath);
        }

        #endregion

        #region Execute

        [TestMethod]
        public void FetchCurrentServerLogExecuteWithNonExistingLogExpectedReturnsEmptyString()
        {
            var serverLogPath = Path.Combine(_testDir, string.Format("ServerLog_{0}.txt", Guid.NewGuid()));

            var esb = new FetchCurrentServerLog(serverLogPath);
            var actual = esb.Execute(null, null);
            var msg = ConvertToMsg(actual);
            Assert.AreEqual(string.Empty, msg.Message.ToString());
        }


        [TestMethod]
        public void FetchCurrentServerLogExecuteWithExistingLogExpectedReturnsContentsOfLog()
        {
            const string Expected = "Hello world";
            var serverLogPath = Path.Combine(_testDir, string.Format("ServerLog_{0}.txt", Guid.NewGuid()));
            File.WriteAllText(serverLogPath, Expected);

            var esb = new FetchCurrentServerLog(serverLogPath);
            var actual = esb.Execute(null, null);
            var msg = ConvertToMsg(actual);
            Assert.AreEqual(Expected, msg.Message.ToString());
        }

        #endregion

        #region HandlesType

        [TestMethod]
        public void FetchCurrentServerLogHandlesTypeExpectedReturnsFetchCurrentServerLogService()
        {
            var esb = new FetchCurrentServerLog();
            var result = esb.HandlesType();
            Assert.AreEqual("FetchCurrentServerLogService", result);
        }

        #endregion

        #region CreateServiceEntry

        [TestMethod]
        public void FetchCurrentServerLogCreateServiceEntryExpectedReturnsDynamicService()
        {
            var esb = new FetchCurrentServerLog();
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

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var fetchCurrentServerLog = new FetchCurrentServerLog();

            //------------Execute Test---------------------------
            var resId = fetchCurrentServerLog.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var fetchCurrentServerLog = new FetchCurrentServerLog();

            //------------Execute Test---------------------------
            var resId = fetchCurrentServerLog.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Any, resId);
        }
    }
}
