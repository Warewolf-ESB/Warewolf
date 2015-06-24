
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Data.Storage.ProtocolBuffers
{
    /// <summary>
    /// Used to track aliases when shaping ;)
    /// </summary>
    [Serializable]
    public class BinaryDataListAlias
    {
        public Guid MasterKeyID;

        public bool IsCOW;

        public string MasterKey;

        public string ChildKey;

        public string MasterNamespace;

        public string MasterColumn;

        public IBinaryDataListEntry MasterEntry;

        
    }
}
