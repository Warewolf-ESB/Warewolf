/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

namespace Warewolf.Options
{
    public interface IOption
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
}
