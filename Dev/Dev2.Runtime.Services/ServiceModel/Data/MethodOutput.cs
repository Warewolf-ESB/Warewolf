#pragma warning disable
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
using System.Runtime.Serialization;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Runtime.ServiceModel.Data
{
    [DataContract]
    public class MethodOutput : IDev2Definition
    {
        public MethodOutput(string name, string mapsTo, string value, bool isRecordSet, string recordSetName, bool isEvaluated, string defaultValue, bool isRequired, string rawValue, bool emptyToNull)
        {
            EmptyToNull = emptyToNull;
            RawValue = rawValue;
            IsRequired = isRequired;
            DefaultValue = defaultValue;
            IsEvaluated = isEvaluated;
            RecordSetName = recordSetName;
            IsRecordSet = isRecordSet;
            Value = value;
            MapsTo = mapsTo;
            Name = name;
        }
        public MethodOutput()
        {

        }
        #region Properties

        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public string MapsTo { get; private set; }

        [DataMember]
        public string Value { get; private set; }

        [DataMember]
        public bool IsRecordSet { get; private set; }

        [DataMember]
        public string RecordSetName { get; private set; }

        [DataMember]
        public bool IsEvaluated { get; private set; }

        [DataMember]
        public string DefaultValue { get; private set; }

        [DataMember]
        public bool IsRequired { get; private set; }

        [DataMember]
        public string RawValue { get; private set; }

        [DataMember]
        public bool EmptyToNull { get; private set; }

        [DataMember]
        public bool IsTextResponse { get;  set; }

        [DataMember]
        public bool IsObject { get; set; }

        [DataMember]
        public bool IsJsonArray { get; set; }

        #endregion

    }
}
