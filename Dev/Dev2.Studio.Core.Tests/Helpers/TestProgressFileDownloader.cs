using Dev2.Common.Utils;
using Dev2.Common.Wrappers.Interfaces;
using Dev2.Studio.Core.Helpers;

namespace Dev2.CustomControls.Progress
{
    class TestProgressFileDownloader : ProgressFileDownloader
    {
        public TestProgressFileDownloader(IDev2WebClient webClient,IFile file,ICryptoProvider crypt)
            : base(webClient, file,crypt)
        {
        }

        public void TestCancelDownload()
        {
            Cancel();
        }

        public void TestRehydrateDialog(string fileName, int progressPercent, long totalBytes)
        {
            RehydrateDialog(fileName, progressPercent, totalBytes);
        }

        public void TestStartUpdate(string fileName, bool cancelled)
        {
            StartUpdate(fileName, cancelled);
        }

        public IProgressNotifier GetProgressDialog()
        {
            return ProgressDialog;
        }
    }
}