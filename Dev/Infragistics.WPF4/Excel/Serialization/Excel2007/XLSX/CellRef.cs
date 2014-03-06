using System;
using System.Globalization;




using System.Windows.Forms;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX
{
    #region CellRef class





	internal class CellRef
	{
		#region Member Variables

		private int row;
		private int column;

		#endregion Member Variables

		#region Constructor

        public CellRef( int row, int column )
        {
            this.row = row;
            this.column = column;
        }

		#endregion Constructor

		#region Methods

            #region FromString
        /// <summary>
        /// Returns a CellRef instance from the specified string.
        /// </summary>
        /// <param name="value">A string implied to be conformant with the ST_CellRef simple XML data type.</param>
        /// <returns>A CellRef instance or null if none could be parsed.</returns>
        public static CellRef FromString( string value )
        {
            if ( string.IsNullOrEmpty(value) )
                return null;

            int row;
            short column;
            CellRef retVal = null;
            
            try
            {
				// MD 4/6/12 - TFS101506
                //Utilities.ParseA1CellAddress( value, WorkbookFormat.Excel2007, out column, out row );
				Utilities.ParseA1CellAddress(value, WorkbookFormat.Excel2007, CultureInfo.InvariantCulture, out column, out row);

                retVal = new CellRef( row, (int)column );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }

            return retVal;
        }
            #endregion FromString

            #region ToString
        /// <summary>
        /// Returns the absolute A1 representation of this instance.
        /// </summary>
        public override string ToString()
        {
            CellReferenceMode cellReferenceMode = CellReferenceMode.A1;

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
            //string colString = FormulaUtilities.CellAddress.GetColumnString( this.column, true, Workbook.MaxExcel2007ColumnCount, null, false, cellReferenceMode );
			string colString = FormulaUtilities.CellAddress.GetColumnString(this.column, true, Workbook.MaxExcel2007ColumnCount, -1, false, cellReferenceMode);

			// MD 2/20/12 - 12.1 - Table Support
            //string rowString = FormulaUtilities.CellAddress.GetRowString( this.row, true, Workbook.MaxExcel2007RowCount, null, false, cellReferenceMode );
			string rowString = FormulaUtilities.CellAddress.GetRowString(this.row, true, Workbook.MaxExcel2007RowCount, -1, false, cellReferenceMode);

            return string.Format( "{0}{1}", colString, rowString );
        }
            #endregion ToString

		#endregion Methods

		#region Properties

		    #region Column
		public int Column
		{
			get { return this.column; }
		} 
		    #endregion Column

		    #region Row
		public int Row
		{
			get { return this.row; }
		} 
		    #endregion Row

		#endregion Properties
	}
    #endregion CellRef class
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