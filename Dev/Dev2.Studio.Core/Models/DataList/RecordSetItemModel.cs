using System;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common.Common;
using Dev2.Data;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.Parsers;
using Dev2.Data.Util;
using Dev2.Studio.Interfaces.DataList;

namespace Dev2.Studio.Core.Models.DataList
{
    public class RecordSetItemModel : DataListItemModel, IRecordSetItemModel
    {
        private ObservableCollection<IRecordSetFieldItemModel> _children;
        private string _searchText;

        public RecordSetItemModel(string displayname, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection = enDev2ColumnArgumentDirection.None, string description = "", IDataListItemModel parent = null, OptomizedObservableCollection<IRecordSetFieldItemModel> children = null, bool hasError = false, string errorMessage = "", bool isEditable = true, bool isVisible = true, bool isSelected = false, bool isExpanded = true) 
            : base(displayname, dev2ColumnArgumentDirection, description, hasError, errorMessage, isEditable, isVisible, isSelected, isExpanded)
        {
            Children = children;            
        }

        public ObservableCollection<IRecordSetFieldItemModel> Children
        {
            get
            {
                if (!string.IsNullOrEmpty(_searchText))
                {
                    if (_children != null)
                    {
                        var itemModels = _children.Where(model => model.IsVisible).ToObservableCollection();
                        return itemModels;
                    }
                }
                return _children ?? (_children = new ObservableCollection<IRecordSetFieldItemModel>());
            }
            set
            {
                _children = value;
                NotifyOfPropertyChange(() => Children);
            }
        }

        public void Filter(string searchText)
        {
            if(!string.IsNullOrEmpty(searchText))
            {
                if (_children != null)
                {
                    foreach (var recordSetFieldItemModel in _children)
                    {
                        recordSetFieldItemModel.Filter(searchText);
                    }
                }
                IsVisible = _children != null && _children.Any(model => model.IsVisible) ? true : !string.IsNullOrEmpty(DisplayName) && DisplayName.ToLower().Contains(searchText.ToLower());
            }
            else
            {
                IsVisible = true;
                if (_children != null)
                {
                    foreach(var recordSetFieldItemModel in _children)
                    {
                        recordSetFieldItemModel.IsVisible = true;
                    }
                }
            }
            _searchText = searchText;
            NotifyOfPropertyChange(()=>Children);
        }

        public override bool Input
        {
            get
            {
                return _columnIODir == enDev2ColumnArgumentDirection.Both
                       || _columnIODir == enDev2ColumnArgumentDirection.Input;
            }
            set
            {
                SetColumnIODirectionFromInput(value);
                if(Children.Count > 0)
                {
                    SetChildInputValues(value);
                }
            }
        }

        public override bool Output
        {
            get
            {
                return _columnIODir == enDev2ColumnArgumentDirection.Both || _columnIODir == enDev2ColumnArgumentDirection.Output;
            }
            set
            {
                SetColumnIODirectionFromOutput(value);
                if (Children.Count > 0)
                {
                    SetChildOutputValues(value);
                }
            }
        }

        private void SetChildInputValues(bool value)
        {
            if(Children != null)
            {
                foreach(var dataListItemModel in Children)
                {
                    var child = dataListItemModel;
                    if (!string.IsNullOrEmpty(child.DisplayName))
                    {
                        child.Input = value;
                    }
                }
            }
        }

        private void SetChildOutputValues(bool value)
        {
            if (Children != null)
            {
                foreach (var dataListItemModel in Children)
                {
                    var child = dataListItemModel;
                    if (!string.IsNullOrEmpty(child.DisplayName))
                    {
                        child.Output = value;
                    }
                }
            }
        }

        #region Overrides of DataListItemModel

        public override string ValidateName(string name)
        {
            Dev2DataLanguageParser parser = new Dev2DataLanguageParser();
            if (!string.IsNullOrEmpty(name))
            {
                name = DataListUtil.RemoveRecordsetBracketsFromValue(name);

                if(!string.IsNullOrEmpty(name))
                {
                    var intellisenseResult = parser.ValidateName(name, "Recordset");
                    if(intellisenseResult != null)
                    {
                        SetError(intellisenseResult.Message);
                    }
                    else
                    {
                        if(!string.Equals(ErrorMessage, StringResources.ErrorMessageDuplicateValue, StringComparison.InvariantCulture) &&
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

        #endregion

        #region Overrides of DataListItemModel

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

        #endregion
    }
}