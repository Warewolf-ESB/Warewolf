/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Wrappers;
using Dev2.Studio.Core.Helpers;

namespace Dev2.CustomControls.Progress
{
    class TestProgressFileDownloader : ProgressFileDownloader
    {
        public TestProgressFileDownloader(IDev2WebClient webClient, IFile file)
            : base(webClient, file)
        {
        }

        public void TestCancelDownload() => Cancel();

        public void TestRehydrateDialog(string fileName, int progressPercent, long totalBytes) => RehydrateDialog(fileName, progressPercent, totalBytes);

        public IProgressNotifier GetProgressDialog() => ProgressDialog;
    }
}
