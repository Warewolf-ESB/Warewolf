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
using System.Text;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core.AppResources.Enums;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.AppResources.Comparers
{
    /// <summary>
    /// Used to uniquely identify a specific WorkSurface (I.E. something displayed in a Tab)
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2/27/2013</date>
    public class WorkSurfaceKey : IWorkSurfaceKey,IEquatable<WorkSurfaceKey>
    {
        public WorkSurfaceContext WorkSurfaceContext { get; set; }

        public Guid? ServerID { get; set; }

        public Guid? ResourceID { get; set; }
        
        public Guid? EnvironmentID { get; set; }

        public string StrValue => ToString();

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Context_{0}_", WorkSurfaceContext);
            if (ServerID.HasValue)
                sb.AppendFormat("ServerID_{0}_", ServerID);
            if (ResourceID.HasValue)
                sb.AppendFormat("ResourceID_{0}_", ResourceID);
            if (EnvironmentID.HasValue)
                sb.AppendFormat("EnvironmentID_{0}_", EnvironmentID);
            var returnString = sb.ToString().Replace('-', '_');
            return returnString;
        }

        //left this in because jurie. 
        public bool Equals(WorkSurfaceKey other)
        {
            return WorkSurfaceKeyEqualityComparer.Current.Equals(this, other);
        }

        #region Equality members



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
            return Equals((WorkSurfaceKey)obj);
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
                var hashCode = (int)WorkSurfaceContext;
                hashCode = (hashCode * 397) ^ ServerID.GetHashCode();
                hashCode = (hashCode * 397) ^ ResourceID.GetHashCode();
                hashCode = (hashCode * 397) ^ EnvironmentID.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(WorkSurfaceKey left, WorkSurfaceKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(WorkSurfaceKey left, WorkSurfaceKey right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}
