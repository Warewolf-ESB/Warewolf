using System;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Dev2.Studio.Core.DataList;
using Dev2.Studio.Core.Interfaces.DataList;
using System.Xml;

namespace Dev2.Studio.Core.Models.DataList
{
    public class DataListItemModel : BaseDataListItemModel, IDataListItemModel
    {
        #region Fields

        private string _description;
        private IDataListItemModel _parent;
        private bool _hasError;
        private string _errorMessage;
        private bool _isEditable;
        private bool _isVisable;
        private bool _isSelected;
        private string _lastIndexedName;
        private bool _isUsed;
        private enDev2ColumnArgumentDirection _columnIODir;

        #endregion Fields

        #region Ctor

        /*
         * This is a piss poor implementation of optional parameters.... Constructor chaining would be far better suited to our needs, or even better a factory or builder!!!! ;)
         */
        public DataListItemModel(string displayname, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection = enDev2ColumnArgumentDirection.None, string description = "", IDataListItemModel parent = null, OptomizedObservableCollection<IDataListItemModel> children = null, bool hasError = false, string errorMessage = "", bool isEditable = true, bool isVisable = true, bool isSelected = false, bool isExpanded = true)
        {
            Validator = new DataListValidator();
            Description = description;
            Parent = parent;
            Children = children;
            HasError = hasError;
            ErrorMessage = errorMessage;
            IsEditable = isEditable;
            IsVisable = isVisable;
            DisplayName = displayname;
            IsSelected = isSelected;
            IsExpanded = isExpanded;
            LastIndexedName = Name;
            IsUsed = true;
            ColumnIODirection = dev2ColumnArgumentDirection;
        }

        #endregion Ctor

        #region Properties

        public bool UpdatingChildren { get; private set; }

        public IDataListValidator Validator { get; set; }

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

        public string LastIndexedName
        {
            get
            {
                return _lastIndexedName;
            }
            set
            {
                _lastIndexedName = value;
                NotifyOfPropertyChange(() => LastIndexedName);
            }
        }

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

        public IDataListItemModel Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                _parent = value;
                NotifyOfPropertyChange(() => Parent);
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

        public bool Input
        {
            get
            {
                return (_columnIODir == enDev2ColumnArgumentDirection.Both 
                    || _columnIODir == enDev2ColumnArgumentDirection.Input);
            }
            set
            {
                SetColumnIODirection(value, enDev2ColumnArgumentDirection.Input);
                SetChildInputValues(value);
            }
        }

        public bool Output
        {
            get
            {
                return (_columnIODir == enDev2ColumnArgumentDirection.Both 
                    || _columnIODir == enDev2ColumnArgumentDirection.Output);
            }
            set
            {
                SetColumnIODirection(value, enDev2ColumnArgumentDirection.Output);
                SetChildOutputValues(value);
            }
        }

        public bool IsVisable
        {
            get
            {
                return _isVisable;
            }
            set
            {
                _isVisable = value;
                NotifyOfPropertyChange(() => IsVisable);
            }
        }

        public bool IsBlank
        {
            get
            {
                return string.IsNullOrWhiteSpace(DisplayName) && string.IsNullOrWhiteSpace(Description);
            }
        }

        public bool IsRecordset
        {
            get
            {
                return Children.Count > 0;
            }
        }

        public bool IsField
        {
            get
            {
                return Parent != null;
            }
        }

        #endregion Properties

        #region Methods

        public void RemoveError()
        {
            HasError = false;
            ErrorMessage = string.Empty;
        }

        public void SetError(string errorMessage)
        {
            HasError = true;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Determines whether [name is valid].
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        ///   <c>true</c> if [name is valid]; otherwise, <c>false</c>.
        /// </returns>
        public override string ValidateName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                if (IsRecordset)
                {
                    name = DataListUtil.RemoveRecordsetBracketsFromValue(name);
                }
                else if (IsField)
                {
                    name = DataListUtil.ExtractFieldNameFromValue(name);
                }

                try
                {
                    if(!string.IsNullOrEmpty(name))
                    {
                    XmlConvert.VerifyName(name);
                    if (!string.Equals(ErrorMessage, StringResources.ErrorMessageDuplicateValue, StringComparison.InvariantCulture) && 
                        !string.Equals(ErrorMessage, StringResources.ErrorMessageDuplicateVariable, StringComparison.InvariantCulture) && 
                        !string.Equals(ErrorMessage, StringResources.ErrorMessageDuplicateRecordset, StringComparison.InvariantCulture))
                    {
                        RemoveError();
                    }
                }
                }
                catch
                {
                    SetError(StringResources.ErrorMessageInvalidChar);
                }
            }
            return name;
        }

        private void SetChildInputValues(bool value)
        {
            UpdatingChildren = true;

            if (Children != null)
            {
                foreach (DataListItemModel child in Children)
                {
                    child.Input = value;
                }
            }

            UpdatingChildren = false;
        }

        private void SetChildOutputValues(bool value)
        {
            UpdatingChildren = true;

            if (Children != null)
            {
                foreach (DataListItemModel child in Children)
                {
                    child.Output = value;
                }
            }

            UpdatingChildren = false;
        }

        private void SetColumnIODirection(bool value, enDev2ColumnArgumentDirection direction)
        {
            enDev2ColumnArgumentDirection original = _columnIODir;

            if (!value)
            {
                _columnIODir = (enDev2ColumnArgumentDirection)(_columnIODir - direction);
            }
            else
            {
                _columnIODir = _columnIODir + (int)direction;
            }

            if (original != _columnIODir)
            {
                NotifyIOPropertyChanged();
            }
        }

        private void NotifyIOPropertyChanged()
        {
            NotifyOfPropertyChange(() => ColumnIODirection);
            NotifyOfPropertyChange(() => Input);
            NotifyOfPropertyChange(() => Output);
        }
        #endregion
    }
}
