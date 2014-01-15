using System;
using System.IO;
using System.Windows;
using SharpSetup.Base;
using SharpSetup.Prerequisites.Base;

namespace Gui
{
    /// <summary>
    /// Interaction logic for InstallationStep.xaml
    /// </summary>
    public partial class InstallationStep
    {
        readonly InstallationMode mode;
        public InstallationStep(InstallationMode mode)
        {
            InitializeComponent();
            this.mode = mode;
        }

        /// <summary>
        /// Handles the Entered event of the InstallationStep control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SharpSetup.UI.Wpf.Base.ChangeStepRoutedEventArgs"/> instance containing the event data.</param>
        private void InstallationStep_Entered(object sender, SharpSetup.UI.Wpf.Base.ChangeStepRoutedEventArgs e)
        {
            try
            {
                var instLoc = MsiConnection.Instance.GetProperty("INSTALLLOCATION");

                InstallVariables.InstallRoot = instLoc;

                if(mode == InstallationMode.Uninstall)
                {
                    MsiConnection.Instance.Uninstall();

                    if(File.Exists(Properties.Resources.MainMsiFile))
                        MsiConnection.Instance.Open(Properties.Resources.MainMsiFile, true);

                    CleanUp(instLoc);

                    // remove the studio directory ;)
                    var studioDir = instLoc + "/Studio";
                    TerminateFiles(studioDir);

                    // remove the workspace directory ;)
                    var workspaceDir = instLoc + "/Server/Workspaces";
                    TerminateFiles(workspaceDir);

                    // remove the Studio Server directory ;)
                    var studioServerDir = instLoc + "/Server/Studio Server";
                    TerminateFiles(studioServerDir);

                    // clean up the ssl generation stuff ;)
                    var SSLGeneration = instLoc + "/Server/SSL Generation";
                    TerminateFiles(SSLGeneration);

                    // remove the Instance Store directory ;)
                    var instDir = instLoc + "/Server/InstanceStore";
                    TerminateFiles(instDir);

                }
                else if(mode == InstallationMode.Install)
                {
                    PrerequisiteManager.Instance.Install();

                    try
                    {
                        CleanUp(instLoc);
                    }
                    catch(Exception e1)
                    {
                        MessageBox.Show("Installation failed: " + e1.Message);
                    }

                    // set the shortcut variable ;)
                    MsiConnection.Instance.SetProperty("INSTALLSHORTCUT", InstallVariables.InstallShortcuts ? "1" : "0");

                    // start the install process ;)
                    MsiConnection.Instance.Install();

                }
                else
                {
                    MessageBox.Show("Unknown mode");
                }
            }
            catch(MsiException mex)
            {
                if(mex.ErrorCode != (uint)InstallError.UserExit)
                    MessageBox.Show("Installation failed: " + mex.Message);
                Wizard.Finish();
            }

            Wizard.NextStep();
        }


        /// <summary>
        /// Terminates the files as per the uninstaller ;)
        /// </summary>
        /// <param name="loc">The loc.</param>
        private void TerminateFiles(string loc)
        {
            try
            {
                Directory.Delete(loc, true);
            }
            catch
            {
                // Best Effort ;)
            }
        }


        /// <summary>
        /// Cleans up what the installer needs to be removed ;)
        /// </summary>
        /// <param name="baseLoc">The base loc.</param>
        private void CleanUp(string baseLoc)
        {
            // force a clean up of server files ;)
            ProcessServerDirectory(baseLoc);

            // force a clean up of studio files ;)
            ProcessStudioDirectory(baseLoc);
        }

        private void ProcessStudioDirectory(string baseLoc)
        {
            var dir = baseLoc + "/Studio/";

            if(Directory.Exists(dir))
            {

                var files = Directory.GetFiles(dir);

                foreach(var file in files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // best effort ;)
                    }
                }
            }
        }

        /// <summary>
        /// Processes the server directory.
        /// </summary>
        /// <param name="baseLoc">The base loc.</param>
        private void ProcessServerDirectory(string baseLoc)
        {
            var dir = baseLoc + "/Server/";

            if(Directory.Exists(dir))
            {

                var files = Directory.GetFiles(dir);

                foreach(var file in files)
                {
                    // avoid removing config files ;)
                    if(CanDelete(file.ToLower()))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch
                        {
                            // best effort ;)
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether this instance can delete the specified file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified file; otherwise, <c>false</c>.
        /// </returns>
        private bool CanDelete(string file)
        {
            if(file.IndexOf("secureconfig", StringComparison.Ordinal) < 0
                && file.IndexOf("lifecycle", StringComparison.Ordinal) < 0
                && file.IndexOf("serverlog", StringComparison.Ordinal) < 0
                && !IsSSLCert(file))
            {
                return true;
            }

            if(file.IndexOf("runtime.config", StringComparison.Ordinal) > 0)
            {
                return true;
            }

            return false;

        }

        /// <summary>
        /// Determines whether file might be ssl cert.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        private bool IsSSLCert(string file)
        {
            if(file.IndexOf(".cer", StringComparison.Ordinal) >= 0
                && file.IndexOf(".crt", StringComparison.Ordinal) >= 0
                && file.IndexOf(".der", StringComparison.Ordinal) >= 0
                && file.IndexOf(".csr", StringComparison.Ordinal) >= 0
                && file.IndexOf(".pfx", StringComparison.Ordinal) >= 0
                && file.IndexOf(".p12", StringComparison.Ordinal) >= 0
                && file.IndexOf(".key", StringComparison.Ordinal) >= 0)
            {
                return true;
            }

            return false;
        }

    }
}
