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
	/// Collection of <see cref="NamedReference"/> objects. 
	/// Used to specify <see cref="XamCalculationManager"/>'s <see cref="XamCalculationManager.NamedReferences"/> property.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>NamedReferenceCollection</b> collection contains <see cref="NamedReference"/> instances. NamedReferences in
	/// the collection must have unique <see cref="NamedReference.ReferenceId"/>s.
	/// </para>
	/// </remarks>
	/// <seealso cref="NamedReference"/>
	/// <seealso cref="XamCalculationManager.NamedReferences"/>
	public class NamedReferenceCollection : ObservableCollectionExtended<NamedReference>
	{
		#region Member Vars

		private Dictionary<string, NamedReference> _table;
		private XamCalculationManager _calcManager;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="NamedReferenceCollection"/>.
		/// </summary>
		public NamedReferenceCollection( )
			: base( false, true )
		{
			_table = new Dictionary<string, NamedReference>( StringComparer.InvariantCultureIgnoreCase );
		}

		#endregion // Constructor

		#region Base Overrides

		#region NotifyItemsChanged

		/// <summary>
		/// Indicates whether to call notification methods when items are added or removed.
		/// </summary>
		protected override bool NotifyItemsChanged
		{
			get
			{
				return true;
			}
		}

		#endregion // NotifyItemsChanged

		#region OnItemAdded

		/// <summary>
		/// Invoked when an item has been added.
		/// </summary>
		/// <param name="item">Item that was added.</param>
		protected override void OnItemAdded( NamedReference item )
		{
			base.OnItemAdded( item );

			item.InitializeParentCollection( this );
			_table.Add( ToTableKey( item ), item );

			item._referenceManager.RegisterWithCalcManager( );
		}

		#endregion // OnItemAdded

		#region OnItemAdding

		/// <summary>
		/// Invoked when an item is about to be added.
		/// </summary>
		/// <param name="item">The item that is being added.</param>
		protected override void OnItemAdding( NamedReference item )
		{
			this.ValidateItem( item );

			base.OnItemAdding( item );
		}

		#endregion // OnItemAdding

		#region OnItemRemoved

		/// <summary>
		/// Invoked when an item has been removed.
		/// </summary>
		/// <param name="item">The item that was removed.</param>
		protected override void OnItemRemoved( NamedReference item )
		{
			base.OnItemRemoved( item );

			_table.Remove( ToTableKey( item ) );

			item._referenceManager.UnregisterFromCalcManager( );
		}

		#endregion // OnItemRemoved

		#endregion // Base Overrides

		#region Indexers

		/// <summary>
		/// Gets the NamedReference in the collection with the specified reference id.
		/// </summary>
		/// <param name="referenceId"><see cref="NamedReference.ReferenceId"/> of the NamedReference to get.</param>
		/// <returns>NamedReference instance if a match is found otherwise null.</returns>
		public NamedReference this[string referenceId]
		{
			get
			{
				return this.GetItem( referenceId );
			}
		}

		#endregion // Indexers

		#region Properties

		#region Internal Properties

		#region CalcManager

		/// <summary>
		/// Gets the associated calc manager.
		/// </summary>
		internal XamCalculationManager CalcManager
		{
			get
			{
				return _calcManager;
			}
		}

		#endregion // CalcManager

		#endregion // Internal Properties

		#endregion // Properties

		#region Methods

		#region Internal Methods

		#region GetItem

		/// <summary>
		/// Finds a matching NamedReference. If none is found returns null.
		/// </summary>
		/// <param name="name">ReferenceId of the named reference.</param>
		/// <returns>Found NamedReference or null if no match is found.</returns>
		internal NamedReference GetItem( string name )
		{
			NamedReference nr;
			if ( _table.TryGetValue( ToTableKey( name ), out nr ) )
				return nr;

			return null;
		}

		#endregion // GetItem

		#region InitializeCalcManager

		/// <summary>
		/// Initializes the reference to the associated calc manager.
		/// </summary>
		/// <param name="calcManager"></param>
		internal void InitializeCalcManager( XamCalculationManager calcManager )
		{
			if ( _calcManager != calcManager && null != _calcManager )
				throw new InvalidOperationException( SRUtil.GetString("NamedReferencUsedMoreThanOnce") );

			_calcManager = calcManager;

			this.ReregisterAllReferences( );
		}

		#endregion // InitializeCalcManager

		#region OnChangingReferenceId

		/// <summary>
		/// This method is called when the ReferenceId property of a named reference that exists in this collection
		/// is about to be changed. It will raise an exception if there's another named reference in the collection
		/// with the same name as the 'newName'.
		/// </summary>
		/// <param name="item">Named reference whose ReferenceId property is changing.</param>
		/// <param name="newName">New name.</param>
		internal void OnChangingReferenceId( NamedReference item, string newName )
		{
			if ( _table.ContainsKey( ToTableKey( newName ) ) )
				throw new InvalidOperationException( SRUtil.GetString( "LE_InvalidOperationException_2", newName ) );

			_table.Remove( ToTableKey( item ) );

			_table.Add( ToTableKey( newName ), item );
		}

		#endregion // OnChangingReferenceId

		#region RegisterAllReferencesHelper

		/// <summary>
		/// Either adds or removes all references based on whether 'unregister' parameter is false or true, respectively.
		/// </summary>
		/// <param name="unregister">If true, the references will be removed. Otherwise references will be added.</param>
		private void RegisterAllReferencesHelper( bool unregister )
		{
			foreach ( var ii in this )
			{
				if ( unregister )
					ii._referenceManager.UnregisterFromCalcManager( );
				else
				{
					// Make sure the named reference is initialized with calc manager.
					// 
					ii._referenceManager.InitCalcManager( _calcManager );

					ii._referenceManager.RegisterWithCalcManager( true );
				}
			}
		}

		#endregion // RegisterAllReferencesHelper

		#region ReregisterAllReferences

		/// <summary>
		/// Adds all the references to the calc manager.
		/// </summary>
		internal void ReregisterAllReferences( )
		{
			this.RegisterAllReferencesHelper( false );
		}

		#endregion // ReregisterAllReferences

		#region UnregisterAllReferences

		/// <summary>
		/// Removes all the references from the calc manager.
		/// </summary>
		internal void UnregisterAllReferences( )
		{
			this.RegisterAllReferencesHelper( true );
		}

		#endregion // UnregisterAllReferences

		#endregion // Internal Methods

		#region Private Methods

		#region ToTableKey

		private string ToTableKey( NamedReference n )
		{
			return ToTableKey( n.ReferenceId );
		}

		private string ToTableKey( string name )
		{
			return name ?? string.Empty;
		} 

		#endregion // ToTableKey

		#region ValidateItem

		/// <summary>
		/// Validates to make sure the specified item is valid for adding to this collection.
		/// </summary>
		/// <param name="item">Item to be added.</param>
		private void ValidateItem( NamedReference item )
		{
			CoreUtilities.ValidateNotNull( item );

			if ( null != item.ParentCollection && item.ParentCollection != this )
				throw new InvalidOperationException( SRUtil.GetString( "LE_Exception_1" ) );

			// SSP 10/18/11 TFS91256
			// Apparently the collection editor of blend adds the item right away, which means
			// the reference id will not be set.
			// 
			//if ( string.IsNullOrEmpty( item.ReferenceId ) )
			//	throw new InvalidOperationException( SRUtil.GetString( "LE_InvalidOperationException_3" ) );

			// SSP 10/18/11 TFS91256
			// Related to above. Sice we can't prevent adding of an item with its reference id not set,
			// we should at least make sure the user doesn't add multiple items with duplicate reference ids.
			// 
			//if ( _table.ContainsKey( item.ReferenceId ) )
			if ( _table.ContainsKey( ToTableKey( item ) ) )
				throw new InvalidOperationException( SRUtil.GetString( "LE_InvalidOperationException_2", item.ReferenceId ) );
		}

		#endregion // ValidateItem

		#endregion // Private Methods

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