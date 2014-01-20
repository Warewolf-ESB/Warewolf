using System;
using System.ComponentModel;
using Dev2.Communication;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Interfaces
{
    public interface IContextualResourceModel : IResourceModel, INotifyPropertyChanged
    {
        IEnvironmentModel Environment { get; }
        Guid ServerID { get; set; }
        void UpdateIconPath(string iconPath);
        bool IsNewWorkflow { get; set; }
        event Action<IContextualResourceModel> OnResourceSaved;
        event Action OnDataListChanged;
        event EventHandler<DesignValidationMemo> OnDesignValidationReceived;
    }
}
