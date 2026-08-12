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
using Dev2.Common.Interfaces;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel.Data
{
    [CollectionDataContract]
    public class NamespaceList : List<NamespaceItem>
    {
        public override string ToString() => JsonConvert.SerializeObject(this);
    }

    [DataContract]
    public class NamespaceItem:INamespaceItem
    {
        #region ToString

        public override string ToString() => JsonConvert.SerializeObject(this);

        #endregion

        [DataMember]
        public string AssemblyLocation { get; set; }

        [DataMember]
        public string AssemblyName { get; set; }

        [DataMember]
        public string FullName { get; set; }

        [DataMember]
        public string MethodName { get; set; }

        [DataMember]
        public string JsonObject { get; set; }
    }
}
