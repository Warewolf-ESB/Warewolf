using Dev2.Common.Interfaces.ToolBase;

namespace Dev2.Common.Interfaces
{
    public interface IWebServiceHeaderBuilder
    {
        void BuildHeader(IHeaderRegion region, string content);
    }
}
