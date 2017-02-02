using System.Collections.Generic;
using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.Common.Interfaces
{
    public interface IFileChooserView : IView
    {
        void ShowView(IList<string> files);
        void RequestClose();
        void ShowView(IList<string> files, string filter);
        void ShowView(bool allowMultipleSelection);
    }
}
