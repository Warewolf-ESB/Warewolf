using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Specialized;
using System.Collections;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Windows.Automation.Peers;
using Infragistics.AutomationPeers;
using System.Windows.Threading;
using System.Threading;
using Infragistics.Collections;
using Infragistics.Controls;
using Infragistics.Controls.Primitives;
using System.Windows.Data;
using System.Linq;



using Infragistics.Windows.Licensing;


using Infragistics.Calculations;
using Infragistics.Calculations.Engine;

namespace Infragistics.Calculations
{
	
	/// <summary>
	/// Defines a tree structure of references used by the formula editor to display to the user a list of
	/// fields/summaries or any other references in a data grid that can be used in a formula.
	/// </summary>
	public class CalculationReferenceNode
	{
		#region Member Variables

		private string _displayName;
		private bool _isDataReference;
		private ReferenceNodeType _nodeType;

		#endregion  // Member Variables

		#region Constructor

		/// <summary>
		/// Creates a new <see cref="CalculationReferenceNode"/> instance.
		/// </summary>
		/// <param name="displayName">The name to use for displaying this reference.</param>
		/// <param name="isDataReference">Indicates if the reference can be part of a formula.</param>
		/// <param name="nodeType">The type of reference</param>
		public CalculationReferenceNode(string displayName, bool isDataReference, ReferenceNodeType nodeType)
		{
			_displayName = displayName;
			_isDataReference = isDataReference;
			_nodeType = nodeType;
		}

		/// <summary>
		/// Creates a new <see cref="CalculationReferenceNode"/> instance.
		/// </summary>
		/// <param name="reference">The reference that the node represents.</param>
		/// <param name="displayName">The name to use for displaying this reference.</param>
		/// <param name="isDataReference">Indicates if the reference can be part of a formula.</param>
		/// <param name="nodeType">The type of reference</param>
		public CalculationReferenceNode(ICalculationReference reference, string displayName, bool isDataReference, ReferenceNodeType nodeType)
			: this(displayName, isDataReference, nodeType)
		{
			this.Reference = reference;
		}

		#endregion  // Constructor

		#region Properties

		/// <summary>
		/// Gets or sets the reference that the node represents.
		/// </summary>
		public ICalculationReference Reference
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the child references owned by this node.
		/// </summary>
		public ReadOnlyObservableCollection<CalculationReferenceNode> ChildReferences
		{
			get;
			set;
		}

		/// <summary>
		/// Indicates if the reference can be part of a formula. For example, a grid 
		/// or a band can't be part of a formula because they don't represent values
		/// however a column can be part of a formula.
		/// </summary>		
		public bool IsDataReference
		{
			get { return _isDataReference; }
		}

		/// <summary>
		/// Gets or sets the value indicating whether the node should be expanded if displayed in a tree and it has children.
		/// </summary>
		public bool IsExpanded
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the major sort priority for the node within its parent.
		/// </summary>
		public int SortPriority
		{
			get;
			set;
		}

		/// <summary>
		/// Returns the name to use for displaying this reference.
		/// </summary>
		public string DisplayName
		{
			get { return _displayName; }
		}

		/// <summary>
		/// Returns the resolved name to use for displaying this reference.
		/// </summary>
		public string DisplayNameResolved
		{
			get
			{
				if (_displayName != null)
					return _displayName;

				if (this.Reference != null)
					return this.Reference.ElementName;

				return null;
			}
		}

		/// <summary>
		/// Gets the type of reference represented by the node.
		/// </summary>
		public ReferenceNodeType NodeType
		{
			get { return _nodeType; }
		}

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