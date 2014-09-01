using Dev2.Common.Interfaces.Core.Graph;
using Unlimited.Framework.Converters.Graph.Ouput;

namespace Unlimited.Framework.Converters.Graph
{
    public class OutputDescriptionFactory
    {
        public static IOutputDescription CreateOutputDescription(OutputFormats format)
        {
            return new OutputDescription
            {
                Format = format,
            };
        }
    }
}
