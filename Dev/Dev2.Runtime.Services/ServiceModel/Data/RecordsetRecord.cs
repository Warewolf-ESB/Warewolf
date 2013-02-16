using System;
using System.Collections.Generic;

namespace Dev2.Runtime.ServiceModel.Data
{
    // DO NOT inherit from List<RecordsetCell> otherwise the Label property disappears from the JSON serialization
    // This behaviour appears to be caused by the IEnumerable<T> interface of IList<T>
    public class RecordsetRecord
    {
        readonly List<RecordsetCell> _cells;

        #region CTOR

        public RecordsetRecord()
        {
            _cells = new List<RecordsetCell>();
        }

        public RecordsetRecord(IEnumerable<RecordsetCell> cells)
        {
            _cells = new List<RecordsetCell>(cells);
        }

        #endregion

        #region Properties

        public string Label { get; set; }

        public RecordsetCell this[int index]
        {
            get
            {
                return _cells[index];
            }
            set
            {
                _cells[index] = value;
            }
        }

        public int Count
        {
            get
            {
                return _cells.Count;
            }
        }

        public RecordsetCell[] Cells
        {
            get
            {
                return _cells.ToArray();
            }
        }

        #endregion

        #region Methods

        public void AddRange(IEnumerable<RecordsetCell> items)
        {
            _cells.AddRange(items);
        }

        public void Add(RecordsetCell item)
        {
            if(item == null)
            {
                throw new ArgumentNullException("item");
            }
            _cells.Add(item);
        }

        public void Clear()
        {
            _cells.Clear();
        }

        #endregion

    }
}
