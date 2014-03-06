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
using System.Windows.Controls.Primitives;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Specialized;

namespace Infragistics
{
	#region ISupportRecycling

	/// <summary>
	/// An interface that should be implemented on objects that represent control objects that should be Recycled.
	/// </summary>
	public interface ISupportRecycling
	{
		/// <summary>
		/// Gets the <see cref="Type"/> of the <see cref="FrameworkElement"/> that is being recycled.
		/// </summary>
		Type RecyclingElementType
		{
			get;
		}

		/// <summary>
		/// Offers another way to recyle an element, other than Type.
		/// </summary>
		string RecyclingIdentifier
		{
			get;
		}

		/// <summary>
		/// Creates a new instance of the <see cref="FrameworkElement"/> that represents the object.
		/// </summary>
		/// <returns></returns>
		FrameworkElement CreateInstanceOfRecyclingElement();

		/// <summary>
		/// Invoked when a <see cref="FrameworkElement"/>  is being attached to the object.
		/// </summary>
		/// <param name="element"></param>
		void OnElementAttached(FrameworkElement element);

		/// <summary>
		/// Invoked before a <see cref="FrameworkElement"/> is released from the object.
		/// </summary>
		/// <param name="element"></param>
		/// <returns>Returns false, if the element isn't released.</returns>
		bool OnElementReleasing(FrameworkElement element);

		/// <summary>
		/// Invoked when a <see cref="FrameworkElement"/> is no longer attached to the object. 
		/// </summary>
		/// <param name="element"></param>
		void OnElementReleased(FrameworkElement element);

		/// <summary>
		/// Gets/sets a value that determines if the <see cref="FrameworkElement"/> attached has been modified
		/// in such a way that it should just be thrown away when the object is done with it. 
		/// </summary>
		bool IsDirty
		{
			get;
			set;
		}

		/// <summary>
		/// Gets/sets the actual <see cref="FrameworkElement"/> that is attached to the object. If no object is attached
		/// then null is returned. 
		/// </summary>
		FrameworkElement AttachedElement
		{
			get;
			set;
		}
	}

	#endregion // ISupportRecycling

	#region IRecyclableElement
	/// <summary>
	/// An interface for objects that will be managed by the <see cref="RecyclingManager"/> which will allow certain objects to be
	/// temporarily allow the item to avoid recycling.
	/// </summary>
	public interface IRecyclableElement
	{
		/// <summary>
		/// Gets/sets if the object should be recycled.
		/// </summary>
		bool DelayRecycling
		{
			get;
			set;
		}

		/// <summary>
		/// Gets/sets the <see cref="Panel"/> that owns this element. 
		/// </summary>
		Panel OwnerPanel
		{
			get;
			set;
		}
	}
	#endregion //IRecyclableElement

	#region IRecyclableElementHost
	/// <summary>
	/// Interface implemented by a panel that hosts the elements for <see cref="IRecyclableElement"/> items.
	/// </summary>
	internal interface IRecyclableElementHost
	{
		/// <summary>
		/// Invoked when an element is associated with an item in the panel
		/// </summary>
		/// <param name="element">The element being associated with an item</param>
		/// <param name="item">The item represented by the specified element</param>
		/// <param name="isNewlyRealized">True if the element is new; false if the element is being recycled</param>
		void OnElementAttached(ISupportRecycling item, FrameworkElement element, bool isNewlyRealized);

		/// <summary>
		/// Invoked when an element is detached from an item.
		/// </summary>
		/// <param name="element">The element being released</param>
		/// <param name="item">The item that was represented by the element</param>
		/// <param name="isRemoved">True if the element is being removed from the children; otherwise false if the element is being kept for potential recycling later</param>
		void OnElementReleased(ISupportRecycling item, FrameworkElement element, bool isRemoved);

		/// <summary>
		/// Invoked when an element being released is to be considered for recycling.
		/// </summary>
		/// <param name="element">The element being released</param>
		/// <param name="item">The item that was represented by the element</param>
		/// <returns>Return true to indicate that the element should be removed from the panel; otherwise return false to allow the element to be recyled.</returns>
		bool ShouldRemove(ISupportRecycling item, FrameworkElement element);
	} 
	#endregion // IRecyclableElementHost
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