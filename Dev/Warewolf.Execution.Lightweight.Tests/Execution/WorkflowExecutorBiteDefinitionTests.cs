/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;
using Warewolf.Bite;
using Warewolf.Bite.Json;

namespace Warewolf.Execution.Lightweight.Tests.Execution
{
    /// <summary>
    /// A resource whose body is a JSON &lt;BiteDefinition&gt; is read by the WF-free <see cref="BiteReader"/>
    /// and executed like a XAML one: no DynamicActivity is compiled.
    /// </summary>
    [TestClass]
    public class WorkflowExecutorBiteDefinitionTests
    {
        static string XamlPath =>
            Path.Combine(AppContext.BaseDirectory, "TestResources", "hangfiredemo", "Hello World.bite");

        static string JsonResource() => BiteJsonWriter.ConvertResource(File.ReadAllText(XamlPath));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasBiteDefinition_IsTrueOnlyForAJsonBody()
        {
            Assert.IsFalse(WorkflowExecutor.HasBiteDefinition(new StringBuilder(File.ReadAllText(XamlPath))));
            Assert.IsTrue(WorkflowExecutor.HasBiteDefinition(new StringBuilder(JsonResource())));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildBitePreparedWorkflow_ReadsTheGraphWithoutCompilingAnActivity()
        {
            var fromJson = WorkflowExecutor.BuildBitePreparedWorkflow(JsonResource());

            Assert.IsTrue(fromJson.IsLoaded);
            Assert.IsTrue(fromJson.FromBite);
            Assert.IsNull(fromJson.Activity, "A JSON body is never compiled to a DynamicActivity.");
            Assert.IsNotNull(fromJson.StartActivity);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildBitePreparedWorkflow_UnknownFormatVersion_Throws()
        {
            var resource = JsonResource().Replace("FormatVersion=\"1\"", "FormatVersion=\"99\"");

            Assert.ThrowsException<BiteFormatException>(() => WorkflowExecutor.BuildBitePreparedWorkflow(resource));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void RentPreparedWorkflow_JsonBody_IsPooledAndReused()
        {
            WorkflowExecutor.ClearWorkflowPool();
            var resource = JsonResource();
            var first = WorkflowExecutor.RentPreparedWorkflow(XamlPath, null, resource);
            WorkflowExecutor.ReturnPreparedWorkflow(XamlPath, first);

            var second = WorkflowExecutor.RentPreparedWorkflow(XamlPath, null, resource);

            Assert.AreSame(first, second);
            WorkflowExecutor.ClearWorkflowPool();
        }
    }
}
