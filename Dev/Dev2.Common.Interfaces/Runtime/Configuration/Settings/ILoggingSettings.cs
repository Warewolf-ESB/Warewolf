using System.Collections.ObjectModel;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Common.Interfaces.ComponentModel;

namespace Dev2.Common.Interfaces.Runtime.Configuration.Settings
{
    public interface ILoggingSettings : INotifyPropertyChangedEx
    {
        bool RunPostWorkflow { get; set; }
        bool IsLoggingEnabled { get; set; }
        bool IsVersionLogged { get; set; }
        bool IsTypeLogged { get; set; }
        bool IsDurationLogged { get; set; }
        bool IsDataAndTimeLogged { get; set; }
        bool IsInputLogged { get; set; }
        bool IsOutputLogged { get; set; }
        bool LogAll { get; set; }
        bool IsInitializing { get; set; }
        int NestedLevelCount { get; set; }
        string LogFileDirectory { get; set; }
        string ServiceInput { get; set; }
        IWorkflowDescriptor PostWorkflow { get; set; }
        ObservableCollection<IWorkflowDescriptor> Workflows { get; }
        string Error { get; set; }
        XElement ToXml();
    }
}