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
    }
}