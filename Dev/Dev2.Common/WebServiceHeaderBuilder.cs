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
            var hasEmptyHeaders = region.Headers?.Any(value => !string.IsNullOrEmpty(value.Name)) ?? false;
            if (hasEmptyHeaders)
            {
                return;
            }

            var isValidJson = content?.IsValidJson() ?? false;
            if (isValidJson)
            {
                var jsonHeader = new NameValue(GlobalConstants.ContentType, GlobalConstants.ApplicationJsonHeader);

                region.Headers = new ObservableCollection<INameValue> { jsonHeader, new NameValue() };
            }
            else
            {
                var isValidXml = content.IsValidXml();
                if (isValidXml)
                {
                    var jsonHeader = new NameValue(GlobalConstants.ContentType, GlobalConstants.ApplicationXmlHeader);

                    region.Headers = new ObservableCollection<INameValue> { jsonHeader, new NameValue() };
                }
            }
        }
    }
}
