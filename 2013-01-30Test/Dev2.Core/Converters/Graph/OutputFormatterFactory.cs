using Unlimited.Framework.Converters.Graph.Interfaces;
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
    }
}
