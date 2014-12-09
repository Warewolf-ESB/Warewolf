
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
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Runtime.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable CheckNamespace
namespace Dev2.Tests.Runtime.ServiceModel
// ReSharper restore CheckNamespace
{
    // BUG 9500 - 2013.05.31 - TWR : added proper testing
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PluginSourceTests
    {
        #region Save

        [TestMethod]
        public void SavePluginSourceWithExistingSourceExpectedServerWorkspaceUpdated()
        {
            //Initialize test resource, save then change path
            string uniquePathText = Guid.NewGuid().ToString()+"\\test plugin source";
            var testResource = new Resource { ResourceName = "test plugin source", ResourcePath = "initialpath", ResourceType = ResourceType.PluginSource, ResourceID = Guid.NewGuid() };
            new PluginSources().Save(testResource.ToString(), GlobalConstants.ServerWorkspaceID, Guid.Empty);
            testResource.ResourcePath = uniquePathText;

            //Execute save again on test resource
            new PluginSources().Save(testResource.ToString(), GlobalConstants.ServerWorkspaceID, Guid.Empty);

            //Assert resource saved
            var getSavedResource = Resources.ReadXml(GlobalConstants.ServerWorkspaceID, ResourceType.PluginSource, testResource.ResourceID.ToString());
            const string PathStartText = "<Category>";
            int start = getSavedResource.IndexOf(PathStartText, StringComparison.Ordinal);
            if(start > 0)
            {
                start += PathStartText.Length;
                int end = (getSavedResource.IndexOf("</Category>", start, StringComparison.Ordinal));
                var savedPath = getSavedResource.Substring(start, end - start);
                Assert.AreEqual(uniquePathText, savedPath);
            }
            else
            {
                Assert.Fail("Resource xml malformed after save");
            }
        }

        #endregion

        #region CTOR

        [TestMethod]
        public void PluginSourceContructorWithDefaultExpectedInitializesProperties()
        {
            var source = new PluginSource();
            Assert.AreEqual(Guid.Empty, source.ResourceID);
            Assert.AreEqual(ResourceType.PluginSource, source.ResourceType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PluginSourceContructorWithNullXmlExpectedThrowsArgumentNullException()
        {
            var source = new PluginSource(null);
        }

        [TestMethod]
        public void PluginSourceContructorWithInvalidXmlExpectedDoesNotThrowExceptionAndInitializesProperties()
        {
            var xml = new XElement("root");
            var source = new PluginSource(xml);
            Assert.AreNotEqual(Guid.Empty, source.ResourceID);
            Assert.IsTrue(source.IsUpgraded);
            Assert.AreEqual(ResourceType.PluginSource, source.ResourceType);
        }

        [TestMethod]
        public void PluginSourceContructorWithValidXmlExpectedInitializesProperties()
        {
            var xml = XmlResource.Fetch("PluginSource");

            var source = new PluginSource(xml);
            Assert.AreEqual(Guid.Parse("00746beb-46c1-48a8-9492-e2d20817fcd5"), source.ResourceID);
            Assert.AreEqual(ResourceType.PluginSource, source.ResourceType);
            Assert.AreEqual(@"C:\Development\DEV2 SCRUM Project\Branches\BUG_9500_PluginNamespaces\BPM Resources\Plugins\Dev2.PluginTester.dll", source.AssemblyLocation);
            Assert.AreEqual("Dev2.PluginTester", source.AssemblyName);
        }

        #endregion

        #region ToXml

        [TestMethod]
        public void PluginSourceToXmlExpectedSerializesProperties()
        {
            var expected = new PluginSource
            {
                AssemblyLocation = "Plugins\\someDllIMadeUpToTest.dll",
                AssemblyName = "dev2.test.namespacefortesting",
            };

            var xml = expected.ToXml();

            var actual = new PluginSource(xml);

            Assert.AreEqual(expected.ResourceType, actual.ResourceType);
            Assert.AreEqual(expected.AssemblyLocation, actual.AssemblyLocation);
            Assert.AreEqual(expected.AssemblyName, actual.AssemblyName);
        }

        [TestMethod]
        public void PluginSourceToXmlWithNullPropertiesExpectedSerializesPropertiesAsEmpty()
        {
            var expected = new PluginSource
            {
                AssemblyLocation = null,
                AssemblyName = null,
            };

            var xml = expected.ToXml();

            var actual = new PluginSource(xml);

            Assert.AreEqual(expected.ResourceType, actual.ResourceType);
            Assert.AreEqual("", actual.AssemblyLocation);
            Assert.AreEqual("", actual.AssemblyName);
        }

        #endregion
    }
}
