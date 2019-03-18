#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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