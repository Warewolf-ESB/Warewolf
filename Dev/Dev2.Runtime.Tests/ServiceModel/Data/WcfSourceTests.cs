using System;
using System.Collections.Generic;
using System.Reflection;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.Ouput;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    [TestClass]
    public class WcfSourceTests
    {
        public WcfSource GetSource()
        {
            return new WcfSource(new FakeWcfProxyService()) {Name = "WcfSource", EndpointUrl = "TestUrl"};
        }

        [TestMethod]
        public void WcfSource_InstantiateNewSource_ReturnsSuccess()
        {
            var source = GetSource();

            Assert.IsNotNull(source);
        }

        [TestMethod]
        public void WcfSource_InitializeProperties_ReturnsSuccess()
        {
            var source = GetSource();
            source.EndpointUrl = "TestUrl";
            source.ContactName = "Test.svc";
            source.Name = "WcfSource";
            source.Id = Guid.NewGuid();
            source.Path = "";
            source.Type = enSourceType.WcfSource;

            Assert.IsNotNull(source);
            Assert.IsNotNull(source.EndpointUrl);
            Assert.IsNotNull(source.ContactName);
            Assert.IsNotNull(source.Name);
            Assert.IsNotNull(source.Id);
            Assert.IsNotNull(source.Path);
            Assert.IsNotNull(source.Type);
        }

        [TestMethod]
        public void WcfSource_Equals_ReturnsSuccess()
        {
            var source = GetSource();
            
            Assert.IsTrue(source.Equals(new WcfSource()));
        }

        
        [TestMethod]
        public void WcfSource_ToXml_ReturnsSuccess()
        {
            var source = GetSource();
            var xml = source.ToXml();

            var actual = new WcfSource(xml);

            Assert.AreEqual(source.ResourceType, actual.ResourceType);
        }

        [TestMethod]
        public void WcfSource_Execute_ReturnsSuccess()
        {
            var source = GetSource();
           source.Execute();
        }

        [TestMethod]
        public void WcfSource_ExecuteWithService_ReturnsSuccess()
        {
            var source = GetSource();
            source.ExecuteMethod(new WcfService() {Source = source});

            Assert.IsNotNull(source);
        }

        [TestMethod]
        public void WcfSource_ExecuteWithAction_ReturnsSuccess()
        {
            var source = GetSource();
            source.ExecuteMethod(new WcfAction());

            Assert.IsNotNull(source);
        }

        [TestMethod]
        public void WcfSource_DataList_ReturnsSuccess()
        {
            var source = GetSource();
            source.DataList = "test";

            Assert.IsNotNull(source.DataList);
        }
    }

    public class FakeWcfProxyService : IWcfProxyService
    {
        public IOutputDescription ExecuteWebService(WcfService src)
        {
            return new OutputDescription();
        }

        public object ExcecuteMethod(IWcfAction action, string endpointUrl)
        {
            return new object();
        }

        public Dictionary<MethodInfo, ParameterInfo[]> GetMethods(string endpoint)
        {
            return new Dictionary<MethodInfo, ParameterInfo[]>();
        }
    }
}
