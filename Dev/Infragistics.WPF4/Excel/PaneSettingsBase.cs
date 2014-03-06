using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Abstract base class for classes which control pane settings.
	/// </summary>
	/// <seealso cref="FrozenPaneSettings"/>
	/// <seealso cref="UnfrozenPaneSettings"/>



	public

		 abstract class PaneSettingsBase
	{
		#region Member Variables

		// MD 6/31/08 - Excel 2007 Format
		private DisplayOptions displayOptions;

		private int firstColumnInRightPane;
		private int firstRowInBottomPane;

		// MD 5/24/07
		// Property removed: uncomment when property is re-added
		//private PaneLocation paneWithActiveCell = PaneLocation.TopLeft;

		#endregion Member Variables

		#region Constructor

		// MD 6/31/08 - Excel 2007 Format
		// The pane settings now need a reference to the owning display options (its actually the workbook they need, but just incase they need 
		// something on their owner too, we might as well store that).
		//internal PaneSettingsBase() { }
		internal PaneSettingsBase( DisplayOptions displayOptions ) 
		{
			this.displayOptions = displayOptions;
		}

		#endregion Constructor

		#region Methods

		#region InitializeFrom

		internal virtual void InitializeFrom( PaneSettingsBase paneSettings )
		{
			this.firstColumnInRightPane = paneSettings.firstColumnInRightPane;
			this.firstRowInBottomPane = paneSettings.firstRowInBottomPane;

			// MD 5/24/07
			// Property removed: uncomment when property is re-added
			//this.paneWithActiveCell = paneSettings.paneWithActiveCell;
		}

		#endregion InitializeFrom

		#region Reset

		/// <summary>
		/// Resets the pane settings to their defaults.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The defaults used for each setting are the same defaults with which Microsoft Excel creates a blank worksheet.
		/// </p>
		/// </remarks>
		public virtual void Reset()
		{
			this.firstColumnInRightPane = 0;
			this.firstRowInBottomPane = 0;

			// MD 5/24/07
			// Property removed: uncomment when property is re-added
			//this.paneWithActiveCell = PaneLocation.TopLeft;
		}

		#endregion Reset

		#endregion Methods

		#region Properties

		#region Public Properties

		#region FirstColumnInRightPane

		/// <summary>
		/// Gets or sets the first visible column in the right pane(s) of the worksheet. 
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This affects the scroll position for the right pane(s) of the worksheet and 
		/// is only used if the worksheet is split vertically.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The assigned value is outside the valid column range (0 to one less than <see cref="Workbook.MaxExcelColumnCount"/> or 
		/// <see cref="Workbook.MaxExcel2007ColumnCount"/>, depending on the workbook's 
		/// <see cref="Workbook.CurrentFormat"/>).
		/// </exception>
		/// <value>The zero-based index of the first visible column in the right pane(s).</value>
		public int FirstColumnInRightPane
		{
			get { return this.firstColumnInRightPane; }
			set
			{
				if ( this.firstColumnInRightPane != value )
				{
					// MD 6/31/08 - Excel 2007 Format
					//Utilities.VerifyColumnIndex( value, "value" );
					// MD 4/12/11 - TFS67084
					//Utilities.VerifyColumnIndex( this.displayOptions.Worksheet.Workbook, value, "value" );
					Utilities.VerifyColumnIndex(this.displayOptions.Worksheet, value, "value");

					this.firstColumnInRightPane = value;
				}
			}
		}

		#endregion FirstColumnInRightPane

		#region FirstRowInBottomPane

		/// <summary>
		/// Gets or sets the first visible row in the bottom pane(s) of the worksheet. 
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This affects the scroll position for the bottom pane(s) of the worksheet and 
		/// is only used if the worksheet is split horizontally.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The assigned value is outside the valid row range (0 to one less than <see cref="Workbook.MaxExcelRowCount"/> or 
		/// <see cref="Workbook.MaxExcel2007RowCount"/>, depending on the workbook's 
		/// <see cref="Workbook.CurrentFormat"/>).
		/// </exception>
		/// <value>The zero-based index of the first visible row in the bottom pane(s).</value>
		public int FirstRowInBottomPane
		{
			get { return this.firstRowInBottomPane; }
			set
			{
				if ( this.firstRowInBottomPane != value )
				{
					// MD 6/31/08 - Excel 2007 Format
					//Utilities.VerifyRowIndex( value, "value" );
					// MD 2/24/12 - 12.1 - Table Support
					// The workbook may be null.
					//Utilities.VerifyRowIndex( this.displayOptions.Worksheet.Workbook, value, "value" );
					Utilities.VerifyRowIndex(this.displayOptions.Worksheet, value, "value");

					this.firstRowInBottomPane = value;
				}
			}
		}

		#endregion FirstRowInBottomPane

		// MD 5/24/07
		// This doesn't work correctly and isn't really useful if it did, when there is a better use for it, readd it
		
#region Infragistics Source Cleanup (Region)











































#endregion // Infragistics Source Cleanup (Region)


		#endregion Public Properties

		#region Abstract Properties

		internal abstract bool HasHorizontalSplit { get;}
		internal abstract bool HasVerticalSplit { get;}

		#endregion Abstract Properties

		#region Internal Properties

		#region Worksheet

		internal DisplayOptions DisplayOptions
		{
			get { return this.displayOptions; }
		} 

		#endregion Worksheet

		#endregion Internal Properties

		#endregion Properties

        #region Methods

        //  BF 8/11/08  Excel2007 Format
        #region ShouldSerialize
        internal virtual bool ShouldSerialize()
        {
            return this.firstColumnInRightPane != 0 && this.firstRowInBottomPane != 0;
        }
        #endregion ShouldSerialize

        #endregion Methods
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