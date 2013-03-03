using System;
using System.Xml.Linq;
using Dev2.Common.ServiceModel;
using Dev2.DynamicServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class Resource : IResource
    {
        #region CTOR

        public Resource()
        {
        }

        public Resource(XElement xml)
        {
            if(xml == null)
            {
                throw new ArgumentNullException("xml");
            }

            Guid resourceID;
            if(!Guid.TryParse(xml.AttributeSafe("ID"), out resourceID))
            {
                // This is here for legacy XML!
                resourceID = Guid.NewGuid();
                IsUpgraded = true;
            }
            ResourceID = resourceID;
            Version = xml.AttributeSafe("Version");
            ResourceType = ParseResourceType(xml.AttributeSafe("ResourceType"));
            ResourceName = xml.AttributeSafe("Name");
            ResourcePath = xml.ElementSafe("Category");

            // This is here for legacy XML!
            if(ResourceType == ResourceType.Unknown)
            {
                #region Check source type

                var sourceTypeStr = xml.AttributeSafe("Type");
                enSourceType sourceType;
                if(Enum.TryParse(sourceTypeStr, out sourceType))
                {
                    switch(sourceType)
                    {
                        case enSourceType.Dev2Server:
                            ResourceType = ResourceType.Server;
                            IsUpgraded = true;
                            break;
                        case enSourceType.SqlDatabase:
                        case enSourceType.MySqlDatabase:
                            ResourceType = ResourceType.DbSource;
                            IsUpgraded = true;
                            break;
                        case enSourceType.Plugin:
                            ResourceType = ResourceType.PluginService;
                            IsUpgraded = true;
                            break;
                    }
                }

                #endregion

                #region Check action type

                var action = xml.Element("Action");
                if(action != null)
                {
                    var actionTypeStr = action.AttributeSafe("Type");
                    enActionType actionType;
                    if(Enum.TryParse(actionTypeStr, out actionType))
                    {
                        switch(actionType)
                        {
                            case enActionType.InvokeStoredProc:
                                ResourceType = ResourceType.DbService;
                                IsUpgraded = true;
                                break;
                            case enActionType.Plugin:
                                ResourceType = ResourceType.PluginService;
                                IsUpgraded = true;
                                break;
                            case enActionType.Workflow:
                                ResourceType = ResourceType.Workflow;
                                IsUpgraded = true;
                                break;
                        }
                    }
                }

                #endregion
            }
        }

        #endregion

        #region Properties

        [JsonIgnore]
        public bool IsUpgraded { get; set; }

        /// <summary>
        /// The resource ID that uniquely identifies the resource.
        /// </summary>   
        public Guid ResourceID { get; set; }

        /// <summary>
        /// The version that uniquely identifies the resource.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the type of the resource.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ResourceType ResourceType { get; set; }

        /// <summary>
        /// The display name of the resource.
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// Gets or sets the category of the resource.
        /// </summary>
        public string ResourcePath { get; set; }

        /// <summary>
        /// Gets or sets the contents of the resource.
        /// </summary>
        [JsonIgnore]
        public string Contents { get; set; }

        /// <summary>
        /// Gets or sets the file path of the resource.
        /// <remarks>
        /// Must only be used by the catalog!
        /// </remarks>
        /// </summary>   
        [JsonIgnore]
        public string FilePath { get; set; }

        #endregion

        #region Save

        public virtual void Save(Guid workspaceID, Guid dataListID)
        {
            var xml = ToXml();
            Resources.Save(workspaceID, Resources.RootFolders[ResourceType], ResourceName, xml.ToString());
        }

        #endregion

        #region ToXml

        public virtual XElement ToXml()
        {
            return new XElement(Resources.RootElements[ResourceType],
                new XAttribute("ID", ResourceID),
                new XAttribute("Version", Version ?? string.Empty),
                new XAttribute("Name", ResourceName ?? string.Empty),
                new XAttribute("ResourceType", ResourceType),
                new XElement("DisplayName", ResourceName ?? string.Empty),
                new XElement("Category", ResourcePath ?? string.Empty)
                );
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        #endregion

        #region ParseResourceType

        protected ResourceType ParseResourceType(string resourceTypeStr)
        {
            ResourceType resourceType;
            Enum.TryParse(resourceTypeStr, out resourceType);
            return resourceType;
        }

        #endregion

        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(IResource other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            if(ReferenceEquals(this, other))
            {
                return true;
            }
            return ResourceID.Equals(other.ResourceID) && string.Equals(Version, other.Version);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param>
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
            return Equals((IResource)obj);
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
                return (ResourceID.GetHashCode() * 397) ^ (Version != null ? Version.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Resource left, IResource right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Resource left, IResource right)
        {
            return !Equals(left, right);
        }

        #endregion

    }
}
