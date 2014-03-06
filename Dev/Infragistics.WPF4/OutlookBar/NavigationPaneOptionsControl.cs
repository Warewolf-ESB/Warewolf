using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Controls;
using Infragistics.Windows.OutlookBar.Internal;
using System.ComponentModel;

namespace Infragistics.Windows.OutlookBar
{
    /// <summary>
    /// A control used to present options that provide control over the visibility and order of <see cref="OutlookBarGroup"/>s in the navigation area of the <see cref="XamOutlookBar"/>.
    /// </summary>
    [TemplatePart(Name = "PART_ListBox", Type = typeof(Grid))]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class NavigationPaneOptionsControl : Control
    {
        #region Member variables

        ListBox _lbGroups;                                          
        private XamOutlookBar _xamBar;                              
        private ObservableCollection<ListBoxItem> _listBoxGroupProxyItems;
        #endregion //Member variables

        #region Constructors

        static NavigationPaneOptionsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NavigationPaneOptionsControl),
                new FrameworkPropertyMetadata(typeof(NavigationPaneOptionsControl)));

            #region CommitChangesAndCloseCommand CommandBinding

            CommandBinding cbCommit = new CommandBinding(NavigationPaneOptionsControlCommands.CommitChangesAndCloseCommand,
                NavigationPaneOptionsControl.ExecutedCommitChangesAndCloseCommand, NavigationPaneOptionsControl.CanExecuteCommand);

            CommandManager.RegisterClassCommandBinding(typeof(NavigationPaneOptionsControl), cbCommit);

            #endregion //CommitChangesAndCloseCommand CommandBinding

            #region MoveSelectedDownCommand CommandBinding

            CommandBinding cbMoveSelectedDownCommand = new CommandBinding(NavigationPaneOptionsControlCommands.MoveSelectedDownCommand,
                NavigationPaneOptionsControl.ExecutedMoveSelectedDownCommand, NavigationPaneOptionsControl.CanExecuteCommand);

            CommandManager.RegisterClassCommandBinding(typeof(NavigationPaneOptionsControl), cbMoveSelectedDownCommand);

            #endregion //MoveSelectedDownCommand CommandBinding

            #region MoveSelectedUpCommand CommandBinding

            CommandBinding cbMoveSelectedUpCommand = new CommandBinding(NavigationPaneOptionsControlCommands.MoveSelectedUpCommand,
                NavigationPaneOptionsControl.ExecutedMoveSelectedUpCommand, NavigationPaneOptionsControl.CanExecuteCommand);

            CommandManager.RegisterClassCommandBinding(typeof(NavigationPaneOptionsControl), cbMoveSelectedUpCommand);

            #endregion //MoveSelectedUpCommand CommandBinding

            #region ResetGroupSequenceAndVisibiltyCommand CommandBinding

            CommandBinding cbResetGroupSequenceAndVisibiltyCommand = new CommandBinding(NavigationPaneOptionsControlCommands.ResetGroupSequenceAndVisibilityCommand,
                NavigationPaneOptionsControl.ExecutedResetGroupSequenceAndVisibiltyCommand, NavigationPaneOptionsControl.CanExecuteCommand);

            CommandManager.RegisterClassCommandBinding(typeof(NavigationPaneOptionsControl), cbResetGroupSequenceAndVisibiltyCommand);

            #endregion //ResetGroupSequenceAndVisibiltyCommand CommandBinding
        }

        /// <summary>
        /// Iitializes a new NavigationPaneOptionsControl
        /// </summary>
        /// <seealso cref="XamOutlookBar"/>
		/// <seealso cref="OutlookBarGroup"/>
		public NavigationPaneOptionsControl()
        {
            this.Loaded += delegate(object sender, RoutedEventArgs e)
            {
                FocusFirstGroup();
            };
        }

        #endregion //Constructors

        #region Base Class Overrides

        #region OnApplyTemplate
        /// <summary>
        /// Called when the template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _lbGroups = this.GetTemplateChild("PART_ListBox") as ListBox;
			LoadGroups();
        }

        #endregion //OnApplyTemplate

        #region OnPreviewKeyDown

        /// <summary>
        /// Invoked when an unhandled System.Windows.Input.Keyboard.PreviewKeyDown attached
        /// event reaches an element in its route that is derived from this class. 
        /// </summary>
        /// <param name="e">The System.Windows.Input.KeyEventArgs that contains the event data.</param>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Space && Keyboard.FocusedElement is ListBoxItem)
            {
                ListBoxItem listItem = Keyboard.FocusedElement as ListBoxItem;
                OutlookBarGroupProxy focusedItem = listItem.Content as OutlookBarGroupProxy;
                if (focusedItem != null)
                {
                    if (!(Keyboard.FocusedElement is CheckBox))
                        focusedItem.IsSelected = !focusedItem.IsSelected;
                }
            }
            base.OnPreviewKeyDown(e);
        }

        #endregion //OnPreviewKeyDown	
    
        #region OnPreviewMouseDown

        /// <summary>
        /// Invoked when an unhandled System.Windows.Input.Mouse.PreviewMouseDown attached routed
        /// event reaches an element in its route that is derived from this class. 
        /// </summary>
        /// <param name="e">The System.Windows.Input.MouseButtonEventArgs that contains the event data.</param>
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (_lbGroups != null)
            {
                ListBoxItem listItem = ItemsControl.ContainerFromElement(_lbGroups, e.OriginalSource as DependencyObject) as ListBoxItem;
                if (listItem != null)
                {
                    if (listItem.Content is OutlookBarGroupProxy)
                        _lbGroups.SelectedItem = listItem;
                }
            }
            base.OnPreviewMouseDown(e);
        }

        #endregion //OnPreviewMouseDown	
    
        #region OnPreviewMouseDoubleClick

        /// <summary>
        /// Raises the System.Windows.Controls.Control.PreviewMouseDoubleClick routed event.
        /// </summary>
        /// <param name="e">The System.Windows.Input.MouseButtonEventArgs that contains the event data.</param>
        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (_lbGroups != null)
            {
                ListBoxItem listItem = ItemsControl.ContainerFromElement(_lbGroups, e.OriginalSource as DependencyObject) as ListBoxItem;
                if (listItem != null)
                {
                    DependencyObject o = listItem.InputHitTest(e.GetPosition(listItem)) as DependencyObject;
                    CheckBox groupIsVisibleCheckBox = FindParent(o, typeof(CheckBox)) as CheckBox;
                    if (groupIsVisibleCheckBox == null)
                    {
                        OutlookBarGroupProxy item = listItem.Content as OutlookBarGroupProxy;

                        if (item != null)
                            item.IsSelected = !item.IsSelected;
                    }
                }
            }
            base.OnPreviewMouseDoubleClick(e);
        }

        #endregion //OnPreviewMouseDoubleClick	
    
        #endregion //Base Class Overrides

        #region Properties

        #region Public Properties

        #region OutlookBar

        /// <summary>
        /// Returns/sets the <see cref="XamOutlookBar"/> control that the <see cref="NavigationPaneOptionsControl"/> is presenting options for.
        /// </summary>
		/// <seealso cref="XamOutlookBar"/>
		/// <seealso cref="OutlookBarGroup"/>
		public XamOutlookBar OutlookBar
        {
            get { return _xamBar; }
            set
            {
                _xamBar = value;
                if (_xamBar != null)
                    if (_xamBar.NavigationPaneOptionsControlStyle != null)
                        this.Style = _xamBar.NavigationPaneOptionsControlStyle;
                if (IsLoaded)
                    LoadGroups();
            }
        }

        #endregion //OutlookBar

        #endregion //Public Properties

        #endregion //Properties

        #region Methods

        #region Private Methods

        #region Load, Exit, Reset, Update Methods

        private void LoadGroups()
        {
            // loads XamOutlookBar.Groups and binds to the listbox
            if (_xamBar == null || _lbGroups == null)
                return;

            _listBoxGroupProxyItems = new ObservableCollection<ListBoxItem>();
			foreach (OutlookBarGroup gr in _xamBar.Groups)
				_listBoxGroupProxyItems.Add(new ListBoxItem() { Content = new OutlookBarGroupProxy(gr) });

            Binding b = new Binding();
            b.Source = _listBoxGroupProxyItems;

            _lbGroups.SetBinding(ListBox.ItemsSourceProperty, b);

            if (_lbGroups.Items.Count > 0)
                _lbGroups.SelectedIndex = 0;

            FocusFirstGroup();
        }

        private void FocusFirstGroup()
        {
            if (_lbGroups != null)
            {
                if (_lbGroups.Items.CurrentItem != null)
                {
                    ListBoxItem listItem = (ListBoxItem)(_lbGroups.Items.CurrentItem);
                    if (listItem != null)
                    {
                        Keyboard.Focus(listItem);
                        listItem.Focus();
                    }
                }
            }
        }

        private void ResetToInitialState()
        {
            // restores initial order na visibility using a command 
            // see ExecutedResetGroupSequenceAndVisibiltyCommand
            if (_xamBar == null)
                return;

            List<OutlookBarGroupProxy> sortArray = new List<OutlookBarGroupProxy>();
            foreach (ListBoxItem lbi in _listBoxGroupProxyItems)
            {
                OutlookBarGroupProxy gr = lbi.Content as OutlookBarGroupProxy;
                sortArray.Add(gr);
            }

            sortArray.Sort(new GroupsOrderComparer());  // this restores initial order
            _listBoxGroupProxyItems.Clear();
            foreach (OutlookBarGroupProxy gr in sortArray)
            {
                gr.IsSelected = gr.Group.IsVisibleOnStart;
                _listBoxGroupProxyItems.Add(new ListBoxItem() { Content = gr });
            }
            if (_listBoxGroupProxyItems.Count > 0)
                _lbGroups.SelectedIndex = 0;
            _xamBar.RaiseGroupsReset(new RoutedEventArgs());
        }

        private void UpdateXamOutlookBar()
        {
            // set groups order and visibility from the UI - checkboxes, oreder in the list box
            try
            {
                if (_xamBar == null)
                    return;
                _xamBar.Groups.CanEdit = true;

                _xamBar._contextMenuGroups.Clear();
                _xamBar._overflowAreaGroups.Clear();
                _xamBar._navigationAreaGroups.Clear();

                // creates groups from the proxy collection
                OutlookBarGroupCollection groups = new OutlookBarGroupCollection(_xamBar);
                groups.CanEdit = true;
                foreach (ListBoxItem lbi in _listBoxGroupProxyItems)
                {
                    OutlookBarGroupProxy gr = lbi.Content as OutlookBarGroupProxy;
					gr.Group.Visibility = gr.IsSelected ?
						Visibility.Visible : Visibility.Collapsed;  // selected means visible (UI)
                    groups.Add(gr.Group);
                }
                
                _xamBar.SetGroups(groups);

				// JM 10-6-10 Noticed the need for this while fixing TFS37329.
				_xamBar.VerifySelectedGroup(true);
            }
            finally
            {
                _xamBar.Groups.CanEdit = false;
            }
        }

        private DependencyObject FindParent(DependencyObject parent, Type parentType)
        {
            for (; parent != null; parent = VisualTreeHelper.GetParent(parent))
                if (parent.GetType() == parentType)
                    return parent;

            return null;
        }

        #endregion //Load, Reset, Update Methods	
    
        #region Commands Methods

        #region CanExecuteCommand

        private static void CanExecuteCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            NavigationPaneOptionsControl navPane = sender as NavigationPaneOptionsControl;

            e.CanExecute = false;

            if (navPane._xamBar == null)
                return;

            if (navPane == null)
                return;

            if (navPane._lbGroups == null)
                return;

            if (e.Command == NavigationPaneOptionsControlCommands.CommitChangesAndCloseCommand)
            {
                e.CanExecute = true;
            }
            else if (e.Command == NavigationPaneOptionsControlCommands.ResetGroupSequenceAndVisibilityCommand)
            {
                e.CanExecute = true;
            }
            else if (e.Command == NavigationPaneOptionsControlCommands.MoveSelectedDownCommand)
            {
                e.CanExecute = navPane._lbGroups.SelectedIndex >= 0
                    && navPane._lbGroups.SelectedIndex < (navPane._lbGroups.Items.Count - 1);
            }
            else if (e.Command == NavigationPaneOptionsControlCommands.MoveSelectedUpCommand)
            {
                e.CanExecute = navPane._lbGroups.SelectedIndex > 0;
            }
        }

        #endregion //CanExecuteCommand	

        #region Commands - ExecutedXXXCommand

        private static void ExecutedCommitChangesAndCloseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            // updates XamOutlookBar with the changes made by the user
            // closes parent tool window

            NavigationPaneOptionsControl navPane = sender as NavigationPaneOptionsControl;

            if (navPane == null)
                return;
            navPane.UpdateXamOutlookBar();

			ToolWindow toolWindow = ToolWindow.GetToolWindow(navPane);
			if (toolWindow != null)
				toolWindow.DialogResult = true;
        }

        private static void ExecutedMoveSelectedDownCommand(object sender, ExecutedRoutedEventArgs e)
        {
            // changes order of selected item in the list box
            NavigationPaneOptionsControl navPane = sender as NavigationPaneOptionsControl;
            if (navPane != null && navPane._lbGroups != null)
                navPane.SwapGroups(navPane._lbGroups.SelectedIndex, navPane._lbGroups.SelectedIndex + 1);
        }

        private static void ExecutedMoveSelectedUpCommand(object sender, ExecutedRoutedEventArgs e)
        {
            // changes order of selected item in the list box
            NavigationPaneOptionsControl navPane = sender as NavigationPaneOptionsControl;
            if (navPane != null && navPane._lbGroups != null)
                navPane.SwapGroups(navPane._lbGroups.SelectedIndex, navPane._lbGroups.SelectedIndex - 1);
        }

        private void SwapGroups(int selectedIndex, int nextIndex)
        {
            _lbGroups.SelectedItem = null;
            _listBoxGroupProxyItems.Move(selectedIndex, nextIndex);
            _lbGroups.SelectedIndex = nextIndex;
        }
        private static void ExecutedResetGroupSequenceAndVisibiltyCommand(object sender, ExecutedRoutedEventArgs e)
        {
            // restores the initial oreder and visibility of XamOutlookBar.Groups
            NavigationPaneOptionsControl navPane = sender as NavigationPaneOptionsControl;

            if (navPane == null)
                return;
            navPane.ResetToInitialState();
        }

        #endregion //Commands - Executed	
    
        #endregion //Commands Methods	
    
        #endregion //Private Methods	
    
        #endregion //Methods

        #region Commands
        // see NavigationPaneOptionsControlCommands
        #endregion //Commands
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