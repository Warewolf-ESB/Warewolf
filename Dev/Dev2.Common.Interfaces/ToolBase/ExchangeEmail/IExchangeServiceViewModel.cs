namespace Dev2.Common.Interfaces.ToolBase.ExchangeEmail
{
    public interface IExchangeServiceViewModel
    {
        ISourceToolRegion<IExchangeSource> SourceRegion { get; set; }
    }
}
