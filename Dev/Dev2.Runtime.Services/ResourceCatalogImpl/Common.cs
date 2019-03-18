#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System;
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
        static readonly ConcurrentDictionary<string, object> FileLocks = new ConcurrentDictionary<string, object>();
        static ConcurrentDictionary<Guid, object> WorkspaceLocks { get; } = new ConcurrentDictionary<Guid, object>();
        static readonly object LoadLock = new object();
        public static object GetFileLock(string file) => FileLocks.GetOrAdd(file, o => new object());

        public static object GetWorkspaceLock(Guid workspaceID)
        {
            lock (LoadLock)
            {
                return WorkspaceLocks.GetOrAdd(workspaceID, guid => new object());
            }
        }

        static void SetErrors(XElement resourceElement, IList<ICompileMessageTO> compileMessagesTO)
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
                    var xElements = errorMessagesElement.Elements("ErrorMessage");
                    var firstOrDefault = xElements.FirstOrDefault(element =>
                    {
                        var xAttribute = element.Attribute("InstanceID");
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

        static void UpdateIsValid(XElement resourceElement)
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

        static void UpdateXmlToDisk(IResource resource, IList<ICompileMessageTO> compileMessagesTO, StringBuilder resourceContents)
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

            var result = resourceElement.ToStringBuilder();

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
                    catch (Exception ex)
                    {
                        Transaction.Current.Rollback();
                    }
                }

            }
        }
    }
}
