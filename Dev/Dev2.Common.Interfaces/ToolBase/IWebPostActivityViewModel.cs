namespace Dev2.Common.Interfaces.ToolBase
{
    public interface IWebPostActivityViewModel : IWebServiceBaseViewModel
    {
        IWebPostInputArea InputArea { get; set; }

        IParameterRegion GetParameterRegion();
    }

}
