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

	internal class ControlReferenceManagerCollection : ObservableCollectionExtended<ControlReferenceManager>
	{
		#region Member Vars

		private XamCalculationManager _calcManager;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="NamedReferenceCollection"/>.
		/// </summary>
		/// <param name="calcManager">Calc manager.</param>
		public ControlReferenceManagerCollection( XamCalculationManager calcManager )
			: base( false, true )
		{
			CoreUtilities.ValidateNotNull( calcManager );
			_calcManager = calcManager;
		}

		#endregion // Constructor

		#region Base Overrides

		#region NotifyItemsChanged

		protected override bool NotifyItemsChanged
		{
			get
			{
				return true;
			}
		}

		#endregion // NotifyItemsChanged

		#region OnItemAdded

		protected override void OnItemAdded( ControlReferenceManager item )
		{
			base.OnItemAdded( item );

			item._referenceManager.InitCalcManager( _calcManager );
		}

		#endregion // OnItemAdded

		#region OnItemRemoved

		protected override void OnItemRemoved( ControlReferenceManager item )
		{
			base.OnItemRemoved( item );

			item._referenceManager.UnregisterFromCalcManager( );
		}

		#endregion // OnItemRemoved

		#endregion // Base Overrides

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
		/// <param name="name">Name of the named reference.</param>
		/// <returns>Found NamedReference or null if no match is found.</returns>
		internal ControlReferenceManager GetItem( string name )
		{
			foreach ( var ii in this )
			{
				if ( RefParser.AreStringsEqual( name, ii._referenceManager._name, true ) )
					return ii;
			}

			return null;
		}

		/// <summary>
		/// Finds the ControlReferenceManager instance associated with the specified control.
		/// </summary>
		/// <param name="control">Control whose ControlReferenceManager to get.</param>
		/// <returns>Matching ControlReferenceManager instance or null if none is found.</returns>
		internal ControlReferenceManager GetItem( object control )
		{
			return ( from ii in this where ii.Control == control select ii ).FirstOrDefault( );
		}

		#endregion // GetItem

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
					ii._referenceManager.RegisterWithCalcManager( true );
			}
		}

		#endregion // RegisterAllReferencesHelper

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