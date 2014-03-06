using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;






using System.Drawing;
using Infragistics.Shared;

namespace Infragistics.Documents.Excel

{
	/// <summary>
	/// Represents the workbook window options which are saved with custom views.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// The properties explicitly defined on this class and not inherited from the base class, 
	/// <see cref="WindowOptions"/>, are options that can be saved with a custom view, but 
	/// not with a workbook. Therefore, these properties will not be applied when the 
	/// <see cref="CustomView.Apply"/> method is called.
	/// </p>
	/// </remarks>
	/// <seealso cref="CustomView.WindowOptions"/>
	/// <seealso cref="WorkbookWindowOptions"/>



	public

		 class CustomViewWindowOptions : WindowOptions
	{
		#region Constants

        //  BF 8/11/08  Excel2007 Format
        internal static readonly Rectangle defaultBoundsInPixels = new Rectangle( 10, 50, 600, 400 );

        #endregion Constants

		#region Member Variables

		private CustomView customView;

        //  BF 8/11/08  Excel2007 Format
		//private Rectangle boundsInPixels = new Rectangle( 10, 50, 600, 400 );
		private Rectangle boundsInPixels = defaultBoundsInPixels;

        private bool maximized = true;
		private bool showFormulaBar = true;
		private bool showStatusBar = true;

		// Only used during loading
		private int selectedWorksheetTabId;

		#endregion Member Variables

		#region Constructor

		internal CustomViewWindowOptions( CustomView customView )
			: base( customView.Workbook ) 
		{
			this.customView = customView;
		}

		#endregion Constructor

		#region Base Class Overrides

		#region AllowNullSelectedWorksheet






		internal override bool AllowNullSelectedWorksheet
		{
			// The selected worksheet can always be null in a custom view window options. If it is, the currently
			// selected worksheet in the workbook will not be changed when the custom view is applied.
			get { return true; }
		}

		#endregion AllowNullSelectedWorksheet

		#region GetDisplayOptionsForWorksheet

		internal override DisplayOptions GetDisplayOptionsForWorksheet( Worksheet worksheet )
		{
			return this.customView.GetDisplayOptions( worksheet );
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

			this.boundsInPixels = new Rectangle( 10, 50, 600, 400 );
			this.maximized = true;
			this.showFormulaBar = true;
			this.showStatusBar = true;
		}

		#endregion Reset

		#endregion Base Class Overrides

		#region Properties

		#region Public Properties

		#region BoundsInPixels

		/// <summary>
		/// Gets or sets the pixel bounds of the workbook's MDI child window when <see cref="CustomView"/> 
		/// owning these window options is applied.
		/// </summary>
		/// <remarks>
		/// <p class="note">
		/// <b>Note:</b> This property will have no affect on the workbook if <see cref="Maximized"/> is True. However, in this case,
		/// the value of this property will still be saved with the workbook.
		/// </p>
		/// <p class="note">
        /// <b>Note:</b> This setting cannot be saved with a <see cref="Workbook"/> in the Excel file. It can only be saved
        /// with a CustomView. Therefore, there is no corresponding property in <see cref="WorkbookWindowOptions"/> and
		/// calling <see cref="T:CustomView.Apply"/> on the associated CustomView will not apply this property. Only by 
		/// applying the custom view through the Microsoft Excel user interface will the setting on this property be applied.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// The left or top of the value assigned is outside the bounds of -32768 and 32767.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The width or height of the value assigned is outside the bounds of 0 and 65535.
		/// </exception>
		/// <value>
		/// The pixel bounds of the workbook's MDI child window when CustomView owning these 
		/// window options is applied.
		/// </value>
		/// <seealso cref="CustomView.Apply"/>
		public Rectangle BoundsInPixels
		{
			get { return this.boundsInPixels; }
			set
			{
				if ( this.boundsInPixels != value )
				{
					if ( value.Left < -32768 || 32767 < value.Left || value.Top < -32768 || 32767 < value.Top )
						throw new ArgumentException( SR.GetString( "LE_ArgumentException_TopLeftWindowBounds" ), "value" );

					if ( value.Width < 0 || 65535 < value.Width || value.Height < 0 || 65535 < value.Height )
						throw new ArgumentException( SR.GetString( "LE_ArgumentException_WidthHeightWindowBounds" ), "value" );

					this.boundsInPixels = value;
				}
			}
		}

		#endregion BoundsInPixels

		#region Maximized

		/// <summary>
		/// Gets or sets the value indicating whether the workbook's MDI child window will be maximized
		/// when the <see cref="CustomView"/> owning these window options is applied.
		/// </summary>
		/// <remarks>
		/// <p class="note">
		/// <b>Note:</b> This setting cannot be saved with a <see cref="Workbook"/> in the Excel file. It can only be saved
		/// with a CustomView. Therefore, there is no corresponding property in <see cref="WorkbookWindowOptions"/> and
		/// calling <see cref="T:CustomView.Apply"/> on the associated CustomView will not apply this property. Only by 
		/// applying the custom view through the Microsoft Excel user interface will the setting on this property be applied.
		/// </p>
		/// </remarks>
		/// <value>
		/// The value indicating whether the workbook's MDI child window will be maximized when the CustomView owning these 
		/// window options is applied.
		/// </value>
		/// <seealso cref="CustomView.Apply"/>
		/// <seealso cref="WorkbookWindowOptions.Minimized"/>
		public bool Maximized
		{
			get { return this.maximized; }
			set { this.maximized = value; }
		}

		#endregion Maximized

		#region ShowFormulaBar

		/// <summary>
		/// Gets or sets the value indicating whether Microsoft Excel will display the formula bar when
		/// the <see cref="CustomView"/> owning these window options is applied.
		/// </summary>
		/// <remarks>
		/// <p class="note">
		/// <b>Note:</b> This setting cannot be saved with a <see cref="Workbook"/> in the Excel file. It can only be saved
		/// with a CustomView. Therefore, there is no corresponding property in <see cref="WorkbookWindowOptions"/> and
		/// calling <see cref="CustomView.Apply"/> on the associated CustomView will not apply this property. Only by 
		/// applying the custom view through the Microsoft Excel user interface will the setting on this property be applied.
		/// </p>
		/// </remarks>
		/// <value>
		/// The value indicating whether Microsoft Excel will display the formula bar when the CustomView owning these window 
		/// options is applied.
		/// </value>
		/// <seealso cref="CustomView.Apply"/>
		public bool ShowFormulaBar
		{
			get { return this.showFormulaBar; }
			set { this.showFormulaBar = value; }
		}

		#endregion ShowFormulaBar

		#region ShowStatusBar

		/// <summary>
		/// Gets or sets the value indicating whether Microsoft Excel will display the status bar when
		/// the <see cref="CustomView"/> owning these window options is applied.
		/// </summary>
		/// <remarks>
		/// <p class="note">
		/// <b>Note:</b> This setting cannot be saved with a <see cref="Workbook"/> in the Excel file. It can only be saved
		/// with a CustomView. Therefore, there is no corresponding property in <see cref="WorkbookWindowOptions"/> and
		/// calling <see cref="T:CustomView.Apply"/> on the associated CustomView will not apply this property. Only by 
		/// applying the custom view through the Microsoft Excel user interface will the setting on this property be applied.
		/// </p>
		/// </remarks>
		/// <value>
		/// The value indicating whether Microsoft Excel will display the status bar when the CustomView owning these window 
		/// options is applied.
		/// </value>
		/// <seealso cref="CustomView.Apply"/>
		public bool ShowStatusBar
		{
			get { return this.showStatusBar; }
			set { this.showStatusBar = value; }
		}

		#endregion ShowStatusBar

		#endregion Public Properties

		#region Internal Properties

		#region SelectedWorksheetTabId







		internal int SelectedWorksheetTabId
		{
			get { return this.selectedWorksheetTabId; }
			set { this.selectedWorksheetTabId = value; }
		}

		#endregion SelectedWorksheetTabId

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