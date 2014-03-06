using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Media;

namespace Infragistics.Controls.Layouts.Primitives
{
	#region ILayoutItem Interface

	/// <summary>
	/// Interface for implementing layout items.
	/// </summary>
	public interface ILayoutItem
	{
		#region Visibility

		/// <summary>
		/// Indicates the visibility state of the item. Items that are collapsed are ignored.
		/// </summary>
		Visibility Visibility { get; }

		#endregion // Visibility

		#region MaximumSize

		/// <summary>
		/// Gets the maximum size of the layout item.
		/// </summary>
		Size MaximumSize { get; }

		#endregion // MaximumSize

		#region MinimumSize

		/// <summary>
		/// Gets the minimum size of the layout item.
		/// </summary>
		Size MinimumSize { get; }

		#endregion // MinimumSize

		#region PreferredSize

		/// <summary>
		/// Gets the preferred size of the layout item.
		/// </summary>
		Size PreferredSize { get; }

		#endregion // PreferredSize
	}

	#endregion // ILayoutItem Interface

	#region IAutoSizeLayoutItem Interface

	// SSP 7/24/09 NAS9.2 Auto-sizing
	// 
	/// <summary>
	/// Used by the grid-bag layout manager to ensure that auto-sized items are not resized smaller 
	/// than their preferred sizes when shrinking all the items to auto-fit in the available extent.
	/// </summary>
	public interface IAutoSizeLayoutItem : ILayoutItem
	{
		/// <summary>
		/// Returns true to indicate that the width is auto-sized and thus the item should not be 
		/// resized smaller than its preferred width when proportionally shrinking all the items to
		/// auto-fit in the available space for the layout.
		/// </summary>
		bool IsWidthAutoSized { get; }

		/// <summary>
		/// Returns true to indicate that the height is auto-sized and thus the item should not be 
		/// resized smaller than its preferred width when proportionally shrinking all the items to
		/// auto-fit in the available space for the layout.
		/// </summary>
		bool IsHeightAutoSized { get; }
	}

	#endregion // IAutoSizeLayoutItem Interface

	#region ILayoutContainer Interface

	/// <summary>
	/// ILayoutContainer interface.
	/// </summary>
	public interface ILayoutContainer
	{
		#region GetBounds

		/// <summary>
		/// Returns the container bounds.
		/// </summary>        
		/// <param name="containerContext">Context used in calls to the container.</param>
		Rect GetBounds( object containerContext );

		#endregion // GetBounds

		#region PositionItem

		/// <summary>
		/// Called by the layout manager to position a layout item.
		/// </summary>
		/// <param name="item">The <see cref="ILayoutItem"/> to position.</param>
		/// <param name="rect">The <see cref="Rect"/> of the item.</param>
		/// <param name="containerContext">Context used in calls to the container.</param>
		void PositionItem( ILayoutItem item, Rect rect, object containerContext );

		#endregion // PositionItem
	}

	#endregion // ILayoutContainer Interface

	#region LayoutItemsCollection Class

	/// <summary>
	/// ILayoutItem collection.
	/// </summary>
	public class LayoutItemsCollection : ICollection<ILayoutItem>
	{
		#region Private variables

		private Dictionary<ILayoutItem, object> _layoutItemsConstraints;

		private List<ILayoutItem> _layoutItems;
		private LayoutManagerBase _layoutManager;

		#endregion // Private variables

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="layoutManager">The <see cref="LayoutManagerBase"/>.</param>
		public LayoutItemsCollection( LayoutManagerBase layoutManager )
		{
			if ( null == layoutManager )
				throw new ArgumentNullException( "layoutManager" );

			_layoutManager = layoutManager;
			_layoutItems = new List<ILayoutItem>( );
		}

		#endregion // Constructor

		#region Properties

		#region Private/Internal Properties

		#region LayoutItemsConstraints

		/// <summary>
		/// Layout items with their constraints.
		/// </summary>
		private Dictionary<ILayoutItem, object> LayoutItemsConstraints
		{
			get
			{
				if ( null == this._layoutItemsConstraints )
				{
					this._layoutItemsConstraints = new Dictionary<ILayoutItem, object>( );
				}

				return this._layoutItemsConstraints;
			}
		}

		#endregion // LayoutItemsConstraints

		#region LayoutManager






		private LayoutManagerBase LayoutManager
		{
			get
			{
				return this._layoutManager;
			}
		}

		#endregion // LayoutManager

		#endregion // Private/Internal Properties

		#region Protected Properties

		#region LayoutItems

		/// <summary>
		/// Layout items.
		/// </summary>
		protected List<ILayoutItem> LayoutItems
		{
			get
			{
				return _layoutItems;
			}
		}

		#endregion // LayoutItems

		#endregion // Protected Properties

		#region Public Properties

		#region Count

		/// <summary>
		/// Returns the number of items this LayoutItemsCollection contains.
		/// </summary>
		public int Count
		{
			get
			{
				return this.LayoutItems.Count;
			}
		}

		#endregion // Count

		#region Indexer

		/// <summary>
		/// Gets the item at specified index.
		/// </summary>
		public ILayoutItem this[int index]
		{
			get
			{
				return this.GetItem( index );
			}
		}

		#endregion // Indexer

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Private/Internal Methods

		#region InternalSetConstraint

		/// <summary>
		/// Sets the constraint for the item. If the item doesn't exist in the collection, its added.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="constraint"></param>
		/// <param name="index">Specify -1 to add the item at the end of the list.</param>
		private void InternalSetConstraint( ILayoutItem item, object constraint, int index )
		{
			this.ValidateConstraintObject( constraint );

			if ( !this.LayoutItemsConstraints.ContainsKey( item ) )
			{
				if ( index < 0 )
					this.LayoutItems.Add( item );
				else
					this.LayoutItems.Insert( index, item );
			}

			this.LayoutItemsConstraints[item] = constraint;

			// Notify that the layout has changed.
			//
			this.OnLayoutChanged( );
		}

		#endregion // InternalSetConstraint

		#region OnLayoutChanged

		/// <summary>
		/// This method is called whenever a layout item is added, removed or the layout is cleared. Implementation of this method calls InvalidateLayout to invalidate any cached information.
		/// </summary>
		private void OnLayoutChanged( )
		{
			this.LayoutManager.InternalOnLayoutChanged( );
		}

		#endregion // OnLayoutChanged

		#region ValidateConstraintObject

		/// <summary>
		/// Implementation should throw an exception if the passed in constraint is not a valid
		/// constraint for this layout manager. It usually checks the type.
		/// </summary>
		/// <param name="constraint"></param>
		private void ValidateConstraintObject( object constraint )
		{
			this.LayoutManager.InternalValidateConstraints( constraint );
		}

		#endregion // ValidateConstraintObject

		#endregion // Private/Internal Methods

		#region Public Methods

		#region Add

		/// <summary>
		/// Adds an item to be managed by this layout manager. It uses null as the constraint so the underlying layout manager must support null constraints.
		/// </summary>
		/// <param name="item">The item to add to the layout manager.</param>
		public void Add( ILayoutItem item )
		{
			this.Add( item, null );
		}

		/// <summary>
		/// Adds an item to be managed by this layout manager.
		/// </summary>
		/// <param name="item">The item to add to the layout manager.</param>
		/// <param name="constraint">The constraint to assign to the item.</param>
		public void Add( ILayoutItem item, object constraint )
		{
			this.InternalSetConstraint( item, constraint, -1 );
		}

		#endregion // AddItem

		#region Contains

		/// <summary>
		/// Returns true if the passed in item contained in this layout manager.
		/// </summary>
		/// <param name="item">The <see cref="ILayoutItem"/> to check for in the collection.</param>
		/// <returns>True if the specified item is contained in this layout manager.</returns>
		public bool Contains( ILayoutItem item )
		{
			return this.LayoutItemsConstraints.ContainsKey( item );
		}

		#endregion // Contains

		#region Clear

		/// <summary>
		/// Removes all the items.
		/// </summary>
		public void Clear( )
		{
			this.LayoutItems.Clear( );
			this.LayoutItemsConstraints.Clear( );

			// Notify that the layout has changed.
			//
			this.OnLayoutChanged( );
		}

		#endregion // Clear

		#region CopyTo

		/// <summary>
		/// Copies the items from the collection into the array.
		/// </summary>
		/// <param name="array">Array to which to copy items.</param>
		/// <param name="arrayIndex">Index in the array at which to begin copying items.</param>
		public void CopyTo( ILayoutItem[] array, int arrayIndex )
		{
			_layoutItems.CopyTo( array, arrayIndex );
		}

		#endregion // CopyTo

		#region GetConstraint

		/// <summary>
		/// Returns the constraint object associated with the item. Throws an exception if the item
		/// does not exist (ie. it hasn't been added through AddLayoutItem method has been removed).
		/// </summary>
		/// <param name="item">The <see cref="ILayoutItem"/> whose constraint should be retrieved.</param>
		/// <returns>The constraint object associated with the item.</returns>
		public object GetConstraint( ILayoutItem item )
		{
            
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

            object constraint;

            if (!this.LayoutItemsConstraints.TryGetValue(item, out constraint))
#pragma warning disable 436
				throw new ArgumentException(SR.GetString("LE_GridBag_ItemNotInLayoutItems", item), "item");
#pragma warning restore 436

            return constraint;
        }

		#endregion // GetConstraint

		#region GetItem

		/// <summary>
		/// Gets the item at specified index.
		/// </summary>
		/// <param name="index">The index of the item to retrieve.</param>
		/// <returns>The <see cref="ILayoutItem"/> at the specified index.</returns>
		public ILayoutItem GetItem( int index )
		{
			return (ILayoutItem)this.LayoutItems[index];
		}

		#endregion // GetItem

		#region IndexOf

		/// <summary>
		/// Retruns the index of the passed in item in the layout items collection.
		/// </summary>
		/// <param name="item">The <see cref="ILayoutItem"/> whose index should be retrieved.</param>
		/// <returns>The index of the specified item, or -1 if the item was not found.</returns>
		/// <remarks>
		/// <p>Layout manager keeps track of the order in which items are added. Some layout managers may layout items in the order in which they were added.</p>
		/// </remarks>
		public int IndexOf( ILayoutItem item )
		{
			return this.LayoutItems.IndexOf( item );
		}

		#endregion // IndexOf

		#region Insert

		/// <summary>
		/// Inserts the item at specified index. It uses null as the constraint so the underlying layout manager must support null constraints.
		/// </summary>
		/// <param name="index">Specify -1 to add the item at the end of the list.</param>
		/// <param name="item">The <see cref="ILayoutItem"/> to add to the collection.</param>
		public void Insert( int index, ILayoutItem item )
		{
			this.Insert( index, item, null );
		}

		/// <summary>
		/// Inserts the item at specified index in with the specified constraint.
		/// </summary>
		/// <param name="index">Specify -1 to add the item at the end of the list.</param>
		/// <param name="item">The <see cref="ILayoutItem"/> to add to the collection.</param>
		/// <param name="constraint">The constraint to apply to the specified item.</param>
		public void Insert( int index, ILayoutItem item, object constraint )
		{
			this.InternalSetConstraint( item, constraint, index );
		}

		#endregion // Insert

		#region Remove

		/// <summary>
		/// Removes an item from this layout manager if it exists.
		/// </summary>
		/// <param name="item">The <see cref="ILayoutItem"/> to remove.</param>
		public bool Remove( ILayoutItem item )
		{
			if ( this.LayoutItemsConstraints.ContainsKey( item ) )
			{
				this.LayoutItems.Remove( item );
				this.LayoutItemsConstraints.Remove( item );

				// Notify that the layout has changed.
				//
				this.OnLayoutChanged( );
				return true;
			}

			return false;
		}

		#endregion // Remove

		#region RemoveAt

		/// <summary>
		/// Removes the item at specified index.
		/// </summary>
		/// <param name="index">The index of the item to remove.</param>
		public void RemoveAt( int index )
		{
			this.LayoutItems.RemoveAt( index );
		}

		#endregion // RemoveAt

		#region SetConstraint

		/// <summary>
		/// Sets the constraint for the item. If the item doesn't exist in the collection, its added.
		/// </summary>
		/// <param name="item">The <see cref="ILayoutItem"/> whose constraint should be set.</param>
		/// <param name="constraint">The constraint to assign to the specified object.</param>
		public void SetConstraint( ILayoutItem item, object constraint )
		{
			this.InternalSetConstraint( item, constraint, -1 );
		}

		#endregion // SetConstraint

		#endregion // Public Methods

		#endregion // Methods

		#region ICollection<ILayoutItem> Members

		#region Collection<ILayoutItem>.IsReadOnly

		/// <summary>
		/// Indicates whether the collection is read-only.
		/// </summary>
		bool ICollection<ILayoutItem>.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		#endregion // Collection<ILayoutItem>.IsReadOnly

		#region IEnumerable.GetEnumerator

		/// <summary>
		/// IEnumerable Interface Implementation. Returns a type safe enumerator.
		/// </summary>
		/// <returns></returns>
		IEnumerator IEnumerable.GetEnumerator( )
		{
			return _layoutItems.GetEnumerator( );
		}

		#endregion // IEnumerable.GetEnumerator

		#region IEnumerable<ILayoutItem> Members

		IEnumerator<ILayoutItem> IEnumerable<ILayoutItem>.GetEnumerator( )
		{
			return _layoutItems.GetEnumerator( );
		}

		#endregion // IEnumerable<ILayoutItem> Members

		#endregion // ICollection<ILayoutItem> Members
	}

	#endregion // LayoutItemsCollection Class

	#region LayoutManagerBase Class

	/// <summary>
	/// Base class for others to implement their own layout managers.
	/// </summary>
	public abstract class LayoutManagerBase
	{
		#region Private variables

		private LayoutItemsCollection _layoutItems;

		#endregion // Private variables

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		public LayoutManagerBase( )
		{
		}

		#endregion // Constructor

		#region Methods

		#region Private/Internal Methods

		#region GetContainerBoundsHelper

		// SSP 2/16/10 TFS27086
		// 
		/// <summary>
		/// If the container rect's width or height is NaN or infinity then it will use the preferred width or height for it.
		/// </summary>
		/// <param name="container"></param>
		/// <param name="containerContext"></param>
		/// <param name="lm"></param>
		/// <returns></returns>
		internal static Rect GetContainerBoundsHelper( ILayoutContainer container, object containerContext, LayoutManagerBase lm )
		{
			Rect rect = container.GetBounds( containerContext );
			bool assignWidth = double.IsInfinity( rect.Width ) || double.IsNaN( rect.Width );
			bool assignHeight = double.IsInfinity( rect.Height ) || double.IsNaN( rect.Height );

			if ( assignWidth || assignHeight )
			{
				Size preferredSize = lm.CalculatePreferredSize( container, containerContext );

				if ( assignWidth )
					rect.Width = preferredSize.Width;

				if ( assignHeight )
					rect.Height = preferredSize.Height;
			}

			return rect;
		}

		#endregion // GetContainerBoundsHelper

		#region InternalOnLayoutChanged






		internal void InternalOnLayoutChanged( )
		{
			this.OnLayoutChanged( );
		}

		#endregion // InternalOnLayoutChanged

		#region InternalValidateConstraints







		internal void InternalValidateConstraints( object constraint )
		{
			this.ValidateConstraintObject( constraint );
		}

		#endregion // InternalValidateConstraints

		#endregion // Private/Internal Methods

		#region Protected Methods

		#region OnLayoutChanged

		/// <summary>
		/// This method is called whenever a layout item is added, removed or the layout is cleared. Implementation of this method calls InvalidateLayout to invalidate any cached information.
		/// </summary>
		protected virtual void OnLayoutChanged( )
		{
			this.InvalidateLayout( );
		}

		#endregion // OnLayoutChanged

		#region ValidateConstraintObject

		/// <summary>
		/// Implementation should throw an exception if the passed in constraint is not a valid
		/// constraint for this layout manager. It usually checks the type.
		/// </summary>
		/// <param name="constraint">The constraint to check.</param>
		protected abstract void ValidateConstraintObject( object constraint );

		#endregion // ValidateConstraintObject

		#endregion // Protected Methods

		#region Public Methods

		#region CalculateMaximumSize

		/// <summary>
		/// Calculates the maximum size required to layout the items at their maximum sizes.
		/// </summary>
		/// <param name="container">Object that implements the ILayoutContainer to provide bounds information</param>
		/// <param name="containerContext">Context used in calls to the <paramref name="container"/></param>
		/// <returns>A <see cref="Size"/> object representing the minimum size required to layout the items.</returns>
		public abstract Size CalculateMaximumSize( ILayoutContainer container, object containerContext );

		#endregion // CalculateMaximumSize

		#region CalculateMinimumSize

		/// <summary>
		/// Calculates the minimum size required to layout the items.
		/// </summary>
		/// <param name="container">Object that implements the ILayoutContainer to provide bounds information</param>
		/// <param name="containerContext">Context used in calls to the <paramref name="container"/></param>
		/// <returns>A <see cref="Size"/> object representing the minimum size required to layout the items.</returns>
		public abstract Size CalculateMinimumSize( ILayoutContainer container, object containerContext );

		#endregion // CalculateMinimumSize

		#region CalculatePreferredSize

		/// <summary>
		/// Calculates the preferred size required to layout the items.
		/// </summary>
		/// <param name="container">Object that implements the ILayoutContainer to provide bounds information</param>
		/// <param name="containerContext">Context used in calls to the <paramref name="container"/></param>
		/// <returns>A <see cref="Size"/> object representing the preferred size required to layout the items.</returns>
		public abstract Size CalculatePreferredSize( ILayoutContainer container, object containerContext );

		#endregion // CalculatePreferredSize

		#region InvalidateLayout

		/// <summary>
		/// Invalidates any cached information so the layout manager recalculates everything next.
		/// </summary>
		public virtual void InvalidateLayout( )
		{
		}

		#endregion // InvalidateLayout

		#region LayoutContainer

		/// <summary>
		/// Lays out items contained in this layout manager by calling PositionItem off the
		/// passed in container for each item.
		/// </summary>
		/// <param name="container">Object that implements the ILayoutContainer to provide bounds information</param>
		/// <param name="containerContext">Context used in calls to the <paramref name="container"/></param>
		public abstract void LayoutContainer( ILayoutContainer container, object containerContext );

		#endregion // LayoutContainer

		#endregion // Public Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region LayoutItems

		/// <summary>
		/// Layout items collection.
		/// </summary>
		public LayoutItemsCollection LayoutItems
		{
			get
			{
				if ( null == this._layoutItems )
					this._layoutItems = new LayoutItemsCollection( this );

				return this._layoutItems;
			}
		}

		#endregion // LayoutItems

		#endregion // Public Properties

		#endregion // Properties
	}

	#endregion // LayoutManagerBase Class
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