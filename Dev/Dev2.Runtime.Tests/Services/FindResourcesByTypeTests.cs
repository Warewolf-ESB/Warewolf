/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Resources;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class FindResourcesByTypeTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(FindResourcesByType))]
        public void FindResourcesByType_Execute_ExpectIQueueSource()
        {
            var expected = new IQueueSource[]
            {
                new RabbitMQSource()
            };
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(o => o.FindByType(typeof(IQueueSource).FullName)).Returns(expected);
            //------------Setup for test-------------------------
            var service = new FindResourcesByType(new Lazy<IResourceCatalog>(() => mockResourceCatalog.Object));
            IWorkspace workspace = null;
            var values = new Dictionary<string, StringBuilder>();
            values.Add("Type", new StringBuilder(typeof(IQueueSource).FullName));

            //------------Execute Test---------------------------
            var result = service.Execute(values, workspace);


            //------------Assert Results-------------------------
            var serializer = new Dev2JsonSerializer();
            var expectedString = serializer.Serialize(expected, Newtonsoft.Json.Formatting.None);
            Assert.AreEqual(expectedString, result.ToString());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(FindResourcesByType))]
        public void FindResourcesByType_Execute_ExpectIDb()
        {
            var expected = new IDb[]
            {
                new DbSource()
            };
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(o => o.FindByType(typeof(IDb).FullName)).Returns(expected);
            //------------Setup for test-------------------------
            var service = new FindResourcesByType(new Lazy<IResourceCatalog>(() => mockResourceCatalog.Object));
            IWorkspace workspace = null;
            var values = new Dictionary<string, StringBuilder>();
            values.Add("Type", new StringBuilder(typeof(IDb).FullName));

            //------------Execute Test---------------------------
            var result = service.Execute(values, workspace);


            //------------Assert Results-------------------------
            var serializer = new Dev2JsonSerializer();
            var expectedString = serializer.Serialize(expected, Newtonsoft.Json.Formatting.None);
            Assert.AreEqual(expectedString, result.ToString());
        }
    }
}
