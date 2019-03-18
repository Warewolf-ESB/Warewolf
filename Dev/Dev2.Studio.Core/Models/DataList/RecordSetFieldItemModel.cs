#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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