using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Calculations.Engine;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Infragistics.Controls.Interactions.Primitives
{
	/// <summary>
	/// Represents a function category and contains all functions within that category.
	/// </summary>
	public class FunctionCategory : INotifyPropertyChanged
	{
		#region Member Variables

		private FilteredCollection<FunctionInfo> _functions;
		private bool _isExpanded;
		private string _name;

		#endregion  // Member Variables

		#region Constructor

		internal FunctionCategory(string name, List<FunctionInfo> functions)
		{
			_functions = new FilteredCollection<FunctionInfo>(functions);
			_name = name;
		}

		#endregion  // Constructor

		#region Interfaces

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Occurs when a property changes on the instance.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#endregion  // Interfaces

		#region Methods

		#region ApplyFunctionNameFilter

		internal void ApplyFunctionNameFilter(string functionSearchText)
		{
			Func<FunctionInfo, bool> filterPredicate = null;

			if (functionSearchText != null)
				filterPredicate = info => info.Name.IndexOf(functionSearchText, StringComparison.CurrentCultureIgnoreCase) >= 0;

			_functions.ApplyFilter(filterPredicate);
		}

		#endregion  // ApplyFunctionNameFilter

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

		#endregion  // Methods

		#region Properties

		#region Functions

		/// <summary>
		/// Gets the collection of functions in the category.
		/// </summary>
		public IEnumerable<FunctionInfo> Functions
		{
			get { return this._functions; }
		}

		#endregion  // Functions

		#region IsExpanded

		/// <summary>
		/// Gets or sets the value indicating whether the category is expanded in the UI.
		/// </summary>
		public bool IsExpanded
		{
			get { return _isExpanded; }
			set 
			{
				if (_isExpanded == value)
					return;

				_isExpanded = value;
				this.OnPropertyChanged("IsExpanded");
			}
		}

		#endregion  // IsExpanded

		#region Name

		/// <summary>
		/// Gets the name of the function category.
		/// </summary>
		public string Name
		{
			get { return _name; }
		}

		#endregion  // Name

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