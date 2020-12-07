#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
using Warewolf.Data;
using Warewolf.Resource.Errors;



namespace Dev2.Runtime.ServiceModel.Data
{
    [Serializable]
    public abstract class ResourceBase : IResource, IFilePathResource
    {
        IVersionInfo _versionInfo;

        protected ResourceBase()
        {
        }

        protected ResourceBase(IResource copy)
        {
            ResourceID = copy.ResourceID;
            ResourceName = copy.ResourceName;
            ResourceType = copy.ResourceType;
            AuthorRoles = copy.AuthorRoles;
            FilePath = copy.FilePath;
        }

        protected ResourceBase(XElement xml)
        {
            if (xml == null)
            {
                throw new ArgumentNullException(nameof(xml));
            }

            if (!Guid.TryParse(xml.AttributeSafe("ID"), out Guid resourceId))
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
            VersionInfo = String.IsNullOrEmpty(xml.ElementStringSafe("VersionInfo")) ? null : new VersionInfo(xml.ElementStringSafe("VersionInfo"), ResourceID);
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

                if (action != null)
                {
                    var actionTypeStr = action.AttributeSafe("Type");
                    ResourceType = GetResourceTypeFromString(actionTypeStr);
                    IsUpgraded = true;
                }

                #endregion
            }
            var isValidStr = xml.AttributeSafe("IsValid");
            if (bool.TryParse(isValidStr, out bool isValid))
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
        /// Gets or sets the file path of the resource.
        /// <remarks>
        /// Must only be used by the catalog!
        /// </remarks>
        /// </summary>   
        [JsonIgnore]
        public string FilePath { get; set; }

        [JsonIgnore] public string Path => GetResourceFromUnknownWorkspacePath(); //GetResourcePath(GlobalConstants.ServerWorkspaceID);
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

        public bool HasDataList => DataList != null && !string.IsNullOrWhiteSpace(DataList.ToString());

        public string GetResourcePath(Guid workspaceID)
        {
            if (FilePath == null && IsReservedService)
            {
                return ResourceName;
            }
            return FilePath?.Replace(EnvironmentVariables.GetWorkspacePath(workspaceID) + "\\", "").Replace(".xml", "").Replace(".bite", "") ?? "";
        }

        public string GetResourceFromUnknownWorkspacePath()
        {
            if (FilePath is null || IsReservedService)
            {
                return ResourceName;
            }

            var filePath = FilePath;
            var workspacePath = EnvironmentVariables.WorkspacePath;
            if (filePath.StartsWith(workspacePath))
            {
                var removeWorkspacePath = FilePath.Replace(workspacePath + "\\", "");
                var workspaceIdEnd = removeWorkspacePath.IndexOf("\\", StringComparison.Ordinal);
                var workspaceId = removeWorkspacePath.Substring(0, workspaceIdEnd);
                var resourcesDir = removeWorkspacePath.Replace(workspaceId, "");
                var programData = resourcesDir.Replace("\\ProgramData\\", "");
                var resourceFile = programData.Replace("\\Resources\\", "");

                var removeXml = resourceFile.Replace(".xml", "");
                var removeBite = removeXml.Replace(".bite", "");
                return removeBite;
            }

            return filePath.Replace(EnvironmentVariables.ResourcePath, "").Replace(".bite", "");
        }

        public string GetSavePath()
        {
            var resourcePath = GetResourcePath(GlobalConstants.ServerWorkspaceID);
            var savePath = resourcePath;
            var resourceNameIndex = resourcePath.LastIndexOf(ResourceName, StringComparison.InvariantCultureIgnoreCase);
            if (resourceNameIndex >= 0)
            {
                savePath = resourcePath.Substring(0, resourceNameIndex);
            }
            return savePath;
        }

        public IVersionInfo VersionInfo
        {
            get
            {
                return _versionInfo;
            }
            set
            {
                if (value != null)
                {
                    _versionInfo = value;
                }
            }
        }

        public void ReadDataList(XElement xml)
        {
            DataList = xml.ElementSafeStringBuilder("DataList");
        }

        public void SetIsNew(XElement xml)
        {
            var xElement = xml.Element("IsNewWorkflow");
            if (xElement != null)
            {
                var tmp = xElement.Value;
                Boolean.TryParse(tmp, out bool isNew);
                IsNewResource = isNew;
            }
        }

        public void GetInputsOutputs(XElement xml)
        {
            var tmpB = xml.Elements().FirstOrDefault(c => c.Name == GlobalConstants.ActionsRootTag);
            var tmpA = xml.Elements().FirstOrDefault(c => c.Name == GlobalConstants.ActionRootTag);
            if (tmpB != null)
            {
                tmpA = tmpB.Elements().FirstOrDefault(c => c.Name == GlobalConstants.ActionRootTag);
            }
            if (tmpA != null)
            {
                Inputs = tmpA.ElementStringSafe(GlobalConstants.InputRootTag);
                Outputs = tmpA.ElementStringSafe(GlobalConstants.OutputRootTag);
            }
        }

        public void UpdateErrorsBasedOnXML(XElement xml)
        {
            var errorMessagesElement = xml.Element("ErrorMessages");
            Errors = new List<IErrorInfo>();
            if (errorMessagesElement != null)
            {
                var errorMessageElements = errorMessagesElement.Elements("ErrorMessage");
                foreach (var errorMessageElement in errorMessageElements)
                {
                    var fixTypeString = errorMessageElement.AttributeSafe("FixType");
                    Enum.TryParse(fixTypeString, true, out FixType fixType);
                    var errorTypeString = errorMessageElement.AttributeSafe("ErrorType");
                    Enum.TryParse(errorTypeString, true, out ErrorType errorType);
                    Guid.TryParse(errorMessageElement.AttributeSafe("InstanceID"), out Guid instanceId);
                    Enum.TryParse(errorMessageElement.AttributeSafe("MessageType"), true, out CompileMessageType messageType);
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

        public virtual XElement ToXml()
        {
            var rootElement = GetRootElement();
            return new XElement(rootElement,
                new XAttribute("ID", ResourceID),
                new XAttribute("Name", ResourceName ?? string.Empty),
                new XAttribute("ResourceType", ResourceType),
                new XAttribute("IsValid", IsValid),
                new XElement("DisplayName", ResourceName ?? string.Empty),
                new XElement("AuthorRoles", AuthorRoles ?? string.Empty),
                
                new XElement("ErrorMessages", WriteErrors() ?? null)
                
                );
        }

        public StringBuilder ToStringBuilder()
        {
            var xe = ToXml();
            return xe.ToStringBuilder();
        }

        public override string ToString() => JsonConvert.SerializeObject(this);

        public bool Equals(IResource other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return ResourceID.Equals(other.ResourceID);
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
            return Equals((IResource)obj);
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                return ResourceID.GetHashCode() * 397;
            }
        }

        public static bool operator ==(ResourceBase left, IResource right) => Equals(left, right);

        public static bool operator !=(ResourceBase left, IResource right) => !Equals(left, right);

        public XElement UpgradeXml(XElement xml, IResource resource)
        {
            if (IsUpgraded)
            {
                xml.SetAttributeValue("ID", ResourceID);
                xml.SetAttributeValue("Name", ResourceName ?? string.Empty);
                xml.SetAttributeValue("ResourceType", ResourceType);
            }
            if (!xml.Descendants("VersionInfo").Any() && resource.VersionInfo != null)
            {
                var versionInfo = new XElement("VersionInfo");
                versionInfo.SetAttributeValue("DateTimeStamp", DateTime.Now);
                versionInfo.SetAttributeValue("Reason", resource.VersionInfo.Reason);
                versionInfo.SetAttributeValue("User", resource.VersionInfo.User);
                versionInfo.SetAttributeValue("VersionNumber", resource.VersionInfo.VersionNumber);
                versionInfo.SetAttributeValue("ResourceId", ResourceID);
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

        public void LoadDependencies(XElement xml)
        {
            if (xml == null)
            {
                return;
            }
            if (ResourceType == "WorkflowService")
            {
                TryGetDependenciesForWorkflowService(xml);
            }
        }

        string GetRootElement()
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
            throw new Exception(ErrorResource.BadResource);
        }

        XElement WriteErrors()
        {
            if (Errors == null || Errors.Count == 0)
            {
                return null;
            }

            XElement xElement = null;
            foreach (var errorInfo in Errors)
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

        static string GetResourceTypeFromString(string actionTypeStr)
        {
            if (Enum.TryParse(actionTypeStr, out enActionType actionType))
            {
                switch (actionType)
                {
                    case enActionType.InvokeWebService:
                        return "WebService";
                    case enActionType.InvokeStoredProc:
                        return "DbService";
                    case enActionType.Plugin:
                        return "PluginService";
                    case enActionType.Workflow:
                        return "WorkflowService";
                    default:
                        break;
                }
            }
            return "Unknown";
        }

        void TryGetDependenciesForWorkflowService(XElement xml)
        {
            var loadXml = xml.Descendants("XamlDefinition").ToList();
            if (loadXml.Count != 1)
            {
                return;
            }

            var el = loadXml[0];

            using (var textReader = new StringReader(el.Value))
            {
                var errors = new StringBuilder();
                try
                {
                    GetDependenciesForWorkflowService(el, textReader);
                }
                catch (Exception e)
                {
                    var resName = xml.AttributeSafe("Name");
                    errors.AppendLine("Loading dependencies for [ " + resName + " ] caused " + e.Message);
                }
            }
        }

        // TODO: this method should not hard code the loaders for various Sources.
        private void GetDependenciesForWorkflowService(XElement el, StringReader textReader)
        {
            var elementToUse = el.HasElements ? el : XElement.Load(textReader, LoadOptions.None);
            RootActivity = elementToUse;
            var dependenciesFromXml = from desc in elementToUse.Descendants()
                                      where
                                          IsDependency(desc.Name.LocalName) &&
                                          desc.Attribute("UniqueID") != null
                                      select desc;
            var xElements = dependenciesFromXml as List<XElement> ?? dependenciesFromXml.ToList();
            var count = xElements.Count;

            if (count > 0)
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
                    Guid.TryParse(uniqueIdAsString, out Guid uniqueId);
                    Guid.TryParse(resourceIdAsString, out Guid resId);
                    Dependencies.Add(
                        new ResourceForTree
                        {
                            ResourceID = resId,
                            UniqueID = uniqueId,
                            ResourceName = resourceName,
                            ResourceType = resourceType
                        });
                    AddRemoteServerDependencies(element);
                });
            }
            AddEmailSources(elementToUse);
            AddWebSources(elementToUse);
            AddSharepointSources(elementToUse);
            AddDotnetSources(elementToUse);
            AddRabbitMqSources(elementToUse);
            AddDatabaseSourcesForSqlBulkInsertTool(elementToUse);
        }

        protected XElement RootActivity { get; set; }

#pragma warning disable S1067 // Expressions should not be too complex
        private static bool IsDependency(string localName) => localName.Contains("DsfMySqlDatabaseActivity") ||
                                                   localName.Contains("DsfSqlServerDatabaseActivity") ||
                                                   localName.Contains("DsfDotNetDllActivity") ||
                                                   localName.Contains("DsfWebGetActivity") ||
                                                   localName.Contains("DsfActivity");
#pragma warning restore S1067 // Expressions should not be too complex

        static string GetResourceTypeFromName(string localName)
        {
            switch (localName)
            {
                case "DsfMySqlDatabaseActivity":
                case "DsfSqlServerDatabaseActivity":
                    return "DbService";
                case "DsfDotNetDllActivity":
                case "DsfEnhancedDotNetDllActivity":
                    return "PluginService";
                case "DsfWebGetActivity":
                    return "WebService";
                default:
                    return "Unknown";
            }
        }

        void AddDatabaseSourcesForSqlBulkInsertTool(XElement elementToUse)
        {
            if (elementToUse == null)
            {
                return;
            }
            if (Dependencies == null)
            {
                Dependencies = new List<IResourceForTree>();
            }
            var dependenciesFromXml = from desc in elementToUse.Descendants()
                                      where desc.Name.LocalName.Contains("DbSource") && desc.HasAttributes
                                      select desc;
            var xElements = dependenciesFromXml as List<XElement> ?? dependenciesFromXml.ToList();
            foreach (var element in xElements)
            {
                var resourceIdAsString = element.AttributeSafe("ResourceID");
                var resourceName = element.AttributeSafe("ResourceName");
                var actionTypeStr = element.AttributeSafe("Type");
                var resourceType = GetResourceTypeFromString(actionTypeStr);
                Guid.TryParse(resourceIdAsString, out Guid resId);
                var resourceForTree = Dependencies.FirstOrDefault(tree => tree.ResourceID == resId);
                if (resourceForTree == null)
                {
                    Dependencies.Add(
                        new ResourceForTree
                        {
                            ResourceID = resId,
                            UniqueID = Guid.Empty,
                            ResourceName = resourceName,
                            ResourceType = resourceType
                        });
                }
            }
        }

        void AddEmailSources(XElement elementToUse)
        {
            if (elementToUse == null)
            {
                return;
            }
            if (Dependencies == null)
            {
                Dependencies = new List<IResourceForTree>();
            }
            var dependenciesFromXml = from desc in elementToUse.Descendants()
                                      where desc.Name.LocalName.Contains("EmailSource") && desc.HasAttributes
                                      select desc;
            var xElements = dependenciesFromXml as List<XElement> ?? dependenciesFromXml.ToList();
            foreach (var element in xElements)
            {
                var resourceIdAsString = element.AttributeSafe("ResourceID");
                var resourceName = element.AttributeSafe("ResourceName");
                var actionTypeStr = element.AttributeSafe("Type");
                var resourceType = GetResourceTypeFromString(actionTypeStr);
                Guid.TryParse(resourceIdAsString, out Guid resId);
                var resourceForTree = Dependencies.FirstOrDefault(tree => tree.ResourceID == resId);
                if (resourceForTree == null)
                {
                    Dependencies.Add(
                        new ResourceForTree
                        {
                            ResourceID = resId,
                            UniqueID = Guid.Empty,
                            ResourceName = resourceName,
                            ResourceType = resourceType
                        });
                }
            }
        }

        void AddDotnetSources(XElement elementToUse)
        {
            if (elementToUse == null)
            {
                return;
            }
            if (Dependencies == null)
            {
                Dependencies = new List<IResourceForTree>();
            }
            var dependenciesFromXml = from desc in elementToUse.Descendants()
                                      where (desc.Name.LocalName.Contains("DsfEnhancedDotNetDllActivity")) && desc.HasAttributes
                                      select desc;
            var xElements = dependenciesFromXml as List<XElement> ?? dependenciesFromXml.ToList();

            foreach (var element in xElements)
            {
                var resourceIdAsString = element.AttributeSafe("SourceId");
                var resourceName = element.AttributeSafe("ResourceName");
                var actionTypeStr = element.AttributeSafe("Type");
                var resourceType = GetResourceTypeFromString(actionTypeStr);
                Guid.TryParse(resourceIdAsString, out Guid resId);
                var resourceForTree = Dependencies.FirstOrDefault(tree => tree.ResourceID == resId);
                if (resourceForTree == null)
                {
                    Dependencies.Add(
                        new ResourceForTree
                        {
                            ResourceID = resId,
                            UniqueID = Guid.Empty,
                            ResourceName = resourceName,
                            ResourceType = resourceType
                        });
                }
            }
        }

        void AddRabbitMqSources(XElement elementToUse)
        {
            if (elementToUse == null)
            {
                return;
            }
            if (Dependencies == null)
            {
                Dependencies = new List<IResourceForTree>();
            }
            var dependenciesFromXml = from desc in elementToUse.Descendants()
                                      where (desc.Name.LocalName.Contains("DsfPublishRabbitMQActivity") || desc.Name.LocalName.Contains("DsfConsumeRabbitMQActivity")) && desc.HasAttributes
                                      select desc;
            var xElements = dependenciesFromXml as List<XElement> ?? dependenciesFromXml.ToList();
            foreach (var element in xElements)
            {
                var resourceIdAsString = element.AttributeSafe("RabbitMQSourceResourceId");
                var uniqueIdAsString = element.AttributeSafe("UniqueID");
                Guid.TryParse(uniqueIdAsString, out Guid uniqueId);
                Guid.TryParse(resourceIdAsString, out Guid resId);
                var resourceForTree = Dependencies.FirstOrDefault(tree => tree.ResourceID == resId);
                if (resourceForTree == null)
                {
                    Dependencies.Add(
                        new ResourceForTree
                        {
                            ResourceID = resId,
                            UniqueID = uniqueId,
                            ResourceName = "",
                            ResourceType = "RabbitMQSource"
                        });
                }
            }
        }

        void AddWebSources(XElement elementToUse)
        {
            if (elementToUse == null)
            {
                return;
            }
            if (Dependencies == null)
            {
                Dependencies = new List<IResourceForTree>();
            }
            var dependenciesFromXml = from desc in elementToUse.Descendants()
                                      where IsWebSourceName(desc.Name.LocalName) && desc.HasAttributes
                                      select desc;
            var xElements = dependenciesFromXml as List<XElement> ?? dependenciesFromXml.ToList();
            foreach (var element in xElements)
            {
                var resourceIdAsString = element.AttributeSafe("SourceId");
                var uniqueIdAsString = element.AttributeSafe("UniqueID");
                var resourceName = element.AttributeSafe("ResourceName");
                Guid.TryParse(uniqueIdAsString, out Guid uniqueId);
                Guid.TryParse(resourceIdAsString, out Guid resId);
                var resourceForTree = Dependencies.FirstOrDefault(tree => tree.ResourceID == resId);
                if (resourceForTree == null)
                {
                    Dependencies.Add(
                        new ResourceForTree
                        {
                            ResourceID = resId,
                            UniqueID = uniqueId,
                            ResourceName = resourceName,
                            ResourceType = "WebSource"
                        });
                }
            }
        }

        private static bool IsWebSourceName(string localName) => localName.Contains("DsfWebDeleteActivity")
                                                    || localName.Contains("WebGetActivity")
                                                    || localName.Contains("WebPostActivity")
                                                    || localName.Contains("WebPutActivity");

        void AddSharepointSources(XElement elementToUse)
        {
            if (elementToUse == null)
            {
                return;
            }
            if (Dependencies == null)
            {
                Dependencies = new List<IResourceForTree>();
            }
            var dependenciesFromXml = from desc in elementToUse.Descendants()
                                      where IsSharepointName(desc.Name.LocalName) && desc.HasAttributes
                                      select desc;
            var xElements = dependenciesFromXml as List<XElement> ?? dependenciesFromXml.ToList();
            foreach (var element in xElements)
            {
                var resourceIdAsString = element.AttributeSafe("SharepointServerResourceId");
                var uniqueIdAsString = element.AttributeSafe("UniqueID");
                var resourceName = element.AttributeSafe("ResourceName");
                Guid.TryParse(uniqueIdAsString, out Guid uniqueId);
                Guid.TryParse(resourceIdAsString, out Guid resId);
                var resourceForTree = Dependencies.FirstOrDefault(tree => tree.ResourceID == resId);
                if (resourceForTree == null)
                {
                    Dependencies.Add(
                        new ResourceForTree
                        {
                            ResourceID = resId,
                            UniqueID = uniqueId,
                            ResourceName = resourceName,
                            ResourceType = "SharepointSource"
                        });
                }
            }
        }

#pragma warning disable S1067 // Expressions should not be too complex
        private static bool IsSharepointName(string localName) => localName.Contains("SharepointCopyFileActivity")
                                                    || localName.Contains("SharepointCreateListItemActivity")
                                                    || localName.Contains("SharepointDeleteFileActivity")
                                                    || localName.Contains("SharepointDeleteListItemActivity")
                                                    || localName.Contains("SharepointFileDownLoadActivity")
                                                    || localName.Contains("SharepointFileUploadActivity")
                                                    || localName.Contains("SharepointMoveFileActivity")
                                                    || localName.Contains("SharepointReadFolderItemActivity")
                                                    || localName.Contains("SharepointReadListActivity")
                                                    || localName.Contains("SharepointUpdateListItemActivity");
#pragma warning restore S1067 // Expressions should not be too complex

        void AddRemoteServerDependencies(XElement element)
        {
            var environmentIdString = element.AttributeSafe("EnvironmentID");
            if (Guid.TryParse(environmentIdString, out Guid environmentId) && environmentId != Guid.Empty)
            {
                if (environmentId == Guid.Empty)
                {
                    return;
                }

                var resourceName = element.AttributeSafe("FriendlySourceName");
                Dependencies.Add(
                    new ResourceForTree
                    {
                        ResourceID = environmentId,
                        UniqueID = Guid.Empty,
                        ResourceName = resourceName,
                        ResourceType = "Server"
                    });
            }
        }

        public static void ParseProperties(string s, Dictionary<string, string> properties)
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            var props = s.Split(';');
            foreach (var p in props.Select(prop => prop.Split('=')).Where(p => p.Length >= 1))
            {
                var key = p[0];
                if (!properties.ContainsKey(key))
                {
                    continue;
                }
                properties[key] = string.Join("=", p.Skip(1));
            }
        }
    }
}