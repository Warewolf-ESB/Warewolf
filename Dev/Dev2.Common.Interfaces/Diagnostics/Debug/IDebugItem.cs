/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Xml.Serialization;

namespace Dev2.Common.Interfaces.Diagnostics.Debug
{
    // If you add/remove columns here then 
    // change DebugState.Serialize/Deserialize
    public interface IDebugItem : IXmlSerializable
    {
        List<IDebugItemResult> ResultsList { get; set; }
        bool Contains(string filterText);
        void Add(IDebugItemResult itemToAdd, bool isDeserialize = false);
        void AddRange(List<IDebugItemResult> itemsToAdd);
        IList<IDebugItemResult> FetchResultsList();
        void FlushStringBuilder();
        void TryCache(IDebugItemResult item);
        string SaveFile(string contents, string fileName);
    }
}