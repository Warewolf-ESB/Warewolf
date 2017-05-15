/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Data;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable once CheckNamespace
namespace Dev2.DataList.Contract
{
    public class Dev2Definition : IDev2Definition
    {
        #region Ctor

        public Dev2Definition()
        {
        }

        public Dev2Definition(string name, string mapsTo, string value, bool isEvaluated, string defaultValue, bool isRequired, string rawValue) : this(name, mapsTo, value, string.Empty, isEvaluated, defaultValue, isRequired, rawValue) { }

        public Dev2Definition(string name, string mapsTo, string value, string recordSet, bool isEvaluated, string defaultValue, bool isRequired, string rawValue)
            // ReSharper disable once IntroduceOptionalParameters.Global
            : this(name, mapsTo, value, recordSet, isEvaluated, defaultValue, isRequired, rawValue, false)
        {
        }

        public Dev2Definition(string name, string mapsTo, string value, string recordSet, bool isEvaluated, string defaultValue, bool isRequired, string rawValue, bool emptyToNull)
        {
            Name = name;
            MapsTo = mapsTo;
            Value = value;
            RecordSetName = recordSet;
            IsEvaluated = isEvaluated;
            DefaultValue = defaultValue;
            IsRequired = isRequired;
            RawValue = rawValue;
            EmptyToNull = emptyToNull;
        }
        #endregion

        #region Properties

        public string Name { get; set; }

        public string MapsTo { get; set; }

        public string Value { get; set; }

        public bool IsRecordSet => !(RecordSetName == null || RecordSetName.Equals(string.Empty));

        public string RecordSetName { get; set; }

        public bool IsEvaluated { get; set; }

        public string DefaultValue { get; set; }

        public bool IsRequired { get; set; }

        public string RawValue { get; set; }

        public bool EmptyToNull { get; set; }
        public bool IsTextResponse { get;  set; }
        public bool IsObject { get; set; }
        public bool IsArray { get; set; }

        #endregion
    }
}
