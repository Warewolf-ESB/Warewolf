//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Runtime.Serialization.Formatters.Binary;
//using Dev2.Common;
//using Dev2.DataList.Contract.Binary_Objects;
//using System.Collections.Concurrent;
//using Ionic.Zip;

//namespace Dev2.DataList.Contract.Persistence
//{

//    /// <summary>
//    /// Basic persistence provider 
//    /// </summary>
//    public class DataListTemporalProvider : IDataListPersistenceProvider
//    {

//        private static readonly IList<Guid> _activeDataList = new List<Guid>();

//        private static readonly ConcurrentDictionary<Guid, IBinaryDataList> _repo = new ConcurrentDictionary<Guid, IBinaryDataList>();
//        private static readonly IList<Guid> _persistedDataList = new List<Guid>();
//        private static readonly string _rootPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
//        private const string _savePath = @"Dev2\DataListServer\";
//        private const string _saveName = "Dev2DataList.dle";
//        private const string _archiveName = "Dev2BinaryDataListEntry";
//        private const string _archivePassword = "Dev2PasswordBDL3Pwd__3d9117ff@ee025bc1a5eb!";
//        private static readonly string _dataListPersistPath = Path.Combine(_rootPath, _savePath); //_debugLoc + "\\persistSettings.dat";

//        private static bool fileSystemInit = false;

//        private static object _persistGuard = new object();
//        private static object _activeDLGuard = new object();

//        public bool WriteDataList(Guid datalistID, IBinaryDataList datalist, ErrorResultTO errors)
//        {
//            bool result = false;
//            FileStream fs;

//            if (datalistID != GlobalConstants.NullDataListID)
//            {
//                string dlStr = datalistID.ToString();
//                string basePath = Path.Combine(_dataListPersistPath, dlStr);
//                string path = Path.Combine(basePath, _saveName);
//                if (!_activeDataList.Contains(datalistID))
//                {
//                    lock (_activeDLGuard)
//                    {
//                        _activeDataList.Add(datalistID);
//                    }

//                    // init the structure
//                    Directory.CreateDirectory(basePath);

//                    fs = new FileStream(path, FileMode.CreateNew);
//                }
//                else
//                {
//                    fs = new FileStream(path, FileMode.Create);
//                }

//                BinaryFormatter bf = new BinaryFormatter();

//                // http://stackoverflow.com/questions/964697/how-to-compress-a-net-object-instance-using-gzip
//                try
//                {
//                    // Compress with Zip stream ;)
//                    using (ZipOutputStream zip = new ZipOutputStream(fs, false))
//                    {
//                        zip.Password = _archivePassword;
//                        zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestSpeed;
//                        zip.PutNextEntry(_archiveName);
//                        bf.Serialize(zip, datalist);
//                        zip.Flush();
//                        zip.Dispose();
//                    }
//                    result = true;
//                }
//                finally
//                {
//                    fs.Close();
//                    fs.Dispose();
//                }
//            }


//            return result;
//        }

//        public IBinaryDataList ReadDatalist(Guid datalistID, ErrorResultTO errors)
//        {

//            IBinaryDataList result = null;
//            BinaryFormatter bf = new BinaryFormatter();

//            if (_activeDataList.Contains(datalistID))
//            {
//                FileStream fs = new FileStream(Path.Combine(_dataListPersistPath, datalistID.ToString(), _saveName), FileMode.Open);
//                try
//                {
//                    // Compress with Zip stream ;)
//                    using (ZipInputStream zip = new ZipInputStream(fs, false))
//                    {
//                        zip.Password = _archivePassword;
//                        zip.GetNextEntry();
//                        result = (IBinaryDataList)bf.Deserialize(zip);

//                        zip.Dispose();
//                    }
//                }
//                finally
//                {
//                    fs.Close();
//                    fs.Dispose();
//                }
//            }

//            return result;
//        }

//        public bool PersistChildChain(Guid id)
//        {
//            bool result = false;
//            Guid myID = id;
//            IBinaryDataList value;
//            BinaryFormatter bf = new BinaryFormatter();

//            lock (_persistGuard)
//            {
//                while (_repo.TryGetValue(myID, out value))
//                {
//                    // Persist the value chain to disk ;)
//                    Stream s = null;
//                    try
//                    {
//                        s = File.Open(Path.Combine(_dataListPersistPath, myID.ToString()), FileMode.OpenOrCreate);

//                        bf.Serialize(s, value);
//                        result = true;
//                        _persistedDataList.Add(myID);
//                    }
//                    catch (Exception)
//                    {
//                        // Just swallow it, we need to do best effort ;)
//                        result = false;
//                    }
//                    finally
//                    {
//                        if (s != null)
//                        {
//                            s.Close();
//                        }
//                    }
//                    //  move to next in the chain....
//                    myID = value.ParentUID;
//                }
//            }

//            return result;
//        }

//        public void DeleteDataList(Guid id, bool onlyIfNotPersisted)
//        {

//            bool okDelete = false;
//            // check for persistence
//            if (_persistedDataList.Contains(id) && !onlyIfNotPersisted)
//            {
//                okDelete = true;
//            }
//            else if (!_persistedDataList.Contains(id) && onlyIfNotPersisted)
//            {
//                okDelete = true;
//            }

//            if (okDelete)
//            {
//                try
//                {
//                    lock (_activeDLGuard)
//                    {
//                        if (_activeDataList.Contains(id))
//                        {
//                            _activeDataList.Remove(id);
//                            Directory.Delete(Path.Combine(_dataListPersistPath, id.ToString()));

//                            // TODO : Refactor out ;)
//                            if (_persistedDataList.Remove(id))
//                            {
//                                // delete from file system too ;)
//                                File.Delete(Path.Combine(_dataListPersistPath, id.ToString()));
//                            }
//                        }
//                    }
//                }
//                catch (Exception) { /* Fail safe */ }
//            }
//        }

//        // TODO : Refactor to use single file system instance ;)
//        public void InitPersistence()
//        {
//            lock (_persistGuard)
//            {
//                if (!fileSystemInit)
//                {
//                    if (!Directory.Exists(_dataListPersistPath))
//                    {
//                        Directory.CreateDirectory(_dataListPersistPath);
//                    }

//                    string[] toLoad = Directory.GetFiles(_dataListPersistPath);
//                    BinaryFormatter bf = new BinaryFormatter();

//                    IBinaryDataList tmp = null;

//                    foreach (string l in toLoad)
//                    {
//                        Stream s = null;
//                        try
//                        {
//                            s = File.Open(l, FileMode.Open);
//                            if (s.Length > 0)
//                            {
//                                tmp = (IBinaryDataList) bf.Deserialize(s);
//                                if (tmp.UID != GlobalConstants.NullDataListID)
//                                {
//                                    _repo[tmp.UID] = tmp;
//                                    _persistedDataList.Add(tmp.UID);
//                                }

//                            }
//                        }
//                        catch
//                        {
//                            // Just move on... Bad data format ;)
//                            File.Delete(toLoad);
//                        }
//                        finally
//                        {
//                            if (s != null)
//                            {
//                                s.Close();
//                            }
//                        }

//                    }
//                    fileSystemInit = true; // singal an init
//                }
//            }
//        }
//    }
//}
