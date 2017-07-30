using System;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ToolBase;

namespace Dev2.Common
{
    public class WebServiceHeaderBuilder : IWebServiceHeaderBuilder
    {
        public void BuildHeader(IHeaderRegion region, string content)
        {

            var isValidJson = content?.IsValidJson() ?? false;
            if (isValidJson)
            {
                var jsonHeader = new NameValue("Content-Type", "application/json");
                SetupHeader(region, jsonHeader);
            }
            else
            {
                var isValidXml = content.IsValidXml();
                if (isValidXml)
                {
                    var jsonHeader = new NameValue("Content-Type", "application/xml");
                    SetupHeader(region, jsonHeader);
                }
            }
        }

        private static void SetupHeader(IHeaderRegion region, NameValue jsonHeader)
        {
            if (region.Headers == null ||
                region.Headers.All(value => string.IsNullOrEmpty(value.Value) && string.IsNullOrEmpty(value.Name)))
            {
                region.Headers = new ObservableCollection<INameValue> { jsonHeader, new NameValue() };
            }
            else
            {
                bool ExistingHeaders(INameValue value) => value.Name.Equals(jsonHeader.Name, StringComparison.InvariantCultureIgnoreCase) &&
                                                          value.Value.Equals(jsonHeader.Value, StringComparison.InvariantCultureIgnoreCase);

                var emptyHeader = region.Headers.FirstOrDefault(value => string.IsNullOrEmpty(value.Name) && string.IsNullOrEmpty(value.Value));
                region.Headers.Remove(emptyHeader);
                if (!region.Headers.Any(ExistingHeaders))
                {
                    region.Headers.Add(jsonHeader);
                }
                region.Headers.Add(emptyHeader);
            }
        }
    }
}