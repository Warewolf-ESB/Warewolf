/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Data.Binary_Objects;

// ReSharper disable once CheckNamespace
namespace Dev2.DataList.Contract.Binary_Objects
{
    [Serializable]
    public class Dev2Column : IEquatable<Dev2Column>
    {

        #region Properties

        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>
        /// <value>
        /// The name of the column.
        /// </value>
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets the column description.
        /// </summary>
        /// <value>
        /// The column description.
        /// </value>
        public string ColumnDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is editable.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is editable; otherwise, <c>false</c>.
        /// </value>
        public bool IsEditable { get; set; }

        /// <summary>
        /// Gets or sets the argument direction.
        /// </summary>
        /// <value>
        /// The argument direction.
        /// </value>
        public enDev2ColumnArgumentDirection ColumnIODirection { get; set; }

        #endregion Properties
        
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="Dev2Column" /> class.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="columnDesciption">The column desciption.</param>
        internal Dev2Column(string columnName, string columnDesciption)
            : this(columnName,columnDesciption,true)
        {          
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dev2Column" /> class.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="ioDir">The io dir.</param>
        internal Dev2Column(string columnName, enDev2ColumnArgumentDirection ioDir)
            : this(columnName, string.Empty, true, ioDir)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Dev2Column" /> class.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="columnDesciption">The column desciption.</param>
        /// <param name="ioDir">The io dir.</param>
        internal Dev2Column(string columnName, string columnDesciption, enDev2ColumnArgumentDirection ioDir)
            : this(columnName,columnDesciption,true, ioDir)
        {          
        }



        /// <summary>
        /// Initializes a new instance of the <see cref="Dev2Column" /> class.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="columnDesciption">The column desciption.</param>
        /// <param name="isEditable">if set to <c>true</c> [is editable].</param>
        internal Dev2Column(string columnName, string columnDesciption,bool isEditable) 
            : this(columnName, columnDesciption, isEditable, enDev2ColumnArgumentDirection.None)
        {
        }

        // <summary>
        /// <summary>
        /// Initializes a new instance of the <see cref="Dev2Column" /> class.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="columnDesciption">The column desciption.</param>
        /// <param name="isEditable">if set to <c>true</c> [is editable].</param>
        /// <param name="dir">The column dir.</param>
        internal Dev2Column(string columnName, string columnDesciption, bool isEditable, enDev2ColumnArgumentDirection dir)
        {
            ColumnName = columnName;
            ColumnDescription = columnDesciption;
            IsEditable = isEditable;
            ColumnIODirection = dir; 
        }

        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
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

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
            {
                return false;
            }
            if(ReferenceEquals(this, obj))
            {
                return true;
            }
            if(obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((Dev2Column)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ColumnName?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (ColumnDescription?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ IsEditable.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)ColumnIODirection;
                return hashCode;
            }
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
