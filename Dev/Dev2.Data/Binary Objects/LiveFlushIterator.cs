using Dev2.Common;
using Dev2.Data.SystemTemplates;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;
using Dev2.DataList.Contract.Value_Objects;
using System;
using System.Collections.Generic;

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
        Dev2RecordsetIndexScope dris = new Dev2RecordsetIndexScope();

        IDataListCompiler c = DataListFactory.CreateDataListCompiler();

        public LiveFlushIterator(Guid loc)
        {
            ErrorResultTO errors;
            _liveFlushingLocation = loc;

            _bdl = c.FetchBinaryDataList(loc, out errors);

            if (errors.HasErrors())
            {
                throw new Exception(errors.MakeDataListReady());
            }

        }

        public void PublishLiveIterationData()
        {
            if(_bdl != null && _liveFlushingLocation != GlobalConstants.NullDataListID)
            {
                ErrorResultTO errors;
                // push the exising version of the DataList on change ;)
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
                ErrorResultTO errors;
                
                string error;
                
                Dev2TokenConverter tc = new Dev2TokenConverter();
                enRecordsetIndexType idxType;

                bool amendedData = false;
                // We do not care about data language in these cases, skip all the junk and get it done son ;)
                
                while (scopedFrame.HasData())
                {
                    DataListPayloadFrameTO<string> tmp = scopedFrame.FetchNextFrameItem();
                    string exp = tmp.Expression.Replace("(*)", "()"); // force conversion ;)
                    string val = (tmp.Value as string);

                    IIntellisenseResult token = tc.ParseTokenForMatch(exp, _bdl.FetchIntellisenseParts());

                    if (token != null)
                    {
                        // Get rs and field
                        string rs = token.Option.Recordset;
                        string field = token.Option.Field;
                        string idx = token.Option.RecordsetIndex;

                        if (rs != _lastRs && !string.IsNullOrEmpty(rs))
                        {
                            // Flush any existing row data for a different recordset ;)
                            //if (_rowData != null)
                            //{
                            //    // flush the rowData out ;)
                            //    _entry.TryPutRecordRowAt(_rowData, _upsertIdx, out error);
                            //    if (error != string.Empty)
                            //    {
                            //        throw new Exception(error);
                            //    }

                            //    // push the exising version of the DataList on change ;)
                            //    c.PushBinaryDataListInServerScope(_liveFlushingLocation, _bdl, out errors);
                            //    amendedData = false;
                            //}
     
                            _bdl.TryGetEntry(rs, out _entry, out error);
                            if (error != string.Empty || _entry == null)
                            {
                                throw new Exception("Upsert Execption : " + error);
                            }

                            // stash last rs
                            _lastRs = rs;
                            
                            // build new row data
                            int cnt = _entry.Columns.Count;
                            _rowData = new List<IBinaryDataListItem>(cnt);
                            InitRowBuffer(cnt);
                        }

                       
                        if (!token.Option.IsScalar)
                        {
                            // set commit flag ;)
                            amendedData = true;

                            int colIdx = _entry.InternalFetchColumnIndex(field);

                            IBinaryDataListItem itm = _rowData[colIdx];

                            idxType = DataListUtil.GetRecordsetIndexType(idx);
                            //idxType = DataListUtil.GetRecordsetIndexType(idx);
                            _upsertIdx = dris.FetchRecordsetIndex(token, _entry, isFramed);

                            if (_upsertIdx == 0)
                            {
                                throw new Exception("Invalid recordset index of 0");
                            }

                            // if numeric fetch the index
                            if (idxType == enRecordsetIndexType.Numeric)
                            {
                                Int32.TryParse(idx, out _upsertIdx);
                            }

                            //itm.UpdateRecordset(rs);
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
                            IBinaryDataListItem itm = DataListConstants.baseItem.Clone();
                            
                            // else scalar and we need to get the entry ;(
                            IBinaryDataListEntry scalarEntry;
                            _bdl.TryGetEntry(field, out scalarEntry, out error);
                            itm.UpdateField(field);
                            itm.UpdateValue(val);

                            scalarEntry.TryPutScalar(itm, out error);

                            if (error != string.Empty)
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
                    error = string.Empty;
                    _entry.TryPutRecordRowAt(_rowData, _upsertIdx, out error);
                    if(error != string.Empty)
                    {
                        throw new Exception(error);
                    }

                    dris.MoveIndexesToNextPosition();
                }

                if(isTerminalFlush)
                {
                    c.PushBinaryDataListInServerScope(_liveFlushingLocation, _bdl, out errors);
                }
                
                // clear out the buffer
                ClearRowBuffer();
            }

        private void InitRowBuffer(int cnt)
        {
            if(_rowData != null)
            {
                for(int i = 0; i < cnt; i++)
                {
                    IBinaryDataListItem itm = DataListConstants.baseItem.Clone();
                    itm.UpdateRecordset(_lastRs);
                    _rowData.Add(itm);
                }
            }
        }

        private void ClearRowBuffer()
        {
            if (_rowData != null)
            {
                foreach(IBinaryDataListItem t in _rowData)
                {
                    t.ToClear();
                }
            }
        }
    }


}
