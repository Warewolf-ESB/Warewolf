using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Calculations.Engine;

namespace Infragistics.Controls.Interactions.Primitives
{
	/// <summary>
	/// Provides information about it a function being displayed in the <see cref="FormulaEditorDialog"/>.
	/// </summary>
	public class FunctionInfo : 
		IComparable,
		IComparable<FunctionInfo>,
		IFormulaElement
	{
		#region Member Variables

		private FormulaEditorBase _editor;
		private CalculationFunction _function;
		private string _signature;

		#endregion  // Member Variables

		#region Constructor

		internal FunctionInfo(FormulaEditorBase editor, CalculationFunction function)
		{
			_editor = editor;
			_function = function;
		}

		#endregion  // Constructor

		#region Base Class Overrides

		#region ToString

		/// <summary>
		/// Gets the string representation of the <see cref="FunctionInfo"/>.
		/// </summary>
		public override string ToString()
		{
			return this.Name;
		}

		#endregion  // ToString

		#endregion  // Base Class Overrides

		#region Interfaces

		#region IComparable Members

		int IComparable.CompareTo(object obj)
		{
			if (obj is OperandInfo)
				return 1;

			return ((IComparable<FunctionInfo>)this).CompareTo(obj as FunctionInfo);
		}

		#endregion

		#region IComparable<FunctionInfo> Members

		int IComparable<FunctionInfo>.CompareTo(FunctionInfo other)
		{
			if (other == null)
				return 1;

			return String.Compare(this.Name, other.Name, StringComparison.CurrentCultureIgnoreCase);
		}

		#endregion

		#region IFormulaElement Members

		FormulaEditorBase IFormulaElement.Editor
		{
			get { return _editor; }
		}

		#endregion

		#endregion  // Interfaces

		#region Properties

		#region Description

		/// <summary>
		/// Gets the description of the function.
		/// </summary>
		public string Description
		{
			get { return _function.Description; }
		}

		#endregion  // Description

		#region Dialog

		/// <summary>
		/// Gets the <see cref="FormulaEditorDialog"/> in which the function is displayed.
		/// </summary>
		public FormulaEditorDialog Dialog
		{
			get { return _editor as FormulaEditorDialog; }
		}

		#endregion  // Dialog

		#region Function

		/// <summary>
		/// Gets the function represented by the <see cref="FunctionInfo"/>.
		/// </summary>
		public CalculationFunction Function
		{
			get { return _function; }
		}

		#endregion  // Function

		#region Name

		/// <summary>
		/// Gets the name of the function.
		/// </summary>
		public string Name
		{
			get { return _function.Name.ToUpper(); }
		}

		#endregion  // Name

		#region Signature

		/// <summary>
		/// Gets the signature of the function.
		/// </summary>
		public string Signature
		{
			get
			{
				if (_signature == null)
					_signature = FormulaEditorUtilities.GetFunctionSignature(_function);

				return _signature;
			}
		}

		#endregion  // Signature

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