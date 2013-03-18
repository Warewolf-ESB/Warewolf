using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Dev2.DataList.Contract;

namespace Dev2.Server.DataList.Persistence
{
    internal sealed class DataListPersistenceProvider //: IDataListPersistenceProvider
    {
        //private const int DefaultMemoryBucketSize = 512;
        //private const int DefaultMemoryAllocationSizeInMb = 4;

        //private ReaderWriterLockSlim _mappingGuard;
        //private Dictionary<Guid, DatalistMapping> _mapping;

        //private DatalistMemoryProvider _memoryProvider;

        //internal DataListPersistenceProvider()
        //    : this(DefaultMemoryBucketSize, DefaultMemoryAllocationSizeInMb)
        //{
        //}

        //internal DataListPersistenceProvider(int memoryBucketSize, int memoryAllocationSizeInMb)
        //{
        //    _memoryProvider = new DatalistMemoryProvider(memoryBucketSize, memoryAllocationSizeInMb);
        //    _mappingGuard = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        //    _mapping = new Dictionary<Guid, DatalistMapping>();
        //}

        //public bool WriteNewDatalist(Guid datalistID, byte[] datalist, ErrorResultTO errors)
        //{
        //    DatalistMapping map;

        //    _mappingGuard.EnterWriteLock();

        //    try
        //    {
        //        _mapping.Add(datalistID, map = new DatalistMapping(datalistID));
        //        map.Data = datalist;
        //    }
        //    finally
        //    {
        //        _mappingGuard.ExitWriteLock();
        //    }

        //    return true;
        //}

        //public bool WriteOverDatalist(Guid datalistID, byte[] datalist, ErrorResultTO errors)
        //{
        //    DatalistMapping map;
        //    bool found;

        //    _mappingGuard.EnterReadLock();

        //    try
        //    {
        //        found = _mapping.TryGetValue(datalistID, out map);
        //    }
        //    finally
        //    {
        //        _mappingGuard.ExitReadLock();
        //    }

        //    if (!found)
        //    {
        //        errors.AddError("DatalistPersistenceProvider: WriteOverDatalist operation failed, no datalist exists with ID \"" + datalistID.ToString() + "\"");
        //    }
        //    else
        //    {
        //        Interlocked.Exchange<byte[]>(ref map.Data, datalist);
        //    }

        //    return found;
        //}

        //public byte[] ReadDatalist(Guid datalistID, ErrorResultTO errors)
        //{
        //    DatalistMapping map;
        //    bool found;

        //    _mappingGuard.EnterReadLock();

        //    try
        //    {
        //        found = _mapping.TryGetValue(datalistID, out map);
        //    }
        //    finally
        //    {
        //        _mappingGuard.ExitReadLock();
        //    }

        //    byte[] result;

        //    if (!found)
        //    {
        //        result = null;
        //        errors.AddError("DatalistPersistenceProvider: ReadDatalist operation failed, no datalist exists with ID \"" + datalistID.ToString() + "\"");
        //    }
        //    else
        //    {
        //        byte[] data = Interlocked.CompareExchange<byte[]>(ref map.Data, null, null);
        //        int length = data.Length;
        //        result = new byte[length];

        //        if (length < 8)
        //        {
        //            for (int i = 0; i < result.Length; i++)
        //                result[i] = data[i];
        //        }
        //        else
        //        {
        //            Buffer.BlockCopy(data, 0, result, 0, length);
        //        }
        //    }

        //    return result;
        //}



        //private sealed class DatalistMapping
        //{
        //    private Guid _id;

        //    public byte[] Data;

        //    public Guid ID { get { return _id; } }

        //    public DatalistMapping(Guid id)
        //    {
        //        _id = id;
        //    }
        //}
    }
}
