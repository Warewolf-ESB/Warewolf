
namespace Dev2.Activities.Designers.DsfMultiAssign
{
    public partial class DsfMultiAssignActivityOverLayTemplate
    {
        public DsfMultiAssignActivityOverLayTemplate()
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
            set
            {
            }
        }

        #endregion
    }
}
