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
using System.Collections.Generic;
using System.Runtime.Serialization;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.Graph;

namespace Unlimited.Framework.Converters.Graph.Ouput
{
    /// <summary>
    ///     Stores the information necessary to describe the shape of a data source
    /// </summary>
    [Serializable]
    public class DataSourceShape : IDataSourceShape
    {
        #region Constructors

        public DataSourceShape()
        {
            Paths = new List<IPath>();
        }

        #endregion Constructors

        #region Properties

        [DataMember(Name = "Paths")]
        public List<IPath> Paths { get; set; }

        #endregion Properties

        public bool Equals(IDataSourceShape other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            var collectionEquals = CommonEqualityOps.CollectionEquals(Paths, other.Paths, EqualityFactory.GetEqualityComparer<IPath>(EqualsMethod, GetHashCodeMethod));
            return collectionEquals;
        }

        private int GetHashCodeMethod(IPath path)
        {
            return path.GetHashCode();
        }

        private bool EqualsMethod(IPath path, IPath path1)
        {
            if (path == null && path1 == null) return true;
            if (path == null || path1 == null) return false;
            var equalTypes = path.GetType() == path1.GetType();
            var equals = string.Equals(path.ActualPath, path1.ActualPath)
                && string.Equals(path.DisplayPath, path1.DisplayPath)
                && string.Equals(path.OutputExpression, path1.OutputExpression)
                && string.Equals(path.SampleData, path1.SampleData)
                && equalTypes;
            return equals;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IDataSourceShape)obj);
        }

        public override int GetHashCode()
        {
            return (Paths != null ? Paths.GetHashCode() : 0);
        }
    }
}