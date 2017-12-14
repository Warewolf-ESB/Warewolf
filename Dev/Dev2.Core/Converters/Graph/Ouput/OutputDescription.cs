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
using Dev2.Comparers;

namespace Unlimited.Framework.Converters.Graph.Ouput
{
    /// <summary>
    ///     Stores the information necessary for an implementation of IOutputFormatter to format data coming form a source
    /// </summary>
    [DataContract(Name = "OutputDescription")]
    [Serializable]
    public class OutputDescription : IOutputDescription
    {
        #region Constructors

        public OutputDescription()
        {
            Format = OutputFormats.Unknown;
            DataSourceShapes = new List<IDataSourceShape>();
        }

        #endregion Constructors

        #region Properties

        [DataMember(Name = "Format")]
        public OutputFormats Format { get; set; }

        [DataMember(Name = "DataSourceShapes")]
        public List<IDataSourceShape> DataSourceShapes { get; set; }

        #endregion Properties

        public bool Equals(IOutputDescription other)
        {
            var collectionEquals = CommonEqualityOps.CollectionEquals(DataSourceShapes, other.DataSourceShapes, new DataSourceShapeComparer());
            return Format == other.Format && collectionEquals;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OutputDescription)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Format * 397) ^ (DataSourceShapes != null ? DataSourceShapes.GetHashCode() : 0);
            }
        }
    }
}