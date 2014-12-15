
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
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using Dev2.Studio.AppResources.ExtensionMethods;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class WorkflowDesignerLinkBehaviours : Behavior<FrameworkElement>, IDisposable
    {
        #region Class Members

        ToggleButton _expandAllButton;
        ToggleButton _collapseAllButton;

        #endregion Class Members

        #region Dependency Properties

        #region ExpandAllCommand

        public ICommand ExpandAllCommand
        {
            get { return (ICommand)GetValue(ExpandAllCommandProperty); }
            set { SetValue(ExpandAllCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExpandAllCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExpandAllCommandProperty =
            DependencyProperty.Register("ExpandAllCommand", typeof(ICommand), typeof(WorkflowDesignerLinkBehaviours), new PropertyMetadata(null));

        #endregion ExpandAllCommand

        #region CollapseAllCommand

        public ICommand CollapseAllCommand
        {
            get { return (ICommand)GetValue(CollapseAllCommandProperty); }
            set { SetValue(CollapseAllCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExpandAllCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CollapseAllCommandProperty =
            DependencyProperty.Register("CollapseAllCommand", typeof(ICommand), typeof(WorkflowDesignerLinkBehaviours), new PropertyMetadata(null));

        #endregion ExpandAllCommand

        #endregion Dependency Properties

        #region Override Methods

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObjectLoaded;
            AssociatedObject.Unloaded += AssociatedObjectOnUnloaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            CleanUp();
        }

        #endregion Override Methods

        #region Event Handlers

        private void AssociatedObjectOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            CleanUp();
            routedEventArgs.Handled = true;
        }

        private void AssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
            _expandAllButton = AssociatedObject.FindNameAcrossNamescopes("expandAllButton") as ToggleButton;
            _collapseAllButton = AssociatedObject.FindNameAcrossNamescopes("collapseAllButton") as ToggleButton;

            if (_expandAllButton != null)
            {
                Binding expandAllBinding = new Binding("IsChecked") 
                { 
                    Source = _expandAllButton 
                };

                _expandAllButton.Command = ExpandAllCommand;
                _expandAllButton.SetBinding(ButtonBase.CommandParameterProperty, expandAllBinding);
            }

            if (_collapseAllButton != null)
            {
                Binding collapseAllBinding = new Binding("IsChecked") 
                { 
                    Source = _collapseAllButton 
                };

                _collapseAllButton.Command = CollapseAllCommand;
                _collapseAllButton.SetBinding(ButtonBase.CommandParameterProperty, collapseAllBinding);
            }
        }

        #endregion Event Handlers

        #region Tear Down

        private void CleanUp()
        {
            AssociatedObject.Loaded -= AssociatedObjectLoaded;
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;

            if (_expandAllButton != null)
            {
                _expandAllButton.Command = null;
                _expandAllButton = null;
            }

            if (_collapseAllButton != null)
            {
                _collapseAllButton.Command = null;
                _collapseAllButton = null;
            }
        }

        public void Dispose()
        {
            CleanUp();
        }

        #endregion Tear Down
    }
}
