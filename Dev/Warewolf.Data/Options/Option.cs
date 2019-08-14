/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Warewolf.Options;

namespace Warewolf.Data.Options
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
        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public string Default => string.Empty;

        public string[] Suggestions => new string[] { "" };
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
        public int Value
        {
            get => _value;
            set
            {
                SetProperty(ref _value, value);
            }
        }

        public int Default => 0;
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
        public bool Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public bool Default => true;
    }
}
