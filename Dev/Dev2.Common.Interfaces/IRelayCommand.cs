using System.Windows.Input;

namespace Dev2.Common.Interfaces
{
    public interface IRelayCommand : ICommand
    {
        void RaiseCanExecuteChanged();
    }
}