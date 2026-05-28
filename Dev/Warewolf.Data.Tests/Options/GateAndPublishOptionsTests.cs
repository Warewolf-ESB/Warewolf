/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Data.Options;
using Warewolf.Data.Options.Enums;

namespace Warewolf.Data.Tests.Options
{
    /// <summary>
    /// Track-C T1 batch: <see cref="GateOptions"/>, <see cref="NoBackoff"/>,
    /// <see cref="RabbitMqPublishOptions"/>, <see cref="FileParameter"/>, and the small nested
    /// Resume/Correlation types that ride along cheaply.
    /// </summary>
    [TestClass]
    public class GateAndPublishOptionsTests
    {
        const string Owner = "Ashley Lewis";
        const string Category = "Warewolf.Data.Options";

        // ---- GateOptions -------------------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void GateOptions_Default_GateOpts_IsContinue()
        {
            var gate = new GateOptions();
            Assert.IsInstanceOfType(gate.GateOpts, typeof(Continue));
            Assert.AreEqual(GateResumeAction.Continue, gate.GateOpts.Resume);
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void GateOptions_Notify_WithHandler_Fires()
        {
            var gate = new GateOptions();
            var calls = 0;
            gate.OnChange += () => calls++;
            gate.Notify();
            gate.Notify();
            Assert.AreEqual(2, calls);
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void GateOptions_Notify_NoHandler_DoesNotThrow()
        {
            var gate = new GateOptions();
            gate.Notify(); // no subscribers — null-conditional path
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void GateOptions_GateOpts_CanBeSetToEndWorkflow()
        {
            var gate = new GateOptions { GateOpts = new EndWorkflow() };
            Assert.AreEqual(GateResumeAction.EndWorkflow, gate.GateOpts.Resume);
        }

        // ---- Continue / EndWorkflow --------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void Continue_Default_Resume_AndStrategy()
        {
            var c = new Continue();
            Assert.AreEqual(GateResumeAction.Continue, c.Resume);
            Assert.IsInstanceOfType(c.Strategy, typeof(NoBackoff));
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void EndWorkflow_Default_Resume_IsEndWorkflow()
        {
            var e = new EndWorkflow();
            Assert.AreEqual(GateResumeAction.EndWorkflow, e.Resume);
        }

        // ---- NoBackoff ---------------------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void NoBackoff_Default_Algorithm_AndMaxRetries()
        {
            var nb = new NoBackoff();
            Assert.AreEqual(RetryAlgorithm.NoBackoff, nb.RetryAlgorithm);
            Assert.AreEqual(3, nb.MaxRetries);
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void NoBackoff_Create_Default_YieldsThreeTruesThenFalse()
        {
            var nb = new NoBackoff();
            var sequence = nb.Create().ToList();
            CollectionAssert.AreEqual(new[] { true, true, true, false }, sequence);
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void NoBackoff_Create_ZeroMaxRetries_YieldsOnlyFalse()
        {
            var nb = new NoBackoff { MaxRetries = 0 };
            var sequence = nb.Create().ToList();
            CollectionAssert.AreEqual(new[] { false }, sequence);
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void NoBackoff_Create_CustomMaxRetries_YieldsCustomThenFalse()
        {
            var nb = new NoBackoff { MaxRetries = 5 };
            var sequence = nb.Create().ToList();
            Assert.AreEqual(6, sequence.Count);
            Assert.IsTrue(sequence.Take(5).All(b => b));
            Assert.IsFalse(sequence[5]);
        }

        // ---- RabbitMqPublishOptions --------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void RabbitMqPublishOptions_Default_AutoCorrelation_IsExecutionID()
        {
            var rmq = new RabbitMqPublishOptions();
            Assert.IsInstanceOfType(rmq.AutoCorrelation, typeof(ExecutionID));
            Assert.AreEqual(CorrelationAction.ExecutionID, rmq.AutoCorrelation.Correlation);
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void RabbitMqPublishOptions_Notify_WithHandler_Fires()
        {
            var rmq = new RabbitMqPublishOptions();
            var calls = 0;
            rmq.OnChange += () => calls++;
            rmq.Notify();
            Assert.AreEqual(1, calls);
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void RabbitMqPublishOptions_Notify_NoHandler_DoesNotThrow()
        {
            var rmq = new RabbitMqPublishOptions();
            rmq.Notify();
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void ExecutionID_Default_Correlation_IsExecutionID()
        {
            Assert.AreEqual(CorrelationAction.ExecutionID, new ExecutionID().Correlation);
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void CustomTransactionID_Default_Correlation_IsCustomTransactionID()
        {
            Assert.AreEqual(CorrelationAction.CustomTransactionID, new CustomTransactionID().Correlation);
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void Manual_Default_Correlation_IsManual_AndCorrelationIdRoundtrips()
        {
            var m = new Manual { CorrelationID = "abc-123" };
            Assert.AreEqual(CorrelationAction.Manual, m.Correlation);
            Assert.AreEqual("abc-123", m.CorrelationID);
        }

        // ---- FileParameter -----------------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void FileParameter_Properties_Roundtrip()
        {
            var fp = new FileParameter
            {
                Key = "k",
                FileName = "f.txt",
                ContentType = "text/plain",
                FileBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 }),
            };
            Assert.AreEqual("k", fp.Key);
            Assert.AreEqual("f.txt", fp.FileName);
            Assert.AreEqual("text/plain", fp.ContentType);
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, fp.FileBytes);
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FileParameter_FileBytes_NullBase64_ThrowsArgumentNullException()
        {
            var fp = new FileParameter { FileBase64 = null };
            _ = fp.FileBytes;
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FileParameter_FileBytes_EmptyBase64_ThrowsArgumentNullException()
        {
            var fp = new FileParameter { FileBase64 = "" };
            _ = fp.FileBytes;
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        [ExpectedException(typeof(FormatException))]
        public void FileParameter_FileBytes_InvalidBase64_RethrowsFromCatch()
        {
            // not-base64 hits Convert.FromBase64String which throws FormatException;
            // the inner catch re-throws, so callers see the original exception.
            var fp = new FileParameter { FileBase64 = "###not-base64###" };
            _ = fp.FileBytes;
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void FileParameter_RenderDescription_IncludesKeyContentAndFileName()
        {
            var fp = new FileParameter { Key = "k", FileName = "f.txt", FileBase64 = "QUJD" };
            var sb = new StringBuilder();
            fp.RenderDescription(sb);
            var text = sb.ToString();
            StringAssert.Contains(text, "Key: k");
            StringAssert.Contains(text, "File Content: QUJD");
            StringAssert.Contains(text, "File Name: f.txt");
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void FileParameter_IsEmptyRow_OnlyTrue_WhenAllFieldsEmpty()
        {
            Assert.IsTrue(new FileParameter().IsEmptyRow);
            // Any single field populated should flip IsEmptyRow false (uses &= so all must be empty).
            Assert.IsFalse(new FileParameter { Key = "k" }.IsEmptyRow);
            Assert.IsFalse(new FileParameter { FileName = "f" }.IsEmptyRow);
            Assert.IsFalse(new FileParameter { FileBase64 = "b" }.IsEmptyRow);
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void FileParameter_IsIncompleteRow_TrueWhenAnyFieldEmpty_FalseOnlyWhenAllSet()
        {
            // |= semantics: true if any of Key/FileName/FileBase64 is empty.
            Assert.IsTrue(new FileParameter().IsIncompleteRow);
            Assert.IsTrue(new FileParameter { Key = "k" }.IsIncompleteRow);
            Assert.IsTrue(new FileParameter { Key = "k", FileName = "f" }.IsIncompleteRow);
            Assert.IsFalse(new FileParameter { Key = "k", FileName = "f", FileBase64 = "b" }.IsIncompleteRow);
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void TextParameter_IsEmptyRow_IsIncompleteRow_Semantics()
        {
            // TextParameter sits right next to FileParameter in the same file and rides this batch.
            Assert.IsTrue(new TextParameter().IsEmptyRow);
            Assert.IsTrue(new TextParameter().IsIncompleteRow);
            Assert.IsFalse(new TextParameter { Key = "k", Value = "v" }.IsEmptyRow);
            Assert.IsFalse(new TextParameter { Key = "k", Value = "v" }.IsIncompleteRow);
            Assert.IsFalse(new TextParameter { Key = "k" }.IsEmptyRow);   // not both empty
            Assert.IsTrue(new TextParameter { Key = "k" }.IsIncompleteRow); // value still empty
        }
    }
}
