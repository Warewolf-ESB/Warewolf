/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2;
using System;
using System.Collections.Generic;
using System.Linq;
using Warewolf.Data;

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

        public string HelpText => Studio.Resources.Languages.HelpText.OptionAutocompleteHelpText;

        public string Tooltip => Studio.Resources.Languages.Tooltips.OptionAutocompleteTooltip;

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

        public string HelpText => Studio.Resources.Languages.HelpText.OptionIntHelpText;

        public string Tooltip => Studio.Resources.Languages.Tooltips.OptionIntTooltip;

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

        public string HelpText => Studio.Resources.Languages.HelpText.OptionBoolHelpText;

        public string Tooltip => Studio.Resources.Languages.Tooltips.OptionBoolTooltip;

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

        public string HelpText => Studio.Resources.Languages.HelpText.OptionEnumHelpText;

        public string Tooltip => Studio.Resources.Languages.Tooltips.OptionEnumTooltip;

        private int _value;

        public IEnumerable<KeyValuePair<string, int>> Options { get; set; }

        public List<string> OptionNames
        {
            get
            {
                var optionNames = new List<string>();
                foreach (var opt in Options)
                {
                    optionNames.Add(opt.Key.ToString());
                }
                return optionNames;
            }
        }

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

    public class OptionEnumGen : BindableBase, IOptionEnumGen
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string HelpText => Studio.Resources.Languages.HelpText.OptionEnumGenHelpText;

        public string Tooltip => Studio.Resources.Languages.Tooltips.OptionEnumGenTooltip;

        private KeyValuePair<string, int> _value;

        public IEnumerable<KeyValuePair<string, int>> Options { get; set; }

        public event EventHandler<OptionValueChangedArgs<KeyValuePair<string, int>>> ValueUpdated;

        public KeyValuePair<string, int> Value
        {
            get => _value;
            set
            {
                var eventArgs = new OptionValueChangedArgs<KeyValuePair<string, int>>(_name, _value, value);
                _value = value;
                RaisePropertyChanged(nameof(Value));
                ValueUpdated?.Invoke(this, eventArgs);
            }
        }

        KeyValuePair<string, int> IOptionBasic<string, int>.Default { get; }

        public object Clone()
        {
            return new OptionEnumGen
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
            var item = obj as OptionEnumGen;
            if (item is null)
            {
                return -1;
            }
            return string.Compare(item.Name, Name, StringComparison.InvariantCulture) | (item.Value.Equals(Value) ? 0 : -1);
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

        public string HelpText => Studio.Resources.Languages.HelpText.OptionComboboxHelpText;

        public string Tooltip => Studio.Resources.Languages.Tooltips.OptionComboboxTooltip;

        public Dictionary<string, IEnumerable<IOption>> Options { get; } = new Dictionary<string, IEnumerable<IOption>>();

        public List<string> OptionNames
        {
            get
            {
                return Options.Keys.ToList();
            }
        }

        public IEnumerable<IOption> SelectedOptions
        { 
            get
            {
                if (string.IsNullOrEmpty(Value))
                {
                    return new List<IOption>();
                }
                return Options[Value];
            }
        }

        private string _value;

        public event EventHandler<OptionValueChangedArgs<string>> ValueUpdated;

        public string Value
        {
            get => _value;
            set
            {
                if (SetProperty(ref _value, value))
                {
                    RaisePropertyChanged(nameof(SelectedOptions));
                }
            }
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

    public class OptionWorkflow : BindableBase, IOptionWorkflow
    {
        private Guid _value;

        public Guid Value
        {
            get => _value;
            set
            {
                SetProperty(ref _value, value);
            }
        }

        public string HelpText => Studio.Resources.Languages.HelpText.OptionWorkflowHelpText;

        public string Tooltip => Studio.Resources.Languages.Tooltips.OptionWorkflowTooltip;

        public Guid Default;

        public string Name { get; set; }

        Guid IOptionBasic<Guid>.Default => Guid.Empty;

        private string _workflowName;

        public string WorkflowName 
        {
            get => _workflowName;
            set
            {
                SetProperty(ref _workflowName, value);
            }
        }

        private ICollection<IServiceInputBase> _inputs;
        public ICollection<IServiceInputBase> Inputs
        {
            get => _inputs;
            set
            {
                SetProperty(ref _inputs, value);
            }
        }

        public event EventHandler<OptionValueChangedArgs<Guid>> ValueUpdated;

        public object Clone()
        {
            return new OptionWorkflow
            {
                Name = Name,
                Value = _value,
                Inputs = _inputs
            };
        }

        public int CompareTo(object obj)
        {
            if (obj is null)
            {
                return -1;
            }
            var item = obj as OptionWorkflow;
            if (item is null)
            {
                return -1;
            }

            return string.Compare(item.Name, Name, StringComparison.InvariantCulture) | (item.Value == Value ? 0 : -1);
        }
    }
}
