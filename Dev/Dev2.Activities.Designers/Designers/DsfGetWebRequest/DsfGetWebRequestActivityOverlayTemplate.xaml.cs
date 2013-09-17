using System.Windows;
using System.Windows.Input;

namespace Dev2.Activities.Designers.DsfGetWebRequest
{
    /// <summary>
    ///     Interaction logic for DsfGetWebRequestActivityOverlayTemplate.xaml
    /// </summary>
    public partial class DsfGetWebRequestActivityOverlayTemplate
    {
        public DsfGetWebRequestActivityOverlayTemplate()
        {
            InitializeComponent();
        }

        #region Overrides of ActivityTemplate

        public override IActivityViewModelBase ActivityViewModelBase { get; set; }

        #endregion

        #region Events

        private void UrlOnLoaded(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            element.Focus();  
        }

        #endregion
    }
}