#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
            var fixedDataList = dataList.Replace(GlobalConstants.SerializableResourceQuote, "\"")
                                        .Replace(GlobalConstants.SerializableResourceSingleQuote, "\'");

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
                        LoadInputsFromAllElements(Inputs, rootEl);
                    }
                    else
                    {
                        LoadInputsAndOutputs(rootEl);
                    }
                }
            }

            Inputs.RemoveAll(string.IsNullOrEmpty);
            Outputs.RemoveAll(string.IsNullOrEmpty);
        }

        void LoadInputsAndOutputs(XElement rootEl)
        {
            if (rootEl is null)
            {
                return;
            }

            SetInputAttributesContainingColumnIoDirectionAndIsJson(rootEl);

            SetOutputAttributesContainingColumnIoDirectionAndIsJson(rootEl);

            var xElements = rootEl.Elements().Where(el => el.HasElements);
            var enumerable = xElements as IList<XElement> ?? xElements.ToList();

            SetInputAttributesContainingColumnIoDirection(enumerable);
            SetOutputAttributesContainingColumnIoDirection(enumerable);
        }

        private void SetOutputAttributesContainingColumnIoDirection(IList<XElement> enumerable)
        {
            var outputs = enumerable.Elements()
                .Select(element =>
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
                });
            Outputs.AddRange(outputs);
        }

        private void SetInputAttributesContainingColumnIoDirection(IList<XElement> enumerable)
        {
            Inputs.AddRange(enumerable.Elements()
                .Select(element =>
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
                })
                .Where(o => !string.IsNullOrEmpty(o)));
        }

        private void SetOutputAttributesContainingColumnIoDirectionAndIsJson(XElement rootEl)
        {
            var outputs = rootEl.Elements()
                                .Where(el =>
                                {
                                    var firstOrDefault = el.Attributes("ColumnIODirection").FirstOrDefault();
                                    var isJson = el.Attribute("IsJson")?.Value.ToLower() == "true" ? true : false;

                                    var removeCondition = firstOrDefault != null &&
                                                            (firstOrDefault.Value == enDev2ColumnArgumentDirection.Output.ToString() ||
                                                            firstOrDefault.Value == enDev2ColumnArgumentDirection.Both.ToString());
                                    return removeCondition && (!el.HasElements || isJson);
                                })
                                .Select(element => element.Name.ToString());

            Outputs.AddRange(outputs);
        }

        private void SetInputAttributesContainingColumnIoDirectionAndIsJson(XElement rootEl)
        {
            var inputs = rootEl.Elements()
                               .Where(el =>
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
                                })
                                .Select(element =>
                                {
                                    var name = element.Name.ToString();
                                    return name;
                                });

            Inputs.AddRange(inputs);
        }

        static void LoadInputsFromAllElements(List<string> inputs, XElement rootEl)
        {
            if (rootEl is null)
            {
                return;
            }

            var inputsWithNoElements = rootEl.Elements()
                                            .Where(element => !element.HasElements)
                                            .Select(element => element.Name.ToString());
            inputs.AddRange(inputsWithNoElements);

            var elementsWithElements = rootEl.Elements().Where(el => el.HasElements);
            var enumerable = elementsWithElements as IEnumerable<XElement> ?? elementsWithElements.ToList();
            inputs.AddRange(elementsWithElements.Elements().Select(element =>
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