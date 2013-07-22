using System;
using System.Text;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Services.Execution
{
    public class PluginServiceExecution : ServiceExecutionAbstract<PluginService, PluginSource>
    {
        readonly RemoteObjectHandler _remoteHandler;
      
        #region Constuctors

        public PluginServiceExecution(IDSFDataObject dataObj,bool handlesFormatting) :base(dataObj,handlesFormatting)
        {
            var handler = new RemoteObjectHandler();
            _remoteHandler = handler;
        }

        #endregion

        #region Execute
        protected override object ExecuteService()
        {
            var dataBuilder = new StringBuilder("<Args><Args>");

            foreach (var parameter in Service.Method.Parameters)
            {
                dataBuilder.Append("<Arg>");
                dataBuilder.Append("<TypeOf>");
                dataBuilder.Append(parameter.Type.Name.ToLower());
                dataBuilder.Append("</TypeOf>");
                dataBuilder.Append("<Value>");
                dataBuilder.Append(parameter.Value);
                dataBuilder.Append("</Value>");
                dataBuilder.Append("</Arg>");
            }

            dataBuilder.Append("</Args></Args>");

            var result = _remoteHandler.RunPlugin(Source.AssemblyLocation, Service.Namespace, Service.Method.Name, dataBuilder.ToString(), Service.OutputDescription);

            return result;
        }
        #endregion

    }
}
