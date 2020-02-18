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
using System.Linq;
using System.Windows.Input;
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

        private string _helpText = Studio.Resources.Languages.HelpText.OptionAutocompleteHelpText;
        public string HelpText
        {
            get => _helpText;
            set => SetProperty(ref _helpText, value);
        }

        private string _tooltip = Studio.Resources.Languages.Tooltips.OptionAutocompleteTooltip;
        public string Tooltip
        {
            get => _tooltip;
            set => SetProperty(ref _tooltip, value);
        }
        private string _value;

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

        private string _helpText = Studio.Resources.Languages.HelpText.OptionIntHelpText;
        public string HelpText
        {
            get => _helpText;
            set => SetProperty(ref _helpText, value);
        }

        private string _tooltip = Studio.Resources.Languages.Tooltips.OptionIntTooltip;
        public string Tooltip
        {
            get => _tooltip;
            set => SetProperty(ref _tooltip, value);
        }

        private int _value;

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

        private string _helpText = Studio.Resources.Languages.HelpText.OptionBoolHelpText;
        public string HelpText
        {
            get => _helpText;
            set => SetProperty(ref _helpText, value);
        }

        private string _tooltip = Studio.Resources.Languages.Tooltips.OptionBoolTooltip;
        public string Tooltip
        {
            get => _tooltip;
            set => SetProperty(ref _tooltip, value);
        }
        private bool _value;
        public bool Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
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
        private string _helpText = Studio.Resources.Languages.HelpText.OptionEnumHelpText;
        public string HelpText
        {
            get => _helpText;
            set => SetProperty(ref _helpText, value);
        }

        private string _tooltip = Studio.Resources.Languages.Tooltips.OptionEnumTooltip;
        public string Tooltip
        {
            get => _tooltip;
            set => SetProperty(ref _tooltip, value);
        }
        private int _value;

        public IEnumerable<KeyValuePair<string, int>> Values { get; set; }

        public List<string> OptionNames
        {
            get
            {
                var optionNames = new List<string>();
                foreach (var opt in Values)
                {
                    optionNames.Add(opt.Key.ToString());
                }
                return optionNames;
            }
        }

        private string _optionName;
        public string OptionName
        {
            get => _optionName;
            set
            {
                if (value is null)
                {
                    value = Values?.First().Key ?? "";
                }

                if (SetProperty(ref _optionName, value))
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        Value = Values?.First(o => o.Key == value).Value ?? 0;
                    }
                }
            }
        }

        public int Value
        {
            get => _value;
            set
            {
                if (SetProperty(ref _value, value) || OptionName is null)
                {
                    OptionName = Values?.First(o => o.Value == value).Key;
                }
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

        private string _helpText = Studio.Resources.Languages.HelpText.OptionEnumGenHelpText;
        public string HelpText
        {
            get => _helpText;
            set => SetProperty(ref _helpText, value);
        }

        private string _tooltip = Studio.Resources.Languages.Tooltips.OptionEnumGenTooltip;
        public string Tooltip
        {
            get => _tooltip;
            set => SetProperty(ref _tooltip, value);
        }
        private KeyValuePair<string, int> _value;

        public IEnumerable<KeyValuePair<string, int>> Values { get; set; }

        public KeyValuePair<string, int> Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
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


        private string _helpText = Studio.Resources.Languages.HelpText.OptionComboboxHelpText;
        public string HelpText
        {
            get => _helpText;
            set => SetProperty(ref _helpText, value);
        }

        private string _tooltip = Studio.Resources.Languages.Tooltips.OptionComboboxTooltip;
        public string Tooltip
        {
            get => _tooltip;
            set => SetProperty(ref _tooltip, value);
        }

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

    public class OptionRadioButtons : BindableBase, IOptionRadioButton
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _helpText;
        public string HelpText
        {
            get => _helpText;
            set => SetProperty(ref _helpText, value);
        }

        private string _tooltip;
        public string Tooltip
        {
            get => _tooltip;
            set => SetProperty(ref _tooltip, value);
        }

        public Dictionary<string, IEnumerable<IOption>> Options { get; } = new Dictionary<string, IEnumerable<IOption>>();

        private List<OptionName> _list;
        public List<OptionName> OptionNames
        {
            get
            {
                if (_list is null)
                {
                    _list = Options.Keys.Select(o => new OptionName(this, o)).ToList();
                }
                return _list;
            }
        }
        public class OptionName : IDisposable
        {
            private OptionRadioButtons _optionRadioButton;

            public OptionName(OptionRadioButtons optionRadioButton, string name)
            {
                this._optionRadioButton = optionRadioButton;
                Name = name;
            }

            public string Name { get; }
            public bool IsChecked
            {
                get => _optionRadioButton?.Value == Name;
                set
                {
                    if (value && _optionRadioButton != null)
                    {
                        _optionRadioButton.Value = Name;
                    }
                }
            }

            public void Dispose()
            {
                _optionRadioButton = null;
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

        public Orientation Orientation { get; set; }

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
            var item = obj as OptionRadioButtons;
            if (item is null)
            {
                return -1;
            }

            return string.Compare(item.Name, Name, StringComparison.InvariantCulture) | string.Compare(item.Value, Value);
        }
    }

    public class OptionWorkflow : BindableBase, IOptionWorkflow
    {
        private string _helpText = Studio.Resources.Languages.HelpText.OptionWorkflowHelpText;
        public string HelpText
        {
            get => _helpText;
            set => SetProperty(ref _helpText, value);
        }

        private string _tooltip = Studio.Resources.Languages.Tooltips.OptionWorkflowTooltip;
        public string Tooltip
        {
            get => _tooltip;
            set => SetProperty(ref _tooltip, value);
        }
        public string Name { get; set; }

        private IWorkflow _workflow;
        public IWorkflow Workflow
        {
            get => _workflow;
            set => SetProperty(ref _workflow, value);
        }

        public object Clone()
        {
            return new OptionWorkflow
            {
                Name = Name,
                Workflow = Workflow,
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

            return string.Compare(item.Name, Name, StringComparison.InvariantCulture) | (item.Workflow == Workflow ? 0 : -1);
        }
    }

    public class OptionActivity : BindableBase, IOptionActivity
    {
        private object _value; // ModelItem _value;
        public object Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public string Name { get; set; }
        private string _helpText = Studio.Resources.Languages.HelpText.OptionWorkflowHelpText;
        public string HelpText
        {
            get => _helpText;
            set => SetProperty(ref _helpText, value);
        }

        private string _tooltip = Studio.Resources.Languages.Tooltips.OptionWorkflowTooltip;
        public string Tooltip
        {
            get => _tooltip;
            set => SetProperty(ref _tooltip, value);
        }

        public object Clone()
        {
            return new OptionActivity
            {
                Name = Name,
                Value = Value,
            };
        }

        public int CompareTo(object obj)
        {
            if (obj is null)
            {
                return -1;
            }
            var item = obj as OptionActivity;
            if (item is null)
            {
                return -1;
            }

            return string.Compare(item.Name, Name, StringComparison.InvariantCulture) | (item.Value == Value ? 0 : -1);
        }
    }

    public class OptionConditionExpression : BindableBase, IOption
    {
        public string Name { get; set; }

        public ICommand DeleteCommand { get; set; }

        private string _left;
        public string Left
        {
            get => _left;
            set
            {
                SetProperty(ref _left, value);
            }
        }
        public static INamedInt[] MatchTypes { get; } = NamedInt.GetAll(typeof(enDecisionType)).ToArray();

        private INamedInt _selectedMatchType;
        public INamedInt SelectedMatchType
        {
            get => _selectedMatchType;
            set
            {
                if (value != null && SetProperty(ref _selectedMatchType, value))
                {
                    MatchType = (enDecisionType)value.Value;
                    RaisePropertyChanged(nameof(IsBetween));
                    RaisePropertyChanged(nameof(IsSingleOperand));
                }
            }
        }
        public enDecisionType MatchType { get; set; }

        private string _right;
        public string Right
        {
            get => _right;
            set
            {
                SetProperty(ref _right, value);
            }
        }

        private string _from;
        public string From
        {
            get => _from;
            set
            {
                SetProperty(ref _from, value);
            }
        }

        private string _to;
        public string To
        {
            get => _to;
            set
            {
                SetProperty(ref _to, value);
            }
        }
        public bool IsBetween => MatchType.IsTripleOperand();
        public bool IsSingleOperand => MatchType.IsSingleOperand();
        public bool IsEmptyRow
        {
            get
            {
                var isEmptyRow = string.IsNullOrEmpty(Left);
                isEmptyRow &= SelectedMatchType is null;
                if (IsSingleOperand)
                {
                    isEmptyRow &= string.IsNullOrEmpty(Right);
                }
                if (IsBetween)
                {
                    isEmptyRow &= string.IsNullOrEmpty(From);
                    isEmptyRow &= string.IsNullOrEmpty(To);
                }
                return isEmptyRow;
            }
        }

        private string _helpText;
        public string HelpText
        {
            get => _helpText;
            set => SetProperty(ref _helpText, value);
        }

        private string _tooltip;

        public string Tooltip
        {
            get => _tooltip;
            set => SetProperty(ref _tooltip, value);
        }
        public object Clone()
        {
            return new OptionConditionExpression
            {
                Name = Name,
                Left = Left,
                SelectedMatchType = SelectedMatchType,
                Right = Right,
                From = From,
                To = To,
            };
        }

        public int CompareTo(object obj)
        {
            if (obj is null)
            {
                return -1;
            }
            var item = obj as OptionConditionExpression;
            if (item is null)
            {
                return -1;
            }
            return string.Compare(item.Name, Name, StringComparison.InvariantCulture) |
                   string.Compare(item.Left, Left, StringComparison.InvariantCulture) |
                   (item.SelectedMatchType == SelectedMatchType ? 0 : -1) |
                   string.Compare(item.Right, Right, StringComparison.InvariantCulture) |
                   string.Compare(item.From, From, StringComparison.InvariantCulture) |
                   string.Compare(item.To, To, StringComparison.InvariantCulture);
        }
    }
}
