/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Controls;
using ChargeBee.Api;
using ChargeBee.Models;
using Dev2;
using Dev2.Common;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Studio.Interfaces;
using Warewolf.Licensing;

// ReSharper disable CC0091
// ReSharper disable InconsistentNaming

namespace Warewolf.Studio.CustomControls
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [ComVisible(true)]
    public class ScriptManager
    {
        WebBrowserView mForm;

        public ScriptManager(WebBrowserView form)
        {
            mForm = form;
        }

        static IServer GetEnvironment()
        {
            var serverRepository = CustomContainer.Get<IServerRepository>();
            var server = serverRepository.ActiveServer;
            if(server == null)
            {
                var shellViewModel = CustomContainer.Get<IShellViewModel>();
                server = shellViewModel?.ActiveServer;
            }

            if(server != null && server.Permissions == null)
            {
                server.Permissions = new List<IWindowsGroupPermission>();
                if(server.AuthorizationService?.SecurityService != null)
                {
                    server.Permissions.AddRange(server.AuthorizationService.SecurityService.Permissions);
                }
            }

            return server;
        }

#pragma warning disable CC0091
        public string RetriveSubscription()
        {
            var serializer = new Dev2JsonSerializer();
            var controller = new CommunicationController { ServiceName = nameof(GetLicenseKey) };
            var server = GetEnvironment();
            var resultData = controller.ExecuteCommand<ExecuteMessage>(server.Connection, GlobalConstants.ServerWorkspaceID);
            var data = serializer.Deserialize<ILicenseData>(resultData.Message);
            return serializer.Serialize(data);
        }

#pragma warning disable CC0091
        public string CheckoutNew(string email, string name, string surname, string plan)
        {
            try
            {
                //TODO: The calls to chargebee must be moved into the usage.dll
                ApiConfig.Configure("warewolf-test", "test_VMxitsiobdAyth62k0DiqpAUKocG6sV3");
                var customer = Customer.Create()
                    .FirstName(name)
                    .LastName(surname)
                    .Email(email)
                    .Request();
                var subscription = Subscription.CreateForCustomer(customer.Customer.Id)
                    .PlanId(plan)
                    .PlanQuantity(GetNumberOfCores())
                    .Request();

                //TODO: use ILicenseData
                GlobalConstants.LicenseCustomerId = customer.Customer.Id;
                GlobalConstants.LicensePlanId = subscription.Subscription.PlanId;
                GlobalConstants.LicenseSubscriptionId = subscription.Subscription.Id;
                if(subscription.Subscription.Status.ToString() == "InTrial" && subscription.Subscription.Status.ToString() == "Active")
                {
                    GlobalConstants.IsLicensed = true;
                    //TODO: Call SaveLicenseKey Service to save to the secure.config
                    // var controller = new CommunicationController { ServiceName = nameof(SaveLicenseKey) };
                    // var server = GetEnvironment();
                    //  var resultData =controller.ExecuteCommand<ILicenseData>(server.Connection, GlobalConstants.ServerWorkspaceID);
                }

                return "success";
            }
            catch(Exception)
            {
                GlobalConstants.IsLicensed = false;
                return "failed";
            }
        }

        int GetNumberOfCores()
        {
            var coreCount = 0;
            foreach(var item in new ManagementObjectSearcher("Select * from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }

            return coreCount;
        }

        public void CloseBrowser()
        {
            //TODO: Refresh studio to enable save button
            mForm.Close();
        }

        public static void SetSilent(WebBrowser browser, bool silent)
        {
            if(browser is null)
            {
                throw new ArgumentNullException(nameof(browser));
            }

            // get an IWebBrowser2 from the document
            if(browser.Document is IOleServiceProvider sp)
            {
                var iidIWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                var iidIWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                sp.QueryService(ref iidIWebBrowserApp, ref iidIWebBrowser2, out var webBrowser);
                webBrowser?.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
            }
        }

        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }
    }
}