using System;
using System.Text;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Services.Execution
{
    public class PluginServiceExecution : ServiceExecutionAbstract<PluginService, PluginSource>
    {
        readonly RemoteObjectHandler _remoteHandler;

        #region Constructors

        public PluginServiceExecution(IDSFDataObject dataObj, bool handlesFormatting)
            : base(dataObj, handlesFormatting)
        {
            var handler = new RemoteObjectHandler();
            _remoteHandler = handler;
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
            var dataBuilder = new StringBuilder("<Args><Args>");
            errors = new ErrorResultTO();
            _remoteHandler.Errors.ClearErrors();

            foreach(var parameter in Service.Method.Parameters)
            {
                dataBuilder.Append("<Arg>");
                dataBuilder.Append("<TypeOf>");
                if(parameter.Type == null)
                {
                    //if Type is null we have big issues ;(
                    throw new Exception("Null Type");
                }
                dataBuilder.Append(parameter.Type.Name.ToLower());
                dataBuilder.Append("</TypeOf>");
                dataBuilder.Append("<Value>");

                var tmpInjectValue = parameter.Value;

                if(parameter.Value == null)
                {
                    tmpInjectValue = GlobalConstants.NullPluginValue;
                }

                dataBuilder.Append(tmpInjectValue);

                dataBuilder.Append("</Value>");
                dataBuilder.Append("</Arg>");
            }

            dataBuilder.Append("</Args></Args>");

            var result = _remoteHandler.RunPlugin(Source.AssemblyLocation, Service.Namespace, Service.Method.Name, dataBuilder.ToString(), Service.OutputDescription);

            errors.MergeErrors(_remoteHandler.Errors);

            return result;
        }
        #endregion

    }
}
