/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/



using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Warewolf.Data.Options;
using enDecisionType = Warewolf.Options.enDecisionType;

namespace Warewolf.Data.Tests
{
    [TestClass]
    [TestCategory(nameof(CollectionExtentions))]
    public class CollectionExtentionsTests
    {
        [TestMethod]
        [Timeout(100)]
        [Owner("Siphamandla Dube")]
        public void CollectionExtentions_IsItemDuplicate_ExpectTrue()
        {
            var testItem = "item one";
            var testList = new List<string> { { "item one" } };

            var sut = testList.IsItemDuplicate(testItem);

            Assert.IsTrue(sut);
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Siphamandla Dube")]
        public void CollectionExtentions_IsItemDuplicate_False_ExpectNoDuplicates()
        {
            var testItem = "item one";
            var testList = new HashSet<string> { { "item one" } };

            testList.AddItem(testItem, false);

            Assert.AreEqual(1, testList.Count());
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Siphamandla Dube")]
        public void CollectionExtentions_IsItemDuplicate_False_ExpectDuplicates()
        {
            var testItem = "item one";
            var testList = new List<string> { { "item one" } };

            testList.AddItem(testItem, false);

            Assert.AreEqual(2, testList.Count());
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(CollectionExtentions))]
        public void ConditionExpression_ToConditions()
        {

            var conditions = new List<ConditionExpression>
            {
                new ConditionExpression
                {
                    Left = "[[a]]",
                    Cond = new ConditionBetween()
                    {
                        MatchType = enDecisionType.IsBetween,
                        From = "2",
                        To = "10",
                    }
                },
                new ConditionExpression
                {
                    Left = "[[b]]",
                    Cond = new ConditionMatch()
                    {
                        MatchType = enDecisionType.NotBetween,
                        Right = "22",
                    }
                },
                new ConditionExpression
                {
                    Cond = new ConditionMatch()
                    {
                        MatchType = enDecisionType.Choose
                    }
                }
            };

            var result = conditions.ToConditions().ToList();

            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(typeof(ConditionBetween), result[0].GetType());

            var conditionBetween = result[0] as ConditionBetween;

            Assert.IsNotNull(conditionBetween);
            Assert.AreEqual("[[a]]", conditionBetween.Left);
            Assert.AreEqual(enDecisionType.IsBetween, conditionBetween.MatchType);
            Assert.AreEqual("2", conditionBetween.From);
            Assert.AreEqual("10", conditionBetween.To);
        }
    }
}
