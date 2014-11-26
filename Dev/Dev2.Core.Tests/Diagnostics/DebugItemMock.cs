
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
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics;

namespace Dev2.Tests.Diagnostics
{
    class DebugItemMock : DebugItem
    {

        public int SaveFileHitCount { get; set; }
        public string SaveFileContents { get; private set; }

        #region Implementation of IDebugItem

        public new bool Contains(string filterText)
        {
            return false;
        }



        public void AddRange(IList<IDebugItemResult> itemsToAdd)
        {
        }

        public new IList<IDebugItemResult> FetchResultsList()
        {
            return null;
        }

        public new void FlushStringBuilder()
        {
        }        

        public override string SaveFile(string contents, string fileName)
        {
            SaveFileHitCount++;
            SaveFileContents = contents;
            return null;
        }

        #endregion
    }
}
