using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Studio.Core.Wizards.Interfaces
{
    public interface IWizardEngine
    {
        /// <summary>
        /// Gets invocation details for an activity's wizard.
        /// </summary>
        /// <param name="activity">The model item representing the activity.</param>
        /// <param name="hostResource">The resource which hosts the acticity.</param>
        /// <param name="dataListCompiler">The data list compiler.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">The ServiceLocator is null, please ensure that MEF imports are satisfied on this instance of the WizardEngine.</exception>
        WizardInvocationTO GetActivityWizardInvocationTO(ModelItem activity, IContextualResourceModel hostResource, IDataListCompiler dataListCompiler = null);

        /// <summary>
        /// Gets invocation details for an activity's settings wizard.
        /// </summary>
        /// <param name="activity">The model item representing the activity.</param>
        /// <param name="hostResource">The resource which hosts the acticity.</param>
        /// <param name="dataListCompiler">The data list compiler.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">The ServiceLocator is null, please ensure that MEF imports are satisfied on this instance of the WizardEngine.</exception>
        WizardInvocationTO GetActivitySettingsWizardInvocationTO(ModelItem activity, IContextualResourceModel hostResource, IDataListCompiler dataListCompiler = null);

        /// <summary>
        /// Determines whether the specified activity has a wizard.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="hostResource">The host resource.</param>
        /// <returns>
        ///   <c>true</c> if the specified activity has wizard; otherwise, <c>false</c>.
        /// </returns>
        bool HasWizard(ModelItem activity, IEnvironmentModel environmentModel);

        /// <summary>
        /// Command to create a wizard.
        /// </summary>
        /// <param name="parentResource">The parent resource.</param>
        void CreateResourceWizard(IContextualResourceModel parentResource);

        /// <summary>
        /// Command to edit a wizard.
        /// </summary>
        /// <param name="parentResource">The parent resource.</param>
        void EditResourceWizard(IContextualResourceModel parentResource);

        /// <summary>
        /// Edits the wizard.
        /// </summary>
        /// <param name="resource">The resource.</param>
        void EditWizard(IContextualResourceModel resource);     

        /// <summary>
        /// Gets the parent for the give wizard, if the wizard is not a resource wizzard an exception is thrown, if no parent exists null is returned.
        /// </summary>
        /// <param name="wizardResource">The wizard resource.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Can't get a parent for a resource that is not a wizard. The attempt was made on ' + wizardResource.ResourceName + '.</exception>
        IContextualResourceModel GetParent(IContextualResourceModel wizardResource);

        /// <summary>
        /// Gets the wizard for the give resource, if the resource is a wizzard an exception is thrown, if no wizard exists null is returned.
        /// </summary>
        /// <param name="parentResource">The parent resource.</param>
        IContextualResourceModel GetWizard(IContextualResourceModel resource);        

        bool IsWizard(IContextualResourceModel resource);
        bool IsResourceWizard(IContextualResourceModel resource);
        bool IsSystemWizard(IContextualResourceModel resource);
    }
}
