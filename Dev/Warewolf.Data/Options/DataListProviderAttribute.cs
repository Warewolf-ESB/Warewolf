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
using System.Linq;

namespace Warewolf.Options
{
    public class DataProviderAttribute : Attribute
    {
        private readonly Type _providerType;

        public DataProviderAttribute(Type dataProvider)
        {
            _providerType = dataProvider;
        }

        public object Get()
        {
            return Activator.CreateInstance(_providerType);
        }
    }

    public class MultiDataProviderAttribute : Attribute
    {
        private readonly Type[] _providerTypes;

        public MultiDataProviderAttribute(params Type[] dataProvider)
        {
            _providerTypes = dataProvider;
        }

        public object[] Get()
        {
            return _providerTypes.Select(providerType => Activator.CreateInstance(providerType)).ToArray();
        }
    }

    public class DataValueAttribute : Attribute
    {
        private string _valueFieldName;

        public DataValueAttribute(string v)
        {
            this._valueFieldName = v;
        }

        internal string Get()
        {
            return _valueFieldName;
        }
    }
    public class HelpTextAttribute : Attribute
    {
        private string _helpTextValue;

        public HelpTextAttribute(string v)
        {
            var prop = typeof(Studio.Resources.Languages.HelpText).GetProperty(v);
            this._helpTextValue = prop.GetValue(null).ToString();
        }

        internal string Get()
        {
            return _helpTextValue;
        }
    }
    public class TooltipAttribute : Attribute
    {
        private string _tooltipValue;

        public TooltipAttribute(string v)
        {
            var prop = typeof(Studio.Resources.Languages.Tooltips).GetProperty(v);
            this._tooltipValue = prop.GetValue(null).ToString();
        }

        internal string Get()
        {
            return _tooltipValue;
        }
    }

}