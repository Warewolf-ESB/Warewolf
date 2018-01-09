namespace Dev2.Common.Interfaces.ToolBase
{
    public interface IWebPutInputArea : IToolRegion, IHeaderRegion
    {
        string QueryString { get; set; }
        string RequestUrl { get; set; }
        string PutData { get; set; }

    }
}