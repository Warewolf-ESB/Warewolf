using System;
using System.Collections.Generic;
using Dev2;
using Dev2.Common.Interfaces.Toolbox;

namespace Warewolf.Core
{
    public class ToolDescriptor:IToolDescriptor, IEquatable<ToolDescriptor>
    {


        // ReSharper disable TooManyDependencies
        public ToolDescriptor(Guid id, IWarewolfType designer,  IWarewolfType activity, string name, string icon, Version version, bool isSupported, string category, ToolType toolType,string iconUri)
            // ReSharper restore TooManyDependencies
        {
            if(id==Guid.Empty) throw  new ArgumentNullException("id","empty guids not allowed fo tools");
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "id", id }, { "designer", designer }, { "activity", activity }, { "name", name }, { "icon", icon }, { "version", version }, { "category", category }, { iconUri, "iconUri" } });
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
        }

        
        #region Implementation of IToolDescriptor

        /// <summary>
        /// Tool identifier. Originates from containing dll
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>
        /// The type that will be instantiated as the designer
        /// </summary>
        public IWarewolfType Designer { get; private set; }
        /// <summary>
        /// something or the other; //todo: check what this was meant to do in diagram
        /// </summary>
        //public IWarewolfType Model { get; private set; }
        /// <summary>
        /// Server activity that this will instantiate
        /// </summary>
        public IWarewolfType Activity { get; private set; }
        /// <summary>
        /// Name of tool as per toolbox
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Icon that will be displayed
        /// </summary>
        public string Icon { get; private set; }

        public string IconUri { get; private set; }
        /// <summary>
        /// Version as per dll
        /// </summary>
        public Version Version { get; private set; }
        /// <summary>
        /// Help text for help window
        /// </summary>
        //public IHelpDescriptor Helpdescriptor { get; private set; }
        /// <summary>
        /// Is supported locally
        /// </summary>
        public bool IsSupported { get; private set; }
        /// <summary>
        /// Tool category for toolbox
        /// </summary>
        public string Category { get; private set; }
        /// <summary>
        /// Native or user
        /// </summary>
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
