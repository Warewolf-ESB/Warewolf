using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.Navigation;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class NavigationItemViewModelDeployHighlightingBehavior : Behavior<Control>
    {
        #region Class Members

        private Brush _originalBackground;
        private AbstractTreeViewModel _navigationItemViewModel;

        #endregion Class Members

        #region Override Methods

        protected override void OnAttached()
        {
            base.OnAttached();

            _originalBackground = AssociatedObject.Background;
            _navigationItemViewModel = AssociatedObject.DataContext as AbstractTreeViewModel;
            //TODO Juries SubscribeToEvents();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            //TODO Juries UnsubscribeToEvents();
            _originalBackground = null;
            _navigationItemViewModel = null;
        }

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
            NavigationItemViewModelDeployHighlightingBehavior navigationItemViewModelDeployHighlightingBehavior = d as NavigationItemViewModelDeployHighlightingBehavior;

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
            //TODO Jurie
            //if (OverridePredicate(_navigationItemViewModel))
            //{
            //    AssociatedObject.Background = OverrideHighlightBrush;
            //}
            //else
            //{
            //    AssociatedObject.Background = _originalBackground;
            //}
        }

        //TODO Jurie
        /// <summary>
        /// Determines if an item is being overridden
        /// </summary>
        //private bool OverridePredicate(AbstractTreeViewModel navigationItemViewModel)
        //{
        //    return navigationItemViewModel != null && navigationItemViewModel.IsChecked.Value && navigationItemViewModel.DataContext != null &&
        //               TargetEnvironment != null && TargetEnvironment.Resources != null &&
        //               TargetEnvironment.Resources.All().Any(r => ResourceModelEqualityComparer.Current.Equals(r, navigationItemViewModel.DataContext));
        //}

        //TODO Jurie
        ///// <summary>
        ///// Subscribes to the associated objects events for this behavior
        ///// </summary>
        //private void SubscribeToEvents()
        //{
        //    if (AssociatedObject != null)
        //    {
        //        _navigationItemViewModel.Checked -= _navigationItemViewModel_Checked;
        //        _navigationItemViewModel.Checked += _navigationItemViewModel_Checked;
        //    }
        //}

        ///// <summary>
        ///// Unsubscribes from the associated objects events for this behavior
        ///// </summary>
        //private void UnsubscribeToEvents()
        //{
        //    if (AssociatedObject != null)
        //    {
        //        _navigationItemViewModel.Checked -= _navigationItemViewModel_Checked;
        //    }
        //}

        #endregion Private Methods

        #region Event Handler Methods

        private void _navigationItemViewModel_Checked(object sender, EventArgs e)
        {
            UpdateHighlighting();
        }

        #endregion Event Handler Methods
    }
}
