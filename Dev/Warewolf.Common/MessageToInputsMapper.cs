/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Warewolf.Web;

namespace Warewolf
{
    public class MessageToInputsMapper
    {
        public MessageToInputsMapper()
        {
        }

        public string Map(string message, List<(string variableName, string messageValue)> inputs, bool isJson, bool isXML,bool mapWholeMessage)
        {
            var mappedDataObject = new JObject();
            if (inputs.Count > 0)
            {
                if (mapWholeMessage)
                {
                    mappedDataObject.Add(inputs[0].variableName, message);
                }
                else if ((isXML || isJson))
                {
                    var jObject = new JObject();
                    if (isXML)
                    {
                        var sXNode = JsonConvert.SerializeXNode(XDocument.Parse(message), Newtonsoft.Json.Formatting.Indented, true);
                        jObject = JsonConvert.DeserializeObject(sXNode) as JObject;
                    }
                    else if (isJson)
                    {
                        jObject = JsonConvert.DeserializeObject(message) as JObject;
                    }
                    foreach (var input in inputs)
                    {
                        var foundProp = jObject.Properties()?.FirstOrDefault(p => p.Name == input.messageValue);
                        if (foundProp != null)
                        {
                            mappedDataObject.Add(input.variableName, foundProp.Value);
                        }
                    }
                }
            }
            return JsonConvert.SerializeObject(mappedDataObject);
        }
        
    }
}