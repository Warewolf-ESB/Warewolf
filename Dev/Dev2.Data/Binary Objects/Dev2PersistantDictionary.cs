using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Dev2.Data.Binary_Objects
{
    public class Dev2PersistantDictionary<T>
        where T : class
    {
        #region Fields

        private readonly string _completeFilename = @"C:\persist.dic";
        private FileStream _file;
        //private readonly string _indexFile = string.Empty;
        private readonly ConcurrentDictionary<string, string> _lstIndexes = new ConcurrentDictionary<string, string>();
        private JsonSerializerSettings _settings;
        private object _opsLock = new object();
        private static readonly long _compactThresholdSize = 500 * 1024 * 1024;
        private long _lastCompactSize;
        private bool _hasBeenRemoveSinceLastCompact = false;

        #endregion Fields

        #region Constructors

        public Dev2PersistantDictionary(string filename)
        {
            if(!string.IsNullOrEmpty(filename))
            {
                _completeFilename = filename;
            }
            //_indexFile = _completeFilename + ".idx";
            _file = new FileStream(_completeFilename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            _settings = new JsonSerializerSettings();
            _settings.TypeNameHandling = TypeNameHandling.Objects;
            
            //Init();
        }

        #endregion Constructors

        #region Indexers

        public T this[string key]
        {
            get
            {
                return Read(key);
            }
            set
            {
                if(value == null)
                {
                    throw new ArgumentNullException("value", "Cannot add null to dictionary");
                }
                Add(key, value);
            }
        }

        public KeyValuePair<string, T> this[int index]
        {
            get
            {
                var keyValuePair = _lstIndexes.ElementAt(index);
                return new KeyValuePair<string, T>(keyValuePair.Key, Read(keyValuePair.Value));
            }
        }

        #endregion

        #region Properties

        public int Count
        {
            get
            {
                return _lstIndexes.Count;
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return _lstIndexes.Keys;
            }
        }

        #endregion

        #region Private Methods

        private T Read(string key)
        {
            lock (_opsLock)
            {
                byte[] rawData = ReadBytes(key);

                if (rawData == null || rawData.Length == 0)
                {
                    return null;
                }

                string val = Encoding.UTF8.GetString(rawData);
                var convertFromJsonTo = ConvertFromJsonTo(val);

                return convertFromJsonTo;
            }
        }

        private byte[] ReadBytes(string key)
        {
            lock (_opsLock)
            {
                if (!_lstIndexes.ContainsKey(key))
                {
                    return null;
                }

                long pos;
                int len;
                GetPositionLength(key, out pos, out len);

                //long jumpTo = pos - 1 * len;

                _file.Seek(pos, SeekOrigin.Begin);
                var bytesRead = new byte[len];
                _file.Read(bytesRead, 0, len);

                return bytesRead;
            }
        }

        private void Compact()
        {
            if (!_hasBeenRemoveSinceLastCompact)
            {
                return;
            }

            // Get tmp file path
            string directory = Path.GetDirectoryName(_completeFilename);
            if (directory == null)
            {
                throw new Exception(string.Format("Unable to create compact path. '{0}' doesn't contain a valid directory name.", _completeFilename));
            }

            string tempFile = string.Format("{0}.tmp", Guid.NewGuid());
            string tempPath = Path.Combine(directory, tempFile);

            // Open temp file to write entries to
            try
            {
                using (FileStream tmpFileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    // Write entries sequentially into the tmp file, this will exclude any removed entries.
                    foreach (var key in Keys.ToList())
                    {
                        byte[] data = ReadBytes(key);
                        if (data != null && data.Length > 0)
                        {
                            _lstIndexes[key] = tmpFileStream.Position + "|" + data.Length;
                            tmpFileStream.Write(data, 0, data.Length);
                        }
                        else
                        {
                            string tmp;
                            _lstIndexes.TryRemove(key, out tmp);
                        }
                    }

                    tmpFileStream.Close();
                }
            }
            catch(Exception e)
            {
                throw new Exception("Compacting data to the temp file failed.", e);
            }

            // Swap files
            string backupPath = string.Format("{0}{1}",_completeFilename, ".bak");

            try
            {
                _file.Close();
                _file.Dispose();
            }
            catch(Exception e)
            {
                throw new Exception("Unable to close current file for compacting.", e);
            }

            try
            {
                File.Move(_completeFilename, backupPath);
            }
            catch (Exception e)
            {
                throw new Exception("Unable to backup current file for compacting.", e);
            }

            try
            {
                File.Move(tempPath, _completeFilename);
            }
            catch (Exception e)
            {
                File.Move(backupPath, _completeFilename);
                File.Delete(tempPath);
                throw new Exception("Unable to swap compacted file with the old file.", e);
            }

            // Remove old file
            File.Delete(backupPath);

            // Open file again
            _file = new FileStream(_completeFilename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            _lastCompactSize = _file.Length;
            _hasBeenRemoveSinceLastCompact = false;
        }

        private T ConvertFromBytes(byte[] payload)
        {
            BinaryFormatter bf = new BinaryFormatter();
            T convertFromBytes = null;
            lock(_opsLock)
            {
                using(MemoryStream ms = new MemoryStream(payload))
                {

                    ms.Position = 0;
                    try
                    {
                        convertFromBytes = (T)bf.Deserialize(ms);
                    }
                    catch(SerializationException e)
                    {
                        Console.WriteLine(e);
                    }
                }

            }
            return convertFromBytes;
        }

        private T ConvertFromJsonTo(string payload)
        {

            try
            {
                T obj = JsonConvert.DeserializeObject<T>(payload, _settings);
                return obj;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            //            BinaryFormatter binaryFormatter = new BinaryFormatter();
            //            var serializationStream = new MemoryStream();
            //            serializationStream.Read(payload, 0, payload.Length);
            //            var deserialize = binaryFormatter.Deserialize(serializationStream);
            return null;
        }

        private string ConvertToJson(T payload)
        {
            // TODO : Fix this, it keeps bombing out ?!
            try
            {
                string result = JsonConvert.SerializeObject(payload, _settings);
                return result;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }

        private MemoryStream ConvertToStream(T payload)
        {
            // TODO : Fix this, it keeps bombing out ?!
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();

            try
            {
                bf.Serialize(ms, payload);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

            ms.Position = 0;
            return ms;

        }

        private void GetPositionLength(string key, out long position, out int length)
        {
            string tmp;
            if (!_lstIndexes.TryGetValue(key, out tmp))
            {
                throw new Exception(string.Format("Key '{0}' doesn't exist in index.", key));
            }

            position = Convert.ToInt64(tmp.Split(new[] { '|' })[0]);
            length = Convert.ToInt32(tmp.Split(new[] { '|' })[1]);
        }

        //private void Close()
        //{
        //    if (_lstIndexes == null || _lstIndexes.Count == 0)
        //    {
        //        return;
        //    }

        //    using (FileStream fs = new FileStream(_indexFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        //    {
        //        BinaryFormatter bin = new BinaryFormatter();
        //        bin.Serialize(fs, _lstIndexes);
        //    }
        //}

        //private void Init()
        //{
        //    using (FileStream fs = new FileStream(_indexFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        //    {
        //        if (fs.Length < 1)
        //        {
        //            return;
        //        }

        //        BinaryFormatter bin = new BinaryFormatter();
        //        _lstIndexes = bin.Deserialize(fs) as Dictionary<string, string>;
        //    }
        //}

        #endregion

        #region Public Methods

        public void Add(string key, T objToAdd)
        {
            lock(_opsLock)
            {
                if (_file.Length - _lastCompactSize > _compactThresholdSize)
                {
                    Compact();
                }

                var convertToJson = ConvertToJson(objToAdd);

                string tmp;
                if(_lstIndexes.TryGetValue(key, out tmp))
                {
                    _file.Seek(0, SeekOrigin.End);
                    _lstIndexes[key] = _file.Position + "|" + convertToJson.Length;
                    _file.Write(Encoding.ASCII.GetBytes(convertToJson.ToCharArray()), 0, convertToJson.Length);
                }
                else
                {
                    _file.Seek(0, SeekOrigin.End);
                    var length = convertToJson.Length;
                    _lstIndexes.TryAdd(key, _file.Position + "|" + convertToJson.Length.ToString());
                    _file.Write(Encoding.ASCII.GetBytes(convertToJson.ToCharArray()), 0, length);
                }
            }
        }

        public void Remove(string key)
        {
            lock(_opsLock)
            {
                string tmp;
                if (_lstIndexes.TryRemove(key, out tmp))
                {
                    _hasBeenRemoveSinceLastCompact = true;
                }
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }
}