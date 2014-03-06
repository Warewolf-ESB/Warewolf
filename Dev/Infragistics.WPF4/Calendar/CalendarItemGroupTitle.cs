using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;

using System.Windows.Automation.Peers;
using Infragistics.AutomationPeers;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Input;
using Infragistics.Controls.Primitives;

namespace Infragistics.Controls.Editors.Primitives
{
	/// <summary>
	/// Represents the header for a specific <see cref="CalendarItemGroup"/> in a <see cref="CalendarBase"/>
	/// </summary>
    //[System.ComponentModel.ToolboxItem(false)]
    [TemplateVisualState(Name = VisualStateUtilities.StateNormal,              GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateMouseOver,           GroupName = VisualStateUtilities.GroupCommon)]

	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!

	public class CalendarItemGroupTitle : ContentControl, IResourceProviderClient
	{

		#region Private Members

		private bool _isMouseOver;
		private bool _isInitialized;

		private bool _hasVisualStateGroups;

		private CalendarItemGroup _group;

		#endregion //Private Members	
    
		#region Constructor

		static CalendarItemGroupTitle()
		{

			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarItemGroupTitle), new FrameworkPropertyMetadata(typeof(CalendarItemGroupTitle)));
			//UIElement.FocusableProperty.OverrideMetadata(typeof(CalendarItemGroupTitle), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
			//Control.HorizontalContentAlignmentProperty.OverrideMetadata(typeof(CalendarItemGroupTitle), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentCenterBox));
			//Control.VerticalContentAlignmentProperty.OverrideMetadata(typeof(CalendarItemGroupTitle), new FrameworkPropertyMetadata(KnownBoxes.VerticalAlignmentCenterBox));

        }

		/// <summary>
		/// Initializes a new <see cref="CalendarItemGroupTitle"/>
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This constructor is only used for styling purposes. At runtime, the controls are automatically generated.</p>
		/// </remarks>
		public CalendarItemGroupTitle()
		{



		}
		#endregion //Constructor

        #region Base class overrides

		#region OnApplyTemplate

		/// <summary>
		/// Called when the template is applied.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// OnApplyTemplate is a .NET framework method exposed by the FrameworkElement. This class overrides
		/// it to get the focus site from the control template whenever template gets applied to the control.
		/// </p>
		/// </remarks>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this._isInitialized = true;

			this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

			this._group = PresentationUtilities.GetTemplatedParent(this) as CalendarItemGroup;

			this.Group = this._group;

			if (_group != null)
				_group.RegisterTitle(this);

			this.UpdateVisualStates(false);

		}

		#endregion //OnApplyTemplate

        #region OnCreateAutomationPeer
        /// <summary>
        /// Returns an <see cref="AutomationPeer"/> that represents the element
        /// </summary>
        /// <returns>A <see cref="CalendarItemGroupTitleAutomationPeer"/> instance</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new CalendarItemGroupTitleAutomationPeer(this);
        }
        #endregion //OnCreateAutomationPeer

		#region OnMouseEnter
		/// <summary>
		/// Invoked when the mouse is moved within the bounds of the element.
		/// </summary>
		/// <param name="e">Provides information about the mouse position.</param>
		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			_isMouseOver = true;

			this.UpdateVisualStates();

		}
		#endregion //OnMouseEnter

		#region OnMouseLeave
		/// <summary>
		/// Invoked when the mouse is moved outside the bounds of the element.
		/// </summary>
		/// <param name="e">Provides information about the mouse position.</param>
		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			_isMouseOver = false;

			this.UpdateVisualStates();

		}
		#endregion //OnMouseLeave

        #endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region ComputedBackground

		private static readonly DependencyPropertyKey ComputedBackgroundPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedBackground",
			typeof(Brush), typeof(CalendarItemGroupTitle), new SolidColorBrush(Colors.Transparent), null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedBackground"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedBackgroundProperty = ComputedBackgroundPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the brush to use for the background based on the element's state.
		/// </summary>
		/// <seealso cref="ComputedBackgroundProperty"/>
		public Brush ComputedBackground
		{
			get
			{
				return (Brush)this.GetValue(CalendarItemGroupTitle.ComputedBackgroundProperty);
			}
			internal set
			{
				this.SetValue(CalendarItemGroupTitle.ComputedBackgroundPropertyKey, value);
			}
		}

		#endregion //ComputedBackground

		#region ComputedForeground

		private static readonly DependencyPropertyKey ComputedForegroundPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedForeground",
			typeof(Brush), typeof(CalendarItemGroupTitle), new SolidColorBrush(Colors.Black), null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedForeground"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedForegroundProperty = ComputedForegroundPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the brush to use for the Foreground based on the element's state
		/// </summary>
		/// <seealso cref="ComputedForegroundProperty"/>
		public Brush ComputedForeground
		{
			get
			{
				return (Brush)this.GetValue(CalendarItemGroupTitle.ComputedForegroundProperty);
			}
			internal set
			{
				this.SetValue(CalendarItemGroupTitle.ComputedForegroundPropertyKey, value);
			}
		}

		#endregion //ComputedForeground

		#region Group

		private static readonly DependencyPropertyKey GroupPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("Group",
			typeof(CalendarItemGroup), typeof(CalendarItemGroupTitle), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="Group"/> dependency property
		/// </summary>
		public static readonly DependencyProperty GroupProperty = GroupPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the associated <see cref="CalendarItemGroup"/> (read-only)
		/// </summary>
		/// <seealso cref="GroupProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		public CalendarItemGroup Group
		{
			get
			{
				return _group;
			}
			private set
			{
				this.SetValue(CalendarItemGroupTitle.GroupPropertyKey, value);
			}
		}

		#endregion //Group
		
		#endregion //Public Properties

		#endregion //Properties

		#region Methods

		#region Private Methods

		// JJD 07/19/12 - TFS108812
		#region ClearGroup

		internal void ClearGroup()
		{
			_group = null;
			this.ClearValue(GroupPropertyKey);

		}

		#endregion //ClearGroup	
    
		#region SetProviderBrushes

		internal virtual void SetProviderBrushes()
		{
			if (this._group == null)
				return;

			CalendarBase cal = CalendarBase.GetCalendar(this._group);

			if (cal == null)
				return;

			CalendarResourceProvider rp = cal.ResourceProviderResolved;

			if (rp == null)
				return;

			CalendarResourceId idBackground;
			CalendarResourceId idForeground;
			
			if (_isMouseOver == true)
			{
				idBackground = CalendarResourceId.MouseOverGroupTitleBackgroundBrush;
				idForeground = CalendarResourceId.MouseOverGroupTitleForegroundBrush;
			}
			else
			{
				idBackground = CalendarResourceId.GroupTitleBackgroundBrush;
				idForeground = CalendarResourceId.GroupTitleForegroundBrush;
			}

			this.ComputedBackground = rp[idBackground] as Brush;
			this.ComputedForeground = rp[idForeground] as Brush;
		}

		#endregion //SetProviderBrushes

		#region VisualState... Methods

		/// <summary>
		/// Called to set the VisualStates of the editor
		/// </summary>
		/// <param name="useTransitions">Determines whether transitions should be used.</param>
		protected virtual void SetVisualState(bool useTransitions)
		{

			// Set Common states
			if (this._isMouseOver)
				VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
			else
				VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);

			this.SetProviderBrushes();

		}

		// JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
		internal static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			CalendarItemGroupTitle calItem = target as CalendarItemGroupTitle;

			if (calItem != null)
				calItem.UpdateVisualStates();
		}

		// JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
		/// <summary>
		/// Called to set the visual states of the control
		/// </summary>
		protected void UpdateVisualStates()
		{
			this.UpdateVisualStates(true);
		}

		/// <summary>
		/// Called to set the visual states of the control
		/// </summary>
		/// <param name="useTransitions">Determines whether transitions should be used.</param>
		protected void UpdateVisualStates(bool useTransitions)
		{

			if (false == this._hasVisualStateGroups)
			{
				this.SetProviderBrushes();
				return;
			}


			if (!this._isInitialized)
				useTransitions = false;

			this.SetVisualState(useTransitions);
		}

		#endregion //VisualState... Methods

		#endregion //Private Methods	
    
		#endregion //Methods	
	
		#region IResourceProviderClient Members

		void IResourceProviderClient.OnResourcesChanged()
		{
			this.SetProviderBrushes();
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