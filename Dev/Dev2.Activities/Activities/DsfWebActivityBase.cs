using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Runtime.ServiceModel.Data;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.String.Json;
using Warewolf.Storage;
using WarewolfParserInterop;

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

        protected void PushXmlIntoEnvironment(string input, int update, IDSFDataObject dataObj)
        {
            if (OutputDescription == null)
            {
                dataObj.Environment.AddError("There are no outputs");
                return;
            }
            int i = 0;
            foreach (var serviceOutputMapping in Outputs)
            {
                OutputDescription.DataSourceShapes[0].Paths[i].OutputExpression = DataListUtil.AddBracketsToValueIfNotExist(serviceOutputMapping.MappedFrom);
                i++;
            }
            if (OutputDescription.DataSourceShapes.Count == 1 && OutputDescription.DataSourceShapes[0].Paths.All(a => a is StringPath))
            {
                dataObj.Environment.Assign(Outputs.First().MappedTo, input, update);
                return;
            }
            var formater = OutputFormatterFactory.CreateOutputFormatter(OutputDescription);
            try
            {
                if (string.IsNullOrEmpty(input))
                {
                    dataObj.Environment.AddError("No Web Response received");
                }
                else
                {
                    input = formater.Format(input).ToString();

                    string toLoad = DataListUtil.StripCrap(input); // clean up the rubish ;)
                    XmlDocument xDoc = new XmlDocument();
                    toLoad = string.Format("<Tmp{0}>{1}</Tmp{0}>", Guid.NewGuid().ToString("N"), toLoad);
                    xDoc.LoadXml(toLoad);

                    if (xDoc.DocumentElement != null)
                    {
                        XmlNodeList children = xDoc.DocumentElement.ChildNodes;
                        IDictionary<string, int> indexCache = new Dictionary<string, int>();
                        var outputDefs =
                            Outputs.Select(
                                a =>
                                    new Dev2Definition(a.MappedFrom, a.MappedTo, "", a.RecordSetName, true, "", true,
                                        a.MappedTo) as IDev2Definition).ToList();
                        TryConvert(children, outputDefs, indexCache, update, dataObj);
                    }
                }
            }
            catch (Exception e)
            {
                dataObj.Environment.AddError(e.Message);
                Dev2Logger.Error(e.Message, e);
            }
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

        [ExcludeFromCodeCoverage]
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
                HttpContent httpContent = new StringContent(putData,Encoding.UTF8);
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

        void TryConvert(XmlNodeList children, IList<IDev2Definition> outputDefs, IDictionary<string, int> indexCache, int update, IDSFDataObject dataObj, int level = 0)
        {
            // spin through each element in the XML
            foreach (XmlNode c in children)
            {
                // scalars and recordset fetch
                if (level > 0)
                {
                    var c1 = c;
                    var recSetName = outputDefs.Where(definition => definition.RecordSetName == c1.Name);
                    var dev2Definitions = recSetName as IDev2Definition[] ?? recSetName.ToArray();
                    if (dev2Definitions.Length != 0)
                    {
                        // fetch recordset index
                        int fetchIdx;
                        var idx = indexCache.TryGetValue(c.Name, out fetchIdx) ? fetchIdx : 1;
                        // process recordset
                        var nl = c.ChildNodes;
                        foreach (XmlNode subc in nl)
                        {
                            // Extract column being mapped to ;)
                            foreach (var definition in dev2Definitions)
                            {
                                if (definition.MapsTo == subc.Name || definition.Name == subc.Name)
                                {
                                    dataObj.Environment.AssignWithFrame(new AssignValue(definition.RawValue, UnescapeRawXml(subc.InnerXml)), update);
                                }
                            }
                        }
                        // update this recordset index
                        dataObj.Environment.CommitAssign();
                        indexCache[c.Name] = ++idx;
                    }
                    else
                    {
                        var nameToMatch = c1.Name;
                        var useValue = false;
                        if (c.Name == GlobalConstants.NaughtyTextNode)
                        {
                            if (c1.ParentNode != null)
                            {
                                nameToMatch = c1.ParentNode.Name;
                                useValue = true;
                            }
                        }
                        var scalarName = outputDefs.FirstOrDefault(definition => definition.Name == nameToMatch);
                        if (scalarName != null)
                        {
                            var value = UnescapeRawXml(c1.InnerXml);
                            if (useValue)
                            {
                                value = UnescapeRawXml(c1.Value);
                            }
                            dataObj.Environment.AssignWithFrame(new AssignValue(DataListUtil.AddBracketsToValueIfNotExist(scalarName.MapsTo), value), update);
                        }
                    }
                }
                else
                {
                    if (level == 0)
                    {
                        // Only recurse if we're at the first level!!
                        TryConvert(c.ChildNodes, outputDefs, indexCache, update, dataObj, ++level);
                    }
                }
            }
        }

        string UnescapeRawXml(string innerXml)
        {
            if (innerXml.StartsWith("&lt;") && innerXml.EndsWith("&gt;"))
            {
                return new StringBuilder(innerXml).Unescape().ToString();
            }
            return innerXml;
        }
    }
}