using Dev2.Common.Interfaces.Toolbox;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels.ToolBox
{
    public class ToolDescriptorViewModel : BindableBase,IToolDescriptorViewModel
    {
        IToolDescriptor _tool;
        bool _isEnabled;

        public ToolDescriptorViewModel(IToolDescriptor tool, bool isEnabled)
        {
            IsEnabled = isEnabled;
            Tool = tool;
        }

        #region Implementation of IToolDescriptorViewModel

        public IToolDescriptor Tool
        {
            get
            {
                return _tool;
            }
            private set
            {
                OnPropertyChanged("Tool");
                _tool = value;
            }
        }
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            private set
            {
                OnPropertyChanged("IsEnabled");
                _isEnabled = value;
            }
        }

        #endregion
    }
}
