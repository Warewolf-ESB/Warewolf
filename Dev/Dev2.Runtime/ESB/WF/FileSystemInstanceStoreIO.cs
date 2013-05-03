using Dev2.Common;
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
        public Boolean SaveAllInstanceData(Guid instanceId,SaveWorkflowCommand command)
        {
            Boolean isExistingInstance = false;
            try
            {
                String fileName = String.Format("{0}.xml", instanceId);
                String fullPath = Path.Combine(_dataDirectory, fileName);
                isExistingInstance = File.Exists(fullPath);

                XElement root = new XElement("Instance");
                root.Add(new XAttribute("InstanceId", instanceId));
                XDocument xml = new XDocument(root);

                NetDataContractSerializer serializer = new NetDataContractSerializer();

                XElement section = new XElement("InstanceData");
                root.Add(section);
                foreach (var entry in command.InstanceData)
                {
                    SaveSingleEntry(serializer, section, entry);
                }
                SaveInstanceDocument(fullPath, xml);
            }
            catch (Exception exception)
            {
               ServerLogger.LogError(string.Format("SaveAllInstanceData Exception: {0}", exception.Message));
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
        public void SaveAllInstanceMetaData(Guid instanceId,SaveWorkflowCommand command)
        {
            try
            {
                String fileName = String.Format("{0}.meta.xml", instanceId);
                String fullPath = Path.Combine(_dataDirectory, fileName);

                XElement root = new XElement("Instance");
                root.Add(new XAttribute("InstanceId", instanceId));
                XDocument xml = new XDocument(root);

                NetDataContractSerializer serializer =
                    new NetDataContractSerializer();

                XElement section = new XElement("InstanceMetadata");
                root.Add(section);
                foreach (var entry in command.InstanceMetadataChanges)
                {
                    SaveSingleEntry(serializer, section, entry);
                }
                SaveInstanceDocument(fullPath, xml);
            }
            catch (Exception exception)
            {
                ServerLogger.LogError(string.Format("SaveAllMetaData Exception: {0}", exception.Message));
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
            if (entry.Value.IsDeletedValue)
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
            lock (fullPath)
            {
                using (FileStream stream = new FileStream(fullPath, FileMode.Create))
                {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Encoding = Encoding.UTF8;
                    using (XmlWriter writer = XmlWriter.Create(stream, settings))
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
            Boolean result = false;
            try
            {
                instanceData = new Dictionary<XName, InstanceValue>();
                instanceMetadata = new Dictionary<XName, InstanceValue>();

                String fileName = String.Format("{0}.xml", instanceId);
                String fullPath = Path.Combine(_dataDirectory, fileName);

                if (!File.Exists(fullPath))
                {
                    return result;
                }

                NetDataContractSerializer serializer = new NetDataContractSerializer();

                //load instance data
                XElement xml = XElement.Load(fullPath);
                var entries =
                    (from e in xml.Element("InstanceData").Elements("Entry")
                     select e).ToList();
                foreach (XElement entry in entries)
                {
                    LoadSingleEntry(serializer, instanceData, entry);
                }

                //load instance metadata
                fileName = String.Format("{0}.meta.xml", instanceId);
                fullPath = Path.Combine(_dataDirectory, fileName);
                xml = XElement.Load(fullPath);
                entries =
                    (from e in xml.Element(
                         "InstanceMetadata").Elements("Entry")
                     select e).ToList();
                foreach (XElement entry in entries)
                {
                    LoadSingleEntry(serializer, instanceMetadata, entry);
                }

                result = true;
            }
            catch (Exception exception)
            {
                ServerLogger.LogError(string.Format("LoadInstance Exception: {0}", exception.Message));
                throw new InstancePersistenceException(exception.Message, exception);
            }
            return result;
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
            if (!options.HasFlag(InstanceValueOptions.WriteOnly))
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

            lock (fullPath)
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }    
            }
            

            fileName = String.Format("{0}.meta.xml", instanceId);
            fullPath = Path.Combine(_dataDirectory, fileName);

            lock (fileName)
            {
                if (File.Exists(fullPath))
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
                lock (fullPath)
                {
                    if (!isDelete)
                    {
                        if (!File.Exists(fullPath))
                        {
                            File.Create(fullPath);
                        }
                    }
                    else
                    {
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                ServerLogger.LogError(string.Format("PersistInstanceAssociation Exception: {0}", exception.Message));
                throw new InstancePersistenceException(exception.Message, exception);
            }
        }

        public Guid GetInstanceAssociation(Guid instanceKey)
        {
            Guid instanceId = Guid.Empty;
            try
            {
                var files = Directory.GetFiles(_dataDirectory, string.Format("Key.{0}.*.xml", instanceKey));
                if (files.Length > 0)
                {
                    // TWR: Changed to use filename only as full path might also include periods!!
                    var fileName = Path.GetFileName(files[0]);
                    if (fileName != null)
                    {
                        var nodes = fileName.Split('.');
                        if (nodes.Length == 4)
                        {
                            Guid.TryParse(nodes[2], out instanceId);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                ServerLogger.LogError(string.Format("GetInstanceAssociation Exception: {0}", exception.Message));
                throw new InstancePersistenceException(exception.Message, exception);
            }
            return instanceId;
        }

        public void DeleteInstanceAssociation(Guid instanceKey)
        {
            try
            {
                String[] files = Directory.GetFiles(_dataDirectory,String.Format("Key.*.{0}.xml", instanceKey));
                if (files.Length > 0)
                {
                    foreach (String file in files)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception exception)
            {
                ServerLogger.LogError(string.Format("DeleteInstanceAssociation Exception: {0}", exception.Message));
                throw new InstancePersistenceException(exception.Message, exception);
            }
        }

        #endregion

        #region Private methods
        private static object _dirLock = new object();

        private void CreateDataDirectory()
        {
            lock (_dirLock)
            {
                _dataDirectory = Path.Combine(EnvironmentVariables.ApplicationPath, "InstanceStore");
                if (!Directory.Exists(_dataDirectory))
                {
                    Directory.CreateDirectory(_dataDirectory);
                }
            }
        }

        private XElement Serialize(NetDataContractSerializer serializer, XElement parent, String name, Object value)
        {
            XElement element = new XElement(name);
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, value);
                stream.Position = 0;

                element.Add(XElement.Load(stream));

                // Travis.Frisinger
                //using (StreamReader reader = new StreamReader(stream))
                //{
                //    element.Add(XElement.Load(stream));
                //}
            }
            parent.Add(element);
            return element;
        }

        private Object Deserialize(NetDataContractSerializer serializer, XElement element)
        {
            Object result = null;
            using (MemoryStream stream = new MemoryStream())
            {
                using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream))
                {
                    foreach (XNode node in element.Nodes())
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

        //private const int CurrentFileSystemVersion = 1;

        //private Dictionary<Guid, FileSystemEntry> _fileEntries;
        //private string _entryFilePath;

        //private object DeserializeObjectFromFileEntry(Guid guid)
        //{
        //    object result = null;
        //    FileSystemEntry entry;

        //    if (_fileEntries.TryGetValue(guid, out entry))
        //    {
        //        NetDataContractSerializer serializer = new NetDataContractSerializer();

        //        using (FileStream stream = new FileStream(_entryFilePath, FileMode.Open))
        //        {
        //            stream.Seek(entry.Position, SeekOrigin.Begin);
        //            result = serializer.Deserialize(stream);
        //        }
        //    }

        //    return result;
        //}

        //private unsafe void SerializeFileEntries(string filepath)
        //{
        //    if (File.Exists(filepath))
        //    {
        //        _entryFilePath = filepath;
        //        if (File.Exists(filepath + ".bak")) File.Delete(filepath + ".bak");
        //        File.Copy(filepath, filepath + ".bak");

        //        byte[] data = new byte[__FileSystemHeader.SizeInBytes + ((__FileSystemEntry.SizeInBytes * _fileEntries.Count) + (16 * _fileEntries.Count))];
        //        __FileSystemHeader header = new __FileSystemHeader() { Version = CurrentFileSystemVersion, TotalEntries = _fileEntries.Count };

        //        fixed (byte* pBuffer = data)
        //        {
        //            *((__FileSystemHeader*)(pBuffer)) = header;
        //            int ePosition = __FileSystemHeader.SizeInBytes;
        //            int offset = (__FileSystemEntry.SizeInBytes * _fileEntries.Count) + ePosition;

        //            foreach (KeyValuePair<Guid, FileSystemEntry> kvp in _fileEntries)
        //            {
        //                *((__FileSystemEntry*)(pBuffer + ePosition)) = kvp.Value.ToRawEntry();
        //                byte[] temp = kvp.Key.ToByteArray();
        //                Buffer.BlockCopy(temp, 0, data, offset, temp.Length);
        //                offset += temp.Length;
        //                ePosition += __FileSystemEntry.SizeInBytes;
        //            }
        //        }


        //        using (FileStream stream = File.Create(filepath, 4096, FileOptions.SequentialScan))
        //        {
        //            stream.Write(data, 0, data.Length);
        //        }
        //    }
        //}

        //private unsafe void DeserializeFileEntries(string filepath)
        //{
        //    Dictionary<Guid, FileSystemEntry> mappedEntries = new Dictionary<Guid, FileSystemEntry>();

        //    if (File.Exists(filepath))
        //    {
        //        byte[] hBuffer = new byte[__FileSystemHeader.SizeInBytes];

        //        using (FileStream stream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.None))
        //        {
        //            stream.Read(hBuffer, 0, __FileSystemHeader.SizeInBytes);
        //            __FileSystemHeader header;
        //            fixed (byte* pHeader = hBuffer) header = *((__FileSystemHeader*)pHeader);

        //            byte[] eBuffer = new byte[header.TotalEntries * __FileSystemEntry.SizeInBytes];
        //            stream.Read(eBuffer, 0, eBuffer.Length);

        //            __FileSystemEntry[] entries = new __FileSystemEntry[header.TotalEntries];

        //            fixed (byte* pHeader = eBuffer)
        //            {
        //                for (int i = 0; i < entries.Length; i++)
        //                    entries[i] = *((__FileSystemEntry*)(pHeader + (i * __FileSystemEntry.SizeInBytes)));
        //            }

        //            stream.Read(eBuffer, 0, 16 * entries.Length);
        //            hBuffer = new byte[16];

        //            for (int i = 0; i < entries.Length; i++)
        //            {
        //                Buffer.BlockCopy(eBuffer, i * 16, hBuffer, 0, 16);
        //                Guid identifier = new Guid(hBuffer);
        //                mappedEntries.Add(identifier, new FileSystemEntry(entries[i], identifier, i));
        //            }
        //        }
        //    }

        //    _fileEntries = mappedEntries;
        //}

        //private sealed class FileSystemEntry
        //{
        //    private int _index;
        //    private int _position;
        //    private int _length;
        //    private DateTime _timestamp;
        //    private Guid _identifier;

        //    public int Index { get { return _index; } }
        //    public int Position { get { return _position; } }
        //    public int Length { get { return _length; } }
        //    public DateTime Timestamp { get { return _timestamp; } }
        //    public Guid Identifier { get { return _identifier; } }

        //    public FileSystemEntry(__FileSystemEntry entry, Guid identifier, int index)
        //    {
        //        _index = index;
        //        _position = entry.Position;
        //        _length = entry.Length;
        //        _timestamp = DateTime.FromOADate(entry.Timestamp);
        //        _identifier = identifier;
        //    }

        //    public __FileSystemEntry ToRawEntry()
        //    {
        //        return new __FileSystemEntry() { Position = _position, Length = _length, Timestamp = _timestamp.ToOADate() };
        //    }
        //}

        //[StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
        //private struct __FileSystemHeader
        //{
        //    public const int SizeInBytes = 8;

        //    [FieldOffset(0)]
        //    public int Version;
        //    [FieldOffset(4)]
        //    public int TotalEntries;
        //}

        //[StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
        //private struct __FileSystemEntry
        //{
        //    public const int SizeInBytes = 16;

        //    [FieldOffset(0)]
        //    public int Position;
        //    [FieldOffset(4)]
        //    public int Length;
        //    [FieldOffset(8)]
        //    public double Timestamp;
        //}
    }
}

