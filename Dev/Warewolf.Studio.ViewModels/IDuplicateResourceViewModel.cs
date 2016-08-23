using System.Windows.Input;

namespace Warewolf.Studio.ViewModels
{
    public interface IDuplicateResourceViewModel
    {
        ICommand CancelCommand { get; }
        ICommand CreateCommand { get; }
        bool FixReferences { get; }
        string NewResourceName { get; }
    }
}