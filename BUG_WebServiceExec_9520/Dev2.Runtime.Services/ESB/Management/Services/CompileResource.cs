//using System;
//using System.Collections.Generic;
//using Dev2.DynamicServices;
//using System.Text;
//using Dev2.Workspaces;

//namespace Dev2.Runtime.ESB.Management.Services
//{
//    /// <summary>
//    /// Compile a resource
//    /// </summary>
//    public class CompileResource : IEsbManagementEndpoint
//    {
//        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
//        {
//            StringBuilder result = new StringBuilder();
//            IDynamicServicesHost theHost = theWorkspace.Host;

//            string resourceXml;

//            values.TryGetValue("ResourceXml", out resourceXml);

//            List<DynamicServiceObjectBase> compiledResources = theHost.GenerateObjectGraphFromString(resourceXml);
//            if (compiledResources.Count == 0)
//            {
//                return string.Format("<{0}>{1}</{0}>", "Result", Resources.CompilerMessage_BuildFailed);
//            }

//            compiledResources.ForEach(c =>
//            {
//                if (c.ObjectType == enDynamicServiceObjectType.DynamicService)
//                {
//                    (c as DynamicService).Actions.ForEach(action => theHost.MapServiceActionDependencies(action));
//                }

//                c.Compile();
//                result.Append(c.GetCompilerErrors());
//            });

//            return result.ToString();
//        }

//        public DynamicService CreateServiceEntry()
//        {
//            DynamicService compileService = new DynamicService();
//            compileService.Name = HandlesType();
//            ServiceAction serviceAction = new ServiceAction();
//            serviceAction.Name = HandlesType();
//            serviceAction.ActionType = enActionType.InvokeManagementDynamicService;
//            //serviceAction.SourceName = "ManagementDynamicService";
//            serviceAction.SourceMethod = HandlesType();
//            serviceAction.DataListSpecification = "<root><ResourceXml/></root>";

//            return compileService;
//        }

//        public string HandlesType()
//        {
//            return "CompileResourceService";
//        }
//    }
//}
