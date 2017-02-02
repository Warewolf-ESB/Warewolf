using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin;
using Unlimited.Framework.Converters.Graph;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dev2.Activities
{
    //[ToolDescriptorInfo("DotNetDll", "DotNet DLL", ToolType.Native, "6AEB1038-6332-46F9-8BDD-641DE4EA038E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Resources", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Resources_Dot_net_DLL")]
    public class DsfDotNetDllActivity : DsfMethodBasedActivity
    {
        public IPluginAction Method { get; set; }
        public INamespaceItem Namespace { get; set; }

        public IOutputDescription OutputDescription { get; set; }

        public DsfDotNetDllActivity()
        {
            Type = "DotNet DLL Connector";
            DisplayName = "DotNet DLL";
        }


        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO errors, int update)
        {
            errors = new ErrorResultTO();
            if (Namespace == null)
            {
                errors.AddError(ErrorResource.NoNamespaceSelected);
                return;
            }
            if (Method == null)
            {
                errors.AddError(ErrorResource.NoMethodSelected);
                return;
            }

                
            ExecuteService(update, out errors, Method, Namespace, dataObject);
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
                while (itrCollection.HasMoreData())
                {
                    int pos = 0;
                    foreach (var itr in itrs)
                    {
                        string injectVal = itrCollection.FetchNextValue(itr);
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
                        int i = 0;
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
                    ResponseManager.PushResponseIntoEnvironment(result, update, dataObject,false);
                }
            }
            catch (Exception e)
            {
                errors.AddError(e.Message);
            }
        }

        public IResponseManager ResponseManager { get; set; }
        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

    }
}
