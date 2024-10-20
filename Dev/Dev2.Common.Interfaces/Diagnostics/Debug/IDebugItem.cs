/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Diagnostics.Debug
{
    // If you add/remove columns here then 
    // change DebugState.Serialize/Deserialize
    public interface IDebugItem 
    {
        List<IDebugItemResult> ResultsList { get; set; }
        bool Contains(string filterText);
        void Add(IDebugItemResult itemToAdd, bool isDeserialize);
        void Add(IDebugItemResult itemToAdd);
        void AddRange(List<IDebugItemResult> itemsToAdd);
        IList<IDebugItemResult> FetchResultsList();
        void FlushStringBuilder();
        string SaveFile(string contents, string fileName);
    }
}