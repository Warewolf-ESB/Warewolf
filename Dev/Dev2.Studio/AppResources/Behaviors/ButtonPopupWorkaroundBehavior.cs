
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class ButtonPopupWorkaroundBehavior : Behavior<ButtonBase>, IDisposable
    {
        #region Class Members

        private bool _isButtonDown;

        #endregion Class Members

        #region Dependency Properties

        #region IsInWorkaroundState

        public bool IsInWorkaroundState
        {
            get { return (bool)GetValue(IsInWorkaroundStateProperty); }
            set { SetValue(IsInWorkaroundStateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsInWorkaroundState.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsInWorkaroundStateProperty =
            DependencyProperty.Register("IsInWorkaroundState", typeof(bool), typeof(ButtonPopupWorkaroundBehavior), new PropertyMetadata(false));

        #endregion IsInWorkaroundState

        #endregion Dependency Properties

        #region Override Methods

        /// <summary>
        /// Called when [attached].
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            SubscribeToEvents();
            IsInWorkaroundState = false;
        }

        /// <summary>
        /// Called when [detaching].
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            UnsubscribeFromEvents();
            IsInWorkaroundState = false;
        }

        #endregion OverrideMethods

        #region Private Methods

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        private void SubscribeToEvents()
        {
            AssociatedObject.PreviewMouseUp -= AssociatedObject_PreviewMouseUp;
            AssociatedObject.PreviewMouseUp += AssociatedObject_PreviewMouseUp;

            AssociatedObject.PreviewMouseDown -= AssociatedObject_PreviewMouseDown;
            AssociatedObject.PreviewMouseDown += AssociatedObject_PreviewMouseDown;
        }

        /// <summary>
        /// Unsubscribes from events.
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            AssociatedObject.PreviewMouseUp -= AssociatedObject_PreviewMouseUp;
            AssociatedObject.PreviewMouseDown -= AssociatedObject_PreviewMouseDown;
        }

        #endregion Private Methods

        #region Event Handlers

        /// <summary>
        /// Handles the PreviewMouseDown event of the AssociatedObject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs" /> instance containing the event data.</param>
        private void AssociatedObject_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _isButtonDown = true;
        }

        /// <summary>
        /// Handles the PreviewMouseUp event of the AssociatedObject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs" /> instance containing the event data.</param>
        private void AssociatedObject_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(_isButtonDown == false)
            {
                IsInWorkaroundState = true;
            }
            else
            {
                _isButtonDown = false;
            }
        }

        #endregion Event Handlers

        #region Dispose

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Dispose()
        {
            UnsubscribeFromEvents();
        }

        #endregion Dispose
    }
}
