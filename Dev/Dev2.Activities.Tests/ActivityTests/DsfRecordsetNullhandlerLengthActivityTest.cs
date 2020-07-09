using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using ActivityUnitTests;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Enums;
using Dev2.Diagnostics;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Storage;
using WarewolfParserInterop;

namespace Dev2.Tests.Activities.ActivityComparerTests.CountRecordset
{
    [TestClass]
    public class DsfRecordsetNullhandlerLengthActivityTest : BaseActivityUnitTest
    {
        [TestMethod]
        [Timeout(60000)]
        [TestCategory("DsfRecordsetNullhandlerLength")]
        [Owner("Candice Daniel")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void RecordsetNullhandlerLength_BlankResultVariable()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var environmentID = Guid.Empty;
            var resourceID = Guid.NewGuid();
            var env = new ExecutionEnvironment();
            User = new Mock<IPrincipal>().Object;
            var dataObject = new DsfDataObject(CurrentDl, ExecutionId)
            {

                ServerID = Guid.NewGuid(),
                ExecutingUser = User,
                IsDebug = true,
                EnvironmentID = environmentID,
                Environment = env,
                IsRemoteInvokeOverridden = false,
                DataList = new StringBuilder(CurrentDl),
                IsServiceTestExecution = true
            };
            env.Assign("[[Person().Name]]", "Bob", 0);
            env.Assign("[[Person().Name]]", "Dora", 0);
            env.Assign("[[Person().Name]]", "Superman", 0);
            env.Assign("[[Person().Name]]", "Batman", 0);
            env.Assign("[[Person().Name]]", "Orlando", 0);
            var act = new DsfActivity
            {
                ResourceID = new InArgument<Guid>(resourceID),
                EnvironmentID = Guid.Empty
            };
            var activity = new DsfRecordsetNullhandlerLengthActivity() { UniqueID = uniqueId, RecordsetName = "[[list()]]" };
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            var result = ExecuteProcess(dataObject, true, null, false, true, false, environmentID);
            //---------------Assert Precondition----------------
            Assert.AreEqual(Warewolf.Resource.Errors.ErrorResource.BlankResultVariable, result.Environment.FetchErrors());
        }
    }
}
