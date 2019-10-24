/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;

namespace Warewolf.Options
{
    public class OptionAutocomplete : BindableBase, IOptionAutocomplete
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _value;

        public event EventHandler<OptionValueChangedArgs<string>> ValueUpdated;

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public string Default => string.Empty;

        public string[] Suggestions { get; set; }

        public object Clone()
        {
            return new OptionAutocomplete
            {
                Name = _name,
                Value = _value
            };
        }

        public int CompareTo(object obj)
        {
            if (obj is null)
            {
                return -1;
            }
            var item = obj as OptionAutocomplete;
            if (item is null)
            {
                return -1;
            }

            return string.Compare(item.Name, Name, StringComparison.InvariantCulture) | string.Compare(item.Value, Value);
        }
    }

    public class OptionInt : BindableBase, IOptionInt
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private int _value;

        public event EventHandler<OptionValueChangedArgs<int>> ValueUpdated;

        public int Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public int Default => 0;

        public object Clone()
        {
            return new OptionInt
            {
                Name = _name,
                Value = _value
            };
        }

        public int CompareTo(object obj)
        {
            if (obj is null)
            {
                return -1;
            }
            var item = obj as OptionInt;
            if (item is null)
            {
                return -1;
            }
            return string.Compare(item.Name, Name, StringComparison.InvariantCulture) | (item.Value == Value ? 0 : -1);
        }
    }

    public class OptionBool : BindableBase, IOptionBool
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private bool _value;

        public event EventHandler<OptionValueChangedArgs<bool>> ValueUpdated;

        public bool Value
        {
            get => _value;
            set
            {
                var eventArgs = new OptionValueChangedArgs<bool>(_name, _value, value);
                _value = value;
                RaisePropertyChanged(nameof(Value));
                ValueUpdated?.Invoke(this, eventArgs);
            }
        }

        public bool Default => true;

        public object Clone()
        {
            return new OptionBool
            {
                Name = _name,
                Value = _value
            };
        }

        public int CompareTo(object obj)
        {
            if (obj is null)
            {
                return -1;
            }
            var item = obj as OptionBool;
            if (item is null)
            {
                return -1;
            }
            return string.Compare(item.Name, Name, StringComparison.InvariantCulture) | (item.Value == Value ? 0 : -1);
        }
    }

    public class OptionEnum : BindableBase, IOptionEnum
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private int _value;

        public event EventHandler<OptionValueChangedArgs<int>> ValueUpdated;

        public int Value
        {
            get => _value;
            set
            {
                var eventArgs = new OptionValueChangedArgs<int>(_name, _value, value);
                _value = value;
                RaisePropertyChanged(nameof(Value));
                ValueUpdated?.Invoke(this, eventArgs);
            }
        }

        public int Default { get; set; }

        public object Clone()
        {
            return new OptionEnum
            {
                Name = _name,
                Value = _value
            };
        }

        public int CompareTo(object obj)
        {
            if (obj is null)
            {
                return -1;
            }
            var item = obj as OptionEnum;
            if (item is null)
            {
                return -1;
            }
            return string.Compare(item.Name, Name, StringComparison.InvariantCulture) | (item.Value == Value ? 0 : -1);
        }
    }

    public class OptionCombobox : BindableBase, IOptionComboBox
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public Dictionary<string, IEnumerable<IOption>> Options { get; } = new Dictionary<string, IEnumerable<IOption>>();

        private string _value;

        public event EventHandler<OptionValueChangedArgs<string>> ValueUpdated;

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public int CompareTo(object obj)
        {
            if (obj is null)
            {
                return -1;
            }
            var item = obj as OptionCombobox;
            if (item is null)
            {
                return -1;
            }

            return string.Compare(item.Name, Name, StringComparison.InvariantCulture) | string.Compare(item.Value, Value);
        }
    }
}
