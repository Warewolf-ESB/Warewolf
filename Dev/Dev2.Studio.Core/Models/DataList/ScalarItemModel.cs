#pragma warning disable
﻿using System;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.Parsers;
using Dev2.Studio.Interfaces.DataList;

namespace Dev2.Studio.Core.Models.DataList
{
    public class ScalarItemModel : DataListItemModel, IScalarItemModel
    {
        public ScalarItemModel(string displayname)
            : this(displayname, enDev2ColumnArgumentDirection.None, "", false, "", true, true, false, true)
        {
        }

        public ScalarItemModel(string displayname, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection)
            : this(displayname, dev2ColumnArgumentDirection, "", false, "", true, true, false, true)
        {
        }

        public ScalarItemModel(string displayname, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection, string description, bool hasError, string errorMessage, bool isEditable, bool isVisible, bool isSelected)
            : this(displayname, dev2ColumnArgumentDirection, description, hasError, errorMessage, isEditable, isVisible, isSelected, true)
        {
        }

        public ScalarItemModel(string displayname, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection, string description, bool hasError, string errorMessage, bool isEditable, bool isVisible, bool isSelected, bool isExpanded) 
            : base(displayname, dev2ColumnArgumentDirection, description, hasError, errorMessage, isEditable, isVisible, isSelected, isExpanded)
        {
        }

        #region Overrides of DataListItemModel

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString() => DisplayName;

        public override string ValidateName(string name)
        {
            var parser = new Dev2DataLanguageParser();
            if (!string.IsNullOrEmpty(name))
            {
                var intellisenseResult = parser.ValidateName(name, "Scalar");
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
            return name;
        }

        public void Filter(string searchText)
        {
            IsVisible = !string.IsNullOrEmpty(searchText) ? !string.IsNullOrEmpty(DisplayName) && DisplayName.ToLower().Contains(searchText.ToLower()) : true;
        }

        #endregion
    }
}