using System;
using System.Collections.Generic;
using System.Text;





using System.Drawing;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Represents the window options which are saved with the workbook.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// The properties explicitly defined on this class and not inherited from the base class, 
	/// <see cref="WindowOptions"/>, are options that can be saved with a workbook, but 
	/// not with a custom view.
	/// </p>
	/// </remarks>
	/// <seealso cref="Workbook.WindowOptions"/>
	/// <seealso cref="CustomViewWindowOptions"/>



	public

		 class WorkbookWindowOptions : WindowOptions
	{
		#region Member Variables

		private Rectangle boundsInTwips = new Rectangle( 1000, 1000, 15000, 10000 );
		private int firstVisibleTabIndex;
		private bool minimized;
		private int selectedWorksheetIndex;
		
		#endregion Member Variables

		#region Constructor

		internal WorkbookWindowOptions( Workbook workbook )
			: base( workbook ) { }

		#endregion Constructor

		#region Base Class Overrides

		#region AllowNullSelectedWorksheet






		internal override bool AllowNullSelectedWorksheet
		{
			// The selected worksheet can only be null if there are no worksheets
			get { return this.Workbook.Worksheets.Count == 0; }
		}

		#endregion AllowNullSelectedWorksheet

		#region GetDisplayOptionsForWorksheet

		internal override DisplayOptions GetDisplayOptionsForWorksheet( Worksheet worksheet )
		{
			return worksheet.DisplayOptions;
		}

		#endregion GetDisplayOptionsForWorksheet

		#region Reset

		/// <summary>
		/// Resets the window options to their default settings.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The defaults used for each setting are the same defaults with which Microsoft Excel creates a blank workbook.
		/// </p>
		/// </remarks>
		public override void Reset()
		{
			base.Reset();

			this.boundsInTwips = new Rectangle( 1000, 1000, 15000, 10000 );
			this.firstVisibleTabIndex = 0;
			this.minimized = false;
			this.selectedWorksheetIndex = 0;
		}

		#endregion Reset

		#endregion Base Class Overrides

		#region Properties

		#region Public Properties

		#region BoundsInTwips

		/// <summary>
		/// Gets or sets the bounds of the workbook's MDI child window in twips (1/20th of a point).
		/// </summary>
		/// <remarks>
		/// <p class="note">
		/// <b>Note:</b> This setting cannot be saved with a <see cref="CustomView"/> in the Excel file. It can only be saved
		/// with a <see cref="Workbook"/>. Therefore, there is no corresponding property in <see cref="CustomViewWindowOptions"/> and
		/// a newly created CustomView will not be initialized with the setting from this property.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// The left or top of the value assigned is outside the bounds of -32768 and 32767.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The width or height of the value assigned is outside the bounds of 0 and 65535.
		/// </exception>
		/// <value>The bounds of the workbook's MDI child window in twips (1/20th of a point).</value>
		public Rectangle BoundsInTwips
		{
			get { return this.boundsInTwips; }
			set
			{
				if ( this.boundsInTwips != value )
				{
					if ( value.Left < -32768 || 32767 < value.Left ||
						value.Top < -32768 || 32767 < value.Top )
					{
						throw new ArgumentException( SR.GetString( "LE_ArgumentException_TopLeftWindowBounds" ), "value" );
					}

					if ( value.Width < 0 || 65535 < value.Width ||
						value.Height < 0 || 65535 < value.Height )
					{
						throw new ArgumentException( SR.GetString( "LE_ArgumentException_WidthHeightWindowBounds" ), "value" );
					}

					this.boundsInTwips = value;
				}
			}
		}

		#endregion BoundsInTwips

		#region FirstVisibleTabIndex

		/// <summary>
		/// Gets or sets the index of the first visible tab in the worksheet tab bar.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the worksheet tab bar is not visible, this value will not be used, but it is still saved with the workbook.
		/// </p>
		/// <p class="note">
		/// <b>Note:</b> This setting cannot be saved with a <see cref="CustomView"/> in the Excel file. It can only be saved
		/// with a <see cref="Workbook"/>. Therefore, there is no corresponding property in <see cref="CustomViewWindowOptions"/> and
		/// a newly created CustomView will not be initialized with the setting from this property.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The value assigned is less than zero.
		/// </exception>
		/// <value>The index of the first visible tab in the worksheet tab bar.</value>
		/// <seealso cref="WindowOptions.TabBarVisible"/>
		/// <seealso cref="WindowOptions.TabBarWidth"/>
		public int FirstVisibleTabIndex
		{
			get { return this.firstVisibleTabIndex; }
			set
			{
				if ( this.firstVisibleTabIndex != value )
				{
					if ( value < 0 )
						throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_FirstVisibleTabIndex" ) );

					this.firstVisibleTabIndex = value;
				}
			}
		}

		#endregion FirstVisibleTabIndex

		#region Minimized

		/// <summary>
		/// Gets or sets the value indicating whether the workbook's MDI child window is minimized in Microsoft Excel.
		/// </summary>
		/// <remarks>
		/// <p class="note">
		/// <b>Note:</b> This setting cannot be saved with a <see cref="CustomView"/> in the Excel file. It can only be saved
		/// with a <see cref="Workbook"/>. Therefore, there is no corresponding property in <see cref="CustomViewWindowOptions"/> and
		/// a newly created CustomView will not be initialized with the setting from this property.
		/// </p>
		/// </remarks>
		/// <value>The value indicating whether the workbook's MDI child window is minimized in Microsoft Excel.</value>
		/// <seealso cref="CustomViewWindowOptions.Maximized"/>
		public bool Minimized
		{
			get { return this.minimized; }
			set { this.minimized = value; }
		}

		#endregion Minimized

		#endregion Public Properties

		#region Internal Properties

		#region SelectedWorksheetIndex







		internal int SelectedWorksheetIndex
		{
			get { return this.selectedWorksheetIndex; }
			set { this.selectedWorksheetIndex = value; }
		}

		#endregion SelectedWorksheetIndex

		#endregion Internal Properties

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