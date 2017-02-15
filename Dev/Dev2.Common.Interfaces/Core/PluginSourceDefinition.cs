using System;
using System.Collections.ObjectModel;

namespace Dev2.Common.Interfaces.Core
{
    public class PluginSourceDefinition : IPluginSource
    {

        public PluginSourceDefinition()
        {
                
        }

        public PluginSourceDefinition(IPlugin db)
        {
            SelectedDll = new DllListing { FullName = db.AssemblyLocation, Name = db.AssemblyName, Children = new Collection<IFileListing>(), IsDirectory = false };
            Id = db.ResourceID;
            Path = db.GetSavePath();
            Name = db.ResourceName;
            ConfigFilePath = db.ConfigFilePath;
            SetAssemblyName(db);
        }
        
        private void SetAssemblyName(IPlugin db)
        {
            if (db.AssemblyLocation.StartsWith("GAC:"))
            {
                GACAssemblyName = db.AssemblyLocation;
                FileSystemAssemblyName = string.Empty;
            }
            else
            {
                FileSystemAssemblyName = db.AssemblyLocation;
                GACAssemblyName = string.Empty;
            }
        }
        #region Equality members

        public bool Equals(IPluginSource other)
        {
            // ReSharper disable once PossibleNullReferenceException
            return string.Equals(Name, other.Name) &&
                string.Equals(Path, other.Path) &&
                string.Equals(ConfigFilePath, other.ConfigFilePath) &&
                string.Equals(FileSystemAssemblyName, other.FileSystemAssemblyName) &&
                string.Equals(GACAssemblyName, other.GACAssemblyName);
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
                var hashCode = Name?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (Path?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (ConfigFilePath?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (FileSystemAssemblyName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (GACAssemblyName?.GetHashCode() ?? 0);
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
        public string ConfigFilePath { get; set; }
        public string FileSystemAssemblyName { get; set; }
        public string GACAssemblyName { get; set; }

        #endregion
    }
}
