using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Data.Util;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.DataList.Contract.Value_Objects
{
    /// <summary>
    /// Used to cache indexing for evaluate and upsert ;)
    /// </summary>
    public class Dev2RecordsetIndexScope
    {
        private readonly IDictionary<string, int> _recordsetAppendIndexs = new Dictionary<string, int>();
        private readonly IDictionary<string, int> _recordsetOverwriteIndexs = new Dictionary<string, int>();


        private readonly IList<IIntellisenseResult> nonFramedTokens = new List<IIntellisenseResult>();

        /// <summary>
        /// Fetches the index of the recordset.
        /// </summary>
        /// <param name="part">The part.</param>
        /// <param name="entry">The entry.</param>
        /// <param name="isFramed"></param>
        /// <returns></returns>
        public int FetchRecordsetIndex(IIntellisenseResult part, IBinaryDataListEntry entry, bool isFramed)
        {
            // figure out the index type ;)
            enRecordsetIndexType idxType = DataListUtil.GetRecordsetIndexTypeRaw(part.Option.RecordsetIndex);

            int result = 1;

            if(idxType == enRecordsetIndexType.Numeric)
            {
                // fix index insert ;)
                if(!Int32.TryParse(part.Option.RecordsetIndex, out result))
                {
                    result = -1;
                }
            }
            else if(idxType == enRecordsetIndexType.Blank)
            {
                // append mode
                if(!_recordsetAppendIndexs.TryGetValue(part.Option.Recordset, out result))
                {
                    if(entry != null)
                    {
                        result = entry.FetchAppendRecordsetIndex();
                        _recordsetAppendIndexs[part.Option.Recordset] = result; // stash in cache
                    }
                }

                // Assign behavior ;)
                if(!isFramed)
                {
                    // duplicate match ;)
                    if(nonFramedTokens.Contains(part))
                    {
                        // reset the framing ;)
                        nonFramedTokens.Clear();
                        nonFramedTokens.Add(part);

                        // inc this index ;)
                        _recordsetAppendIndexs[part.Option.Recordset]++;
                        result++;
                    }
                    else
                    {
                        // current append indexes are alright ;)
                        nonFramedTokens.Add(part);
                    }
                }
            }
            else if(idxType == enRecordsetIndexType.Star)
            {
                // overwrite mode from idx 1
                if(!_recordsetOverwriteIndexs.TryGetValue(part.Option.Recordset, out result))
                {
                    result = 1;
                    _recordsetOverwriteIndexs[part.Option.Recordset] = result;
                }
            }

            return result;
        }

        /// <summary>
        /// Moves the indexes to next position.
        /// </summary>
        public void MoveIndexesToNextPosition()
        {
            // Loop all indexes and inc for next frame

            IList<string> keys = _recordsetOverwriteIndexs.Keys.ToList();
            foreach(string k in keys)
            {
                _recordsetOverwriteIndexs[k]++;
            }

            keys = _recordsetAppendIndexs.Keys.ToList();
            foreach(string k in keys)
            {
                _recordsetAppendIndexs[k]++;
            }

        }
    }
}
