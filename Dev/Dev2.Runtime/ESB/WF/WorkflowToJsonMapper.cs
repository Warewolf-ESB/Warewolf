using Dev2.Activities.WF;
using Dev2.Communication;
using Newtonsoft.Json;
using System.Text;

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
            var builder = XamlActivityHelper.GetXamlActivityBuilderAsDataActivities(new StringBuilder(requestInfo.ActivityXaml));
            if (builder == null) { return string.Empty; }

            var graph = new WorkflowToX6Converter().ConvertToX6Json(builder, requestInfo.WorkflowXML);
            return graph;
        }
    }
}
