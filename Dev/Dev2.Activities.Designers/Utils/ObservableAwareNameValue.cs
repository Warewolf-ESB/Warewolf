/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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

        public override string Name
        {
            get => _name;
            set
            {
                if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(_value) && string.IsNullOrEmpty(_name) && ReferenceEquals(_sourceCollection.Last(), this))
                {
                    _sourceCollection.Add(new ObservableAwareNameValue(_sourceCollection, _update));
                }
                _name = value;
                _update?.Invoke(_name);
                OnPropertyChanged();
            }
        }

        public override string Value
        {
            get => _value;
            set
            {
                if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(_value) && string.IsNullOrEmpty(_name) && ReferenceEquals(_sourceCollection.Last(), this))
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
    }
}
