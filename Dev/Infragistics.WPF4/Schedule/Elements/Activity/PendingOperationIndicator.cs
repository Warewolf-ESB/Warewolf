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
using System.Windows.Threading;
using Infragistics.AutomationPeers;
using System.Diagnostics;
using System.ComponentModel;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Custom control that indicates that an operation is pending
	/// </summary>
	[TemplateVisualState(Name = VisualStateUtilities.StateActive, GroupName = VisualStateUtilities.GroupActive)]
	[TemplateVisualState(Name = VisualStateUtilities.StateInactive, GroupName = VisualStateUtilities.GroupActive)]
	[DesignTimeVisible(false)]
	public class PendingOperationIndicator : Control
	{
		#region Member Variables

		#endregion //Member Variables

		#region Constructor
		static PendingOperationIndicator()
		{

			PendingOperationIndicator.DefaultStyleKeyProperty.OverrideMetadata(typeof(PendingOperationIndicator), new FrameworkPropertyMetadata(typeof(PendingOperationIndicator)));
			UIElement.FocusableProperty.OverrideMetadata(typeof(PendingOperationIndicator), new FrameworkPropertyMetadata(KnownBoxes.FalseBox)); // AS 12/16/10 TFS61923

		}

		/// <summary>
		/// Initializes a new <see cref="PendingOperationIndicator"/>
		/// </summary>
		public PendingOperationIndicator()
		{



		}
		#endregion //Constructor

		#region Base class overrides

		#region OnApplyTemplate

		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this.ChangeVisualState(false);
		}

		#endregion //OnApplyTemplate

		#endregion //Base class overrides	
    
		#region Properties

		#region Public Properties

		#region IsActive

		/// <summary>
		/// Identifies the <see cref="IsActive"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsActiveProperty = DependencyPropertyUtilities.Register("IsActive",
			typeof(bool), typeof(PendingOperationIndicator),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsActiveChanged))
			);

		private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			PendingOperationIndicator instance = (PendingOperationIndicator)d;

			instance.UpdateVisualState();
		}

		/// <summary>
		/// Returns or sets whether the indicator is active
		/// </summary>
		/// <seealso cref="IsActiveProperty"/>
		public bool IsActive
		{
			get
			{
				return (bool)this.GetValue(PendingOperationIndicator.IsActiveProperty);
			}
			set
			{
				this.SetValue(PendingOperationIndicator.IsActiveProperty, value);
			}
		}

		#endregion //IsActive

		#endregion //Public Properties	
    
		#endregion //Properties	

		#region Methods

		#region Private Methods

		#region ChangeVisualState
		private void ChangeVisualState(bool useTransitions)
		{
			if (this.IsActive)
				VisualStateManager.GoToState(this, VisualStateUtilities.StateActive, useTransitions);
			else
				VisualStateManager.GoToState(this, VisualStateUtilities.StateInactive, useTransitions);

		}
		#endregion //ChangeVisualState

		#region UpdateVisualState

		private void UpdateVisualState()
		{
			
			this.ChangeVisualState(true);
		}
		#endregion // UpdateVisualState

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