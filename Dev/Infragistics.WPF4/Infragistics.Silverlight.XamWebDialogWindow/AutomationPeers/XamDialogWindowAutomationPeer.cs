using System;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

using Infragistics.Controls.Interactions;

namespace Infragistics.AutomationPeers
{
    /// <summary>
    /// Exposes <see cref="XamDialogWindow" /> types to UI
    /// automation.
    /// </summary>
    public class XamDialogWindowAutomationPeer : FrameworkElementAutomationPeer,
    IWindowProvider, ITransformProvider
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="XamDialogWindowAutomationPeer"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public XamDialogWindowAutomationPeer(XamDialogWindow owner)
            : base(owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            this.OwningDialogWindow.IsActiveChanged += OwningDialogWindow_IsActiveChanged;
            this.OwningDialogWindow.PropertyChanged += OwningDialogWindow_PropertyChanged;
            this.OwningDialogWindow.WindowStateChanged += OwningDialogWindow_WindowStateChanged;
        }

        #endregion Constructors

        #region Properties

        private XamDialogWindow OwningDialogWindow
        {
            get
            {
                return (XamDialogWindow)Owner;
            }
        }
        #endregion Properties

        #region Methods

        #region Static

        #region ToWindowVisualState


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal static WindowVisualState ToWindowVisualState(WindowState state)
        {
            WindowVisualState windowState;

            switch (state)
            {
                case WindowState.Maximized:
                    windowState = WindowVisualState.Maximized;
                    break;
                case WindowState.Minimized:
                    windowState = WindowVisualState.Minimized;
                    break;
                default:
                    windowState = WindowVisualState.Normal;
                    break;
            }

            return windowState;
        }
        #endregion //ToWindowVisualState

        #endregion //Static

        #region Internal

        #region RaiseCanMaximizePropertyChangedEvent


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal void RaiseCanMaximizePropertyChangedEvent(bool oldValue, bool newValue)
        {
            if (oldValue != newValue && ListenerExists(AutomationEvents.PropertyChanged))
            {
                this.RaisePropertyChangedEvent(WindowPatternIdentifiers.CanMaximizeProperty, oldValue, newValue);
            }
        }
        #endregion //RaiseCanMaximizePropertyChangedEvent

        #region RaiseCanMinimizePropertyChangedEvent


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal void RaiseCanMinimizePropertyChangedEvent(bool oldValue, bool newValue)
        {
            if (oldValue != newValue && ListenerExists(AutomationEvents.PropertyChanged))
            {
                this.RaisePropertyChangedEvent(WindowPatternIdentifiers.CanMinimizeProperty, oldValue, newValue);
            }
        }
        #endregion //RaiseCanMinimizePropertyChangedEvent

        #region RaiseCanMovePropertyChangedEvent


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal void RaiseCanMovePropertyChangedEvent(bool oldValue, bool newValue)
        {
            if (oldValue != newValue && ListenerExists(AutomationEvents.PropertyChanged))
            {
                this.RaisePropertyChangedEvent(TransformPatternIdentifiers.CanMoveProperty, oldValue, newValue);
            }
        }
        #endregion //RaiseCanMovePropertyChangedEvent

        #region RaiseCanResizePropertyChangedEvent


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal void RaiseCanResizePropertyChangedEvent(bool oldValue, bool newValue)
        {
            if (oldValue != newValue && ListenerExists(AutomationEvents.PropertyChanged))
            {
                this.RaisePropertyChangedEvent(TransformPatternIdentifiers.CanResizeProperty, oldValue, newValue);
            }
        }
        #endregion //RaiseCanResizePropertyChangedEvent

        #region RaiseIsTopmostPropertyChangedEvent


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal void RaiseIsTopmostPropertyChangedEvent(bool oldValue, bool newValue)
        {
            if (oldValue != newValue && ListenerExists(AutomationEvents.PropertyChanged))
            {
                this.RaisePropertyChangedEvent(WindowPatternIdentifiers.IsTopmostProperty, oldValue, newValue);
            }
        }
        #endregion //RaiseIsTopmostPropertyChangedEvent

        #region RaiseIsModalPropertyChangedEvent


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal void RaiseIsModalPropertyChangedEvent(bool oldValue, bool newValue)
        {
            if (oldValue != newValue && ListenerExists(AutomationEvents.PropertyChanged))
            {
                this.RaisePropertyChangedEvent(WindowPatternIdentifiers.IsModalProperty, oldValue, newValue);
            }
        }
        #endregion //RaiseIsModalPropertyChangedEvent

        #region RaiseWindowVisualStatePropertyChangedEvent
        /// <summary>
        /// Raises the window VisualStatePropertyChangedEvent.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        internal void RaiseWindowVisualStatePropertyChangedEvent(WindowVisualState oldValue, WindowVisualState newValue)
        {
            if (oldValue != newValue && ListenerExists(AutomationEvents.PropertyChanged))
            {
                this.RaisePropertyChangedEvent(WindowPatternIdentifiers.WindowVisualStateProperty, oldValue, newValue);
            }
        }
        #endregion //RaiseWindowVisualStatePropertyChangedEvent

        #endregion //Internal

        #endregion //Methods

        #region AutomationPeer overrides

        /// <summary>
        /// Returns the control type for the <see cref="T:System.Windows.UIElement"/> that is associated with this <see cref="T:System.Windows.Automation.Peers.FrameworkElementAutomationPeer"/>. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType"/>.
        /// </summary>
        /// <returns>A value of the enumeration.</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Window;
        }

        /// <summary>
        /// Returns the control pattern for the <see cref="T:System.Windows.UIElement"/> that is associated with this <see cref="T:System.Windows.Automation.Peers.FrameworkElementAutomationPeer"/>.
        /// </summary>
        /// <param name="patternInterface">One of the enumeration values.</param>
        /// <returns>See Remarks.</returns>
        public override object GetPattern(PatternInterface patternInterface)
        {
            if ((patternInterface == PatternInterface.Window) || (patternInterface == PatternInterface.Transform))
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        /// <summary>
        /// Returns the name of the <see cref="T:System.Windows.UIElement"/> that is associated with this <see cref="T:System.Windows.Automation.Peers.FrameworkElementAutomationPeer"/>. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName"/>.
        /// </summary>
        /// <returns>
        /// The name of the owner type that is associated with this <see cref="T:System.Windows.Automation.Peers.FrameworkElementAutomationPeer"/>. See Remarks.
        /// </returns>
        protected override string GetClassNameCore()
        {
            return this.Owner.GetType().Name;
        }

        /// <summary>
        /// Returns the text label of the <see cref="T:System.Windows.FrameworkElement"/> that is associated with this <see cref="T:System.Windows.Automation.Peers.FrameworkElementAutomationPeer"/>. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetName"/>.
        /// </summary>
        /// <returns>
        /// The text label of the element that is associated with this automation peer.
        /// </returns>
        protected override string GetNameCore()
        {
            string name = base.GetNameCore();
            if (string.IsNullOrEmpty(name))
            {
                AutomationPeer labeledBy = GetLabeledByCore();
                if (labeledBy != null)
                {
                    name = labeledBy.GetName();
                }

                if (string.IsNullOrEmpty(name) && this.OwningDialogWindow.Header != null)
                {
                    name = this.OwningDialogWindow.Header.ToString();
                }
            }

            return name;
        }

        #endregion AutomationPeer overrides

        #region Event Handlers

        #region OwningDialogWindow_IsActiveChanged
        /// <summary>
        /// Handles the IsActiveChanged event of the OwningDialogWindow property as XamDialogWindow.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OwningDialogWindow_IsActiveChanged(object sender, EventArgs e)
        {
            bool newValue = this.OwningDialogWindow.IsActive;
            bool oldValue = !newValue;
            RaiseIsTopmostPropertyChangedEvent(oldValue, newValue);
        }
        #endregion //OwningDialogWindow_IsActiveChanged

        #region OwningDialogWindow_PropertyChanged
        /// <summary>
        /// Handles the PropertyChanged event of the OwningDialogWindow property as XamDialogWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OwningDialogWindow_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            string propertyName = e.PropertyName;

            switch (propertyName)
            {
                case "IsMoveable":
                    this.RaiseCanMovePropertyChangedEvent(!this.CanMove, this.CanMove);
                    break;
                case "IsResizable":
                    this.RaiseCanResizePropertyChangedEvent(!this.CanResize, !this.CanResize);
                    break;
            }
        }
        #endregion //OwningDialogWindow_PropertyChanged


        #region OwningDialogWindow_WindowStateChanged
        /// <summary>
        /// Handles the WindowStateChanged event of the OwningDialogWindow property as XamDialogWindow.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="WindowStateChangedEventArgs"/> instance containing the event data.</param>
        private void OwningDialogWindow_WindowStateChanged(object sender, WindowStateChangedEventArgs e)
        {

            WindowVisualState oldValue = ToWindowVisualState(e.NewWindowState);
            WindowVisualState newValue = ToWindowVisualState(e.PreviousWindowState);

            this.RaiseWindowVisualStatePropertyChangedEvent(oldValue, newValue);

            bool oldCanMaximize;
            bool newCanMaximize;

            bool oldCanMinimize;
            bool newCanMinimize;

            if (newValue != WindowVisualState.Maximized && !this.IsModal)
            {
                newCanMaximize = true;
            }
            else
            {
                newCanMaximize = false;
            }

            if (oldValue != WindowVisualState.Maximized && !this.IsModal)
            {
                oldCanMaximize = true;
            }
            else
            {
                oldCanMaximize = false;
            }

            if (newValue != WindowVisualState.Minimized && !this.IsModal)
            {
                newCanMinimize = true;
            }
            else
            {
                newCanMinimize = false;
            }

            if (oldValue != WindowVisualState.Minimized && !this.IsModal)
            {
                oldCanMinimize = true;
            }
            else
            {
                oldCanMinimize = false;
            }

            this.RaiseCanMaximizePropertyChangedEvent(oldCanMaximize, newCanMaximize);
            this.RaiseCanMinimizePropertyChangedEvent(oldCanMinimize, newCanMinimize);

        }
        #endregion //OwningDialogWindow_WindowStateChanged

        #endregion Event Handlers

        #region GetBoundingRectangleCore

        /// <summary>
        /// Gets the <see cref="System.Windows.Rect"/> that represents the bounds of the control.
        /// </summary>
        /// <returns></returns>
        protected override System.Windows.Rect GetBoundingRectangleCore()
        {

            System.Windows.Rect r = base.GetBoundingRectangleCore();




            r.X += this.OwningDialogWindow._moveTransform.X;
            r.Y += this.OwningDialogWindow._moveTransform.Y;
            
            return r;
        }

        #endregion // GetBoundingRectangleCore

        #region ITransformProvider

        #region Move
        /// <summary>
        /// Moves the control.
        /// </summary>
        /// <param name="x">The absolute screen coordinates of the left side of the control.</param>
        /// <param name="y">The absolute screen coordinates of the top of the control.</param>
        public void Move(double x, double y)
        {



            this.OwningDialogWindow.Left = x;
            this.OwningDialogWindow.Top = y;
        }
        #endregion //Move

        #region Resize
        /// <summary>
        /// Resizes the control.
        /// </summary>
        /// <param name="width">The new width of the window, in pixels.</param>
        /// <param name="height">The new height of the window, in pixels.</param>
        public void Resize(double width, double height)
        {
            this.OwningDialogWindow.Width = width;
            this.OwningDialogWindow.Height = height;
        }
        #endregion //Resize

        #region Rotate
        /// <summary>
        /// Rotates the control.
        /// </summary>
        /// <param name="degrees">The number of degrees to rotate the control. A positive number rotates the control clockwise. A negative number rotates the control counterclockwise.</param>
        public void Rotate(double degrees)
        {
        }
        #endregion //Rotate

        #region CanMove
        /// <summary>
        /// Gets a value indicating whether the element can be moved.
        /// </summary>
        /// <value></value>
        /// <returns>true if the element can be moved; otherwise, false. </returns>
        public bool CanMove
        {
            get
            {
                return this.OwningDialogWindow.IsMoveable;
            }
        }
        #endregion //CanMove

        #region CanResize
        /// <summary>
        /// Gets a value indicating whether the element can be resized.
        /// </summary>
        /// <value></value>
        /// <returns>true if the element can be resized; otherwise, false. </returns>
        public bool CanResize
        {
            get
            {
                return this.OwningDialogWindow.IsResizable;
            }
        }
        #endregion //CanResize

        #region CanRotate
        /// <summary>
        /// Gets a value indicating whether the element can be rotated.
        /// </summary>
        /// <value></value>
        /// <returns>true if the element can be rotated; otherwise, false.</returns>
        public bool CanRotate
        {
            get { return false; }
        }
        #endregion //CanRotate

        #endregion ITransformProvider

        #region IWindowProvider

        #region InteractionState
        /// <summary>
        /// Gets the interaction state of the window.
        /// </summary>
        /// <value></value>
        /// <returns>The interaction state of the control, as a value of the enumeration. </returns>
        public WindowInteractionState InteractionState
        {
            get
            {
                return WindowInteractionState.Running;
            }
        }
        #endregion //InteractionState

        #region IsModal
        /// <summary>
        /// Gets a value indicating whether the window is modal.
        /// </summary>
        /// <value></value>
        /// <returns>true if the window is modal; otherwise, false.</returns>
        public bool IsModal
        {
            get
            {
                return this.OwningDialogWindow.IsModal;
            }
        }
        #endregion //IsModal

        #region IsTopmost
        /// <summary>
        /// Gets a value indicating whether the window is the topmost element in the z-order of layout.
        /// </summary>
        /// <value></value>
        /// <returns>true if the window is topmost; otherwise, false.</returns>
        public bool IsTopmost
        {
            get
            {
                if (this.OwningDialogWindow.IsActive)
                {
                    return true;
                }

                return false;
            }
        }
        #endregion //IsTopmost

        #region Maximizable
        /// <summary>
        /// Gets a value indicating whether the window can be maximized.
        /// </summary>
        /// <value></value>
        /// <returns>true if the window can be maximized; otherwise, false.</returns>
        public bool Maximizable
        {
            get
            {
                if (this.OwningDialogWindow.MaximizeButtonVisibility == System.Windows.Visibility.Visible &&
                    this.OwningDialogWindow.IsModal == false)
                {
                    return true;
                }

                return false;
            }
        }
        #endregion //Maximizable

        #region Minimizable
        /// <summary>
        /// Gets a value indicating whether the window can be minimized.
        /// </summary>
        /// <value></value>
        /// <returns>true if the window can be minimized; otherwise, false.</returns>
        public bool Minimizable
        {
            get
            {
                if (this.OwningDialogWindow.MinimizeButtonVisibility == System.Windows.Visibility.Visible &&
                    this.OwningDialogWindow.IsModal == false)
                {
                    return true;
                }

                return false;
            }
        }
        #endregion //Minimizable

        #region VisualState
        /// <summary>
        /// Gets the visual state of the window.
        /// </summary>
        /// <value></value>
        /// <returns>The visual state of the window, as a value of the enumeration. </returns>
        public WindowVisualState VisualState
        {
            get
            {
                switch (this.OwningDialogWindow.WindowState)
                {
                    case WindowState.Maximized:
                        return WindowVisualState.Maximized;
                    case WindowState.Minimized:
                        return WindowVisualState.Minimized;
                    case WindowState.Normal:
                        return WindowVisualState.Normal;
                }

                return WindowVisualState.Normal;
            }
        }
        #endregion //VisualState

        #region Close
        /// <summary>
        /// Closes the window.
        /// </summary>
        public void Close()
        {
            this.OwningDialogWindow.Close();
        }
        #endregion //Close

        #region SetVisualState
        /// <summary>
        /// Changes the visual state of the window (such as minimizing or maximizing it).
        /// </summary>
        /// <param name="state">The visual state of the window to change to, as a value of the enumeration.</param>
        public void SetVisualState(WindowVisualState state)
        {
            switch (state)
            {
                case WindowVisualState.Maximized:
                    this.OwningDialogWindow.WindowState = WindowState.Maximized;
                    break;
                case WindowVisualState.Minimized:
                    this.OwningDialogWindow.WindowState = WindowState.Minimized;
                    break;
                case WindowVisualState.Normal:
                    this.OwningDialogWindow.WindowState = WindowState.Normal;
                    break;
            }
        }
        #endregion //SetVisualState

        #region WaitForInputIdle
        /// <summary>
        /// Blocks the calling code for the specified time or until the associated process enters an idle state, whichever completes first.
        /// </summary>
        /// <param name="milliseconds">The amount of time, in milliseconds, to wait for the associated process to become idle.</param>
        /// <returns>
        /// true if the window has entered the idle state; false if the timeout occurred.
        /// </returns>
        public bool WaitForInputIdle(int milliseconds)
        {
            return false;
        }
        #endregion //WaitForInputIdle

        #endregion IWindowProvider
    }
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