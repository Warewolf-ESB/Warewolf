using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.Controls
{
	// SSP 3/12/10 TFS27090
	// 
	/// <summary>
	/// A decorator class whose desired size only grows.
	/// </summary>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class GrowOnlyDecorator : Decorator
	{
		#region Member Vars

		private Size _lastDesiredSize;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Initializes a new <see cref="GrowOnlyDecorator"/>
		/// </summary>
		public GrowOnlyDecorator( )
		{
		}

		#endregion // Constructor

		#region Base class overrides

		#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride( Size availableSize )
		{
			Size desired = base.MeasureOverride( availableSize );

			if ( this.ResetSize )
			{
				this.ResetSize = false;
				_lastDesiredSize = Size.Empty;
			}

			if ( this.GrowWidth )
			{
				if ( !double.IsNaN( _lastDesiredSize.Width ) && ( double.IsNaN( desired.Width ) || desired.Width < _lastDesiredSize.Width ) )
					desired.Width = _lastDesiredSize.Width;

				_lastDesiredSize.Width = desired.Width;
			}

			if ( this.GrowHeight )
			{
				if ( !double.IsNaN( _lastDesiredSize.Height ) && ( double.IsNaN( desired.Height ) || desired.Height < _lastDesiredSize.Height ) )
					desired.Height = _lastDesiredSize.Height;

				_lastDesiredSize.Height = desired.Height;
			}

			return desired;
		}

		#endregion // MeasureOverride

		#endregion // Base class overrides

		#region Properties

		#region Public Properties

		#region GrowHeight

		/// <summary>
		/// Identifies the <see cref="GrowHeight"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty GrowHeightProperty = DependencyProperty.Register(
			"GrowHeight",
			typeof( bool ),
			typeof( GrowOnlyDecorator ),
			new FrameworkPropertyMetadata( KnownBoxes.TrueBox, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Specifies whether to grow height. Default value is true.
		/// </summary>
		[Bindable( true )]
		public bool GrowHeight
		{
			get
			{
				return (bool)this.GetValue( GrowHeightProperty );
			}
			set
			{
				this.SetValue( GrowHeightProperty, value );
			}
		}

		#endregion // GrowHeight

		#region GrowWidth

		/// <summary>
		/// Identifies the <see cref="GrowWidth"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty GrowWidthProperty = DependencyProperty.Register(
			"GrowWidth",
			typeof( bool ),
			typeof( GrowOnlyDecorator ),
			new FrameworkPropertyMetadata( KnownBoxes.TrueBox, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Specifies whether to grow width. Default value is true.
		/// </summary>
		[Bindable( true )]
		public bool GrowWidth
		{
			get
			{
				return (bool)this.GetValue( GrowWidthProperty );
			}
			set
			{
				this.SetValue( GrowWidthProperty, value );
			}
		}

		#endregion // GrowWidth

		#region ResetSize

		/// <summary>
		/// Identifies the <see cref="ResetSize"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ResetSizeProperty = DependencyProperty.Register(
			"ResetSize",
			typeof( bool ),
			typeof( GrowOnlyDecorator ),
			new FrameworkPropertyMetadata( KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Resets the desired size so next time the element is measured, it will return
		/// desired size based on the contents.
		/// </summary>
		[Bindable( false )]
		public bool ResetSize
		{
			get
			{
				return (bool)this.GetValue( ResetSizeProperty );
			}
			set
			{
				this.SetValue( ResetSizeProperty, value );
			}
		}

		#endregion // ResetSize

		#endregion // Public Properties

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