using NUnit.Framework;
using Warewolf.Options;

namespace Warewolf.Data.Options.Tests
{
    [TestFixture]
    public class ConditionExpressionTests
    {
        [Test]
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
        
        [Test]
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
        
        [Test]
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
    }
}
