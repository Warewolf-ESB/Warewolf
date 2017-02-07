using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Diagnostics;
using Dev2.Runtime.ServiceModel.Data;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;

namespace Dev2.Activities
{
    public class DsfWebActivityBase : DsfActivity
    {
        private readonly WebRequestMethod _method;
        private const string UserAgent = "User-Agent";

        protected DsfWebActivityBase(WebRequestDataDto webRequestDataDto)
        {
            _method = webRequestDataDto.WebRequestMethod;
            Type = webRequestDataDto.Type;
            DisplayName = webRequestDataDto.DisplayName;
        }

        public IList<INameValue> Headers { get; set; }
        public string QueryString { get; set; }
        public IOutputDescription OutputDescription { get; set; }
        public IResponseManager ResponseManager { get; set; }
        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            if (env == null)
            {
                return new List<DebugItem>();
            }
            base.GetDebugInputs(env, update);

            var head = Headers.Select(a => new NameValue(ExecutionEnvironment.WarewolfEvalResultToString(env.Eval(a.Name, update)), ExecutionEnvironment.WarewolfEvalResultToString(env.Eval(a.Value, update)))).Where(a => !(String.IsNullOrEmpty(a.Name) && String.IsNullOrEmpty(a.Value)));
            var query = ExecutionEnvironment.WarewolfEvalResultToString(env.Eval(QueryString, update));
            var url = ResourceCatalog.GetResource<WebSource>(Guid.Empty, SourceId);
            string headerString = string.Join(" ", head.Select(a => a.Name + " : " + a.Value));

            DebugItem debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "URL"), debugItem);
            AddDebugItem(new DebugEvalResult(url.Address, "", env, update), debugItem);
            _debugInputs.Add(debugItem);
            debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "Query String"), debugItem);
            AddDebugItem(new DebugEvalResult(query, "", env, update), debugItem);
            _debugInputs.Add(debugItem);
            debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "Headers"), debugItem);
            AddDebugItem(new DebugEvalResult(headerString, "", env, update), debugItem);
            _debugInputs.Add(debugItem);

            return _debugInputs;
        }

        public virtual HttpClient CreateClient(IEnumerable<NameValue> head, string query, WebSource source)
        {
            var httpClient = new HttpClient();
            if (source.AuthenticationType == AuthenticationType.User)
            {
                var byteArray = Encoding.ASCII.GetBytes(String.Format("{0}:{1}", source.UserName, source.Password));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }

            if (head != null)
            {
                IEnumerable<NameValue> nameValues = head.Where(nameValue => !String.IsNullOrEmpty(nameValue.Name) && !String.IsNullOrEmpty(nameValue.Value));
                foreach (var nameValue in nameValues)
                {
                    httpClient.DefaultRequestHeaders.Add(nameValue.Name, nameValue.Value);
                }
            }

            httpClient.DefaultRequestHeaders.Add(UserAgent, GlobalConstants.UserAgentString);

            var address = source.Address;
            if (!string.IsNullOrEmpty(query))
            {
                address = address + query;
            }
            try
            {
                var baseAddress = new Uri(address);
                httpClient.BaseAddress = baseAddress;
            }
            catch (UriFormatException e)
            {
                //CurrentDataObject.Environment.AddError(e.Message);// To investigate this
                Dev2Logger.Error(e.Message, e); // Error must be added on the environment
                return httpClient;
            }

            return httpClient;
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

        protected virtual string PerformWebPostRequest(IEnumerable<NameValue> head, string query, WebSource source, string putData)
        {
            var headerValues = head as NameValue[] ?? head.ToArray();
            var httpClient = CreateClient(headerValues, query, source);
            if (httpClient != null)
            {
                var address = source.Address;
                if (query != null)
                {
                    address = address + query;
                }
                if (_method == WebRequestMethod.Get)
                {
                    var taskOfString = httpClient.GetStringAsync(new Uri(address));
                    return taskOfString.Result;
                }
                Task<HttpResponseMessage> taskOfResponseMessage;
                if (_method == WebRequestMethod.Delete)
                {
                    taskOfResponseMessage = httpClient.DeleteAsync(new Uri(address));
                    bool ranToCompletion = taskOfResponseMessage.Status == TaskStatus.RanToCompletion;
                    return ranToCompletion ? "The task completed execution successfully" : "The task completed due to an unhandled exception";
                }
                if (_method == WebRequestMethod.Post)
                {
                    taskOfResponseMessage = httpClient.PostAsync(new Uri(address), new StringContent(putData));
                    var message = taskOfResponseMessage.Result.Content.ReadAsStringAsync().Result;
                    return message;
                }
                HttpContent httpContent = new StringContent(putData, Encoding.UTF8);
                var contentType = headerValues.FirstOrDefault(value => value.Name.ToLower() == "Content-Type".ToLower());
                if (contentType != null)
                {
                    httpContent.Headers.ContentType = new MediaTypeHeaderValue(contentType.Value);
                }
                var httpRequest = new HttpRequestMessage(HttpMethod.Put, new Uri(address))
                {
                    Content = httpContent
                };
                taskOfResponseMessage = httpClient.SendAsync(httpRequest);
                var resultAsString = taskOfResponseMessage.Result.Content.ReadAsStringAsync().Result;
                return resultAsString;
            }
            return null;
        }
    }
}