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
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Controls;
using Dev2;
using Dev2.Common;
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
        private WebBrowserView mForm;
        private IShellViewModel _shellViewModel;

        public ScriptManager(WebBrowserView form)
        {
            mForm = form;
            _shellViewModel = CustomContainer.Get<IShellViewModel>();
        }

#pragma warning disable CC0091
        public string RetrieveSubscription()
        {
            try
            {
                var result = _shellViewModel?.ActiveServer.ResourceRepository.RetrieveSubscription();
                return result;
            }
            catch(Exception)
            {
                //handle error
                return GlobalConstants.Failed;
            }
        }

#pragma warning disable CC0091
        public string CreateSubscription(string email, string firstName, string lastName, string planId)
        {
            try
            {
                var subscriptionData = new SubscriptionData
                {
                    PlanId = planId,
                    NoOfCores = GetNumberOfCores(),
                    CustomerFirstName = firstName,
                    CustomerLastName = lastName,
                    CustomerEmail = email
                };
                var result = _shellViewModel?.ActiveServer.ResourceRepository.CreateSubscription(subscriptionData);
                //TODO:Handle error message
                return result;
            }
            catch(Exception)
            {
                return GlobalConstants.Failed;
            }
        }

        private int GetNumberOfCores()
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
            _shellViewModel.UpdateStudioLicense();
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