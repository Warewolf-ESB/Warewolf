namespace Dev2.Common.Interfaces.ToolBase
{
    public interface IWebServiceGetViewModel: IWebServiceBaseViewModel
    {
        IWebGetInputArea InputArea { get; set; }
    }
}