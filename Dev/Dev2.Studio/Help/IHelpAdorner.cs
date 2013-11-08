using System.Windows.Input;

namespace Dev2.Help
{
    public interface IHelpAdorner
    {
        ICommand CloseHelpCommand { get; }
        string HelpText { get; }
    }
}