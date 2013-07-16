#region

using Dev2.Services;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Navigation;

#endregion

namespace Dev2.Studio.ViewModels.Navigation
{
    /// <summary>
    /// Represents a node in the tree that contains a Wizard Resource
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/01/23</date>
    public class WizardTreeViewModel : ResourceTreeViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WizardTreeViewModel" /> class.
        /// </summary>
        /// <param name="validationService">The validation service.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="dataContext">The data context.</param>
        /// <param name="activityFullName">Full name of the activity.</param>
        /// <date>2013/01/23</date>
        /// <author>
        /// Jurie.smit
        /// </author>
        public WizardTreeViewModel(IDesignValidationService validationService, ITreeNode parent, IContextualResourceModel dataContext, string activityFullName = null) :
            base(validationService, parent, dataContext, activityFullName)
        {
        }
    }
}
