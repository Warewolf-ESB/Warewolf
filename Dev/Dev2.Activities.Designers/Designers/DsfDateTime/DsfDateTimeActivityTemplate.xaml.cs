using System.Windows.Input;

namespace Dev2.Activities.Designers.DsfDateTime
{
    // Interaction logic for DsfDateTimeActivityTemplate.xaml
    public partial class DsfDateTimeActivityTemplate
    {
        public DsfDateTimeActivityTemplate()
        {
            InitializeComponent();
        }

        void DsfDateTimeActivityTemplate_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        #region Overrides of ActivityTemplate

        public override IActivityViewModelBase ActivityViewModelBase
        {
            get
            {
                return (IActivityViewModelBase)DataContext;
            } 
            set{}
        }

        #endregion
    }
}
