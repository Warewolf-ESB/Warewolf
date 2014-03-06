using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Infragistics.Controls.Menus;
using System.ComponentModel;

namespace Infragistics.Controls.Grids.Primitives
{
    /// <summary>
    /// A specialized <see cref="XamMenuItem"/> which contains elements necessary for the <see cref="FilterSelectionControl"/>.
    /// </summary>
    public class FilterMenuXamMenuItem : XamMenuItem, INotifyPropertyChanged
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterMenuXamMenuItem"/> class.
        /// </summary>
        public FilterMenuXamMenuItem()
        {
            this.DefaultStyleKey = typeof(FilterMenuXamMenuItem);
        }

        #endregion Constructor

        #region Overrides

        #region GetContainerForItemOverride
        /// <summary>
        /// Creates a new XamMenuItem to use to display the object.
        /// </summary>
        /// <returns>A new XamMenuItem.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            FilterMenuXamMenuItem item = null;
            if (this.DefaultItemsContainer != null)
            {
                item = this.DefaultItemsContainer.LoadContent() as FilterMenuXamMenuItem;
            }

            if (item == null)
                return new FilterMenuXamMenuItem();
            else
                return item;
        }
        #endregion

        #region ChangeVisualState

        /// <summary>
        /// Sets the VisualStates on the control to represent the current settings.
        /// </summary>
        /// <param name="useTransitions"></param>
        protected override void ChangeVisualState(bool useTransitions)
        {
            base.ChangeVisualState(useTransitions);

            if (this.IsSeparator)
            {
                VisualStateManager.GoToState(this, "IsSeperator", useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, "IsNotSeperator", useTransitions);
            }
        }

        #endregion // ChangeVisualState

        #region OnPropertyChanged

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="name"></param>
        protected override void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);
            if (name == "ParentItemsControl")
            {
                this.OnPropertyChanged("ParentMenu");
            }
        }

        #endregion // OnPropertyChanged

        #region UpdateRoleState
        /// <summary>
        /// Based on the Role of the <see cref="XamMenuItem"/> sets up the visual states of the control.
        /// </summary>
        protected override void UpdateRoleState()
        {
            base.UpdateRoleState();

            if (this.Role == Infragistics.Controls.Menus.MenuItemRole.SubmenuHeader)
                this.SetValue(XamMenuItem.CheckBoxVisibilityResolvedProperty, Visibility.Visible);

            if (this.Role == Infragistics.Controls.Menus.MenuItemRole.TopLevelHeader)
                this.SetValue(XamMenuItem.CheckBoxVisibilityResolvedProperty, Visibility.Collapsed);
        }
        #endregion // UpdateRoleState

        #endregion // Overrides

        #region Properties

        #region IsSeparator

        /// <summary>
        /// Identifies the <see cref="IsSeparator"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsSeparatorProperty = DependencyProperty.Register("IsSeparator", typeof(bool), typeof(FilterMenuXamMenuItem), new PropertyMetadata(new PropertyChangedCallback(IsSeperatorChanged)));

        /// <summary>
        /// Gets / sets if this <see cref="FilterMenuXamMenuItem"/> is a visual separator.
        /// </summary>
        public bool IsSeparator
        {
            get { return (bool)this.GetValue(IsSeparatorProperty); }
            set { this.SetValue(IsSeparatorProperty, value); }
        }

        private static void IsSeperatorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FilterMenuXamMenuItem ctrl = (FilterMenuXamMenuItem)obj;
            ctrl.OnPropertyChanged("IsSeparator");
            ctrl.IsHitTestVisible = !ctrl.IsSeparator;
            ctrl.IsTabStop = !ctrl.IsSeparator;
            ctrl.IsKeyboardNavigable = !ctrl.IsSeparator;
            ctrl.ChangeVisualState(false);
        }

        #endregion // IsSeparator

        #region Cell

        /// <summary>
        /// Identifies the <see cref="Cell"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CellProperty = DependencyProperty.Register("Cell", typeof(CellBase), typeof(FilterMenuXamMenuItem), new PropertyMetadata(new PropertyChangedCallback(CellChanged)));

        /// <summary>
        /// Gets / sets the <see cref="CellBase"/> that this <see cref="FilterMenuXamMenuItem"/> will associate against.
        /// </summary>
        public CellBase Cell
        {
            get { return (CellBase)this.GetValue(CellProperty); }
            set { this.SetValue(CellProperty, value); }
        }

        private static void CellChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FilterMenuXamMenuItem ctrl = (FilterMenuXamMenuItem)obj;
            ctrl.OnPropertyChanged("Cell");

            if (ctrl.Cell != null && ctrl.DataContext != null)
            {
                FilterMenuTrackingObject fmto = ctrl.DataContext as FilterMenuTrackingObject;
                if (fmto != null)
                {
                    // invalidate checkbox

                }
            }
        }

        #endregion // Cell

        #region ParentMenu
        /// <summary>
        /// Gets a reference to the parent XamMenu of the XamMenuItem.
        /// </summary>
        public XamMenuBase ParentMenu
        {
            get
            {
                return this.ParentXamMenu;
            }
        }
        #endregion // ParentMenu

        #endregion // Properties
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