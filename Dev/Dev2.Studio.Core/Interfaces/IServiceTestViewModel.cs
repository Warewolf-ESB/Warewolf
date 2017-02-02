using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core.ViewModels;

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace Dev2.Studio.Core.Interfaces
{
    public interface IServiceTestViewModel : IDisposable,INotifyPropertyChanged, IUpdatesHelp
    {
        IServiceTestModel SelectedServiceTest { get; set; }
        IServiceTestCommandHandler ServiceTestCommandHandler { get; set; }
        string RunAllTestsUrl { get; set; }
        string TestPassingResult { get; set; }
        ObservableCollection<IServiceTestModel> Tests  { get; set; }
        bool IsLoading { get; set; }

        ICommand DuplicateTestCommand { get; set; }
        ICommand DeleteTestCommand { get; set; }
        ICommand RunAllTestsInBrowserCommand { get; set; }
        ICommand RunAllTestsCommand { get; set; }
        ICommand RunSelectedTestInBrowserCommand { get; set; }
        ICommand RunSelectedTestCommand { get; set; }
        ICommand StopTestCommand { get; set; }
        ICommand CreateTestCommand { get; set; }
        string DisplayName { get; set; }
        bool CanSave { get; set; }
        bool IsDirty { get; }
        string ErrorMessage { get; set; }
        IWorkflowDesignerViewModel WorkflowDesignerViewModel { get; set; }
        ICommand DeleteTestStepCommand { get; set; }
        Guid ResourceID { get; }

        void Save();

        bool HasDuplicates();
        void ShowDuplicatePopup();
        void RefreshCommands();
        void PrepopulateTestsUsingDebug(List<IDebugTreeViewItemViewModel> models);
    }
}