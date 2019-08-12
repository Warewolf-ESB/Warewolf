/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.ComponentModel;

namespace Warewolf.Options
{
    public interface IOption : INotifyPropertyChanged
    {
        string Name { get; set; }
    }

    public interface IOptionBasic<T> : IOption
    {
        T Value { get; set; }
        T Default { get; }
    }

    public interface IOptionAutocomplete : IOptionBasic<string>
    {
        string[] Suggestions { get; }
    }

    public interface IOptionInt : IOptionBasic<int>
    {

    }

    public interface IOptionBool : IOptionBasic<bool>
    {

    }

    public class OptionAutocomplete : IOptionAutocomplete
    {
        private readonly string _name;
        public string Name
        {
            get => _name;
            set => OnPropertyChanged(nameof(Name));
        }

        private readonly string _value;
        public string Value
        {
            get => _value;
            set => OnPropertyChanged(nameof(Value));
        }

        public string Default => string.Empty;

        public string[] Suggestions => new string[] { "" };

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }

    public class OptionInt : IOptionInt
    {
        private readonly string _name;
        public string Name
        {
            get => _name;
            set => OnPropertyChanged(nameof(Name));
        }

        private readonly int _value;
        public int Value
        {
            get => _value;
            set => OnPropertyChanged(nameof(Value));
        }

        public int Default => 0;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }

    public class OptionBool : IOptionBool
    {
        private readonly string _name;
        public string Name
        {
            get => _name;
            set => OnPropertyChanged(nameof(Name));
        }

        private readonly bool _value;
        public bool Value
        {
            get => _value;
            set => OnPropertyChanged(nameof(Value));
        }

        public bool Default => true;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
