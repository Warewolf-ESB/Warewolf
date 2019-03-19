#pragma warning disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.DB;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.String.Json;
using WarewolfParserInterop;


namespace Dev2.Activities
{
    public class ResponseManager : IResponseManager
    {
        public bool IsObject { get; set; }
        public string ObjectName { get; set; }
        public IOutputDescription OutputDescription { get; set; }
        public ICollection<IServiceOutputMapping> Outputs { get; set; }
        public void PushResponseIntoEnvironment(string input, int update, IDSFDataObject dataObj) => PushResponseIntoEnvironment(input, update, dataObj, true);
#pragma warning disable S1541 // Methods and properties should not be too complex
        public void PushResponseIntoEnvironment(string input, int update, IDSFDataObject dataObj, bool formatResult)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            if (dataObj == null)
            {
                throw new ArgumentNullException(nameof(dataObj));
            }

            try
            {
                if (!IsObject && (Outputs == null || Outputs.Count == 0))
                {
                    return;
                }
                if (!IsObject && OutputDescription != null && OutputDescription.DataSourceShapes.Count == 1 && OutputDescription.DataSourceShapes[0].Paths.All(a => a is StringPath))
                {
                    var serviceOutputMapping = Outputs.First();
                    if (serviceOutputMapping != null)
                    {
                        dataObj.Environment.Assign(serviceOutputMapping.MappedTo, input, update);
                    }
                    return;
                }
                if (IsObject)
                {
                    AssignObject(input, update, dataObj);
                }
                else
                {
                    TryFormatOutput(input, update, dataObj, formatResult);
                }
            }
            catch (Exception e)
            {
                dataObj.Environment.AddError(e.Message);
                Dev2Logger.Error(e.Message, e, GlobalConstants.WarewolfError);
            }
        }

        void TryFormatOutput(string input, int update, IDSFDataObject dataObj, bool formatResult)
        {
            IOutputFormatter formater = null;
            if (OutputDescription != null)
            {
                var i = 0;
                foreach (var serviceOutputMapping in Outputs)
                {
                    OutputDescription.DataSourceShapes[0].Paths[i].OutputExpression = !string.IsNullOrEmpty(DataListUtil.RemoveLanguageBrackets(serviceOutputMapping.MappedTo)) ? DataListUtil.AddBracketsToValueIfNotExist(serviceOutputMapping.MappedTo) : string.Empty;
                    i++;
                }
                formater = OutputFormatterFactory.CreateOutputFormatter(OutputDescription);
            }
            if (!string.IsNullOrEmpty(input))
            {
                FormatForOutput(input, update, dataObj, formatResult, formater);
            }
        }

        void AssignObject(string input, int update, IDSFDataObject dataObj)
        {

            try
            {
                dataObj.Environment.AssignJson(new AssignValue(ObjectName, input), update);
            }
            catch (Exception ex1)
            {
                Dev2Logger.Error(ex1, GlobalConstants.WarewolfError);
            }

        }

        void FormatForOutput(string input, int update, IDSFDataObject dataObj, bool formatResult, IOutputFormatter formater)
        {
            var formattedInput = input;
            if (formater != null && formatResult)
            {
                formattedInput = formater.Format(input).ToString();
            }

            var xDoc = new XmlDocument();
            formattedInput = string.Format("<Tmp{0}>{1}</Tmp{0}>", Guid.NewGuid().ToString("N"), formattedInput);
            xDoc.LoadXml(formattedInput);

            if (xDoc.DocumentElement != null)
            {
                var children = xDoc.DocumentElement.ChildNodes;
                IDictionary<string, int> indexCache = new Dictionary<string, int>();
                var outputDefs =
                    Outputs.Select(
                        a =>
                            new Dev2Definition(DataListUtil.GetVariableNameToMapOutputTo(a.MappedTo), a.MappedTo, "", a.RecordSetName, true, "", true,
                                a.MappedTo) as IDev2Definition).ToList();
                TryConvert(children, outputDefs, indexCache, update, dataObj);
            }
        }

        public string UnescapeRawXml(string innerXml)
        {
            if (innerXml.StartsWith("&lt;", StringComparison.Ordinal) && innerXml.EndsWith("&gt;", StringComparison.Ordinal))
            {
                return new StringBuilder(innerXml).Unescape().ToString();
            }
            return innerXml;
        }

        public void TryConvert(XmlNodeList children, IList<IDev2Definition> outputDefs, IDictionary<string, int> indexCache, int update, IDSFDataObject dataObj) => TryConvert(children, outputDefs, indexCache, update, dataObj, 0);

        public void TryConvert(XmlNodeList children, IList<IDev2Definition> outputDefs, IDictionary<string, int> indexCache, int update, IDSFDataObject dataObj, int level)
        {
            foreach (XmlNode c in children)
            {
                TryConvertXmlNode(outputDefs, indexCache, ref update, dataObj, ref level, c);
            }
        }

        private void TryConvertXmlNode(IList<IDev2Definition> outputDefs, IDictionary<string, int> indexCache, ref int update, IDSFDataObject dataObj, ref int level, XmlNode c)
        {
            if (level > 0)
            {
                update = TryConvertXmlNodeForLevel(outputDefs, indexCache, update, dataObj, c);
            }
            else
            {
                if (level == 0)
                {
                    TryConvert(c.ChildNodes, outputDefs, indexCache, update, dataObj, ++level);
                }
            }
        }

        private int TryConvertXmlNodeForLevel(IList<IDev2Definition> outputDefs, IDictionary<string, int> indexCache, int update, IDSFDataObject dataObj, XmlNode c)
        {
            var c1 = c;
            var recSetName = outputDefs.Where(definition => definition.RecordSetName == c1.Name);
            var dev2Definitions = recSetName as IDev2Definition[] ?? recSetName.ToArray();
            if (dev2Definitions.Length != 0)
            {
                update = TryConvertXmlNodeForDev2Definition(indexCache, update, dataObj, c, dev2Definitions);
            }
            else
            {
                MapScalarValue(outputDefs, update, dataObj, c1);
            }

            return update;
        }

        private int TryConvertXmlNodeForDev2Definition(IDictionary<string, int> indexCache, int update, IDSFDataObject dataObj, XmlNode c, IDev2Definition[] dev2Definitions)
        {
            var idx = indexCache.TryGetValue(c.Name, out int fetchIdx) ? fetchIdx : 1;
            var nl = c.ChildNodes;
            foreach (XmlNode subc in nl)
            {
                update = TryConvertSubXmlNode(update, dataObj, dev2Definitions, fetchIdx, subc);
            }
            dataObj.Environment.CommitAssign();
            indexCache[c.Name] = ++idx;
            return update;
        }

        private int TryConvertSubXmlNode(int update, IDSFDataObject dataObj, IDev2Definition[] dev2Definitions, int fetchIdx, XmlNode subc)
        {
            foreach (var definition in dev2Definitions)
            {
                if (definition.MapsTo == subc.Name || definition.Name == subc.Name)
                {
                    if (update == 0)
                    {
                        update = fetchIdx;
                    }
                    dataObj.Environment.AssignWithFrame(new AssignValue(definition.RawValue, UnescapeRawXml(subc.InnerXml)), update);
                }
            }

            return update;
        }

        void MapScalarValue(IList<IDev2Definition> outputDefs, int update, IDSFDataObject dataObj, XmlNode c1)
        {
            var scalarName = outputDefs.FirstOrDefault(definition => definition.Name == c1.Name);
            if (scalarName != null)
            {
                dataObj.Environment.AssignWithFrame(new AssignValue(DataListUtil.AddBracketsToValueIfNotExist(scalarName.RawValue), UnescapeRawXml(c1.InnerXml)), update);
            }
        }
    }
}