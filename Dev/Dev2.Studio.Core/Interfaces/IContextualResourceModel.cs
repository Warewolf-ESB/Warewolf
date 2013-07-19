using System;

namespace Dev2.Studio.Core.Interfaces
{
    public interface IContextualResourceModel : IResourceModel
    {
        IEnvironmentModel Environment { get; }
        Guid ServerID { get; set; }
        void UpdateIconPath(string iconPath);
        bool IsNewWorkflow { get; set; }
        string ServerResourceType { get; set; }
        event Action<IContextualResourceModel> OnResourceSaved;
        event Action OnDataListChanged;
    }
}
