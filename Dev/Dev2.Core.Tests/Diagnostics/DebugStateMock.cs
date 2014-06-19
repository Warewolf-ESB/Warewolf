using System.Collections.Generic;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;

namespace Dev2.Tests.Diagnostics
{
    public class DebugStateMock : DebugState
    {
        public int SaveFileHitCount { get; set; }
        public string SaveFileContents { get; private set; }

        //9142 TODO
        public string SaveFile(string contents)
        {
            SaveFileHitCount++;
            SaveFileContents = contents;
            return null;
        }

        //9142 TODO
        public void TryCache(IList<IDebugItem> items)
        {
            
        }
    }
}
