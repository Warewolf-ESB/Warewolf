using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Diagnostics;

namespace Dev2.Tests.Diagnostics
{
    class DebugItemMock : DebugItem
    {

        public int SaveFileHitCount { get; set; }
        public string SaveFileContents { get; private set; }

        public DebugItemMock()
        {
            
        }

        #region Implementation of IDebugItem

        public new bool Contains(string filterText)
        {
            return false;
        }

        public new void Add(IDebugItemResult itemToAdd, bool isDeserialize = false)
        {
        }

        public new void AddRange(IList<IDebugItemResult> itemsToAdd)
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
