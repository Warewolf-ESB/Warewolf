using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers.ComPlugin;
using Microsoft.JScript;
using Unlimited.Framework.Converters.Graph;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("DotNetDll", "Com DLL", ToolType.Native, "6AEB1038-6332-46F9-8BDD-642DE4EA029E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Resources", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Resources_Com_DLL")]
    public class DsfComDllActivity : DsfMethodBasedActivity, IEquatable<DsfComDllActivity>
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
            Common.Utilities.PerformActionInsideImpersonatedContext(Common.Utilities.ServerUser, () => { _result = ComPluginServiceExecutionFactory.InvokeComPlugin(args).ToString(); });

            ResponseManager = new ResponseManager { OutputDescription = OutputDescription, Outputs = Outputs, IsObject = IsObject, ObjectName = ObjectName };
            ResponseManager.PushResponseIntoEnvironment(_result, update, dataObject, false);
        }

        public IResponseManager ResponseManager { get; set; }
        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

        public bool Equals(DsfComDllActivity other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            var equalityComparer = EqualityFactory.GetEqualityComparer<DsfComDllActivity>(
                (p1, p2) =>
                {
                    bool methodsAreEqual = (p1.Method == null && p2.Method == null);

                    if (p1.Method == null || p2.Method == null)
                    {
                        methodsAreEqual = false;
                    }
                    else
                    {
                        if (p1.Method != null && p2.Method != null)
                        {
                            methodsAreEqual = string.Equals(p1.Method.Method, p2.Method.Method)
                                              && string.Equals(p1.Method.Dev2ReturnType, p2.Method.Dev2ReturnType)
                                              && string.Equals(p1.Method.ErrorMessage, p2.Method.ErrorMessage)
                                              && string.Equals(p1.Method.FullName, p2.Method.FullName)
                                              && string.Equals(p1.Method.MethodResult, p2.Method.MethodResult)
                                              && string.Equals(p1.Method.OutputVariable, p2.Method.OutputVariable)
                                              && p1.Method.ReturnType == p2.Method.ReturnType
                                              && Equals(p1.Method.HasError, p2.Method.HasError)
                                              && Equals(p1.Method.IsObject, p2.Method.IsObject)
                                              && Equals(p1.Method.IsProperty, p2.Method.IsProperty)
                                              && Equals(p1.Method.IsVoid, p2.Method.IsVoid)
                                              && Equals(p1.Method.ID, p2.Method.ID)
                                              && Equals(p1.Namespace.AssemblyLocation, p2.Namespace.AssemblyLocation)
                                              && CommonEqualityOps.CollectionEquals(p1.Method.Inputs, p2.Method.Inputs, EqualityFactory.GetEqualityComparer<IServiceInput>(
                                                  (input1, input2) =>
                                                  {
                                                      return string.Equals(input1.ActionName, input2.ActionName)
                                                             && string.Equals(input1.ActionName, input2.ActionName);
                                                  }, input => 1))
                                ;
                        }
                    }
                    return methodsAreEqual;
                },
                p =>
                {
                    int hashCode = base.GetHashCode();
                    hashCode = (hashCode * 397) ^ (_result?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 397) ^ (Method?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 397) ^ (Namespace?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 397) ^ (OutputDescription?.GetHashCode() ?? 0);
                    return hashCode;
                });

            var @equals = equalityComparer.Equals(this, other);

            return base.Equals(other) && @equals;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DsfComDllActivity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (_result != null ? _result.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Method != null ? Method.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Namespace != null ? Namespace.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OutputDescription != null ? OutputDescription.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
