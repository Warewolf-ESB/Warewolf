using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Documents.Excel.Serialization;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;




using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 5/13/11 - Data Validations / Page Breaks
	/// <summary>
	/// Base class for all data validations rules which can be applied to a cell.
	/// </summary>
	/// <seealso cref="Excel.Worksheet.DataValidationRules"/>
	/// <seealso cref="AnyValueDataValidationRule"/>
	/// <seealso cref="ListDataValidationRule"/>
	/// <seealso cref="CustomDataValidationRule"/>
	/// <seealso cref="OneConstraintDataValidationRule"/>
	/// <seealso cref="TwoConstraintDataValidationRule"/>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 
	public

		 abstract class DataValidationRule

		: ICloneable 

	{
		#region Member Variables

		private string errorMessageDescription;
		private string errorMessageTitle;
		private DataValidationErrorStyle errorStyle;
		private string inputMessageDescription;
		private string inputMessageTitle;

		// MD 6/11/12 - TFS113884
		private string loadingWorkbookPath;

		private DataValidationRuleCollection parentCollection;
		private bool showErrorMessageForInvalidValue;
		private bool showInputMessage;

		#endregion  // Member Variables

		#region Constructor

		internal DataValidationRule()
		{
			this.errorStyle = DataValidationErrorStyle.Stop;
			this.showErrorMessageForInvalidValue = true;
			this.showInputMessage = true;
		}

		#endregion  // Constructor

		#region Interfaces


		#region ICloneable Members

		object ICloneable.Clone()
		{
			return this.Clone();
		}

		#endregion


		#endregion  // Interfaces

		#region Methods

		internal abstract Formula GetFormula1(string address);
		internal abstract Formula GetFormula2(string address);
		internal abstract void SetFormula1(Formula formula, string address);
		internal abstract void SetFormula2(Formula formula, string address);
		internal abstract void VerifyState(DataValidationRuleCollection collection, WorksheetReferenceCollection references);

		#region Public Methods

		/// <summary>
		/// Creates a copy of this rule which can be applied to other worksheets.
		/// </summary>
		public DataValidationRule Clone()
		{
			DataValidationRule rule = (DataValidationRule)this.MemberwiseClone();
			rule.parentCollection = null;
			return rule;
		}

		#endregion  // Public Methods

		#region Internal Methods

		#region OnAddedToCollection

		internal virtual void OnAddedToCollection(DataValidationRuleCollection parentCollection)
		{
			this.parentCollection = parentCollection;
		}

		#endregion  // OnAddedToCollection

		#region OnRemovedFromCollection

		internal virtual void OnRemovedFromCollection()
		{
			this.parentCollection = null;
		}

		#endregion  // OnRemovedFromCollection

		#region ValidateMessageTitle

		internal void ValidateMessage(string message, string propertyName, int maxCharacters)
		{
			if (string.IsNullOrEmpty(message) == false && maxCharacters < message.Length)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_DV_InvalidMessageLength", propertyName, maxCharacters));
		}

		#endregion  // ValidateMessageTitle

		#endregion  // Internal Methods

		#endregion  // Methods

		#region Properties

		#region Abstract Properties

		internal abstract bool AllowNullInternal { get; set; }
		internal abstract DataValidationOperatorType OperatorType { get; }
		internal abstract DataValidationType ValidationType { get; }

		#endregion  // Abstract Properties

		#region Public Properties

		#region ErrorMessageDescription

		/// <summary>
		/// Gets or sets the description which appears in the dialog box when an invalid value is applied to a cell
		/// in Microsoft Excel.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This value is only used when <see cref="ShowErrorMessageForInvalidValue"/> is True.
		/// </p>
		/// <p class="note">
		/// <b>Note:</b> the title cannot be more than 225 characters.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Occurs when the specified value is longer than 225 characters.
		/// </exception>
		/// <value>The description to show the user or null to use a default error description.</value>
		/// <seealso cref="ErrorMessageTitle"/>
		/// <seealse cref="ShowErrorMessageForInvalidValue"/>
		public string ErrorMessageDescription
		{
			get { return this.errorMessageDescription; }
			set
			{
				if (this.errorMessageDescription == value)
					return;

				this.ValidateMessage(value, "ErrorMessageDescription", 225);
				this.errorMessageDescription = value;
			}
		}

		#endregion  // ErrorMessageDescription

		#region ErrorMessageTitle

		/// <summary>
		/// Gets or sets the title which appears in the dialog box when an invalid value is applied to a cell
		/// in Microsoft Excel.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This value is only used when <see cref="ShowErrorMessageForInvalidValue"/> is True.
		/// </p>
		/// <p class="note">
		/// <b>Note:</b> the title cannot be more than 32 characters.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Occurs when the specified value is longer than 32 characters.
		/// </exception>
		/// <value>The title to show the user or null to use a default error dialog title.</value>
		/// <seealso cref="ErrorMessageDescription"/>
		/// <seealse cref="ShowErrorMessageForInvalidValue"/>
		public string ErrorMessageTitle
		{
			get { return this.errorMessageTitle; }
			set
			{
				if (this.errorMessageTitle == value)
					return;

				this.ValidateMessage(value, "ErrorMessageTitle", 32);
				this.errorMessageTitle = value;
			}
		}

		#endregion  // ErrorMessageTitle

		#region ErrorStyle

		/// <summary>
		/// Gets or sets the value which indicates whether the value is allowed when it is invalid and which options are given to 
		/// the user in the error dialog shown by Microsoft Excel.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This value is only used when <see cref="ShowErrorMessageForInvalidValue"/> is True.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// Occurs when the value is not a member of the <see cref="DataValidationErrorStyle"/> enumeration.
		/// </exception>
		/// <seealso cref="ShowErrorMessageForInvalidValue"/>
		public DataValidationErrorStyle ErrorStyle
		{
			get { return this.errorStyle; }
			set
			{
				if (this.errorStyle == value)
					return;

				if (Enum.IsDefined(typeof(DataValidationErrorStyle), value) == false)
					throw new InvalidEnumArgumentException("value", (int)value, typeof(DataValidationErrorStyle));

				this.errorStyle = value;
			}
		}

		#endregion  // ErrorStyle

		#region InputMessageDescription

		/// <summary>
		/// Gets or sets the description in the tooltip which appears when the user selects the cell in Microsoft Excel.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This value is only used when <see cref="ShowInputMessage"/> is True.
		/// </p>
		/// <p class="note">
		/// <b>Note:</b> the description cannot be more than 255 characters.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Occurs when the specified value is longer than 255 characters.
		/// </exception>
		/// <value>The description to show the user in the tooltip.</value>
		/// <seealso cref="InputMessageTitle"/>
		/// <seealse cref="ShowInputMessage"/>
		public string InputMessageDescription
		{
			get { return this.inputMessageDescription; }
			set
			{
				if (this.inputMessageDescription == value)
					return;

				this.ValidateMessage(value, "InputMessageDescription", 255);
				this.inputMessageDescription = value;
			}
		}

		#endregion  // InputMessageDescription

		#region InputMessageTitle

		/// <summary>
		/// Gets or sets the title in the tooltip which appears when the user selects the cell in Microsoft Excel.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This value is only used when <see cref="ShowInputMessage"/> is True.
		/// </p>
		/// <p class="note">
		/// <b>Note:</b> the title cannot be more than 32 characters.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Occurs when the specified value is longer than 32 characters.
		/// </exception>
		/// <value>The title to show the user in the tooltip.</value>
		/// <seealso cref="InputMessageDescription"/>
		/// <seealse cref="ShowInputMessage"/>
		public string InputMessageTitle
		{
			get { return this.inputMessageTitle; }
			set 
			{
				if (this.inputMessageTitle == value)
					return;

				this.ValidateMessage(value, "InputMessageTitle", 32);
				this.inputMessageTitle = value; 
			}
		}

		#endregion  // InputMessageTitle

		#region ShowErrorMessageForInvalidValue

		/// <summary>
		/// Gets or sets the value which indicates whether the error dialog should appear in Microsoft Excel when invalid data 
		/// is entered in the cell.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// When the value is False, invalid data can be entered into cells, but when the user chooses to subsequently circle 
		/// invalid values in Microsoft Excel, the cell will be circled.
		/// </p>
		/// </remarks>
		/// <value>True to show the error dialog for invalid cell data; False otherwise.</value>
		/// <seealso cref="ErrorMessageDescription"/>
		/// <seealso cref="ErrorMessageTitle"/>
		/// <seealso cref="ErrorStyle"/>
		public bool ShowErrorMessageForInvalidValue
		{
			get { return this.showErrorMessageForInvalidValue; }
			set { this.showErrorMessageForInvalidValue = value; }
		}

		#endregion  // ShowErrorMessageForInvalidValue

		#region ShowInputMessage

		/// <summary>
		/// Gets or sets the value which indicates whether to show the user an input prompt tooltip when the user selects 
		/// the cell in Microsoft Excel.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The input prompt will only be shown if this value is True and the <see cref="InputMessageDescription"/> is not null.
		/// </p>
		/// </remarks>
		/// <seealso cref="InputMessageDescription"/>
		/// <seealso cref="InputMessageTitle"/>
		public bool ShowInputMessage
		{
			get { return this.showInputMessage; }
			set { this.showInputMessage = value; }
		}

		#endregion  // ShowInputMessage

		#endregion  // Public Properties

		#region Internal Properties

		// MD 12/21/11 - TFS97840
		// Data validation formulas need to be parsed slightly differently, so they need their own formula types.
		#region FormulaType

		internal virtual FormulaType FormulaType
		{
			get { return FormulaType.NonListDataValidationFormula; }
		}

		#endregion  // FormulaType

		// MD 6/11/12 - TFS113884
		#region LoadingWorkbookPath

		internal string LoadingWorkbookPath
		{
			get { return this.loadingWorkbookPath; }
			set { this.loadingWorkbookPath = value; }
		}

		#endregion // LoadingWorkbookPath

		#region ParentCollection

		internal DataValidationRuleCollection ParentCollection
		{
			get { return this.parentCollection; }
		}

		#endregion  // ParentCollection

		#region Workbook

		internal Workbook Workbook
		{
			get
			{
				// MD 6/13/12 - CalcEngineRefactor
				//if (this.parentCollection == null)
				//    return null;
				//
				//return this.parentCollection.Worksheet.Workbook;
				Worksheet worksheet = this.Worksheet;

				if (worksheet == null)
					return null;

				return worksheet.Workbook;
			}
		}

		#endregion  // Workbook

		// MD 6/13/12 - CalcEngineRefactor
		#region Worksheet

		internal Worksheet Worksheet
		{
			get
			{
				if (this.parentCollection == null)
					return null;

				return this.parentCollection.Worksheet;
			}
		}

		#endregion  // Worksheet

		#endregion  // Internal Properties

		#endregion  // Properties
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