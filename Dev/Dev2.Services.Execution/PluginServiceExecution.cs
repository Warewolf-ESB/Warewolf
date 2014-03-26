using System;
using Dev2.DataList.Contract;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin;

namespace Dev2.Services.Execution
{
    public class PluginServiceExecution : ServiceExecutionAbstract<PluginService, PluginSource>
    {
        #region Constructors

        public PluginServiceExecution(IDSFDataObject dataObj, bool handlesFormatting)
            : base(dataObj, handlesFormatting)
        {
        }

        #endregion

        #region Execute

        public override void BeforeExecution(ErrorResultTO errors)
        {
        }

        public override void AfterExecution(ErrorResultTO errors)
        {
        }

        protected override object ExecuteService(out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();

            var args = new PluginInvokeArgs { AssemblyLocation = Source.AssemblyLocation, AssemblyName = Source.AssemblyName, Fullname = Service.Namespace, Method = Service.Method.Name, Parameters = Service.Method.Parameters };

            object result = null;

            try
            {
                result = PluginServiceExecutionFactory.InvokePlugin(args);
            }
            catch(Exception e)
            {
                errors.AddError(e.Message);
            }

            return result;
        }
        #endregion

    }
}
