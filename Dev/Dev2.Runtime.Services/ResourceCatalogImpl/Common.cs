using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Xml.Linq;
using ChinhDo.Transactions;
using Dev2.Common.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using ServiceStack.Common.Extensions;

namespace Dev2.Runtime.ResourceCatalogImpl
{
    public static  class Common
    {
        private static readonly ConcurrentDictionary<string, object> FileLocks = new ConcurrentDictionary<string, object>();
        private static ConcurrentDictionary<Guid, object> WorkspaceLocks { get; } = new ConcurrentDictionary<Guid, object>();
        static readonly object LoadLock = new object();
        public static object GetFileLock(string file)
        {
            return FileLocks.GetOrAdd(file, o => new object());
        }
        public static object GetWorkspaceLock(Guid workspaceID)
        {
            lock (LoadLock)
            {
                return WorkspaceLocks.GetOrAdd(workspaceID, guid => new object());
            }
        }

        private static  void SetErrors(XElement resourceElement, IList<ICompileMessageTO> compileMessagesTO)
        {
            if (compileMessagesTO == null || compileMessagesTO.Count == 0)
            {
                return;
            }
            var errorMessagesElement = GetErrorMessagesElement(resourceElement);
            if (errorMessagesElement == null)
            {
                errorMessagesElement = new XElement("ErrorMessages");
                resourceElement.Add(errorMessagesElement);
            }
            else
            {
                compileMessagesTO.ForEach(to =>
                {
                    IEnumerable<XElement> xElements = errorMessagesElement.Elements("ErrorMessage");
                    XElement firstOrDefault = xElements.FirstOrDefault(element =>
                    {
                        XAttribute xAttribute = element.Attribute("InstanceID");
                        if (xAttribute != null)
                        {
                            return xAttribute.Value == to.UniqueID.ToString();
                        }
                        return false;
                    });
                    firstOrDefault?.Remove();
                });

            }

            foreach (var compileMessageTO in compileMessagesTO)
            {
                var errorMessageElement = new XElement("ErrorMessage");
                errorMessagesElement.Add(errorMessageElement);
                errorMessageElement.Add(new XAttribute("InstanceID", compileMessageTO.UniqueID));
                errorMessageElement.Add(new XAttribute("Message", compileMessageTO.MessageType.GetDescription()));
                errorMessageElement.Add(new XAttribute("ErrorType", compileMessageTO.ErrorType));
                errorMessageElement.Add(new XAttribute("MessageType", compileMessageTO.MessageType));
                errorMessageElement.Add(new XAttribute("FixType", compileMessageTO.ToFixType()));
                errorMessageElement.Add(new XAttribute("StackTrace", ""));
                errorMessageElement.Add(new XCData(compileMessageTO.MessagePayload));
            }
        }

        static XElement GetErrorMessagesElement(XElement resourceElement)
        {
            var errorMessagesElement = resourceElement.Element("ErrorMessages");
            return errorMessagesElement;
        }

        private static void UpdateIsValid(XElement resourceElement)
        {
            var isValid = false;
            var isValidAttrib = resourceElement.Attribute("IsValid");
            var errorMessagesElement = resourceElement.Element("ErrorMessages");
            if (errorMessagesElement == null || !errorMessagesElement.HasElements)
            {
                isValid = true;
            }
            if (isValidAttrib == null)
            {
                resourceElement.Add(new XAttribute("IsValid", isValid));
            }
            else
            {
                isValidAttrib.SetValue(isValid);
            }
        }

        public static string SanitizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "";
            }

            if (path.ToLower().StartsWith("root\\"))
            {
                path = path.Remove(0, 5);
            }

            if (path.ToLower().Equals("root"))
            {
                path = path.Remove(0, 4);
            }

            if (path.StartsWith("\\"))
            {
                path = path.Remove(0, 1);
            }

            return path.Replace("\\\\", "\\")
                 .Replace("\\\\", "\\");
        }

        public static void UpdateResourceXml(IResourceCatalog  resourceCatalog,  Guid workspaceID, IResource effectedResource, IList<ICompileMessageTO> compileMessagesTO)
        {
            var resourceContents = resourceCatalog.GetResourceContents(workspaceID, effectedResource.ResourceID);
            UpdateXmlToDisk(effectedResource, compileMessagesTO, resourceContents);
            var serverResource = resourceCatalog.GetResource(Guid.Empty, effectedResource.ResourceName);
            if (serverResource != null)
            {
                resourceContents = resourceCatalog.GetResourceContents(Guid.Empty, serverResource.ResourceID);
                UpdateXmlToDisk(serverResource, compileMessagesTO, resourceContents);
            }
        }

       private static void UpdateXmlToDisk(IResource resource, IList<ICompileMessageTO> compileMessagesTO, StringBuilder resourceContents)
        {

            var resourceElement = resourceContents.ToXElement();
            if (compileMessagesTO.Count > 0)
            {
                SetErrors(resourceElement, compileMessagesTO);
                UpdateIsValid(resourceElement);
            }
            else
            {
                UpdateIsValid(resourceElement);
            }

            StringBuilder result = resourceElement.ToStringBuilder();

            var signedXml = HostSecurityProvider.Instance.SignXml(result);

            lock (GetFileLock(resource.FilePath))
            {
                var fileManager = new TxFileManager();
                using (TransactionScope tx = new TransactionScope())
                {
                    try
                    {
                        signedXml.WriteToFile(resource.FilePath, Encoding.UTF8, fileManager);
                        tx.Complete();
                    }
                    catch
                    {
                        Transaction.Current.Rollback();
                    }
                }

            }
        }
    }
}
