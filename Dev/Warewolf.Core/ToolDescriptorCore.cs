using System;
using System.Collections.Generic;
using Dev2;
using Dev2.Common.Interfaces.Toolbox;

namespace Warewolf.Core
{
    public class ToolDescriptor : IToolDescriptor, IEquatable<ToolDescriptor>
    {        
        public ToolDescriptor(Guid id, IWarewolfType designer, IWarewolfType activity, string name, string icon, Version version, bool isSupported, string category, ToolType toolType, string iconUri, string filterTag,string toolTip, string helpText)        
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException("id", "empty guids not allowed fo tools");
            }

            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "id", id }, { "designer", designer }, { "activity", activity }, { "name", name }, { "icon", icon }, { "version", version }, { "category", category }, { iconUri, "iconUri" }, { filterTag, "filterTag" } });
            ToolType = toolType;
            Category = category;
            IsSupported = isSupported;

            Version = version;
            Icon = icon;
            Name = name;
            Activity = activity;
            IconUri = iconUri;
            Designer = designer;
            Id = id;
            FilterTag = filterTag;
            ResourceToolTip = toolTip;
            ResourceHelpText = helpText;
        }

        #region Implementation of IToolDescriptor
        
        public Guid Id { get; private set; }
        public IWarewolfType Designer { get; private set; }
        public IWarewolfType Activity { get; private set; }
        public string Name { get; private set; }
        public string Icon { get; private set; }
        public string IconUri { get; private set; }
        public string FilterTag { get; set; }
        public string ResourceToolTip { get; set; }
        public string ResourceHelpText { get; set; }    
        public Version Version { get; private set; }
        public bool IsSupported { get; private set; }
        public string Category { get; private set; }
        public ToolType ToolType { get; private set; }

        #region Overrides of Object

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
            return Equals((ToolDescriptor)obj);
        }
        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(ToolDescriptor other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Id.Equals(other.Id) && Equals(Version, other.Version);
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
                return (Id.GetHashCode() * 397) ^ (Version != null ? Version.GetHashCode() : 0);
            }
        }

        public static bool operator ==(ToolDescriptor left, ToolDescriptor right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ToolDescriptor left, ToolDescriptor right)
        {
            return !Equals(left, right);
        }

            #endregion
            #endregion

        #endregion
    }
}
