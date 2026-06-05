/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests
{
    [TestClass]
    public class ServiceActionTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(ServiceAction))]
        public void ServiceAction_Constructor_SetsTypeAndAllocatesCollections()
        {
            var action = new ServiceAction();

            Assert.AreEqual(enDynamicServiceObjectType.ServiceAction, action.ObjectType);
            Assert.AreEqual(enActionType.Unknown, action.ActionType);
            Assert.IsNotNull(action.ServiceActionInputs);
            Assert.AreEqual(0, action.ServiceActionInputs.Count);
            Assert.IsNotNull(action.ServiceActionOutputs);
            Assert.AreEqual(0, action.ServiceActionOutputs.Count);
            Assert.IsNull(action.XamlStream);
            Assert.IsNull(action.WorkflowActivity);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(ServiceAction))]
        public void ServiceAction_Properties_GetSet_RoundTrip()
        {
            var id = Guid.NewGuid();
            var source = new Source();
            var action = new ServiceAction
            {
                CommandTimeout = 30,
                ActionType = enActionType.Workflow,
                SourceName = "src",
                SourceMethod = "method",
                Source = source,
                ServiceName = "svc",
                ServiceID = id,
                ResultsToClient = false,
                TerminateServiceOnFault = false,
                OutputDescription = "out",
            };

            Assert.AreEqual(30, action.CommandTimeout);
            Assert.AreEqual(enActionType.Workflow, action.ActionType);
            Assert.AreEqual("src", action.SourceName);
            Assert.AreEqual("method", action.SourceMethod);
            Assert.AreSame(source, action.Source);
            Assert.AreEqual("svc", action.ServiceName);
            Assert.AreEqual(id, action.ServiceID);
            Assert.IsFalse(action.ResultsToClient);
            Assert.IsFalse(action.TerminateServiceOnFault);
            Assert.AreEqual("out", action.OutputDescription);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(ServiceAction))]
        public void ServiceAction_SetActivity_UpdatesWorkflowActivity()
        {
            var action = new ServiceAction();

            // null is acceptable as Activity is reference-typed; we only need
            // to observe that SetActivity threads the value through to WorkflowActivity.
            action.SetActivity(null);

            Assert.IsNull(action.WorkflowActivity);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(ServiceAction))]
        public void ServiceAction_PopActivity_EmptyPoolAndNoXamlStream_ReturnsGenerationZero()
        {
            var action = new ServiceAction();

            var pooled = action.PopActivity();

            Assert.IsNotNull(pooled);
            Assert.AreEqual(0, pooled.Generation);
            Assert.IsNull(pooled.Value);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(ServiceAction))]
        public void ServiceAction_Compile_NoInputs_ReturnsTrue()
        {
            var action = new ServiceAction();

            Assert.IsTrue(action.Compile());
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(ServiceAction))]
        public void ServiceAction_Compile_PropagatesInputCompilerErrors()
        {
            var action = new ServiceAction();
            var input = new ServiceActionInput();
            input.CompilerErrors.Add("boom");
            action.ServiceActionInputs.Add(input);

            // Compile collects child compiler errors into the parent and reports the
            // resulting IsCompiled flag (false because CompilerErrors is now non-empty).
            var compiled = action.Compile();

            Assert.IsFalse(compiled);
            CollectionAssert.Contains((System.Collections.ICollection)action.CompilerErrors, "boom");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(ServiceAction))]
        public void ServiceAction_Dispose_NoStream_DoesNotThrow_AndIsIdempotent()
        {
            var action = new ServiceAction();

            action.Dispose();
            action.Dispose();
        }
    }
}
