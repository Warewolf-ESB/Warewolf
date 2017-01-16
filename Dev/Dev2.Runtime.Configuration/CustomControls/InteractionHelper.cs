/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Windows.Controls
{
    /// <summary>
    /// The InteractionHelper provides controls with support for all of the
    /// common interactions like mouse movement, mouse clicks, key presses,
    /// etc., and also incorporates proper event semantics when the control is
    /// disabled.
    /// </summary>
    internal sealed class InteractionHelper
    {
        // TODO: Consult with user experience experts to validate the double
        // click distance and time thresholds.

        /// <summary>
        /// The threshold used to determine whether two clicks are temporally
        /// local and considered a double click (or triple, quadruple, etc.).
        /// 500 milliseconds is the default double click value on Windows.
        /// This value would ideally be pulled form the system settings.
        /// </summary>
        private const double SequentialClickThresholdInMilliseconds = 500.0;

        /// <summary>
        /// The threshold used to determine whether two clicks are spatially
        /// local and considered a double click (or triple, quadruple, etc.)
        /// in pixels squared.  We use pixels squared so that we can compare to
        /// the distance delta without taking a square root.
        /// </summary>
        private const double SequentialClickThresholdInPixelsSquared = 3.0 * 3.0;

        /// <summary>
        /// Gets the control the InteractionHelper is targeting.
        /// </summary>
        public Control Control { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the control has focus.
        /// </summary>
        public bool IsFocused { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the mouse is over the control.
        /// </summary> 
        public bool IsMouseOver { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the read-only property is set.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Linked file.")]
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets a value indicating whether the mouse button is pressed down
        /// over the control.
        /// </summary>
        public bool IsPressed { get; private set; }

        /// <summary>
        /// Gets or sets the last time the control was clicked.
        /// </summary>
        /// <remarks>
        /// The value is stored as Utc time because it is slightly more
        /// per formant than converting to local time.
        /// </remarks>
        private DateTime LastClickTime { get; set; }

        /// <summary>
        /// Gets or sets the mouse position of the last click.
        /// </summary>
        /// <remarks>The value is relative to the control.</remarks>
        private Point LastClickPosition { get; set; }

        /// <summary>
        /// Gets the number of times the control was clicked.
        /// </summary>
        public int ClickCount { get; set; }

        /// <summary>
        /// Reference used to call UpdateVisualState on the base class.
        /// </summary>
        private readonly IUpdateVisualState _updateVisualState;

        /// <summary>
        /// Initializes a new instance of the InteractionHelper class.
        /// </summary>
        /// <param name="control">Control receiving interaction.</param>
        public InteractionHelper(Control control)
        {
            Debug.Assert(control != null, "control should not be null!");
            Control = control;
            _updateVisualState = control as IUpdateVisualState;

            // Wire up the event handlers for events without a virtual override
            control.Loaded += OnLoaded;
            control.IsEnabledChanged += OnIsEnabledChanged;
        }

        #region UpdateVisualState
        /// <summary>
        /// Update the visual state of the control.
        /// </summary>
        /// <param name="useTransitions">
        /// A value indicating whether to automatically generate transitions to
        /// the new state, or instantly transition to the new state.
        /// </param>
        /// <remarks>
        /// UpdateVisualState works differently than the rest of the injected
        /// functionality.  Most of the other events are overridden by the
        /// calling class which calls Allow, does what it wants, and then calls
        /// Base.  UpdateVisualState is the opposite because a number of the
        /// methods in InteractionHelper need to trigger it in the calling
        /// class.  We do this using the IUpdateVisualState internal interface.
        /// </remarks>
        private void UpdateVisualState(bool useTransitions)
        {
            _updateVisualState?.UpdateVisualState(useTransitions);
        }

        /// <summary>
        /// Update the visual state of the control.
        /// </summary>
        /// <param name="useTransitions">
        /// A value indicating whether to automatically generate transitions to
        /// the new state, or instantly transition to the new state.
        /// </param>
        public void UpdateVisualStateBase(bool useTransitions)
        {
            // Handle the Common states
            if(!Control.IsEnabled)
            {
                VisualStates.GoToState(Control, useTransitions, VisualStates.StateDisabled, VisualStates.StateNormal);
            }
            else if(IsReadOnly)
            {
                VisualStates.GoToState(Control, useTransitions, VisualStates.StateReadOnly, VisualStates.StateNormal);
            }
            else if(IsPressed)
            {
                VisualStates.GoToState(Control, useTransitions, VisualStates.StatePressed, VisualStates.StateMouseOver, VisualStates.StateNormal);
            }
            else if(IsMouseOver)
            {
                VisualStates.GoToState(Control, useTransitions, VisualStates.StateMouseOver, VisualStates.StateNormal);
            }
            else
            {
                VisualStates.GoToState(Control, useTransitions, VisualStates.StateNormal);
            }

            // Handle the Focused states
            if(IsFocused)
            {
                VisualStates.GoToState(Control, useTransitions, VisualStates.StateFocused, VisualStates.StateUnfocused);
            }
            else
            {
                VisualStates.GoToState(Control, useTransitions, VisualStates.StateUnfocused);
            }
        }
        #endregion UpdateVisualState

        /// <summary>
        /// Handle the control's Loaded event.
        /// </summary>
        /// <param name="sender">The control.</param>
        /// <param name="e">Event arguments.</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateVisualState(false);
        }

        /// <summary>
        /// Handle changes to the control's IsEnabled property.
        /// </summary>
        /// <param name="sender">The control.</param>
        /// <param name="e">Event arguments.</param>
        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool enabled = (bool)e.NewValue;
            if(!enabled)
            {
                IsPressed = false;
                IsMouseOver = false;
                IsFocused = false;
            }

            UpdateVisualState(true);
        }

        /// <summary>
        /// Update the visual state of the control when its template is changed.
        /// </summary>
        public void OnApplyTemplateBase()
        {
            UpdateVisualState(false);
        }

    }
}
