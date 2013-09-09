using System.Windows;
using Dev2.Activities.Designers;

namespace Dev2.Activities.QuickVariableInput
{
    /// <summary>
    /// Interaction logic for QuickVariableInputView.xaml
    /// </summary>
    public partial class QuickVariableInputView
    {
        IActivityViewModelBase _activityViewModelBase;

        public QuickVariableInputView()
        {
            InitializeComponent();
        }
        public QuickVariableInputViewModel QuickVariableInputViewModel
        {
            get
            {
                return (QuickVariableInputViewModel)DataContext;
            }
            set
            {
                DataContext = value;
            }
        }

        public override IActivityViewModelBase ActivityViewModelBase
        {
            get
            {
                return _activityViewModelBase;
            }
            set
            {
                _activityViewModelBase = value;
                QuickVariableInputViewModel.ActivityViewModelBase = value;
            }
        }

        void VarialbeListOnLoaded(object sender, RoutedEventArgs args)
        {
            var element = (FrameworkElement)sender;
            element.Focus();
        }
    }
}
