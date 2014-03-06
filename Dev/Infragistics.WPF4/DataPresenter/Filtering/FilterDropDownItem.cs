using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using System.Diagnostics;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Infragistics.Shared;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Internal;
using Infragistics.Windows.DataPresenter.Internal;
using Infragistics.Windows.Reporting;
using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.Editors;
using System.Windows.Markup;
using System.ComponentModel;

namespace Infragistics.Windows.DataPresenter
{

	#region FilterDropDownItem Class

	/// <summary>
	/// Represents an entry in the filter drop-down list.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// Filter drop-down list is displayed when a filter icon in a field label is clicked or when a filter
	/// cell's drop-down button is clicked. The filter drop-down list provides the user with a list of options
	/// to filter the records by.
	/// </para>
	/// <para class="body">
	/// You can access and modify the filter drop-down list by hooking into the 
	/// <see cref="DataPresenterBase.RecordFilterDropDownOpening"/> or the 
	/// <see cref="DataPresenterBase.RecordFilterDropDownPopulating"/> event. The associated event args expose
	/// <b>DropDownItems</b> property that returns a list of <b>FilterDropDownItem</b> objects.
	/// </para>
	/// </remarks>
	/// <seealso cref="RecordFilterDropDownOpeningEventArgs.DropDownItems"/>
	/// <seealso cref="RecordFilterDropDownPopulatingEventArgs.DropDownItems"/>
	public class FilterDropDownItem 

		: ComboBoxDataItem

	{
		#region Member Vars

		private bool _isCellValue;

		// JJD 06/29/10 - TFS32174 - added
		internal RecordManager.SameFieldRecordsSortComparer.CellInfo _cellInfo;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="FilterDropDownItem"/>.
		/// </summary>
		/// <param name="value">Data value of the item.</param>
		/// <param name="displayText">Display text of the item.</param>
		/// <param name="isCellValue">Whether the value is a cell value.</param>
		internal FilterDropDownItem( object value, string displayText, bool isCellValue )

			// JJD 02/17/12 - TFS101703 - fall back to picking up the DisplayText from a ComparisonCondition
			//: base(value, displayText)
            : base(value, GetDisplayTextHelper(value, displayText))

		{
			_isCellValue = isCellValue;
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="FilterDropDownItem"/>.
		/// </summary>
		/// <param name="specialOperand">Special operand that will be used to filter records when this entry is selected.</param>
		/// <param name="displayText">Display text.</param>
		/// <seealso cref="IsSpecialOperand"/>
		// SSP 3/23/10 TFS29800
		// We need to use the Language to format operand month names. Changed the FilterDropDownItem
		// constructor that took in special operand to also take in the display text.
		// 
		//public FilterDropDownItem( SpecialFilterOperandBase specialOperand )
		//	: this( specialOperand, GridUtilities.ToString( specialOperand.DisplayContent, specialOperand.Name ), false )
		public FilterDropDownItem( SpecialFilterOperandBase specialOperand, string displayText )
			: this( specialOperand, displayText, false )
		{
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="FilterDropDownItem"/>.
		/// </summary>
		/// <param name="condition">Condition that will be used to filter records when this entry is selected.</param>
		/// <param name="displayText">Text that will be displayed in the drop-down to depict this entry.</param>
		/// <seealso cref="IsCondition"/>
		public FilterDropDownItem( ICondition condition, string displayText )
			: this( condition, displayText, false )
		{
		}
		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="FilterDropDownItem"/>.
		/// </summary>
		/// <param name="action">Command that will be executed when this entry is selected.</param>
		/// <param name="displayText">Text that will be displayed in the drop-down to depict this entry.</param>
		/// <seealso cref="IsAction"/>
		public FilterDropDownItem( ICommand action, string displayText )
			: this( action, displayText, false )
		{
		}

		#endregion // Constructor

		#region Properties

		#region Public Properties







		#region IsAction

		/// <summary>
		/// Indicates that this drop-down item represents an action that will be taken when the item is selected.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Indicates whether this drop-down item represents an action that will be taken when the item is selected. 
		/// The underlying Value will be an instance of <b>ICommand</b>. When the user selects the entry in the 
		/// drop-down, the command's Execute method will be called to perform the action.
		/// </para>
		/// <para class="body">
		/// For example, the built-in '(Custom)' entry provided by the data presenter in the filter drop-down is an 
		/// action. The associated value implements ICommand interface whose Execute implementation displays the custom 
		/// filter selection control (<see cref="CustomFilterSelectionControl"/>.
		/// </para>
		/// <para class="body">
		/// You can add your own custom action to the filter drop-down. To do so implement ICommand on a class. Then
		/// add an entry to the filter drop-down list where the value of that entry is the instance of that class. When
		/// the user clicks the entry in the filter drop-down, the ICommand.Execute method will be called, which is
		/// where you perform the desired action. The data presenter will take no further actions. It will be upto you
		/// to manipulate the record filter of the field to reflect the desired filter criteria.
		/// </para>
		/// </remarks>
		public bool IsAction
		{
			get
			{
				return !_isCellValue && this.Value is ICommand;
			}
		}

		#endregion // IsAction

		#region IsCellValue

		/// <summary>
		/// Indicates that this drop-down item represents a cell value.
		/// </summary>
		public bool IsCellValue
		{
			get
			{
				return _isCellValue;
			}
		}

		#endregion // IsCellValue

		#region IsCondition

		/// <summary>
		/// Indicates that this drop-down item represents an an ICondition.
		/// </summary>
		public bool IsCondition
		{
			get
			{
				return !_isCellValue && this.Value is ICondition;
			}
		}

		#endregion // IsCondition

		#region IsSpecialOperand

		/// <summary>
		/// Indicates that this drop-down item represents a special operand, like (Blanks), (NonBlanks).
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Indicates whether this drop-down item represents a special operand that will be used to filter records
		/// when the item is selected. The underlying Value will be an instance of <see cref="SpecialFilterOperandBase"/> 
		/// derived class.
		/// </para>
		/// <para class="body">
		/// For example, the built-in entries of 'Today', 'Yesterday', 'This Week' etc... are implemented
		/// using special operands.
		/// </para>
		/// <para class="body">
		/// You can add your own custom operand the filter drop-down. To do so implement derive a class from
		/// <see cref="SpecialFilterOperandBase"/> and provide the necessary logic to filter data in the derived
		/// class. Then add an entry to the filter drop-down list where the value of that entry is the instance 
		/// of that class. When the user selects the entry in the filter drop-down, the associated special operand
		/// will be used for filtering records.
		/// </para>
		/// </remarks>
		public bool IsSpecialOperand
		{
			get
			{
				return ! _isCellValue && this.Value is SpecialFilterOperandBase;
			}
		}

		#endregion // IsSpecialOperand
    
		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Private Methods

		#region GetDisplayTextHelper

		// JJD 02/17/12 - TFS101703 - added
		private static string GetDisplayTextHelper(object value, string displayText)
		{
			if (displayText != null)
				return displayText;

			// JJD 02/17/12 - TFS101703 - fall back to picking up the DisplayText from a ComparisonCondition
			ComparisonCondition cc = value as ComparisonCondition;

			if (cc != null)
				return cc.DisplayText;

			return null;
		}

		#endregion //GetDisplayTextHelper

		#endregion //Private Methods	
    
		#endregion //Methods
	}

	#endregion // FilterDropDownItem Class

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