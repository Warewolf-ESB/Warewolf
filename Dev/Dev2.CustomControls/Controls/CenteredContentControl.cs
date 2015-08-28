/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// Copyright (C) Josh Smith - November 2006 

using System.Windows;
using System.Windows.Controls;

namespace WPF.JoshSmith.Controls
{
    /// <summary>
    ///     A ContentControl which exposes two dependency properties, CenterX and CenterY.
    ///     Those properties can be bound to if this element is in a Canvas, and it is positioned
    ///     via the Left and Top attached properties of the Canvas class.
    /// </summary>
    public class CenteredContentControl : ContentControl
    {
        #region Constructor

        /// <summary>
        ///     Instance constructor.
        /// </summary>
        public CenteredContentControl()
        {
            // When the element first loads, initialize 
            // the two dependency properties.
            Loaded += delegate
            {
                UpdateCenterX();
                UpdateCenterY();
            };
        }

        #endregion // Constructor

        #region CenterX / CenterY

        private static readonly DependencyPropertyKey CenterXPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "CenterX",
                typeof (double),
                typeof (CenteredContentControl),
                new UIPropertyMetadata());

        private static readonly DependencyPropertyKey CenterYPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "CenterY",
                typeof (double),
                typeof (CenteredContentControl),
                new UIPropertyMetadata());

        /// <summary>
        ///     Identifier for the read-only CenterX dependency property.
        /// </summary>
        public static readonly DependencyProperty CenterXProperty = CenterXPropertyKey.DependencyProperty;

        /// <summary>
        ///     Identifier for the read-only CenterX dependency property.
        /// </summary>
        public static readonly DependencyProperty CenterYProperty = CenterYPropertyKey.DependencyProperty;

        /// <summary>
        ///     Returns the horizontal offset of this element within its containing Canvas.
        ///     Note, this property only returns a meaningful value if the Canvas.Left attached
        ///     property is set on this element.
        /// </summary>
        public double CenterX
        {
            get { return (double) GetValue(CenterXProperty); }
        }

        /// <summary>
        ///     Returns the vertical offset of this element within its containing Canvas.
        ///     Note, this property only returns a meaningful value if the Canvas.Top attached
        ///     property is set on this element.
        /// </summary>
        public double CenterY
        {
            get { return (double) GetValue(CenterYProperty); }
        }

        #endregion // CenterX / CenterY

        #region OnPropertyChanged

        /// <summary>
        ///     Updates the CenterX and CenterY properties.
        /// </summary>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            bool sizeChanged =
                e.Property.Name == "ActualWidth" ||
                e.Property.Name == "ActualHeight";

            // If the property that changed does not affect the element's size
            // and it is not an attached property of Canvas, then there is 
            // nothing to do.
            if (!sizeChanged &&
                !typeof (Canvas).IsAssignableFrom(e.Property.OwnerType))
                return;

            if (e.Property.Name == "Left" || e.Property.Name == "ActualWidth")
            {
                UpdateCenterX();
            }
            else if (e.Property.Name == "Top" || e.Property.Name == "ActualHeight")
            {
                UpdateCenterY();
            }
        }

        #endregion // OnPropertyChanged

        #region Private Helpers

        private void UpdateCenterX()
        {
            double left = Canvas.GetLeft(this);
            double offset = left + ActualWidth/2;
            SetValue(CenterXPropertyKey, offset);
        }

        private void UpdateCenterY()
        {
            double top = Canvas.GetTop(this);
            double offset = top + ActualHeight/2;
            SetValue(CenterYPropertyKey, offset);
        }

        #endregion // Private Helpers
    }
}