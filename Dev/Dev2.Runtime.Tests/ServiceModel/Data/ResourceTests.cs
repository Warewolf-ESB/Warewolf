
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Data.ServiceModel;
using Dev2.Providers.Errors;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Runtime.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    // ReSharper disable InconsistentNaming
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ResourceTests
    {
        #region Equals

        [TestMethod]
        public void EqualsWithSameItemKeyExpectedReturnsTrue()
        {
            var key = new Resource();
            var result = key.Equals(key);
            Assert.IsTrue(result);
            result = key.Equals((object)key);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void EqualsWithNullExpectedReturnsFalse()
        {
            var key = new Resource();
            var result = key.Equals(null);
            Assert.IsFalse(result);
            result = key.Equals((object)null);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void EqualsWithDifferentItemKeyHavingSamePropertiesExpectedReturnsTrue()
        {
            var key = new Resource();
            var other = new Resource();
            var result = key.Equals(other);
            Assert.IsTrue(result);
            result = key.Equals((object)other);
            Assert.IsTrue(result);

        }

        [TestMethod]
        public void EqualsWithDifferentItemKeyHavingDifferentPropertiesExpectedReturnsFalse()
        {
            var key = new Resource();
            var other = new Resource { ResourceID = Guid.NewGuid() };
            var result = key.Equals(other);
            Assert.IsFalse(result);
            result = key.Equals((object)other);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void EqualsWithObjectExpectedReturnsFalse()
        {
            var key = new Resource();
            var result = key.Equals(new object());
            Assert.IsFalse(result);
        }
        #endregion

        [TestMethod]
        public void ConstructWhereValidXMLExpectResource()
        {
            //------------Setup for test--------------------------
            var validXML = GetValidXMLString();
            var textReader = new StringReader(validXML);
            XElement element = XElement.Load(textReader, LoadOptions.None);
            //------------Execute Test---------------------------
            var resource = new Resource(element);
            //------------Assert Results-------------------------
            Assert.IsNotNull(resource);
            Assert.AreEqual("1736ca6e-b870-467f-8d25-262972d8c3e8", resource.ResourceID.ToString());
            Assert.AreEqual("Bug6619", resource.ResourceName);
            Assert.AreEqual(ResourceType.WorkflowService, resource.ResourceType);
        }

        [TestMethod]
        public void ToXMLWhereValidResourceWIthErrorInfoDataIsValidFalse()
        {
            //------------Setup for test--------------------------
            var validXML = GetValidXMLString();
            var textReader = new StringReader(validXML);
            XElement element = XElement.Load(textReader, LoadOptions.None);
            var resource = new Resource(element);
            resource.Errors.Clear();
            resource.IsValid = false;
            resource.Errors.Add(new ErrorInfo { ErrorType = ErrorType.Critical, FixType = FixType.None, Message = "Fix Me", StackTrace = "Line 1" });
            //------------Execute Test---------------------------
            var xElement = resource.ToXml();
            //------------Assert Results-------------------------
            Assert.IsNotNull(xElement);
            var errorMessagesElement = xElement.Element("ErrorMessages");
            Assert.IsNotNull(errorMessagesElement);
            var errorMessageElement = errorMessagesElement.Element("ErrorMessage");
            Assert.IsNotNull(errorMessageElement);
            Assert.AreEqual("Fix Me", errorMessageElement.Attribute("Message").Value);
            Assert.AreEqual("Line 1", errorMessageElement.Attribute("StackTrace").Value);
            Assert.AreEqual("None", errorMessageElement.Attribute("FixType").Value);
            Assert.AreEqual("Critical", errorMessageElement.Attribute("ErrorType").Value);
        }

        [TestMethod]
        public void ConstructWhereValidResourceXMLWIthErrorInfoDataIsValidFalse()
        {
            //------------Setup for test--------------------------
            var validXML = GetValidXMLString();
            var textReader = new StringReader(validXML);
            XElement element = XElement.Load(textReader, LoadOptions.None);
            //------------Execute Test---------------------------
            var resource = new Resource(element);
            //------------Assert Results-------------------------
            Assert.IsNotNull(resource);
            Assert.AreEqual(3, resource.Errors.Count);
            Assert.AreEqual("Line 1", resource.Errors[0].StackTrace);
            Assert.AreEqual(ErrorType.None, resource.Errors[2].ErrorType);
            Assert.AreEqual("Error Message 2", resource.Errors[1].Message);
            Assert.AreEqual(FixType.None, resource.Errors[1].FixType);
        }

        [TestMethod]
        public void ConstructWhereValidXMLWithOneDependenciesExpectResourceWithOneItemInTree()
        {
            //------------Setup for test--------------------------
            var validXML = GetValidXMLString();
            var textReader = new StringReader(validXML);
            XElement element = XElement.Load(textReader, LoadOptions.None);
            //------------Execute Test---------------------------
            var resource = new Resource(element);
            //------------Assert Results-------------------------
            Assert.IsNotNull(resource);
            Assert.IsNotNull(resource.Dependencies);
            Assert.AreEqual(1, resource.Dependencies.Count());
            var resourceForTrees = resource.Dependencies.ToList();
            var resourceForTreeChild1 = resourceForTrees[0];
            Assert.AreEqual(new Guid("7bce06ec-778d-4a64-9dfe-1a826785f0b0"), resourceForTreeChild1.UniqueID);
        }

        [TestMethod]
        public void ConstructWhereValidXMLWithMultipleDependenciesExpectResourceWithGreaterThanOneItemInTree()
        {
            //------------Setup for test--------------------------
            var validXML = GetValidXMLStringWithMultipleDependencies();
            var textReader = new StringReader(validXML);
            XElement element = XElement.Load(textReader, LoadOptions.None);
            //------------Execute Test---------------------------
            var resource = new Resource(element);
            //------------Assert Results-------------------------
            Assert.IsNotNull(resource);
            Assert.IsNotNull(resource.Dependencies);
            Assert.AreEqual(2, resource.Dependencies.Count);
            var resourceForTrees = resource.Dependencies.ToList();
            var resourceForTreeChild1 = resourceForTrees[0];
            var resourceForTreeChild2 = resourceForTrees[1];
            Assert.AreEqual(new Guid("7bce06dc-778d-4b64-9dfe-1a826585f0b0"), resourceForTreeChild2.UniqueID);
            Assert.AreEqual(ResourceType.WorkflowService, resourceForTreeChild2.ResourceType);
            Assert.AreEqual(new Guid("7bce06ec-778d-4a64-9dfe-1a826785f0b0"), resourceForTreeChild1.UniqueID);
            Assert.AreEqual(ResourceType.WorkflowService, resourceForTreeChild1.ResourceType);
        }

        [TestMethod]
        public void ConstructWhereValidXMLWithMultipleServiceDependencyExpectResourceWithServiceDependency()
        {
            //------------Setup for test--------------------------
            var validXML = GetValidXMLStringWithMultipleDependenciesWithServiceDependencies();
            var textReader = new StringReader(validXML);
            XElement element = XElement.Load(textReader, LoadOptions.None);
            //------------Execute Test---------------------------
            var resource = new Resource(element);
            //------------Assert Results-------------------------
            Assert.IsNotNull(resource);
            Assert.IsNotNull(resource.Dependencies);
            Assert.AreEqual(3, resource.Dependencies.Count());
            var resourceForTrees = resource.Dependencies.ToList();
            var resourceForTreeService = resourceForTrees.Find(tree => tree.UniqueID == new Guid("48869a05-7121-4e45-970e-a40f6a2f8fd9"));
            Assert.IsNotNull(resourceForTreeService);
            Assert.AreEqual(ResourceType.PluginService, resourceForTreeService.ResourceType);
        }


        [TestMethod]
        public void ConstructWhereValidXMLFromResourceWithOneDependenciesExpectResourceWithOneItemInTree()
        {
            //------------Setup for test--------------------------
            var validXML = GetStringFromResource();
            var textReader = new StringReader(validXML);
            XElement element = XElement.Load(textReader, LoadOptions.None);
            //------------Execute Test---------------------------
            var resource = new Resource(element);
            //------------Assert Results-------------------------
            Assert.IsNotNull(resource);
            Assert.IsNotNull(resource.Dependencies);
            Assert.AreEqual(1, resource.Dependencies.Count());
            var resourceForTrees = resource.Dependencies.ToList();
            var resourceForTreeRoot = resourceForTrees[0];
            Assert.AreEqual(new Guid("7bce06ec-778d-4a64-9dfe-1a826785f0b0"), resourceForTreeRoot.UniqueID);
        }

        [TestMethod]
        public void Construct_UnitTest_LoadDependenciesWhereHasRemoteWorkflowDependency_ExpectedRemoteServerDependencyAdded()
        {
            //------------Setup for test--------------------------
            XElement element = XmlResource.Fetch("say hello remote");
            //------------Execute Test---------------------------
            var resource = new Resource(element);
            //------------Assert Results-------------------------
            Assert.IsNotNull(resource);
            Assert.IsNotNull(resource.Dependencies);
            Assert.AreEqual(4, resource.Dependencies.Count);
            IResourceForTree serverDependency = resource.Dependencies.First(tree => tree.ResourceID == Guid.Parse("889d3f22-40c5-4466-84bc-d49a5874ae53"));
            Assert.IsNotNull(serverDependency);
            Assert.AreEqual("server - tfs bld", serverDependency.ResourceName);
            Assert.AreEqual(ResourceType.Server, serverDependency.ResourceType);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Resource_LoadDependencies")]
        public void Resource_LoadDependencies_HasEmailSource_ShouldHaveEmailSourceInDepencyList()
        {
            //------------Setup for test--------------------------
            XElement element = XmlResource.Fetch("EmailTest");
            //------------Execute Test---------------------------
            var resource = new Resource(element);
            //------------Assert Results-------------------------
            Assert.IsNotNull(resource);
            Assert.IsNotNull(resource.Dependencies);
            Assert.AreEqual(1, resource.Dependencies.Count);
            Assert.AreEqual("TestEmailSource", resource.Dependencies[0].ResourceName);
            Assert.AreEqual("988e1146-ddb8-456d-8a01-4377a707605b", resource.Dependencies[0].ResourceID.ToString());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Resource_LoadDependencies")]
        public void Resource_LoadDependencies_HasDatabaseSourceFromSqlBulkInsertTool_ShouldHaveDatabaseSourceInDepencyList()
        {
            //------------Setup for test--------------------------
            XElement element = XmlResource.Fetch("Big Bulk Testing");
            //------------Execute Test---------------------------
            var resource = new Resource(element);
            //------------Assert Results-------------------------
            Assert.IsNotNull(resource);
            Assert.IsNotNull(resource.Dependencies);
            Assert.AreEqual(2, resource.Dependencies.Count);
            Assert.AreEqual("GenDev", resource.Dependencies[1].ResourceName);
            Assert.AreEqual("62505a00-b304-4ac0-a55c-50ce85111f16", resource.Dependencies[1].ResourceID.ToString());
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Resource_XmlConstructor")]
        public void Resource_XmlConstructor_Invoked_ShouldLoadDataList()
        {
            #region DataList String
            const string dataList = "<DataList>" +
                                    "<result Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" />" +
                                    "<Rows Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<res Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<start Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" />" +
                                    "<end Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" />" +
                                    "<InsertedData Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\">" +
                                    "<BigID Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column1 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column2 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column3 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column4 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column5 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column6 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column7 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column8 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column9 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column10 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column11 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column12 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column13 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column14 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column15 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column16 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column17 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column18 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column19 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column20 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column21 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column22 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column23 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column24 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column25 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column26 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column27 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column28 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column29 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column30 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column31 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column32 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column33 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column34 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column35 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column36 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column37 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column38 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column39 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column40 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column41 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column42 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column43 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column44 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column45 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column46 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column47 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column48 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column49 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column50 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column51 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column52 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column53 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column54 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column55 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column56 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column57 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column58 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column59 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column60 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column61 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column62 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column63 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column64 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column65 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column66 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column67 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column68 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column69 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column70 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column71 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column72 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column73 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column74 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column75 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column76 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column77 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column78 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column79 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column80 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column81 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column82 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column83 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column84 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column85 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column86 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column87 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column88 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column89 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column90 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column91 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column92 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column93 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column94 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column95 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column96 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column97 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column98 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column99 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column100 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "</InsertedData>" +
                                    "<Row Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\">" +
                                    "<BigID Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<Column100 Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "</Row></DataList>";
            #endregion
            //------------Setup for test--------------------------
            XElement element = XmlResource.Fetch("Big Bulk Testing");
            //------------Execute Test---------------------------
            var resource = new Resource(element);
            //------------Assert Results-------------------------
            Assert.IsFalse(String.IsNullOrEmpty(resource.DataList));
            var expected = resource.DataList.Replace(Environment.NewLine, "").Replace(" ", "");
            var actual = dataList.Replace(" ", "");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Resource_XmlConstructor")]
        public void Resource_XmlConstructor_Invoked_ServiceShouldHaveInputsAndOutputs()
        {
            var inputs = "<Inputs>" +
                                  "<Input Name=\"CityName\" Source=\"CityName\" EmptyToNull=\"false\" DefaultValue=\"Paris-Aeroport Charles De Gaulle\" />" +
                                  "<Input Name=\"CountryName\" Source=\"CountryName\" EmptyToNull=\"false\" DefaultValue=\"France\" />" +
                                  "</Inputs>";
            var outputs = "<Outputs>" +
                "<Output OriginalName=\"Location\" Name=\"Location\" MapsTo=\"Location\" Value=\"[[Location]]\" RecordsetName=\"\" RecordsetAlias=\"\" Recordset=\"\" /> " +
                "<Output OriginalName=\"Time\" Name=\"Time\" MapsTo=\"Time\" Value=\"[[Time]]\" RecordsetName=\"\" RecordsetAlias=\"\" Recordset=\"\" />                 " +
                "<Output OriginalName=\"Wind\" Name=\"Wind\" MapsTo=\"Wind\" Value=\"[[Wind]]\" RecordsetName=\"\" RecordsetAlias=\"\" Recordset=\"\" />                 " +
               "<Output OriginalName=\"Visibility\" Name=\"Visibility\" MapsTo=\"Visibility\" Value=\"[[Visibility]]\" RecordsetName=\"\" RecordsetAlias=\"\" Recordset=\"\" />" +
               "<Output OriginalName=\"Temperature\" Name=\"Temperature\" MapsTo=\"Temperature\" Value=\"[[Temperature]]\" RecordsetName=\"\" RecordsetAlias=\"\" Recordset=\"\" />" +
               "<Output OriginalName=\"DewPoint\" Name=\"DewPoint\" MapsTo=\"DewPoint\" Value=\"[[DewPoint]]\" RecordsetName=\"\" RecordsetAlias=\"\" Recordset=\"\" />" +
               "<Output OriginalName=\"RelativeHumidity\" Name=\"RelativeHumidity\" MapsTo=\"RelativeHumidity\" Value=\"[[RelativeHumidity]]\" RecordsetName=\"\" RecordsetAlias=\"\" Recordset=\"\" />" +
               "<Output OriginalName=\"Pressure\" Name=\"Pressure\" MapsTo=\"Pressure\" Value=\"[[Pressure]]\" RecordsetName=\"\" RecordsetAlias=\"\" Recordset=\"\" />" +
               "<Output OriginalName=\"Status\" Name=\"Status\" MapsTo=\"Status\" Value=\"[[Status]]\" RecordsetName=\"\" RecordsetAlias=\"\" Recordset=\"\" />" +
            "</Outputs>";
            //------------Setup for test--------------------------
            XElement element = XmlResource.Fetch("WebService");
            //------------Execute Test---------------------------
            var resource = new Resource(element);
            //------------Assert Results-------------------------
            Assert.IsFalse(String.IsNullOrEmpty(resource.DataList));
            Assert.AreEqual("<DataList />", resource.DataList);
            inputs = inputs.Replace(Environment.NewLine, "").Replace(" ", "");
            outputs = outputs.Replace(Environment.NewLine, "").Replace(" ", "");
            var actual = resource.Inputs.Replace(Environment.NewLine, "").Replace(" ", "");
            Assert.AreEqual(inputs, actual);
            actual = resource.Outputs.Replace(Environment.NewLine, "").Replace(" ", "");
            Assert.AreEqual(outputs, actual);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Resource_XmlConstructor")]
        public void Resource_XmlConstructor_Invoked_ServiceNoInputsShouldHaveInputsAndOutputs()
        {
            //------------Setup for test--------------------------
            XElement element = XmlResource.Fetch("WebServiceWithoutInputs");
            //------------Execute Test---------------------------
            var resource = new Resource(element);
            //------------Assert Results-------------------------
            Assert.IsFalse(String.IsNullOrEmpty(resource.DataList));
            Assert.AreEqual("<DataList />", resource.DataList);
            Assert.IsFalse(String.IsNullOrEmpty(resource.Inputs));
            Assert.IsFalse(String.IsNullOrEmpty(resource.Outputs));
        }

        string GetValidXMLString()
        {
            return "<Service Name=\"Bug6619\" ID=\"1736ca6e-b870-467f-8d25-262972d8c3e8\" ServerID=\"51a58300-7e9d-4927-a57b-e5d700b11b55\" IsValid=\"true\">" +
                "<Action Name=\"InvokeWorkflow\" Type=\"Workflow\">" +
                "<XamlDefinition>" +
                "<Activity mc:Ignorable=\"sads sap\" x:Class=\"Bug6619\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
                "xmlns:ddd=\"clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data\" xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"  " +
                "xmlns:mva=\"clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities\" xmlns:s=\"clr-namespace:System;assembly=mscorlib\"" +
                " xmlns:sads=\"http://schemas.microsoft.com/netfx/2010/xaml/activities/debugger\" xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\"  " +
                "xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\"  " +
                "xmlns:uf=\"clr-namespace:Unlimited.Framework;assembly=Dev2.Core\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">" +
                "<x:Members>" +
                "<x:Property Name=\"AmbientDataList\" Type=\"InOutArgument(scg:List(x:String))\" />" +
                "<x:Property Name=\"ParentWorkflowInstanceId\" Type=\"InOutArgument(s:Guid)\" />" +
                "<x:Property Name=\"ParentServiceName\" Type=\"InOutArgument(x:String)\" />" +
                "</x:Members>" +
                "<sap:VirtualizedContainerService.HintSize>654,676</sap:VirtualizedContainerService.HintSize>" +
                "<mva:VisualBasic.Settings>Assembly references and imported namespaces for internal implementation</mva:VisualBasic.Settings>" +
                "<Flowchart DisplayName=\"Bug6619\" sap:VirtualizedContainerService.HintSize=\"614,636\">" +
                "<Flowchart.Variables>" +
                "<Variable x:TypeArguments=\"scg:List(x:String)\" Name=\"InstructionList\" />" +
                "<Variable x:TypeArguments=\"x:String\" Name=\"LastResult\" />" +
                "<Variable x:TypeArguments=\"x:Boolean\" Name=\"HasError\" />" +
                "<Variable x:TypeArguments=\"x:String\" Name=\"ExplicitDataList\" />" +
                "<Variable x:TypeArguments=\"x:Boolean\" Name=\"IsValid\" />" +
                "<Variable x:TypeArguments=\"uf:UnlimitedObject\" Name=\"d\" />" +
                "<Variable x:TypeArguments=\"uaba:Util\" Name=\"t\" />" +
                "<Variable x:TypeArguments=\"ddd:Dev2DataListDecisionHandler\" Name=\"Dev2DecisionHandler\" />" +
                "</Flowchart.Variables>" +
                "<sap:WorkflowViewStateService.ViewState>" +
                "<scg:Dictionary x:TypeArguments=\"x:String, x:Object\">" +
                "<x:Boolean x:Key=\"IsExpanded\">False</x:Boolean>" +
                "<av:Point x:Key=\"ShapeLocation\">270,2.5</av:Point>" +
                "<av:Size x:Key=\"ShapeSize\">60,75</av:Size>" +
                "<av:PointCollection x:Key=\"ConnectorLocation\">300,77.5 300,174</av:PointCollection>" +
                "</scg:Dictionary>" +
                "</sap:WorkflowViewStateService.ViewState>" +
                "<Flowchart.StartNode>" +
                "<FlowStep x:Name=\"__ReferenceID0\">" +
                "<sap:WorkflowViewStateService.ViewState>" +
                "<scg:Dictionary x:TypeArguments=\"x:String, x:Object\">" +
                "<av:Point x:Key=\"ShapeLocation\">175,174</av:Point>" +
                "<av:Size x:Key=\"ShapeSize\">250,87</av:Size>" +
                "</scg:Dictionary>" +
                "</sap:WorkflowViewStateService.ViewState>" +
                "<uaba:DsfActivity ActionName=\"{x:Null}\" ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" " +
                "DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" " +
                "ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" " +
                "SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" Add=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"Bug6619Dep\" FriendlySourceName=\"localhost\" " +
                "HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,87\" IconPath=\"pack://application:,,,/Dev2.Studio;component/images/workflowservice2.png\" " +
                "InputMapping=\"\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"True\" OnResumeClearAmbientDataList=\"False\" " +
                "OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"\" RemoveInputFromOutput=\"False\" ServiceName=\"Bug6619Dep\" " +
                "SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"Bug6619Dep\" Type=\"Workflow\" UniqueID=\"7bce06ec-778d-4a64-9dfe-1a826785f0b0\">" +
                "<uaba:DsfActivity.AmbientDataList>" +
                "<InOutArgument x:TypeArguments=\"scg:List(x:String)\" />" +
                "</uaba:DsfActivity.AmbientDataList>" +
                "<uaba:DsfActivity.HelpLink>" +
                "<InArgument x:TypeArguments=\"x:String\">" +
                "<Literal x:TypeArguments=\"x:String\" Value=\"\" />" +
                "</InArgument>" +
                "</uaba:DsfActivity.HelpLink>" +
                "<uaba:DsfActivity.ParentInstanceID>" +
                "<InOutArgument x:TypeArguments=\"x:String\" />" +
                "</uaba:DsfActivity.ParentInstanceID>" +
                "<sap:WorkflowViewStateService.ViewState>" +
                "<scg:Dictionary x:TypeArguments=\"x:String, x:Object\">" +
                "<x:Boolean x:Key=\"IsExpanded\">True</x:Boolean>" +
                "</scg:Dictionary>" +
                "</sap:WorkflowViewStateService.ViewState>" +
                "</uaba:DsfActivity>" +
                "</FlowStep>" +
                "</Flowchart.StartNode>" +
                "<x:Reference>__ReferenceID0</x:Reference>" +
                "</Flowchart>" +
                "</Activity>" +
                "</XamlDefinition>" +
                "</Action>" +
                "<Comment/>" +
                "<Category>Bugs</Category>" +
                "<Tags/>" +
                "<IconPath>pack://application:,,,/Dev2.Studio;component/images/workflowservice2.png</IconPath>" +
                "<DisplayName>Workflow</DisplayName>" +
                "<DataList/>" +
                "<AuthorRoles />" +
                "<ErrorMessages>" +
                "<ErrorMessage Message=\"Error Message 1\" FixType=\"None\" ErrorType=\"Critical\" StackTrace=\"Line 1\" />" +
                "<ErrorMessage Message=\"Error Message 2\" FixType=\"None\" ErrorType=\"Warning\" StackTrace=\"Line 2\" />" +
                "<ErrorMessage Message=\"Error Message 3\" FixType=\"None\" ErrorType=\"None\" StackTrace=\"Line 3\" />" +
                "</ErrorMessages>" +
                "<UnitTestTargetWorkflowService />" +
                "<HelpLink />" +
                "<BizRule />" +
                "<WorkflowActivityDef />" +
                "<Source />" +
                "<Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\">" +
                "<SignedInfo>" +
                "<CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" />" +
                "<SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\" />" +
                "<Reference URI=\"\">" +
                "<Transforms>" +
                "<Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\" />" +
                "</Transforms>" +
                "<DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" />" +
                "<DigestValue>2VkOcM2OPhMQW6V2F1NcFQywprc=</DigestValue>" +
                "</Reference>" +
                "</SignedInfo>" +
                "<SignatureValue>Aw4KEyJkPEYNZq3kJ22My0kc8PWrbuV4l2d2OYebadrCOS3KcEar9kEJaNIqrbox9W8PYYKX77S56wbEX6UwXq8g9OaV9LTR99iQcuOGEIDzl59GKiGkIZ/9xZslDId6M1IYqXPtefEgMzAAx0GPTvpDQrQAEyizk7JDxrmRUXY=</SignatureValue>" +
                "</Signature>" +
                "</Service>";
        }

        string GetValidXMLStringWithMultipleDependencies()
        {
            return "<Service Name=\"Bug6619\" ID=\"1736ca6e-b870-467f-8d25-262972d8c3e8\" ServerID=\"51a58300-7e9d-4927-a57b-e5d700b11b55\">" +
                "<Action Name=\"InvokeWorkflow\" Type=\"Workflow\">" +
                "<XamlDefinition>" +
                "<Activity mc:Ignorable=\"sads sap\" x:Class=\"Bug6619\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
                "xmlns:ddd=\"clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data\" xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"  " +
                "xmlns:mva=\"clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities\" xmlns:s=\"clr-namespace:System;assembly=mscorlib\"" +
                " xmlns:sads=\"http://schemas.microsoft.com/netfx/2010/xaml/activities/debugger\" xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\"  " +
                "xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\"  " +
                "xmlns:uf=\"clr-namespace:Unlimited.Framework;assembly=Dev2.Core\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">" +
                "<x:Members>" +
                "<x:Property Name=\"AmbientDataList\" Type=\"InOutArgument(scg:List(x:String))\" />" +
                "<x:Property Name=\"ParentWorkflowInstanceId\" Type=\"InOutArgument(s:Guid)\" />" +
                "<x:Property Name=\"ParentServiceName\" Type=\"InOutArgument(x:String)\" />" +
                "</x:Members>" +
                "<sap:VirtualizedContainerService.HintSize>654,676</sap:VirtualizedContainerService.HintSize>" +
                "<mva:VisualBasic.Settings>Assembly references and imported namespaces for internal implementation</mva:VisualBasic.Settings>" +
                "<Flowchart DisplayName=\"Bug6619\" sap:VirtualizedContainerService.HintSize=\"614,636\">" +
                "<Flowchart.Variables>" +
                "<Variable x:TypeArguments=\"scg:List(x:String)\" Name=\"InstructionList\" />" +
                "<Variable x:TypeArguments=\"x:String\" Name=\"LastResult\" />" +
                "<Variable x:TypeArguments=\"x:Boolean\" Name=\"HasError\" />" +
                "<Variable x:TypeArguments=\"x:String\" Name=\"ExplicitDataList\" />" +
                "<Variable x:TypeArguments=\"x:Boolean\" Name=\"IsValid\" />" +
                "<Variable x:TypeArguments=\"uf:UnlimitedObject\" Name=\"d\" />" +
                "<Variable x:TypeArguments=\"uaba:Util\" Name=\"t\" />" +
                "<Variable x:TypeArguments=\"ddd:Dev2DataListDecisionHandler\" Name=\"Dev2DecisionHandler\" />" +
                "</Flowchart.Variables>" +
                "<sap:WorkflowViewStateService.ViewState>" +
                "<scg:Dictionary x:TypeArguments=\"x:String, x:Object\">" +
                "<x:Boolean x:Key=\"IsExpanded\">False</x:Boolean>" +
                "<av:Point x:Key=\"ShapeLocation\">270,2.5</av:Point>" +
                "<av:Size x:Key=\"ShapeSize\">60,75</av:Size>" +
                "<av:PointCollection x:Key=\"ConnectorLocation\">300,77.5 300,174</av:PointCollection>" +
                "</scg:Dictionary>" +
                "</sap:WorkflowViewStateService.ViewState>" +
                "<Flowchart.StartNode>" +
                "<FlowStep x:Name=\"__ReferenceID0\">" +
                "<sap:WorkflowViewStateService.ViewState>" +
                "<scg:Dictionary x:TypeArguments=\"x:String, x:Object\">" +
                "<av:Point x:Key=\"ShapeLocation\">175,174</av:Point>" +
                "<av:Size x:Key=\"ShapeSize\">250,87</av:Size>" +
                "</scg:Dictionary>" +
                "</sap:WorkflowViewStateService.ViewState>" +
                "<uaba:DsfActivity ActionName=\"{x:Null}\" ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" " +
                "DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" " +
                "ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" " +
                "SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" Add=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"Bug6619Dep\" FriendlySourceName=\"localhost\" " +
                "HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,87\" IconPath=\"pack://application:,,,/Dev2.Studio;component/images/workflowservice2.png\" " +
                "InputMapping=\"\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"True\" OnResumeClearAmbientDataList=\"False\" " +
                "OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"\" RemoveInputFromOutput=\"False\" ServiceName=\"Bug6619Dep\" " +
                "SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"Bug6619Dep\" Type=\"Workflow\" UniqueID=\"7bce06ec-778d-4a64-9dfe-1a826785f0b0\">" +
                "<uaba:DsfActivity.AmbientDataList>" +
                "<InOutArgument x:TypeArguments=\"scg:List(x:String)\" />" +
                "</uaba:DsfActivity.AmbientDataList>" +
                "<uaba:DsfActivity.HelpLink>" +
                "<InArgument x:TypeArguments=\"x:String\">" +
                "<Literal x:TypeArguments=\"x:String\" Value=\"\" />" +
                "</InArgument>" +
                "</uaba:DsfActivity.HelpLink>" +
                "<uaba:DsfActivity.ParentInstanceID>" +
                "<InOutArgument x:TypeArguments=\"x:String\" />" +
                "</uaba:DsfActivity.ParentInstanceID>" +
                "<sap:WorkflowViewStateService.ViewState>" +
                "<scg:Dictionary x:TypeArguments=\"x:String, x:Object\">" +
                "<x:Boolean x:Key=\"IsExpanded\">True</x:Boolean>" +
                "</scg:Dictionary>" +
                "</sap:WorkflowViewStateService.ViewState>" +
                "</uaba:DsfActivity>" +
                 "<uaba:DsfActivity ActionName=\"{x:Null}\" ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" " +
                "DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" " +
                "ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" " +
                "SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" Add=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"Bug6619Dep\" FriendlySourceName=\"localhost\" " +
                "HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,87\" IconPath=\"pack://application:,,,/Dev2.Studio;component/images/workflowservice2.png\" " +
                "InputMapping=\"\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"True\" OnResumeClearAmbientDataList=\"False\" " +
                "OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"\" RemoveInputFromOutput=\"False\" ServiceName=\"Bug6619Dep\" " +
                "SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"Bug6619Dep2\" Type=\"Workflow\" UniqueID=\"7bce06dc-778d-4b64-9dfe-1a826585f0b0\">" +
                "<uaba:DsfActivity.AmbientDataList>" +
                "<InOutArgument x:TypeArguments=\"scg:List(x:String)\" />" +
                "</uaba:DsfActivity.AmbientDataList>" +
                "<uaba:DsfActivity.HelpLink>" +
                "<InArgument x:TypeArguments=\"x:String\">" +
                "<Literal x:TypeArguments=\"x:String\" Value=\"\" />" +
                "</InArgument>" +
                "</uaba:DsfActivity.HelpLink>" +
                "<uaba:DsfActivity.ParentInstanceID>" +
                "<InOutArgument x:TypeArguments=\"x:String\" />" +
                "</uaba:DsfActivity.ParentInstanceID>" +
                "<sap:WorkflowViewStateService.ViewState>" +
                "<scg:Dictionary x:TypeArguments=\"x:String, x:Object\">" +
                "<x:Boolean x:Key=\"IsExpanded\">True</x:Boolean>" +
                "</scg:Dictionary>" +
                "</sap:WorkflowViewStateService.ViewState>" +
                "</uaba:DsfActivity>" +
                "</FlowStep>" +
                "</Flowchart.StartNode>" +
                "<x:Reference>__ReferenceID0</x:Reference>" +
                "</Flowchart>" +
                "</Activity>" +
                "</XamlDefinition>" +
                "</Action>" +
                "<Comment/>" +
                "<Category>Bugs</Category>" +
                "<Tags/>" +
                "<IconPath>pack://application:,,,/Dev2.Studio;component/images/workflowservice2.png</IconPath>" +
                "<DisplayName>Workflow</DisplayName>" +
                "<DataList/>" +
                "<AuthorRoles />" +
                "<UnitTestTargetWorkflowService />" +
                "<HelpLink />" +
                "<BizRule />" +
                "<WorkflowActivityDef />" +
                "<Source />" +
                "<Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\">" +
                "<SignedInfo>" +
                "<CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" />" +
                "<SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\" />" +
                "<Reference URI=\"\">" +
                "<Transforms>" +
                "<Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\" />" +
                "</Transforms>" +
                "<DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" />" +
                "<DigestValue>2VkOcM2OPhMQW6V2F1NcFQywprc=</DigestValue>" +
                "</Reference>" +
                "</SignedInfo>" +
                "<SignatureValue>Aw4KEyJkPEYNZq3kJ22My0kc8PWrbuV4l2d2OYebadrCOS3KcEar9kEJaNIqrbox9W8PYYKX77S56wbEX6UwXq8g9OaV9LTR99iQcuOGEIDzl59GKiGkIZ/9xZslDId6M1IYqXPtefEgMzAAx0GPTvpDQrQAEyizk7JDxrmRUXY=</SignatureValue>" +
                "</Signature>" +
                "</Service>";
        }

        string GetValidXMLStringWithMultipleDependenciesWithServiceDependencies()
        {
            return "<Service Name=\"Bug6619\" ID=\"1736ca6e-b870-467f-8d25-262972d8c3e8\" ServerID=\"51a58300-7e9d-4927-a57b-e5d700b11b55\">" +
                "<Action Name=\"InvokeWorkflow\" Type=\"Workflow\">" +
                "<XamlDefinition>" +
                "<Activity mc:Ignorable=\"sads sap\" x:Class=\"Bug6619\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
                "xmlns:ddd=\"clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data\" xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"  " +
                "xmlns:mva=\"clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities\" xmlns:s=\"clr-namespace:System;assembly=mscorlib\"" +
                " xmlns:sads=\"http://schemas.microsoft.com/netfx/2010/xaml/activities/debugger\" xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\"  " +
                "xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\"  " +
                "xmlns:uf=\"clr-namespace:Unlimited.Framework;assembly=Dev2.Core\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">" +
                "<x:Members>" +
                "<x:Property Name=\"AmbientDataList\" Type=\"InOutArgument(scg:List(x:String))\" />" +
                "<x:Property Name=\"ParentWorkflowInstanceId\" Type=\"InOutArgument(s:Guid)\" />" +
                "<x:Property Name=\"ParentServiceName\" Type=\"InOutArgument(x:String)\" />" +
                "</x:Members>" +
                "<sap:VirtualizedContainerService.HintSize>654,676</sap:VirtualizedContainerService.HintSize>" +
                "<mva:VisualBasic.Settings>Assembly references and imported namespaces for internal implementation</mva:VisualBasic.Settings>" +
                "<Flowchart DisplayName=\"Bug6619\" sap:VirtualizedContainerService.HintSize=\"614,636\">" +
                "<Flowchart.Variables>" +
                "<Variable x:TypeArguments=\"scg:List(x:String)\" Name=\"InstructionList\" />" +
                "<Variable x:TypeArguments=\"x:String\" Name=\"LastResult\" />" +
                "<Variable x:TypeArguments=\"x:Boolean\" Name=\"HasError\" />" +
                "<Variable x:TypeArguments=\"x:String\" Name=\"ExplicitDataList\" />" +
                "<Variable x:TypeArguments=\"x:Boolean\" Name=\"IsValid\" />" +
                "<Variable x:TypeArguments=\"uf:UnlimitedObject\" Name=\"d\" />" +
                "<Variable x:TypeArguments=\"uaba:Util\" Name=\"t\" />" +
                "<Variable x:TypeArguments=\"ddd:Dev2DataListDecisionHandler\" Name=\"Dev2DecisionHandler\" />" +
                "</Flowchart.Variables>" +
                "<sap:WorkflowViewStateService.ViewState>" +
                "<scg:Dictionary x:TypeArguments=\"x:String, x:Object\">" +
                "<x:Boolean x:Key=\"IsExpanded\">False</x:Boolean>" +
                "<av:Point x:Key=\"ShapeLocation\">270,2.5</av:Point>" +
                "<av:Size x:Key=\"ShapeSize\">60,75</av:Size>" +
                "<av:PointCollection x:Key=\"ConnectorLocation\">300,77.5 300,174</av:PointCollection>" +
                "</scg:Dictionary>" +
                "</sap:WorkflowViewStateService.ViewState>" +
                "<Flowchart.StartNode>" +
                "<FlowStep x:Name=\"__ReferenceID0\">" +
                "<sap:WorkflowViewStateService.ViewState>" +
                "<scg:Dictionary x:TypeArguments=\"x:String, x:Object\">" +
                "<av:Point x:Key=\"ShapeLocation\">175,174</av:Point>" +
                "<av:Size x:Key=\"ShapeSize\">250,87</av:Size>" +
                "</scg:Dictionary>" +
                "</sap:WorkflowViewStateService.ViewState>" +
                "<uaba:DsfActivity ActionName=\"{x:Null}\" ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" " +
                "DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" " +
                "ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" " +
                "SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" Add=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"Bug6619Dep\" FriendlySourceName=\"localhost\" " +
                "HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,87\" IconPath=\"pack://application:,,,/Dev2.Studio;component/images/workflowservice2.png\" " +
                "InputMapping=\"\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"True\" OnResumeClearAmbientDataList=\"False\" " +
                "OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"\" RemoveInputFromOutput=\"False\" ServiceName=\"Bug6619Dep\" " +
                "SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"Bug6619Dep\" Type=\"Workflow\" UniqueID=\"7bce06ec-778d-4a64-9dfe-1a826785f0b0\">" +
                "<uaba:DsfActivity.AmbientDataList>" +
                "<InOutArgument x:TypeArguments=\"scg:List(x:String)\" />" +
                "</uaba:DsfActivity.AmbientDataList>" +
                "<uaba:DsfActivity.HelpLink>" +
                "<InArgument x:TypeArguments=\"x:String\">" +
                "<Literal x:TypeArguments=\"x:String\" Value=\"\" />" +
                "</InArgument>" +
                "</uaba:DsfActivity.HelpLink>" +
                "<uaba:DsfActivity.ParentInstanceID>" +
                "<InOutArgument x:TypeArguments=\"x:String\" />" +
                "</uaba:DsfActivity.ParentInstanceID>" +
                "<sap:WorkflowViewStateService.ViewState>" +
                "<scg:Dictionary x:TypeArguments=\"x:String, x:Object\">" +
                "<x:Boolean x:Key=\"IsExpanded\">True</x:Boolean>" +
                "</scg:Dictionary>" +
                "</sap:WorkflowViewStateService.ViewState>" +
                "</uaba:DsfActivity>" +
                 "<uaba:DsfActivity ActionName=\"{x:Null}\" ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" " +
                "DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" " +
                "ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" " +
                "SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" Add=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"Bug6619Dep\" FriendlySourceName=\"localhost\" " +
                "HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,87\" IconPath=\"pack://application:,,,/Dev2.Studio;component/images/workflowservice2.png\" " +
                "InputMapping=\"\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"True\" OnResumeClearAmbientDataList=\"False\" " +
                "OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"\" RemoveInputFromOutput=\"False\" ServiceName=\"Bug6619Dep\" " +
                "SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"Bug6619Dep2\" Type=\"Workflow\" UniqueID=\"7bce06dc-778d-4b64-9dfe-1a826585f0b0\">" +
                "<uaba:DsfActivity.AmbientDataList>" +
                "<InOutArgument x:TypeArguments=\"scg:List(x:String)\" />" +
                "</uaba:DsfActivity.AmbientDataList>" +
                "<uaba:DsfActivity.HelpLink>" +
                "<InArgument x:TypeArguments=\"x:String\">" +
                "<Literal x:TypeArguments=\"x:String\" Value=\"\" />" +
                "</InArgument>" +
                "</uaba:DsfActivity.HelpLink>" +
                "<uaba:DsfActivity.ParentInstanceID>" +
                "<InOutArgument x:TypeArguments=\"x:String\" />" +
                "</uaba:DsfActivity.ParentInstanceID>" +
                "<sap:WorkflowViewStateService.ViewState>" +
                "<scg:Dictionary x:TypeArguments=\"x:String, x:Object\">" +
                "<x:Boolean x:Key=\"IsExpanded\">True</x:Boolean>" +
                "</scg:Dictionary>" +
                "</sap:WorkflowViewStateService.ViewState>" +
                "</uaba:DsfActivity>" +
               " <uaba:DsfActivity ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" DataTags=\"{x:Null}\" " +
                "ExplicitDataList=\"{x:Null}\" HelpLink=\"{x:Null}\" IconPath=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" " +
                "ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" " +
                "ScopingObject=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" ActionName=\"Connect\" Add=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" " +
                "DisplayName=\"DEV2Plugin_SQLServer_Connect\" FriendlySourceName=\"DEV2Plugin_SQLServer\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,104\" " +
                "InputMapping=\"&lt;Inputs&gt;&lt;Input Name=&quot;XML&quot; Source=&quot;XML&quot;&gt;&lt;Validator Type=&quot;Required&quot; /&gt;&lt;/Input&gt;&lt;/Inputs&gt;\" " +
                "InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"False\" OnResumeClearAmbientDataList=\"False\" " +
                "OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"&lt;Outputs&gt;&lt;Output Name=&quot;Error&quot; MapsTo=&quot;Error&quot; Value=&quot;[[Error]]&quot; /&gt;&lt;Output Name=&quot;Connect&quot; MapsTo=&quot;Connect&quot; Value=&quot;[[Connect]]&quot; /&gt;&lt;/Outputs&gt;\" " +
                "RemoveInputFromOutput=\"False\" ServiceName=\"DEV2Plugin_SQLServer_Connect\" SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"DEV2Plugin_SQLServer_Connect\" Type=\"Plugin\" " +
                "UniqueID=\"48869a05-7121-4e45-970e-a40f6a2f8fd9\">" +
                "<uaba:DsfActivity.AmbientDataList>" +
                "<InOutArgument x:TypeArguments=\"scg:List(x:String)\" />" +
                "</uaba:DsfActivity.AmbientDataList>" +
                "<uaba:DsfActivity.ParentInstanceID>" +
                "<InOutArgument x:TypeArguments=\"x:String\" />" +
                "</uaba:DsfActivity.ParentInstanceID>" +
                "<sap:WorkflowViewStateService.ViewState>" +
                "<scg:Dictionary x:TypeArguments=\"x:String, x:Object\">" +
                "<x:Boolean x:Key=\"IsExpanded\">True</x:Boolean>" +
                "</scg:Dictionary>" +
                "</sap:WorkflowViewStateService.ViewState>" +
                "</uaba:DsfActivity>" +
                "</FlowStep>" +
                    "</Flowchart.StartNode>" +
                "<x:Reference>__ReferenceID0</x:Reference>" +
                "</Flowchart>" +
                "</Activity>" +
                "</XamlDefinition>" +
                "</Action>" +
                "<Comment/>" +
                "<Category>Bugs</Category>" +
                "<Tags/>" +
                "<IconPath>pack://application:,,,/Dev2.Studio;component/images/workflowservice2.png</IconPath>" +
                "<DisplayName>Workflow</DisplayName>" +
                "<DataList/>" +
                "<AuthorRoles />" +
                "<UnitTestTargetWorkflowService />" +
                "<HelpLink />" +
                "<BizRule />" +
                "<WorkflowActivityDef />" +
                "<Source />" +
                "<Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\">" +
                "<SignedInfo>" +
                "<CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" />" +
                "<SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\" />" +
                "<Reference URI=\"\">" +
                "<Transforms>" +
                "<Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\" />" +
                "</Transforms>" +
                "<DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" />" +
                "<DigestValue>2VkOcM2OPhMQW6V2F1NcFQywprc=</DigestValue>" +
                "</Reference>" +
                "</SignedInfo>" +
                "<SignatureValue>Aw4KEyJkPEYNZq3kJ22My0kc8PWrbuV4l2d2OYebadrCOS3KcEar9kEJaNIqrbox9W8PYYKX77S56wbEX6UwXq8g9OaV9LTR99iQcuOGEIDzl59GKiGkIZ/9xZslDId6M1IYqXPtefEgMzAAx0GPTvpDQrQAEyizk7JDxrmRUXY=</SignatureValue>" +
                "</Signature>" +
                "</Service>";
        }

        string GetStringFromResource()
        {
            return "<Service Name=\"Bug6619\" ID=\"1736ca6e-b870-467f-8d25-262972d8c3e8\" ServerID=\"51a58300-7e9d-4927-a57b-e5d700b11b55\">" +
                "<Action Name=\"InvokeWorkflow\" Type=\"Workflow\">" +
                "<XamlDefinition>&lt;Activity mc:Ignorable=\"sads sap\" x:Class=\"Bug6619\"" +
                " xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\"" +
                " xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"" +
                " xmlns:ddd=\"clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data\"" +
                " xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"" +
                " xmlns:mva=\"clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities\"" +
                " xmlns:s=\"clr-namespace:System;assembly=mscorlib\"" +
                " xmlns:sads=\"http://schemas.microsoft.com/netfx/2010/xaml/activities/debugger\"" +
                " xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\"" +
                " xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\"" +
                " xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\"" +
                " xmlns:uf=\"clr-namespace:Unlimited.Framework;assembly=Dev2.Core\"" +
                " xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"&gt;" +
                "&lt;x:Members&gt;" +
                "&lt;x:Property Name=\"AmbientDataList\" Type=\"InOutArgument(scg:List(x:String))\" /&gt;" +
                "&lt;x:Property Name=\"ParentWorkflowInstanceId\" Type=\"InOutArgument(s:Guid)\" /&gt;" +
                "&lt;x:Property Name=\"ParentServiceName\" Type=\"InOutArgument(x:String)\" /&gt;" +
                "&lt;/x:Members&gt;" +
                "&lt;sap:VirtualizedContainerService.HintSize&gt;654,676&lt;/sap:VirtualizedContainerService.HintSize&gt;" +
                "&lt;mva:VisualBasic.Settings&gt;Assembly references and imported namespaces for internal implementation&lt;/mva:VisualBasic.Settings&gt;" +
                "&lt;Flowchart DisplayName=\"Bug6619\" sap:VirtualizedContainerService.HintSize=\"614,636\"&gt;" +
                "&lt;Flowchart.Variables&gt;" +
                "&lt;Variable x:TypeArguments=\"scg:List(x:String)\" Name=\"InstructionList\" /&gt;" +
                "&lt;Variable x:TypeArguments=\"x:String\" Name=\"LastResult\" /&gt;" +
                "&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"HasError\" /&gt;" +
                "&lt;Variable x:TypeArguments=\"x:String\" Name=\"ExplicitDataList\" /&gt;" +
                "&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"IsValid\" /&gt;" +
                "&lt;Variable x:TypeArguments=\"uf:UnlimitedObject\" Name=\"d\" /&gt;" +
                "&lt;Variable x:TypeArguments=\"uaba:Util\" Name=\"t\" /&gt;" +
                "&lt;Variable x:TypeArguments=\"ddd:Dev2DataListDecisionHandler\" Name=\"Dev2DecisionHandler\" /&gt;" +
                "&lt;/Flowchart.Variables&gt;" +
                "&lt;sap:WorkflowViewStateService.ViewState&gt;" +
                "&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;" +
                "&lt;x:Boolean x:Key=\"IsExpanded\"&gt;False&lt;/x:Boolean&gt;" +
                "&lt;av:Point x:Key=\"ShapeLocation\"&gt;270,2.5&lt;/av:Point&gt;" +
                "&lt;av:Size x:Key=\"ShapeSize\"&gt;60,75&lt;/av:Size&gt;" +
                "&lt;av:PointCollection x:Key=\"ConnectorLocation\"&gt;300,77.5 300,174&lt;/av:PointCollection&gt;" +
                "&lt;/scg:Dictionary&gt;" +
                "&lt;/sap:WorkflowViewStateService.ViewState&gt;" +
                "&lt;Flowchart.StartNode&gt;" +
                "&lt;FlowStep x:Name=\"__ReferenceID0\"&gt;" +
                "&lt;sap:WorkflowViewStateService.ViewState&gt;" +
                "&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;" +
                "&lt;av:Point x:Key=\"ShapeLocation\"&gt;175,174&lt;/av:Point&gt;" +
                "&lt;av:Size x:Key=\"ShapeSize\"&gt;250,87&lt;/av:Size&gt;" +
                "&lt;/scg:Dictionary&gt;" +
                "&lt;/sap:WorkflowViewStateService.ViewState&gt;" +
                "&lt;uaba:DsfActivity ActionName=\"{x:Null}\" ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" " +
                "DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\"" +
                " ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\"" +
                " SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" Add=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"Bug6619Dep\" FriendlySourceName=\"localhost\" HasError=\"[HasError]\"" +
                " sap:VirtualizedContainerService.HintSize=\"250,87\" IconPath=\"pack://application:,,,/Dev2.Studio;component/images/workflowservice2.png\" " +
                " InputMapping=\"\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"True\" OnResumeClearAmbientDataList=\"False\" " +
                " OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"\" RemoveInputFromOutput=\"False\" " +
                " ServiceName=\"Bug6619Dep\" SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"Bug6619Dep\" Type=\"Workflow\" UniqueID=\"7bce06ec-778d-4a64-9dfe-1a826785f0b0\"&gt;" +
                "&lt;uaba:DsfActivity.AmbientDataList&gt;" +
                "&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;" +
                "&lt;/uaba:DsfActivity.AmbientDataList&gt;" +
                "&lt;uaba:DsfActivity.HelpLink&gt;" +
                "&lt;InArgument x:TypeArguments=\"x:String\"&gt;" +
                "&lt;Literal x:TypeArguments=\"x:String\" Value=\"\" /&gt;" +
                "&lt;/InArgument&gt;" +
                "&lt;/uaba:DsfActivity.HelpLink&gt;" +
                "&lt;uaba:DsfActivity.ParentInstanceID&gt;" +
                "&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;" +
                "&lt;/uaba:DsfActivity.ParentInstanceID&gt;" +
                "&lt;sap:WorkflowViewStateService.ViewState&gt;" +
                "&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;" +
                "&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;" +
                "&lt;/scg:Dictionary&gt;" +
                "&lt;/sap:WorkflowViewStateService.ViewState&gt;" +
                "&lt;/uaba:DsfActivity&gt;" +
                "&lt;/FlowStep&gt;" +
                "&lt;/Flowchart.StartNode&gt;" +
                "&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;" +
                "&lt;/Flowchart&gt;" +
                "&lt;/Activity&gt;</XamlDefinition>" +
                "</Action><Comment></Comment><Category>Bugs</Category><Tags></Tags><IconPath>pack://application:,,,/Dev2.Studio;component/images/workflowservice2.png</IconPath>" +
                "<DisplayName>Workflow</DisplayName><DataList></DataList><AuthorRoles /><UnitTestTargetWorkflowService /><HelpLink /><BizRule /><WorkflowActivityDef /><Source />" +
                "<Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><SignedInfo><CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" />" +
                "<SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\" /><Reference URI=\"\"><Transforms><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\" />" +
                "</Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>2VkOcM2OPhMQW6V2F1NcFQywprc=</DigestValue></Reference></SignedInfo>" +
                "<SignatureValue>Aw4KEyJkPEYNZq3kJ22My0kc8PWrbuV4l2d2OYebadrCOS3KcEar9kEJaNIqrbox9W8PYYKX77S56wbEX6UwXq8g9OaV9LTR99iQcuOGEIDzl59GKiGkIZ/9xZslDId6M1IYqXPtefEgMzAAx0GPTvpDQrQAEyizk7JDxrmRUXY=</SignatureValue>" +
                "</Signature></Service>";
        }

        #region ParseProperties

        // PBI 5656 - 2013.05.20 - TWR - Created
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourceParsePropertiesWithNullStringExpectedThrowsArgumentNullException()
        {
            Resource.ParseProperties(null, null);
        }

        // PBI 5656 - 2013.05.20 - TWR - Created
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourceParsePropertiesWithNullPropertiesExpectedThrowsArgumentNullException()
        {
            Resource.ParseProperties("", null);
        }

        // PBI 5656 - 2013.05.20 - TWR - Created
        [TestMethod]
        public void ResourceParsePropertiesWithInvalidPropertiesExpectedSafelyIgnoresInvalidProperties()
        {
            const string TestStr = "address=http://www.webservicex.net/globalweather.asmx/GetCitiesByCountry?CountryName=South%20Africa;AuthenticationType=Anonymous";
            var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Address", null },
                { "UserName", null }
            };
            Resource.ParseProperties(TestStr, properties);
            Assert.IsNotNull(properties["Address"]);
            Assert.IsNull(properties["UserName"]);
        }

        // PBI 5656 - 2013.05.20 - TWR - Created
        [TestMethod]
        public void ResourceParsePropertiesWithValidPropertiesExpectedParsesProperties()
        {
            const string Address = "http://www.webservicex.net/globalweather.asmx/GetCitiesByCountry?CountryName=South%20Africa";
            const string AuthenticationType = "User";
            const string UserName = "wert=9^4=&";
            var testStr = string.Format("address={0};AuthenticationType={1};UserName={2}", Address, AuthenticationType, UserName);

            var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Address", null },
                { "AuthenticationType", null },
                { "UserName", null }
            };

            Resource.ParseProperties(testStr, properties);

            Assert.AreEqual(Address, properties["Address"]);
            Assert.AreEqual(AuthenticationType, properties["AuthenticationType"]);
            Assert.AreEqual(UserName, properties["UserName"]);
        }

        #endregion


    }
}
