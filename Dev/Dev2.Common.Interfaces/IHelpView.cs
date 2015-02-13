using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.Common.Interfaces
{
    public interface IHelpView : IView {
        string GetCurrentHelpText();
    }
}