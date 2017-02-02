using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Data.Util;
using Dev2.Runtime.DynamicProxy;
using Dev2.Runtime.Interfaces;
using Unlimited.Framework.Converters.Graph;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class WcfProxyService : IWcfProxyService
    {
        public IOutputDescription ExecuteWebService(WcfService src)
        {
            var source = (WcfSource)src.Source;
            var factory = new DynamicProxyFactory(source.EndpointUrl);

            var contract = factory.Contracts.FirstOrDefault();

            if (contract == null)
            {
                throw new DynamicProxyException(ErrorResource.NoContractFound);
            }

            var proxy = factory.CreateProxy(contract.Name);

            var parameters = src.Method.Parameters?.Select(a => new MethodParameter { Name = a.Name, Value = a.Value, TypeName = a.TypeName })
                                 .ToList() ?? new List<MethodParameter>();
            var paramObjects =
                parameters.Select(methodParameter => Convert.ChangeType(methodParameter.Value, Type.GetType(methodParameter.TypeName))).ToArray();

            var result = proxy.CallMethod(src.Method.Name, paramObjects);
            var dataBrowser = DataBrowserFactory.CreateDataBrowser();
            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            var method = GetMethod(src.Method.Name, proxy);
            if (result != null)
            {
                result = AdjustPluginResult(result, method);

                var tmpData = dataBrowser.Map(result);
                dataSourceShape.Paths.AddRange(tmpData);
            }

            var output = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            output.DataSourceShapes.Add(dataSourceShape);
            return output;
        }

        public object ExcecuteMethod(IWcfAction action, string endpointUrl)
        {
            var factory = new DynamicProxyFactory(endpointUrl);

            var contract = factory.Contracts.FirstOrDefault();

            if (contract == null)
            {
                throw new DynamicProxyException(ErrorResource.NoContractFound);
            }

            var proxy = factory.CreateProxy(contract.Name);

            var parameters = action.Inputs?.Select(
                                 a =>
                                     new MethodParameter
                                     {
                                         EmptyToNull = a.EmptyIsNull,
                                         IsRequired = a.RequiredField,
                                         Name = a.Name,
                                         Value = a.Value,
                                         TypeName = a.TypeName
                                     }).ToList() ?? new List<MethodParameter>();
            var paramObjects =
                parameters.Select(methodParameter => Convert.ChangeType(methodParameter.Value, Type.GetType(methodParameter.TypeName))).ToArray();

            var result = proxy.CallMethod(action.Method, paramObjects);

            var method = GetMethod(action.Method, proxy);

            if (result != null)
            {
                result = AdjustPluginResult(result, method);
            }

            return result;
        }

        private MethodInfo GetMethod(string method, DynamicProxy.DynamicProxy proxy)
        {
            return proxy.ProxyType.GetMethods().First(n => n.Name == method);
        }

        private object AdjustPluginResult(object result, MethodInfo methodToRun)
        {

            // When it returns a primitive or string and it is not XML or JSON, make it so ;)
            if ((methodToRun.ReturnType.IsPrimitive || methodToRun.ReturnType.FullName == "System.String")
                && !DataListUtil.IsXml(result.ToString()) && !DataListUtil.IsJson(result.ToString()))
            {
                // add our special tags ;)
                result = string.Format("<{0}>{1}</{2}>", GlobalConstants.PrimitiveReturnValueTag, result, GlobalConstants.PrimitiveReturnValueTag);
            }

            return result;
        }

        public Dictionary<MethodInfo, ParameterInfo[]> GetMethods(string endpoint)
        {
            var factory = new DynamicProxyFactory(endpoint);
            var serviceModel = new ServiceModel { ParamsList = new Dictionary<MethodInfo, ParameterInfo[]>() };

            var contract = factory.Contracts.FirstOrDefault();

            if (contract == null)
            {
                throw new DynamicProxyException(ErrorResource.NoContractFound);
            }

            var proxy = factory.CreateProxy(contract.Name);

            var type = proxy.ProxyType;

            var methods = proxy.ProxyType.GetMethods();

            foreach (var methodInfo in methods.Where(methodInfo => methodInfo.DeclaringType == type))
            {
                serviceModel.MethodName = methodInfo.Name;

                var paramList = methodInfo.GetParameters();

                serviceModel.ParamsList.Add(methodInfo, paramList);
            }

            return serviceModel.ParamsList;
        }
    }
}
