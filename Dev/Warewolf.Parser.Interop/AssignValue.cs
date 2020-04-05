/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Warewolf.Data;

namespace WarewolfParserInterop
{
    public class AssignValue : IAssignValue, IEquatable<AssignValue>
    {
        public AssignValue(string name, string value)
        {
            Value = value;
            Name = name;
        }

        #region Implementation of IAssignValue

        public string Name { get; private set; }
        public string Value { get; private set; }

        #endregion

        #region Equality members

        public bool Equals(AssignValue other) => string.Equals(Name, other.Name) && string.Equals(Value, other.Value);

        public override bool Equals(object obj) => Equals((AssignValue)obj);

        public override int GetHashCode() => throw new NotImplementedException();

        #endregion
    }
}
