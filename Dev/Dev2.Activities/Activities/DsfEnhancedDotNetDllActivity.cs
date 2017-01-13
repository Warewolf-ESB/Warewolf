using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin;
using Unlimited.Framework.Converters.Graph;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("DotNetDll", "DotNet DLL", ToolType.Native, "6AEB1038-6332-46F9-8BDD-641DE4EA038D", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Resources", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Resources_Dot_net_DLL")]
    public class DsfEnhancedDotNetDllActivity : DsfMethodBasedActivity
    {
        public IPluginAction Method { get; set; }
        public INamespaceItem Namespace { get; set; }
        public IPluginConstructor Constructor { get; set; }
        public ICollection<IServiceInput> ConstructorInputs { get; set; }
        public List<Dev2MethodInfo> MethodsToRun { get; set; }
        public string ExistingObject { get; set; }
        public IOutputDescription OutputDescription { get; set; }

        public DsfEnhancedDotNetDllActivity()
        {
            Type = "DotNet DLL Connector";
            DisplayName = "DotNet DLL";
            ConstructorInputs = new List<IServiceInput>();
            Inputs = new List<IServiceInput>();
            ExistingObject = string.Empty;
            MethodsToRun = new List<Dev2MethodInfo>();
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


            ExecuteService(update, out errors, Constructor, Method, Namespace, dataObject);
        }

        protected void ExecuteService(int update, out ErrorResultTO errors, IPluginConstructor constructor, IPluginAction method, INamespaceItem namespaceItem, IDSFDataObject dataObject)
        {
            errors = new ErrorResultTO();
            var itrs = new List<IWarewolfIterator>(5);
            IWarewolfListIterator itrCollection = new WarewolfListIterator();
            var warewolfEvalResult = dataObject.Environment.Eval(ExistingObject, update);
            var existingObject = ExecutionEnvironment.WarewolfEvalResultToString(warewolfEvalResult);
            var constructorParameters = ConstructorInputs.Select(p => new ConstructorParameter()
            {
                Name = p.Name,
                TypeName = p.TypeName,
                Value = p.Value,
                IsRequired = p.RequiredField,
                EmptyToNull = p.EmptyIsNull
            });
            var methodParameters = Inputs.Select(a => new MethodParameter
            {
                EmptyToNull = a.EmptyIsNull
                ,
                IsRequired = a.RequiredField
                ,
                Name = a.Name
                ,
                Value = a.Value
                ,
                TypeName = a.TypeName
            }).ToList();
            var parameters = constructorParameters as ConstructorParameter[] ?? constructorParameters.ToArray();
            BuildParameterIterators(update, parameters.Cast<MethodParameter>().ToList(), itrCollection, itrs, dataObject);

            var args = new PluginInvokeArgs
            {
                AssemblyLocation = Namespace.AssemblyLocation,
                AssemblyName = Namespace.AssemblyName,
                Fullname = namespaceItem.FullName,
                PluginConstructor = constructor,
                Method = method.Method,
                Parameters = methodParameters,
                MethodsToRun = MethodsToRun
            };

            try
            {
                if (itrCollection.HasMoreData())
                {
                    while (itrCollection.HasMoreData())
                    {
                        int pos = 0;
                        foreach (var itr in itrs)
                        {
                            string injectVal = itrCollection.FetchNextValue(itr);
                            var param = parameters.ToList()[pos];


                            param.Value = param.EmptyToNull &&
                                          (injectVal == null ||
                                           string.Compare(injectVal, string.Empty,
                                               StringComparison.InvariantCultureIgnoreCase) == 0)
                                ? null
                                : injectVal;

                            pos++;
                        }
                        CreateOutputFormater(args);
                        var result = PluginServiceExecutionFactory.InvokePlugin(args, existingObject).ToString();
                        ResponseManager = new ResponseManager { OutputDescription = OutputDescription, Outputs = Outputs, IsObject = IsObject, ObjectName = ObjectName };
                        ResponseManager.PushResponseIntoEnvironment(result, update, dataObject, false);
                    }
                }
                else
                {
                    CreateOutputFormater(args);
                    if (string.IsNullOrEmpty(existingObject))
                    {
                        existingObject= PluginServiceExecutionFactory.CreateInstance(args);
                    }
                    var result = PluginServiceExecutionFactory.InvokePlugin(args, existingObject).ToString();
                    ResponseManager = new ResponseManager { OutputDescription = OutputDescription, Outputs = Outputs, IsObject = IsObject, ObjectName = ObjectName };
                    ResponseManager.PushResponseIntoEnvironment(result, update, dataObject, false);
                }

            }
            catch (Exception e)
            {
                errors.AddError(e.Message);
            }
        }

        private void CreateOutputFormater(PluginInvokeArgs args)
        {
            if(!IsObject)
            {
                int i = 0;
                if(Outputs != null)
                {
                    foreach(var serviceOutputMapping in Outputs)
                    {
                        OutputDescription.DataSourceShapes[0].Paths[i].OutputExpression = DataListUtil.AddBracketsToValueIfNotExist(serviceOutputMapping.MappedTo);
                        i++;
                    }
                }
                if(OutputDescription != null)
                {
                    var outputFormatter = OutputFormatterFactory.CreateOutputFormatter(OutputDescription);
                    args.OutputFormatter = outputFormatter;
                }
            }
        }

        public IResponseManager ResponseManager { get; set; }
        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

    }
}