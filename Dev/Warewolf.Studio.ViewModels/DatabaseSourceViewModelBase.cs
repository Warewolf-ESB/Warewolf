using System.Collections.Generic;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Warewolf.Studio.ViewModels
{
    public interface IDatabaseSourceViewModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        NameValue ServerType { get; set; }
        AuthenticationType AuthenticationType { get; set; }
        ComputerName ServerName { get; set; }
        string EmptyServerName { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        ICommand TestCommand { get; set; }
        ICommand CancelTestCommand { get; set; }
        ICommand OkCommand { get; set; }
        string TestMessage { get; }
        string HeaderText { get; set; }
        bool TestPassed { get; set; }
        bool TestFailed { get; set; }
        bool Testing { get; }
        string ResourceName { get; set; }
        IList<ComputerName> ComputerNames { get; set; }
    }
    public abstract class DatabaseSourceViewModelBase : SourceBaseImpl<IDbSource>, IDatabaseSourceViewModel
    {
        public DatabaseSourceViewModelBase(string image)
            : base(image)
        {
        }

        #region Implementation of IDatabaseSourceViewModel

        public NameValue ServerType { get; set; }
        public AuthenticationType AuthenticationType { get; set; }
        public ComputerName ServerName { get; set; }
        public string EmptyServerName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public ICommand TestCommand { get; set; }
        public ICommand CancelTestCommand { get; set; }
        public ICommand OkCommand { get; set; }
        public string TestMessage { get; }
        public string HeaderText { get; set; }
        public bool TestPassed { get; set; }
        public bool TestFailed { get; set; }
        public bool Testing { get; }
        public string ResourceName { get; set; }
        public IList<ComputerName> ComputerNames { get; set; }

        #endregion
    }
}