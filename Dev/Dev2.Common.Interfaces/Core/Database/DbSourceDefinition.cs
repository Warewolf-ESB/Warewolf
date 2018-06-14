﻿using System;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Runtime.ServiceModel.Data;


namespace Dev2.Common.Interfaces.Core
{
    public class DbSourceDefinition : IDbSource, IEquatable<DbSourceDefinition>
    {
        AuthenticationType _authenticationType;

        public DbSourceDefinition()
        {

        }

        public DbSourceDefinition(IDb db)
        {
            AuthenticationType = db.AuthenticationType;
            DbName = db.DatabaseName;
            Id = db.ResourceID;
            Path = db.GetSavePath();
            Name = db.ResourceName;
            Password = db.Password;
            ServerName = db.Server;
            ConnectionTimeout = db.ConnectionTimeout;
            Type = db.ServerType;
            UserName = db.UserID;
        }

        #region Implementation of IDbSourceDefinition

        #region Equality members

        public bool Equals(IDbSource other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            var equals = true;
            equals &= string.Equals(ServerName, other.ServerName);
            equals &= Type == other.Type;
            equals &= string.Equals(UserName, other.UserName);
            equals &= string.Equals(Password, other.Password);
            equals &= AuthenticationType == other.AuthenticationType;
            equals &= Id == other.Id;
            equals &= string.Equals(DbName, other.DbName);
            equals &= ConnectionTimeout == other.ConnectionTimeout;            
            return equals;
        }

        public bool Equals(DbSourceDefinition other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            var equals = true;
            equals &= string.Equals(ServerName, other.ServerName);
            equals &= Type == other.Type;
            equals &= string.Equals(UserName, other.UserName);
            equals &= string.Equals(Password, other.Password);
            equals &= AuthenticationType == other.AuthenticationType;
            equals &= Id == other.Id;
            equals &= string.Equals(DbName, other.DbName);
            equals &= ConnectionTimeout == other.ConnectionTimeout;

            return equals;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((DbSourceDefinition)obj);
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ServerName?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (int)Type;
                hashCode = (hashCode * 397) ^ (UserName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Password?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (int)AuthenticationType;
                hashCode = (hashCode * 397) ^ ConnectionTimeout;
                hashCode = (hashCode * 397) ^ (DbName?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public static bool operator ==(DbSourceDefinition left, DbSourceDefinition right) => Equals(left, right);

        public static bool operator !=(DbSourceDefinition left, DbSourceDefinition right) => !Equals(left, right);

        #endregion

        public string ServerName { get; set; }
        public enSourceType Type { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int ConnectionTimeout { get; set; }
        public AuthenticationType AuthenticationType
        {
            get
            {
                return _authenticationType;
            }
            set
            {
                _authenticationType = value;
            }
        }
        public string DbName { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public Guid Id { get; set; }
        public bool ReloadActions { get; set; }

        #endregion

        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString() => Name;

        #endregion
    }
}
