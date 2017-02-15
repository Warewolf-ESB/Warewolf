using System;

// ReSharper disable InconsistentNaming

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

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
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

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(IRabbitMQServiceSourceDefinition other)
        {
            return Equals(other as RabbitMQServiceSourceDefinition);
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