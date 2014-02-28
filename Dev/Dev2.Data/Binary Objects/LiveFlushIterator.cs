using System;
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Data.Builders;
using Dev2.Data.Util;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;
using Dev2.DataList.Contract.Value_Objects;

namespace Dev2.DataList.Contract.Builders
{
    /// <summary>
    /// Flush data iteratively
    /// </summary>
    public class LiveFlushIterator
    {
        readonly Guid _liveFlushingLocation;
        readonly IBinaryDataList _bdl;
        string _lastRs = string.Empty;
        IList<IBinaryDataListItem> _rowData;
        IBinaryDataListEntry _entry;
        int _upsertIdx = 1;
        readonly Dev2RecordsetIndexScope dris = new Dev2RecordsetIndexScope();

        readonly IDataListCompiler c = DataListFactory.CreateDataListCompiler();

        public LiveFlushIterator(Guid loc)
        {
            ErrorResultTO errors;
            _liveFlushingLocation = loc;

            _bdl = c.FetchBinaryDataList(loc, out errors);

            if(errors.HasErrors())
            {
                throw new Exception(errors.MakeDataListReady());
            }

        }

        public void PublishLiveIterationData()
        {
            if(_bdl != null && _liveFlushingLocation != GlobalConstants.NullDataListID)
            {
                ErrorResultTO errors;
                // push the existing version of the DataList on change ;)
                c.PushBinaryDataListInServerScope(_liveFlushingLocation, _bdl, out errors);
                if(errors.HasErrors())
                {
                    throw new Exception(errors.MakeDataListReady());
                }
            }
        }

        // 2 seconds for 10k entries with 75 columns ;)
        public void FlushIterations(PayloadIterationFrame<string> scopedFrame, bool isFramed, bool isTerminalFlush)
        {
            string error;

            Dev2TokenConverter tc = new Dev2TokenConverter();

            bool amendedData = false;
            // We do not care about data language in these cases, skip all the junk and get it done son ;)

            while(scopedFrame.HasData())
            {
                DataListPayloadFrameTO<string> tmp = scopedFrame.FetchNextFrameItem();
                string exp = tmp.Expression; //.Replace("(*)", "()"); // force conversion ;)
                string val = tmp.Value;

                IIntellisenseResult token = tc.ParseTokenForMatch(exp, _bdl.FetchIntellisenseParts());

                if(token != null)
                {
                    // Get rs and field
                    string rs = token.Option.Recordset;
                    string field = token.Option.Field;
                    string idx = token.Option.RecordsetIndex;

                    if(rs != _lastRs && !string.IsNullOrEmpty(rs))
                    {
                        // Flush any existing row data for a different recordset ;)
                        if(_rowData != null)
                        {
                            if(!token.Option.IsScalar)
                            {
                                DumpColAtATime();
                            }

                            amendedData = false;
                        }

                        _bdl.TryGetEntry(rs, out _entry, out error);
                        if(error != string.Empty || _entry == null)
                        {
                            throw new Exception("Upsert Exception : " + error);
                        }

                        // stash last rs
                        _lastRs = rs;

                        // build new row data
                        int cnt = _entry.Columns.Count;
                        _rowData = new List<IBinaryDataListItem>(cnt);
                        InitRowBuffer(cnt);
                    }


                    if(!token.Option.IsScalar)
                    {
                        // set commit flag ;)
                        amendedData = true;

                        int colIdx = _entry.InternalFetchColumnIndex(field);

                        IBinaryDataListItem itm = _rowData[colIdx];

                        enRecordsetIndexType idxType = DataListUtil.GetRecordsetIndexTypeRaw(idx);

                        int tmpIdx = dris.FetchRecordsetIndex(token, _entry, isFramed);

                        if(tmpIdx != _upsertIdx)
                        {
                            // silly users making algorithms slow ;(
                            // we need to dump data at this point... 1 fliping column at a time
                            DumpColAtATime();

                        }

                        _upsertIdx = tmpIdx;

                        if(_upsertIdx == 0)
                        {
                            throw new Exception("Invalid recordset index of 0");
                        }

                        // if numeric fetch the index
                        if(idxType == enRecordsetIndexType.Numeric)
                        {
                            Int32.TryParse(idx, out _upsertIdx);
                        }


                        itm.UpdateIndex(_upsertIdx);
                        itm.UpdateField(field);
                        itm.UpdateValue(val);

                        if(_rowData == null)
                        {
                            throw new Exception("Invalid Bulk Load Data");
                        }

                        _rowData[colIdx] = itm;
                    }
                    else
                    {
                        IBinaryDataListItem itm = Dev2BinaryDataListFactory.CreateBaseItem();

                        // else scalar and we need to get the entry ;(
                        IBinaryDataListEntry scalarEntry;
                        _bdl.TryGetEntry(field, out scalarEntry, out error);
                        itm.UpdateField(field);
                        itm.UpdateValue(val);

                        scalarEntry.TryPutScalar(itm, out error);

                        if(error != string.Empty)
                        {
                            throw new Exception(error);
                        }
                    }
                }
                else
                {
                    throw new Exception("Null token for [ " + exp + " ]");
                }
            }

            // flush the rowData out ;)
            if(_entry != null && _entry.IsRecordset && amendedData)
            {
                _entry.TryPutRecordRowAt(_rowData, _upsertIdx, out error);
                if(error != string.Empty)
                {
                    throw new Exception(error);
                }

                dris.MoveIndexesToNextPosition();
            }

            if(isTerminalFlush)
            {
                ErrorResultTO errors;
                c.PushBinaryDataListInServerScope(_liveFlushingLocation, _bdl, out errors);
            }

            // clear out the buffer
            ClearRowBuffer();
        }

        private void DumpColAtATime()
        {
            // silly users making algorithms slow ;(
            // we need to dump data at this point... 1 flipping column at a time
            foreach(IBinaryDataListItem item in _rowData)
            {
                if(item.ItemCollectionIndex != -1)
                {
                    string error;
                    _entry.TryPutRecordItemAtIndex(item, item.ItemCollectionIndex, out error);
                }
            }

            // then we need to re-init the collection
            int cnt = _rowData.Count;
            _rowData = new List<IBinaryDataListItem>(cnt);
            InitRowBuffer(cnt);
        }

        private void InitRowBuffer(int cnt)
        {
            if(_rowData != null)
            {
                for(int i = 0; i < cnt; i++)
                {
                    IBinaryDataListItem itm = Dev2BinaryDataListFactory.CreateBaseItem();
                    itm.UpdateRecordset(_lastRs);
                    _rowData.Add(itm);
                }
            }
        }

        private void ClearRowBuffer()
        {
            if(_rowData != null)
            {
                foreach(IBinaryDataListItem t in _rowData)
                {
                    t.ToClear();
                }
            }
        }
    }


}
