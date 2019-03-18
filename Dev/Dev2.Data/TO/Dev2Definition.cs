#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Data;





namespace Dev2.DataList.Contract
{
    public class Dev2Definition : IDev2Definition
    {
        public Dev2Definition()
        {
        }

        public Dev2Definition(string name, string mapsTo, string value, bool isEvaluated, string defaultValue, bool isRequired, string rawValue) : this(name, mapsTo, value, string.Empty, isEvaluated, defaultValue, isRequired, rawValue) { }

        public Dev2Definition(string name, string mapsTo, string value, string recordSet, bool isEvaluated, string defaultValue, bool isRequired, string rawValue)
            
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
        public bool IsJsonArray { get; set; }

        public static Dev2Definition NewObject(string name, string mapsTo, string value, bool isEval, string defaultValue, bool isRequired, string rawValue, bool emptyToNull) {
            return new Dev2Definition(name, mapsTo, value, string.Empty, isEval, defaultValue, isRequired, rawValue, emptyToNull);
        }
    }
}
