using System.Windows;
using System.Windows.Input;

namespace Dev2.Activities.Designers.DsfGetWebRequest
{
    /// <summary>
    ///     Interaction logic for DsfGetWebRequestActivityTemplate.xaml
    /// </summary>
    public partial class DsfGetWebRequestActivityTemplate
    {
        public DsfGetWebRequestActivityTemplate()
        {
            InitializeComponent();
        }

        #region Overrides of ActivityTemplate

        public override IActivityViewModelBase ActivityViewModelBase { get; set; }

        #endregion

        void FocusedTextBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            element.Focus();
        }
    }
}