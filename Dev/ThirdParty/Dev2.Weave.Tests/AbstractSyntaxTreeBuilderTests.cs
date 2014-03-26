using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Parsing.Intellisense;

namespace Dev2.Weave.Tests
{
    [TestClass] // ReSharper disable InconsistentNaming
    public class AbstractSyntaxTreeBuilderTests
    {
        [TestMethod]
        [Owner("Tshepo")]
        [TestCategory("SyntaxTreeBuilder_Build")]
        public void SyntaxTreeBuilder_Build_SingleVariable_HasEventLogsIsFalse()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[[entry]]");
            Assert.IsFalse(builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        [Owner("Tshepo")]
        [TestCategory("SyntaxTreeBuilder_Build")]
        public void SyntaxTreeBuilder_Build_VariablesSideBySide_HasEventLogsIsFalse()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[[entry]][[var]]");
            Assert.IsFalse(builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        [Owner("Tshepo")]
        [TestCategory("SyntaxTreeBuilder_Build")]
        public void SyntaxTreeBuilder_Build_VariablesSideBySideWithAnAdditionOperator_HasEventLogsIsFalse()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[[entry]]+[[var]]");
            Assert.IsFalse(builder.EventLog.HasEventLogs);
        }


        [TestMethod]
        [Owner("Tshepo")]
        [TestCategory("SyntaxTreeBuilder_Build")]
        public void SyntaxTreeBuilder_Build_VariablesSideBySide_WithModOperator_HasEventLogsIsFalse()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[[rec().a]]%[[rec().b]] ");
            Assert.IsFalse(builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        [Owner("Tshepo")]
        [TestCategory("SyntaxTreeBuilder_Build")]
        public void SyntaxTreeBuilder_Build_InvalidVar_HasEventLogsIsTrue()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[[entry");
            Assert.IsTrue(builder.EventLog.HasEventLogs);
        }
    }
}
