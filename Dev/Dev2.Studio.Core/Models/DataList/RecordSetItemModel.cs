using Dev2.Data.Binary_Objects;
using Dev2.Studio.Core.Interfaces.DataList;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Data.Parsers;
using Dev2.Data.Util;

namespace Dev2.Studio.Core.Models.DataList
{
    public class RecordSetItemModel : DataListItemModel, IRecordSetItemModel
    {
        private ObservableCollection<IRecordSetFieldItemModel> _backupChildren;

        private string _filterText;
        private ObservableCollection<IRecordSetFieldItemModel> _children;

        public RecordSetItemModel(string displayname, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection = enDev2ColumnArgumentDirection.None, string description = "", IDataListItemModel parent = null, OptomizedObservableCollection<IRecordSetFieldItemModel> children = null, bool hasError = false, string errorMessage = "", bool isEditable = true, bool isVisible = true, bool isSelected = false, bool isExpanded = true) 
            : base(displayname, dev2ColumnArgumentDirection, description, hasError, errorMessage, isEditable, isVisible, isSelected, isExpanded)
        {
            Children = children;
        }

        //private enDev2ColumnArgumentDirection _columnIODir = enDev2ColumnArgumentDirection.None;

        public ObservableCollection<IRecordSetFieldItemModel> Children
        {
            get
            {
                return _children ?? (_children = new ObservableCollection<IRecordSetFieldItemModel>());
            }
            set
            {
                _children = value;
                NotifyOfPropertyChange(() => Children);
            }
        }

        #region Old Code

        //public enDev2ColumnArgumentDirection ColumnIODirection
        //{
        //    get
        //    {
        //        return _columnIODir;
        //    }
        //    set
        //    {
        //        _columnIODir = value;

        //        NotifyIOPropertyChanged();
        //    }
        //}

        //public string Description
        //{
        //    get
        //    {
        //        return _description;
        //    }
        //    set
        //    {
        //        _description = value;
        //        NotifyOfPropertyChange(() => Description);
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

        #endregion


        public string FilterText
        {
            get
            {
                string child = "";
                if (Children != null)
                {
                    child = String.Join("", Children.Select(a => a.DisplayName));
                }
                return DisplayName + child;
            }
            set
            {
                _filterText = value;
            }
        }

        public void Filter(string searchText)
        {
            Children.Clear();
            if (_backupChildren != null)
            {
                foreach (IRecordSetFieldItemModel recordSetFieldItemModel in _backupChildren)
                {
                    Children.Add(recordSetFieldItemModel);
                }
            }

            if (string.IsNullOrEmpty(searchText))
            {
                return;
            }

            if (!String.IsNullOrEmpty(searchText))
            {
                _backupChildren = _backupChildren ?? new ObservableCollection<IRecordSetFieldItemModel>();
                foreach (var dataListItemModel in Children)
                {
                    _backupChildren.Add(dataListItemModel);
                }
            }

            _backupChildren = Children;
            Children = new ObservableCollection<IRecordSetFieldItemModel>(Children.Where(a => a.DisplayName.ToUpper().Contains(searchText.ToUpper())));
        }

        
        public override string ValidateName(string name)
        {
            Dev2DataLanguageParser parser = new Dev2DataLanguageParser();
            if (!string.IsNullOrEmpty(name))
            {
                name = DataListUtil.RemoveRecordsetBracketsFromValue(name);

                if (!string.IsNullOrEmpty(name))
                {
                    var intellisenseResult = parser.ValidateName(name, "Recordset");
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
    }
}