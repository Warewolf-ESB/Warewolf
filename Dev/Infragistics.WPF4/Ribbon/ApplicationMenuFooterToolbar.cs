using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.Ribbon;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Displays the toolbar that is positioned at the bottom of the application menu.
	/// </summary>
	/// <seealso cref="XamRibbon"/>
	/// <seealso cref="ApplicationMenu"/>
	/// <seealso cref="ApplicationMenu.FooterToolbar"/>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class ApplicationMenuFooterToolbar : ToolbarBase, IRibbonToolLocation
	{
		#region Private Members

		private ApplicationMenu _appMenu;

		#endregion //Private Members	
    
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ApplicationMenuFooterToolbar"/> class.
		/// </summary>
		public ApplicationMenuFooterToolbar()
		{
		}

		static ApplicationMenuFooterToolbar()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ApplicationMenuFooterToolbar), new FrameworkPropertyMetadata(typeof(ApplicationMenuFooterToolbar)));

			FrameworkElementFactory fefPanel = new FrameworkElementFactory(typeof(StackPanel));
			fefPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

			ItemsPanelTemplate template = new ItemsPanelTemplate(fefPanel);
			template.Seal();
			ItemsControl.ItemsPanelProperty.OverrideMetadata(typeof(ApplicationMenuFooterToolbar), new FrameworkPropertyMetadata(template));
		}

		#endregion //Constructor

		#region Base class overrides
        
            #region OnCreateAutomationPeer
        /// <summary>
        /// Returns an automation peer that exposes the <see cref="ApplicationMenuFooterToolbar"/> to UI Automation.
        /// </summary>
        /// <returns>A <see cref="Infragistics.Windows.Automation.Peers.Ribbon.ToolbarBaseAutomationPeer"/></returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ToolbarBaseAutomationPeer(this);
        }
            #endregion

			#region PrepareContainerForItemOverride

		/// <summary>
		/// Prepares the container to 'host' the item.
		/// </summary>
		/// <param name="element">The container that wraps the item.</param>
		/// <param name="item">The data item that is wrapped.</param>
		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			base.PrepareContainerForItemOverride(element, item);
		}

			#endregion //PrepareContainerForItemOverride	

		#endregion //Base class overrides

		#region Methods

			#region Internal Methods

				#region Initialize

		internal void Initialize(ApplicationMenu appmenu)
		{
			this._appMenu = appmenu;
		}

				#endregion //Initialize	
    
			#endregion //Internal Methods	
    
		#endregion //Methods	
 		
		#region IRibbonToolLocation Members

		ToolLocation IRibbonToolLocation.Location
		{
			get { return ToolLocation.ApplicationMenuFooterToolbar; }
		}

		#endregion
   
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