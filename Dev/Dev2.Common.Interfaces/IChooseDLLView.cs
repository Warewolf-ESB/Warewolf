using Microsoft.Practices.Prism.Mvvm;


namespace Dev2.Common.Interfaces
{
    public interface IChooseDLLView : IView
    {
        void ShowView(IDLLChooser chooser);
        void RequestClose();
    }
}
