using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;






using Infragistics.Shared;

namespace Infragistics.Documents.Excel

{
	/// <summary>
	/// Class which exposes the worksheet display options which can only be controlled through the custom view.
	/// </summary>
	/// <seealso cref="CustomView.GetDisplayOptions"/>
	/// <seealso cref="WorksheetDisplayOptions"/>



	public

		 class CustomViewDisplayOptions : DisplayOptions
	{
		#region Member Variables

		private int magnificationInCurrentView = DisplayOptions.DefaultMagnificationInNormalView;

		#endregion Member Variables

		#region Constructors

		internal CustomViewDisplayOptions( Worksheet worksheet )
			: base( worksheet ) { }

		#endregion Constructors

		#region Base Class Overrides

		#region InitializeFrom






		internal override void InitializeFrom( DisplayOptions displayOptions )
		{
			base.InitializeFrom( displayOptions );

			WorksheetDisplayOptions worksheetOptions = displayOptions as WorksheetDisplayOptions;

			// If the display options is a worksheet window options, we need to initialize the current view 
			// magnification based on the current view
			if ( worksheetOptions != null )
			{
				switch ( worksheetOptions.View )
				{
					case WorksheetView.Normal:
						this.magnificationInCurrentView = worksheetOptions.MagnificationInNormalView;
						break;

					case WorksheetView.PageBreakPreview:
						this.magnificationInCurrentView = worksheetOptions.MagnificationInPageBreakView;
						break;

					case WorksheetView.PageLayout:
						this.magnificationInCurrentView = worksheetOptions.MagnificationInPageLayoutView;
						break;

					default:
						Utilities.DebugFail( "Unknown view type" );
						break;
				}

				return;
			}

			CustomViewDisplayOptions customViewOptions = displayOptions as CustomViewDisplayOptions;

			// If the display options is a custom view display options, just copy over the members
			if ( customViewOptions != null )
			{
				this.magnificationInCurrentView = customViewOptions.magnificationInCurrentView;
				return;
			}

			Utilities.DebugFail( "Unknown display options" );
		}

		#endregion InitializeFrom

		#region Reset

		/// <summary>
		/// Resets the display options to their default settings.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The defaults used for each setting are the same defaults with which Microsoft Excel creates a blank worksheet.
		/// </p>
		/// </remarks>
		public override void Reset()
		{
			base.Reset();

			this.magnificationInCurrentView = DisplayOptions.DefaultMagnificationInNormalView;
		}

		#endregion Reset

		#endregion Base Class Overrides

		#region Properties

		#region MagnificationInCurrentView

		/// <summary>
		/// Gets or sets the magnification level of the worksheet in the current <see cref="DisplayOptions.View"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Magnifications are stored as percentages of the normal viewing magnification. A value of 100 indicates normal magnification
		/// whereas a value of 200 indicates a zoom that is twice the normal viewing magnification.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The assigned value is outside the valid range of magnification levels for a worksheet. The level must be between 10 and 400.
		/// </exception>
		/// <value>The magnification level of the worksheet in the current View.</value>
		/// <seealso cref="WorksheetDisplayOptions.MagnificationInNormalView"/>
		/// <seealso cref="WorksheetDisplayOptions.MagnificationInPageBreakView"/>
		/// <seealso cref="WorksheetDisplayOptions.MagnificationInPageLayoutView"/>
		public int MagnificationInCurrentView
		{
			get { return this.magnificationInCurrentView; }
			set
			{
				if ( this.magnificationInCurrentView != value )
				{
					if ( value < 10 || 400 < value )
						throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_MagnificationLevel" ) );

					this.magnificationInCurrentView = value;
				}
			}
		}

		#endregion MagnificationInCurrentView

		#endregion Properties
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