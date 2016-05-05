using System;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Parsers;
using Dev2.Data.Util;
using Dev2.Studio.Core.Interfaces.DataList;

namespace Dev2.Studio.Core.Models.DataList
{
    public class ScalarItemModel : DataListItemModel, IScalarItemModel
    {
        public ScalarItemModel(string displayname, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection = enDev2ColumnArgumentDirection.None, string description = "", bool hasError = false, string errorMessage = "", bool isEditable = true, bool isVisible = true, bool isSelected = false, bool isExpanded = true)
            : base(displayname, dev2ColumnArgumentDirection, description, hasError, errorMessage, isEditable, isVisible, isSelected, isExpanded)
        {
        }

        #region Old Code to be removed

        //public enDev2ColumnArgumentDirection ColumnIODirection
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public string Description
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public string DisplayName
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public string ErrorMessage
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public bool HasError
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public bool Input
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public bool IsBlank
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public bool IsCheckBoxVisible
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public bool IsEditable
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public bool IsExpanded
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public bool IsHeader
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public bool IsSelected
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public bool IsUsed
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public bool IsVisible
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public bool Output
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public void RemoveError()
        //{
        //    throw new NotImplementedException();
        //}

        //public void SetError(string errorMessage)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion


        public override string ValidateName(string name)
        {
            Dev2DataLanguageParser parser = new Dev2DataLanguageParser();

            //  var newName = DataListUtil.ExtractFieldNameFromValue(name);

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

            return name;
        }
    }
}