using System;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Execution
{
    /// <summary>
    /// Webservice Execution Container
    /// </summary>
    public class WebServiceContainer : EsbExecutionContainer
    {

        public WebServiceContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
            
        }
        public override Guid Execute(out ErrorResultTO errors)
        {
            throw new NotImplementedException();

            // OLD CODE!!!!
            //dynamic webServiceException = null;
            //dynamic error = new UnlimitedObject();

            //if (service.ActionType == enActionType.InvokeWebService)
            //{
            //    string svc = service.Source.Invoker.AvailableServices.FirstOrDefault();

            //    if (string.IsNullOrEmpty(svc))
            //    {
            //        error.Error = "Web Service not found in dynamic proxy";
            //    }

            //    string method = service.SourceMethod;

            //    var arguments = new List<string>();
            //    service.ServiceActionInputs.ForEach(c => { if (c.Value != null) arguments.Add(c.Value.ToString()); });

            //    string[] args = arguments.ToArray();

            //    string result = string.Empty;
            //    try
            //    {
            //        result = service.Source.Invoker.InvokeMethod<string>(svc, method, args);
            //    }
            //    catch (Exception ex)
            //    {
            //        error.Error = "Error Processing Web Service Request";
            //        error.ErrorDetail = new UnlimitedObject(ex).XmlString;
            //        ExceptionHandling.WriteEventLogEntry("Application",
            //                                             string.Format("{0}.{1}", GetType().Name, "WebServiceCommand"),
            //                                             error.XmlString, EventLogEntryType.Error);
            //        return error;
            //    }

            //    return
            //        UnlimitedObject.GetStringXmlDataAsUnlimitedObject(string.Format("<{0}>{1}</{0}>", "WebServiceResult",
            //                                                                        result));
            //}
            //return new UnlimitedObject().XmlString;
        }
    }
}
