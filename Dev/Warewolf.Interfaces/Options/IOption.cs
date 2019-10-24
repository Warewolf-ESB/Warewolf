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

namespace Warewolf.Options
{
    public interface IOption : ICloneable, IComparable
    {
        string Name { get; set; }
    }

    public interface IOptionComboBox : IOption, IOptionNotifyUpdate<string>
    {
        string Value { get; set; }
    }

    public interface IOptionBasic<T> : IOption, IOptionNotifyUpdate<T>
    {
        T Value { get; set; }
        T Default { get; }
    }

    public interface IOptionAutocomplete : IOptionBasic<string>
    {
        string[] Suggestions { get; }
    }

    public interface IOptionEnum : IOptionBasic<Enum>
    {

    }

    public interface IOptionInt : IOptionBasic<int>
    {

    }

    public interface IOptionBool : IOptionBasic<bool>
    {

    }

    public interface IOptionNotifyUpdate<T>
    {
        event EventHandler<OptionValueChangedArgs<T>> ValueUpdated;
    }

    public class OptionValueChangedArgs<T>
    {
        public OptionValueChangedArgs(string name, T oldValue, T newValue)
        {
            Name = name;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public string Name { get; }
        public T OldValue { get; }
        public T NewValue { get; }
    }
}
