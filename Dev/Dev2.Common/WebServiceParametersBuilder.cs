/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ToolBase;

namespace Dev2.Common
{
    public class WebServiceParametersBuilder : IWebServiceParametersBuilder
    {
        public void BuildParameters(IParameterRegion region, string content)
        {
            var hasEmptyParameters = region.Parameters?.Any(value => !string.IsNullOrEmpty(value.Name)) ?? false;
            if (hasEmptyParameters)
            {
                return;
            }

            var isValidJson = content?.IsValidJson() ?? false;
            if (isValidJson)
            {
                var jsonHeader = new NameValue(GlobalConstants.ContentType, GlobalConstants.ApplicationJsonHeader);

                region.Parameters = new ObservableCollection<INameValue> { jsonHeader, new NameValue() };
            }
            else
            {
                var isValidXml = content.IsValidXml();
                if (isValidXml)
                {
                    var jsonHeader = new NameValue(GlobalConstants.ContentType, GlobalConstants.ApplicationXmlHeader);

                    region.Parameters = new ObservableCollection<INameValue> { jsonHeader, new NameValue() };
                }
            }
        }
    }
}
