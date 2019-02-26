using Dev2.Common;
using Dev2.Common.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Dev2.Activities.Utils
{
    public class ObservableAwareNameValue : NameValue
    {
        readonly ObservableCollection<INameValue> _sourceCollection;
        readonly Action<string> _update;

        public ObservableAwareNameValue(ObservableCollection<INameValue> sourceCollection, Action<string> update)
        {
            _sourceCollection = sourceCollection;
            _update = update;

            Name = "";

            Value = "";
            AddRowCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(AddRow);
            RemoveRowCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(RemoveRow);

        }

        void RemoveRow()
        {
            if (!ReferenceEquals(_sourceCollection.Last(), this))
            {
                _sourceCollection.Remove(this);
            }
        }

        void AddRow()
        {
            _sourceCollection.Insert(_sourceCollection.IndexOf(this), new ObservableAwareNameValue(_sourceCollection, _update));
        }

        #region Overrides of NameValue

        public override string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (!String.IsNullOrEmpty(value) && String.IsNullOrEmpty(_value) && String.IsNullOrEmpty(_name) && ReferenceEquals(_sourceCollection.Last(), this))
                {
                    _sourceCollection.Add(new ObservableAwareNameValue(_sourceCollection, _update));
                }
                _name = value;
                _update?.Invoke(_name);
                OnPropertyChanged();
            }
        }

        #region Overrides of NameValue

        public override string Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (!String.IsNullOrEmpty(value) && String.IsNullOrEmpty(_value) && String.IsNullOrEmpty(_name) && ReferenceEquals(_sourceCollection.Last(), this))
                {
                    _sourceCollection.Add(new ObservableAwareNameValue(_sourceCollection, _update));
                }
                _value = value;
                _update?.Invoke(_value);
                OnPropertyChanged();
            }
        }
        public ICommand RemoveRowCommand { get; set; }
        public ICommand AddRowCommand { get; set; }

        #endregion

        #endregion
    }
}
