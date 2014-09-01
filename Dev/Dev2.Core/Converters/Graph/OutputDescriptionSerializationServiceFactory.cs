using Dev2.Common.Interfaces.Core.Graph;
using Unlimited.Framework.Converters.Graph.Output;

namespace Unlimited.Framework.Converters.Graph
{
    public class OutputDescriptionSerializationServiceFactory
    {
        public static IOutputDescriptionSerializationService CreateOutputDescriptionSerializationService()
        {
            return new OutputDescriptionSerializationService();
        }
    }
}
