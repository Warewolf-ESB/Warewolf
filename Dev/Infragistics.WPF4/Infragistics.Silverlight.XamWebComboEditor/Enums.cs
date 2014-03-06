
namespace Infragistics.Controls.Editors
{
    #region CustomValueEnteredActions

    /// <summary>
    /// An Enum, that allows a user to specify what action should occur, when entering data into the combo editor
    /// that doesn't exist in the underlying ItemsSource.
    /// </summary>
    public enum CustomValueEnteredActions
    {
        /// <summary>
        /// This will prevent a user from typing in invalid data.
        /// </summary>
        Ignore,

        /// <summary>
        /// The data will be allowed to be typed. But no items will be selected, and the underlying ItemsSource will remain untouched.
        /// </summary>
        Allow,

        /// <summary>
        /// The data will be added to the underlying ItemsSource, and will become valid data for selection.
        /// </summary>
        Add
    }

    #endregion //CustomValueEnteredActions

    #region LasteInvokeAction

    internal enum LastInvokeAction
    {
        ArrowKeys, 

        TextBox

    }


    #endregion // LasteInvokeAction

	#region RowType

	/// <summary>
	/// An enumeration that contains all the different types of rows that the <see cref="XamMultiColumnComboEditor"/> supports.
	/// </summary>

	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_MultiColumnCombo, Version = FeatureInfo.Version_11_2)]

	public enum RowType
	{
		/// <summary>
		/// A row that represents a record of data.
		/// </summary>
		DataRow,

		/// <summary>
		/// A row that represents the header for a group of data rows.
		/// </summary>
		HeaderRow
	}

	#endregion // RowType

	#region FilterMode

	/// <summary>
	/// An enumeration that contains all the different filterin modes that the <see cref="XamMultiColumnComboEditor"/> supports.
	/// </summary>

	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_MultiColumnCombo, Version = FeatureInfo.Version_11_2)]

	public enum FilterMode
	{
		/// <summary>
		/// The <see cref="XamMultiColumnComboEditor"/> will filter the items in the dropdown list by including only those items whose primary column starts with the text typed in the control's TextBox.
		/// </summary>
		/// <remarks>
		/// The 'primary column' is the column that corresponds to the value set on the control's DisplayMemberPath property.
		/// </remarks>
		FilterOnPrimaryColumnOnly,

		/// <summary>
		/// The <see cref="XamMultiColumnComboEditor"/> will filter the items in the dropdown list by including only those items that contain the text typed in the control's TextBox in any text column.
		/// </summary>
		FilterOnAllColumns
	}

	#endregion // FilterMode

	#region MultiColumnComboEditorCommand
	/// <summary>
	/// An enumeration of available commands that apply to the <see cref="XamMultiColumnComboEditor"/>.
	/// </summary>

	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_MultiColumnCombo, Version = FeatureInfo.Version_11_2)]

	public enum MultiColumnComboEditorCommand
	{
		/// <summary>
		/// Clears the list of the currently selected items.
		/// </summary>
		ClearSelection
	}
	#endregion //MultiColumnComboEditorCommand

	#region ComboColumnFixedState

	/// <summary>
    /// An enumeration that describes if a column is fixed, and if so on what side is it fixed.
    /// </summary>
    public enum ComboColumnFixedState
    {
        /// <summary>
        /// Column is not fixed.
        /// </summary>
        NotFixed,

        /// <summary>
        /// Column is fixed to the left.
        /// </summary>
        Left,

        /// <summary>
        /// Column is fixed to the right. 
        /// </summary>
        Right
    }

    #endregion // ComboColumnFixedState

    #region ComboColumnWidthType
    /// <summary>
    /// An Enum that describes the type of width for the Column.
    /// </summary>
    public enum ComboColumnWidthType
    {
        /// <summary>
        /// A column's width will size to the largest header or cell in the column. 
        /// Note: while scrolling the width of the column may grow as larger content comes into view, however,
        /// it will never decrease in width.
        /// </summary>
        Auto,

        /// <summary>
        /// A column's width will size to the largest header or cell in the column. However, this will only occur
        /// when the grid first loads. Or when a user double clicks on the edge of a column header to resize. 
        /// </summary>
        InitialAuto,


        /// <summary>
        /// A column's width will size to the header of a column. 
        /// </summary>
        SizeToHeader,

        /// <summary>
        /// A column's width will size to the largest cell in the column. 
        /// Note: while scrolling the width of the column may grow as larger content comes into view, however,
        /// it will never decrease in width.
        /// </summary>
        SizeToCells,

        /// <summary>
        /// A column's width will size to fill any remaing space in the dropdown's grid. 
        /// If more than one column has a star value specified, the remaing width will be split
        /// evenly amongst the columns. 
        /// If other columns already are taking up the majority of the space, the column's width will be zero.
        /// If the dropdown grid's width is Infinity, then the column will act as ComboColumnWidthType.Auto width.
        /// </summary>
        Star,

        /// <summary>
        /// A column's width will size to the value specified. 
        /// </summary>
        Numeric
    }
    #endregion // ComboColumnWidthType
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