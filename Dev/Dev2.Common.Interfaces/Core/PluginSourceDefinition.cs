#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Dev2.Common.Interfaces.Core
{
    public class PluginSourceDefinition : IPluginSource
    {
        [ExcludeFromCodeCoverage]
        public PluginSourceDefinition()
        {
                
        }

        public PluginSourceDefinition(IPlugin plugin)
        {
            SelectedDll = new DllListing
            {
                FullName = plugin.AssemblyLocation,
                Name = plugin.AssemblyName,
                Children = new Collection<IFileListing>(),
                IsDirectory = false
            };
            Id = plugin.ResourceID;
            Path = plugin.GetSavePath();
            Name = plugin.ResourceName;
            ConfigFilePath = plugin.ConfigFilePath;
            SetAssemblyName(plugin);
        }

        void SetAssemblyName(IPlugin plugin)
        {
            if (plugin.AssemblyLocation.StartsWith("GAC:"))
            {
                GACAssemblyName = plugin.AssemblyLocation;
                FileSystemAssemblyName = string.Empty;
            }
            else
            {
                FileSystemAssemblyName = plugin.AssemblyLocation;
                GACAssemblyName = string.Empty;
            }
        }

        public bool Equals(IPluginSource other)
        {
            var equals = true;
            equals &= string.Equals(Name, other.Name);
            equals &= string.Equals(Path, other.Path);
            equals &= string.Equals(ConfigFilePath, other.ConfigFilePath);
            equals &= string.Equals(FileSystemAssemblyName, other.FileSystemAssemblyName);
            equals &= string.Equals(GACAssemblyName, other.GACAssemblyName);
            return equals;
        }


        public override bool Equals(object obj)
        {
            if (obj is null)
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

#pragma warning disable S1541 // Methods and properties should not be too complex
        public override int GetHashCode()
#pragma warning restore S1541 // Methods and properties should not be too complex
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

        public static bool operator ==(PluginSourceDefinition left, PluginSourceDefinition right) => Equals(left, right);

        public static bool operator !=(PluginSourceDefinition left, PluginSourceDefinition right) => !Equals(left, right);

        public string Name { get; set; }
        public Guid Id { get; set; }
        public IFileListing SelectedDll { get; set; }
        public string Path { get; set; }
        public string ConfigFilePath { get; set; }
        public string FileSystemAssemblyName { get; set; }
        public string GACAssemblyName { get; set; }
    }
}
