#pragma warning disable
﻿using System;



namespace Dev2.Common.Interfaces.Core
{
    public class RabbitMQServiceSourceDefinition : IRabbitMQServiceSourceDefinition, IEquatable<RabbitMQServiceSourceDefinition>
    {
        public RabbitMQServiceSourceDefinition(IRabbitMQ source)
        {
            ResourceID = source.ResourceID;
            ResourceName = source.ResourceName;
            HostName = source.HostName;
            Port = source.Port;
            UserName = source.UserName;
            ResourcePath = source.GetSavePath();
            Password = source.Password;
            VirtualHost = source.VirtualHost;
        }

        public RabbitMQServiceSourceDefinition()
        {
        }

        #region Equality members
        
        public bool Equals(RabbitMQServiceSourceDefinition other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(HostName, other.HostName) &&
                string.Equals(Port, other.Port) &&
                string.Equals(UserName, other.UserName) &&
                string.Equals(Password, other.Password) &&
                string.Equals(VirtualHost, other.VirtualHost);
        }

        public bool Equals(IRabbitMQServiceSourceDefinition other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(HostName, other.HostName) &&
                string.Equals(Port, other.Port) &&
                string.Equals(UserName, other.UserName) &&
                string.Equals(Password, other.Password) &&
                string.Equals(VirtualHost, other.VirtualHost);
        }

        #endregion Equality members

        public string HostName { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
        public string ResourcePath { get; set; }
        public Guid ResourceID { get; set; }
        public string ResourceName { get; set; }
    }
}