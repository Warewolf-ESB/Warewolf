/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Linq;
using System.Xml.Linq;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;



namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.InternalServices {
    /// <summary>
    /// Summary description for FindDependenciesServiceTest
    /// </summary>
    [TestClass]
    public class FindDependenciesServiceTest {

        readonly string _webserverURI = "http://localhost:3142/services/";

        [TestMethod]
       
        public void FindDependencies_ExistingService_Expected_AllDependanciesReturned() {
            //WorkflowName - WorkflowMappingsInnerWorkflow
            var postData = $"{_webserverURI}{@"FindDependencyService?ResourceId=2ac0f29a-638e-4f9a-a2cb-b9694087f96c"}";
            var response = XElement.Parse(TestHelper.PostDataToWebserver(postData));

            var nodes = response.DescendantNodes();
            var nodesArray = nodes.ToArray();

            Assert.AreEqual(5, nodesArray.Length);

            var txt0 = "<node id=\"2ac0f29a-638e-4f9a-a2cb-b9694087f96c\" x=\"\" y=\"\" broken=\"false\">\r\n  <dependency id=\"9b914373-47e4-4cc5-a169-95e8c21c8efe\" />\r\n  <dependency id=\"9b914373-47e4-4cc5-a169-95e8c21c8efe\" />\r\n  <dependency id=\"9b914373-47e4-4cc5-a169-95e8c21c8efe\" />\r\n</node>";
            Assert.AreEqual(txt0,  nodesArray[0].ToString());
            Assert.AreEqual("<dependency id=\"9b914373-47e4-4cc5-a169-95e8c21c8efe\" />", nodesArray[1].ToString());
            Assert.AreEqual("<dependency id=\"9b914373-47e4-4cc5-a169-95e8c21c8efe\" />", nodesArray[2].ToString());
            Assert.AreEqual("<dependency id=\"9b914373-47e4-4cc5-a169-95e8c21c8efe\" />", nodesArray[3].ToString());
            Assert.AreEqual("<node id=\"9b914373-47e4-4cc5-a169-95e8c21c8efe\" x=\"\" y=\"\" broken=\"false\"></node>", nodesArray[4].ToString());
        }

    }
}
