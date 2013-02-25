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
        Guid liveFlushingLocation;
        IBinaryDataList bdl;
        string lastRs = string.Empty;
        IList<IBinaryDataListItem> rowData;
        IBinaryDataListEntry entry = null;
        int upsertIdx = 1;
        Dev2RecordsetIndexScope dris = new Dev2RecordsetIndexScope();

        IDataListCompiler c = DataListFactory.CreateDataListCompiler();

        public LiveFlushIterator(Guid loc)
        {
            ErrorResultTO errors;
            liveFlushingLocation = loc;

            bdl = c.FetchBinaryDataList(loc, out errors);

            if (errors.HasErrors())
            {
                throw new Exception(errors.MakeDataListReady());
            }

        }

        public void PublishLiveIterationData()
        {
            if(bdl != null && liveFlushingLocation != GlobalConstants.NullDataListID)
            {
                ErrorResultTO errors;
                // push the exising version of the DataList on change ;)
                c.PushBinaryDataListInServerScope(liveFlushingLocation, bdl, out errors);
                if(errors.HasErrors())
                {
                    throw new Exception(errors.MakeDataListReady());
                }
            }
        }

        // 2 seconds for 10k entries with 75 columns ;)
        public void FlushIterations(PayloadIterationFrame<string> _scopedFrame, bool isFramed, bool isTerminalFlush)
        {
                ErrorResultTO errors;
                
                string error;
                
                Dev2TokenConverter tc = new Dev2TokenConverter();
                enRecordsetIndexType idxType = enRecordsetIndexType.Error;

                // We do not care about data language in these cases, skip all the junk and get it done son ;)
                
                while (_scopedFrame.HasData())
                {
                    DataListPayloadFrameTO<string> tmp = _scopedFrame.FetchNextFrameItem();
                    string exp = tmp.Expression.Replace("(*)", "()"); // force conversion ;)
                    string val = (tmp.Value as string);

                    IIntellisenseResult token = tc.ParseTokenForMatch(exp, bdl.FetchIntellisenseParts());

                    if (token != null)
                    {
                        // Get rs and field
                        string rs = token.Option.Recordset;
                        string field = token.Option.Field;
                        string idx = token.Option.RecordsetIndex;

                        if (rs != lastRs)
                        {
                            // Flush any existing row data for a different recordset ;)
                            if (rowData != null)
                            {
                                // flush the rowData out ;)
                                entry.TryPutRecordRowAt(rowData, upsertIdx, out error);
                                if (error != string.Empty)
                                {
                                    throw new Exception(error);
                                }

                                // push the exising version of the DataList on change ;)
                                c.PushBinaryDataListInServerScope(liveFlushingLocation, bdl, out errors);
                            }
     
                            bdl.TryGetEntry(rs, out entry, out error);
                            if (error != string.Empty || entry == null)
                            {
                                throw new Exception("Upsert Execption : " + error);
                            }

                            // stash last rs
                            lastRs = rs;
                            
                            // build new row data
                            int cnt = entry.Columns.Count;
                            rowData = new List<IBinaryDataListItem>(cnt);
                            InitRowBuffer(cnt);
                        }

                        
 
                        if (!token.Option.IsScalar)
                        {

                            int colIdx = entry.InternalFetchColumnIndex(field);

                            IBinaryDataListItem itm = rowData[colIdx];

                            idxType = DataListUtil.GetRecordsetIndexType(idx);
                            //idxType = DataListUtil.GetRecordsetIndexType(idx);
                            upsertIdx = dris.FetchRecordsetIndex(token, entry, isFramed);

                            // if numeric fetch the index
                            if (idxType == enRecordsetIndexType.Numeric)
                            {
                                Int32.TryParse(idx, out upsertIdx);
                            }

                            itm.UpdateRecordset(rs);
                            itm.UpdateIndex(upsertIdx);
                            itm.UpdateField(field);
                            itm.UpdateValue(val);

                            if(rowData == null)
                            {
                                throw new Exception("Invalid Bulk Load Data");
                            }

                            rowData[colIdx] = itm;
                        }
                        else
                        {
                            IBinaryDataListItem itm = DataListConstants.baseItem.Clone();
                            
                            // else scalar
                            itm.UpdateField(field);
                            itm.UpdateValue(val);

                            entry.TryPutScalar(itm, out error);

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
                if(entry.IsRecordset)
                {
                    error = string.Empty;
                    entry.TryPutRecordRowAt(rowData, upsertIdx, out error);
                    if(error != string.Empty)
                    {
                        throw new Exception(error);
                    }

                    dris.MoveIndexesToNextPosition();
                }

                if(isTerminalFlush)
                {
                    c.PushBinaryDataListInServerScope(liveFlushingLocation, bdl, out errors);
                }
                
                // clear out the buffer
                ClearRowBuffer();
            }

        private void InitRowBuffer(int cnt)
        {
            if(rowData != null)
            {
                for(int i = 0; i < cnt; i++)
                {
                    rowData.Add(DataListConstants.baseItem.Clone());
                }
            }
        }

        private void ClearRowBuffer()
        {
            if (rowData != null)
            {
                foreach(IBinaryDataListItem t in rowData)
                {
                    t.Clear();
                }
            }
        }
    }


}
