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
    internal sealed class InteractionHelper
    {

        public Control Control { get; private set; }
        
        public bool IsFocused { get; private set; }
        
        public bool IsMouseOver { get; private set; }
        
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Linked file.")]
        public bool IsReadOnly { get; set; }
        
        public bool IsPressed { get; private set; }
        
        public int ClickCount { get; set; }
        
        private readonly IUpdateVisualState _updateVisualState;
        
        public InteractionHelper(Control control)
        {
            Debug.Assert(control != null, "control should not be null!");
            Control = control;
            _updateVisualState = control as IUpdateVisualState;
            
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
        
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateVisualState(false);
        }
        
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
        
        public void OnApplyTemplateBase()
        {
            UpdateVisualState(false);
        }

    }
}
