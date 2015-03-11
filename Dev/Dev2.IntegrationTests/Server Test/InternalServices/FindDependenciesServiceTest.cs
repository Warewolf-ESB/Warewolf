
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
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
        
        private readonly string _webserverURI = ServerSettings.WebserverURI;

        [TestMethod]
        public void FindDependencies_ExistingService_Expected_AllDependanciesReturned() {
            string postData = String.Format("{0}{1}", _webserverURI, @"FindDependencyService?ResourceId=e59b7fe3-ad37-4363-8678-74601b9ea3cb");
            XElement response = XElement.Parse(TestHelper.PostDataToWebserver(postData));

            IEnumerable<XNode> nodes = response.DescendantNodes();
            int count = nodes.Count();
            // More than 2 nodes indicate that the service returned dependancies
            Assert.AreEqual(29, count);

        }

    }
}
