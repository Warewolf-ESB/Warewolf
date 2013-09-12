using Dev2.Common;
using Dev2.Data;
using Dev2.Data.DataListCache;
using Dev2.DataList.Contract.Binary_Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Dev2.DataList.Contract.Persistence {

    /// <summary>
    /// Basic in memory provider for client and test
    /// </summary>
    public class DataListTemporalProvider : IDataListPersistenceProvider {

        private static readonly ConcurrentDictionary<Guid, IBinaryDataList> Repo = new ConcurrentDictionary<Guid, IBinaryDataList>();
        private static readonly IList<Guid> PersistedDataList = new List<Guid>();
        private static readonly string RootPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private const string _savePath = @"Warewolf\DataListServer\";
        private static readonly string DataListPersistPath = Path.Combine(RootPath, _savePath);

        private static bool _fileSystemInit;

        private static readonly object _persistGuard = new object();

        public bool WriteDataList(Guid datalistID, IBinaryDataList datalist, ErrorResultTO errors) {
            bool result = false;

            if (datalistID != GlobalConstants.NullDataListID)
            {
                Repo[datalistID] = datalist;
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Reads the datalist.
        /// </summary>
        /// <param name="datalistID">The datalist unique identifier.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public IBinaryDataList ReadDatalist(Guid datalistID, ErrorResultTO errors) {

            IBinaryDataList result;

            if (!Repo.TryGetValue(datalistID, out result))
            {
                 if (PersistedDataList.Contains(datalistID))
                 {
                     result = LoadPersistedDataList(DataListPersistPath+datalistID.ToString());
                     if (result == null && datalistID != GlobalConstants.NullDataListID)
                     {
                         errors.AddError("Persisted DataList [ " + datalistID + " ] was not found on disk!");
                     }
                     else
                     {
                         // Remove it for now...
                     }
                 }
                 else
                 {
                     errors.AddError("Cannot locate persisted DataList [ " + datalistID + " ] from disk!");
                 }
            }

            return result;
        }

        /// <summary>
        /// Persists the child chain.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <returns></returns>
        public bool PersistChildChain(Guid id) {
            bool result = false;
            Guid myID = id;
            IBinaryDataList value;
            BinaryFormatter bf = new BinaryFormatter();

            lock (_persistGuard)
            {
                while (Repo.TryGetValue(myID, out value))
                {
                    Stream s = null;
                    try
                    {
                        s = File.Open(Path.Combine(DataListPersistPath, myID.ToString()), FileMode.OpenOrCreate);

                        bf.Serialize(s, value);
                        result = true;

                        PersistedDataList.Add(myID);
                        IBinaryDataList tmp;
                        Repo.TryRemove(myID, out tmp);
                    }
                    catch (Exception ex)
                    {
                        ServerLogger.LogError(ex);
                        // Just swallow it, we need to do best effort ;)
                        result = false;
                    }
                    finally
                    {
                        if (s != null)
                        {
                            s.Close();
                        }
                    }
                    //  move to next in the chain....
                    myID = value.ParentUID;
                }
            }

            return result;
        }

        /// <summary>
        /// Deletes the data list.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="onlyIfNotPersisted">if set to <c>true</c> [only difference not persisted].</param>
        public void DeleteDataList(Guid id, bool onlyIfNotPersisted)
        {

            bool okDelete = false;
            // check for persistence
            if (PersistedDataList.Contains(id) && !onlyIfNotPersisted)
            {
                okDelete = true;
            }
            else if (!PersistedDataList.Contains(id) && onlyIfNotPersisted)
            {
                okDelete = true;
            }

            if (okDelete)
            {
                try
                {
                    IBinaryDataList tmp;
                    if (!Repo.TryRemove(id, out tmp)) // cache miss, check persisted DL cache?
                    {
                        // Now check to see if it is persisted as well ;)
                        // Only singled thread access per id
                        if (PersistedDataList.Remove(id))
                        {
                            // delete from file system too ;)
                            File.Delete(Path.Combine(DataListPersistPath, id.ToString()));
                        }
                    }

                    BackgroundDispatcher.Instance.Add(tmp);

                }
                catch (Exception ex)
                {
                    ServerLogger.LogError(ex);
                    /* Fail safe */
                }

            }
        }



        /// <summary>
        /// Initializes the persistence.
        /// </summary>
        public void InitPersistence() {
            lock (_persistGuard)
            {
                if (!_fileSystemInit)
                {
                    if (!Directory.Exists(DataListPersistPath))
                    {
                        Directory.CreateDirectory(DataListPersistPath);
                    }

                    string[] toLoad = Directory.GetFiles(DataListPersistPath);

                    foreach (string l in toLoad)
                    {
                        IBinaryDataList tmp = LoadPersistedDataList(l);
                        if (tmp != null)
                        {
                            PersistedDataList.Add(tmp.UID); // add to persisted collection
                        }
                    }
                    _fileSystemInit = true; // signal an init
                }
            }
        }

        /// <summary>
        /// Loads the persisted data list.
        /// </summary>
        /// <param name="toLoad">The automatic load.</param>
        /// <returns></returns>
        private IBinaryDataList LoadPersistedDataList(string toLoad)
        {
            Stream s = null;
            IBinaryDataList tmp = null;
            bool badFormat = false;
            BinaryFormatter bf = new BinaryFormatter();
            try
            {
                s = File.Open(toLoad, FileMode.Open);
                if (s.Length > 0)
                {
                    tmp = (IBinaryDataList) bf.Deserialize(s);
                }
            }
            catch(Exception ex)
            {
                ServerLogger.LogError(ex);
                // Do nothing
                badFormat = true;
            }
            finally
            {
                if (s != null)
                {
                    s.Close();

                    if (badFormat)
                    {
                        try
                        {
                            // Bad format, delete
                            File.Delete(toLoad);

                        }
                        catch(Exception ex)
                        {
                            ServerLogger.LogError(ex);
                        }
                    }
                }
            }

            return tmp;
        }

    }
}
