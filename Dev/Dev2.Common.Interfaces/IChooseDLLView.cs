using Microsoft.Practices.Prism.Mvvm;
// ReSharper disable InconsistentNaming

namespace Dev2.Common.Interfaces
{
    public interface IChooseDLLView : IView
    {
        void ShowView(IDLLChooser chooser);
        void RequestClose();
    }
}
