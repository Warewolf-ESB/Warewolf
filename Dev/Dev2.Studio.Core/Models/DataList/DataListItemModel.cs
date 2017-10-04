/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Caliburn.Micro;
using Dev2.Data.Interfaces.Enums;
using Dev2.Studio.Interfaces.DataList;




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

        public DataListItemModel(string displayname): this(displayname, enDev2ColumnArgumentDirection.None, "", false, "", true, true, false, true)
        {
        }

        public DataListItemModel(string displayname, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection, string description, bool hasError, string errorMessage, bool isEditable, bool isVisible, bool isSelected, bool isExpanded)
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
                return _columnIODir == enDev2ColumnArgumentDirection.Both || _columnIODir == enDev2ColumnArgumentDirection.Input;
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

        public bool IsBlank => string.IsNullOrWhiteSpace(DisplayName) && string.IsNullOrWhiteSpace(Description);

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

        public bool Equals(IDataListItemModel other)
        {
            return string.Equals(Description, other.Description)
                && HasError == other.HasError
                && string.Equals(ErrorMessage, other.ErrorMessage)
                && IsEditable == other.IsEditable
                && IsVisible == other.IsVisible
                && IsSelected == other.IsSelected 
                && IsUsed == other.IsUsed
                && AllowNotes == other.AllowNotes 
                && IsComplexObject == other.IsComplexObject
                && string.Equals(DisplayName, other.DisplayName) 
                && ColumnIODirection == other.ColumnIODirection   
                && string.Equals(Name, other.Name)
                && Equals(IsBlank, other.IsBlank)
                && Equals(Output, other.Output)
                && Equals(Input, other.Input);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DataListItemModel) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Description != null ? Description.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ HasError.GetHashCode();
                hashCode = (hashCode * 397) ^ (ErrorMessage != null ? ErrorMessage.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsEditable.GetHashCode();
                hashCode = (hashCode * 397) ^ IsVisible.GetHashCode();
                hashCode = (hashCode * 397) ^ IsSelected.GetHashCode();
                hashCode = (hashCode * 397) ^ IsUsed.GetHashCode();
                hashCode = (hashCode * 397) ^ AllowNotes.GetHashCode();
                hashCode = (hashCode * 397) ^ IsComplexObject.GetHashCode();
                hashCode = (hashCode * 397) ^ (DisplayName != null ? DisplayName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)ColumnIODirection;
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}