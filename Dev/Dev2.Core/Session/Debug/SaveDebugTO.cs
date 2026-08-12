#pragma warning disable
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
using System.Runtime.Serialization;

namespace Dev2.Session
{
    [DataContract]
    public class SaveDebugTO
    {
        #region Properties

        [DataMember]
        public string WorkflowXaml { get; set; }

        [DataMember]
        public string DataList { get; set; }

        [DataMember]
        public string ServiceName { get; set; }

        [DataMember]
        public bool IsDebugMode { get; set; }

        [DataMember]
        public bool RememberInputs { get; set; }

        [DataMember]
        public string XmlData { get; set; }

        [DataMember]
        public string JsonData { get; set; }

        [DataMember]
        public string WorkflowID { get; set; }

        [DataMember]
        public int DataListHash { get; set; }

        #endregion Properties
    }
}