using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Infragistics.Windows.Helpers;
using System.ComponentModel;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// Control during a drag operation to allow the end user to determine where the pane should be 
	/// docked.
	/// </summary>
	[TemplatePart(Name = "PART_DockLeft", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_DockRight", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_DockTop", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_DockBottom", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_DockCenter", Type = typeof(FrameworkElement))]
	//[ToolboxItem(false)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class DockingIndicator : Control
	{
		#region Constructor
		static DockingIndicator()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DockingIndicator), new FrameworkPropertyMetadata(typeof(DockingIndicator)));
			UIElement.FocusableProperty.OverrideMetadata(typeof(DockingIndicator), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
			UIElement.IsEnabledProperty.OverrideMetadata(typeof(DockingIndicator), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsEnabledChanged)));
		}

		/// <summary>
		/// Initializes a new <see cref="DockingIndicator"/>
		/// </summary>
		public DockingIndicator()
		{
		}
		#endregion //Constructor

		#region Properties

		#region Public Properties

		#region CanDockBottom

		internal static readonly DependencyPropertyKey CanDockBottomPropertyKey =
			DependencyProperty.RegisterReadOnly("CanDockBottom",
			typeof(bool), typeof(DockingIndicator), new FrameworkPropertyMetadata(KnownBoxes.TrueBox,null, new CoerceValueCallback(CoerceCanDockBottom)));

		private static object CoerceCanDockBottom(DependencyObject d, object newValue)
		{
			return CoerceCanDockHelper(d, newValue, DockingIndicatorPosition.Bottom);
		}

		/// <summary>
		/// Identifies the <see cref="CanDockBottom"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CanDockBottomProperty =
			CanDockBottomPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating whether the dock bottom indicator should be enabled.
		/// </summary>
		/// <seealso cref="CanDockBottomProperty"/>
		//[Description("Returns a boolean indicating whether the dock bottom indicator should be enabled.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		[ReadOnly(true)]
		public bool CanDockBottom
		{
			get
			{
				return (bool)this.GetValue(DockingIndicator.CanDockBottomProperty);
			}
		}

		#endregion //CanDockBottom

		#region CanDockTop

		internal static readonly DependencyPropertyKey CanDockTopPropertyKey =
			DependencyProperty.RegisterReadOnly("CanDockTop",
			typeof(bool), typeof(DockingIndicator), new FrameworkPropertyMetadata(KnownBoxes.TrueBox, null, new CoerceValueCallback(CoerceCanDockTop)));

		private static object CoerceCanDockTop(DependencyObject d, object newValue)
		{
			return CoerceCanDockHelper(d, newValue, DockingIndicatorPosition.Top);
		}

		/// <summary>
		/// Identifies the <see cref="CanDockTop"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CanDockTopProperty =
			CanDockTopPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating whether the dock top indicator should be enabled.
		/// </summary>
		/// <seealso cref="CanDockTopProperty"/>
		//[Description("Returns a boolean indicating whether the dock top indicator should be enabled.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		[ReadOnly(true)]
		public bool CanDockTop
		{
			get
			{
				return (bool)this.GetValue(DockingIndicator.CanDockTopProperty);
			}
		}

		#endregion //CanDockTop

		#region CanDockLeft

		internal static readonly DependencyPropertyKey CanDockLeftPropertyKey =
			DependencyProperty.RegisterReadOnly("CanDockLeft",
			typeof(bool), typeof(DockingIndicator), new FrameworkPropertyMetadata(KnownBoxes.TrueBox, null, new CoerceValueCallback(CoerceCanDockLeft)));

		private static object CoerceCanDockLeft(DependencyObject d, object newValue)
		{
			return CoerceCanDockHelper(d, newValue, DockingIndicatorPosition.Left);
		}

		/// <summary>
		/// Identifies the <see cref="CanDockLeft"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CanDockLeftProperty =
			CanDockLeftPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating whether the dock left indicator should be enabled.
		/// </summary>
		/// <seealso cref="CanDockLeftProperty"/>
		//[Description("Returns a boolean indicating whether the dock left indicator should be enabled.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		[ReadOnly(true)]
		public bool CanDockLeft
		{
			get
			{
				return (bool)this.GetValue(DockingIndicator.CanDockLeftProperty);
			}
		}

		#endregion //CanDockLeft

		#region CanDockRight

		internal static readonly DependencyPropertyKey CanDockRightPropertyKey =
			DependencyProperty.RegisterReadOnly("CanDockRight",
			typeof(bool), typeof(DockingIndicator), new FrameworkPropertyMetadata(KnownBoxes.TrueBox, null, new CoerceValueCallback(CoerceCanDockRight)));

		private static object CoerceCanDockRight(DependencyObject d, object newValue)
		{
			return CoerceCanDockHelper(d, newValue, DockingIndicatorPosition.Right);
		}

		/// <summary>
		/// Identifies the <see cref="CanDockRight"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CanDockRightProperty =
			CanDockRightPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating whether the dock right indicator should be enabled.
		/// </summary>
		/// <seealso cref="CanDockRightProperty"/>
		//[Description("Returns a boolean indicating whether the dock right indicator should be enabled.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		[ReadOnly(true)]
		public bool CanDockRight
		{
			get
			{
				return (bool)this.GetValue(DockingIndicator.CanDockRightProperty);
			}
		}

		#endregion //CanDockRight

		#region CanDockCenter

		internal static readonly DependencyPropertyKey CanDockCenterPropertyKey =
			DependencyProperty.RegisterReadOnly("CanDockCenter",
			typeof(bool), typeof(DockingIndicator), new FrameworkPropertyMetadata(KnownBoxes.TrueBox, null, new CoerceValueCallback(CoerceCanDockCenter)));

		private static object CoerceCanDockCenter(DependencyObject d, object newValue)
		{
			return CoerceCanDockHelper(d, newValue, DockingIndicatorPosition.Center);
		}

		/// <summary>
		/// Identifies the <see cref="CanDockCenter"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CanDockCenterProperty =
			CanDockCenterPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating whether the dock center indicator should be enabled.
		/// </summary>
		/// <seealso cref="CanDockCenterProperty"/>
		//[Description("Returns a boolean indicating whether the dock center indicator should be enabled.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		[ReadOnly(true)]
		public bool CanDockCenter
		{
			get
			{
				return (bool)this.GetValue(DockingIndicator.CanDockCenterProperty);
			}
		}

		#endregion //CanDockCenter

		#region HotTrackPosition

		internal static readonly DependencyPropertyKey HotTrackPositionPropertyKey =
			DependencyProperty.RegisterReadOnly("HotTrackPosition",
			typeof(DockingIndicatorPosition?), typeof(DockingIndicator), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="HotTrackPosition"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HotTrackPositionProperty =
			HotTrackPositionPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the position within the indicator that is currently hottracked.
		/// </summary>
		/// <seealso cref="HotTrackPositionProperty"/>
		//[Description("Returns the position within the indicator that is currently hottracked.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		[ReadOnly(true)]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<DockingIndicatorPosition>))] // AS 5/15/08 BR32816
		public DockingIndicatorPosition? HotTrackPosition
		{
			get
			{
				return (DockingIndicatorPosition?)this.GetValue(DockingIndicator.HotTrackPositionProperty);
			}
		}

		#endregion //HotTrackPosition

		#region Position

		/// <summary>
		/// Identifies the <see cref="Position"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position",
			typeof(DockingIndicatorPosition), typeof(DockingIndicator), new FrameworkPropertyMetadata(DockingIndicatorPosition.Center, new PropertyChangedCallback(OnPositionChanged)));

		private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			d.CoerceValue(CanDockBottomProperty);
			d.CoerceValue(CanDockRightProperty);
			d.CoerceValue(CanDockTopProperty);
			d.CoerceValue(CanDockLeftProperty);
			d.CoerceValue(CanDockCenterProperty);
		}

		/// <summary>
		/// Returns or sets the position of the indicator.
		/// </summary>
		/// <seealso cref="PositionProperty"/>
		//[Description("Returns or sets the position of the indicator.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		public DockingIndicatorPosition Position
		{
			get
			{
				return (DockingIndicatorPosition)this.GetValue(DockingIndicator.PositionProperty);
			}
			set
			{
				this.SetValue(DockingIndicator.PositionProperty, value);
			}
		}

		#endregion //Position

		#endregion //Public Properties

		#endregion //Properties

		#region Methods

		#region CoerceCanDockHelper
		private static object CoerceCanDockHelper(DependencyObject d, object newValue, DockingIndicatorPosition otherPosition)
		{
			if (false.Equals(newValue))
				return newValue;

			DockingIndicatorPosition position = (DockingIndicatorPosition)d.GetValue(PositionProperty);

			if (position == DockingIndicatorPosition.Center || position == otherPosition)
			{
				// if the element is disabled, consider this to be disabled as well
				if (true.Equals(d.GetValue(FrameworkElement.IsEnabledProperty)))
					return newValue;
			}

			return KnownBoxes.FalseBox;
		}

		#endregion //CoerceCanDockHelper

		#region GetPosition
		/// <summary>
		/// Returns the indicator element that the mouse is over.
		/// </summary>
		/// <param name="point">The point to evaluate</param>
		/// <returns>The indicator position if over one or null if not over any</returns>
		internal DockingIndicatorPosition? GetPosition(Point point)
		{
			IInputElement elementFromPoint = this.InputHitTest(point);

			// if we found a point then lets figure out which position
			if (null != elementFromPoint)
			{
				switch (this.Position)
				{
					case DockingIndicatorPosition.Center:
						break;
					default:
						// if its one of the globals and we're over an element, we'll
						// assume that position. otherwise people need to have a template
						// contains all the part names for the left/right/bottom/top
						return this.Position;
				}

				DependencyObject dep = elementFromPoint as DependencyObject;

				while (dep != null)
				{
					string name = dep.GetValue(FrameworkElement.NameProperty) as string;

					if (null != name && name.StartsWith("PART_Dock"))
					{
						switch (name)
						{
							case "PART_DockLeft":
								return DockingIndicatorPosition.Left;
							case "PART_DockRight":
								return DockingIndicatorPosition.Right;
							case "PART_DockTop":
								return DockingIndicatorPosition.Top;
							case "PART_DockBottom":
								return DockingIndicatorPosition.Bottom;
							case "PART_DockCenter":
								return DockingIndicatorPosition.Center;
						}
					}

					dep = Utilities.GetParent(dep);
				}
			}

			return null;
		} 
		#endregion //GetPosition

		#region OnIsEnabledChanged
		private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			d.CoerceValue(CanDockBottomProperty);
			d.CoerceValue(CanDockLeftProperty);
			d.CoerceValue(CanDockRightProperty);
			d.CoerceValue(CanDockTopProperty);
			d.CoerceValue(CanDockCenterProperty);
		} 
		#endregion //OnIsEnabledChanged

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