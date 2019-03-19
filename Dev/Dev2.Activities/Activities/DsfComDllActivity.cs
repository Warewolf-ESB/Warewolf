#pragma warning disable
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
