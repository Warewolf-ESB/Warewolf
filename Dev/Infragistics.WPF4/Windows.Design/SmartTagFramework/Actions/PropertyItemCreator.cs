using System;

namespace Infragistics.Windows.Design.SmartTagFramework
{
	/// <summary>
	/// Helper class for creating DesignerActionPropertyItems
	/// </summary>
	public class PropertyItemCreator : IDisposable
	{
		private Type						_owningType;
		private DesignerActionItemGroup		_group;
		private int							_orderNumber;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="owningType"></param>
		/// <param name="group"></param>
		public PropertyItemCreator(Type owningType, DesignerActionItemGroup group)
		{
			this._owningType = owningType;
			this._group = group;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="owningType"></param>
		/// <param name="group"></param>
		/// <param name="orderNumber"></param>
		public PropertyItemCreator(Type owningType, DesignerActionItemGroup group, int orderNumber)
			: this(owningType, group)
		{
			this._orderNumber = orderNumber;
		}

		/// <summary>
		/// Returns a DesignerActionPropertyItem instance for the specified property.
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public DesignerActionPropertyItem GetPropertyActionItem(string propertyName)
		{
			this._orderNumber++;

			return new DesignerActionPropertyItem(propertyName, propertyName, this._owningType, this._group, this._orderNumber);
		}

		#region IDisposable Members

		void IDisposable.Dispose()
		{
		}

		#endregion
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