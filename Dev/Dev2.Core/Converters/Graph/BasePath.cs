/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
    // Deliberately [Serializable] rather than [DataContract]: DataSourceShape/BasePath's
    // OutputDescription XML is round-tripped via DataContractSerializer's plain-CLR-object
    // (POCO) fallback, which serializes members using compiler-generated backing-field names
    // (e.g. "_x003C_Paths_x003E_k__BackingField") and silently no-ops on a mismatched shape
    // instead of throwing. Real persisted resources (and existing test fixtures) depend on
    // that exact fallback wire format and lenient-failure behavior; switching to [DataContract]
    // changes both the wire format and the failure mode, breaking backward compatibility with
    // already-persisted OutputDescription XML. This type has no ISerializable/GetObjectData
    // and is never round-tripped through BinaryFormatter, so [Serializable] carries no
    // .NET 8+/BinaryFormatter-deprecation risk.
    [Serializable]
    public abstract class BasePath : IPath
    {
        #region Constructor

        protected BasePath()
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

        public override string ToString() => ActualPath;

        #endregion Override Methods

        #region Abstract Methods

        public abstract IEnumerable<IPathSegment> GetSegements();
        public abstract IPathSegment CreatePathSegment(string pathSegmentString);

        #endregion Abstract Methods
    }
}