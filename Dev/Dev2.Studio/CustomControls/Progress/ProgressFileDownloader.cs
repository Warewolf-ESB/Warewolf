/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Security.Cryptography;
using System.Windows;
using Dev2.Common.Interfaces.Utils;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Helpers;
using Dev2.Studio.Core.Helpers;

namespace Dev2.CustomControls.Progress
{
    public class ProgressFileDownloader : IProgressFileDownloader
    {
        readonly IDev2WebClient _webClient;
        protected readonly IProgressNotifier ProgressDialog;
        private readonly IFile _file;
        private readonly ICryptoProvider _cryptoProvider;
        private readonly Window _owner;
        private string _tmpFileName = "";

        #region Properties

        public bool IsBusyDownloading { get; private set; }

        #endregion

        #region CTOR
        public ProgressFileDownloader(Window owner) // cant cover this because a window needs to be shown to be a parent of something else. 
            : this(new Dev2WebClient(new WebClient()), new FileWrapper(), new CryptoProvider(new SHA256CryptoServiceProvider()))
        {
            _owner = owner;
        }

        public static Func<Window, Action, IProgressNotifier> GetProgressDialogViewModel = (owner, cancelAction) => DialogViewModel(owner, cancelAction);

        
        static IProgressNotifier DialogViewModel(Window owner, Action cancelAction)
        {
            var dialog = new ProgressDialog(owner);
            dialog.Closed += (sender, args) => cancelAction();
            var dialogViewModel = new ProgressDialogViewModel(cancelAction, dialog.Show, dialog.Close);
            dialog.DataContext = dialogViewModel;
            return dialogViewModel;
        }

        public ProgressFileDownloader(IDev2WebClient webClient, IFile file, ICryptoProvider cryptoProvider)
        {
            VerifyArgument.IsNotNull("webClient", webClient);
            VerifyArgument.IsNotNull("file", file);
            VerifyArgument.IsNotNull("cryptoProvider", cryptoProvider);
            _webClient = webClient;

            ProgressDialog = GetProgressDialogViewModel(_owner, Cancel);
            _file = file;
            _cryptoProvider = cryptoProvider;
            _webClient.DownloadProgressChanged += OnDownloadProgressChanged;
            ShutDownAction = ShutdownAndInstall;
            if (!Directory.Exists("Installers"))
                Directory.CreateDirectory("Installers");

        }

        public static void PerformCleanup(IDirectory dir, string path, IFile file)
        {
            try
            {
                foreach(var v in dir.GetFiles(path).Where(a => a.Contains("tmp")))
                    file.Delete(v);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
                //best effort.
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

        static void ShutdownAndInstall(string fileName)
        {
            Application.Current.Shutdown();
            Process.Start(fileName);
        }

        #endregion

   }
}
