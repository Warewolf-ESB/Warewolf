/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
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
    }
}