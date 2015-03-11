/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Dev2.Common;
using Dev2.PathOperations.Interfaces;
using Dev2.Reflection;

namespace Dev2.PathOperations
{
    /// <summary>
    ///     A file system repository.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    public class FileRepository<TKey, TItem> : IRepository<TKey, TItem>
        where TItem : class, IRepositoryItem<TKey>
    {
        private readonly string _fileExtension;
        private readonly ReaderWriterLockSlim _fileLock = new ReaderWriterLockSlim();
        private readonly ConcurrentDictionary<TKey, TItem> _items = new ConcurrentDictionary<TKey, TItem>();
        private readonly string _repositoryPath;

        #region Ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="FileRepository{TKey, TItem}" /> class.
        /// </summary>
        /// <param name="path">The path of the repository.</param>
        /// <param name="fileExtension">The file extension.</param>
        /// <exception cref="System.ArgumentNullException">path</exception>
        protected FileRepository(string path, string fileExtension)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }
            if (string.IsNullOrEmpty(fileExtension))
            {
                throw new ArgumentNullException("fileExtension");
            }
            _repositoryPath = path;
            _fileExtension = fileExtension.StartsWith(".") ? fileExtension : "." + fileExtension;

            Directory.CreateDirectory(_repositoryPath);
        }

        #endregion

        #region Count

        /// <summary>
        ///     Gets the number of items in the repository.
        /// </summary>
        public int Count
        {
            get { return _items.Count; }
        }

        #endregion

        #region Get

        /// <summary>
        ///     Gets the item with the specified key.
        /// </summary>
        /// <param name="key">The key to be queried.</param>
        /// <param name="force"><code>true</code> if the item should be re-read even it is found; <code>false</code> otherwise.</param>
        /// <returns>The item with specified key; or <code>null</code> if not found.</returns>
        /// <exception cref="System.ArgumentNullException">key</exception>
        public virtual TItem Get(TKey key, bool force = false)
        {
            if (IsNullHelper<TKey>.IsNull(key))
            {
                throw new ArgumentNullException("key");
            }

            TItem item;
            _fileLock.EnterUpgradeableReadLock();
            try
            {
                if (force || !_items.TryGetValue(key, out item))
                {
                    _fileLock.EnterWriteLock();
                    try
                    {
                        item = Read(key);
                        OnAfterGet(item);
                        _items[key] = item;
                    }
                    finally
                    {
                        _fileLock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                _fileLock.ExitUpgradeableReadLock();
            }
            return item;
        }

        #endregion

        #region Save

        /// <summary>
        ///     Saves the specified item to the repository.
        /// </summary>
        /// <param name="item">The item to be saved.</param>
        public void Save(TItem item)
        {
            if (item == null)
            {
                return;
            }

            _fileLock.EnterWriteLock();
            try
            {
                Write(item);
                _items[item.Key] = item;
            }
            finally
            {
                _fileLock.ExitWriteLock();
            }
        }

        #endregion

        #region Delete

        /// <summary>
        ///     Deletes the specified item from the repository.
        /// </summary>
        /// <param name="item">The item to be deleted.</param>
        public void Delete(TItem item)
        {
            if (item == null)
            {
                return;
            }

            _fileLock.EnterWriteLock();
            try
            {
                TItem result;
                _items.TryRemove(item.Key, out result);
                Delete(item.Key);
            }
            finally
            {
                _fileLock.ExitWriteLock();
            }
        }

        #endregion

        #region Read

        /// <summary>
        ///     Reads the item with the specified key from the file system.
        /// </summary>
        /// <param name="key">The key to be queried.</param>
        /// <returns>The item with the specified key or <code>null</code> if not found or deserialization failed.</returns>
        private TItem Read(TKey key)
        {
            string filePath = GetFileName(key);
            bool fileExists = File.Exists(filePath);
            using (FileStream stream = File.Open(filePath, FileMode.OpenOrCreate))
            {
                var formatter = new BinaryFormatter();
                if (fileExists)
                {
                    try
                    {
                        return (TItem) formatter.Deserialize(stream);
                    }
                        // ReSharper disable EmptyGeneralCatchClause 
                    catch (Exception ex)
                        // ReSharper restore EmptyGeneralCatchClause
                    {
                        Dev2Logger.Log.Error(ex);
                    }
                }
            }

            return null;
        }

        #endregion

        #region Write

        /// <summary>
        ///     Writes the specified item to the file system.
        /// </summary>
        /// <param name="item">The item to be written.</param>
        private void Write(TItem item)
        {
            if (item == null)
            {
                return;
            }

            string filePath = GetFileName(item.Key);
            using (FileStream stream = File.Open(filePath, FileMode.OpenOrCreate))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, item);
            }
        }

        #endregion

        #region Delete

        /// <summary>
        ///     Deletes the item with specified key from the file system.
        /// </summary>
        /// <param name="key">The key of the item to be deleted.</param>
        private void Delete(TKey key)
        {
            string filePath = GetFileName(key);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        #endregion

        #region GetFileName

        /// <summary>
        ///     Gets the name of the file for the given key.
        /// </summary>
        /// <param name="key">The key to be queried.</param>
        /// <returns>The name of the file for the given key.</returns>
        private string GetFileName(TKey key)
        {
            return Path.Combine(_repositoryPath, key + _fileExtension);
        }

        #endregion

        #region OnAfterGet

        /// <summary>
        ///     Called after <see cref="Get" />ting the item
        ///     but before it is loaded into the dictionary.
        /// </summary>
        /// <param name="item">The item that was read.</param>
        protected virtual void OnAfterGet(TItem item)
        {
        }

        #endregion
    }
}