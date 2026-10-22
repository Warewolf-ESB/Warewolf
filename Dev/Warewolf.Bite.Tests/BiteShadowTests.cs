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
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Warewolf.Bite.Tests
{
    /// <summary>Shadow mode comparison - WF removal T1.7.</summary>
    [TestClass]
    public class BiteShadowTests
    {
        const string A = "aaaaaaaa-0000-0000-0000-000000000001";
        static readonly string OneComment = Xaml.Workflow($"<Flowchart.StartNode><FlowStep>{Xaml.Comment(A, extra: "Text=\"hello\"")}</FlowStep></Flowchart.StartNode>");

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteShadow")]
        public void BiteShadow_IdenticalGraphs_ReportsAMatch()
        {
            var reference = Xaml.Read(OneComment);

            var result = BiteShadow.Compare(reference, OneComment, "r1");

            Assert.IsTrue(result.Matched, result.ToString());
            Assert.AreEqual(0, result.DifferenceCount);
            Assert.IsNull(result.CandidateError);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteShadow")]
        public void BiteShadow_DifferentValue_ReportsThePathButNeverTheValue()
        {
            const string secret = "P@ssw0rd-never-log-me";
            var reference = (DsfCommentActivity)Xaml.Read(OneComment)!;
            reference.Text = secret; // the reference graph now holds a value the XAML does not

            var result = BiteShadow.Compare(reference, OneComment, "r1");

            Assert.IsFalse(result.Matched);
            Assert.AreEqual(1, result.DifferenceCount, result.ToString());
            StringAssert.Contains(result.Differences[0], ".Text [");
            Assert.IsFalse(result.ToString().Contains(secret), "A logged shadow result must not carry property values.");
            Assert.IsFalse(result.ToString().Contains("hello"), "Nor the candidate's value.");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteShadow")]
        public void BiteShadow_CandidateReaderFails_IsReportedNotThrown()
        {
            var reference = Xaml.Read(OneComment);

            var result = BiteShadow.Compare(reference, "<not-xaml", "r1");

            Assert.IsFalse(result.Matched);
            StringAssert.Contains(result.CandidateError, nameof(BiteFormatException));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteShadow")]
        public void BiteShadow_IdsMintedForActivitiesSavedWithoutOne_AreNotDifferences()
        {
            var unsaved = Xaml.Workflow("<Flowchart.StartNode><FlowStep><uaba:DsfCommentActivity DisplayName=\"c\" /></FlowStep></Flowchart.StartNode>");
            var reference = Xaml.Read(unsaved, resourceId: "a-different-seed"); // a different minted id, as the WF path would have

            var result = BiteShadow.Compare(reference, unsaved, "r1");

            Assert.IsTrue(result.Matched, result.ToString());
        }

        [DataTestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteShadow")]
        [DataRow("true", true)]
        [DataRow("TRUE", true)]
        [DataRow(" 1 ", true)]
        [DataRow("false", false)]
        [DataRow("0", false)]
        [DataRow("", false)]
        [DataRow(null, false)]
        public void BiteShadow_IsEnabled_OnlyForTrueOrOne(string? value, bool expected)
        {
            Assert.AreEqual(expected, BiteShadow.IsEnabled(value));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteShadow")]
        public void BiteShadowParseObserver_SinkThrows_DoesNotThrowToTheCaller()
        {
            var observer = new BiteShadowParseObserver(_ => throw new InvalidOperationException("sink down"));

            observer.Parsed(Guid.NewGuid(), new StringBuilder(OneComment), Xaml.Read(OneComment)!);
        }
    }
}
