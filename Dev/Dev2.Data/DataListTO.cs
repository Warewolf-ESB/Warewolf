using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Util;

namespace Dev2.Data
{
    public class DataListTO
    {
        public DataListTO(string dataList)
        {
            Inputs = new List<string>();
            Outputs = new List<string>();
            using (var stringReader = new StringReader(dataList))
            {
                var xDoc = XDocument.Load(stringReader);

                var rootEl = xDoc.Element("DataList");

                if(rootEl != null)
                {   
                    Inputs.AddRange(                     
                        rootEl.Elements().Where(el =>
                        {
                            var firstOrDefault = el.Attributes("ColumnIODirection").FirstOrDefault();
                            var removeCondition = firstOrDefault != null &&
                                                  (firstOrDefault.Value == enDev2ColumnArgumentDirection.Input.ToString() ||
                                                   firstOrDefault.Value == enDev2ColumnArgumentDirection.Both.ToString());
                            return (removeCondition && !el.HasElements);
                        }).Select(element => element.Name.ToString()));
                        
                    Outputs.AddRange(
                        rootEl.Elements().Where(el =>
                        {
                            var firstOrDefault = el.Attributes("ColumnIODirection").FirstOrDefault();
                            var removeCondition = firstOrDefault != null &&
                                                  (firstOrDefault.Value == enDev2ColumnArgumentDirection.Output.ToString() ||
                                                   firstOrDefault.Value == enDev2ColumnArgumentDirection.Both.ToString());
                            return (removeCondition && !el.HasElements);
                        }).Select(element => element.Name.ToString()));
                }

                if(rootEl != null)
                {
                    var xElements = rootEl.Elements().Where(el => el.HasElements);
                    var enumerable = xElements as IList<XElement> ?? xElements.ToList();
                    Inputs.AddRange(enumerable.Elements().Select(element =>
                    {
                        var xAttribute = element.Attributes("ColumnIODirection").FirstOrDefault();
                        var include = xAttribute != null &&
                                      (xAttribute.Value == enDev2ColumnArgumentDirection.Input.ToString() ||
                                       xAttribute.Value == enDev2ColumnArgumentDirection.Both.ToString());
                        if (include)
                        {
                            if (element.Parent != null)
                            {
                                return DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.CreateRecordsetDisplayValue(element.Parent.Name.ToString(), element.Name.ToString(), "*"));
                            }
                        }
                        return "";
                    }));

                    Outputs.AddRange(enumerable.Elements().Select(element =>
                    {
                        var xAttribute = element.Attributes("ColumnIODirection").FirstOrDefault();
                        var include = xAttribute != null &&
                                      (xAttribute.Value == enDev2ColumnArgumentDirection.Output.ToString() ||
                                       xAttribute.Value == enDev2ColumnArgumentDirection.Both.ToString() );
                        if(include)
                        {
                            if(element.Parent != null)
                            {
                                return DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.CreateRecordsetDisplayValue(element.Parent.Name.ToString(), element.Name.ToString(), "*"));
                            }
                        }
                        return "";
                    }));
                }
            }
            Inputs.RemoveAll(string.IsNullOrEmpty);
            Outputs.RemoveAll(string.IsNullOrEmpty);
        }

        public List<string> Outputs { get; set; }

        public List<string> Inputs { get; set; }
    }
}