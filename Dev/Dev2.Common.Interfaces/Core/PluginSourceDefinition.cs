using System;

namespace Dev2.Common.Interfaces.Core
{
    public class PluginSourceDefinition : IPluginSource
    {
        #region Equality members

        public bool Equals(IPluginSource other)
        {
            // ReSharper disable once PossibleNullReferenceException
            return string.Equals(Name, other.Name) && Id.Equals(other.Id) && Equals(SelectedDll, other.SelectedDll) && string.Equals(Path, other.Path);
        }


        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
            {
                return false;
            }
            if(ReferenceEquals(this, obj))
            {
                return true;
            }
            if(obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((PluginSourceDefinition)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Name != null ? Name.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ Id.GetHashCode();
                hashCode = (hashCode * 397) ^ (SelectedDll != null ? SelectedDll.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Path != null ? Path.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(PluginSourceDefinition left, PluginSourceDefinition right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PluginSourceDefinition left, PluginSourceDefinition right)
        {
            return !Equals(left, right);
        }

        #endregion

        #region Implementation of IPluginSource

        public string Name { get; set; }
        public Guid Id { get; set; }
        public IFileListing SelectedDll { get; set; }
        public string Path { get; set; }

        #endregion
    }
}
