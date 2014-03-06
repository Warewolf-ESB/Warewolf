using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Interop;
using Infragistics.Windows.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using Infragistics.Collections;

namespace Infragistics.Windows.DockManager
{
    // AS 3/30/09 TFS16355 - WinForms Interop
    // Moved from within the ContentPane to a non-nested class and changed 
    // from a hard reference to ContentPane to an interface. I also moved the
    // HwndHost attached property from ContentPane to here.
    //
    // AS 10/13/08 TFS6032
    // Helper class to maintain a list of the hwndhosts within the pane. We 
    // can also use this to get around a bug in the hwndhost whereby they 
    // delay hiding the window when the isvisible changes to false. A subsequent
    // reparenting of the WindowsFormsHost before the window is actually hidden 
    // causes a flicker as the Control repositions itself.
    //
    #region HwndHostInfo class
    internal class HwndHostInfo
    {
        #region Member Variables

        private IHwndHostInfoOwner _owner;
        private WeakList<HwndHost> _hosts;

        private static bool _canHideAll = true;

        // AS 11/25/08 TFS8265
        private static bool _canCheckHasFocus = true;

        // AS 3/30/09 TFS16355 - WinForms Interop
        internal static readonly Type WebBrowserType;
        private WeakList<IHwndHostContainer> _containers;
        private bool? _lastHasHwndHostResult;

        #endregion //Member Variables

        #region Constructor
        static HwndHostInfo()
        {
            // AS 3/30/09 TFS16355 - WinForms Interop
            WebBrowserType = Type.GetType("System.Windows.Controls.WebBrowser, " + typeof(Control).Assembly.FullName);
        }

        internal HwndHostInfo(IHwndHostInfoOwner owner)
        {
            this._owner = owner;
            this._hosts = new WeakList<HwndHost>();
        }
        #endregion //Constructor

        #region Properties

        #region HasHwndHost
        internal bool HasHwndHost
        {
            // AS 3/30/09 TFS16355 - WinForms Interop
            //get { return this._hosts.Count > 0; }
            get
            {
                bool hasHwndHost = false;

                if (this._hosts.Count > 0)
                    hasHwndHost = true;
                else if (null != _containers)
                {
                    foreach (IHwndHostContainer container in _containers)
                    {
                        if (container.HasHwndHost)
                        {
                            hasHwndHost = true;
                            break;
                        }
                    }
                }

                _lastHasHwndHostResult = hasHwndHost;
                return hasHwndHost;
            }
        }

        #endregion //HasHwndHost

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region HasWebBrowser
        internal bool HasWebBrowser
        {
            get
            {
                if (WebBrowserType != null)
                {
                    foreach (HwndHost host in GetHosts())
                    {
                        if (IsWebBrowser(host))
                            return true;
                    }
                }

                return false;
            }
        }
        #endregion //HasWebBrowser

        // AS 10/13/08 TFS6032
        // We need to maintain a list of the hwndhosts within the pane. When we have one 
        // or more then the content needs to be hidden when the pane is unpinned and not 
        // displayed within the flyout.
        //
        #region HwndHost
        internal static readonly DependencyProperty HwndHostProperty
            = DependencyProperty.RegisterAttached("HwndHost", typeof(HwndHostInfo), typeof(HwndHostInfo),
                new FrameworkPropertyMetadata((HwndHostInfo)null,
                    FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior | // AS 11/4/08 TFS9618
                    FrameworkPropertyMetadataOptions.Inherits,
                    new PropertyChangedCallback(OnHwndHostChanged)));

        internal static HwndHostInfo GetHwndHost(DependencyObject d)
        {
            return (HwndHostInfo)d.GetValue(HwndHostProperty);
        }

        internal static void SetHwndHost(DependencyObject d, HwndHostInfo value)
        {
            d.SetValue(HwndHostProperty, value);
        }

        /// <summary>
        /// Handles changes to the HwndHost property.
        /// </summary>
        private static void OnHwndHostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HwndHost host = d as HwndHost;

            if (null != host)
            {
                HwndHostInfo oldInfo = e.OldValue as HwndHostInfo;
                HwndHostInfo newInfo = e.NewValue as HwndHostInfo;

                if (null != oldInfo)
                    oldInfo.Remove(host);

                if (null != newInfo)
                    newInfo.Add(host);
            }

            // AS 3/30/09 TFS16355 - WinForms Interop
            IHwndHostContainer container = d as IHwndHostContainer;

            if (null != container)
            {
                Debug.Assert(host == null);

                HwndHostInfo oldInfo = e.OldValue as HwndHostInfo;
                HwndHostInfo newInfo = e.NewValue as HwndHostInfo;

                if (null != oldInfo)
                    oldInfo.Remove(container);

                if (null != newInfo)
                    newInfo.Add(container);
            }
        }
        #endregion //HwndHost

		// AS 9/8/09 TFS21921
		#region Owner
		internal IHwndHostInfoOwner Owner
		{
			get { return _owner; }
		} 
		#endregion //Owner

        #endregion //Properties

        #region Methods

        #region Add/Remove(HwndHost)
        internal void Add(HwndHost host)
        {
            this._hosts.Add(host);

            // AS 3/30/09 TFS16355 - WinForms Interop
            // If we haven't been asked or we were and told the owner we didn't 
            // have any then tell them we have one now.
            //
            //if (1 == this._hosts.Count)
            if (1 == this._hosts.Count && _lastHasHwndHostResult != true)
            {
                // AS 3/30/09 TFS16355 - WinForms Interop
                //this._pane.VerifyContentVisibility();
                this._owner.OnHasHostsChanged();
            }
        }

        internal void Remove(HwndHost host)
        {
            this._hosts.Remove(host);
            this._hosts.Compact();

            // AS 3/30/09 TFS16355 - WinForms Interop
            // If we're removing the last host and we previously told the owner
            // that we had hwnds and we don't have any more then notify that this 
            // has changed.
            //
            //if (0 == this._hosts.Count)
            if (0 == this._hosts.Count && _lastHasHwndHostResult == true && !this.HasHwndHost)
            {
                // AS 3/30/09 TFS16355 - WinForms Interop
                //this._pane.VerifyContentVisibility();
                this._owner.OnHasHostsChanged();
            }
        }

        #endregion //Add/Remove(HwndHost)

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region Add/Remove(IHwndHostContainer)
        internal void Add(IHwndHostContainer container)
        {
            if (null == _containers)
                _containers = new WeakList<IHwndHostContainer>();

            this._containers.Add(container);

            // if we just added a container and we previously told the owner we
            // didn't have any (or the owner didn't ask yet) and we have one within
            // this container then notify the owner
            if (1 == this._containers.Count && _lastHasHwndHostResult != true && container.HasHwndHost)
            {
                this._owner.OnHasHostsChanged();
            }
        }

        internal void Remove(IHwndHostContainer container)
        {
            Debug.Assert(null != _containers);

            if (null != _containers)
            {
                this._containers.Remove(container);
                this._containers.Compact();

                // if we're removing the last container and the last time we reported
                // that we did have hwndhosts and we don't have any now then notify
                // the owner
                if (0 == this._containers.Count && _lastHasHwndHostResult == true && !this.HasHwndHost)
                {
                    this._owner.OnHasHostsChanged();
                }
            }
        }

        #endregion //Add/Remove(IHwndHostContainer)

        // AS 2/9/09 TFS13375
        #region GetFocusedHwndHost
        internal FrameworkElement GetFocusedHwndHost()
        {
            if (_canCheckHasFocus)
            {
                try
                {
                    
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

                    foreach (HwndHost host in this.GetHosts())
                    {
                        if (((IKeyboardInputSink)host).HasFocusWithin())
                            return host;
                    }
                }
                catch (System.Security.SecurityException)
                {
                    _canCheckHasFocus = false;
                }
            }

            return null;
        }
        #endregion //GetFocusedHwndHost

        #region GetHosts
        internal IEnumerable<HwndHost> GetHosts()
        {
            foreach (HwndHost host in this._hosts)
            {
                if (null != host)
                    yield return host;
            }

            if (null != _containers)
            {
                foreach (IHwndHostContainer container in _containers)
                {
                    foreach (HwndHost host in container.GetHosts())
                    {
                        if (null != host)
                            yield return host;
                    }
                }
            }
        }
        #endregion //GetHosts

        internal void HideAll()
        {
            // AS 3/30/09 TFS16355 - WinForms Interop
            // Take the containers into account.
            //
            //if (false == _canHideAll || this._hosts.Count == 0)
            if (false == _canHideAll)
                return;

            if (_hosts.Count == 0 && (_containers == null || _containers.Count == 0))
                return;

            try
            {
                this.HideAllImpl();
            }
            catch (System.Security.SecurityException)
            {
                _canHideAll = false;
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private void HideAllImpl()
        {
            // AS 3/30/09 TFS16355 - WinForms Interop
            //foreach (HwndHost host in this._hosts)
            foreach(HwndHost host in this.GetHosts())
            {
                if (null == host)
                    continue;

                IntPtr hwnd = host.Handle;

                if (IntPtr.Zero != hwnd)
                    NativeWindowMethods.ShowWindow(hwnd, NativeWindowMethods.SW_HIDE);
            }

            // AS 3/30/09 TFS16355 - WinForms Interop
            // Remove any empty entries.
            //
            this._hosts.Compact();

            // AS 3/30/09 TFS16355 - WinForms Interop
            //if (this._hosts.Count == 0)
            if (this.HasHwndHost == false)
            {
                // AS 3/30/09 TFS16355 - WinForms Interop
                //this._pane.VerifyContentVisibility();
                this._owner.OnHasHostsChanged();
            }
        }

        // AS 11/25/08 TFS8265
        internal bool HasFocusWithin()
        {
            
#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

            return this.GetFocusedHwndHost() != null;
        }

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region IsWebBrowser
        private static bool IsWebBrowser(HwndHost host)
        {
            return host != null && WebBrowserType.IsAssignableFrom(host.GetType());
        }
        #endregion //IsWebBrowser

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region OnHasHwndHostChanged
        internal static void OnHasHwndHostChanged(IHwndHostContainer container)
        {
            DependencyObject d = container as DependencyObject;

            if (null != d)
            {
                HwndHostInfo hhi = GetHwndHost(d);

                if (null != hhi)
                {
                    bool hasHwndHost = container.HasHwndHost;

                    if (hhi._lastHasHwndHostResult != hasHwndHost)
                        hhi._owner.OnHasHostsChanged();
                }
            }
        }
        #endregion //OnHasHwndHostChanged

        #endregion //Methods
    }
    #endregion //HwndHostInfo class

    #region IHwndHostInfoOwner
    internal interface IHwndHostInfoOwner
    {
        void OnHasHostsChanged();
    } 
    #endregion //IHwndHostInfoOwner

    // AS 3/30/09 TFS16355 - WinForms Interop
    // We need a way for the dockmanager to convey that it contains an
    // hwnd host.
    //
    #region IHwndHostContainer
    internal interface IHwndHostContainer
    {
        bool HasHwndHost { get; }

        IEnumerable<HwndHost> GetHosts();
    } 
    #endregion //IHwndHostContainer
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved