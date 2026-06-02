/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    /// <summary>
    /// Branch coverage for <see cref="Service.CreateInputsMethod"/>. The method
    /// is invoked indirectly by every Service subclass (DbService, WebService,
    /// PluginService, …) but several individual branches are not exercised by
    /// existing tests. We drive these via <see cref="DbService"/> as the
    /// simplest concrete subclass.
    /// </summary>
    [TestClass]
    [TestCategory("Runtime Hosting")]
    [ExcludeFromCodeCoverage]
    public class ServiceCreateInputsMethodBranchTests
    {
        static XElement BuildServiceXml(string innerActionFragment) =>
            XElement.Parse(
                $@"<Service ID=""{Guid.NewGuid()}"" Name=""Svc"" ResourceType=""DbService"">
                     <Actions>
                       <Action Name=""dbo.X"" Type=""InvokeStoredProc"" SourceID=""{Guid.NewGuid()}"" SourceName=""s"" SourceMethod=""dbo.X"">
                         {innerActionFragment}
                         <Outputs />
                       </Action>
                     </Actions>
                   </Service>");

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Service))]
        public void CreateInputsMethod_EmptySelfClosedInput_IsSkipped()
        {
            // <Input /> with no attributes and IsEmpty -> continue branch.
            var xml = BuildServiceXml(@"<Inputs><Input /></Inputs>");

            var sut = new DbService(xml);

            Assert.IsNotNull(sut.Method);
            Assert.AreEqual(0, sut.Method.Parameters.Count,
                "Empty self-closed <Input /> elements should be skipped.");
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Service))]
        public void CreateInputsMethod_InputWithValidatorRequired_SetsIsRequiredTrue()
        {
            var xml = BuildServiceXml(
                @"<Inputs>
                    <Input Name=""userId"" Source=""userId"" EmptyToNull=""false"" DefaultValue="""" NativeType=""System.Int32"">
                      <Validator Type=""Required"" />
                    </Input>
                  </Inputs>");

            var sut = new DbService(xml);

            Assert.AreEqual(1, sut.Method.Parameters.Count);
            var p = sut.Method.Parameters[0];
            Assert.AreEqual("userId", p.Name);
            Assert.IsTrue(p.IsRequired, "Validator Type=Required should set IsRequired true.");
            Assert.AreEqual("System.Int32", p.TypeName);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Service))]
        public void CreateInputsMethod_InputWithValidatorNonRequired_LeavesIsRequiredFalse()
        {
            var xml = BuildServiceXml(
                @"<Inputs>
                    <Input Name=""x"" Source=""x"" EmptyToNull=""false"" DefaultValue="""" NativeType=""System.String"">
                      <Validator Type=""SomethingElse"" />
                    </Input>
                  </Inputs>");

            var sut = new DbService(xml);

            Assert.AreEqual(1, sut.Method.Parameters.Count);
            Assert.IsFalse(sut.Method.Parameters[0].IsRequired,
                "Validator Type other than Required should leave IsRequired false.");
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Service))]
        public void CreateInputsMethod_InputWithoutNativeType_DefaultsToSystemObject()
        {
            // No NativeType attribute -> typeof(object) fallback path.
            var xml = BuildServiceXml(
                @"<Inputs>
                    <Input Name=""anything"" Source=""anything"" EmptyToNull=""false"" DefaultValue="""" />
                  </Inputs>");

            var sut = new DbService(xml);

            Assert.AreEqual(1, sut.Method.Parameters.Count);
            Assert.AreEqual(typeof(object).FullName, sut.Method.Parameters[0].TypeName);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Service))]
        public void CreateInputsMethod_InputWithEmptyToNullTrue_ParsesToTrue()
        {
            var xml = BuildServiceXml(
                @"<Inputs>
                    <Input Name=""n"" Source=""n"" EmptyToNull=""true"" DefaultValue=""dflt"" NativeType=""System.String"" />
                  </Inputs>");

            var sut = new DbService(xml);

            Assert.AreEqual(1, sut.Method.Parameters.Count);
            var p = sut.Method.Parameters[0];
            Assert.IsTrue(p.EmptyToNull, "EmptyToNull=true should parse to true.");
            Assert.AreEqual("dflt", p.DefaultValue);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Service))]
        public void CreateInputsMethod_InputWithUnparseableEmptyToNull_DefaultsToFalse()
        {
            // bool.TryParse fails -> short-circuit && leaves the result false.
            var xml = BuildServiceXml(
                @"<Inputs>
                    <Input Name=""n"" Source=""n"" EmptyToNull=""not-a-bool"" DefaultValue="""" NativeType=""System.String"" />
                  </Inputs>");

            var sut = new DbService(xml);

            Assert.AreEqual(1, sut.Method.Parameters.Count);
            Assert.IsFalse(sut.Method.Parameters[0].EmptyToNull,
                "Non-bool EmptyToNull value should fall back to false.");
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Service))]
        public void CreateInputsMethod_ActionWithExecuteAction_PrefersExecuteActionOverSourceMethod()
        {
            // ExecuteAction attribute present -> overrides SourceMethod for ExecuteAction.
            var xml = XElement.Parse(
                $@"<Service ID=""{Guid.NewGuid()}"" Name=""Svc"" ResourceType=""DbService"">
                     <Actions>
                       <Action Name=""dbo.X"" Type=""InvokeStoredProc""
                               SourceID=""{Guid.NewGuid()}"" SourceName=""s""
                               SourceMethod=""dbo.X""
                               ExecuteAction=""SomeOtherAction"">
                         <Inputs />
                         <Outputs />
                       </Action>
                     </Actions>
                   </Service>");

            var sut = new DbService(xml);

            Assert.AreEqual("dbo.X", sut.Method.Name, "Name comes from SourceMethod.");
            Assert.AreEqual("SomeOtherAction", sut.Method.ExecuteAction,
                "ExecuteAction attribute should override SourceMethod for the ExecuteAction field.");
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Service))]
        public void CreateInputsMethod_ActionWithoutExecuteAction_FallsBackToSourceMethod()
        {
            // ExecuteAction missing -> falls back to SourceMethod (the other arm
            // of the IsNullOrEmpty ternary on line 133 of Service.cs).
            var xml = BuildServiceXml(@"<Inputs />");

            var sut = new DbService(xml);

            Assert.AreEqual("dbo.X", sut.Method.Name);
            Assert.AreEqual("dbo.X", sut.Method.ExecuteAction,
                "Missing ExecuteAction should fall back to SourceMethod.");
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Service))]
        public void CreateInputsMethod_MultipleInputs_AllParametersAreCaptured()
        {
            // Multiple Inputs, mix of attribute-bearing and self-closed ->
            // exercises the loop body and the continue branch in the same call.
            var xml = BuildServiceXml(
                @"<Inputs>
                    <Input Name=""a"" Source=""a"" EmptyToNull=""false"" DefaultValue="""" NativeType=""System.String"" />
                    <Input />
                    <Input Name=""b"" Source=""b"" EmptyToNull=""true"" DefaultValue=""0"" NativeType=""System.Int32"" />
                  </Inputs>");

            var sut = new DbService(xml);

            Assert.AreEqual(2, sut.Method.Parameters.Count,
                "The empty self-closed input should be skipped, leaving 2 captured parameters.");
            CollectionAssert.AreEquivalent(
                new[] { "a", "b" },
                sut.Method.Parameters.Select(p => p.Name).ToArray());
        }
    }
}
