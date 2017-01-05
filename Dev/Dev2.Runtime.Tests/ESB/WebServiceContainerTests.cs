/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Dev2.Data.TO;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Tests.Runtime.XML;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.ESB
{
    [TestClass]
    public class WebServiceContainerTests
    {
        static readonly XElement WebSourceWithInputsXml = XmlResource.Fetch("WebSource");
        static readonly XElement WebServiceWithInputsXml = XmlResource.Fetch("WebService");
        static readonly XElement WebServiceWithInputsResponseXml = XmlResource.Fetch("WebServiceResponse");

        #region HandlesOutputFormatting
        [TestMethod]
        public void WebServiceContainerServiceInputsExpectedServiceActionWithInputs()
        {
            //------------------------------------Setup -------------------------------------------------------------------------
            var sa = CreateServiceAction(WebServiceWithInputsXml, WebSourceWithInputsXml);
            //------------------------------------Execute-----------------------------------------------------------------------
            List<ServiceActionInput> serviceActionInputs = sa.ServiceActionInputs;
            //------------------------------------Assert------------------------------------------------------------------------
            Assert.AreEqual(2, serviceActionInputs.Count);
            Assert.AreEqual("CityName", serviceActionInputs[0].Source);
            Assert.AreEqual("CountryName", serviceActionInputs[1].Source);
        }

        #endregion

        #region Execute

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WebserviceExecutionContainer_Execute")]
        public void WebserviceExecutionContainer_Execute_WhenErrors_ExpectValidErrors()
        {
            //------------Setup for test--------------------------
            var container = CreateWebServiceContainer(WebServiceWithInputsXml, WebSourceWithInputsXml, WebServiceWithInputsResponseXml.ToString(), true);

            ErrorResultTO errors;

            //------------Execute Test---------------------------
            container.Execute(out errors, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(5, errors.FetchErrors().Count);
            Assert.AreEqual(" [[CityName]] does not exist in your Data List", errors.FetchErrors()[0]);
            Assert.AreEqual("Faulty Things Happened", errors.FetchErrors()[4]);

        }

        //[TestMethod]
        //public void WebServiceContainerExecuteWithValidServiceHavingInputsExpectedExecutesService()
        //{
        //    var container = CreateWebServiceContainer(WebServiceWithInputsXml, WebSourceWithInputsXml, WebServiceWithInputsResponseXml.ToString());

        //    ErrorResultTO errors;
        //    var dataListID = container.Execute(out errors, 0);
        //    var compiler = DataListFactory.CreateDataListCompiler();

        //    var result = compiler.ConvertFrom(dataListID, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);

        //    Assert.IsNotNull(result);

        //    var resultXml = XElement.Parse(result.ToString());

        //    var expectedRoot = (XElement)WebServiceWithInputsResponseXml.FirstNode;
        //    foreach(var actualNode in resultXml.Elements())
        //    {
        //        var actualName = actualNode.Name.LocalName;
        //        if(!actualName.StartsWith("Dev2System"))
        //        {
        //            var expectedNode = expectedRoot.Element(actualName);
        //            if(expectedNode != null)
        //            {
        //                Assert.AreEqual(expectedNode.Value, actualNode.Value);
        //            }
        //        }
        //    }
        //}

        //[TestMethod]
        //public void WebServiceContainerExecuteWithValidServiceHavingNoInputsExpectedExecutesService()
        //{
        //    var container = CreateWebServiceContainer(WebServiceWithoutInputsXml, WebSourceWithoutInputsXml, WebServiceWithoutInputsResponse);

        //    ErrorResultTO errors;
        //    var dataListId = container.Execute(out errors, 0);
        //    var compiler = DataListFactory.CreateDataListCompiler();

        //    var result = compiler.ConvertFrom(dataListId, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);

        //    Assert.IsNotNull(result);

        //    var resultXml = XElement.Parse(result.ToString());

        //    var expectedRoot = WebServiceWithoutInputsResponseXml;
        //    foreach(var actualNode in resultXml.Elements())
        //    {
        //        var actualName = actualNode.Name.LocalName;
        //        if(!actualName.StartsWith("Dev2System"))
        //        {
        //            var expectedNode = expectedRoot.Element(actualName);
        //            if(expectedNode != null)
        //            {
        //                Assert.AreEqual(expectedNode.Value, actualNode.Value);
        //            }
        //        }
        //    }
        //}

        #endregion

        #region CreateWebServiceContainer

        static WebServiceContainer CreateWebServiceContainer(XElement serviceXml, XElement sourceXml, string response, bool isFaulty = false)
        {

            var dataObj = new Mock<IDSFDataObject>();
            dataObj.Setup(d => d.DataListID).Returns(new Guid());

            var workspace = new Mock<IWorkspace>();
            var esbChannel = new Mock<IEsbChannel>();

            var sa = CreateServiceAction(serviceXml, sourceXml);
            WebServiceContainer container;

            if(!isFaulty)
            {

                container = new WebServiceContainerMock(sa, dataObj.Object, workspace.Object, esbChannel.Object)
                {
                    WebRequestRespsonse = response
                };
            }
            else
            {
                container = new FaultyWebServiceContainerMock(sa, dataObj.Object, workspace.Object, esbChannel.Object)
                {
                    WebRequestRespsonse = string.Empty
                };
            }

            return container;
        }
        #endregion

        #region CreateServiceAction

        static ServiceAction CreateServiceAction(XElement serviceXml, XElement sourceXml)
        {
            var graph = new ServiceDefinitionLoader().GenerateServiceGraph(new StringBuilder(serviceXml.ToString()));

            var ds = (DynamicService)graph[0];
            var sa = ds.Actions[0];
            sa.Source = new Source { ResourceDefinition = new StringBuilder(sourceXml.ToString()) };
            return sa;
        }

        #endregion

    }

    internal class FaultyWebServiceContainerMock : WebServiceContainerMock
    {
        public FaultyWebServiceContainerMock(ServiceAction sa, IDSFDataObject dsfDataObject, IWorkspace workspace, IEsbChannel esbChannel)
            : base(sa, dsfDataObject, workspace, esbChannel)
        {
        }

        #region Overrides of WebServiceContainerMock

        public override Guid Execute(out ErrorResultTO errors, int update)
        {
            errors = new ErrorResultTO();
            errors.AddError(" [[CityName]] does not exist in your Data List");
            errors.AddError("Error");
            errors.AddError("Error 2");
            errors.AddError("Error 3");
            errors.AddError("Faulty Things Happened");
            return DataObject.DataListID;
        }

        #endregion
    }
}
