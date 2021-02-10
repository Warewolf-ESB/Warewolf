using Dev2.Common.Interfaces.ToolBase;

namespace Dev2.Common.Interfaces
{
    public interface IWebServiceParametersBuilder
    {
        void BuildParameters(IParameterRegion region, string content);
    }
}
