using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2;
using Dev2.Common.Interfaces.DataList.DatalistView;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels.VariableList
{
    public class VariableListViewRecordSetViewModel : BindableBase,  IVariablelistViewRecordSetViewModel, IEquatable<VariableListViewRecordSetViewModel>
    {

        string _name;
        bool _input;
        bool _output;
        string _notes;
        bool _used;
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        ICollection<IVariableListViewColumnViewModel> _columns;
        readonly IVariableListViewModel _parent;
        // ReSharper restore FieldCanBeMadeReadOnly.Local
        string _recordsetName;
        bool _visible;
        bool _inputVisible;
        bool _outputVisible;
        string _toolTip;
        bool _isValid;
        bool _deleteVisible;
        ICollection<IVariablelistViewRecordSetViewModel> _parentCollection;

        // ReSharper disable once TooManyDependencies
        public VariableListViewRecordSetViewModel(string recsetName, ICollection<IVariableListViewColumnViewModel> dataListViewColumns, IVariableListViewModel parent, ICollection<IVariablelistViewRecordSetViewModel> parentCollection)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "recsetName", recsetName }, { "dataListViewColumns", dataListViewColumns }, { "parent", parent } });
            _name = recsetName;
            _columns = dataListViewColumns;
            _parent = parent;
            _parentCollection = parentCollection;

            _notes = "";
            InputVisible = true;
            OutputVisible = true;
            EditNotes = new DelegateCommand(DeleteThis);
            Delete = new DelegateCommand(DeleteThis);
        }

        #region Implementation of IDataListViewItem
        public void DeleteThis()
        {
            _parent.Delete(this);
        }
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                if(ReferenceEquals( ParentCollection.Last(),this))
                {
                    ParentCollection.Add(new VariableListViewRecordSetViewModel("", new ObservableCollection<IVariableListViewColumnViewModel>(),  _parent, _parentCollection));
                }
                OnPropertyChanged("Name");
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
                OnPropertyChanged("Input");
                _input = value;
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
                OnPropertyChanged("Output");
                _output = value;
                foreach(var dataListViewColumn in _columns)
                {
                    dataListViewColumn.Output = value;
                }

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
                OnPropertyChanged("Notes");
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
                OnPropertyChanged("Used");
                DeleteVisible = !value;
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
                OnPropertyChanged("Visible");
                _visible = value;
            }
        }

        #endregion

        #region Implementation of IDatalistViewRecordSet

        public ICollection<IVariableListViewColumnViewModel> Columns
        {
            get
            {
                return _columns;
            }
        }

        public void AddColumn(IVariableListViewColumnViewModel variableListViewColumn)
        {
            if(!Columns.Contains(variableListViewColumn))
            Columns.Add(variableListViewColumn);
        }

        public void RemoveColumn(IVariableListViewColumnViewModel variableListViewColumn)
        {

            if (Columns.Contains(variableListViewColumn))
            Columns.Remove(variableListViewColumn);
        }

        #endregion

        public bool Equals(VariableListViewRecordSetViewModel other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            if(ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(_name, other._name) && Equals(_columns, other._columns);
        }

        #region Overrides of Object

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
            return Equals((VariableListViewRecordSetViewModel)obj);
        }

        #endregion

        #region Equality members

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyFieldInGetHashCode
                return ((_name != null ? _name.GetHashCode() : 0) * 397);
                // ReSharper restore NonReadonlyFieldInGetHashCode
            }
        }

        public static bool operator ==(VariableListViewRecordSetViewModel left, VariableListViewRecordSetViewModel right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(VariableListViewRecordSetViewModel left, VariableListViewRecordSetViewModel right)
        {
            return !Equals(left, right);
        }

        #endregion

        #region Implementation of IDatalistViewRecordSetViewModel

        public DelegateCommand EditNotes { get; private set; }
        public DelegateCommand Delete { get; private set; }
        public bool DeleteVisible
        {
            get
            {
                return _deleteVisible;
            }
            set
            {
                if (value != _deleteVisible)
                {

                    _deleteVisible = value;
                    InputVisible = !value;
                    OutputVisible = !value;
                    OnPropertyChanged(() => DeleteVisible);
                }
            }
        }

        public bool InputVisible
        {
            get
            {
                return _inputVisible;
            }
            set
            {
                if (value != _inputVisible)
                {
                    
                    _inputVisible = value;
                    DeleteVisible = !value;
                    OnPropertyChanged(() => InputVisible);
                }
            }
        }
        public bool OutputVisible
        {
            get
            {
                return _outputVisible;
            }
            set
            {
                if (value != _outputVisible)
                {
                    
                    _outputVisible = value;
                    DeleteVisible = !value;
                    OnPropertyChanged(() => OutputVisible);
                }
            }
        }
        public string ToolTip
        {
            get
            {
                return _toolTip;
            }
            set
            {
                _toolTip = value;
                OnPropertyChanged(() => ToolTip);
            }
        }
        public bool IsValid
        {
            get
            {
                return _isValid;
            }
            set
            {
                _isValid = value;
                OnPropertyChanged(() => IsValid);
            }
        }
        public ICollection<IVariablelistViewRecordSetViewModel> ParentCollection
        {
            get
            {
                return _parentCollection;
            }
        }

        #endregion
    }

        #endregion
    
}