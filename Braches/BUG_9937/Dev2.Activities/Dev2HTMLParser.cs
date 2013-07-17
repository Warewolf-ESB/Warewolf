using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2;
using System.Activities;
using System.IO;

namespace Unlimited.Applications.BusinessDesignStudio.Activities {
    public static class Dev2HTMLParser {

        /*public static string ParseHTML(string html, string bindingData, IEsbChannel dsfChannel) {
            if (dsfChannel == null) {
                throw new ArgumentNullException("dsfChannel");
            }

            if (string.IsNullOrEmpty(html)) {
                throw new ArgumentNullException("html");
            }

            UnlimitedObject bindingContext = null;
            if (!string.IsNullOrEmpty(bindingData)) {
                bindingContext = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(bindingData);
            }

            string replacedHtml = html;

            UnlimitedObject tags = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(html.ToLower());

            replacedHtml = tags.XmlString.ToLower();

            var tagItems = tags.GetAllElements("dev2html");

            foreach (dynamic tag in tagItems) {
                string name = null;
                string type = null;

                if (tag.name is string) {
                    name = tag.name;
                }

                if (tag.type is string) {
                    type = tag.type;
                }

                List<string> exclusions = new List<string>() { "form", "meta", "pagetitle" };

                if (!string.IsNullOrEmpty(type)) {
                    if (!exclusions.Contains(type.ToLower())) {
                        if ((bindingContext != null) && !string.IsNullOrEmpty(name)) {
                            if (!string.IsNullOrEmpty(type)) {
                                string instruction = UnlimitedObject.GenerateServiceRequest(
                                        type,
                                        string.Empty,
                                        new List<string> { bindingContext.GetElement(name).XmlString }
                                );

                                UnlimitedObject result = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(dsfChannel.ExecuteCommand(instruction));
                                if (!result.HasError) {
                                    replacedHtml = replacedHtml.Replace(tag.XmlString, result.XPath("//Fragment/text()").Inner());
                                }
                            }
                        }
                    }
                }
            }

            return replacedHtml;
        }*/

    }
}
