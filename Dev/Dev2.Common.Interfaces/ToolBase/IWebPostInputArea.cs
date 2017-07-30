namespace Dev2.Common.Interfaces.ToolBase
{
    public interface IWebPostInputArea : IToolRegion, IHeaderRegion
    {
        string PostData { get; set; }
        string QueryString { get; set; }
        string RequestUrl { get; set; }

    }
}
