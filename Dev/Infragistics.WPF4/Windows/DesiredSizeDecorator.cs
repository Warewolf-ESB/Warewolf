using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;

namespace Infragistics.Windows.Controls
{
	// AS 7/13/09 TFS18489
	/// <summary>
	/// Custom class that exposes the desired size of the child via dependency properties.
	/// </summary>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class DesiredSizeDecorator : Decorator
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="DesiredSizeDecorator"/>
		/// </summary>
		public DesiredSizeDecorator()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region MeasureOverride
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			Size desired = base.MeasureOverride(availableSize);

			this.SetValue(ChildDesiredHeightPropertyKey, desired.Height);
			this.SetValue(ChildDesiredWidthPropertyKey, desired.Width);

			return desired;
		}
		#endregion //MeasureOverride

		#endregion //Base class overrides

		#region Properties

		#region ChildDesiredHeight

		private static readonly DependencyPropertyKey ChildDesiredHeightPropertyKey =
			DependencyProperty.RegisterReadOnly("ChildDesiredHeight",
			typeof(double), typeof(DesiredSizeDecorator), new FrameworkPropertyMetadata(double.NaN));

		/// <summary>
		/// Identifies the <see cref="ChildDesiredHeight"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ChildDesiredHeightProperty =
			ChildDesiredHeightPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the desired height of the child.
		/// </summary>
		/// <seealso cref="ChildDesiredHeightProperty"/>
		//[Description("Returns the desired height of the child.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public double ChildDesiredHeight
		{
			get
			{
				return (double)this.GetValue(DesiredSizeDecorator.ChildDesiredHeightProperty);
			}
		}

		#endregion //ChildDesiredHeight

		#region ChildDesiredWidth

		private static readonly DependencyPropertyKey ChildDesiredWidthPropertyKey =
			DependencyProperty.RegisterReadOnly("ChildDesiredWidth",
			typeof(double), typeof(DesiredSizeDecorator), new FrameworkPropertyMetadata(double.NaN));

		/// <summary>
		/// Identifies the <see cref="ChildDesiredWidth"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ChildDesiredWidthProperty =
			ChildDesiredWidthPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the desired width of the child.
		/// </summary>
		/// <seealso cref="ChildDesiredWidthProperty"/>
		//[Description("Returns the desired width of the child.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public double ChildDesiredWidth
		{
			get
			{
				return (double)this.GetValue(DesiredSizeDecorator.ChildDesiredWidthProperty);
			}
		}

		#endregion //ChildDesiredWidth

		#endregion //Properties
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