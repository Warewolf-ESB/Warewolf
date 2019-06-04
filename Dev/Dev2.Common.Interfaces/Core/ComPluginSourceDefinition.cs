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
    public class ComPluginSourceDefinition : IComPluginSource
    {
        [ExcludeFromCodeCoverage]
        public ComPluginSourceDefinition()
        {

        }

        public ComPluginSourceDefinition(IComPlugin db)
        {
            SelectedDll = new DllListing
            {
                Name = db.ComName,
                ClsId = db.ClsId,
                Is32Bit = db.Is32Bit,
                Children = new Collection<IFileListing>(),
                IsDirectory = false
            };
            Id = db.ResourceID;
            ResourcePath = "";
            ClsId = db.ClsId;
            Is32Bit = db.Is32Bit;
            ResourceName = db.ResourceName;
        }
        public bool Equals(IComPluginSource other)
        {
            var equals = true;
            equals &= string.Equals(ResourceName, other.ResourceName);
            equals &= Id.Equals(other.Id);
            equals &= Equals(ClsId, other.ClsId);
            equals &= Is32Bit == other.Is32Bit;
            equals &= Equals(SelectedDll, other.SelectedDll);
            equals &= string.Equals(Name, other.Name);
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
            return Equals((ComPluginSourceDefinition)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ResourceName?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ Id.GetHashCode();
                hashCode = (hashCode * 397) ^ Is32Bit.GetHashCode();
                hashCode = (hashCode * 397) ^ (ClsId?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Name?.GetHashCode() ?? 0;
                return hashCode;
            }
        }

        public static bool operator ==(ComPluginSourceDefinition left, ComPluginSourceDefinition right) => Equals(left, right);

        public static bool operator !=(ComPluginSourceDefinition left, ComPluginSourceDefinition right) => !Equals(left, right);

        public string ResourceName { get; set; }
        public Guid Id { get; set; }
        public bool Is32Bit { get; set; }
        public string ClsId { get; set; }
        public IFileListing SelectedDll { get; set; }
        public string ResourcePath { get; set; }
        public string Name => ResourceName;
    }
}