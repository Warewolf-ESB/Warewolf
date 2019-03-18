#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2;
using Dev2.Common.Interfaces.DB;
using Dev2.Data.Util;
using Dev2.Util;



namespace Warewolf.Core
{
    public class ServiceOutputMapping : ObservableObject, IServiceOutputMapping, IEquatable<ServiceOutputMapping>
    {
        string _mappedFrom;
        string _mappedTo;
        string _recordSetName;

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(ServiceOutputMapping other) => false;

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((ServiceOutputMapping)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode() => 397 ^ MappedFrom.GetHashCode() ^ MappedTo.GetHashCode();

        public static bool operator ==(ServiceOutputMapping left, ServiceOutputMapping right) => Equals(left, right);

        public static bool operator !=(ServiceOutputMapping left, ServiceOutputMapping right) => !Equals(left, right);

        public ServiceOutputMapping(string mappedFrom, string mapping, string recordsetName)
        {
            MappedFrom = mappedFrom;
            MappedTo = mapping;
            RecordSetName = recordsetName;
            if (string.IsNullOrEmpty(_recordSetName) && !string.IsNullOrEmpty(mapping))
            {
                MappedTo = DataListUtil.AddBracketsToValueIfNotExist(mapping);
            }
        }

        public ServiceOutputMapping()
            : this("", "", "")
        {
        }

        public string MappedFrom
        {
            get
            {
                return _mappedFrom;
            }
            set
            {
                _mappedFrom = value;
                OnPropertyChanged();
            }
        }

        [FindMissing]
        public string MappedTo
        {
            get
            {
                return _mappedTo;
            }
            set
            {
                _mappedTo = value;
                UpdateMappingRecordSetValue(value);
                OnPropertyChanged();
            }
        }

        public string RecordSetName
        {
            get
            {
                return _recordSetName;
            }
            set
            {
                UpdateMappedToValue(value);
                _recordSetName = value;
                OnPropertyChanged();
            }
        }

        void UpdateMappedToValue(string newRecordsetName)
        {
            if (!string.IsNullOrEmpty(_recordSetName) && !string.IsNullOrEmpty(_mappedTo) && DataListUtil.IsValueRecordsetWithFields(_mappedTo))
            {
                var recSetName = DataListUtil.ExtractRecordsetNameFromValue(_mappedTo);
                var fieldName = DataListUtil.ExtractFieldNameOnlyFromValue(_mappedTo);
                if (string.Equals(recSetName, _recordSetName, StringComparison.OrdinalIgnoreCase) && !string.Equals(recSetName, newRecordsetName, StringComparison.OrdinalIgnoreCase))
                {
                    MappedTo = DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.CreateRecordsetDisplayValue(newRecordsetName, fieldName, ""));
                }
                if (string.IsNullOrEmpty(newRecordsetName) && string.IsNullOrEmpty(DataListUtil.ExtractRecordsetNameFromValue(_mappedTo)))
                {
                    MappedTo = DataListUtil.AddBracketsToValueIfNotExist(fieldName);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(_recordSetName) && !string.IsNullOrEmpty(newRecordsetName) && !string.IsNullOrEmpty(_mappedTo) && !DataListUtil.IsValueRecordsetWithFields(_mappedTo))
                {
                    var varName = DataListUtil.RemoveLanguageBrackets(_mappedTo);
                    MappedTo = DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.CreateRecordsetDisplayValue(newRecordsetName, varName, ""));
                }
            }
        }
        void UpdateMappingRecordSetValue(string newMappedTo)
        {
            if (!string.IsNullOrEmpty(newMappedTo))
            {
                if (!DataListUtil.IsValueRecordset(newMappedTo) && !string.IsNullOrEmpty(RecordSetName))
                {
                    _recordSetName = string.Empty;
                }
                else
                {
                    if (DataListUtil.IsValueRecordset(newMappedTo) && string.IsNullOrEmpty(RecordSetName))
                    {
                        _recordSetName = DataListUtil.ExtractRecordsetNameFromValue(newMappedTo);
                    }
                }
                OnPropertyChanged("RecordSetName");
            }
        }
    }
}
