
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
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
        public void SyntaxTreeBuilder_Build_VariablesSideBySideWithNewLine_HasEventLogsIsFalse()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            var input = "[[entry]]" + Environment.NewLine + "[[var]]";  
            builder.Build(input);
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
