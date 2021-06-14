/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.SystemTemplates.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests
{
    [TestClass]
    public class Dev2DecisionComparerTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DecisionComparer))]
        public void Dev2DecisionComparer_Both_Null_Equals_Expect_True()
        {
            var dev2DecisionComparer = new Dev2DecisionComparer();

            var isEqual = dev2DecisionComparer.Equals(null, null);

            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DecisionComparer))]
        public void Dev2DecisionComparer_Left_Null_Equals_Expect_False()
        {
            var dev2DecisionB = new Dev2Decision
            {
                Col1 = "[[a]]",
                Col2 = "=",
                Col3 = "bob",
                EvaluationFn = Decisions.Operations.EnDecisionType.IsEqual
            };

            var dev2DecisionComparer = new Dev2DecisionComparer();

            var isEqual = dev2DecisionComparer.Equals(null, dev2DecisionB);

            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DecisionComparer))]
        public void Dev2DecisionComparer_Right_Null_Equals_Expect_False()
        {
            var dev2DecisionA = new Dev2Decision
            {
                Col1 = "[[a]]",
                Col2 = "=",
                Col3 = "bob",
                EvaluationFn = Decisions.Operations.EnDecisionType.IsEqual
            };

            var dev2DecisionComparer = new Dev2DecisionComparer();

            var isEqual = dev2DecisionComparer.Equals(dev2DecisionA, null);

            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DecisionComparer))]
        public void Dev2DecisionComparer_Equals_Expect_True()
        {
            var dev2DecisionA = new Dev2Decision
            {
                Col1 = "[[a]]",
                Col2 = "=",
                Col3 = "bob",
                EvaluationFn = Decisions.Operations.EnDecisionType.IsEqual
            };

            var dev2DecisionB = new Dev2Decision
            {
                Col1 = "[[a]]",
                Col2 = "=",
                Col3 = "bob",
                EvaluationFn = Decisions.Operations.EnDecisionType.IsEqual
            };

            var dev2DecisionComparer = new Dev2DecisionComparer();

            var isEqual = dev2DecisionComparer.Equals(dev2DecisionA, dev2DecisionB);

            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DecisionComparer))]
        public void Dev2DecisionComparer_Equals_Expect_False()
        {
            var dev2DecisionA = new Dev2Decision
            {
                Col1 = "[[a]]",
                Col2 = "=",
                Col3 = "bob",
                EvaluationFn = Decisions.Operations.EnDecisionType.Choose
            };

            var dev2DecisionB = new Dev2Decision
            {
                Col1 = "[[a]]",
                Col2 = "=",
                Col3 = "bob",
                EvaluationFn = Decisions.Operations.EnDecisionType.IsEqual
            };

            var dev2DecisionComparer = new Dev2DecisionComparer();

            var isEqual = dev2DecisionComparer.Equals(dev2DecisionA, dev2DecisionB);

            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DecisionComparer))]
        public void Dev2DecisionComparer_GetHashCode_Expected_Not_Zero()
        {
            var dev2DecisionA = new Dev2Decision
            {
                Col1 = "[[a]]",
                Col2 = "=",
                Col3 = "bob",
                EvaluationFn = Decisions.Operations.EnDecisionType.IsEqual
            };

            var dev2DecisionComparer = new Dev2DecisionComparer();

            var hashCode = dev2DecisionComparer.GetHashCode(dev2DecisionA);

            Assert.AreNotEqual(0, hashCode);
        }
    }
}
