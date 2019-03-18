#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System;
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

        public static bool operator ==(ToolDescriptor left, ToolDescriptor right) => Equals(left, right);

        public static bool operator !=(ToolDescriptor left, ToolDescriptor right) => !Equals(left, right);

        #endregion
        #endregion

        #endregion
    }
}
