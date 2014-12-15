/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// Copyright (C) Josh Smith - July 2008

using System;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using System.Windows.Markup;
using System.Xml;

namespace WPF.JoshSmith.Markup
{
    /// <summary>
    ///     This markup extension conditionally instantiates the XAML you pass it
    ///     if and only if the application is running in full-trust.
    /// </summary>
    /// <remarks>
    ///     Documentation:
    ///     http://joshsmithonwpf.wordpress.com/2008/06/12/writing-xaml-that-gracefully-degrades-in-partial-trust-scenarios/
    /// </remarks>
    [ContentProperty("Xaml")]
    public class IfFullTrustExtension : MarkupExtension
    {
        private static readonly bool FullTrust;

        static IfFullTrustExtension()
        {
            try
            {
                const PermissionState state = PermissionState.Unrestricted;
                new UIPermission(state).Assert();
                FullTrust = true;
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch
            {
            }
            // ReSharper restore EmptyGeneralCatchClause
        }

        /// <summary>
        ///     The XAML that should be turned into live objects
        ///     if running with full-trust from the CLR.
        /// </summary>
        public string Xaml { get; set; }

        /// <summary>
        ///     Returns the objects declared by the Xaml property
        ///     or null, if running in partial-trust.
        /// </summary>
        public override object ProvideValue(IServiceProvider sp)
        {
            object value = null;
            if (FullTrust)
            {
                try
                {
                    using (var str = new StringReader(Xaml))
                    using (XmlReader xml = XmlReader.Create(str))
                        value = XamlReader.Load(xml);
                }
                catch (Exception ex)
                {
                    Debug.Fail("Invalid XAML.\r\n" + ex);
                }
            }
            return value;
        }
    }
}