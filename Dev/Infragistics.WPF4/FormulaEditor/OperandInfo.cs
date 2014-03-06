using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Interactions.Primitives;
using Infragistics.Calculations;
using System.ComponentModel;
using System.Diagnostics;
using Infragistics.Calculations.Engine;

namespace Infragistics.Controls.Interactions.Primitives
{
	/// <summary>
	/// Represents an operand for functions or an item that owns operands.
	/// </summary>
	public class OperandInfo : 
		IComparable,
		IComparable<OperandInfo>,
		IFormulaElement, 
		INotifyPropertyChanged
	{
		#region Member Variables

		private FilteredCollection<OperandInfo> _children;
		private FormulaEditorBase _editor;
		private bool _isEnabled;
		private CalculationReferenceNode _node;
		private string _signature;

		#endregion  // Member Variables

		#region Constructor

		internal OperandInfo(FormulaEditorBase editor, CalculationReferenceNode node)
		{
			_editor = editor;
			_node = node;
			_children = FormulaEditorUtilities.GetOperands(_editor, _node.ChildReferences);

			this.ReinitializeEnabled();
		}

		#endregion  // Constructor

		#region Base Class Overrides

		#region Equals

		/// <summary>
		/// Determines whether the operand is equal to the specified object.
		/// </summary>
		/// <param name="obj">The object to test for equality.</param>
		public override bool Equals(object obj)
		{
			OperandInfo other = obj as OperandInfo;
			if (other == null)
				return false;

			if (this.NodeType != other.NodeType)
				return false;

			if (this.Reference != null && this.Reference == other.Reference)
				return true;

			return String.Equals(this.Name, other.Name, StringComparison.CurrentCultureIgnoreCase);
		}

		#endregion  // Equals

		#region GetHashCode

		/// <summary>
		/// Gets the hash code for the operand.
		/// </summary>
		public override int GetHashCode()
		{
			return this.Name.ToLower().GetHashCode();
		}

		#endregion  // GetHashCode

		#region ToString

		/// <summary>
		/// Gets the string representation of the <see cref="OperandInfo"/>.
		/// </summary>
		public override string ToString()
		{
			return this.Signature;
		}

		#endregion  // ToString

		#endregion  // Base Class Overrides

		#region Interfaces

		#region IComparable Members

		int IComparable.CompareTo(object obj)
		{
			if (obj is FunctionInfo)
				return -1;

			return ((IComparable<OperandInfo>)this).CompareTo(obj as OperandInfo);
		}

		#endregion

		#region IComparable<OperandInfo> Members

		int IComparable<OperandInfo>.CompareTo(OperandInfo other)
		{
			if (other == null)
				return 1;

			return String.Compare(this.Signature, other.Signature, StringComparison.CurrentCultureIgnoreCase);
		}

		#endregion

		#region IFormulaElement Members

		FormulaEditorBase IFormulaElement.Editor
		{
			get { return _editor; }
		}

		#endregion

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Occurs when a property changes on the instance.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#endregion  // Interfaces

		#region Methods

		#region ApplyOperandNameFilter

		internal void ApplyOperandNameFilter(Func<OperandInfo, bool> filterPredicate)
		{
			if (_children != null)
				_children.ApplyFilter(filterPredicate);
		}

		#endregion  // ApplyOperandNameFilter

		#region ClearOperandNameFilterRecursive

		internal void ClearOperandNameFilterRecursive()
		{
			if (_children == null)
				return;

			_children.ApplyFilter(null);

			foreach (var child in _children)
				child.ClearOperandNameFilterRecursive();
		}

		#endregion  // ClearOperandNameFilterRecursive

		#region DoesMatchFilterText

		internal bool DoesMatchFilterText(string filterSearchText)
		{
			return this.Name.IndexOf(filterSearchText, StringComparison.CurrentCultureIgnoreCase) >= 0;
		}

		#endregion  // DoesMatchFilterText

		#region OnPropertyChanged

		private void OnPropertyChanged(string propertyName)
		{
			var handler = this.PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion  // OnPropertyChanged

		#region ReinitializeEnabled

		internal void ReinitializeEnabled()
		{
			IFormulaProvider formulaProvider = _editor.FormulaProvider;
			if (formulaProvider == null)
				_isEnabled = true;
			else
				_isEnabled = _node.Reference != formulaProvider.Reference;
		}

		#endregion  // ReinitializeEnabled

		#endregion  // Methods

		#region Properties

		#region Dialog

		/// <summary>
		/// Gets the <see cref="FormulaEditorDialog"/> in which the operand or owner is displayed.
		/// </summary>
		public FormulaEditorDialog Dialog
		{
			get { return _editor as FormulaEditorDialog; }
		}

		#endregion  // Dialog

		#region Functions

		/// <summary>
		/// Gets the collection of operands or owners owned by this instance, if it is an owner.
		/// </summary>
		public FilteredCollection<OperandInfo> Children
		{
			get { return this._children; }
		}

		#endregion  // Functions

		#region IsDataReference

		/// <summary>
		/// Gets the value which indicates whether this operand can be the source or target for a formula.
		/// </summary>
		public bool IsDataReference
		{
			get { return _node.IsDataReference; }
		}

		#endregion  // IsDataReference

		#region IsEnabled

		/// <summary>
		/// Gets the value which indicates whether this operand is enabled and can be used in the formula.
		/// </summary>
		public bool IsEnabled
		{
			get { return _isEnabled; }
			set { _isEnabled = value; }
		}

		#endregion  // IsEnabled

		#region IsExpanded

		/// <summary>
		/// Gets or sets the value indicating whether the operand or owner is expanded in the UI.
		/// </summary>
		public bool IsExpanded
		{
			get { return _node.IsExpanded; }
			set
			{
				if (_node.IsExpanded == value)
					return;

				_node.IsExpanded = value;
				this.OnPropertyChanged("IsExpanded");
			}
		}

		#endregion  // IsExpanded

		#region Name

		/// <summary>
		/// Gets the name of the operand or owner.
		/// </summary>
		public string Name
		{
			// MD 10/21/11 - TFS93433
			// Make sure we never return a null Name.
			//get { return _node.DisplayNameResolved; }
			get 
			{
				string resolvedName = _node.DisplayNameResolved;

				if (resolvedName == null)
				{
					resolvedName = FormulaEditorUtilities.GetString("OperandNotInitialized");

					if (resolvedName == null)
						resolvedName = "<Error: Operand not initialized>";
				}

				return resolvedName;
			}
		}

		#endregion  // Name

		#region NodeType

		/// <summary>
		/// Gets the type of reference represented by the node.
		/// </summary>
		public ReferenceNodeType NodeType
		{
			get { return _node.NodeType; }
		}

		#endregion  // NodeType

		#region Reference

		/// <summary>
		/// Gets the <see cref="ICalculationReference"/> representing the operand.
		/// </summary>
		public ICalculationReference Reference
		{
			get { return _node.Reference; }
		}

		#endregion  // Reference

		#region Signature

		/// <summary>
		/// Gets the signature of the operand when used in the formula.
		/// </summary>
		public string Signature
		{
			get
			{
				if (_signature == null)
				{
					bool isUsingReferenceIndexing;

					// MD 6/25/12 - TFS113177
					//_signature = FormulaEditorUtilities.GetOperandSignature(_editor.FormulaProvider, this, _editor.Functions, out isUsingReferenceIndexing);
					int indexOffset;
					_signature = FormulaEditorUtilities.GetOperandSignature(_editor.FormulaProvider, this, _editor.Functions, out isUsingReferenceIndexing, out indexOffset);
				}

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