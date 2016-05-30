/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Caliburn.Micro;
using Dev2.Data.Binary_Objects;
using Dev2.Studio.Core.Interfaces.DataList;
// ReSharper disable InconsistentNaming

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Models.DataList
{
    public class DataListItemModel : PropertyChangedBase, IDataListItemModel
    {
        #region Fields

        private string _description;

        private bool _hasError;

        private string _errorMessage;
        private bool _isEditable;
        private bool _isVisible;
        private bool _isSelected;

        private bool _isUsed;

        private bool _allowNotes;
        private bool _isComplexObject;
        private string _displayName;
        private bool _isExpanded = true;
        protected enDev2ColumnArgumentDirection _columnIODir = enDev2ColumnArgumentDirection.None;
        private string _name;


        #endregion Fields

        #region Ctor


        public DataListItemModel(string displayname, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection = enDev2ColumnArgumentDirection.None, string description = "", bool hasError = false, string errorMessage = "", bool isEditable = true, bool isVisible = true, bool isSelected = false, bool isExpanded = true)
        {
            Description = description;
            HasError = hasError;
            ErrorMessage = errorMessage;
            IsEditable = isEditable;
            IsVisible = isVisible;
            DisplayName = displayname;
            IsSelected = isSelected;
            IsExpanded = isExpanded;
            IsUsed = true;
            ColumnIODirection = dev2ColumnArgumentDirection;
        }

        #endregion Ctor

        #region Properties

        public bool IsUsed
        {
            get
            {
                return _isUsed;
            }
            set
            {
                _isUsed = value;
                NotifyOfPropertyChange(() => IsUsed);
            }
        }

        //public string LastIndexedName
        //{
        //    get
        //    {
        //        return _lastIndexedName;
        //    }
        //    set
        //    {
        //        _lastIndexedName = value;
        //        NotifyOfPropertyChange(() => LastIndexedName);
        //    }
        //}

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                NotifyOfPropertyChange(() => IsSelected);
            }
        }

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                _displayName = ValidateName(value);
                Name = value;
                NotifyOfPropertyChange(() => DisplayName);
            }
        }

        public virtual string ValidateName(string name) { return name;}
        
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                NotifyOfPropertyChange(() => Description);
            }
        }


        public bool HasError
        {
            get
            {
                return _hasError;
            }
            set
            {
                _hasError = value;
                NotifyOfPropertyChange(() => HasError);
            }
        }

        public string ErrorMessage
        {
            get
            {
                if (_errorMessage == string.Empty)
                {
                    return null;
                }
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
                NotifyOfPropertyChange(() => ErrorMessage);
            }
        }

        public bool IsEditable
        {
            get
            {
                return _isEditable;
            }
            set
            {
                _isEditable = value;
                NotifyOfPropertyChange(() => IsEditable);
            }
        }

        public enDev2ColumnArgumentDirection ColumnIODirection
        {
            get
            {
                return _columnIODir;
            }
            set
            {
                _columnIODir = value;

                NotifyIOPropertyChanged();
            }
        }

        public virtual bool Input
        {
            get
            {
                return _columnIODir == enDev2ColumnArgumentDirection.Both
                       || _columnIODir == enDev2ColumnArgumentDirection.Input;
            }
            set
            {
                SetColumnIODirectionFromInput(value);
            }
        }

        public virtual bool Output
        {
            get
            {
                return _columnIODir == enDev2ColumnArgumentDirection.Both || _columnIODir == enDev2ColumnArgumentDirection.Output;
            }
            set
            {
                SetColumnIODirectionFromOutput(value);
            }
        }

        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
                NotifyOfPropertyChange(() => IsVisible);
            }
        }

        public bool IsBlank
        {
            get
            {
                return string.IsNullOrWhiteSpace(DisplayName) && string.IsNullOrWhiteSpace(Description);
            }
        }

        //public bool IsRecordset
        //{
        //    get
        //    {
        //        return Children.Count > 0;
        //    }
        //}

        //public bool IsField
        //{
        //    get
        //    {
        //        return Parent != null;
        //    }
        //}

        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                _isExpanded = value;
                NotifyOfPropertyChange(() => IsExpanded);
            }
        }

        #endregion Properties

        #region Methods

        public void RemoveError()
        {
            HasError = false;
            ErrorMessage = string.Empty;
        }

        //public void Filter(string searchText)
        //{
        //    Children.Clear();
        //    if(_backupChildren != null)
        //    {
        //        foreach(var dataListItemModel in _backupChildren)
        //        {
        //            Children.Add(dataListItemModel);
        //        }
        //    }
        //    if(string.IsNullOrEmpty(searchText))
        //    {
        //        return;
        //    }

        //    if(!String.IsNullOrEmpty(searchText))
        //    {
        //        _backupChildren = _backupChildren?? new ObservableCollection<IDataListItemModel>();
        //        foreach(var dataListItemModel in Children)
        //        {
        //            _backupChildren.Add(dataListItemModel);
        //        }
        //    }
        //    _backupChildren = Children;
        //    Children = new ObservableCollection<IDataListItemModel>(Children.Where(a=>a.DisplayName.ToUpper().Contains(searchText.ToUpper())));
        //}

        //public string FilterText
        //{
        //    get
        //    {
        //        string child ="";
        //        if(Children!= null)
        //            child = String.Join("",Children.Select(a=>a.FilterText));
        //        return DisplayName + child;
        //    }
        //    set
        //    {
        //        _filterText = value;
        //    }
        //}

        public bool AllowNotes
        {
            get
            {
                return _allowNotes;
            }

            set
            {
                _allowNotes = value;
                NotifyOfPropertyChange(() => AllowNotes);
            }
        }

        public bool IsComplexObject
        {
            get
            {
                return _isComplexObject;
            }

            set
            {
                _isComplexObject = value;
                NotifyOfPropertyChange(() => IsComplexObject);
            }
        }

        public void SetError(string errorMessage)
        {
            HasError = true;
            ErrorMessage = errorMessage;
        }

        

        //private void SetChildInputValues(bool value)
        //{
        //    UpdatingChildren = true;
        //    var updatedChildren = new OptomizedObservableCollection<IDataListItemModel>();
        //    if(Children != null)
        //    {
        //        foreach(var dataListItemModel in Children)
        //        {
        //            var child = (DataListItemModel)dataListItemModel;
        //            if (!string.IsNullOrEmpty(child.DisplayName))
        //            {
        //                child.UpdatingChildren = true;
        //                child.Input = value;
        //                child.UpdatingChildren = false;
        //                updatedChildren.Add(child);
        //            }
        //        }
        //    }
        //    UpdatingChildren = false;
        //    if(Children != null)
        //    {
        //        Children.Clear();
        //        foreach(var dataListItemModel in updatedChildren)
        //        {
        //            Children.Add(dataListItemModel);
        //        }
        //    }
        //}

        //private void SetChildOutputValues(bool value)
        //{
        //    UpdatingChildren = true;
        //    var updatedChildren = new OptomizedObservableCollection<IDataListItemModel>();
        //    if(Children != null)
        //    {
        //        foreach(var dataListItemModel in Children)
        //        {
        //            var child = (DataListItemModel)dataListItemModel;
        //            if (!string.IsNullOrEmpty(child.DisplayName))
        //            {
        //                child.UpdatingChildren = true;
        //                child.Output = value;
        //                child.UpdatingChildren = false;
        //                updatedChildren.Add(child);
        //            }
        //        }
        //    }
        //    UpdatingChildren = false;
        //    if(Children != null)
        //    {
        //        Children.Clear();
        //        foreach(var dataListItemModel in updatedChildren)
        //        {
        //            Children.Add(dataListItemModel);
        //        }

        //    }
        //}

        protected void SetColumnIODirectionFromInput(bool value)
        {

            if (!value)
            {
                if (_columnIODir == enDev2ColumnArgumentDirection.Both)
                {
                    _columnIODir = enDev2ColumnArgumentDirection.Output;
                }
                else if (_columnIODir == enDev2ColumnArgumentDirection.Input)
                {
                    _columnIODir = enDev2ColumnArgumentDirection.None;
                }
            }
            else
            {
                if (_columnIODir == enDev2ColumnArgumentDirection.Output)
                {
                    _columnIODir = enDev2ColumnArgumentDirection.Both;
                }
                else if (_columnIODir == enDev2ColumnArgumentDirection.None)
                {
                    _columnIODir = enDev2ColumnArgumentDirection.Input;
                }
            }
            NotifyIOPropertyChanged();
        }

        protected void SetColumnIODirectionFromOutput(bool value)
        {

            if (!value)
            {
                if (_columnIODir == enDev2ColumnArgumentDirection.Both)
                {
                    _columnIODir = enDev2ColumnArgumentDirection.Input;
                }
                else if (_columnIODir == enDev2ColumnArgumentDirection.Output)
                {
                    _columnIODir = enDev2ColumnArgumentDirection.None;
                }
            }
            else
            {
                if (_columnIODir == enDev2ColumnArgumentDirection.Input)
                {
                    _columnIODir = enDev2ColumnArgumentDirection.Both;
                }
                else if (_columnIODir == enDev2ColumnArgumentDirection.None)
                {
                    _columnIODir = enDev2ColumnArgumentDirection.Output;
                }
            }
           NotifyIOPropertyChanged();
           
        }

        private void NotifyIOPropertyChanged()
        {
            NotifyOfPropertyChange(() => ColumnIODirection);
            NotifyOfPropertyChange(() => Input);
            NotifyOfPropertyChange(() => Output);
        }

        #endregion Methods

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