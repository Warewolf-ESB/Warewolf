using Dev2.Enums;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    // Place holder interface used to locate model items in WorkflowDesignerViewModel using a string which must be a guid
    public interface IDev2Activity
    {
        /// <summary>
        /// UniqueID is the InstanceID and MUST be a guid.
        /// </summary>
        string UniqueID { get; set; }

        enFindMissingType GetFindMissingType();
    }
}