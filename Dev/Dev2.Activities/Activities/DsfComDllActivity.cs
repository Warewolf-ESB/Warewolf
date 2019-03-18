#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Factories;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Comparer;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers.ComPlugin;
using Unlimited.Framework.Converters.Graph;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("DotNetDll", "Com DLL", ToolType.Native, "6AEB1038-6332-46F9-8BDD-642DE4EA029E", "Dev2.Activities", "1.0.0.0", "Legacy", "Resources", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Resources_Com_DLL")]
    public class DsfComDllActivity : DsfMethodBasedActivity, ISimpePlugin
    {
        internal string _result;
        public IPluginAction Method { get; set; }
        public INamespaceItem Namespace { get; set; }
        public IOutputDescription OutputDescription { get; set; }
        private readonly IResponseManagerFactory _responseManagerFactory;
        public IResponseManager ResponseManager { get; set; }

        public DsfComDllActivity()
            : this(new ResponseManagerFactory())
        { }

        public DsfComDllActivity(IResponseManagerFactory responseManagerFactory)
        {
            Type = "Com DLL Connector";
            DisplayName = "Com DLL";
            _responseManagerFactory = responseManagerFactory;
        }
        
        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            tmpErrors = new ErrorResultTO();
            if (Method == null)
            {
                tmpErrors.AddError(ErrorResource.NoMethodSelected);
                return;
            }
            
            ExecuteService(update, out tmpErrors, Method, dataObject);
        }

        protected void ExecuteService(int update, out ErrorResultTO errors, IPluginAction method, IDSFDataObject dataObject)
        {
            errors = new ErrorResultTO();
            var itrs = new List<IWarewolfIterator>(5);
            IWarewolfListIterator itrCollection = new WarewolfListIterator();
            var source = ResourceCatalog.GetResource<ComPluginSource>(dataObject.WorkspaceID, SourceId);
            var methodParameters = Inputs?.Select(a => new MethodParameter { EmptyToNull = a.EmptyIsNull, IsRequired = a.RequiredField, Name = a.Name, Value = a.Value, TypeName = a.TypeName }).ToList() ?? new List<MethodParameter>();
            BuildParameterIterators(update, methodParameters.ToList(), itrCollection, itrs, dataObject);
            var args = new ComPluginInvokeArgs
            {
                ClsId = source.ClsId,
                Is32Bit = source.Is32Bit,
                Method = method.Method,
                AssemblyName = Namespace?.AssemblyName,
                Parameters = methodParameters
            };

            try
            {
                TryExecute(update, dataObject, itrs, itrCollection, methodParameters, args);
            }
            catch (Exception e)
            {
                errors.AddError(e.Message);
            }
        }

        private void TryExecute(int update, IDSFDataObject dataObject, List<IWarewolfIterator> itrs, IWarewolfListIterator itrCollection, List<MethodParameter> methodParameters, ComPluginInvokeArgs args)
        {
            if (Inputs == null || Inputs.Count == 0)
            {
                PerformExecution(update, dataObject, args);
            }
            else
            {
                while (itrCollection.HasMoreData())
                {
                    var pos = 0;
                    foreach (var itr in itrs)
                    {
                        var injectVal = itrCollection.FetchNextValue(itr);
                        var param = methodParameters.ToList()[pos];


                        param.Value = param.EmptyToNull &&
                                      (injectVal == null ||
                                       string.Compare(injectVal, string.Empty,
                                           StringComparison.InvariantCultureIgnoreCase) == 0)
                            ? null
                            : injectVal;

                        pos++;
                    }
                    PerformExecution(update, dataObject, args);
                }
            }
        }

        void PerformExecution(int update, IDSFDataObject dataObject, ComPluginInvokeArgs args)
        {
            if (!IsObject)
            {
                var i = 0;
                foreach (var serviceOutputMapping in Outputs)
                {
                    OutputDescription.DataSourceShapes[0].Paths[i].OutputExpression = DataListUtil.AddBracketsToValueIfNotExist(serviceOutputMapping.MappedTo);
                    i++;
                }
                var outputFormatter = OutputFormatterFactory.CreateOutputFormatter(OutputDescription);
                args.OutputFormatter = outputFormatter;
            }
            ExecuteInsideImpersonatedContext(args);

            ResponseManager = _responseManagerFactory.New(OutputDescription);
            ResponseManager.Outputs = Outputs;
            ResponseManager.IsObject = IsObject;
            ResponseManager.ObjectName = ObjectName;
            ResponseManager.PushResponseIntoEnvironment(_result, update, dataObject, false);
        }

        protected virtual void ExecuteInsideImpersonatedContext(ComPluginInvokeArgs args)
        {
            Common.Utilities.PerformActionInsideImpersonatedContext(Common.Utilities.ServerUser, () => { _result = ComPluginServiceExecutionFactory.InvokeComPlugin(args).ToString(); });
        }
        
        public override enFindMissingType GetFindMissingType() => enFindMissingType.DataGridActivity;

        public bool Equals(ISimpePlugin other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var comparer = new SimplePluginComparer();
            var equals = comparer.Equals(this, other);
            return base.Equals(other) && equals;
        }

        public override bool Equals(object obj)
        {
            if (obj is DsfComDllActivity dsfComDllActivity)
            {
               return Equals(dsfComDllActivity);
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (_result != null ? _result.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Method != null ? Method.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Namespace != null ? Namespace.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OutputDescription != null ? OutputDescription.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
