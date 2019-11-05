/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Common.Interfaces.Interfaces;
using Dev2.Providers.Validation.Rules;
using Dev2.Util;
using System;

namespace Warewolf.UI
{
    public class DataColumnMapping : ValidatedObject, IDev2TOFn, IEquatable<DataColumnMapping>
    {
        int _indexNumber;
        string _inputColumn;
        IDbColumn _outputColumn;

        [FindMissing]
        public string InputColumn { get => _inputColumn; set => OnPropertyChanged(ref _inputColumn, value); }

        public IDbColumn OutputColumn { get => _outputColumn; set => OnPropertyChanged(ref _outputColumn, value); }

        public int IndexNumber { get => _indexNumber; set => OnPropertyChanged(ref _indexNumber, value); }

        public bool CanRemove() => false;

        public bool CanAdd() => false;

        public void ClearRow() { }

        public bool Inserted { get; set; }

        public override IRuleSet GetRuleSet(string propertyName, string datalist) => new RuleSet();

        public bool Equals(DataColumnMapping other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var outputColumnSame = OutputColumn == null && other.OutputColumn == null;

            if (OutputColumn != null)
            {
                outputColumnSame = OutputColumn.Equals(other.OutputColumn);
            }
            return IndexNumber == other.IndexNumber
                && string.Equals(InputColumn, other.InputColumn)
                && outputColumnSame
                && Inserted == other.Inserted;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((DataColumnMapping)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = IndexNumber;
                hashCode = (hashCode * 397) ^ (InputColumn != null ? InputColumn.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OutputColumn != null ? OutputColumn.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Inserted.GetHashCode();
                return hashCode;
            }
        }
    }
}
