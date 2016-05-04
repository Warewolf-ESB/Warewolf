using Dev2.Data.Binary_Objects;
using Dev2.Studio.Core.Interfaces.DataList;

namespace Dev2.Studio.Core.Models.DataList
{
    public class RecordSetFieldItemModel : DataListItemModel, IRecordSetFieldItemModel
    {
        private IRecordSetItemModel _parent;

        public RecordSetFieldItemModel(string displayname, IRecordSetItemModel parent, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection = enDev2ColumnArgumentDirection.None, string description = "", bool hasError = false, string errorMessage = "", bool isEditable = true, bool isVisible = true, bool isSelected = false, bool isExpanded = true) 
            : base(displayname, dev2ColumnArgumentDirection, description, hasError, errorMessage, isEditable, isVisible, isSelected, isExpanded)
        {
            Parent = parent;
        }

        public RecordSetFieldItemModel(string displayname)
            : this(displayname, null)
        {
        }

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