using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dev2.Common;

namespace Dev2.DataList.Contract
{
    public static class ValueExtractor {

        public static string GetValueFromDataList(string valueReference, object dataList) {
            try {
                XElement evaluationPayload = XElement.Parse(dataList as string);
                string retval = string.Empty;
                if (!(evaluationPayload.HasElements)) {
                    if (evaluationPayload.Name == valueReference) {
                        retval = evaluationPayload.Value;
                    }
                }
                else {
                    IEnumerable<XElement> value = evaluationPayload.Descendants().Where(c => c.Name == (valueReference));//.FirstOrDefault().Value.ToString();
                    if (value.Count() == 0 && (evaluationPayload.Descendants().Count() == 1)) {
                        retval = GetValueFromDataList(valueReference, XElement.Parse(evaluationPayload.FirstNode.ToString(SaveOptions.None)));
                    }
                    else if (value.Count() == 0 && (evaluationPayload.Descendants().Count() > 1)) {
                        foreach (XElement elem in evaluationPayload.Descendants()) {
                            retval = GetValueFromDataList(valueReference, elem);
                        }
                    }
                    else {
                        retval = value.FirstOrDefault().Value;
                    }
                }

                return retval;
            }
            catch(Exception ex) {
                ServerLogger.LogError(ex);
                return string.Empty;
            }
        }

        public static string RemoveDelimiting(string objectToCleanup) {
            while ((objectToCleanup.Contains("&lt;")) && (objectToCleanup.Contains("&gt;"))) {
                objectToCleanup = objectToCleanup.Replace("&lt;", "<").Replace("&gt;", ">");
            }

            return objectToCleanup;
        }
    }
}
