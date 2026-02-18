using Dev2.Common;
using Dev2.Common.X6;
using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.IO;
using System.Xaml;

namespace Dev2.Runtime.ESB.WF
{
    public class WorkflowToX6Mapper
    {
        public static string MapToJson(X6RequestInfo requestInfo)
        {
            if (ChatbotContextBuilder.XamlToX6Json == null)
            {
                return string.Empty;
            }
            return ChatbotContextBuilder.XamlToX6Json(requestInfo);
        }

        public static ActivityBuilder ReadXamlDefinition(string xaml)
        {
            try
            {
                if (!string.IsNullOrEmpty(xaml))
                {
                    using (var sw = new StringReader(xaml))
                    {
                        var xamlXmlWriterSettings = new XamlXmlReaderSettings();
                        var xw = ActivityXamlServices.CreateBuilderReader(new XamlXmlReader(sw, new XamlSchemaContext(), xamlXmlWriterSettings));
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
}
