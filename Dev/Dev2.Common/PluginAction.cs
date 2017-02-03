using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dev2.Common.Annotations;
using Dev2.Common.Interfaces.DB;

// ReSharper disable once CheckNamespace
namespace Dev2.Common.Interfaces
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class PluginAction : IPluginAction, INotifyPropertyChanged, IEquatable<PluginAction>
    {
        public string FullName { get; set; }
        public string Method { get; set; }
        public IList<IServiceInput> Inputs { get; set; }
        public Type ReturnType { get; set; }
        public IList<INameValue> Variables { get; set; }
        public string Dev2ReturnType { get; set; }
        
        public string MethodResult { get; set; }
        public string OutputVariable { get; set; }
        public bool IsObject { get; set; }
        public bool IsVoid { get; set; }
        public string ErrorMessage { get; set; }
        public bool HasError { get; set; }
        public bool IsProperty { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool Equals(PluginAction other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            if (GetHashCode() == other.GetHashCode())
                return true;

            return string.Equals(Method, other.Method);
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
            return Equals((PluginAction)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Inputs?.GetHashCode() ?? 0) * 397) ^ (Method?.GetHashCode() ?? 0);
            }
        }
        public string GetIdentifier()
        {
            return FullName + Method;
        }
        public static bool operator ==(PluginAction left, PluginAction right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PluginAction left, PluginAction right)
        {
            return !Equals(left, right);
        }

        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return Method;
        }

        public Guid ID { get; set; }

        #endregion
    }
}