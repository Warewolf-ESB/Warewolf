using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Providers.Errors;
using Newtonsoft.Json;
// ReSharper disable CheckNamespace

namespace Dev2.Runtime.ServiceModel.Data
{
    [Serializable]
    public abstract class ResourceBase : IResource
    {
        private IVersionInfo _versionInfo;

        protected ResourceBase()
        {
        }

        protected ResourceBase(IResource copy)
        {
            ResourceID = copy.ResourceID;
            ResourceName = copy.ResourceName;
            ResourceType = copy.ResourceType;
            ResourcePath = copy.ResourcePath;
            AuthorRoles = copy.AuthorRoles;
            FilePath = copy.FilePath;
        }

        protected ResourceBase(XElement xml)
        {
            if(xml == null)
            {
                throw new ArgumentNullException("xml");
            }

            Guid resourceId;
            if(!Guid.TryParse(xml.AttributeSafe("ID"), out resourceId))
            {
                // This is here for legacy XML!
                resourceId = Guid.NewGuid();
                IsUpgraded = true;
            }
            ResourceID = resourceId;
            ResourceType = xml.AttributeSafe("ResourceType");
            if (string.IsNullOrEmpty(ResourceType))
            {
                ResourceType = "WorkflowService";
            }
            ResourceName = xml.AttributeSafe("Name");
            ResourcePath = xml.ElementSafe("Category");
            ResourcePath = ResourcePath.Replace("\\\\", "\\");
            if (String.IsNullOrEmpty(ResourcePath))
            {
                ResourcePath = ResourceName;
            }
            VersionInfo = String.IsNullOrEmpty( xml.ElementStringSafe("VersionInfo"))?null: new VersionInfo(xml.ElementStringSafe("VersionInfo"), ResourceID);
            AuthorRoles = xml.ElementSafe("AuthorRoles");

            // This is here for legacy XML!
            if (ResourceType == "Unknown")
            {
                #region Check source type

                var sourceTypeStr = xml.AttributeSafe("Type");
                ResourceType = sourceTypeStr ?? "Unknown";

                #endregion

                #region Check action type

                var actions = xml.Element("Actions");

                var action = actions != null ? actions.Descendants().FirstOrDefault() : xml.Element("Action");

                if(action != null)
                {
                    var actionTypeStr = action.AttributeSafe("Type");
                    ResourceType = GetResourceTypeFromString(actionTypeStr);
                    IsUpgraded = true;
                }

                #endregion
            }
            var isValidStr = xml.AttributeSafe("IsValid");
            bool isValid;
            if(bool.TryParse(isValidStr, out isValid))
            {
                IsValid = isValid;
            }
            UpdateErrorsBasedOnXML(xml);
            LoadDependencies(xml);
            ReadDataList(xml);
            GetInputsOutputs(xml);
            SetIsNew(xml);
        }

        public Version Version { get; set; }
        [JsonIgnore]
        public bool IsUpgraded { get; set; }
        /// <summary>
        /// The resource ID that uniquely identifies the resource.
        /// </summary>   
        public Guid ResourceID { get; set; }
        /// <summary>
        /// Gets or sets the type of the resource.
        /// </summary>
        public string ResourceType { get; set; }
        /// <summary>
        /// The display name of the resource.
        /// </summary>
        public string ResourceName { get; set; }
        /// <summary>
        /// Gets or sets the category of the resource.
        /// </summary>
        public string ResourcePath { get; set; }
        /// <summary>
        /// Gets or sets the file path of the resource.
        /// <remarks>
        /// Must only be used by the catalog!
        /// </remarks>
        /// </summary>   
        [JsonIgnore]
        public string FilePath { get; set; }
        /// <summary>
        /// Gets or sets the author roles.
        /// </summary>
        [JsonIgnore]
        public string AuthorRoles { get; set; }
        [JsonIgnore]
        [XmlIgnore]
        public IList<IResourceForTree> Dependencies { get; set; }
        public bool IsValid { get; set; }
        public List<IErrorInfo> Errors { get; set; }
        public bool ReloadActions { get; set; }
        [JsonIgnore]
        public StringBuilder DataList { get; set; }
        [JsonIgnore]
        public string Inputs { get; set; }
        [JsonIgnore]
        public string Outputs { get; set; }
        public Permissions UserPermissions { get; set; }
        [JsonIgnore]
        public bool IsNewResource { get; set; }
        public abstract bool IsSource { get; }
        public abstract bool IsService { get; }
        public abstract bool IsFolder { get; }
        public abstract bool IsReservedService { get; }
        public abstract bool IsServer { get; }
        public abstract bool IsResourceVersion { get; }
        public IVersionInfo VersionInfo
        {
            get
            {
                return _versionInfo;
            }
            set
            {
                if(value!= null)
                    _versionInfo = value;
            }
        }

        public void ReadDataList(XElement xml)
        {
            DataList = xml.ElementSafeStringBuilder("DataList");
        }

        public void SetIsNew(XElement xml)
        {
            var xElement = xml.Element("IsNewWorkflow");
            if(xElement != null)
            {
                var tmp = xElement.Value;
                bool isNew;
                Boolean.TryParse(tmp, out isNew);
                IsNewResource = isNew;
            }
        }

        public void GetInputsOutputs(XElement xml)
        {
            var tmpB = xml.Elements().FirstOrDefault(c => c.Name == GlobalConstants.ActionsRootTag);
            var tmpA = xml.Elements().FirstOrDefault(c => c.Name == GlobalConstants.ActionRootTag);
            if(tmpB != null)
            {
                tmpA = tmpB.Elements().FirstOrDefault(c => c.Name == GlobalConstants.ActionRootTag);
            }
            if(tmpA != null)
            {
                Inputs = tmpA.ElementStringSafe(GlobalConstants.InputRootTag);
                Outputs = tmpA.ElementStringSafe(GlobalConstants.OutputRootTag);
            }
        }

        public void UpdateErrorsBasedOnXML(XElement xml)
        {
            var errorMessagesElement = xml.Element("ErrorMessages");
            Errors = new List<IErrorInfo>();
            if(errorMessagesElement != null)
            {
                var errorMessageElements = errorMessagesElement.Elements("ErrorMessage");
                foreach(var errorMessageElement in errorMessageElements)
                {
                    FixType fixType;
                    var fixTypeString = errorMessageElement.AttributeSafe("FixType");
                    Enum.TryParse(fixTypeString, true, out fixType);
                    ErrorType errorType;
                    var errorTypeString = errorMessageElement.AttributeSafe("ErrorType");
                    Enum.TryParse(errorTypeString, true, out errorType);
                    Guid instanceId;
                    Guid.TryParse(errorMessageElement.AttributeSafe("InstanceID"), out instanceId);
                    CompileMessageType messageType;
                    Enum.TryParse(errorMessageElement.AttributeSafe("MessageType"), true, out messageType);
                    Errors.Add(new ErrorInfo
                    {
                        InstanceID = instanceId,
                        Message = errorMessageElement.AttributeSafe("Message"),
                        StackTrace = errorMessageElement.AttributeSafe("StackTrace"),
                        FixType = fixType,
                        ErrorType = errorType,
                        FixData = errorMessageElement.Value
                    });
                }
            }
        }

        public string GetResourceTypeFromString(string actionTypeStr)
        {
            enActionType actionType;
            if(Enum.TryParse(actionTypeStr, out actionType))
            {
                switch(actionType)
                {
                    case enActionType.InvokeWebService:
                        return "WebService";
                    case enActionType.InvokeStoredProc:
                        return "DbService";
                    case enActionType.Plugin:
                        return "PluginService";
                    case enActionType.Workflow:
                        return "WorkflowService";
                }
            }
            return "Unknown";
        }

        public bool IsUserInAuthorRoles(string userRoles)
        {
            if(string.IsNullOrEmpty(userRoles))
            {
                return false;
            }

            if(string.IsNullOrEmpty(AuthorRoles))
            {
                return true;
            }

            var user = userRoles.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            var res = AuthorRoles.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            if(user.Contains("Domain Admins"))
            {
                return true;
            }

            if(!user.Any())
            {
                return false;
            }

            return res.Any() && user.Intersect(res).Any();
        }

        public virtual XElement ToXml()
        {
            var rootElement = GetRootElement();
            return new XElement(rootElement,
                new XAttribute("ID", ResourceID),
                new XAttribute("Name", ResourceName ?? string.Empty),
                new XAttribute("ResourceType", ResourceType),
                new XAttribute("IsValid", IsValid),
                new XElement("DisplayName", ResourceName ?? string.Empty),
                new XElement("Category", ResourcePath ?? string.Empty),
                new XElement("AuthorRoles", AuthorRoles ?? string.Empty),
                // ReSharper disable ConstantNullCoalescingCondition
                new XElement("ErrorMessages", WriteErrors() ?? null)
                // ReSharper restore ConstantNullCoalescingCondition
                );
        }

        private string GetRootElement()
        {
            if (IsSource)
            {
                return "Source";
            }
            if (IsService)
            {
                return "Service";
            }
            if (IsResourceVersion)
            {
                return "Service";
            }
            if (IsServer)
            {
                return "Source";
            }
            throw new Exception("Bad Resource. Cannot determine is type.");
        }

        public StringBuilder ToStringBuilder()
        {
            var xe = ToXml();
            return xe.ToStringBuilder();
        }

        private XElement WriteErrors()
        {
            if(Errors == null || Errors.Count == 0) return null;
            XElement xElement = null;
            foreach(var errorInfo in Errors)
            {
                xElement = new XElement("ErrorMessage");
                xElement.Add(new XAttribute("InstanceID", errorInfo.InstanceID));
                xElement.Add(new XAttribute("Message", errorInfo.Message ?? string.Empty));
                xElement.Add(new XAttribute("ErrorType", errorInfo.ErrorType));
                xElement.Add(new XAttribute("FixType", errorInfo.FixType));
                xElement.Add(new XAttribute("StackTrace", errorInfo.StackTrace ?? string.Empty));
                xElement.Add(new XCData(errorInfo.FixData ?? string.Empty));
            }
            return xElement;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

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
            return ResourceID.Equals(other.ResourceID); //&& Version.Equals(other.Version);
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
                return ResourceID.GetHashCode() * 397;
            }
        }

        public static bool operator ==(ResourceBase left, IResource right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ResourceBase left, IResource right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// If this instance <see cref="IsUpgraded"/> then sets the ID, Version, Name and ResourceType attributes on the given XML.
        /// </summary>
        /// <param name="xml">The XML to be upgraded.</param>
        /// <param name="resource"></param>
        /// <returns>The XML with the additional attributes set.</returns>
        public XElement UpgradeXml(XElement xml, IResource resource)
        {
            if(IsUpgraded)
            {
                xml.SetAttributeValue("ID", ResourceID);
                xml.SetAttributeValue("Name", ResourceName ?? string.Empty);
                xml.SetAttributeValue("ResourceType", ResourceType);
                xml.SetAttributeValue("Category", ResourcePath);
            }
            if(!xml.Descendants("VersionInfo").Any() && resource.VersionInfo != null)
            {
                var versionInfo = new XElement("VersionInfo");
                versionInfo.SetAttributeValue("DateTimeStamp",DateTime.Now);
                versionInfo.SetAttributeValue("Reason",resource.VersionInfo.Reason);
                versionInfo.SetAttributeValue("User",resource.VersionInfo.User);
                versionInfo.SetAttributeValue("VersionNumber",resource.VersionInfo.VersionNumber);
                versionInfo.SetAttributeValue("ResourceId",ResourceID);
                versionInfo.SetAttributeValue("VersionId", resource.VersionInfo.VersionId);
                xml.Add(versionInfo);
            }
            xml.SetAttributeValue("ServerVersion", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            if (!xml.Descendants("VersionInfo").Any() && resource.VersionInfo == null)
            {
                var versionInfo = new XElement("VersionInfo");
                versionInfo.SetAttributeValue("DateTimeStamp", DateTime.Now);
                versionInfo.SetAttributeValue("Reason", "Save");
                versionInfo.SetAttributeValue("User", "Unknown");
                versionInfo.SetAttributeValue("VersionNumber", "1");
                versionInfo.SetAttributeValue("ResourceId", ResourceID);
                versionInfo.SetAttributeValue("VersionId", Guid.NewGuid());
                xml.Add(versionInfo);
            }

            return xml;
        }

        private void LoadDependencies(XElement xml)
        {
            if(xml == null)
            {
                return;
            }
            if (ResourceType == "WorkflowService")
            {
                GetDependenciesForWorkflowService(xml);
            }            
        }

        private void GetDependenciesForWorkflowService(XElement xml)
        {
            var loadXml = xml.Descendants("XamlDefinition").ToList();
            if(loadXml.Count != 1)
            {
                return;
            }

            using(var textReader = new StringReader(loadXml[0].Value))
            {
                var errors = new StringBuilder();
                try
                {
                    var elementToUse = loadXml[0].HasElements ? loadXml[0] : XElement.Load(textReader, LoadOptions.None);
                    var dependenciesFromXml = from desc in elementToUse.Descendants()
                        where
                            (desc.Name.LocalName.Contains("DsfMySqlDatabaseActivity") ||
                             desc.Name.LocalName.Contains("DsfSqlServerDatabaseActivity") ||
                             desc.Name.LocalName.Contains("DsfDotNetDllActivity") ||
                             desc.Name.LocalName.Contains("DsfWebGetActivity") ||
                             desc.Name.LocalName.Contains("DsfActivity")) &&
                            desc.Attribute("UniqueID") != null
                        select desc;
                    var xElements = dependenciesFromXml as List<XElement> ?? dependenciesFromXml.ToList();
                    var count = xElements.Count;
                    if(count > 0)
                    {
                        Dependencies = new List<IResourceForTree>();
                        xElements.ForEach(element =>
                        {
                            var uniqueIdAsString = element.AttributeSafe("UniqueID");
                            var actionTypeStr = element.AttributeSafe("Type");
                            var resourceType = GetResourceTypeFromString(actionTypeStr);
                            if (resourceType == "Unknown")
                            {
                                resourceType = GetResourceTypeFromName(element.Name.LocalName);
                            }
                            var resourceIdAsString = element.AttributeSafe(resourceType == "WorkflowService" ? "ResourceID" : "SourceId");
                            var resourceName = element.AttributeSafe("ServiceName");
                            Guid uniqueId;
                            Guid.TryParse(uniqueIdAsString, out uniqueId);
                            Guid resId;
                            Guid.TryParse(resourceIdAsString, out resId);
                            Dependencies.Add(CreateResourceForTree(resId, uniqueId, resourceName, resourceType));
                            AddRemoteServerDependencies(element);
                        });
                    }
                    AddEmailSources(elementToUse);
                    AddRabbitMQSources(elementToUse);
                    AddDatabaseSourcesForSqlBulkInsertTool(elementToUse);
                }
                catch(Exception e)
                {
                    var resName = xml.AttributeSafe("Name");
                    errors.AppendLine("Loading dependencies for [ " + resName + " ] caused " + e.Message);
                }
            }
        }

        private string GetResourceTypeFromName(string localName)
        {
            switch(localName)
            {
                case "DsfMySqlDatabaseActivity":
                case "DsfSqlServerDatabaseActivity":
                    return "DbService";
                case "DsfDotNetDllActivity":
                    return "PluginService";
                case "DsfWebGetActivity":
                    return "WebService";
            }
            return "Unknown";
        }

        private void AddDatabaseSourcesForSqlBulkInsertTool(XElement elementToUse)
        {
            if(elementToUse == null)
            {
                return;
            }
            if(Dependencies == null)
            {
                Dependencies = new List<IResourceForTree>();
            }
            var dependenciesFromXml = from desc in elementToUse.Descendants()
                where desc.Name.LocalName.Contains("DbSource") && desc.HasAttributes
                select desc;
            var xElements = dependenciesFromXml as List<XElement> ?? dependenciesFromXml.ToList();
            var count = xElements.Count;
            if(count == 1)
            {
                var element = xElements[0];
                var resourceIdAsString = element.AttributeSafe("ResourceID");
                var resourceName = element.AttributeSafe("ResourceName");
                var actionTypeStr = element.AttributeSafe("Type");
                var resourceType = GetResourceTypeFromString(actionTypeStr);
                Guid resId;
                Guid.TryParse(resourceIdAsString, out resId);
                Dependencies.Add(CreateResourceForTree(resId, Guid.Empty, resourceName, resourceType));
            }
        }

        private void AddEmailSources(XElement elementToUse)
        {
            if(elementToUse == null)
            {
                return;
            }
            if(Dependencies == null)
            {
                Dependencies = new List<IResourceForTree>();
            }
            var dependenciesFromXml = from desc in elementToUse.Descendants()
                where desc.Name.LocalName.Contains("EmailSource") && desc.HasAttributes
                select desc;
            var xElements = dependenciesFromXml as List<XElement> ?? dependenciesFromXml.ToList();
            var count = xElements.Count;
            if(count == 1)
            {
                var element = xElements[0];
                var resourceIdAsString = element.AttributeSafe("ResourceID");
                var resourceName = element.AttributeSafe("ResourceName");
                var actionTypeStr = element.AttributeSafe("Type");
                var resourceType = GetResourceTypeFromString(actionTypeStr);
                Guid resId;
                Guid.TryParse(resourceIdAsString, out resId);
                Dependencies.Add(CreateResourceForTree(resId, Guid.Empty, resourceName, resourceType));
            }
        }

        private void AddRabbitMQSources(XElement elementToUse)
        {
            if(elementToUse == null)
            {
                return;
            }
            if(Dependencies == null)
            {
                Dependencies = new List<IResourceForTree>();
            }
            var dependenciesFromXml = from desc in elementToUse.Descendants()
                where (desc.Name.LocalName.Contains("DsfPublishRabbitMQActivity") || desc.Name.LocalName.Contains("DsfConsumeRabbitMQActivity")) && desc.HasAttributes
                select desc;
            var xElements = dependenciesFromXml as List<XElement> ?? dependenciesFromXml.ToList();
            var count = xElements.Count;
            if(count == 1)
            {
                var element = xElements[0];
                var resourceIdAsString = element.AttributeSafe("RabbitMQSourceResourceId");
                var uniqueIdAsString = element.AttributeSafe("UniqueID");
                Guid uniqueId;
                Guid.TryParse(uniqueIdAsString, out uniqueId);
                Guid resId;
                Guid.TryParse(resourceIdAsString, out resId);
                Dependencies.Add(CreateResourceForTree(resId, uniqueId, "", "RabbitMQSource"));
            }
        }

        private void AddRemoteServerDependencies(XElement element)
        {
            var environmentIdString = element.AttributeSafe("EnvironmentID");
            Guid environmentId;
            if(Guid.TryParse(environmentIdString, out environmentId) && environmentId != Guid.Empty)
            {
                if(environmentId == Guid.Empty) return;
                var resourceName = element.AttributeSafe("FriendlySourceName");
                Dependencies.Add(CreateResourceForTree(environmentId, Guid.Empty, resourceName, "Server"));
            }
        }

        private static ResourceForTree CreateResourceForTree(Guid resourceId, Guid uniqueId, string resourceName, string resourceType)
        {
            return new ResourceForTree
            {
                UniqueID = uniqueId,
                ResourceID = resourceId,
                ResourceName = resourceName,
                ResourceType = resourceType
            };
        }

        public static void ParseProperties(string s, Dictionary<string, string> properties)
        {
            if(s == null)
            {
                throw new ArgumentNullException("s");
            }
            if(properties == null)
            {
                throw new ArgumentNullException("properties");
            }

            var props = s.Split(';');
            foreach(var p in props.Select(prop => prop.Split('=')).Where(p => p.Length >= 1))
            {
                var key = p[0];
                if(!properties.ContainsKey(key))
                {
                    continue;
                }
                properties[key] = string.Join("=", p.Skip(1));
            }
        }
    }
}