#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Communication;
using Dev2.Common.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dev2.Common.Serializers
{
    // TODO: All references to Dev2.Communication.Dev2JsonSerializer should move to this class
    public class Dev2JsonSerializer : ISerializer
    {

        const Formatting Formatting = Newtonsoft.Json.Formatting.Indented;

        readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };
        readonly JsonSerializerSettings _deSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        };
        public string Serialize<T>(T obj) => this.Serialize<T>(obj, Formatting);
        public string Serialize<T>(T obj, Formatting formatting) => JsonConvert.SerializeObject(obj, formatting, _serializerSettings);

        public T Deserialize<T>(string obj)
        {
            VerifyArgument.IsNotNull("message", obj);
            return JsonConvert.DeserializeObject<T>(obj, _deSerializerSettings);
        }

        public object Deserialize(string obj, Type type)
        {
            VerifyArgument.IsNotNull("message", obj);
            VerifyArgument.IsNotNull("type", type);
            return JsonConvert.DeserializeObject(obj, type, _deSerializerSettings);
        }

        public StringBuilder SerializeToBuilder(object obj)
        {
            var result = new StringBuilder();

            using (StringWriter sw = new StringWriter(result))
            {
                var jsonSerializer = new JsonSerializer
                {
                    TypeNameHandling = _serializerSettings.TypeNameHandling,
                    TypeNameAssemblyFormatHandling = _serializerSettings.TypeNameAssemblyFormatHandling,
                    ReferenceLoopHandling = _serializerSettings.ReferenceLoopHandling,
                    PreserveReferencesHandling = _serializerSettings.PreserveReferencesHandling
                };
                using (var jsonTextWriter = new JsonTextWriter(sw))
                {

                    jsonSerializer.Serialize(jsonTextWriter, obj);
                    jsonTextWriter.Flush();
                    jsonTextWriter.Close();
                }
            }

            return result;
        }

        public T Deserialize<T>(StringBuilder message) where T : class
        {
            if (message != null && message.Length > 0)
            {
                var serializer = new JsonSerializer
                {
                    TypeNameHandling = _deSerializerSettings.TypeNameHandling,
                    TypeNameAssemblyFormatHandling = _serializerSettings.TypeNameAssemblyFormatHandling,
                    ReferenceLoopHandling = _serializerSettings.ReferenceLoopHandling,
                    PreserveReferencesHandling = _serializerSettings.PreserveReferencesHandling
                };
                using (MemoryStream ms = new MemoryStream(message.Length))
                {
                    // now load the stream ;)

                    var length = message.Length;
                    var startIdx = 0;
                    var rounds = (int)Math.Ceiling(length / GlobalConstants.MAX_SIZE_FOR_STRING);


                    for (int i = 0; i < rounds; i++)
                    {
                        var len = (int)GlobalConstants.MAX_SIZE_FOR_STRING;
                        if (len > message.Length - startIdx)
                        {
                            len = message.Length - startIdx;
                        }

                        var bytes = Encoding.UTF8.GetBytes(message.Substring(startIdx, len));
                        ms.Write(bytes, 0, bytes.Length);
                        startIdx += len;
                    }

                    // rewind
                    ms.Flush();
                    ms.Position = 0;

                    try
                    {
                        // finally do the conversion ;)
                        using (StreamReader sr = new StreamReader(ms))
                        {
                            using (JsonReader jr = new JsonTextReader(sr))
                            {
                                var result = serializer.Deserialize(jr, typeof(T));
                                return result as T;
                            }
                        }
                    }

                    catch

                    {
                        // Do nothing default(T) returned below ;)
                    }
                }

            }

            return default(T);
        }

        public void Serialize(StreamWriter streamWriter, object obj)
        {
            using (streamWriter)
            {
                var jsonSerializer = new JsonSerializer
                {
                    TypeNameHandling = _serializerSettings.TypeNameHandling,
                    TypeNameAssemblyFormatHandling = _serializerSettings.TypeNameAssemblyFormatHandling,
                    ReferenceLoopHandling = _serializerSettings.ReferenceLoopHandling,
                    PreserveReferencesHandling = _serializerSettings.PreserveReferencesHandling
                };
                using (var jsonTextWriter = new JsonTextWriter(streamWriter))
                {
                    jsonSerializer.Serialize(jsonTextWriter, obj);
                    jsonTextWriter.Flush();
                    jsonTextWriter.Close();
                }
            }
        }


        public T Deserialize<T>(StreamReader streamWriter)
        {
            using (streamWriter)
            {
                var jsonSerializer = new JsonSerializer
                {
                    TypeNameHandling = _serializerSettings.TypeNameHandling,
                    TypeNameAssemblyFormatHandling = _serializerSettings.TypeNameAssemblyFormatHandling,
                    ReferenceLoopHandling = _serializerSettings.ReferenceLoopHandling,
                    PreserveReferencesHandling = _serializerSettings.PreserveReferencesHandling
                };
                using (var reader = new JsonTextReader(streamWriter))
                {
                    var result = jsonSerializer.Deserialize<T>(reader);
                    return result;
                }
            }
        }
        static bool ParseJson(JToken token, XDocument nodes, string parentNode = "", string currentNode = "")
        {
            if (token.HasValues) //has child Tokens
            {
                foreach (JToken child in token.Children())
                {
                    if (token.Type == JTokenType.Property)
                    {
                        var name = ((JProperty)token).Name;
                        var value = ((JProperty)token).Value;

                        if (parentNode == "")
                        {
                            if (child.HasValues)
                            {
                                nodes.Root.Add(new XElement(name));
                            }
                            parentNode = name;
                        }
                        else
                        {
                            if (!child.HasValues)
                            {
                                currentNode = name;
                            }
                            else
                            {
                                var node = nodes.Descendants(parentNode).FirstOrDefault();
                                if (child.Type == JTokenType.Array)
                                {
                                    node.Add(new XElement(name, new XAttribute("array", "true")));
                                }
                                else
                                {
                                    node.Add(new XElement(name));
                                }
                               
                                parentNode = name;
                                currentNode = name;
                            }
                        }
                    }

                    ParseJson(child, nodes, parentNode, currentNode);
                }
                // we are done parsing and this is a parent node
                return true;
            }
            else
            {
                if (currentNode == "")
                {
                    nodes.Root.Add(new XElement(parentNode, token.ToString()));
                }
                else
                {
                    var node = nodes.Descendants(parentNode).FirstOrDefault();
                    if (token.Type == JTokenType.Array)
                    {
                        node.Add(new XElement(currentNode, new XAttribute("array", "true")));
                    }
                    else
                    {
                        node.Add(new XElement(currentNode, token.ToString()));
                    }
                }
            }
            return false;
        }

        public XDocument DeserializeXNode(string value, string deserializeRootElementName)
        {
            var results = new Dictionary<string, Dictionary<string, int>>();
            var jsonObj = JsonConvert.DeserializeObject(value) as JObject;
            XDocument xmlDoc = new XDocument(new XDeclaration("1.0", "utf-8", ""));
            XElement root = new XElement(deserializeRootElementName);
            xmlDoc.Add(root);

            var rootObject = JObject.Parse(value);
            if (rootObject.HasValues)
            {
                ParseJson(rootObject, xmlDoc);
            }
            return xmlDoc;
        }
    }
}
