#pragma warning disable
﻿using Dev2.Common.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using System;
using System.Windows.Input;

namespace Dev2.Common
{
    public class ServerSource : IServerSource, IEquatable<ServerSource>
    {
        public ServerSource()
        {
        }

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


        public bool Equals(ServerSource other)
        {
            if (other == null)
            {
                return false;
            }
            if (!string.Equals(Address, other.Address, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            if (AuthenticationType != other.AuthenticationType)
            {
                return false;
            }
            if (!string.Equals(UserName, other.UserName, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            if (!string.Equals(Password, other.Password))
            {
                return false;
            }
            if (!string.Equals(Name, other.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            return true;
        }

        public bool Equals(IServerSource other)
        {
            if (other == null)
            {
                return false;
            }
            if (!string.Equals(Address, other.Address, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            if (AuthenticationType != other.AuthenticationType)
            {
                return false;
            }
            if (!string.Equals(UserName, other.UserName, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            if (!string.Equals(Password, other.Password))
            {
                return false;
            }
            if (!string.Equals(Name, other.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            return true;
        }
    }
}