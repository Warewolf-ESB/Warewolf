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

namespace Unlimited.Framework.Converters.Graph
{
    [Serializable]
    public abstract class BasePath : IPath
    {
        #region Constructor

        public BasePath()
        {
            ActualPath = "";
            DisplayPath = "";
            SampleData = "";
            OutputExpression = "";
        }

        #endregion Constructor

        #region Properties

        [DataMember(Name = "ActualPath")]
        public string ActualPath { get; set; }

        [DataMember(Name = "DisplayPath")]
        public string DisplayPath { get; set; }

        [DataMember(Name = "SampleData")]
        public string SampleData { get; set; }

        [DataMember(Name = "OutputExpression")]
        public string OutputExpression { get; set; }

        #endregion Properties

        #region Override Methods

        public override string ToString()
        {
            return ActualPath;
        }

        #endregion Override Methods

        #region Abstract Methods

        public abstract IEnumerable<IPathSegment> GetSegements();
        public abstract IPathSegment CreatePathSegment(string pathSegmentString);

        #endregion Abstract Methods
    }
}