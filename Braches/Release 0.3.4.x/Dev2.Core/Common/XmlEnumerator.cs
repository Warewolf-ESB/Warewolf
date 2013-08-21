using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Dev2 {
    /// <summary>
    /// Author:     Sameer Chunilall
    /// Date:       2011-08-12
    /// Provides a generic list of Key-Value pairs that make up an XElement
    /// IsWebAccessEnabled recursive so the entire XElement graph will be visited.
    /// </summary>
    public class XElementEnumerator {
        public XElementEnumerator() {

        }

        public List<KeyValuePair<string, string>> EnumerateXElementTree(XElement xmlData) {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();

            foreach (var element in xmlData.Descendants()) {
                if (element.HasElements) {
                    EnumerateXElementTree(element);

                }
                else {

                    //string elementValue = string.Empty;
                    //if (!element.HasElements && element.HasAttributes) {
                    //    elementValue = element.ToString();
                    //}
                    //else {
                    //    elementValue = element.Value;
                    //}


                    list.Add(new KeyValuePair<string, string>(element.Name.ToString(), element.Value));
                }
            }

            return list;
        }
    }
}
