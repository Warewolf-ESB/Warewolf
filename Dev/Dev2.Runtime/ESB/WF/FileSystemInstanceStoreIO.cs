//--------------------------------------------------------------------------------
// This file is part of the downloadable code for the Apress book:
// Pro WF: Windows Workflow in .NET 4.0
// Copyright (c) Bruce Bukovics.  All rights reserved.
//
// This code is provided as is without warranty of any kind, either expressed
// or implied, including but not limited to fitness for any particular purpose. 
// You may use the code for any commercial or noncommercial purpose, and combine 
// it with your own code, but cannot reproduce it in whole or in part for 
// publication purposes without prior approval. 
//--------------------------------------------------------------------------------      

using System;
using System.Activities.DurableInstancing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.DurableInstancing;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Dev2.Common;

namespace Dev2.DynamicServices
{
    /// <summary>
    /// Used to persist data to the file system for Workflow Persistence
    /// </summary>
    public class FileSystemInstanceStoreIO
    {
        private String _dataDirectory = String.Empty;

        public FileSystemInstanceStoreIO()
        {
            CreateDataDirectory();
        }

        #region Save Methods

        /// <summary>
        /// Saves all instance data.
        /// </summary>
        /// <param name="instanceId">The instance id.</param>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        /// <exception cref="System.Runtime.DurableInstancing.InstancePersistenceException"></exception>
        public Boolean SaveAllInstanceData(Guid instanceId, SaveWorkflowCommand command)
        {
            Boolean isExistingInstance;
            try
            {
                String fileName = String.Format("{0}.xml", instanceId);
                String fullPath = Path.Combine(_dataDirectory, fileName);
                isExistingInstance = File.Exists(fullPath);

                XElement root = new XElement("Instance");
                root.Add(new XAttribute("WorkflowInstanceId", instanceId));
                XDocument xml = new XDocument(root);

                NetDataContractSerializer serializer = new NetDataContractSerializer();

                XElement section = new XElement("InstanceData");
                root.Add(section);
                foreach(var entry in command.InstanceData)
                {
                    SaveSingleEntry(serializer, section, entry);
                }
                SaveInstanceDocument(fullPath, xml);
            }
            catch(Exception exception)
            {
                this.LogError(exception);
                throw new InstancePersistenceException(exception.Message, exception);
            }
            return isExistingInstance;
        }

        /// <summary>
        /// Saves all instance meta data.
        /// </summary>
        /// <param name="instanceId">The instance id.</param>
        /// <param name="command">The command.</param>
        /// <exception cref="System.Runtime.DurableInstancing.InstancePersistenceException"></exception>
        public void SaveAllInstanceMetaData(Guid instanceId, SaveWorkflowCommand command)
        {
            try
            {
                String fileName = String.Format("{0}.meta.xml", instanceId);
                String fullPath = Path.Combine(_dataDirectory, fileName);

                XElement root = new XElement("Instance");
                root.Add(new XAttribute("WorkflowInstanceId", instanceId));
                XDocument xml = new XDocument(root);

                NetDataContractSerializer serializer =
                    new NetDataContractSerializer();

                XElement section = new XElement("InstanceMetadata");
                root.Add(section);
                foreach(var entry in command.InstanceMetadataChanges)
                {
                    SaveSingleEntry(serializer, section, entry);
                }
                SaveInstanceDocument(fullPath, xml);
            }
            catch(Exception exception)
            {
                this.LogError(exception);
                throw new InstancePersistenceException(exception.Message, exception);
            }
        }

        /// <summary>
        /// Saves the single entry.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <param name="section">The section.</param>
        /// <param name="entry">The entry.</param>
        private void SaveSingleEntry(NetDataContractSerializer serializer, XElement section, KeyValuePair<XName, InstanceValue> entry)
        {
            if(entry.Value.IsDeletedValue)
            {
                return;
            }

            XElement entryElement = new XElement("Entry");
            section.Add(entryElement);
            Serialize(serializer, entryElement, "Key", entry.Key);
            Serialize(serializer, entryElement, "Value", entry.Value.Value);
            Serialize(serializer, entryElement, "Options", entry.Value.Options);
        }

        /// <summary>
        /// Saves the instance document.
        /// </summary>
        /// <param name="fullPath">The full path.</param>
        /// <param name="xml">The XML.</param>
        private static void SaveInstanceDocument(String fullPath, XDocument xml)
        {
            lock(fullPath)
            {
                using(FileStream stream = new FileStream(fullPath, FileMode.Create))
                {
                    XmlWriterSettings settings = new XmlWriterSettings { Encoding = Encoding.UTF8 };
                    using(XmlWriter writer = XmlWriter.Create(stream, settings))
                    {
                        writer.WriteRaw(xml.ToString());
                    }
                }
            }
        }

        #endregion

        #region Load Methods

        public Boolean LoadInstance(Guid instanceId, out IDictionary<XName, InstanceValue> instanceData, out IDictionary<XName, InstanceValue> instanceMetadata)
        {
            try
            {
                instanceData = new Dictionary<XName, InstanceValue>();
                instanceMetadata = new Dictionary<XName, InstanceValue>();

                String fileName = String.Format("{0}.xml", instanceId);
                String fullPath = Path.Combine(_dataDirectory, fileName);

                if(!File.Exists(fullPath))
                {
                    return false;
                }

                NetDataContractSerializer serializer = new NetDataContractSerializer();

                //load instance data
                XElement xml = XElement.Load(fullPath);
                var xElement = xml.Element("InstanceData");
                if(xElement != null)
                {
                    var entries =
                        (from e in xElement.Elements("Entry")
                         select e).ToList();
                    foreach(XElement entry in entries)
                    {
                        LoadSingleEntry(serializer, instanceData, entry);
                    }

                    //load instance metadata
                    fileName = String.Format("{0}.meta.xml", instanceId);
                    fullPath = Path.Combine(_dataDirectory, fileName);
                    xml = XElement.Load(fullPath);
                    var element = xml.Element("InstanceMetadata");
                    if(element != null)
                    {
                        entries =
                            (from e in element.Elements("Entry")
                             select e).ToList();
                    }
                    foreach(XElement entry in entries)
                    {
                        LoadSingleEntry(serializer, instanceMetadata, entry);
                    }
                }
            }
            catch(Exception exception)
            {
                this.LogError(exception);
                throw new InstancePersistenceException(exception.Message, exception);
            }

            return true;
        }

        private void LoadSingleEntry(NetDataContractSerializer serializer, IDictionary<XName, InstanceValue> instanceData, XElement entry)
        {
            XName key =
                (XName)Deserialize(serializer, entry.Element("Key"));
            Object value =
                Deserialize(serializer, entry.Element("Value"));
            InstanceValue iv = new InstanceValue(value);
            InstanceValueOptions options =
                (InstanceValueOptions)Deserialize(
                    serializer, entry.Element("Options"));
            if(!options.HasFlag(InstanceValueOptions.WriteOnly))
            {
                instanceData.Add(key, iv);
            }
        }

        #endregion

        #region Delete Methods

        public void DeleteInstance(Guid instanceId)
        {
            String fileName = String.Format("{0}.xml", instanceId);
            String fullPath = Path.Combine(_dataDirectory, fileName);

            lock(fullPath)
            {
                if(File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }


            fileName = String.Format("{0}.meta.xml", instanceId);
            fullPath = Path.Combine(_dataDirectory, fileName);

            lock(fileName)
            {
                if(File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }

        }

        #endregion

        #region Association Methods

        public string GetSaveInstanceAssociationPath(Guid instanceId, Guid instanceKeyToAssociate)
        {
            var fileName = String.Format("Key.{0}.{1}.xml", instanceKeyToAssociate, instanceId);
            return Path.Combine(_dataDirectory, fileName);
        }

        public void SaveInstanceAssociation(Guid instanceId, Guid instanceKeyToAssociate, Boolean isDelete)
        {
            try
            {
                var fullPath = GetSaveInstanceAssociationPath(instanceId, instanceKeyToAssociate);
                lock(fullPath)
                {
                    if(!isDelete)
                    {
                        if(!File.Exists(fullPath))
                        {
                            File.Create(fullPath);
                        }
                    }
                    else
                    {
                        if(File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                    }
                }
            }
            catch(Exception exception)
            {
                this.LogError(exception);
                throw new InstancePersistenceException(exception.Message, exception);
            }
        }

        public Guid GetInstanceAssociation(Guid instanceKey)
        {
            Guid instanceId = Guid.Empty;
            try
            {
                var files = Directory.GetFiles(_dataDirectory, string.Format("Key.{0}.*.xml", instanceKey));
                if(files.Length > 0)
                {
                    // TWR: Changed to use filename only as full path might also include periods!!
                    var fileName = Path.GetFileName(files[0]);
                    if(fileName != null)
                    {
                        var nodes = fileName.Split('.');
                        if(nodes.Length == 4)
                        {
                            Guid.TryParse(nodes[2], out instanceId);
                        }
                    }
                }
            }
            catch(Exception exception)
            {
                this.LogError(exception);
                throw new InstancePersistenceException(exception.Message, exception);
            }
            return instanceId;
        }

        public void DeleteInstanceAssociation(Guid instanceKey)
        {
            try
            {
                String[] files = Directory.GetFiles(_dataDirectory, String.Format("Key.*.{0}.xml", instanceKey));
                if(files.Length > 0)
                {
                    foreach(String file in files)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch(Exception exception)
            {
                this.LogError(exception);
                throw new InstancePersistenceException(exception.Message, exception);
            }
        }

        #endregion

        #region Private methods
        private static readonly object _dirLock = new object();

        private void CreateDataDirectory()
        {
            lock(_dirLock)
            {
                _dataDirectory = Path.Combine(EnvironmentVariables.ApplicationPath, "InstanceStore");
                if(!Directory.Exists(_dataDirectory))
                {
                    Directory.CreateDirectory(_dataDirectory);
                }
            }
        }

        private void Serialize(NetDataContractSerializer serializer, XElement parent, string name, object value)
        {
            XElement element = new XElement(name);
            using(MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, value);
                stream.Position = 0;

                element.Add(XElement.Load(stream));
            }
            parent.Add(element);
        }

        private Object Deserialize(NetDataContractSerializer serializer, XElement element)
        {
            Object result;
            using(MemoryStream stream = new MemoryStream())
            {
                using(XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream))
                {
                    foreach(XNode node in element.Nodes())
                    {
                        node.WriteTo(writer);
                    }

                    writer.Flush();
                    stream.Position = 0;
                    result = serializer.Deserialize(stream);
                }
            }
            return result;
        }
        #endregion


    }
}

