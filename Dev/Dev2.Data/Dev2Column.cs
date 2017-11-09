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
using Dev2.Data.Interfaces.Enums;


namespace Dev2.DataList.Contract.Binary_Objects
{
    [Serializable]
    public class Dev2Column : IEquatable<Dev2Column>
    {
        #region Properties
        
        public string ColumnName { get; set; }
        
        public string ColumnDescription { get; set; }
        
        public bool IsEditable { get; set; }
        
        public enDev2ColumnArgumentDirection ColumnIODirection { get; set; }

        #endregion Properties
        
        #region Ctor
        
        internal Dev2Column(string columnName, string columnDesciption, bool isEditable, enDev2ColumnArgumentDirection dir)
        {
            ColumnName = columnName;
            ColumnDescription = columnDesciption;
            IsEditable = isEditable;
            ColumnIODirection = dir; 
        }

        #region Equality members
        
        public bool Equals(Dev2Column other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            if(ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(ColumnName, other.ColumnName) && string.Equals(ColumnDescription, other.ColumnDescription) && IsEditable == other.IsEditable && ColumnIODirection == other.ColumnIODirection;
        }

        public static bool operator ==(Dev2Column left, Dev2Column right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Dev2Column left, Dev2Column right)
        {
            return !Equals(left, right);
        }

        #endregion

        #endregion Ctor       
    }
}
