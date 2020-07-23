using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Options;

namespace Warewolf.Data.Options.Tests
{
    [TestClass]
    public class ConditionExpressionTests
    {
        [TestMethod]
        public void ConditionExpression_GivenMatchExpression()
        {
            var condition = new ConditionExpression
            {
                Left = "[[a]]",
                Cond = new ConditionMatch
                {
                    MatchType = enDecisionType.IsEqual,
                    Right = "bob"
                }
            };

            var result = condition.ToString();
            Assert.AreEqual("[[a]] = bob", result);
        }
        
        [TestMethod]
        public void ConditionExpression_GivenBetweenExpression()
        {
            var condition = new ConditionExpression
            {
                Left = "[[a]]",
                Cond = new ConditionBetween()
                {
                    MatchType = enDecisionType.IsBetween,
                    From = "2",
                    To = "10",
                }
            };

            var result = condition.ToString();
            Assert.AreEqual("[[a]] is greater than 2 and less than 10", result);
        }
        
        [TestMethod]
        public void ConditionExpression_GivenNotBetweenExpression()
        {
            var condition = new ConditionExpression
            {
                Left = "[[a]]",
                Cond = new ConditionBetween()
                {
                    MatchType = enDecisionType.NotBetween,
                    From = "2",
                    To = "10",
                }
            };

            var result = condition.ToString();
            Assert.AreEqual("[[a]] is less than 2 and more than 10", result);
        }
        
        [TestMethod]
        public void ConditionExpression_GivenIsSingleOperand()
        {
            var condition = new ConditionExpression
            {
                Left = "[[a]]",
                Cond = new ConditionMatch()
                {
                    MatchType = enDecisionType.IsNull,
                    Right = "2",
                }
            };

            var result = condition.ToString();
            Assert.AreEqual("[[a]] Is NULL", result);
        }

        [TestMethod]
        public void ConditionExpression_ToOptions()
        {
            var condition = new ConditionExpression
            {
                Left = "[[a]]",
                Cond = new ConditionBetween()
                {
                    MatchType = enDecisionType.NotBetween,
                    From = "2",
                    To = "10",
                }
            };

            var result = condition.ToOptions();
            Assert.AreEqual(typeof(OptionConditionExpression), result[0].GetType());

            var optionConditionExpression = result[0] as OptionConditionExpression;

            Assert.IsNotNull(optionConditionExpression);
            Assert.AreEqual("[[a]]", optionConditionExpression.Left);
            Assert.AreEqual(enDecisionType.NotBetween, optionConditionExpression.MatchType);
            Assert.AreEqual("Not Between", optionConditionExpression.SelectedMatchType.Name);
            Assert.AreEqual(32, optionConditionExpression.SelectedMatchType.Value);
            Assert.AreEqual("2", optionConditionExpression.From);
            Assert.AreEqual("10", optionConditionExpression.To);
        }
    }
}
