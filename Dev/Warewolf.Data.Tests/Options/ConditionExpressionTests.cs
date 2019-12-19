using NUnit.Framework;
using Warewolf.Options;

namespace Warewolf.Data.Options.Tests
{
    [TestFixture]
    public class ConditionExpressionTests
    {
        [Test]
        public void ConditionExpression_()
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
    }
}
