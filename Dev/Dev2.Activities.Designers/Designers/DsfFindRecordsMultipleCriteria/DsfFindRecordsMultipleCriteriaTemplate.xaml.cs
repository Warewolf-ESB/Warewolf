using System.Windows.Controls;

namespace Dev2.Activities.Designers.DsfFindRecordsMultipleCriteria
{
    /// <summary>
    /// Interaction logic for DsfFindRecordsMultipleCriteriaTemplate.xaml
    /// </summary>
    public partial class DsfFindRecordsMultipleCriteriaTemplate
    {
        public DsfFindRecordsMultipleCriteriaTemplate()
        {
            InitializeComponent();
        }

        #region Overrides of ActivityTemplate

        public override IActivityViewModelBase ActivityViewModelBase
        {
            get
            {
                return (IActivityViewModelBase)DataContext;
            }
            set { }
        }

        #endregion
    }
}
