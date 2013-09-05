
namespace Dev2.Activities.Designers.DsfMultiAssign
{
    public partial class DsfMultiAssignActivityTemplate
    {
        public DsfMultiAssignActivityTemplate()
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
