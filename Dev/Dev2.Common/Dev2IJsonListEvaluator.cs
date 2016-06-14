using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Dev2
{
    public class Dev2IJsonListEvaluator:IDev2IJsonListEvaluator
    {
        private readonly string _json;

        public Dev2IJsonListEvaluator(string json)
        {
            _json = json;
        }

        #region Implementation of IDev2IJsonListEvaluator

        public IEnumerable<object> EvalaJsonAsList()
        {
            var evalaJsonAsList = new List<object>();
            try
            {

                if (string.IsNullOrEmpty(_json))
                {
                    return evalaJsonAsList;
                }
                // ReSharper disable once AccessToStaticMemberViaDerivedType

                try // Jobject
                {
                    var jdata = JObject.Parse(_json, new JsonLoadSettings
                    {
                        CommentHandling = CommentHandling.Ignore
                        ,
                        LineInfoHandling = LineInfoHandling.Ignore
                    });

                    IList<string> keys = jdata.Properties().Select(p => p.Name).ToList();
                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (var key in keys)
                    {

                        var jToken = jdata[key];
                        var jEnumerable = jToken.Values();

                        // ReSharper disable once LoopCanBeConvertedToQuery
                        foreach (var item in jEnumerable)
                        {
                            var keyValue = item.ToString();
                            var value = keyValue.Split(':').Last().Trim();
                            evalaJsonAsList.Add(value);
                        }
                    }
                    return evalaJsonAsList;
                }
                catch(Exception)
                {
                    // ignored
                }
                try // JArray
                {
                    var jArray = JArray.Parse(_json);
                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (var tokeN in jArray)
                    {
                        var jEnumerable = tokeN.Values();
                        // ReSharper disable once LoopCanBeConvertedToQuery
                        foreach (var tV in jEnumerable)
                        {
                            var value = tV.ToString().Split(':').Last().Trim();
                            evalaJsonAsList.Add(value);
                        }

                    }
                    return evalaJsonAsList;
                }
                catch (Exception)
                {
                    // ignored
                    return evalaJsonAsList;
                }

            }
            catch (Exception)
            {
                // ignored
                return evalaJsonAsList;
            }
        }

        #endregion
    }
}