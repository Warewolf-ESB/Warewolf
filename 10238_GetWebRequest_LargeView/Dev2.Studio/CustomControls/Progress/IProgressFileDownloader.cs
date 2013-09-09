using System;

namespace Dev2.CustomControls.Progress
{
    public interface IProgressFileDownloader
    {
        void Download(Uri address, string fileName, bool dontStartUpdate);
        void StartUpdate(string fileName, bool onlyCloseDialog);
    }
}