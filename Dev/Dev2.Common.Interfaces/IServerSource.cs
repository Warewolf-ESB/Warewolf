using System;
using System.Windows.Input;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Common.Interfaces
{
    public interface IServerSource:IEquatable<IServerSource>
    {
        string ServerName { get; set; }
        string Address { get; set; }
        AuthenticationType AuthenticationType { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        ICommand TestCommand { get; set; }
        string TestMessage { get; set; }
        Guid ID { get; set; }
        string Name { get; set; }
        string ResourcePath { get; set; }
    }
}
