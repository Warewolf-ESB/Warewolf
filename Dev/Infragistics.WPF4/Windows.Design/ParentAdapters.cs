using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Windows.Design.Interaction;
using Microsoft.Windows.Design.Model;
using Microsoft.Windows.Design.Services;

namespace Infragistics.Windows.Design
{
	// AS 10/28/10 TFS16533
#if VS12
	internal
#else
    /// <summary>
    /// Custom ParentAdapter that will delegate calls to the registered parent adapter for a given type.
    /// </summary>
    public
#endif
		class ProxyParentAdapater : ParentAdapter
	{
		#region Member Variables

		private WeakReference _lastAdapter = null;
		private Type _baseAdapterType;

		#endregion // Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ProxyParentAdapater"/>
		/// </summary>
		/// <param name="elementParentAdapterType">The element type whose registered ParentAdapter will be used to perform the parenting operations.</param>
		public ProxyParentAdapater(Type elementParentAdapterType)
		{
			if ( elementParentAdapterType == null )
				throw new ArgumentNullException("elementParentAdapterType");

			_baseAdapterType = elementParentAdapterType;
		}
		#endregion // Constructor

		#region Base class overrides
		/// <summary>
		/// Gets a value indicating whether the specified parent object can be a parent to an object of the specified type.
		/// </summary>
		/// <param name="parent">ModelItem representing the parent</param>
		/// <param name="childType">The type of child item.</param>
		/// <returns></returns>
		public override bool CanParent( ModelItem parent, Type childType )
		{
			return this.GetBaseAdapter(parent).CanParent(parent, childType);
		}

		/// <summary>
		/// Gets a value indicating whether the specified child item is a child of the specified parent item.
		/// </summary>
		/// <param name="parent">ModelItem representing the parent</param>
		/// <param name="child">The model item that represents the child</param>
		/// <returns></returns>
		public override bool IsParent( ModelItem parent, ModelItem child )
		{
			return this.GetBaseAdapter(parent).IsParent(parent, child);
		}

		/// <summary>
		/// Changes the parent of an object to another parent.
		/// </summary>
		/// <param name="newParent">ModelItem representing the parent</param>
		/// <param name="child">The model item that represents the child</param>
		public override void Parent(ModelItem newParent, ModelItem child)
		{
			this.GetBaseAdapter(newParent).Parent(newParent, child);
		}

		/// <summary>
		/// Changes the parent of an object to another parent.
		/// </summary>
		/// <param name="newParent">ModelItem representing the parent</param>
		/// <param name="child">The model item that represents the child</param>
		/// <param name="insertionIndex">The order of control in the children collection.</param>
		public override void Parent( ModelItem newParent, ModelItem child, int insertionIndex )
		{
			this.GetBaseAdapter(newParent).Parent(newParent, child, insertionIndex);
		}

		/// <summary>
		/// Redirect a reference from one parent to another.
		/// </summary>
		/// <param name="parent">ModelItem representing the parent</param>
		/// <param name="childType">The type of child item.</param>
		/// <returns></returns>
		public override ModelItem RedirectParent( ModelItem parent, Type childType )
		{
			return this.GetBaseAdapter(parent).RedirectParent(parent, childType);
		}

		/// <summary>
		/// Replaces the current parent of the specified child with a new parent.
		/// </summary>
		/// <param name="currentParent">ModelItem representing the parent</param>
		/// <param name="newParent">The item that will become the new parent of child.</param>
		/// <param name="child">The child item.</param>
		public override void RemoveParent( ModelItem currentParent, ModelItem newParent, ModelItem child )
		{
			this.GetBaseAdapter(currentParent).RemoveParent(currentParent, newParent, child);
		}
		#endregion // Base class overrides

		#region Methods

		#region GetBaseAdapter
		internal ParentAdapter GetBaseAdapter( ModelItem parent )
		{
			if ( parent == null )
				throw new ArgumentNullException("parent");

			ParentAdapter adapter = Utilities.GetWeakReferenceTargetSafe(_lastAdapter) as ParentAdapter;

			if ( adapter == null )
			{
				var view = DesignerView.FromContext(parent.Context);
				var adapterService = view.Context.Services.GetService<AdapterService>();
				adapter = adapterService.GetAdapter<ParentAdapter>(_baseAdapterType);
				_lastAdapter = new WeakReference(adapter);
			}

			return adapter;
		}
		#endregion // GetBaseAdapter

		#endregion // Methods
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