
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.AppResources.DependencyVisualization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.DependencyGraph
{
    [TestClass]
    public class NodeTest
    {
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Node_ToString")]
        public void Node_ToString_WhenNotBroken_ExpectStringNode()
        {
            //------------Setup for test--------------------------
            var node = new Node("Node 1", 100, 100, false, false);
            const string expected = @"<node id=""Node 1"" x=""100"" y=""100"" broken=""False""></node>";

            //------------Execute Test---------------------------
            var result = node.ToString();

            //------------Assert Results-------------------------
            StringAssert.Contains(result, expected);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Node_ToString")]
        public void Node_ToString_WhenBroken_ExpectStringNode()
        {
            //------------Setup for test--------------------------
            var node = new Node("Node 1", 100, 100, false, true);
            const string expected = @"<node id=""Node 1"" x=""100"" y=""100"" broken=""True""></node>";

            //------------Execute Test---------------------------
            var result = node.ToString();

            //------------Assert Results-------------------------
            StringAssert.Contains(result, expected);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Node_ToString")]
        public void Node_ToString_WhenNodeContainsDependencies_ExpectStringNodeWithDependenices()
        {
            //------------Setup for test--------------------------
            var node = new Node("Node 1", 100, 100, true, true);
            node.NodeDependencies.Add(new Node("Dependant Node",200,100,false,false));
            const string expected = @"<node id=""Node 1"" x=""100"" y=""100"" broken=""True""><dependency id=""Dependant Node"" /></node>";
            
            //------------Execute Test---------------------------
            var result = node.ToString();

            //------------Assert Results-------------------------
            StringAssert.Contains(result, expected);
        }
    }
}
