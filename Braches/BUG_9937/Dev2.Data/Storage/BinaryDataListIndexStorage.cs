using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Dev2.Common;

namespace Dev2.Data.Storage
{
    /// <summary>
    /// Used to store index data ;)
    /// This class is to be used in a single thread instance, DO NOT MULTI THREAD!
    /// </summary>
    public class BinaryDataListIndexStorage : IBinaryDataListIndexStorage
    {

        #region Fields
 
        private static readonly int _compactKeyCnt = 10000;
        private static readonly double _compactFactor = 0.7; // 30% fragmentation
        private static readonly int _pageLen = (1024*1024*8); // 8MB of dadta buffered ;)
        private static readonly int keyLen = 36;
        private readonly string _idxPath;
        private FileStream _fileStream;
        private readonly int _packedLen = 48;
        private int _totalIndexs;
        private int _peekIndexs; 
        private byte[] _internalBuffer;

        private static readonly string _rootPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private const string _savePath = @"Warewolf\DataListServer\";
        

        #endregion

        #region Properties

        public int Count
        {
            get { return _totalIndexs; }
        }

        public ICollection<string> Keys
        {
            get
            {
                ICollection<string> result = new Collection<string>();

                int pos = 0;
                long readMax = (_totalIndexs * _packedLen);

                //[] buffer = new byte[_packedLen];

                while (pos < readMax)
                {
                    _fileStream.Seek(pos, SeekOrigin.Begin);
                    _fileStream.Read(_internalBuffer, 0, _packedLen);

                    SBinaryDataListIndex idx = BytesToStruct(_internalBuffer);

                    if (idx.length != 0)
                    {
                        // found a canidate key
                        result.Add(new string(idx.uniqueKey));
                    }

                    pos += _packedLen; // move index long
                }

                return result;
            }
        }

        public string IndexFilePath
        {
            get { return _idxPath; }
        }

        #endregion


        public BinaryDataListIndexStorage(string fileName)
        {
            _idxPath = Path.Combine(_rootPath, _savePath) + fileName + ".idx";
            File.Create(_idxPath, 1).Close();

            _fileStream = new FileStream(_idxPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            _internalBuffer = new byte[_packedLen];

            //_cacheStream = new MemoryStream(_pageLen); // make new of 8 MB

        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the specified key contains key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(string key)
        {
            return (FindIndex(key) >= 0);

        }

        /// <summary>
        /// Gets the length of the position.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="position">The position.</param>
        /// <param name="length">The length.</param>
        /// <exception cref="System.Exception"></exception>
        public bool GetPositionLength(string key, out long position, out int length)
        {
            SBinaryDataListIndex idx = new SBinaryDataListIndex();
            InitIdx(ref idx);

            bool res = true;

            // TODO :  Fix this so we get the correct data out ;)


            if (FindIndex(key) >= 0)
            {
                // At this point we know _internalBuffer is filled with the matching data ;)
                position = ExtractPositionFromStream(_internalBuffer);
                length = ExtractLengthFromStream(_internalBuffer);
            }
            else
            {
                position = -1;
                length = 0;
                res = false;
                //throw new Exception(string.Format("Key '{0}' doesn't exist in index.", key));
            }


            return res;
        }

        /// <summary>
        /// Adds the index.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="position">The position.</param>
        /// <param name="length">The length.</param>
        public void AddIndex(string key, long position, int length)
        {
            // first find the key if it exist ;)
            long loc = FindIndex(key);
            byte[] bytes = ConvertIndexToBytes(key, position, length);

            if (loc >= 0)
            {
                // we have a hit, update in place ;)
                _fileStream.Seek(loc, SeekOrigin.Begin);
                _fileStream.Write(bytes, 0, _packedLen);
            }
            else
            {
                // it is a new add ;)
                _fileStream.Seek(0, SeekOrigin.End);
                _fileStream.Write(bytes, 0, _packedLen);

                _totalIndexs++;
                _peekIndexs++;
            }
        }

        public void RemoveIndex(string key)
        {
            SBinaryDataListIndex idx = new SBinaryDataListIndex();
            InitIdx(ref idx);

            long idxLoc = FindIndex(key);

            if ( idxLoc >= 0)
            {
                // we have a match ;)
                long pos = (idxLoc * _packedLen);
                byte[] bytes = ConvertIndexToBytes(key, 0, 0);
                _fileStream.Seek(pos, SeekOrigin.Begin);
                _fileStream.Write(bytes, 0, _packedLen);
                _totalIndexs--;

                // do we need to compact ?
                if (((_peekIndexs - _totalIndexs) / (double)_peekIndexs <= _compactFactor) && _totalIndexs >= _compactKeyCnt)
                {
                    // if over 30% can be reclaimed and there are more then 10 000 keys to compact
                    Compact();
                }
            }
         }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (!disposing) return;

            // clean up ;)
            try
            {
                _fileStream.Close();
                _fileStream.Dispose();
            }
            catch(Exception ex)
            {
                ServerLogger.LogError(ex);
            }

            // remove file system resources
            File.Delete(_idxPath);
        }

        /// <summary>
        /// Indexes the idx.
        /// </summary>
        /// <param name="idx">The idx.</param>
        private void InitIdx(ref SBinaryDataListIndex idx)
        {
            idx.uniqueKey = new char[keyLen];
            idx.length = 0;
            idx.position = 0;
        }


        /// <summary>
        /// Compacts this instance.
        /// </summary>
        private void Compact()
        {

            //// _fileStream.Read(buffer, pos, _keyObjLen);
            //// BytesToStruct(buffer, typeof(SBinaryDataListIndex), 0, ref idx);

            //FileStream tmpAV = new FileStream(_idxPath + _compactExt, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            //int pos = 0;
            //int movePos = 0;
            //long readMax = (_peekIndexs * _packedKeyLen);

            //byte[] buffer = new byte[_packedKeyLen];

            //while (pos < readMax)
            //{
            //    _fileStream.Seek(pos, SeekOrigin.Begin);
            //    _fileStream.Read(buffer, 0, _packedKeyLen);
            //    SBinaryDataListIndex idx = BytesToStruct(buffer);

            //    if (idx.length != 0 && idx.position != 0)
            //    {
            //        // found a canidate to move
            //        tmpAV.Write(buffer, pos, _packedKeyLen);
            //        movePos += _packedKeyLen;
            //    }
            //    pos += _packedKeyLen; // move index long
            //}

            //// make the swap ;)
            //tmpAV.Close();
            //tmpAV.Dispose();

            //// swap the files ;)
            //_fileStream.Close();
            //_fileStream.Dispose();
            //File.Delete(_idxPath);

            //File.Move((_idxPath + _compactExt), _idxPath);

            //// re-init ;)
            //_fileStream = new FileStream(_idxPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);

        }

        private byte[] ConvertIndexToBytes(string key, long position, int length)
        {

            Buffer.BlockCopy(BitConverter.GetBytes(position), 0, _internalBuffer, 0, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(length), 0, _internalBuffer, 8, 4);
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(key), 0, _internalBuffer, 12, keyLen);

            return _internalBuffer;

        }

        /// <summary>
        /// Bytes to struct.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="idx">The idx.</param>
        private SBinaryDataListIndex BytesToStruct(byte[] bytes)
        {

            SBinaryDataListIndex idx = new SBinaryDataListIndex();
            InitIdx(ref idx);

            // 0 -7
            long pos = BitConverter.ToInt64(SubRangeOfBytes(bytes,0, 8),0);
            // 8 - 11
            int len = BitConverter.ToInt32(SubRangeOfBytes(bytes, 8, 4), 0);
            
            idx.length = len;
            idx.position = pos;
            idx.uniqueKey = Encoding.UTF8.GetChars(SubRangeOfBytes(bytes, 12, keyLen));

            return idx;
        }

        private char[] ExtractKeyFromStream(byte[] bytes)
        {
            return Encoding.UTF8.GetChars(SubRangeOfBytes(bytes, 12, keyLen));
        }

        private long ExtractPositionFromStream(byte[] bytes)
        {
            return  BitConverter.ToInt64(SubRangeOfBytes(bytes,0, 8),0);
        }

        private int ExtractLengthFromStream(byte[] bytes)
        {
            return BitConverter.ToInt32(SubRangeOfBytes(bytes, 8, 4), 0);
        }

        private byte[] SubRangeOfBytes(byte[] bytes, int start, int len)
        {
            byte[] result = new byte[len];

            Buffer.BlockCopy(bytes, start, result, 0, len);

            return result;
        }

        /// <summary>
        /// Finds the index.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <param name="searchKey">The search key.</param>
        /// <returns></returns>
        private long FindIndex(string searchKey)
        {
            long pos = 0;
            long readMax = (_totalIndexs * _packedLen);

            char[] toMatch = searchKey.ToCharArray();

            _fileStream.Seek(pos, SeekOrigin.Begin);
            while (pos < readMax)
            {
                byte[] readBuffer = new byte[_packedLen];
                long readLen;

                // Page in 8MB at a time or len if less ~ 1.7 million keys at a time ;)
                if (_fileStream.Length <= _pageLen)
                {
                    // take it all ;)
                    readLen = (int)_fileStream.Length;
                    readBuffer = new byte[readLen];
                }
                else
                {
                    // we need to page 1MB / to end at a time ;)
                    readLen = _pageLen;
                    readBuffer = new byte[readLen];
                }

                _fileStream.Read(readBuffer, 0, (int)readLen);

                // Now break up into smaller parts for manip ;)
                int offset = 0;

                while (offset < readLen)
                {
                    _internalBuffer = SubRangeOfBytes(readBuffer, offset, _packedLen);

                    if (KeysMatch(ExtractKeyFromStream(_internalBuffer), toMatch))
                    {
                        return pos;
                    }

                    offset += _packedLen;
                }

                pos += readLen; // move index long
            }

            return -1;
        }

       
        private bool KeysMatch(char[] toExamine, char[] target)
        {
            bool result = true;
            int pos = 0;
            while (pos < keyLen && result)
            {
                if (toExamine[pos] != target[pos])
                {
                    result = false;
                    break;
                }
                pos++;
            }

            return result;
        }
    }


}
