/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.ServiceModel.Helper;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.ServiceModel
{
    [TestClass]
    public class ServiceUtilsTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceUtils_MappingNamesChanged")]
        public void ServiceUtils_MappingNamesChanged_OldMappingsIsNull_True()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var actual = ServiceUtils.MappingNamesChanged(null, new List<IDev2Definition>());

            //------------Assert Results-------------------------
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceUtils_MappingNamesChanged")]
        public void ServiceUtils_MappingNamesChanged_NewMappingsIsNull_True()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var actual = ServiceUtils.MappingNamesChanged(new List<IDev2Definition>(), null);

            //------------Assert Results-------------------------
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceUtils_MappingNamesChanged")]
        public void GivenXmlDoesNotContainDatalist_ServiceUtils_ExtractDataListShouldReturnEmptyString()
        {
            const string xmlDocument = "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>" +
                                   "<dotfuscator version=\"2.3\">\n" +
                                   "<excludelist>" + "<namespace name=\"Unlimited.Applications.BusinessDesignStudio.Activities\" />" + "<namespace name=\"Dev2.Studio.Core.AppResources.Behaviors\" />" + "<namespace name=\"Dev2.Studio.Core.AppResources.WindowManagers\" />" + "<namespace name=\"Dev2.Studio.ActivityDesigners\" />" + "<namespace name=\"Dev2.Studio.Views.Workflow\" />" + "<type name=\"Dev2.Activities.DsfExecuteCommandLineActivity\" />" + "<type name=\"Dev2.Activities.DsfForEachItem\" />" + "<type name=\"Dev2.Activities.DsfGatherSystemInformationActivity\" />" + "<type name=\"Dev2.Activities.DsfRandomActivity\" />" + "<type name=\"Dev2.DynamicServices.DsfDataObject\" excludetype=\"false\">" + "<method name=\"ExtractInMergeDataFromRequest\" signature=\"void(object)\" />" + "<method name=\"ExtractOutMergeDataFromRequest\" signature=\"void(object)\" />" + "</type>" + "<type name=\"Dev2.Runtime.Hosting.DynamicObjectHelper\" excludetype=\"false\">" + "<method name=\"SetID\" signature=\"void(Dev2.DynamicServices.IDynamicServiceObject, object)\" />" + "</type>" + "<type name=\"Dev2.CommandLineParameters\">" + "<method name=\"&lt;GetUsage&gt;b__0\" signature=\"void(CommandLine.Text.HelpText)\" />" + "<method name=\"GetUsage\" signature=\"string()\" />" + "<field name=\"&lt;Install&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;IntegrationTestMode&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;StartService&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;StopService&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;Uninstall&gt;k__BackingField\" signature=\"bool\" />" + "<propertymember name=\"Install\" />" + "<propertymember name=\"IntegrationTestMode\" />" + "<propertymember name=\"StartService\" />" + "<propertymember name=\"StopService\" />" + "<propertymember name=\"Uninstall\" />" + "</type>" + "<type name=\"Dev2.WebServer\" excludetype=\"false\">" + "<method name=\"CreateForm\" signature=\"Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)\" />" + "</type>" + "</excludelist>" +
                                   "</dotfuscator>";
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(xmlDocument);
            var actual = ServiceUtils.ExtractDataList(stringBuilder);
            //------------Assert Results-------------------------
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceUtils_MappingNamesChanged")]
        public void ServiceUtils_MappingNamesChanged_OldAndNewMappingsAreSame_False()
        {
            //------------Setup for test--------------------------
            var oldMappings = new List<IDev2Definition>
            {
                CreateNameMapping("Locales(*).LocationID"), 
                CreateNameMapping("Locales(*).Address"), 
                CreateNameMapping("Locales(*).Lat"), 
                CreateNameMapping("Locales(*).Long")
            };
            var newMappings = new List<IDev2Definition>
            {
                CreateNameMapping("Locales(*).LocationID"),
                CreateNameMapping("Locales(*).Address"),
                CreateNameMapping("Locales(*).Lat"),
                CreateNameMapping("Locales(*).Long")
            };

            //------------Execute Test---------------------------
            var actual = ServiceUtils.MappingNamesChanged(oldMappings, newMappings);

            //------------Assert Results-------------------------
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceUtils_MappingNamesChanged")]
        public void ServiceUtils_MappingNamesChanged_OldAndNewMappingsHaveDifferentCounts_True()
        {
            //------------Setup for test--------------------------
            var oldMappings = new List<IDev2Definition>
            {
                CreateNameMapping("Locales(*).LocationID"), 
                CreateNameMapping("Locales(*).Address"), 
                CreateNameMapping("Locales(*).Lat"), 
                CreateNameMapping("Locales(*).Long")
            };
            var newMappings = new List<IDev2Definition>
            {
                CreateNameMapping("Locales(*).LocationID"), 
                CreateNameMapping("Locales(*).Address"), 
                CreateNameMapping("Locales(*).Long")
            };

            //------------Execute Test---------------------------
            var actual = ServiceUtils.MappingNamesChanged(oldMappings, newMappings);

            //------------Assert Results-------------------------
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceUtils_MappingNamesChanged")]
        public void ServiceUtils_MappingNamesChanged_OldAndNewMappingsAreDifferent_True()
        {
            //------------Setup for test--------------------------
            var oldMappings = new List<IDev2Definition>
            {
                CreateNameMapping("Locales(*).LocationID"),
                CreateNameMapping("Locales(*).Address"),
                CreateNameMapping("Locales(*).Lat"),
                CreateNameMapping("Locales(*).Long")
            };
            var newMappings = new List<IDev2Definition>
            {
                CreateNameMapping("Locales(*).LocationID"),
                CreateNameMapping("Locales(*).Address"),
                CreateNameMapping("Locales(*).Latitude"),
                CreateNameMapping("Locales(*).Long")
            };

            //------------Execute Test---------------------------
            var actual = ServiceUtils.MappingNamesChanged(oldMappings, newMappings);

            //------------Assert Results-------------------------
            Assert.IsTrue(actual);
        }

        static IDev2Definition CreateNameMapping(string name)
        {
            return new Dev2Definition { Name = name };
        }
    }
}
