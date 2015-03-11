/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Core.Graph;
using Unlimited.Framework.Converters.Graph.Ouput;

namespace Unlimited.Framework.Converters.Graph
{
    public class OutputFormatterFactory
    {
        public static IOutputFormatter CreateOutputFormatter(IOutputDescription outputDescription)
        {
            IOutputFormatter outputFormatter;

            if (outputDescription.Format == OutputFormats.ShapedXML)
            {
                outputFormatter = new ShapedXmlOutputFormatter(outputDescription);
            }
            else
            {
                outputFormatter = null;
            }

            return outputFormatter;
        }

        public static IOutputFormatter CreateOutputFormatter(IOutputDescription outputDescription, string rootNodeName)
        {
            IOutputFormatter outputFormatter;

            if (outputDescription.Format == OutputFormats.ShapedXML)
            {
                outputFormatter = new ShapedXmlOutputFormatter(outputDescription, rootNodeName);
            }
            else
            {
                outputFormatter = null;
            }

            return outputFormatter;
        }
    }
}