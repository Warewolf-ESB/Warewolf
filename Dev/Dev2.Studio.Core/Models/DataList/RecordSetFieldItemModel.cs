using Dev2.Data.Binary_Objects;
using Dev2.Data.Parsers;
using Dev2.Data.Util;
using Dev2.Studio.Core.Interfaces.DataList;
using System;

namespace Dev2.Studio.Core.Models.DataList
{
    public class RecordSetFieldItemModel : DataListItemModel, IRecordSetFieldItemModel
    {
        private IRecordSetItemModel _parent;
        private string _displayName;

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

        public new string DisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                _displayName = ValidateName(value);
                //Name = value;
                NotifyOfPropertyChange(() => DisplayName);
            }
        }

        public override string ValidateName(string name)
        {
            Dev2DataLanguageParser parser = new Dev2DataLanguageParser();
            if (!string.IsNullOrEmpty(name))
            {
                name = DataListUtil.ExtractFieldNameFromValue(name);
                if (!string.IsNullOrEmpty(name))
                {
                    var intellisenseResult = parser.ValidateName(name, "Variable");
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
            return name;
        }

        #region Overrides of Object

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return DisplayName;
        }

        #endregion Overrides of Object
    }
}