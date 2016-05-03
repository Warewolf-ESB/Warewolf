using Dev2.Studio.Core.Interfaces.DataList;

namespace Dev2.Studio.Core.Models.DataList
{
    public class RecordSetFieldItemModel : IRecordSetFieldItemModel
    {
        private IRecordSetItemModel _parent;

        public IRecordSetItemModel Parent
        {
            get
            {
                return _parent;
            }

            set
            {
                _parent = value;
            }
        }
    }
}