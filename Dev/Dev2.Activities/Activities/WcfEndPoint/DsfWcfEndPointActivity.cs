using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Unlimited.Framework.Converters.Graph;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Dev2.Activities.WcfEndPoint
{
    [ToolDescriptorInfo("WcfEndPoint", "WCF", ToolType.Native, "6AEB1028-6332-46F9-8BED-641DE4EA038E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Resources", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Resources_WCF")]
    public class DsfWcfEndPointActivity : DsfMethodBasedActivity
    {
        public IWcfAction Method { get; set; }
        public IOutputDescription OutputDescription { get; set; }
        public WcfSource Source { get; set; }
        public DsfWcfEndPointActivity()
        {
            Type = "WCF Connector";
            DisplayName = "WCF Service";
        }

        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO errors, int update)
        {
            errors = new ErrorResultTO();
            if (Method == null)
            {
                errors.AddError(ErrorResource.NoMethodSelected);
                return;
            }
            ExecuteService(update, out errors, Method, dataObject, OutputFormatterFactory.CreateOutputFormatter(OutputDescription));
        }

        protected void ExecuteService(int update, out ErrorResultTO errors, IWcfAction method, IDSFDataObject dataObject, IOutputFormatter formater = null)
        {
            errors = new ErrorResultTO();
            Source = ResourceCatalog.GetResource<WcfSource>(dataObject.WorkspaceID, SourceId);
            var itrs = new List<IWarewolfIterator>(5);
            IWarewolfListIterator itrCollection = new WarewolfListIterator();
            var methodParameters = Inputs.Select(a => new MethodParameter { EmptyToNull = a.EmptyIsNull, IsRequired = a.RequiredField, Name = a.Name, Value = a.Value, TypeName = a.TypeName }).ToList();
            BuildParameterIterators(update, methodParameters.ToList(),itrCollection,itrs,dataObject);
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
                    var result = Source.ExecuteMethod(method);

                    if (result != null)
                    {
                        ResponseManager = new ResponseManager { OutputDescription = OutputDescription, Outputs = Outputs, IsObject = IsObject, ObjectName = ObjectName };
                        ResponseManager.PushResponseIntoEnvironment(result.ToString(), update, dataObject);
                    }
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
