/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.InternalServices {
    /// <summary>
    /// Summary description for FindDependenciesServiceTest
    /// </summary>
    [TestClass]
    public class FindDependenciesServiceTest {
        
        private readonly string _webserverURI = "http://localhost:3142/services/";

        [TestMethod]
       
        public void FindDependencies_ExistingService_Expected_AllDependanciesReturned() {
            //WorkflowName - WorkflowMappingsInnerWorkflow
            string postData = $"{_webserverURI}{@"FindDependencyService?ResourceId=2ac0f29a-638e-4f9a-a2cb-b9694087f96c"}";
            var response = XElement.Parse(TestHelper.PostDataToWebserver(postData));

            var nodes = response.DescendantNodes();
            var count = nodes.Count();
            // More than 2 nodes indicate that the service returned dependancies
            Assert.AreEqual(5, count);

        }

    }
}
