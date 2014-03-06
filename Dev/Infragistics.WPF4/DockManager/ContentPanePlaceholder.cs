using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Infragistics.Windows.Helpers;
using System.ComponentModel;
using System.Windows.Threading;
using System.Diagnostics;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// Element used to maintain the place for a pane when its state is changed (e.g. Docked to Floating).
	/// </summary>
	//[ToolboxItem(false)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class ContentPanePlaceholder : FrameworkElement
	{
		#region Member Variables

		private ContentPane _pane;
		private IContentPaneContainer _container;

		#endregion //Member Variables

		#region Constructor

		static ContentPanePlaceholder()
		{
			UIElement.VisibilityProperty.OverrideMetadata(typeof(ContentPanePlaceholder), new FrameworkPropertyMetadata(KnownBoxes.VisibilityCollapsedBox, null, new CoerceValueCallback(CoerceVisibility)));

			// AS 3/26/10 TFS30153 - DockManager Optimization
			XamDockManager.DockManagerPropertyKey.OverrideMetadata(typeof(ContentPanePlaceholder), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDockManagerChanged)));
		}

		/// <summary>
		/// Initializes a new <see cref="ContentPanePlaceholder"/>
		/// </summary>
		public ContentPanePlaceholder()
		{
		} 
		#endregion //Constructor

		#region Base class overrides

		#region ToString
		/// <summary>
		/// Returns a string representation of the object.
		/// </summary>
		public override string ToString()
		{
			if (this.Pane == null)
				return "ContentPanePlaceholder";

			return "ContentPanePlaceholder: " + this.Pane.ToString();
		}
		#endregion //ToString 

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region PaneLocation

		/// <summary>
		/// Identifies the <see cref="PaneLocation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PaneLocationProperty = XamDockManager.PaneLocationProperty.AddOwner(typeof(ContentPanePlaceholder));

		/// <summary>
		/// Returns the current location of the pane.
		/// </summary>
		/// <seealso cref="PaneLocationProperty"/>
		//[Description("Description")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		[ReadOnly(true)]
		public PaneLocation PaneLocation
		{
			get
			{
				return (PaneLocation)this.GetValue(ContentPane.PaneLocationProperty);
			}
		}
		#endregion //PaneLocation

		#endregion //Public Properties

		#region Internal Properties

		#region Container
		internal IContentPaneContainer Container
		{
			get { return this._container; }
			set { this._container = value; }
		}
		#endregion //Container

		#region Pane
		// AS 7/15/09 TFS18453
		// This isn't directly related to the bug but some requests have come up in the past to be able 
		// to know what ContentPane is associated with the placeholder. There is no reason that I can think 
		// of to not expose this.
		//
		//internal ContentPane Pane
		/// <summary>
		/// Returns the associated <see cref="ContentPane"/> for which this placeholder is associated.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ContentPane Pane
		{
			get { return this._pane; }
		} 
		#endregion //Pane

		#endregion //Internal Properties 

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region Initialize
		internal void Initialize(ContentPane pane)
		{
			this._pane = pane;
		} 
		#endregion //Initialize

		#region IsPane
		internal bool IsPane(ContentPane pane)
		{
			
			return pane == this._pane;
		} 
		#endregion //IsPane

		#endregion //Internal Methods 

		#region Private Methods

		#region CoerceVisibility
		private static object CoerceVisibility(DependencyObject d, object newValue)
		{
			// the placeholder is always collapsed
			return KnownBoxes.VisibilityCollapsedBox;
		}
		#endregion //CoerceVisibility 

		// AS 3/26/10 TFS30153 - DockManager Optimization
		#region OnDockManagerChanged
		private static void OnDockManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// AS 7/15/09 TFS18453
			// This isn't directly related to the issue but if someone removes a placeholder the 
			// contentpane may still retain a reference to it. We should clean that up as well.
			//
			ContentPanePlaceholder placeholder = d as ContentPanePlaceholder;

			// this came up in the ribbon and could come up. when something is in the logical
			// tree but gets removed from teh visual tree, we could get an invalid property
			// change notification. see the ribbon's OnRibbonChanged for more details.
			if (XamDockManager.GetDockManager(placeholder) != e.NewValue)
				return;

			XamDockManager oldDockManager = e.OldValue as XamDockManager;
			XamDockManager newDockManager = e.NewValue as XamDockManager;

			if (null != oldDockManager)
			{
				// AS 7/17/09 TFS18453/TFS19568
				//placeholder.VerifyContentPaneAfterDelay();
				oldDockManager.ActivePaneManager.OnPlaceholderRemoved(placeholder);
			}

			// AS 7/17/09 TFS18453/TFS19568
			if (null != newDockManager)
				newDockManager.ActivePaneManager.OnPlaceholderAdded(placeholder);
		}
		#endregion //OnDockManagerChanged

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