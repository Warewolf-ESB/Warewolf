using System;
using System.Collections.Generic;
using System.Linq;
using Dev2;
using Dev2.Common.Interfaces.DataList.DatalistView;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels.VariableList
{
    public class VariableListItemViewScalarViewModel : BindableBase, IVariableListViewScalarViewModel, IEquatable<VariableListItemViewScalarViewModel>
    {
        readonly IVariableListViewModel _parent;

        string _name;
        bool _input;
        bool _output;
        string _notes;
        bool _used;
        bool _visible;
        readonly DelegateCommand _editNotes;
        readonly DelegateCommand _delete;
        bool _deleteVisible;
        bool _inputVisible;
        bool _outputVisible;
        string _toolTip;
        bool _isValid;
        readonly ICollection<IVariableListViewScalarViewModel> _parentCollection;

        public VariableListItemViewScalarViewModel(string scalarName, IVariableListViewModel parent, ICollection<IVariableListViewScalarViewModel> parentCollection)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "scalarName", scalarName },  { "parent", parent } });
            _parent = parent;
            _parentCollection = parentCollection;
            _editNotes = new DelegateCommand(EditVariableNotes);
            _delete =  new DelegateCommand(DeleteThis);
            _notes = "";
            InputVisible = true;
            OutputVisible = true;
            Used = true;
            Name = scalarName;
            Visible = true;

        }

        void DeleteThis()
        {
          _parent.Delete(this);
        }

        void EditVariableNotes()
        {
        }

        #region Implementation of IDataListViewItem

        public string Name
        {
            get
            {
                return _name;
                
            }
            set
            {
                _name = value;
                if(ReferenceEquals(ParentCollection.LastOrDefault(), this))
                {
                    ParentCollection.Add(new VariableListItemViewScalarViewModel("",_parent,ParentCollection));
                }
                OnPropertyChanged(()=>Name);
            }
        }
        public bool Input
        {
            get
            {
                return _input;
            }
            set
            {

                _input = value;
                OnPropertyChanged(() => Input);
            }
        }
        public bool Output
        {
            get
            {
                return _output;
            }
            set
            {
                _output = value;
                OnPropertyChanged(() => Output);
            }
        }
        public string Notes
        {
            get
            {
                return _notes;
            }
            set
            {
                _notes = value;
                OnPropertyChanged(() => Notes);
            }
        }
        public bool Used
        {
            get
            {
                return _used;
            }
            set
            {
                _used = value;
                DeleteVisible = !value;
                OnPropertyChanged(() => Used);
            }
        }
        public bool Visible
        {
            get
            {
                return _visible;
            }
            set
            {
                _visible = value;
                OnPropertyChanged(() => Visible);
            }
        }

        #region Overrides of Object

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
            {
                return false;
            }
            if(ReferenceEquals(this, obj))
            {
                return true;
            }
            if(obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((VariableListItemViewScalarViewModel)obj);
        }

        #endregion

        #endregion

        #region Implementation of IDatalistViewValueViewModel

        /// <summary>
        /// Command to Edit Noted
        /// </summary>
        public DelegateCommand EditNotes
        {
            get
            {
                return _editNotes;
            }
        }
        /// <summary>
        /// Command to delete
        /// </summary>
        public DelegateCommand Delete
        {
            get
            {
                return _delete;
            }
        }
        /// <summary>
        /// Is Delete Visible
        /// </summary>
        public bool DeleteVisible
        {
            get
            {
                return _deleteVisible;
            }
            set
            {
                if (_deleteVisible != value)
                {
                    _deleteVisible = value;
                    InputVisible = !value;
                    OutputVisible = !value;
                    OnPropertyChanged(() => DeleteVisible);
                }
            }
        }
        /// <summary>
        /// Isinput chackbox visible
        /// </summary>
        public bool InputVisible
        {
            get
            {
                return _inputVisible;
            }
            set
            {
                if (_inputVisible != value)
                {
                    _inputVisible = value;
                    DeleteVisible = !value;
                    OnPropertyChanged(() => InputVisible);
                }
            }
        }
        /// <summary>
        /// Its output chackbox visible
        /// </summary>
        public bool OutputVisible
        {
            get
            {
                return _outputVisible;
            }
            set
            {
                if (_outputVisible != value)
                {
                    _outputVisible = value;
                    DeleteVisible = !value;
                    OnPropertyChanged(() => OutputVisible);
                }
            }
        }
        /// <summary>
        /// Tooltip will be note or the validation message
        /// </summary>
        public string ToolTip
        {
            get
            {
                return _toolTip;
            }
            set
            {
                OnPropertyChanged(() => ToolTip);
                _toolTip = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsValid
        {
            get
            {
                return _isValid;
            }
            set
            {
                OnPropertyChanged(() => IsValid);
                _isValid = value;
            }
        }
        public ICollection<IVariableListViewScalarViewModel> ParentCollection
        {
            get
            {
                return _parentCollection;
            }
        }

        #endregion

        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(VariableListItemViewScalarViewModel other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(_name, other._name);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyFieldInGetHashCode
            return (_name != null ? _name.GetHashCode() : 0);
            // ReSharper restore NonReadonlyFieldInGetHashCode
        }

        public static bool operator ==(VariableListItemViewScalarViewModel left, VariableListItemViewScalarViewModel right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(VariableListItemViewScalarViewModel left, VariableListItemViewScalarViewModel right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}
