#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Studio.Core.Helpers;
using Dev2.Common;

namespace Dev2.CustomControls.Progress
{
    public class ProgressFileDownloader : IProgressFileDownloader
    {
        readonly IDev2WebClient _webClient;
        protected readonly IProgressNotifier ProgressDialog;
        readonly IFile _file;
        readonly Window _owner;
        readonly string _tmpFileName = "";

        public bool IsBusyDownloading { get; private set; }

        #region CTOR

        static Func<Window, Action, IProgressNotifier> getProgressDialogViewModel = (owner, cancelAction) => DialogViewModel(owner, cancelAction);
        
        static IProgressNotifier DialogViewModel(Window owner, Action cancelAction)
        {
            var dialog = new ProgressDialog(owner);
            dialog.Closed += (sender, args) => cancelAction?.Invoke();
            var dialogViewModel = new ProgressDialogViewModel(cancelAction, dialog.Show, dialog.Close);
            dialog.DataContext = dialogViewModel;
            return dialogViewModel;
        }

        public ProgressFileDownloader(IDev2WebClient webClient, IFile file)
        {
            VerifyArgument.IsNotNull("webClient", webClient);
            VerifyArgument.IsNotNull("file", file);
            _webClient = webClient;

            ProgressDialog = GetProgressDialogViewModel?.Invoke(_owner, Cancel);
            _file = file;
            _webClient.DownloadProgressChanged += OnDownloadProgressChanged;
            ShutDownAction = ShutdownAndInstall;
            if (!Directory.Exists("Installers"))
            {
                Directory.CreateDirectory("Installers");
            }
        }

        public static void PerformCleanup(IDirectory dir, string path, IFile file)
        {
            try
            {
                foreach(var v in dir.GetFiles(path).Where(a => a.Contains("tmp")))
                {
                    file.Delete(v);
                }
            }            
            catch (IOException e)
            {
                Dev2Logger.Info("Unable to delete temp file.\n" + e.Message, GlobalConstants.WarewolfInfo);
            }

        }

        #endregion


        #region Cancel

        public void Cancel()
        {
            _webClient.CancelAsync();
            ProgressDialog.Close();
            IsBusyDownloading = false;
            if(_file.Exists(_tmpFileName))
            {
                _file.Delete(_tmpFileName);
            }
        }

        #endregion

        #region OnDownloadProgressChanged // cant test this because the DownloadProgressChangedEventArgs has no public ctor
        void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs args)
        {

            RehydrateDialog((string)args.UserState, args.ProgressPercentage, args.TotalBytesToReceive);
        }

        protected void RehydrateDialog(string fileName, int progressPercent, long totalBytes)
        {
            ProgressDialog.StatusChanged(fileName, progressPercent, totalBytes);
        }

        #endregion

        #region OnDownloadFileCompleted

        public Action<string> ShutDownAction { get; set; }
        public static Func<Window, Action, IProgressNotifier> GetProgressDialogViewModel { get => getProgressDialogViewModel; set => getProgressDialogViewModel = value; }

        static void ShutdownAndInstall(string fileName)
        {
            Application.Current.Shutdown();
            Process.Start(fileName);
        }

        #endregion

   }
}
