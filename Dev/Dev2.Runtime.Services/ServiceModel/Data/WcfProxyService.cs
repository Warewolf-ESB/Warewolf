#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System;
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

        MethodInfo GetMethod(string method, DynamicProxy.DynamicProxy proxy) => proxy.ProxyType.GetMethods().First(n => n.Name == method);

        object AdjustPluginResult(object result, MethodInfo methodToRun)
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
