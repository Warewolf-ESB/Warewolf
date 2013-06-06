//using System;
//using System.Collections.Generic;
//using System.Runtime.Serialization;
//using Dev2.DynamicServices;
//using Dev2.Workspaces;

//namespace Dev2.Runtime.ESB.Management.Services
//{
//    /// <summary>
//    /// Interigate a plugin ;)
//    /// </summary>
//    public class InterrogatePlugin : IEsbManagementEndpoint
//    {
//        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
//        {
//            string assemblyLocation;
//            string assemblyName;
//            string method;
//            string args;

//            values.TryGetValue("AssemblyLocation", out assemblyLocation);
//            values.TryGetValue("AssemblyName", out assemblyName);
//            values.TryGetValue("Method", out method);
//            values.TryGetValue("Args", out args);

//            if(string.IsNullOrEmpty(assemblyLocation) 
//                || string.IsNullOrEmpty(assemblyName) 
//                || string.IsNullOrEmpty(method) 
//                || string.IsNullOrEmpty(args))
//            {
//                throw new InvalidDataContractException("Missing arguements");
//            }

//            AppDomain tmpDomain = AppDomain.CreateDomain("PluginInterrogator");

//            var remoteHandler =
//                (RemoteObjectHandler)
//                tmpDomain.CreateInstanceFromAndUnwrap(typeof(IEsbChannel).Module.Name,
//                                                      typeof(RemoteObjectHandler).ToString());
//            string result = remoteHandler.InterrogatePlugin(assemblyLocation, assemblyName, method, args);
//            result = string.Concat("<InterrogationResult>", result, "</InterrogationResult>");
//            AppDomain.Unload(tmpDomain);

//            return result;
//        }

//        public string HandlesType()
//        {
//            return "InterogatePluginService";
//        }

//        public DynamicService CreateServiceEntry()
//        {
//            DynamicService pluingInterrogatorServicesBinder = new DynamicService();
//            pluingInterrogatorServicesBinder.Name = HandlesType();
//            pluingInterrogatorServicesBinder.DataListSpecification = "<DataList><AssemblyLocation/><AssemblyName/><Method/><Args/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>";

//            ServiceAction pluingInterrogatorServiceActionBinder = new ServiceAction();
//            pluingInterrogatorServiceActionBinder.Name = HandlesType();
//            pluingInterrogatorServiceActionBinder.SourceMethod = HandlesType();
//            pluingInterrogatorServiceActionBinder.ActionType = enActionType.InvokeManagementDynamicService;

//            // Add the action ;)
//            pluingInterrogatorServicesBinder.Actions.Add(pluingInterrogatorServiceActionBinder);

//            return pluingInterrogatorServicesBinder;
//        }
//    }
//}
