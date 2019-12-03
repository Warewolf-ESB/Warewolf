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
    public interface IEnabled
    {
        bool Enabled { get; set; }
    }

    public interface IOptionHelp
    {
        string HelpText { get; }
        string Tooltip { get; }
    }

    public interface IOption : ICloneable, IComparable, IOptionHelp
    {
        string Name { get; set; }
    }

    public interface IOptionComboBox : IOption
    {
        string Value { get; set; }
    }

    public interface IOptionBasic<T> : IOption
    {
        T Value { get; set; }
        T Default { get; }
    }

    public interface IOptionBasic<TKey, TValue> : IOption
    {
        KeyValuePair<TKey, TValue> Value { get; set; }
        KeyValuePair<TKey, TValue> Default { get; }
    }
     

    public interface IOptionAutocomplete : IOptionBasic<string>
    {
        string[] Suggestions { get; }
    }

    public interface IOptionEnum : IOptionBasic<int>
    {
        IEnumerable<KeyValuePair<string, int>> Values { get; set; }
    }

    public interface IOptionEnumGen : IOptionBasic<string, int>
    {
        IEnumerable<KeyValuePair<string, int>> Values { get; set; }
    }

    public interface IOptionInt : IOptionBasic<int>
    {

    }

    public interface IOptionBool : IOptionBasic<bool>
    {

    }

    public interface IOptionWorkflow : IOptionBasic<Guid>
    {
        //NamedGuid WorkflowName { get; set; }
    }
}
