
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable CheckNamespace
namespace Dev2.Integration.Tests.Internal_Services
{
    /// <summary>
    /// Summary description for SystemServices
    /// </summary>
    [TestClass]
    public class RenameResourceCategoryServicesTest
    {
        // ReSharper disable InconsistentNaming
        private readonly string _webServerURI = ServerSettings.WebserverURI;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Description("Can call to RenameResourceCategory with correct arguments")]
        [Owner("Huggs")]
        public void RenameResourceCategoryService_WithValidArguments_ExpectSuccessResult()
        {
            string postData = string.Format("{0}{1}?{2}", _webServerURI, "RenameResourceCategoryService", "OldCategory=Integration Test Resources\\RenameCategoryTest&NewCategory=Integration Test Resources\\TestCategory&ResourceType=WorkflowService");
            string actual = TestHelper.PostDataToWebserver(postData);
            Assert.IsFalse(string.IsNullOrEmpty(actual));
            Assert.AreEqual("<CompilerMessage>Updated Category from 'Integration Test Resources\\RenameCategoryTest' to 'Integration Test Resources\\TestCategory'</CompilerMessage>", actual);
        }
        
    }
}
