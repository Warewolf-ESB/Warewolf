using System;
using System.Collections.ObjectModel;

namespace Dev2.Common.Interfaces.Core
{
    public class ComPluginSourceDefinition : IComPluginSource
    {
        #region Equality members

        public ComPluginSourceDefinition()
        {
            
        }

        public ComPluginSourceDefinition(IComPlugin db)
        {
            SelectedDll = new DllListing { Name = db.ComName, ClsId = db.ClsId, Is32Bit = db.Is32Bit, Children = new Collection<IFileListing>(), IsDirectory = false };
                Id = db.ResourceID;
                ResourcePath = "";
                ClsId = db.ClsId;
                Is32Bit = db.Is32Bit;
                ResourceName = db.ResourceName;
        }
        public bool Equals(IComPluginSource other)
        {
            // ReSharper disable once PossibleNullReferenceException
            return string.Equals(ResourceName, other.ResourceName) && Id.Equals(other.Id) && Equals(ClsId, other.ClsId) && Is32Bit == other.Is32Bit && Id.Equals(other.Id) && Equals(SelectedDll, other.SelectedDll) && string.Equals(Name, ((ComPluginSourceDefinition)other).Name);
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
            return Equals((ComPluginSourceDefinition)obj);
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
                var hashCode = ResourceName?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ Id.GetHashCode();
                hashCode = (hashCode * 397) ^ Is32Bit.GetHashCode();
                hashCode = (hashCode * 397) ^ (ClsId?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Name.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ComPluginSourceDefinition left, ComPluginSourceDefinition right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ComPluginSourceDefinition left, ComPluginSourceDefinition right)
        {
            return !Equals(left, right);
        }

        #endregion

        #region Implementation of IComPluginSource

        public string ResourceName { get; set; }
        public Guid Id { get; set; }
        public bool Is32Bit { get; set; }
        public string ClsId { get; set; }
        public IFileListing SelectedDll { get; set; }
        public string ResourcePath { get; set; }

        #endregion

        #region Binding Name added by Nathi 

        public string Name => ResourceName;

        #endregion

    }
}