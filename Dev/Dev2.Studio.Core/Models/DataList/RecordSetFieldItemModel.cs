using System;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.Parsers;
using Dev2.Data.Util;
using Dev2.Studio.Interfaces.DataList;

namespace Dev2.Studio.Core.Models.DataList
{
    public class RecordSetFieldItemModel : DataListItemModel, IRecordSetFieldItemModel
    {
        private IRecordSetItemModel _parent;

        public RecordSetFieldItemModel(string displayname, IRecordSetItemModel parent)
            : base(displayname, enDev2ColumnArgumentDirection.None, "", false, "", true, true, false, true)
        {
            Parent = parent;
        }

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

        public void Filter(string searchText)
        {
            if (!string.IsNullOrEmpty(searchText))
            {
                IsVisible = !string.IsNullOrEmpty(DisplayName) &&  DisplayName.ToLower().Contains(searchText.ToLower());
            }
            else
            {
                IsVisible = true;
            }
        }

        public override string ValidateName(string name)
        {
            Dev2DataLanguageParser parser = new Dev2DataLanguageParser();
            if (!string.IsNullOrEmpty(name))
            {
                var fieldName = DataListUtil.ExtractFieldNameFromValue(name);
                var recName = DataListUtil.ExtractRecordsetNameFromValue(name);
                if (!string.IsNullOrEmpty(recName))
                {
                    var intellisenseResult = parser.ValidateName(recName, "Recordset");
                    if (intellisenseResult != null)
                    {
                        SetError(intellisenseResult.Message);
                    }
                    else
                    {
                        if (!string.Equals(ErrorMessage, StringResources.ErrorMessageDuplicateValue, StringComparison.InvariantCulture) &&
                            !string.Equals(ErrorMessage, StringResources.ErrorMessageDuplicateVariable, StringComparison.InvariantCulture) &&
                            !string.Equals(ErrorMessage, StringResources.ErrorMessageDuplicateRecordset, StringComparison.InvariantCulture) &&
                            !string.Equals(ErrorMessage, StringResources.ErrorMessageEmptyRecordSet, StringComparison.InvariantCulture))
                        {
                            RemoveError();
                        }
                    }
                }
                if (!string.IsNullOrEmpty(fieldName))
                {
                    var intellisenseResult = parser.ValidateName(fieldName, "Recordset field");
                    if (intellisenseResult != null)
                    {
                        SetError(intellisenseResult.Message);
                    }
                    else
                    {
                        if (!string.Equals(ErrorMessage, StringResources.ErrorMessageDuplicateValue, StringComparison.InvariantCulture) &&
                            !string.Equals(ErrorMessage, StringResources.ErrorMessageDuplicateVariable, StringComparison.InvariantCulture) &&
                            !string.Equals(ErrorMessage, StringResources.ErrorMessageDuplicateRecordset, StringComparison.InvariantCulture) &&
                            !string.Equals(ErrorMessage, StringResources.ErrorMessageEmptyRecordSet, StringComparison.InvariantCulture))
                        {
                            RemoveError();
                            name = fieldName;
                        }
                    }
                }
                else
                {
                    var intellisenseResult = parser.ValidateName(name, "Recordset field");
                    if (intellisenseResult != null)
                    {
                        SetError(intellisenseResult.Message);
                    }                                        
                }
            }
            return name;
        }
    }
}