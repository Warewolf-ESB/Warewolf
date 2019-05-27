#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Comparer;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin;
using Unlimited.Framework.Converters.Graph;
using Warewolf.Resource.Errors;
using Warewolf.Storage;


namespace Dev2.Activities
{
    //[ToolDescriptorInfo("DotNetDll", "DotNet DLL", ToolType.Native, "6AEB1038-6332-46F9-8BDD-641DE4EA038E", "Dev2.Activities", "1.0.0.0", "Legacy", "Resources", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Resources_Dot_net_DLL")]
    public class DsfDotNetDllActivity : DsfMethodBasedActivity, ISimpePlugin
    {
#pragma warning disable S3776,S1541,S134,CC0075,S1066,S1067

        public IPluginAction Method { get; set; }
        public INamespaceItem Namespace { get; set; }

        public IOutputDescription OutputDescription { get; set; }

        public DsfDotNetDllActivity()
        {
            Type = "DotNet DLL Connector";
            DisplayName = "DotNet DLL";
        }
        
        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            tmpErrors = new ErrorResultTO();
            if (Namespace == null)
            {
                tmpErrors.AddError(ErrorResource.NoNamespaceSelected);
                return;
            }
            if (Method == null)
            {
                tmpErrors.AddError(ErrorResource.NoMethodSelected);
                return;
            }
            
            ExecuteService(update, out tmpErrors, Method, Namespace, dataObject);
        }

        protected void ExecuteService(int update, out ErrorResultTO errors, IPluginAction method, INamespaceItem namespaceItem, IDSFDataObject dataObject)
        {
            errors = new ErrorResultTO();
            var itrs = new List<IWarewolfIterator>(5);
            IWarewolfListIterator itrCollection = new WarewolfListIterator();
            var methodParameters = Inputs.Select(a => new MethodParameter { EmptyToNull = a.EmptyIsNull, IsRequired = a.RequiredField, Name = a.Name, Value = a.Value, TypeName = a.TypeName }).ToList();
            BuildParameterIterators(update, methodParameters.ToList(), itrCollection, itrs, dataObject);
            var args = new PluginInvokeArgs
            {
                AssemblyLocation = Namespace.AssemblyLocation,
                AssemblyName = Namespace.AssemblyName,
                Fullname = namespaceItem.FullName,
                Method = method.Method,
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

        private void TryExecute(int update, IDSFDataObject dataObject, List<IWarewolfIterator> itrs, IWarewolfListIterator itrCollection, List<MethodParameter> methodParameters, PluginInvokeArgs args)
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
                var result = PluginServiceExecutionFactory.InvokePlugin(args).ToString();
                ResponseManager = new ResponseManager { OutputDescription = OutputDescription, Outputs = Outputs, IsObject = IsObject, ObjectName = ObjectName };
                ResponseManager.PushResponseIntoEnvironment(result, update, dataObject, false);
            }
        }

        public IResponseManager ResponseManager { get; set; }
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
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((ISimpePlugin) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Method != null ? Method.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Namespace != null ? Namespace.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OutputDescription != null ? OutputDescription.GetHashCode() : 0);
                return hashCode;
            }
        }

#pragma warning restore S3776, S1541, S134, CC0075, S1066, S1067
    }
}
