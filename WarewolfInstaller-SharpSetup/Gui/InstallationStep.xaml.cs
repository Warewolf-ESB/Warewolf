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
        InstallationMode mode;
        public InstallationStep(InstallationMode mode)
        {
            InitializeComponent();
            this.mode = mode;
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
        /// Determines whether this instance can delete the specified file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified file; otherwise, <c>false</c>.
        /// </returns>
        private bool CanDelete(string file)
        {
            if (file.IndexOf("config", StringComparison.Ordinal) < 0
                && file.IndexOf("lifecycle", StringComparison.Ordinal) < 0
                && file.IndexOf("serverlog", StringComparison.Ordinal) < 0)
            {
                return true;
            }

            if (file.IndexOf("runtime.config") > 0)
            {
                return true;
            }

            return false;

        }

        /// <summary>
        /// Cleans up what the installer needs to be removed ;)
        /// </summary>
        /// <param name="baseLoc">The base loc.</param>
        private void CleanUp(string baseLoc)
        {
            // force a clean up of server files ;)
            {

                var dir = baseLoc + "/Server/";

                if (Directory.Exists(dir))
                {

                    var files = Directory.GetFiles(dir);

                    foreach (var file in files)
                    {
                        // avoid removing config files ;)
                        if (CanDelete(file.ToLower()))
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

            // force a clean up of studio files ;)
            {
                var dir = baseLoc + "/Studio/";

                if (Directory.Exists(dir))
                {

                    var files = Directory.GetFiles(dir);

                    foreach (var file in files)
                    {
                        // avoid removing config files ;)
                        if (file.ToLower().IndexOf("config", System.StringComparison.Ordinal) < 0)
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
        }

        private void InstallationStep_Entered(object sender, SharpSetup.UI.Wpf.Base.ChangeStepRoutedEventArgs e)
        {
            try
            {
                var instLoc = MsiConnection.Instance.GetProperty("INSTALLLOCATION");

                if (mode == InstallationMode.Uninstall)
                {
                    MsiConnection.Instance.Uninstall();
                    
                    if (File.Exists(Properties.Resources.MainMsiFile))
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

                    // remove the Instance Store directory ;)
                    var instDir = instLoc + "/Server/InstanceStore";
                    TerminateFiles(instDir);

                }
                else if (mode == InstallationMode.Install)
                {
                    PrerequisiteManager.Instance.Install();

                    try
                    {
                        InstallVariables.InstallRoot = MsiConnection.Instance.GetProperty("INSTALLLOCATION");
                    }
                    catch
                    {
                        // Best effort to fetch product code, if not present we have big issues ;(
                        MessageBox.Show("Cannot locate product code to continue install.");
                        Wizard.Finish();
                    }

                    try
                    {
                        CleanUp(instLoc);
                    }
                    catch (Exception e1)
                    {
                        MessageBox.Show("Installation failed: " + e1.Message);
                    }

                    // start the install process ;)
                    MsiConnection.Instance.Install();

                }
                else
                {
                    MessageBox.Show("Unknown mode");
                }
            }
            catch (MsiException mex)
            {
                if (mex.ErrorCode != (uint)InstallError.UserExit)
                    MessageBox.Show("Installation failed: " + mex.Message);
                Wizard.Finish();
            }

            Wizard.NextStep();
        }
    }
}
