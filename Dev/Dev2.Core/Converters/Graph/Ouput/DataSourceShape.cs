using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Unlimited.Framework.Converters.Graph.Ouput
{
    /// <summary>
    /// Stores the information necessary to describe the shape of a data source
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
    }
}
