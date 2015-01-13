using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.DataList.DatalistView;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels.VariableList
{
    public class DataListViewRecordSetViewModel : BindableBase, IDatalistViewRecordSet, IEquatable<DataListViewRecordSetViewModel>
    {
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
                return ((_name != null ? _name.GetHashCode() : 0) * 397) ^ (_columns != null ? _columns.Select(a=> a!=null?a.GetHashCode():0).Aggregate((a,b)=>a^b.GetHashCode()) : 0);
                // ReSharper restore NonReadonlyFieldInGetHashCode
            }
        }

        public static bool operator ==(DataListViewRecordSetViewModel left, DataListViewRecordSetViewModel right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DataListViewRecordSetViewModel left, DataListViewRecordSetViewModel right)
        {
            return !Equals(left, right);
        }

        #endregion

        string _name;
        bool _input;
        bool _output;
        string _notes;
        bool _used;
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        IList<IDataListViewColumn> _columns;
        // ReSharper restore FieldCanBeMadeReadOnly.Local
        string _recordsetName;
        bool _visible;

        public DataListViewRecordSetViewModel(string recsetName, IList<IDataListViewColumn> dataListViewColumns)
        {
            _name = recsetName;
            _columns = dataListViewColumns;
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

        public IList<IDataListViewColumn> Columns
        {
            get
            {
                return _columns;
            }
        }
        public string RecordsetName
        {
            get
            {
                return _recordsetName;
            }
            set
            {
                _recordsetName = value;
                OnPropertyChanged("RecordsetName");
            }
        }

        public void AddColumn(IDataListViewColumn dataListViewColumn)
        {
            Columns.Add(dataListViewColumn);
        }

        public void RemoveColumn(IDataListViewColumn dataListViewColumn)
        {
            Columns.Remove(dataListViewColumn);
        }

        #endregion

        public bool Equals(DataListViewRecordSetViewModel other)
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
            return Equals((DataListViewRecordSetViewModel)obj);
        }

        #endregion
    }

        #endregion
    
}