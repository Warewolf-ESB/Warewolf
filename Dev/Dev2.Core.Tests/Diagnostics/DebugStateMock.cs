using Dev2.Diagnostics;

namespace Dev2.Tests.Diagnostics
{
    public class DebugStateMock : DebugState
    {
        public int SaveFileHitCount { get; set; }
        public string SaveFileContents { get; private set; }

        public override string SaveFile(string contents)
        {
            SaveFileHitCount++;
            SaveFileContents = contents;
            return null;
        }
    }
}
