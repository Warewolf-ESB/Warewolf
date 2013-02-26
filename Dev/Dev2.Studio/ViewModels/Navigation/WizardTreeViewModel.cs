#region

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
        /// <param name="parent">The parent.</param>
        /// <param name="dataContext">The data context.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public WizardTreeViewModel(ITreeNode parent, IContextualResourceModel dataContext) : 
            this(parent, dataContext, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WizardTreeViewModel" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="dataContext">The data context.</param>
        /// <param name="activityFullName">Full name of the activity.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public WizardTreeViewModel(ITreeNode parent, IContextualResourceModel dataContext, string activityFullName) : 
            base(parent, dataContext, activityFullName)
        {
        }
    }
}
