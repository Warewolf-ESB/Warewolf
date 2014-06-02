using System;

namespace Dev2.CustomControls.Progress
{
    public interface IProgressFileDownloader
    {
        void Download(Uri address, string tmpfileName, bool dontStartUpdate,string fileName, string checkSum);
        void StartUpdate(string fileName, bool onlyCloseDialog);
    }
}