using System.Net;
using Dev2.Studio.Core.Helpers;

namespace Dev2.CustomControls.Progress
{
    class TestProgressFileDownloader : ProgressFileDownloader
    {
        public TestProgressFileDownloader(IDev2WebClient webClient, IProgressDialog progressDialog)
            : base(webClient, progressDialog)
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

        public IProgressDialog GetProgressDialog()
        {
            return _progressDialog;
        }
    }
}