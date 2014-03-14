using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Data.Storage.ProtocolBuffers;

namespace Dev2.Data.Storage
{
    /// <summary>
    /// A key storage struct
    /// </summary>
    public struct BinaryStorageKey
    {
        public long Position;
        public int Length;
    }

    /// <summary>
    /// Used to buffer data internally ;)
    /// </summary>
    public struct InternalStorageBuffer
    {
        public byte[] Buffer;
        public int UsedStorage;
        public int Capacity;

        /// <summary>
        /// Determines whether this instance [can fit segment] the specified seg size.
        /// </summary>
        /// <param name="segSize">Size of the seg.</param>
        /// <returns></returns>
        public bool CanFitSegment(int segSize)
        {
            // +1 to account for post insert sizing ;)
            return (Capacity - (UsedStorage + segSize) > 0);
        }

        /// <summary>
        /// Inserts the segment.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public int InsertSegment(byte[] data)
        {
            var result = UsedStorage; // start location of update ;)
            Array.Copy(data, 0, Buffer, UsedStorage, data.Length);
            UsedStorage += data.Length + 1;

            return result;
        }

        /// <summary>
        /// Fetches from buffer.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <param name="len">The length.</param>
        /// <returns></returns>
        public byte[] FetchFromBuffer(long pos, int len)
        {
            byte[] result = new byte[len];
            Array.Copy(Buffer, pos, result, 0, len);

            return result;
        }

        /// <summary>
        /// Resets the buffer.
        /// </summary>
        public void ResetBuffer()
        {
            UsedStorage = 0;
        }

        /// <summary>
        /// Transfers one buffer to another. Used by the background scrubber ;)
        /// </summary>
        /// <param name="toBuffer">The automatic buffer.</param>
        public void TransferTo(ref InternalStorageBuffer toBuffer)
        {
            System.Buffer.BlockCopy(Buffer, 0, toBuffer.Buffer, 0, UsedStorage);
            toBuffer.UsedStorage = UsedStorage;
        }
    }

    /// <summary>
    /// Used to scrub old memory slabs clean ;)
    /// </summary>
    static class CompactBuffer
    {
        public static InternalStorageBuffer scrubBuffer;

        //private static readonly object _lock = new object();

        public static void Init(int bufCap)
        {
            scrubBuffer = new InternalStorageBuffer { Buffer = new byte[bufCap], Capacity = bufCap, UsedStorage = 0 };
        }

        /// <summary>
        /// Compacts the specified from buffer.
        /// </summary>
        /// <param name="fromBuffer">From buffer.</param>
        /// <param name="indexs">The indexes.</param>
        public static void Compact(ref InternalStorageBuffer fromBuffer, ref ConcurrentDictionary<string, BinaryStorageKey> indexs)
        {
            // lock(_lock)
            //{
            scrubBuffer.ResetBuffer();

            foreach(var storageKey in indexs.Keys)
            {
                // Scrub out old data ;)
                BinaryStorageKey tmpKey;
                if(indexs.TryRemove(storageKey, out tmpKey))
                {
                    long pos = tmpKey.Position;
                    int len = tmpKey.Length;

                    var bytes = fromBuffer.FetchFromBuffer(pos, len);

                    var idx = scrubBuffer.InsertSegment(bytes);

                    tmpKey.Position = idx;

                    indexs[storageKey] = tmpKey;
                }

                //Thread.Sleep(10);
            }

            // now copy over the data ;)
            fromBuffer.ResetBuffer();
            scrubBuffer.TransferTo(ref fromBuffer);
        }
        // }
    }

    /// <summary>
    /// Disk based storage ;)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Dev2BinaryStorage<T> where T : AProtocolBuffer
    {
        // internal location data ;)
        // ReSharper disable StaticFieldInGenericType
        private static readonly string RootPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private const string _savePath = @"Warewolf\DataListServerTmp\";
        private static readonly string DataListPersistPath = Path.Combine(RootPath, _savePath);

        private static readonly object _dirLock = new object();
        private static bool startupCleaned;
        // ReSharper restore StaticFieldInGenericType

        #region Fields

        private readonly string _completeFilename = @"C:\persist.dic";
        private FileStream _file;
        private ConcurrentDictionary<string, BinaryStorageKey> _bufferIndexes = new ConcurrentDictionary<string, BinaryStorageKey>();
        private readonly ConcurrentDictionary<string, BinaryStorageKey> _lstIndexes = new ConcurrentDictionary<string, BinaryStorageKey>();
        private readonly object _opsLock = new object();
        const long _compactThresholdSize = 512 * 1024 * 1024; // 512 MB compact 
        private long _lastCompactSize;
        private bool _hasBeenRemoveSinceLastCompact;

        private int _itemCnt;
        private int _runningRemoveCnt;

        // New internal storage buffer to avoid high disk io, 256 MB of storage by default ;)
        private InternalStorageBuffer _internalBuffer;

        #endregion Fields

        #region Constructors

        public Dev2BinaryStorage(string filename) : this(filename, GlobalConstants.DefaultStorageSegmentSize) { }

        public Dev2BinaryStorage(string filename, int bufCap)
        {
            if(!string.IsNullOrEmpty(filename))
            {
                lock(_dirLock)
                {
                    if(!Directory.Exists(DataListPersistPath))
                    {
                        Directory.CreateDirectory(DataListPersistPath);
                    }
                    else
                    {
                        if(!startupCleaned)
                        {
                            try
                            {
                                foreach(FileInfo f in new DirectoryInfo(DataListPersistPath).GetFiles("*.data"))
                                {
                                    f.Delete();
                                }
                            }
                            // ReSharper disable EmptyGeneralCatchClause
                            catch(Exception)
                            // ReSharper restore EmptyGeneralCatchClause
                            {
                                // Best effort ;)
                            }
                            startupCleaned = true;
                        }
                    }
                }

                _completeFilename = Path.Combine(DataListPersistPath, filename);

            }

            _file = new FileStream(_completeFilename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            _internalBuffer = new InternalStorageBuffer { Buffer = new byte[bufCap], Capacity = bufCap, UsedStorage = 0 };
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

        #endregion

        #region Properties

        public double UsedMemoryMB()
        {
            return _internalBuffer.UsedStorage;
        }

        public double CapacityMemoryMB()
        {
            return _internalBuffer.Capacity;
        }

        public int Count
        {
            get
            {
                return (_lstIndexes.Count + _bufferIndexes.Count);
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return _lstIndexes.Keys.Union(_bufferIndexes.Keys).ToList();
            }
        }

        public int ItemCount
        {
            get
            {
                return _itemCnt;
            }
        }

        #endregion

        #region Private Methods

        private T Read(string key)
        {
            lock(_opsLock)
            {
                try
                {
                    byte[] rawData = ReadBytes(key);

                    if(rawData == null || rawData.Length == 0)
                    {
                        return null;
                    }

                    var fromBytes = ConvertFromBytes(rawData);

                    return fromBytes;
                }
                catch(Exception e)
                {
                    this.LogError(e);
                }

                return null;
            }
        }

        private byte[] ReadBytes(string key)
        {
            lock(_opsLock)
            {

                // not in file or buffer indexes!?
                if(!_lstIndexes.ContainsKey(key) && !_bufferIndexes.ContainsKey(key))
                {
                    return null;
                }

                long pos;
                int len;
                bool inBuffer;
                GetPositionLength(key, out pos, out len, out inBuffer);

                if(!inBuffer)
                {
                    var bytesRead = new byte[len];

                    _file.Seek(pos, SeekOrigin.Begin);
                    _file.Read(bytesRead, 0, len);

                    return bytesRead;
                }

                return _internalBuffer.FetchFromBuffer(pos, len);
            }
        }

        /// <summary>
        /// Compacts the backing file.
        /// </summary>
        /// <exception cref="System.Exception">
        /// Compacting data to the temp file failed.
        /// or
        /// Unable to close current file for compacting.
        /// or
        /// Unable to backup current file for compacting.
        /// or
        /// Unable to swap compacted file with the old file.
        /// </exception>
        public void Compact()
        {

            /*
             * This method is way too slow, we need to fix the speed issues ;)
             */

            if(!_hasBeenRemoveSinceLastCompact)
            {
                return;
            }

            ServerLogger.LogTrace("Compacting, things should be even slower now! [ " + DateTime.Now + " ]");

            // Get tmp file path
            string directory = Path.GetDirectoryName(_completeFilename);
            if(directory == null)
            {
                throw new Exception(
                    string.Format("Unable to create compact path. '{0}' doesn't contain a valid directory name.",
                                    _completeFilename));
            }

            string tempFile = string.Format("{0}.tmp", Guid.NewGuid());
            string tempPath = Path.Combine(directory, tempFile);

            // Open temp file to write entries to
            try
            {
                using(FileStream tmpFileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    // Write entries sequentially into the tmp file, this will exclude any removed entries.
                    foreach(var key in Keys.ToList())
                    {
                        byte[] data = ReadBytes(key);
                        if(data != null && data.Length > 0)
                        {
                            _lstIndexes[key] = new BinaryStorageKey { Length = data.Length, Position = tmpFileStream.Position };

                            tmpFileStream.Write(data, 0, data.Length);
                        }
                        else
                        {
                            BinaryStorageKey tmp;
                            _lstIndexes.TryRemove(key, out tmp);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                throw new Exception("Compacting data to the temp file failed.", e);
            }

            // Swap files
            string backupPath = string.Format("{0}{1}", _completeFilename, ".bak");

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
            catch(Exception e)
            {
                throw new Exception("Unable to backup current file for compacting.", e);
            }

            try
            {
                File.Move(tempPath, _completeFilename);
            }
            catch(Exception e)
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


            ServerLogger.LogTrace("Compacting finished! [ " + DateTime.Now + " ]");

        }

        private T ConvertFromBytes(byte[] payload)
        {

            T convertFromBytes = (T)Activator.CreateInstance(typeof(T), null);

            convertFromBytes.ToObject(payload);

            return convertFromBytes;
        }

        private void GetPositionLength(string key, out long position, out int length, out bool inBuffer)
        {
            BinaryStorageKey tmp;
            inBuffer = false;
            if(_bufferIndexes.TryGetValue(key, out tmp))
            {
                // in buffer ;)
                inBuffer = true;
            }
            else
            {
                if(!_lstIndexes.TryGetValue(key, out tmp))
                {
                    throw new Exception(string.Format("Key '{0}' doesn't exist in index.", key));
                }
            }

            position = Convert.ToInt64(tmp.Position);
            length = Convert.ToInt32(tmp.Length);
        }

        private void MoveBufferToFile()
        {
            // time to dump buffer 
            var offSet = _file.Length;
            _file.Seek(offSet, SeekOrigin.Begin);
            _file.Write(_internalBuffer.Buffer, 0, _internalBuffer.UsedStorage);

            foreach(var tmpKey in _bufferIndexes.Keys)
            {
                BinaryStorageKey bufferKeyValue;
                if(!_bufferIndexes.TryRemove(tmpKey, out bufferKeyValue))
                {
                    throw new Exception("Invalid Buffer State [ " + tmpKey + " ]");
                }

                var newPos = offSet + bufferKeyValue.Position;
                var newLen = bufferKeyValue.Length;

                BinaryStorageKey tmp;
                if(_lstIndexes.TryGetValue(tmpKey, out tmp))
                {
                    // adjust pos to reflect location in file ;)
                    tmp.Length = newLen;
                    tmp.Position = newPos;
                    _lstIndexes[tmpKey] = tmp;
                }
                else
                {
                    tmp = new BinaryStorageKey { Length = newLen, Position = newPos };
                    _lstIndexes.TryAdd(tmpKey, tmp);
                }
            }

            // reset buffer and try again ;)
            _internalBuffer.ResetBuffer();
        }

        #endregion

        #region Public Methods

        public void Add(string key, T objToAdd)
        {
            lock(_opsLock)
            {

                try
                {
                    if(!_bufferIndexes.ContainsKey(key) && !_lstIndexes.ContainsKey(key))
                    {
                        _itemCnt++;
                    }

                    byte[] data = objToAdd.ToByteArray();

                    // we need to add to buffer first ;)
                    if(_internalBuffer.CanFitSegment(data.Length))
                    {
                        // shove into the internal buffer ;)
                        var bufferIdx = _internalBuffer.InsertSegment(data);

                        _bufferIndexes[key] = new BinaryStorageKey { Length = data.Length, Position = bufferIdx };
                    }
                    else
                    {
                        // dump the buffer to file ;)
                        MoveBufferToFile();

                        // once buffer is dumped, check for compaction operation ;)
                        // TODO : Background operation so we can continue to fill buffer ;)
                        if(_file.Length - _lastCompactSize > _compactThresholdSize)
                        {
                            Compact();
                        }

                        if(_internalBuffer.CanFitSegment(data.Length))
                        {
                            // shove into the internal buffer ;)
                            var bufferIdx = _internalBuffer.InsertSegment(data);

                            _bufferIndexes[key] = new BinaryStorageKey { Length = data.Length, Position = bufferIdx };
                        }
                        else
                        {
                            // Too big, dump to file directly ;)
                            BinaryStorageKey tmp;
                            if(_lstIndexes.TryGetValue(key, out tmp))
                            {
                                tmp.Length = data.Length;
                                tmp.Position = _file.Length;
                                _lstIndexes[key] = tmp;
                            }
                            else
                            {
                                tmp = new BinaryStorageKey { Length = data.Length, Position = _file.Length };
                                _lstIndexes.TryAdd(key, tmp);
                            }

                            _file.Seek(_file.Length, SeekOrigin.Begin);
                            _file.Write(data, 0, data.Length);
                        }
                    }
                }
                catch(Exception e)
                {
                    this.LogError(e);
                }

            }
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public void Remove(string key)
        {
            lock(_opsLock)
            {
                BinaryStorageKey tmp;

                if(!_bufferIndexes.TryRemove(key, out tmp))
                {
                    if(_lstIndexes.TryRemove(key, out tmp))
                    {
                        _itemCnt--;

                        _hasBeenRemoveSinceLastCompact = true;
                    }
                }
                else
                {
                    _itemCnt--;
                    _hasBeenRemoveSinceLastCompact = true;
                }
            }
        }

        public int RemoveAll(IEnumerable<Guid> theList)
        {
            int removedItems = 0;
            lock(_opsLock)
            {
                IList<string> listOfKeys = _bufferIndexes.Keys.Where(c => theList.Any(guid => c.IndexOf(guid.ToString(), StringComparison.Ordinal) >= 0)).ToList();
                Parallel.ForEach(listOfKeys, theKey =>
                {
                    BinaryStorageKey tmp;
                    _bufferIndexes.TryRemove(theKey, out tmp);
                    removedItems++;
                    _itemCnt--;

                    var removeKeys = _lstIndexes.Keys.Where(c => c.IndexOf(theKey, StringComparison.Ordinal) >= 0);

                    foreach(var rKey in removeKeys)
                    {
                        _lstIndexes.TryRemove(rKey, out tmp);
                        _itemCnt--;
                    }

                });

                _hasBeenRemoveSinceLastCompact = true;
            }

            return removedItems;
        }

        public int RemoveAll(string key)
        {
            int removedItems = _runningRemoveCnt;
            lock(_opsLock)
            {
                BinaryStorageKey tmp;

                // remove all buffer keys ;)
                var removeKeys = _bufferIndexes.Keys.Where(c => c.IndexOf(key, StringComparison.Ordinal) >= 0);

                foreach(var rKey in removeKeys)
                {
                    _bufferIndexes.TryRemove(rKey, out tmp);
                    _itemCnt--;
                    _runningRemoveCnt++;
                }

                // now we need to pack memory to reclaim space ;)
                CompactMemory();

                // remove all file keys ;)
                removeKeys = _lstIndexes.Keys.Where(c => c.IndexOf(key, StringComparison.Ordinal) >= 0);

                foreach(var rKey in removeKeys)
                {
                    _lstIndexes.TryRemove(rKey, out tmp);
                    _itemCnt--;
                }

                _hasBeenRemoveSinceLastCompact = true;
            }

            return (removedItems - _runningRemoveCnt);
        }

        public void CompactMemory(bool force = false)
        {
            // compact when we get to a set size ;)
            if(force || _runningRemoveCnt > GlobalConstants.MemoryItemCountCompactLevel)
            {
                lock(_opsLock)
                {
                    CompactBuffer.Compact(ref _internalBuffer, ref _bufferIndexes);
                    _runningRemoveCnt = 0;
                }
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Dev2BinaryStorage()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if(!disposing)
            {
                return;
            }

            ServerLogger.LogTrace("There where [ " + _itemCnt + " ] items in this cache [ " + _savePath + " ]");

            _file.Close();
            _file.Dispose();
            // remove the file ;)

            try
            {
                File.Delete(_completeFilename);
            }
            catch(Exception ex)
            {
                this.LogError(ex);
            }
        }


        #endregion
    }
}