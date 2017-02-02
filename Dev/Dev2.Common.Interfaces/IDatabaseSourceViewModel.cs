using System.Collections.Generic;
using System.Windows.Input;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.Common.Interfaces
{
    public interface IDatabaseSourceViewModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        AuthenticationType AuthenticationType { get; set; }
        ComputerName ServerName { get; set; }
        string EmptyServerName { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        ICommand TestCommand { get; set; }
        ICommand CancelTestCommand { get; set; }
        ICommand OkCommand { get; set; }
        string TestMessage { get; set; }
        string HeaderText { get; set; }
        bool TestPassed { get; set; }
        bool TestFailed { get; set; }
        bool Testing { get; }
        string ResourceName { get; set; }
        IList<ComputerName> ComputerNames { get; set; }
        bool UserAuthenticationSelected { get; }
        bool CanSelectWindows { get; set; }
        bool CanSelectUser { get; set; }
        bool CanSelectServer { get; set; }
    }
}