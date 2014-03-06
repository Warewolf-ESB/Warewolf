using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Globalization;




using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Abstract base class for worksheet row and worksheet column.
	/// </summary>
	/// <seealso cref="WorksheetColumn"/>
	/// <seealso cref="WorksheetRow"/>



	public

		 abstract class RowColumnBase :
		ICellFormatOwner,					// MD 1/8/12 - 12.1 - Cell Format Updates
		IWorksheetCellFormatProxyOwner		// MD 5/12/10 - TFS26732
	{
		#region Member Variables

		private WorksheetCellFormatProxy cellFormat;

		// MD 3/15/12 - TFS104581
		// Moved these to WorksheetRow because for columns, they are now stored on the WorksheetColumnBlock.
		//private bool hidden;
		//private int index;
		//
		//// MD 7/26/10 - TFS34398
		//// The outline level can only be 0-7, so we just need a byte to store it.
		////private int outlineLevel;
		//private byte outlineLevel;

		private Worksheet worksheet;		

		#endregion Member Variables

		#region Constructor

		// MD 3/15/12 - TFS104581
		// The index is now stored on the derived classes.
		//internal RowColumnBase( Worksheet worksheet, int index )
		//{
		//    this.worksheet = worksheet;
		//    this.index = index;
		//}
		internal RowColumnBase(Worksheet worksheet)
		{
			this.worksheet = worksheet;
		}

		#endregion Constructor

		#region Interfaces

		// MD 1/8/12 - 12.1 - Cell Format Updates
		#region ICellFormatOwner Members

		WorksheetCellFormatProxy ICellFormatOwner.CellFormatInternal
		{
			get { return this.CellFormatInternal; }
		}

		bool ICellFormatOwner.HasCellFormat
		{
			get { return this.HasCellFormat; }
		}

		#endregion

		// MD 5/12/10 - TFS26732
		#region IWorksheetCellFormatProxyOwner Members

		// MD 3/22/12 - TFS104630
		WorksheetCellFormatData IWorksheetCellFormatProxyOwner.GetAdjacentFormatForBorderResolution(WorksheetCellFormatProxy sender, CellFormatValue borderValue)
		{
			return this.GetAdjacentFormatForBorderResolution(sender, borderValue);
		}

		// MD 10/21/10 - TFS34398
		// We need to pass along options to the handlers of the cell format value change.
		//void IWorksheetCellFormatProxyOwner.OnCellFormatValueChanged(CellFormatValue value)
		//{
		//    this.OnCellFormatValueChanged(value);
		//}
		// MD 4/12/11 - TFS67084
		// We need to pass along the sender now because some object own multiple cell formats.
		//void IWorksheetCellFormatProxyOwner.OnCellFormatValueChanged(CellFormatValue value, CellFormatValueChangedOptions options)
		//{
		//    this.OnCellFormatValueChanged(value, options);
		//}
		// MD 4/18/11 - TFS62026
		// The proxy will now pass along all values that changed in one operation as opposed to one at a time.
		//void IWorksheetCellFormatProxyOwner.OnCellFormatValueChanged(WorksheetCellFormatProxy sender, CellFormatValue value, CellFormatValueChangedOptions options)
		//{
		//    this.OnCellFormatValueChanged(sender, value, options);
		//}
		void IWorksheetCellFormatProxyOwner.OnCellFormatValueChanged(WorksheetCellFormatProxy sender, IList<CellFormatValue> values, CellFormatValueChangedOptions options)
		{
			this.OnCellFormatValueChanged(sender, values, options);
		}

		// MD 2/29/12 - 12.1 - Table Support
		void IWorksheetCellFormatProxyOwner.OnCellFormatValueChanging(WorksheetCellFormatProxy sender, IList<CellFormatValue> values) { }

		// MD 3/1/12 - 12.1 - Table Support
		void IWorksheetCellFormatProxyOwner.VerifyFormatOptions(WorksheetCellFormatProxy sender, WorksheetCellFormatOptions formatOptions) { }

		// MD 2/29/12 - 12.1 - Table Support
		// This is no longer needed.
		//// MD 11/1/11 - TFS94534
		//bool IWorksheetCellFormatProxyOwner.CanOwnStyleFormat
		//{
		//    get { return false; }
		//}

		// MD 1/17/12 - 12.1 - Cell Format Updates
		// This is no longer needed.
		//// MD 7/26/10 - TFS34398
		//Workbook IWorksheetCellFormatProxyOwner.Workbook
		//{
		//    get { return this.Worksheet.Workbook; }
		//}

		#endregion 

		#endregion // Interfaces

		#region Methods

		// MD 3/15/12 - TFS104581
		internal abstract WorksheetCellFormatProxy CreateCellFormatProxy(GenericCachedCollectionEx<WorksheetCellFormatData> cellFormatCollection);

		// MD 3/22/12 - TFS104630
		internal abstract WorksheetCellFormatData GetAdjacentFormatForBorderResolution(WorksheetCellFormatProxy sender, CellFormatValue borderValue);

		// MD 3/5/10 - TFS26342
		#region GetExtentInTwips



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal abstract int GetExtentInTwips(bool ignoreHidden);

		#endregion // GetExtentInTwips

		// MD 1/8/12 - 12.1 - Cell Format Updates
		#region GetResolvedCellFormat

		/// <summary>
		/// Gets the resolved cell formatting for the cells in the row or column.
		/// </summary>
		/// <returns>
		/// A format object describing the actual formatting that will be used when displayed the row or column in Microsoft Excel.
		/// </returns>
		/// <seealso cref="RowColumnBase.CellFormat"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)] 

		public IWorksheetCellFormat GetResolvedCellFormat()
		{
			return new WorksheetCellFormatDataResolved(this.CellFormatInternal);
		}

		#endregion // GetResolvedCellFormat

		// MD 5/12/10 - TFS26732
		#region OnCellFormatValueChanged

		// MD 10/21/10 - TFS34398
		// We need to pass along options to the handlers of the cell format value change.
		//internal abstract void OnCellFormatValueChanged(CellFormatValue value); 
		// MD 4/12/11 - TFS67084
		// We need to pass along the sender now because some object own multiple cell formats.
		//internal abstract void OnCellFormatValueChanged(CellFormatValue value, CellFormatValueChangedOptions options); 
		// MD 4/18/11 - TFS62026
		// The proxy will now pass along all values that changed in one operation as opposed to one at a time.
		//internal abstract void OnCellFormatValueChanged(WorksheetCellFormatProxy sender, CellFormatValue value, CellFormatValueChangedOptions options); 
		internal abstract void OnCellFormatValueChanged(WorksheetCellFormatProxy sender, IList<CellFormatValue> values, CellFormatValueChangedOptions options); 

		#endregion // OnCellFormatValueChanged

		// MD 11/24/10 - TFS34598
		#region OnCurrentFormatChanged

		internal virtual void OnCurrentFormatChanged()
		{
			// MD 1/19/11 - TFS62268
			// We only need to notify the elements that the format changed once, so we shouldn't do it here. Instead, each element 
			// is notified in the Workbook.OnCurrentFormatChanged() method.
			//if (this.cellFormat != null)
			//    this.cellFormat.Element.OnCurrentFormatChanged();
		} 

		#endregion // OnCurrentFormatChanged

		// MD 7/23/10 - TFS35969
		#region OnHiddenChanged

		internal virtual void OnHiddenChanged() { } 

		#endregion // OnHiddenChanged

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region CellFormat

		/// <summary>
		/// Gets the default cell format for cells in this row or column.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Any default properties of the cell's format will take their value from this format when getting a resolved format.
		/// </p>
		/// </remarks>
		/// <value>The default cell format for cells in this row or column.</value>
		public IWorksheetCellFormat CellFormat
		{
			get { return this.CellFormatInternal; }
		}

		internal bool HasCellFormat
		{
			get { return this.cellFormat != null; }
		}

		#endregion CellFormat

		#region Expanded

		/// <summary>
		/// Gets or sets the expanded state of the row or column.
		/// </summary>
		/// <remarks>
		/// <p class="body">This property is deprecated. Use <see cref="Hidden"/> instead.</p>
		/// <p class="note"><B>Note:</B> Hidden and Expanded are opposites (setting Expanded to True is equivalent to setting Hidden to False).</p>
		/// </remarks>
		[EditorBrowsable( EditorBrowsableState.Never )]
		[Obsolete( "Deprecated. Hidden and Expanded control the same aspect of the column, use Hidden instead. Note: Hidden and Expanded are opposites (setting Expanded to True is equivalent to setting Hidden to False).", false )]
		public bool Expanded
		{
			get { return this.Hidden == false; }
			set { this.Hidden = ( value == false ); }
		}

		#endregion Expanded

		#region Hidden

		/// <summary>
		/// Gets or sets the value indicating whether the row or column is hidden.
		/// </summary>
		/// <remarks>
		/// The Hidden state also controls the expanded state of rows or columns in outline groups. Basically, an outline group
		/// simply provides an easy way to hide and unhide all rows or columns in the group at the same time, via the expansion 
		/// indicator.
		/// </remarks>
		/// <value>The value indicating whether the row or column is hidden.</value>
		/// <seealso cref="OutlineLevel"/>
		/// <seealso cref="CustomView.GetHiddenColumns(Worksheet)"/>
		/// <seealso cref="CustomView.GetHiddenRows(Worksheet)"/>
		public bool Hidden
		{
			// MD 3/15/12 - TFS104581
			//get { return this.hidden; }
			get { return this.HiddenInternal; }

			// MD 3/5/10 - TFS26342
			// There is a bit more logic that goes into the Hidden setter now.
			//set { this.hidden = value; }
			set
			{
				// MD 3/15/12 - TFS104581
				//if (this.hidden == value)
				if (this.Hidden == value)
					return;

				// MD 7/23/10 - TFS35969
				// The OnBeforeWorksheetElementResize method now takes the element being resized.
				//this.Worksheet.OnBeforeWorksheetElementResize();
				this.Worksheet.OnBeforeWorksheetElementResize(this);

				int oldExtentInTwipsHiddenIgnored = this.GetExtentInTwips(true);

				// MD 3/15/12 - TFS104581
				//this.hidden = value;
				this.HiddenInternal = value;

				// MD 7/23/10 - TFS35969
				this.OnHiddenChanged();

				// MD 3/15/12 - TFS104581
				//this.Worksheet.OnAfterWorksheetElementResized(this, oldExtentInTwipsHiddenIgnored, this.hidden == false);
				this.Worksheet.OnAfterWorksheetElementResized(this, oldExtentInTwipsHiddenIgnored, value == false);
			}
		}

		#endregion Hidden

		#region Index

		/// <summary>
		/// Gets the 0-based index of the row or column in the worksheet.
		/// </summary>
		/// <value>The 0-based index of the row or column in the worksheet.</value>
		// MD 3/15/12 - TFS104581
		// The index member is now stored on the derived types.
		//public int Index
		//{
		//    get { return this.index; }
		//}
		public abstract int Index { get; }

		#endregion // Moved

		#region OutlineLevel

		/// <summary>
		/// Gets or sets the outline level for the row or column.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Microsoft Excel supports hierarchical grouping of rows and columns with a maximum of seven levels of hierarchy. 
		/// To create a group, set adjacent rows or columns to same outline level. When rows or columns are grouped, an
		/// outline indicator will provide a visual representation of the outline level.  In addition, an outline group has
		/// an expansion indicator at one end of the group, which allows the user to easily hide and unhide all rows or column
		/// in the group with a single click.
		/// </p>
		/// </remarks>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The value assigned is outside the valid outline level range of 0 and 7.
		/// </exception>
		/// <value>The outline level for the row or column.</value>
		/// <seealso cref="Hidden"/>
		public int OutlineLevel
		{
			// MD 3/15/12 - TFS104581
			//get { return this.outlineLevel; }
			get { return this.OutlineLevelInternal; }
			set
			{
				// MD 3/15/12 - TFS104581
				//if ( this.outlineLevel != value )
				if (this.OutlineLevel != value)
				{
					if ( value < 0 || 7 < value )
						throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LER_ArgumentOutOfRangeException_OutlineLevel" ) );

					// MD 7/26/10 - TFS34398
					// Now the outlineLevel member is a byte instead of an int.
					//this.outlineLevel = value;
					// MD 3/15/12 - TFS104581
					//this.outlineLevel = (byte)value;
					this.OutlineLevelInternal = (byte)value;
				}
			}
		}

		#endregion OutlineLevel

		#region Worksheet

		/// <summary>
		/// Gets the worksheet to which the row or column belongs.
		/// </summary>
		/// <value>The worksheet to which the row or column belongs.</value>
		public Worksheet Worksheet
		{
			get { return this.worksheet; }
		}

		#endregion Worksheet

		#endregion Public Properties

		#region Internal Properties

		#region CellFormatInternal

		internal WorksheetCellFormatProxy CellFormatInternal
		{
			get
			{
				if ( this.cellFormat == null )
				{
					Workbook workbook = this.worksheet.Workbook;

					// MD 2/2/12 - TFS100573
					// The workbook could be null.
					//GenericCachedCollection<WorksheetCellFormatData> cellFormatCollection = workbook.CellFormats;
					GenericCachedCollectionEx<WorksheetCellFormatData> cellFormatCollection = null;
					if (workbook != null)
						cellFormatCollection = workbook.CellFormats;

					// MD 5/12/10 - TFS26732
					// Added an owner parameter to the constructor.
					//this.cellFormat = new WorksheetCellFormatProxy( cellFormatCollection, workbook );
					// MD 4/18/11 - TFS62026
					// The workbook no longer needs to be specified.
					//this.cellFormat = new WorksheetCellFormatProxy(cellFormatCollection, workbook, this);
					// MD 3/15/12 - TFS104581
					//this.cellFormat = new WorksheetCellFormatProxy(cellFormatCollection, this);
					this.cellFormat = this.CreateCellFormatProxy(cellFormatCollection);

					// MD 1/8/12 - 12.1 - Cell Format Updates
					// This is no longer needed.
					//this.cellFormat.Style = false;
				}

				return this.cellFormat;
			}
		}

		#endregion CellFormatInternal

		#region HasDataIgnoreHidden







		internal virtual bool HasDataIgnoreHidden
		{
			get
			{
				// MD 3/15/12 - TFS104581
				//if ( this.outlineLevel != 0 )
				if (this.OutlineLevel != 0)
					return true;

				// MD 3/2/12 - 12.1 - Table Support
				//if ( this.HasCellFormat && this.CellFormatInternal.HasDefaultValue == false )
				if (this.HasCellFormat && this.CellFormatInternal.IsEmpty == false)
					return true;

				return false;
			}
		}

		#endregion HasDataIgnoreHidden

		// MD 3/15/12 - TFS104581
		internal abstract bool HiddenInternal { get; set; }
		internal abstract byte OutlineLevelInternal { get; set; }

		// MD 7/2/08 - Excel 2007 Format
		#region VerifyFormatLimits

		internal virtual void VerifyFormatLimits( FormatLimitErrors limitErrors, WorkbookFormat testFormat )
		{
			// MD 1/19/12 - 12.1 - Cell Format Updates
			//// MBS 7/15/08 - Excel 2007 Format
			//if (this.cellFormat != null)
			//    this.cellFormat.Element.VerifyFormatLimits(limitErrors, testFormat);
		}

		#endregion VerifyFormatLimits

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