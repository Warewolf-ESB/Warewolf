using Dev2.Common;
using Dev2.Data;
using Dev2.DataList.Contract.Binary_Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Dev2.DataList.Contract.Persistence {

    /// <summary>
    /// Basic in memory provider for client and test
    /// </summary>
    public class DataListTemporalProvider : IDataListPersistenceProvider {

        private static readonly ConcurrentDictionary<Guid, IBinaryDataList> _repo = new ConcurrentDictionary<Guid, IBinaryDataList>();
        private static readonly IList<Guid> _persistedDataList = new List<Guid>();
        private static readonly string _rootPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private const string _savePath = @"Dev2\DataListServer\";
        private static readonly string _dataListPersistPath = Path.Combine(_rootPath, _savePath); //_debugLoc + "\\persistSettings.dat";

        private static bool fileSystemInit = false;

        private static object _persistGuard = new object();
        private static List<Task> _tasks = new List<Task>();

        // Total number of deletes before we force a GC... Tmp fix for the issue ;)
        //private static readonly int _deleteReclaimCnt = 5;
        //private static int _deleteCnt = 0;

        public bool WriteDataList(Guid datalistID, IBinaryDataList datalist, ErrorResultTO errors) {
            bool result = false;

            if (datalistID != GlobalConstants.NullDataListID)
            {
                _repo[datalistID] = datalist;
                result = true;
            }

            return result;
        }

        public IBinaryDataList ReadDatalist(Guid datalistID, ErrorResultTO errors) {

            IBinaryDataList result;

            if (!_repo.TryGetValue(datalistID, out result))
            {
                 if (_persistedDataList.Contains(datalistID))
                 {
                     result = LoadPersistedDataList(_dataListPersistPath+datalistID.ToString());
                     if (result == null && datalistID != GlobalConstants.NullDataListID)
                     {
                         errors.AddError("Persisted DataList [ " + datalistID + " ] was not found on disk!");
                     }
                     else
                     {
                         // Remove it for now...
                         //DeleteDataList(datalistID, false);
                     }
                 }
                 else
                 {
                     errors.AddError("Cannot locate persisted DataList [ " + datalistID + " ] from disk!");
                 }
            }

            return result;
        }

        public bool PersistChildChain(Guid id) {
            bool result = false;
            Guid myID = id;
            IBinaryDataList value;
            BinaryFormatter bf = new BinaryFormatter();

            lock (_persistGuard)
            {
                while (_repo.TryGetValue(myID, out value))
                {
                    Stream s = null;
                    try
                    {
                        s = File.Open(Path.Combine(_dataListPersistPath, myID.ToString()), FileMode.OpenOrCreate);

                        bf.Serialize(s, value);
                        result = true;

                        _persistedDataList.Add(myID);
                        IBinaryDataList tmp;
                        _repo.TryRemove(myID, out tmp);
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

        public void DeleteDataList(Guid id, bool onlyIfNotPersisted)
        {

            bool okDelete = false;
            // check for persistence
            if (_persistedDataList.Contains(id) && !onlyIfNotPersisted)
            {
                okDelete = true;
            }
            else if (!_persistedDataList.Contains(id) && onlyIfNotPersisted)
            {
                okDelete = true;
            }

            if (okDelete)
            {
                try
                {
                    IBinaryDataList tmp;
                    if (!_repo.TryRemove(id, out tmp)) // cache miss, check persisted DL cache?
                    {
                        // Now check to see if it is persisted as well ;)
                        // Only singled thread access per id
                        if (_persistedDataList.Remove(id))
                        {
                            // delete from file system too ;)
                            File.Delete(Path.Combine(_dataListPersistPath, id.ToString()));
                        }
                    }

                    if (tmp != null)
                    {
                        BackgroundDispatcher.Instance.Add(tmp);
                    }

                }
                catch (Exception ex)
                {
                    ServerLogger.LogError(ex);
                    /* Fail safe */
                }

            }
        }

   

        public void InitPersistence() {
            lock (_persistGuard)
            {
                if (!fileSystemInit)
                {
                    if (!Directory.Exists(_dataListPersistPath))
                    {
                        Directory.CreateDirectory(_dataListPersistPath);
                    }

                    string[] toLoad = Directory.GetFiles(_dataListPersistPath);

                    IBinaryDataList tmp = null;
                    
                    foreach (string l in toLoad)
                    {
                        tmp = LoadPersistedDataList(l);
                        if (tmp != null)
                        {
                            _persistedDataList.Add(tmp.UID); // add to persisted collection
                        }
                    }
                    fileSystemInit = true; // singal an init
                }
            }
        }

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
