
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class NavigationItemViewModelDeployHighlightingBehavior : Behavior<Control>
    {
        #region Class Members

        #endregion Class Members

        #region Override Methods

        #endregion Override Methods

        #region Dependency Properties

        #region OverrideHighlightBrush

        public Brush OverrideHighlightBrush
        {
            get { return (Brush)GetValue(OverrideHighlightBrushProperty); }
            set { SetValue(OverrideHighlightBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HighlightBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OverrideHighlightBrushProperty =
            DependencyProperty.Register("OverrideHighlightBrush", typeof(Brush), typeof(NavigationItemViewModelDeployHighlightingBehavior), new PropertyMetadata(null));


        #endregion OverrideHighlightBrush

        #region TargetEnvironment

        public IEnvironmentModel TargetEnvironment
        {
            get { return (IEnvironmentModel)GetValue(TargetEnvironmentProperty); }
            set { SetValue(TargetEnvironmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TargetEnvironmentModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetEnvironmentProperty =
            DependencyProperty.Register("TargetEnvironment", typeof(IEnvironmentModel), typeof(NavigationItemViewModelDeployHighlightingBehavior), new PropertyMetadata(TargetEnvironmentUpdated));

        private static void TargetEnvironmentUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var navigationItemViewModelDeployHighlightingBehavior = d as NavigationItemViewModelDeployHighlightingBehavior;

            if (navigationItemViewModelDeployHighlightingBehavior != null)
            {
                navigationItemViewModelDeployHighlightingBehavior.UpdateHighlighting();
            }
        }

        #endregion TargetEnvironment

        #endregion Dependency Properties

        #region Private Methods

        /// <summary>
        /// Updates the highlighting for the currently attached object
        /// </summary>
        private void UpdateHighlighting()
        {            
        }        

        #endregion Private Methods

        #region Event Handler Methods

        #endregion Event Handler Methods
    }
}
