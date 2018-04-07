﻿using System;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.Parsers;
using Dev2.Data.Util;
using Dev2.Studio.Interfaces.DataList;

namespace Dev2.Studio.Core.Models.DataList
{
    public class RecordSetFieldItemModel : DataListItemModel, IRecordSetFieldItemModel
    {
        IRecordSetItemModel _parent;

        public RecordSetFieldItemModel(string displayname)
            : this(displayname, null)
        {
        }

        public RecordSetFieldItemModel(string displayname, IRecordSetItemModel parent)
            : this(displayname, parent, enDev2ColumnArgumentDirection.None, "", false, "", true, true, false, true)
        {
        }

        public RecordSetFieldItemModel(string displayname, IRecordSetItemModel parent, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection)
            : this(displayname, parent, dev2ColumnArgumentDirection, "", false, "", true, true, false, true)
        {
        }

        public RecordSetFieldItemModel(string displayname, IRecordSetItemModel parent, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection, string description, bool hasError, string errorMessage, bool isEditable, bool isVisible, bool isSelected)
            : this(displayname, parent, dev2ColumnArgumentDirection, description, hasError, errorMessage, isEditable, isVisible, isSelected, true)
        {
        }

        public RecordSetFieldItemModel(string displayname, IRecordSetItemModel parent, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection, string description, bool hasError, string errorMessage, bool isEditable, bool isVisible, bool isSelected, bool isExpanded) 
            : base(displayname, dev2ColumnArgumentDirection, description, hasError, errorMessage, isEditable, isVisible, isSelected, isExpanded)
        {
            Parent = parent;
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
            IsVisible = !string.IsNullOrEmpty(searchText) ? !string.IsNullOrEmpty(DisplayName) && DisplayName.ToLower().Contains(searchText.ToLower()) : true;
        }

        public override string ValidateName(string name)
        {
            var parser = new Dev2DataLanguageParser();
            if (!string.IsNullOrEmpty(name))
            {
                var fieldName = DataListUtil.ExtractFieldNameFromValue(name);
                var recName = DataListUtil.ExtractRecordsetNameFromValue(name);
                if (!string.IsNullOrEmpty(recName))
                {
                    ValidateName(parser, recName);
                }
                if (!string.IsNullOrEmpty(fieldName))
                {
                    name = ValidateName(name, parser, fieldName);
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

        private string ValidateName(string name, Dev2DataLanguageParser parser, string fieldName)
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

            return name;
        }

        void ValidateName(Dev2DataLanguageParser parser, string recName)
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
    }
}