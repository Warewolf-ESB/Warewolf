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
using Infragistics.Windows.OutlookBar.Internal;
using Infragistics.Windows.Controls;
using System.ComponentModel;

namespace Infragistics.Windows.OutlookBar
{
    /// <summary>
    /// Defines an area that displays <see cref="OutlookBarGroup"/>s that cannot fit in the navigation area of the <see cref="XamOutlookBar"/>.
    /// </summary>
    [TemplatePart(Name = "PART_OverflowGroups", Type = typeof(GroupsPresenter))]
    [TemplatePart(Name = "PART_OverflowMenu", Type = typeof(MenuItem))]
	[TemplatePart(Name = "PART_AddRemoveMenu", Type = typeof(MenuItem))]		// JM 04-30-09 TFS 16159
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class GroupOverflowArea : Control
    {
        #region Member Variables

        MenuItem _overflowMenu;     // this is main overflow menu 
        MenuItem _addRemoveMenu;    // this is Add or Remove Items submenu    

        // holds OutlookBarGroupProxy groups sorted by initial order of appearance
        ObservableCollection<OutlookBarGroupProxy> _groupsAddRemove; 
        
        #endregion //Member Variables	

        #region Constructors

        static GroupOverflowArea()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GroupOverflowArea), new FrameworkPropertyMetadata(typeof(GroupOverflowArea)));
        }

        /// <summary>
		/// Initializes a new <see cref="GroupOverflowArea"/>
        /// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>An instance of this class is automatically created by the <see cref="XamOutlookBar"/> in the control's default Templates. You do not normally need to create an instance of this class.</p>
		/// </remarks>
		public GroupOverflowArea()
        {
        }

        #endregion //Constructors	
    
        #region Base Class Override

        #region OnApplyTemplate
        /// <summary>
        /// Called when the template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_overflowMenu != null)
                _overflowMenu.SubmenuOpened -= OverflowMenu_SubmenuOpened;
            if (_addRemoveMenu != null)
            {
                _addRemoveMenu.SubmenuOpened -= AddRemoveMenu_SubmenuOpened;
                _addRemoveMenu.Loaded -= AddRemoveMenu_Loaded;
            }

			_overflowMenu = this.GetTemplateChild("PART_OverflowMenu") as MenuItem;

			// JM 04-30-09 TFS 16159 - Change to use 'PART_' naming convention but support old name for backward compatibility with customer styles.
			_addRemoveMenu = this.GetTemplateChild("MenuAddRemove") as MenuItem;
			if (_addRemoveMenu == null)
				_addRemoveMenu = this.GetTemplateChild("PART_AddRemoveMenu") as MenuItem;

            // we should rebuild Add or Remove Items Menu at runtime
            if (_overflowMenu != null)
                _overflowMenu.SubmenuOpened += new RoutedEventHandler(OverflowMenu_SubmenuOpened);

            if (_addRemoveMenu != null)
            {
                _addRemoveMenu.SubmenuOpened += new RoutedEventHandler(AddRemoveMenu_SubmenuOpened);
                _addRemoveMenu.Loaded += new RoutedEventHandler(AddRemoveMenu_Loaded);
            }

            RebuildAddRemoveMenu();
        }

        #endregion //OnApplyTemplate	

        #endregion //Base Class Override

        #region Methods

        #region Private Methods

        #region Add or Remove Buttons Menu

        void AddRemoveMenu_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            if (e.Source == _addRemoveMenu)
                RebuildAddRemoveMenu();
        }

        void AddRemoveMenu_Loaded(object sender, RoutedEventArgs e)
        {
            XamOutlookBar xamBar = this.GetValue(XamOutlookBar.OutlookBarProperty) as XamOutlookBar;
            if (xamBar == null)
                return;
            if (_addRemoveMenu.IsEnabled != xamBar.Groups.Count > 0)
                RebuildAddRemoveMenu();
        }

        // this rebuilds Add Or Remove Buttons SubMenu in the overflow area
        void RebuildAddRemoveMenu()
        {
            XamOutlookBar xamBar = this.GetValue(XamOutlookBar.OutlookBarProperty) as XamOutlookBar;

            if (_addRemoveMenu == null || xamBar == null)
                return;

            // Fill the _groupsAddRemove collection from XamOutlookBar.Groups
            _groupsAddRemove = new ObservableCollection<OutlookBarGroupProxy>();
            foreach (OutlookBarGroup gr in xamBar.Groups)
                _groupsAddRemove.Add(new OutlookBarGroupProxy(gr));

            // Sort the groups in initial order of appearance
            List<OutlookBarGroupProxy> sortArray = new List<OutlookBarGroupProxy>(_groupsAddRemove);
            sortArray.Sort(new GroupsOrderComparer());
            _groupsAddRemove = new ObservableCollection<OutlookBarGroupProxy>(sortArray);

            _addRemoveMenu.IsEnabled = _groupsAddRemove.Count > 0;

            _addRemoveMenu.Items.Clear();
            OutlookBarGroupProxy grOptions;
            for (int i = 0; i < _groupsAddRemove.Count; i++)
            {
                grOptions = _groupsAddRemove[i];

                MenuItem menuItem = new MenuItem(); // cerate a menu item

                // create icon
                Image icon = new Image();
                icon.Source = grOptions.Group.SmallImageResolved;

                menuItem.Header = grOptions.Group.Header;
                menuItem.IsChecked = grOptions.IsSelected;
                menuItem.IsCheckable = true;
                menuItem.Icon = icon;

                // set the check mark
				// JM TFS21654 09-04-09
				//Binding binding = new Binding("IsChecked");
				//binding.Source = menuItem;
				//binding.Mode = BindingMode.TwoWay;
				//binding.Converter = new BooleanToVisibilityConverter(); //new ccc();
				//grOptions.Group.SetBinding(Control.VisibilityProperty, binding);
				Binding binding = new Binding("Visibility");
				binding.Source = grOptions.Group;
				binding.Mode = BindingMode.TwoWay;
				binding.Converter = new VisibilityToBooleanConverter();
				menuItem.SetBinding(MenuItem.IsCheckedProperty, binding);

                // Call SetResourceReference for the Style property.
                // Note: This is equivalent to specifying a DynamicResource in xaml
                menuItem.SetResourceReference(MenuItem.StyleProperty, XamOutlookBar.OverflowMenuItemStyleKey);

                _addRemoveMenu.Items.Add(menuItem);

            }//end for - // cerate a menu items

        }

        #endregion //Add or Remove Buttons Menu

        #region Overflow Menu Items

        void OverflowMenu_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            if (e.Source == _overflowMenu)
                RebuildOverflowMenu();
        }

        // this rebuilds groups in the overflow menu
        private void RebuildOverflowMenu()
        {
            ClearMenuGroups();
            AddMenuGroups();
            XamOutlookBar xob = this.GetValue(XamOutlookBar.OutlookBarProperty) as XamOutlookBar;
            if (xob != null)
                xob.SaveGroupOrderAndVisibility();
        }
        
        // adds menu items for OutlookBargroups that are outside from Navigation/Overflow areas
        private void AddMenuGroups()
        {
            XamOutlookBar xamBar = this.GetValue(XamOutlookBar.OutlookBarProperty) as XamOutlookBar;

            if (_overflowMenu == null || xamBar == null)
                return;

            for (int i = 0; i < xamBar._contextMenuGroups.Count; i++)
            {
                OutlookBarGroup gr = xamBar._contextMenuGroups[i];
                if (gr.Visibility != Visibility.Collapsed)
                {
                    XamOutlookBar.SetOutlookBar(gr, xamBar);

                    MenuItem menuItem = new MenuItem();
                    Image icon = new Image();
                    icon.Source = gr.SmallImageResolved;
                    menuItem.Header = gr.Header;
                    menuItem.Icon = icon;
                    menuItem.IsCheckable = false;
                    menuItem.Command = OutlookBarCommands.SelectGroupCommand;
                    menuItem.CommandParameter = gr;
                    menuItem.CommandTarget = xamBar;
                    menuItem.Tag = gr;

                    // Call SetResourceReference for the Styke property.
                    // Note: This is equivalent to specifying a DynamicResource in xaml
                    menuItem.SetResourceReference(MenuItem.StyleProperty, XamOutlookBar.OverflowMenuItemStyleKey);

                    _overflowMenu.Items.Add(menuItem);
                }//end if- skip collapsed
            }//end for - append OutlookBarGroups to the overflow menu 
        }

        private void ClearMenuGroups()
        {
            if (_overflowMenu == null)
                return;

            for (int i = 0; i < _overflowMenu.Items.Count; i++)
            {
                MenuItem mi = _overflowMenu.Items[i] as MenuItem;
                if (mi == null)
                    continue;   // this is some other menu items (fixed)
                if (mi.Tag is OutlookBarGroup)
                {
                    mi.Header = null;
                    _overflowMenu.Items.RemoveAt(i);
                    i--;
                }//end if- this menu item is OutlookBarGroup
            }//end for - remove from menu only these items that are OutlookBarGroup
        }

        #endregion //Overflow Menu Items

        #endregion //Private Methods	
        
        #endregion //Methods	
    
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