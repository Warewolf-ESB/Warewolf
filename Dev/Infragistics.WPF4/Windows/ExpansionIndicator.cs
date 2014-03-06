using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Text;
using System.Diagnostics;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Runtime.CompilerServices;
using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.Controls
{
	/// <summary>
	/// A ToggleButton derived element used in trees and hierarchical grids to display and/or change the expanded state of nodes.
	/// </summary>
	//[Description("A ToggleButton derived element used in trees and hierarchical grids to display and/or change the expanded state of nodes.")]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class ExpansionIndicator : ToggleButton
	{
		#region Constructor
		static ExpansionIndicator()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ExpansionIndicator), new FrameworkPropertyMetadata(typeof(ExpansionIndicator)));
			FrameworkElement.FocusableProperty.OverrideMetadata(typeof(ExpansionIndicator), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

			// AS 2/25/11 TFS66934
			// The recordpresenter will now clear the command itself and hook the click so it doesn't 
			// cause the routed events even when it is visible.
			//
			//// AS 7/9/10 TFS35642 
			//FrameworkElement.VisibilityProperty.OverrideMetadata(typeof(ExpansionIndicator), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisibilityChanged)));
			//ExpansionIndicator.CommandProperty.OverrideMetadata(typeof(ExpansionIndicator), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceCommand)));
		}

		/// <summary>
		/// Initializes a new <see cref="ExpansionIndicator"/>
		/// </summary>
		public ExpansionIndicator()
		{
		} 
		#endregion //Constructor

		#region Base class overrides

		#region OnToggle
		/// <summary>
		/// Used to toggle the <see cref="ToggleButton.IsChecked"/> property of the expansion indicator.
		/// </summary>
		/// <remarks>
		/// <p class="body">If the <see cref="ToggleMode"/> is set to Manual, this method will have no effect.</p>
		/// </remarks>
		protected override void OnToggle()
		{
			if (this.ToggleMode == ExpansionIndicatorToggleMode.Manual)
				return;

			base.OnToggle();
		}
		#endregion //OnToggle

		#endregion //Base class overrides

		#region Properties

		#region ToggleMode

		/// <summary>
		/// Identifies the <see cref="ToggleMode"/> dependency property
		/// </summary>
		[InfragisticsFeatureAttribute(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ClipboardSupport)]
		public static readonly DependencyProperty ToggleModeProperty = DependencyProperty.Register("ToggleMode",
			typeof(ExpansionIndicatorToggleMode), typeof(ExpansionIndicator), new FrameworkPropertyMetadata(ExpansionIndicatorToggleMode.Automatic));

		/// <summary>
		/// Returns or sets whether the <see cref="ToggleButton.IsChecked"/> state will be toggle automatically when clicked or only programatically
		/// </summary>
		/// <seealso cref="ToggleModeProperty"/>
		//[Description("Returns or sets whether the 'IsChecked' state will be toggle automatically when clicked or only programatically")]
		//[Category("Behavior")]
		[Bindable(true)]
		[InfragisticsFeatureAttribute(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ClipboardSupport)]
		public ExpansionIndicatorToggleMode ToggleMode
		{
			get
			{
				return (ExpansionIndicatorToggleMode)this.GetValue(ExpansionIndicator.ToggleModeProperty);
			}
			set
			{
				this.SetValue(ExpansionIndicator.ToggleModeProperty, value);
			}
		}

		#endregion //ToggleMode

		#endregion //Properties

		#region Methods

		// AS 7/9/10 TFS35642 
		#region CoerceCommand
		// AS 2/25/11 TFS66934
		//private static object CoerceCommand(DependencyObject d, object newValue)
		//{
		//    if (newValue != null)
		//    {
		//        // since routed commands have some performance implications (i.e. the canexecute
		//        // has to be routed up the element tree to find out if the element should be 
		//        // enabled). note if it becomes an issue that the element is now enabled we can 
		//        // track that we have coerced the value and then override the IsEnabledCore  
		//        // and return false if we have coerced the command to null. we would just have to 
		//        // coerce the isenabled
		//        if (!Visibility.Visible.Equals(d.GetValue(FrameworkElement.VisibilityProperty)))
		//            return null;
		//    }
		//
		//    return newValue;
		//}
		#endregion //CoerceCommand

		// AS 7/9/10 TFS35642 
		#region OnVisibilityChanged
		// AS 2/25/11 TFS66934
		//private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		//{
		//    d.CoerceValue(ExpansionIndicator.CommandProperty);
		//}
		#endregion //OnVisibilityChanged

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