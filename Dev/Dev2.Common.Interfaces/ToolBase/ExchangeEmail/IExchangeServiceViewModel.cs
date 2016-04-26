namespace Dev2.Common.Interfaces.ToolBase.ExchangeEmail
{
    public interface IExchangeServiceViewModel
    {
        ISourceToolRegion<IExchangeSource> SourceRegion { get; set; }
        void SetDisplayName(string displayName);
        bool GenerateOutputsVisible { get; set; }
    }
}
