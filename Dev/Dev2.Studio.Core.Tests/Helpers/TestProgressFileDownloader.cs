
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Utils;
using Dev2.Common.Interfaces.Wrappers;

namespace Dev2.CustomControls.Progress
{
    class TestProgressFileDownloader : ProgressFileDownloader
    {
        public TestProgressFileDownloader(IWarewolfWebClient webClient,IFile file,ICryptoProvider crypt)
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
