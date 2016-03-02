namespace Dev2.Common.Interfaces.ToolBase
{
    public interface IWebServicePutViewModel : IWebServiceBaseViewModel
    {
        IWebPutInputArea InputArea { get; set; }
    }
}