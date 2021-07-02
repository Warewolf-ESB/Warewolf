/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Runtime.Services;
using Dev2.Data.Util;
using Dev2.Runtime.WebServer.Handlers;
using Warewolf.Data;

namespace Dev2.Runtime.WebServer.Executor
{
    public class SubmittedData
    {
        private readonly IStreamWriterFactory _streamWriterFactory;
        private readonly IStreamContentFactory _streamContentFactory;
        private readonly IMultipartMemoryStreamProviderFactory _multipartMemoryStreamProviderFactory;
        private readonly IMemoryStreamFactory _memoryStreamFactory;

        public SubmittedData()
        {
            _streamContentFactory = new StreamContentFactory();
            _streamWriterFactory = new StreamWriterFactory();
            _multipartMemoryStreamProviderFactory = new MultipartMemoryStreamProviderFactory();
            _memoryStreamFactory = new MemoryStreamFactory();
        }

        public SubmittedData(IStreamWriterFactory streamWriterFactory, IStreamContentFactory streamContentFactory, IMultipartMemoryStreamProviderFactory streamProviderFactory, IMemoryStreamFactory memoryStreamFactory)
        {
            _streamWriterFactory = streamWriterFactory;
            _streamContentFactory = streamContentFactory;
            _multipartMemoryStreamProviderFactory = streamProviderFactory;
            _memoryStreamFactory = memoryStreamFactory;
        }

        internal string GetPostData(ICommunicationContext ctx)
        {
            var baseStr = HttpUtility.UrlDecode(ctx.Request.Uri.ToString());
            baseStr = HttpUtility.UrlDecode(CleanupXml(baseStr));
            string payload = null;
            if (baseStr != null)
                {
                    var startIdx = baseStr.IndexOf("?", StringComparison.Ordinal);
                if (startIdx > 0)
                {
                    payload = baseStr.Substring(startIdx + 1);
                    if (payload.IsXml() || payload.IsJSON())
                    {
                        return payload;
                    }
                }
            }

            if (ctx.Request.Method == "GET")
            {
                return ExtractKeyValuePairForGetMethod(ctx, payload);
            }

            if (ctx.Request.Method == "POST")
            {
                using (var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
                {
                    try
                    {
                        return ExtractKeyValuePairForPostMethod(ctx, reader);
                    }
                    catch (Exception ex)    
                    {
                        Dev2Logger.Error(nameof(AbstractWebRequestHandler), ex, GlobalConstants.WarewolfError);
                    }
                }
            }

            return string.Empty;
        }

        internal static string CleanupXml(string baseStr)
        {
            if (baseStr.Contains("?"))
            {
                var startQueryString = baseStr.IndexOf("?", StringComparison.Ordinal);
                var query = baseStr.Substring(startQueryString + 1);
                if (query.IsJSON())
                {
                    return baseStr;
                }

                var args = HttpUtility.ParseQueryString(query);
                var url = baseStr.Substring(0, startQueryString + 1);
                var results = new List<string>();
                foreach (var arg in args.AllKeys)
                {
                    var txt = args[arg];
                    txt = CheckForEscapeCharacters(txt);
                    results.Add(txt.IsXml() ? arg + "=" + string.Format(GlobalConstants.XMLPrefix + "{0}", Convert.ToBase64String(Encoding.UTF8.GetBytes(txt))) : $"{arg}={txt}");
                }

                return url + string.Join("&", results);
            }

            return baseStr;
        }

        static string CheckForEscapeCharacters(string text)
        {
            var escapeCharacters = new[] { "\\\"" };
            if (escapeCharacters.Any(text.Contains))
            {
                text = Regex.Unescape(text);
            }
            return text;
        }


        internal static string ExtractKeyValuePairForGetMethod(ICommunicationContext ctx, string payload)
        {
            if (payload != null)
            {
                var keyValuePairs = payload.Split(new[] {"&"}, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (var keyValuePair in keyValuePairs)
                {
                    if (keyValuePair.StartsWith("wid="))
                    {
                        continue;
                    }

                    if (keyValuePair.IsXml() || keyValuePair.IsJSON() || (keyValuePair.ToLowerInvariant().Contains("<DataList>".ToLowerInvariant()) && keyValuePair.ToLowerInvariant().Contains("</DataList>".ToLowerInvariant())))
                    {
                        return keyValuePair;
                    }
                }
            }

            var pairs = ctx.Request.QueryString;
            return ExtractKeyValuePairs(pairs, ctx.Request.BoundVariables);
        }

        string ExtractKeyValuePairForPostMethod(ICommunicationContext ctx, StreamReader reader)
        {
            NameValueCollection pairs;
            if (ctx.Request.Content.IsMimeMultipartContent("form-data"))
            {
                pairs = ExtractMultipartFormDataArgumentsFromDataList(ctx, reader);
                return ExtractKeyValuePairs(pairs, ctx.Request.BoundVariables);
            }

            var data = reader.ReadToEnd();

            if (data.IsXml(out XDocument _))
            {
                return data.CleanXmlSOAP();
            }

            if (DataListUtil.IsJson(data))
            {
                return data;
            }

            pairs = ExtractArgumentsFromDataListOrQueryString(ctx, data);
            return ExtractKeyValuePairs(pairs, ctx.Request.BoundVariables);
        }

        NameValueCollection ExtractMultipartFormDataArgumentsFromDataList(ICommunicationContext ctx, StreamReader reader)
        {
            var provider = _multipartMemoryStreamProviderFactory.New();
            var tempStream = _memoryStreamFactory.New();

            var reqContentStream = ctx.Request.Content.ReadAsStreamAsync().Result;
            reqContentStream.CopyTo(tempStream);

            var byteArray = reader.BaseStream.ToByteArray();
            var stream = _memoryStreamFactory.New(byteArray);
            stream.CopyTo(tempStream);
            
            tempStream.Seek(0, SeekOrigin.End);
            var writer = _streamWriterFactory.New(tempStream);
            writer.WriteLine();
            writer.Flush();
            tempStream.Position = 0;

            var streamContent = _streamContentFactory.New(tempStream);
            var requestContentHeaders = ctx.Request.Content.Headers;
            foreach (var header in requestContentHeaders.Headers)
            {
                streamContent.Headers.Add(header.Key, header.Value);
            }

            streamContent.ReadAsMultipartAsync(provider).Wait();
            var valuePairs = new NameValueCollection();
            foreach (HttpContent content in provider.Contents)
            {
                var contentDisposition = content.Headers.ContentDisposition;
                var name = contentDisposition.Name.Trim('"');
                var byteData= content.ReadAsByteArrayAsync().Result;

                var contentType = content.Headers.ContentType;
                var mediaType = contentType?.MediaType;
                if (mediaType == null)
                {
                    valuePairs.Add(name, byteData.ReadToString());
                    continue;
                }

                valuePairs.Add(name, byteData.ToBase64String());
            }

            return valuePairs;
        }

        static NameValueCollection ExtractArgumentsFromDataListOrQueryString(ICommunicationContext ctx, string data)
        {
            var pairs = new NameValueCollection(5);
            var keyValuePairs = data.Split(new[] {"&"}, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var keyValuePair in keyValuePairs)
            {
                var keyValue = keyValuePair.Split(new[] {"="}, StringSplitOptions.RemoveEmptyEntries);
                if (keyValue.Length > 1)
                {
                    pairs.Add(keyValue[0], keyValue[1]);
                }
                else
                {
                    if (keyValue.Length == 1 && (keyValue[0].IsXml() || keyValue[0].IsJSON()))
                    {
                        pairs.Add(keyValue[0], keyValue[0]);
                    }
                }
            }

            if (pairs.Count == 0)
            {
                pairs = ctx.Request.QueryString;
            }

            return pairs;
        }

        internal static string ExtractKeyValuePairs(NameValueCollection pairs, NameValueCollection boundVariables)
        {
            // Extract request keys
            foreach (var key in pairs.AllKeys)
            {
                if (key == "wid") //Don't add the Workspace ID to DataList
                {
                    continue;
                }

                if (key.IsXml(out XDocument _) || key.IsJSON() || (key.ToLowerInvariant().Contains("<DataList>".ToLowerInvariant()) && key.ToLowerInvariant().Contains("<\\DataList>".ToLowerInvariant())))
                {
                    return key; //We have a workspace id and XML DataList
                }

                boundVariables.Add(key, pairs[key]);
            }

            return string.Empty;
        }
    }
}
