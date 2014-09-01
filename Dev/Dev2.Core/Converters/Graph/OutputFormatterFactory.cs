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
