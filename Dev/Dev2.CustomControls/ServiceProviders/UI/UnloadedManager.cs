
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// Copyright (C) Josh Smith - October 2006

using System.Windows;

namespace WPF.JoshSmith.ServiceProviders.UI
{
    #region IUnloadable

    /// <summary>
    /// Provides a means of releasing resources, in conjunction with the UnloadedManager.
    /// </summary>
    public interface IUnloadable
    {
        /// <summary>
        /// Invoked when an object's resources should be released.  The object should
        /// still be in a usable/operable state after this method is invoked.
        /// </summary>
        void Unload();
    }

    #endregion // IUnloadable

    #region UnloadedManager

    /// <summary>
    /// A service provider class which provides a means of releasing resources
    /// when a FrameworkElement's Unloaded event fires.  If the DataContext of
    /// the element implements IUnloadable, it's Unload method will be invoked
    /// when the elements Unloaded event fires.
    /// </summary>
    /// <remarks>
    /// Documentation: 
    /// http://web.archive.org/web/20070127124811/http://www.infusionblogs.com/blogs/jsmith/archive/2006/10/28/917.aspx
    /// </remarks>
    public static class UnloadedManager
    {
        #region IsManaged

        /// <summary>
        /// Identifies the UnloadedManager's IsManaged attached property.  
        /// This field is read-only.
        /// </summary>
        public static readonly DependencyProperty IsManagedProperty =
            DependencyProperty.RegisterAttached(
                "IsManaged",
                typeof(bool),
                typeof(UnloadedManager),
                new UIPropertyMetadata(false, OnIsManagedChanged));

        /// <summary>
        /// Returns true if the specified FrameworkElement's Unloaded event will cause the UnloadedManager to 
        /// unload its associated data object, else false
        /// </summary>
        /// <param name="element">The FrameworkElement to check if it is managed or not.</param>
        public static bool GetIsManaged(FrameworkElement element)
        {
            return (bool)element.GetValue(IsManagedProperty);
        }

        /// <summary>
        /// Sets the IsManaged attached property for the specified FrameworkElement.
        /// </summary>
        /// <param name="element">The FrameworkElement to be managed or unmanaged.</param>
        /// <param name="value">True if the element should be managed by the UnloadedManager.</param>
        public static void SetIsManaged(FrameworkElement element, bool value)
        {
            element.SetValue(IsManagedProperty, value);
        }

        // Invoked when the IsManaged attached property is set for a FrameworkElement.
        static void OnIsManagedChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement elem = depObj as FrameworkElement;
            if(elem == null)
                return;

            bool isManaged = (bool)e.NewValue;
            if(isManaged)
                elem.Unloaded += OnManagedFrameworkElementUnloaded;
            else
                elem.Unloaded -= OnManagedFrameworkElementUnloaded;
        }

        #endregion // IsManaged

        #region OnManagedFrameworkElementUnloaded

        static void OnManagedFrameworkElementUnloaded(object sender, RoutedEventArgs e)
        {
            // Call Unload() on the element's DataContext.

            FrameworkElement elem = sender as FrameworkElement;
            if(elem == null)
                return;

            IUnloadable unloadable = elem.DataContext as IUnloadable;
            if(unloadable == null)
                return;

            unloadable.Unload();

            // Set IsManaged to false for the element so that the Unloaded
            // event handler is detached, which ensures that it is not 
            // referenced any longer.  That will allow the GC to collect it.
            SetIsManaged(elem, false);
        }

        #endregion // OnManagedFrameworkElementUnloaded
    }

    #endregion //UnloadedManager
}
