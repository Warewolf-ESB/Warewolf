using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.Generic;
using Infragistics.Collections;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A collection of <see cref="ColumnTypeMapping"/> objects.
	/// </summary>
	public class ColumnTypeMappingsCollection : CollectionBase<ColumnTypeMapping>
	{
		#region Members

		Dictionary<Type, ColumnTypeMapping> _usedDataTypes;

		#endregion // Members

		/// <summary>
		/// Creates a new instance of the <see cref="ColumnTypeMappingsCollection"/> object.
		/// </summary>
		public ColumnTypeMappingsCollection()
		{
			this._usedDataTypes = new Dictionary<Type, ColumnTypeMapping>();
		}

		#region Constructor

		#endregion // Constructor

		#region Overrides

		#region OnItemAdded
		/// <summary>
		/// Invoked when an Item is added at the specified index.
		/// </summary>
		/// <param propertyName="index"></param>
		/// <param propertyName="item"></param>
		protected override void OnItemAdded(int index, ColumnTypeMapping item)
		{
			if (item == null || item.ColumnType == null || item.DataType == null)
				throw new InvalidColumnTypeMappingException();
			
			if (this._usedDataTypes.ContainsKey(item.DataType))
			{
				ColumnTypeMapping mapping = this._usedDataTypes[item.DataType];
				this.Items.Remove(mapping);
				this._usedDataTypes[item.DataType] = item;

                index = this.IndexOf(item);
			}
			else
				this._usedDataTypes.Add(item.DataType, item);

			base.OnItemAdded(index, item);
		}

		#endregion // OnItemAdded

		#region RemoveItem
		/// <summary>
		/// Removes the item at the specified index.
		/// </summary>
		/// <param propertyName="index"></param>
		/// <returns></returns>
		protected override bool RemoveItem(int index)
		{
			ColumnTypeMapping mapping = this.Items[index];
			this._usedDataTypes.Remove(mapping.DataType);

			return base.RemoveItem(index);
		}
		#endregion // RemoveItem

		#region ResetItems
		/// <summary>
		/// Removes all items from the collection.
		/// </summary>
		protected override void ResetItems()
		{
			this._usedDataTypes.Clear();
			base.ResetItems();
		}
		#endregion // ResetItems

		#endregion // Overrides
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