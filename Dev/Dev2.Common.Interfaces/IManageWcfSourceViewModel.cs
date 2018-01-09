using System;
using System.Windows.Input;

namespace Dev2.Common.Interfaces
{
    public interface IManageWcfSourceViewModel
    {
        string EndpointUrl { get; set; }
        ICommand TestCommand { get; set; }
        ICommand CancelTestCommand { get; set; }
        string TestMessage { get; }
        ICommand SaveCommand { get; set; }
        string HeaderText { get; set; }
        bool TestPassed { get; set; }
        bool TestFailed { get; set; }
        bool Testing { get; }
        string ResourceName { get; set; }
    }

    public interface IWcfSourceModel
    {
        void TestConnection(IWcfServerSource resource);
        void Save(IWcfServerSource toSource);
        string ServerName { get; set; }
        IWcfServerSource FetchSource(Guid resourceID);
    }
}
