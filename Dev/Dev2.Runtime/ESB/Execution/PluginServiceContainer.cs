using System.Text;
using System.Xml.Linq;
using Dev2.DynamicServices;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Execution
{
    // BUG 9619 - 2013.06.05 - TWR - Refactored
    public class PluginServiceContainer : EsbExecutionContainerAbstract<PluginService>
    {
        readonly RemoteObjectHandler _remoteHandler;

        public PluginServiceContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : base(sa, dataObj, theWorkspace, esbChannel, false)
        {
            var handler = new RemoteObjectHandler();

            _remoteHandler = handler;

        }

        protected override PluginService CreateService(XElement serviceXml, XElement sourceXml)
        {
            return new PluginService(serviceXml) { Source = new PluginSource(sourceXml) };
        }

        protected override object ExecuteService(PluginService service)
        {
            var dataBuilder = new StringBuilder("<Args><Args>");
            
            foreach(var parameter in service.Method.Parameters)
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

            var result = _remoteHandler.RunPlugin(service.Source.AssemblyLocation, service.Namespace, service.Method.Name,dataBuilder.ToString(), ServiceAction.OutputDescription);

            return result;
        }
    }
}
