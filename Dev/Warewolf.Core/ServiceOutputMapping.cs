/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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

// ReSharper disable ExplicitCallerInfoArgument

namespace Warewolf.Core
{
    public class ServiceOutputMapping : ObservableObject,IServiceOutputMapping, IEquatable<ServiceOutputMapping>
    {
        private string _mappedFrom;
        private string _mappedTo;
        private string _recordSetName;

        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(ServiceOutputMapping other)
        {
            return false;
        }

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
        public override int GetHashCode()
        {
            return 397 ^ MappedFrom.GetHashCode() ^ MappedTo.GetHashCode();
        }

        public static bool operator ==(ServiceOutputMapping left, ServiceOutputMapping right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ServiceOutputMapping left, ServiceOutputMapping right)
        {
            return !Equals(left, right);
        }

        #endregion

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
        #region Implementation of IDbOutputMapping

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

        private void UpdateMappedToValue(string newRecordsetName)
        {
            if(!string.IsNullOrEmpty(_recordSetName) && !string.IsNullOrEmpty(_mappedTo) && DataListUtil.IsValueRecordsetWithFields(_mappedTo))
            {
                var recSetName = DataListUtil.ExtractRecordsetNameFromValue(_mappedTo);
                var fieldName = DataListUtil.ExtractFieldNameOnlyFromValue(_mappedTo);
                if(string.Equals(recSetName, _recordSetName, StringComparison.OrdinalIgnoreCase))
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
                if(string.IsNullOrEmpty(_recordSetName) && !string.IsNullOrEmpty(newRecordsetName) && !string.IsNullOrEmpty(_mappedTo) && !DataListUtil.IsValueRecordsetWithFields(_mappedTo))
                {
                    var varName = DataListUtil.RemoveLanguageBrackets(_mappedTo);
                    MappedTo = DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.CreateRecordsetDisplayValue(newRecordsetName, varName, ""));
                }
            }    
        }

        #endregion
    }
}
