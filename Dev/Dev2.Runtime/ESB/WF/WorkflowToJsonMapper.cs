using Dev2.Activities.WF;
using Dev2.Common;
using Dev2.Communication;
using Newtonsoft.Json;
using System;
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

                        finalresult.Message = new StringBuilder(json ?? string.Empty);
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
            try
            {
                var activityXamlLength = requestInfo?.ActivityXaml?.Length ?? 0;
                var builder = XamlActivityHelper.GetXamlActivityBuilderAsDataActivities(new StringBuilder(requestInfo.ActivityXaml));
                if (builder == null)
                {
                    return string.Empty;
                }

                var converter = new WorkflowToX6Converter();
                var graph = converter.ConvertToX6Json(builder, requestInfo.WorkflowXML);
                return graph;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error($"MapToJson failed: {ex.Message}", ex, GlobalConstants.WarewolfError);
                return string.Empty;
            }
        }
    }
}
