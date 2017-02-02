using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers.ComPlugin;
using Unlimited.Framework.Converters.Graph;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
// ReSharper disable NonLocalizedString

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dev2.Activities
{
    [ToolDescriptorInfo("DotNetDll", "Com DLL", ToolType.Native, "6AEB1038-6332-46F9-8BDD-642DE4EA029E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Resources", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Resources_Com_DLL")]
    public class DsfComDllActivity : DsfMethodBasedActivity
    {
        public string _result;
        public IPluginAction Method { get; set; }
        public INamespaceItem Namespace { get; set; }
        public IOutputDescription OutputDescription { get; set; }

        public DsfComDllActivity()
        {
            Type = "Com DLL Connector";
            DisplayName = "Com DLL";
        }


        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO errors, int update)
        {
            errors = new ErrorResultTO();           
            if (Method == null)
            {
                errors.AddError(ErrorResource.NoMethodSelected);
                return;
            }

                
            ExecuteService(update, out errors, Method, dataObject);
        }

        protected void ExecuteService(int update, out ErrorResultTO errors, IPluginAction method, IDSFDataObject dataObject)
        {
            errors = new ErrorResultTO();
            var itrs = new List<IWarewolfIterator>(5);
            IWarewolfListIterator itrCollection = new WarewolfListIterator();
            var source = ResourceCatalog.GetResource<ComPluginSource>(dataObject.WorkspaceID, SourceId);
            var methodParameters = Inputs.Select(a => new MethodParameter { EmptyToNull = a.EmptyIsNull, IsRequired = a.RequiredField, Name = a.Name, Value = a.Value, TypeName = a.TypeName }).ToList();
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
                if (Inputs == null || Inputs.Count == 0)
                {
                    PerfromExecution(update, dataObject, args);
                }
                else
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
                        PerfromExecution(update, dataObject, args);
                    }
                }
            }
            catch (Exception e)
            {
                errors.AddError(e.Message);
            }
        }
                
        private void PerfromExecution(int update, IDSFDataObject dataObject, ComPluginInvokeArgs args)
        {
            if(!IsObject)
             {
                int i = 0;
                foreach(var serviceOutputMapping in Outputs)
                {
                    OutputDescription.DataSourceShapes[0].Paths[i].OutputExpression = DataListUtil.AddBracketsToValueIfNotExist(serviceOutputMapping.MappedTo);
                    i++;
                }
                var outputFormatter = OutputFormatterFactory.CreateOutputFormatter(OutputDescription);
                args.OutputFormatter = outputFormatter;
            }
            Common.Utilities.PerformActionInsideImpersonatedContext(Common.Utilities.ServerUser, () => { _result = ComPluginServiceExecutionFactory.InvokeComPlugin(args).ToString(); });
            
            ResponseManager = new ResponseManager { OutputDescription = OutputDescription, Outputs = Outputs, IsObject = IsObject, ObjectName = ObjectName };
            ResponseManager.PushResponseIntoEnvironment(_result, update, dataObject, false);
        }
       
        public IResponseManager ResponseManager { get; set; }
        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

    }
}
