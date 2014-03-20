using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using Gui.Utility;
using Ionic.Zip;
using NetFwTypeLib;
using SharpSetup.Base;
using SharpSetup.UI.Wpf.Base;
using Path = System.IO.Path;

namespace Gui
{
    /// <summary>
    /// Interaction logic for PreInstallProcess.xaml
    /// </summary>
    public partial class PostInstallProcess
    {

        private bool _serviceInstalled;
        private bool _serviceInstallException;

        public PostInstallProcess(int stepNumber, List<string> listOfStepNames)
        {
            InitializeComponent();
            DataContext = new InfoStepDataContext(stepNumber, listOfStepNames);
        }

        /// <summary>
        /// Performs the custom operation when installing ;)
        /// 
        /// Change Log : 
        /// + Release 0.2.13.1 - Swap old secure config name to new
        /// + Release 0.4.1.2 - Install Examples
        /// 
        /// </summary>
        private void CustomOperation()
        {
            SwapConfigs();
            InstallExamples();
        }

        private void InstallExamples()
        {
            var stream = ResourceExtractor.Fetch("Samples.zip");

            if(stream == null)
            {
                return;
            }

            using(stream)
            {
                // Extract zip and unpack ;)
                using(var zp = ZipFile.Read(stream))
                {
                    foreach(var ze in zp)
                    {
                        ze.Extract(InstallVariables.InstallRoot, ExtractExistingFileAction.OverwriteSilently);
                    }
                }
            }
        }

        private void SwapConfigs()
        {
            // Swap config files around
            var oldConfig = InstallVariables.InstallRoot + @"Server\Dev2.Server.exe.secureconfig";
            var newConfig = InstallVariables.InstallRoot + @"Server\Warewolf Server.exe.secureconfig";

            try
            {
                if(File.Exists(oldConfig))
                {
                    if(File.Exists(newConfig))
                    {
                        var newLoc = newConfig + ".new";
                        File.Move(newConfig, newLoc);
                    }

                    File.Move(oldConfig, newConfig);
                }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
                // Just making sure ;)
            }
        }

        /// <summary>
        /// Handles the OnClick event of the BtnRerun control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnRerun_OnClick(object sender, RoutedEventArgs e)
        {
            PostInstallStep_Entered(sender, null);
        }

        /// <summary>
        /// Sets the success message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        private void SetSuccessMessasge(string msg)
        {
            PostInstallMsg.Text = msg;
            postInstallStatusImg.Visibility = Visibility.Visible;
            postInstallStatusImg.Source =
                new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/Images/tick.png",
                                        UriKind.RelativeOrAbsolute));
            postInstallStatusCircularProgressBar.Visibility = Visibility.Hidden;
            CanGoNext = true;
            btnRerun.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Shows the cancel error.
        /// </summary>
        private void ShowCancelError()
        {
            MessageBox.Show("Failed to rollback installer progress", "Error");
        }

        /// <summary>
        /// Sets the cleanup message.
        /// </summary>
        private void SetCleanupMessage()
        {
            PostInstallMsg.Text = InstallVariables.RollbackMessage;
            postInstallStatusImg.Visibility = Visibility.Collapsed;
            postInstallStatusCircularProgressBar.Visibility = Visibility.Visible;
            CanGoNext = false;
            btnRerun.Visibility = Visibility.Hidden;
        }


        /// <summary>
        /// Sets the failure message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        private void SetFailureMessage(string msg)
        {
            PostInstallMsg.Text = msg;
            postInstallStatusImg.Source =
                new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/Images/cross.png",
                                        UriKind.RelativeOrAbsolute));
            postInstallStatusImg.Visibility = Visibility.Visible;
            postInstallStatusCircularProgressBar.Visibility = Visibility.Collapsed;
            CanGoNext = false;
            btnRerun.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Starts the service.
        /// </summary>
        private void StartService(string serverInstallLocation)
        {
            try
            {
                ServiceController sc = new ServiceController(InstallVariables.ServerService);

                ProcessStartInfo psi = new ProcessStartInfo { FileName = serverInstallLocation, Arguments = "-i", WindowStyle = ProcessWindowStyle.Hidden, UseShellExecute = true };

                Process p = Process.Start(psi);

                p.WaitForExit(InstallVariables.DefaultWaitInSeconds);

                // now try and start the service ;)
                if(sc.Status == ServiceControllerStatus.Stopped)
                {
                    sc.Start();
                    // wait start ;)
                    sc.WaitForStatus(ServiceControllerStatus.Running,
                                     TimeSpan.FromSeconds(InstallVariables.DefaultWaitInSeconds));

                    if(sc.Status == ServiceControllerStatus.Running)
                    {
                        _serviceInstalled = true;
                    }
                    else
                    {
                        // wait a bit more ;)

                        sc.WaitForStatus(ServiceControllerStatus.Running,
                                     TimeSpan.FromSeconds(InstallVariables.DefaultWaitInSeconds));

                        if(sc.Status == ServiceControllerStatus.Running)
                        {
                            _serviceInstalled = true;
                        }
                    }
                }
                else if(sc.Status == ServiceControllerStatus.Running)
                {
                    _serviceInstalled = true;
                }
                else
                {
                    // wait some more ;)
                    sc.WaitForStatus(ServiceControllerStatus.Running,
                                     TimeSpan.FromSeconds(InstallVariables.DefaultWaitInSeconds));

                    if(sc.Status == ServiceControllerStatus.Running)
                    {
                        _serviceInstalled = true;
                    }
                }

                sc.Dispose();
            }
            catch(Exception)
            {
                try
                {
                    ServiceController sc = new ServiceController(InstallVariables.ServerService);
                    // maybe it is already installed, just try and start it ;)
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running,
                                     TimeSpan.FromSeconds(InstallVariables.DefaultWaitInSeconds));
                    if(sc.Status == ServiceControllerStatus.Running)
                    {
                        _serviceInstalled = true;
                    }
                    else
                    {
                        _serviceInstallException = true;
                    }
                    sc.Dispose();
                }
                catch
                {
                    _serviceInstallException = true;

                }
            }
        }

        /// <summary>
        /// Installs the service.
        /// </summary>
        /// <param name="installRoot">The install root.</param>
        private void InstallService(string installRoot)
        {
            // Gain access to warewolf exe location ;)
            var serverInstallLocation = Path.Combine(installRoot, "Server", InstallVariables.ServerService + ".exe");

            // TODO : Remove after r 0.2.13.1
            try
            {
                // Perform any post install custom operation ;)
                CustomOperation();
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch(Exception) { }
            // ReSharper restore EmptyGeneralCatchClause

            // Start the service
            StartService(serverInstallLocation);

            // clean up any log files and junk ;)
            var serverRoot = Path.Combine(installRoot, "Server");
            CleanupOperation(serverRoot);

        }

        private static string outputData = string.Empty;
        private bool VerifyPortsAreOpen()
        {
            // WindowStyle = ProcessWindowStyle.Hidden 
            Process p = new Process { StartInfo = { FileName = @"C:\Windows\system32\netsh.exe", Arguments = "http show urlacl", RedirectStandardOutput = true, UseShellExecute = false, WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true } };
            p.OutputDataReceived += OutputHandler;
            p.Start();
            p.BeginOutputReadLine();

            p.WaitForExit(30);

            // Now scan outputData for magical strings ;)
            if(outputData.IndexOf("http://*:3142/", StringComparison.Ordinal) >= 0)
            {
                if(outputData.IndexOf("https://*:3143/", StringComparison.Ordinal) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            // Collect the sort command output. 
            var data = outLine.Data;
            if(!String.IsNullOrEmpty(data))
            {
                outputData += data;
            }
        }

        /// <summary>
        /// Cleanups the operation.
        /// </summary>
        private void CleanupOperation(string installLocation)
        {
            // two install log files
            var path = Path.Combine(installLocation, "Warewolf Server.InstallLog");
            var path2 = Path.Combine(installLocation, "Warewolf Server.InstallState");

            var paths = new[] { path, path2 };

            foreach(var p in paths)
            {
                try
                {
                    File.Delete(p);
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch { }
                // ReSharper restore EmptyGeneralCatchClause
            }

        }

        private void AddFirewallRules()
        {
            AddInboundRule();
            AddOutboundRule();
        }

        private void AddOutboundRule()
        {
            if(!DoesRuleExist(InstallVariables.OutboundHTTPWarewolfRule))
            {
                INetFwRule rule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
                rule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                rule.Description = "Warewolf HTTP/HTTPS OUT";
                rule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
                rule.Enabled = true;
                rule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
                rule.LocalPorts = "3142";
                rule.RemotePorts = "*";
                rule.InterfaceTypes = "All";
                rule.Name = InstallVariables.OutboundHTTPWarewolfRule;
                // Add it 
                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
                Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                firewallPolicy.Rules.Add(rule);
            }

            if(!DoesRuleExist(InstallVariables.OutboundHTTPSWarewolfRule))
            {
                INetFwRule rule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
                rule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                rule.Description = "Warewolf HTTP/HTTPS OUT";
                rule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
                rule.Enabled = true;
                rule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
                rule.LocalPorts = "3143";
                rule.RemotePorts = "*";
                rule.InterfaceTypes = "All";
                rule.Name = InstallVariables.OutboundHTTPSWarewolfRule;
                // Add it 
                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
                Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                firewallPolicy.Rules.Add(rule);
            }
        }

        private void AddInboundRule()
        {
            if(!DoesRuleExist(InstallVariables.InboundHTTPWarewolfRule))
            {
                INetFwRule rule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
                rule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                rule.Description = "Warewolf HTTP/HTTPS IN";
                rule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
                rule.Enabled = true;
                rule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
                rule.LocalPorts = "3142";
                rule.RemotePorts = "*";
                rule.InterfaceTypes = "All";
                rule.Name = InstallVariables.InboundHTTPWarewolfRule;
                // Add it
                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
                Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                firewallPolicy.Rules.Add(rule);
            }

            if(!DoesRuleExist(InstallVariables.InboundHTTPSWarewolfRule))
            {
                INetFwRule rule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
                rule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                rule.Description = "Warewolf HTTP/HTTPS IN";
                rule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
                rule.Enabled = true;
                rule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
                rule.LocalPorts = "3143";
                rule.RemotePorts = "*";
                rule.InterfaceTypes = "All";
                rule.Name = InstallVariables.InboundHTTPSWarewolfRule;
                // Add it
                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
                Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                firewallPolicy.Rules.Add(rule);
            }
        }

        private bool DoesRuleExist(string ruleName)
        {
            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
            Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

            // iterate and find matching rule name ;)
            foreach(INetFwRule rule in firewallPolicy.Rules)
            {
                if(rule.Name == ruleName)
                {
                    return true;
                }
            }

            return false;
        }

        private void AddHttpRules()
        {
            // NOTE Use : netsh.exe http show urlacl - To view urlacl rules ;)

            var args = new[] { @"http add urlacl url=http://*:3142/  user=\Everyone", @"http add urlacl url=https://*:3143/ user=\Everyone" };

            //var args = string.Format("http add urlacl url={0}/ user=\\Everyone", url);
            try
            {
                foreach(var arg in args)
                {
                    bool invoke = ProcessHost.Invoke(null, @"C:\Windows\system32\netsh.exe", arg);
                    if(!invoke)
                    {
                        SetFailureMessage(string.Format("There was an error adding url: {0}", arg));
                    }
                }

            }
            catch(Exception e)
            {
                SetFailureMessage(e.Message);
            }
        }


        /// <summary>
        /// Handles the Entered event of the PostInstallStep control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SharpSetup.UI.Wpf.Base.ChangeStepRoutedEventArgs"/> instance containing the event data.</param>
        private void PostInstallStep_Entered(object sender, ChangeStepRoutedEventArgs e)
        {

            CanGoNext = false;
            postInstallStatusImg.Visibility = Visibility.Collapsed;
            postInstallStatusCircularProgressBar.Visibility = Visibility.Visible;
            btnRerun.Visibility = Visibility.Hidden;
            // Setup a cancel action ;)
            Cancel += delegate
            {
                SetCleanupMessage();
                List<string> listOfStepNames = new List<string> { "License Agreement", "Pre UnInstall", "UnInstall", "Installation", "Post Install", "Finish" };
                var trans = new PreUnInstallProcess(2, listOfStepNames);

                if(!trans.Rollback())
                {
                    ShowCancelError();
                }
                else
                {
                    // Now uninstall?!
                    MsiConnection.Instance.Uninstall();
                    SetSuccessMessasge("Rollback complete");
                }
            };

            // attempts to install service ;)
            if(!string.IsNullOrEmpty(InstallVariables.InstallRoot))
            {

                // Get the BackgroundWorker that raised this event.
                BackgroundWorker worker = new BackgroundWorker();
                bool portAreOpen = false;
                bool isFirewallOpen = false;
                // ReSharper disable ConvertToConstant.Local

                // ReSharper restore ConvertToConstant.Local
                worker.DoWork += delegate
                {
                    // start service up ;)
                    InstallService(InstallVariables.InstallRoot);

                    // Open the required ports ;)
                    AddHttpRules();

                    var cnt = 0;
                    portAreOpen = VerifyPortsAreOpen();
                    while(!portAreOpen && cnt < 5)
                    {
                        Thread.Sleep(3500);
                        // try a second time
                        portAreOpen = VerifyPortsAreOpen();
                        cnt++;
                    }

                    try
                    {
                        AddFirewallRules();
                        isFirewallOpen = true;
                    }
                    // ReSharper disable EmptyGeneralCatchClause
                    catch
                    // ReSharper restore EmptyGeneralCatchClause
                    {
                        // do nothing
                    }

                };

                worker.RunWorkerCompleted += delegate
                {

                    if(_serviceInstalled && !_serviceInstallException)
                    {
                        // verify ports opened
                        if(!isFirewallOpen)
                        {
                            SetFailureMessage("Failed to open Firewall port 3142 and 3143");
                        }
                        else
                        {
                            if(!portAreOpen)
                            {
                                SetFailureMessage("Failed to allow local HTTP and HTTPS traffic on ports 3142 and 3143");
                            }

                            SetSuccessMessasge("Started server service");
                        }
                    }
                    else if(!_serviceInstalled && _serviceInstallException)
                    {
                        SetFailureMessage("Cannot install server as service");
                    }
                    else if(!_serviceInstalled && !_serviceInstallException)
                    {
                        SetFailureMessage("Cannot start server service");
                    }

                    InstallVariables.IsInstallMode = true;
                };

                try
                {
                    worker.RunWorkerAsync();
                }
                catch(Exception e1)
                {
                    MessageBox.Show(e1.Message);
                }
            }
            else
            {
                SetFailureMessage("Installer cannot resolve server install location");
            }
        }
    }
}
