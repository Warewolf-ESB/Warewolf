#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.Util;

namespace Dev2.Data
{
    public class DataListTO
    {
        public DataListTO(string dataList)
            : this(dataList, false)
        {
        }

        public DataListTO(string dataList, bool ignoreColumnDirection)
        {
            var fixedDataList = dataList.Replace(GlobalConstants.SerializableResourceQuote, "\"").Replace(GlobalConstants.SerializableResourceSingleQuote, "\'");
            Inputs = new List<string>();
            Outputs = new List<string>();
            using (var stringReader = new StringReader(fixedDataList))
            {
                var xDoc = XDocument.Load(stringReader);

                var rootEl = xDoc.Element("DataList");

                if (rootEl != null)
                {
                    if (ignoreColumnDirection)
                    {
                        Map(rootEl);
                    }
                    else
                    {
                        MapForInputOutput(rootEl);
                    }
                }
            }
            Inputs.RemoveAll(string.IsNullOrEmpty);
            Outputs.RemoveAll(string.IsNullOrEmpty);
        }

        void MapForInputOutput(XElement rootEl)
        {
            if (rootEl == null)
            {
                return;
            }
            Inputs.AddRange(
                rootEl.Elements().Where(el =>
                {
                    var ioDirection = el.Attributes("ColumnIODirection").FirstOrDefault();
                    var isJsonAttribute = el.Attribute("IsJson");
                    var isJson = false;
                    if (isJsonAttribute != null)
                    {
                        isJson = isJsonAttribute.Value.ToLower() == "true";
                    }
                    var removeCondition = ioDirection != null &&
                                          (ioDirection.Value == enDev2ColumnArgumentDirection.Input.ToString() ||
                                           ioDirection.Value == enDev2ColumnArgumentDirection.Both.ToString());
                    return removeCondition && (!el.HasElements || isJson);
                }).Select(element =>
                {
                    var name = element.Name.ToString();                   
                    return name;
                    
                }));

            Outputs.AddRange(
                rootEl.Elements().Where(el =>
                {
                    var firstOrDefault = el.Attributes("ColumnIODirection").FirstOrDefault();
                    var isJsonAttribute = el.Attribute("IsJson");
                    var isJson = false;
                    if (isJsonAttribute != null)
                    {
                        isJson = isJsonAttribute.Value.ToLower() == "true";
                    }
                    var removeCondition = firstOrDefault != null &&
                                          (firstOrDefault.Value == enDev2ColumnArgumentDirection.Output.ToString() ||
                                           firstOrDefault.Value == enDev2ColumnArgumentDirection.Both.ToString());
                    return removeCondition && (!el.HasElements || isJson);
                }).Select(element => element.Name.ToString()));

            var xElements = rootEl.Elements().Where(el => el.HasElements);
            var enumerable = xElements as IList<XElement> ?? xElements.ToList();
            Inputs.AddRange(enumerable.Elements().Select(element =>
            {
                var xAttribute = element.Attributes("ColumnIODirection").FirstOrDefault();
                var include = xAttribute != null &&
                              (xAttribute.Value == enDev2ColumnArgumentDirection.Input.ToString() ||
                               xAttribute.Value == enDev2ColumnArgumentDirection.Both.ToString());
                if (include && element.Parent != null)
                {
                    return DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.CreateRecordsetDisplayValue(element.Parent.Name.ToString(), element.Name.ToString(), "*"));
                }

                return "";
            }));

            Outputs.AddRange(enumerable.Elements().Select(element =>
            {
                var xAttribute = element.Attributes("ColumnIODirection").FirstOrDefault();
                var include = xAttribute != null &&
                              (xAttribute.Value == enDev2ColumnArgumentDirection.Output.ToString() ||
                               xAttribute.Value == enDev2ColumnArgumentDirection.Both.ToString());
                if (include && element.Parent != null)
                {
                    return DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.CreateRecordsetDisplayValue(element.Parent.Name.ToString(), element.Name.ToString(), "*"));
                }

                return "";
            }));
        }
        
        void Map(XElement rootEl)
        {
            if (rootEl == null)
            {
                return;
            }
            Inputs.AddRange(rootEl.Elements().Where(element => !element.HasElements).Select(element => element.Name.ToString()));
            var xElements = rootEl.Elements().Where(el => el.HasElements);
            var enumerable = xElements as IList<XElement> ?? xElements.ToList();
            Inputs.AddRange(enumerable.Elements().Select(element =>
            {
                if (element.Parent != null)
                {
                    return DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.CreateRecordsetDisplayValue(element.Parent.Name.ToString(), element.Name.ToString(), "*"));
                }
                return "";
            }));
        }

        public List<string> Outputs { get; set; }

        public List<string> Inputs { get; set; }
    }
}