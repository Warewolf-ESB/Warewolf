using Newtonsoft.Json;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Xml.Linq;
using Dev2.Common.X6;
using Dev2.Common;
using System.Activities.XamlIntegration;
using System.Text;
using System.Xaml;

namespace Dev2.Runtime.ESB.WF
{

    public class WorkflowToX6Mapper
    {
        public static string MapToJson(RequestInfo requestInfo)
        {
            var builder = ReadXamlDefinition(requestInfo.ActivityXaml);
            
            var graph = new WorkflowToX6Converter1().ConvertToX6Json(builder, requestInfo.XML);

        }

        public static ActivityBuilder ReadXamlDefinition(string xaml)
        {
            try
            {
                if (!string.IsNullOrEmpty(xaml))
                {
                    using (var sw = new System.IO.StringReader(xaml))
                    {
                        var xamlXmlWriterSettings = new XamlXmlReaderSettings();
                        var xw = ActivityXamlServices.CreateBuilderReader(new System.Xaml.XamlXmlReader(sw, new XamlSchemaContext(), xamlXmlWriterSettings));
                        var load = XamlServices.Load(xw);
                        return load as ActivityBuilder;
                    }
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error("Error loading XAML: ", e, GlobalConstants.WarewolfError);
            }
            return null;
        }
    }

    public class RequestInfo
    {
        public string WorkflowXML { get; set; }
        public string ActivityXaml { get; set; }
    }
}
