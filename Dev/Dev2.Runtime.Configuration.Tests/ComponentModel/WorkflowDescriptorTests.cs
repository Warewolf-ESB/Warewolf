
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
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using Dev2.Runtime.Configuration.ComponentModel;
using Dev2.Runtime.Configuration.Tests.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Runtime.Configuration.Tests.ComponentModel
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class WorkflowDescriptorTests
    {
        #region CTOR

        [TestMethod]
        public void ConstructorWithNullXmlArgumentExpectedDoesNotThrowException()
        {
            var workflow = new WorkflowDescriptor(null);
            Assert.IsNotNull(workflow);
        }

        [TestMethod]
        public void ConstructorWithInvalidXmlArgumentExpectedDoesNotThrowException()
        {
            var workflow = new WorkflowDescriptor(new XElement("x", new XElement("y"), new XElement("z")));
            Assert.IsNotNull(workflow);
        }

        [TestMethod]
        public void ConstructorWithValidXmlArgumentExpectedInitializesAllProperties()
        {
            var xml = XmlResource.Fetch("Workflow");
            var workflow = new WorkflowDescriptor(xml);

            var properties = workflow.GetType().GetProperties();

            foreach(var property in properties)
            {
                if (property.Name == "IsNotifying" || property.Name == "IsSelected")
                {
                    continue;
                }

                var expected = xml.AttributeSafe(property.Name).ToLower();
                var actual = property.GetValue(workflow).ToString().ToLower();
                Assert.AreEqual(expected, actual);
            }
        }

        #endregion

        #region ToXmlExpectedReturnsXml

        [TestMethod]
        public void ToXmlExpectedReturnsXml()
        {
            var workflow = new WorkflowDescriptor();
            var result = workflow.ToXml();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(XElement));
        }

        [TestMethod]
        public void ToXmlExpectedSerializesIDandName()
        {
            var workflow = new WorkflowDescriptor
            {
                ResourceID = Guid.NewGuid().ToString(),
                ResourceName = "Testing123"
            };
            var result = workflow.ToXml();

            var properties = workflow.GetType().GetProperties();
            foreach(var property in properties)
            {
                if (property.Name == "IsNotifying" || property.Name == "IsSelected")
                {
                    continue;  
                }
                var expected = property.GetValue(workflow).ToString().ToLower();
                var actual = result.AttributeSafe(property.Name).ToLower();
                Assert.AreEqual(expected, actual);
            }
        }

        #endregion
    }
}
