using Dev2.Activities.WF;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Communication;
using Newtonsoft.Json;
using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Text;
using System.Xaml;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Runtime.ESB.WF
{
    /// <summary>
    /// Workflow to Json Mapper
    /// </summary>
    public class WorkflowToJsonMapper
    {
        /// <summary>
        /// Processes request to create map workflow to Json
        /// </summary>
        /// <param name="request">Execution request</param>
        public static void Process(EsbExecuteRequest request)
        {
            var data = request.ExecuteResult;
            var finalresult = new ExecuteMessage();

            if (data != null)
            {
                var executionResult = JsonConvert.DeserializeObject<ExecuteMessage>(data.ToString());

                if (null != executionResult)
                {
                    var requestInfo = JsonConvert.DeserializeObject<Common.X6.X6RequestInfo>(executionResult.Message.ToString());
                    if (requestInfo != null)
                    {
                        var json = MapToJson(requestInfo);

                        finalresult.Message = new StringBuilder(json);
                    }
                    var serializer = new Dev2JsonSerializer();
                    request.ExecuteResult = serializer.SerializeToBuilder(finalresult);
                }
            }
        }

        /// <summary>
        /// Maps request to Json
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <returns></returns>
        public static string MapToJson(Common.X6.X6RequestInfo requestInfo)
        {
            var builder = GetXamlActivityBuilderAsDataActivities(new StringBuilder(requestInfo.ActivityXaml));
            if (builder == null) { return string.Empty; }

            var graph = new WorkflowToX6Converter().ConvertToX6Json(builder, requestInfo.WorkflowXML);
            return graph;
        }

        /// <summary>
        /// Gets Xaml ActivityBuilder from xamlDefinition.
        /// </summary>
        /// <param name="xamlDefinition">The xaml definition.</param>
        /// <returns cref="ActivityBuilder">ActivityBuilder</returns>
        public static ActivityBuilder GetXamlActivityBuilderAsDataActivities(StringBuilder xamlDefinition)
        {
            if (xamlDefinition == null || xamlDefinition.Length == 0)
            {
                return null;
            }

            try
            {
                if (GlobalConstants.RuntimeNamespaceClean)
                {
                    xamlDefinition = new Dev2XamlCleaner().CleanServiceDef(xamlDefinition);
                }

#if !(WINDOWS || NETFRAMEWORK)
                DynamicServices.Objects.Dev2XamlLoader.RemoveWindowsElements(ref xamlDefinition);
#endif
                using (var xamlStream = xamlDefinition.EncodeForXmlDocument(tryUnicodeFirst: false))
                {
                    var settings = new XamlXmlReaderSettings
                    {
                        //LocalAssembly = System.Reflection.Assembly.GetAssembly(typeof(VirtualizedContainerService))
                        LocalAssembly = System.Reflection.Assembly.GetAssembly(typeof(DsfFlowDecisionActivity))
                    };
                    using (var reader = new XamlXmlReader(xamlStream, settings))
                    {
                        var xw = ActivityXamlServices.CreateBuilderReader(reader);
                        var load = XamlServices.Load(xw);
                        return load as ActivityBuilder;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
