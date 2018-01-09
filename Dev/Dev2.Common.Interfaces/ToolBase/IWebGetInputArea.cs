namespace Dev2.Common.Interfaces.ToolBase
{
    public interface IWebGetInputArea : IToolRegion,IHeaderRegion
    {
        string QueryString { get; set; }
        string RequestUrl { get; set; }
    }
}