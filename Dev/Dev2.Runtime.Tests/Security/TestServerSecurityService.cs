using System.Collections.Generic;
using System.IO;
using Dev2.Runtime.Security;

namespace Dev2.Tests.Runtime.Security
{
    public class TestServerSecurityService : ServerSecurityService
    {
        readonly List<bool> _onFileChangedEnableRaisingEventsEnabled = new List<bool>();

        public TestServerSecurityService(string fileName)
            : base(fileName)
        {
        }

        public int OnFileChangedHitCount { get; private set; }

        protected override void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            OnFileChangedHitCount++;
            base.OnFileChanged(sender, e);
        }

        public int OnFileRenamedHitCount { get; private set; }

        protected override void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            OnFileRenamedHitCount++;
            base.OnFileRenamed(sender, e);
        }

        public List<bool> OnFileChangedEnableRaisingEventsEnabled { get { return _onFileChangedEnableRaisingEventsEnabled; } }

        protected override void OnFileChangedEnableRaisingEvents(bool enabled)
        {
            _onFileChangedEnableRaisingEventsEnabled.Add(enabled);
            base.OnFileChangedEnableRaisingEvents(enabled);
        }
    }
}