using System;
using System.Windows.Input;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Common.Interfaces.Core
{
    public class ServerSource : IServerSource, IEquatable<ServerSource>
    {
        public ServerSource()
        {
        }

        #region Implementation of IServerSource

        public string ServerName { get; set; }
        public string Address { get; set; }
        public AuthenticationType AuthenticationType { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public ICommand TestCommand { get; set; }
        public string TestMessage { get; set; }
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string ResourcePath { get; set; }

        #endregion

        #region Implementation of IEquatable<IServerSource>

        public bool Equals(IServerSource other) => Equals(other as ServerSource);

        #endregion

        public bool Equals(ServerSource other) => string.Equals(Address, other.Address, StringComparison.InvariantCultureIgnoreCase) && AuthenticationType == other.AuthenticationType && string.Equals(UserName, other.UserName, StringComparison.InvariantCultureIgnoreCase) && string.Equals(Password, other.Password) && string.Equals(Name, other.Name, StringComparison.InvariantCultureIgnoreCase);
    }
}