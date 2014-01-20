using System.Windows;
using System.Windows.Input;
using Dev2.Studio.Core.ViewModels.Base;

namespace Dev2.Settings
{
    public abstract class SettingsItemViewModel : DependencyObject
    {
        protected SettingsItemViewModel()
        {
            CloseHelpCommand = new RelayCommand(o => CloseHelp(), o => true);
        }

        public ICommand CloseHelpCommand { get; private set; }

        public string HelpText
        {
            get { return (string)GetValue(HelpTextProperty); }
            set { SetValue(HelpTextProperty, value); }
        }

        public static readonly DependencyProperty HelpTextProperty = DependencyProperty.Register("HelpText", typeof(string), typeof(SettingsItemViewModel), new PropertyMetadata(null));

        public bool IsDirty
        {
            get { return (bool)GetValue(IsDirtyProperty); }
            set { SetValue(IsDirtyProperty, value); }
        }

        public static readonly DependencyProperty IsDirtyProperty = DependencyProperty.Register("IsDirty", typeof(bool), typeof(SettingsItemViewModel), new PropertyMetadata(false));

        protected abstract void CloseHelp();
    }
}