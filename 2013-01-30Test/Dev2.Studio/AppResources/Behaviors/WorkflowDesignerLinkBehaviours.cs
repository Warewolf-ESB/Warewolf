using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
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
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
        }

        #endregion Override Methods

        #region Event Handlers

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            _expandAllButton = AssociatedObject.FindNameAcrossNamescopes("expandAllButton") as ToggleButton;
            _collapseAllButton = AssociatedObject.FindNameAcrossNamescopes("collapseAllButton") as ToggleButton;

            Binding expandAllBinding = new Binding("IsChecked");
            expandAllBinding.Source = _expandAllButton;

            Binding CollapseAllBinding = new Binding("IsChecked");
            CollapseAllBinding.Source = _collapseAllButton;

            _expandAllButton.Command = ExpandAllCommand;
            _expandAllButton.SetBinding(ButtonBase.CommandParameterProperty, expandAllBinding);

            _collapseAllButton.Command = CollapseAllCommand;
            _collapseAllButton.SetBinding(ButtonBase.CommandParameterProperty, CollapseAllBinding);
        }

        #endregion Event Handlers

        #region Tear Down

        public void Dispose()
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;

            _expandAllButton.Command = null;
            _collapseAllButton.Command = null;

            _expandAllButton = null;
            _collapseAllButton = null;
        }

        #endregion Tear Down
    }
}
