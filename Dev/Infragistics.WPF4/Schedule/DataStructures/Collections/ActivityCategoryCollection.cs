using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Media;



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

using Infragistics.Collections;

namespace Infragistics.Controls.Schedules

{
	#region ActivityCategoryCollection Class

	
	

	/// <summary>
	/// Used by the <see cref="Resource"/>'s <see cref="Resource.CustomActivityCategories"/> property.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// ActivityCategoryCollection is a collection of activity categories.
	/// Resource's <see cref="Resource.CustomActivityCategories"/> property is of this type.
	/// </para>
	/// </remarks>
	/// <seealso cref="Resource.CustomActivityCategories"/>

	[InfragisticsFeature(FeatureName = "ActivityCategories", Version = "11.1")]

	public class ActivityCategoryCollection : ObservableCollectionExtended<ActivityCategory>
	{
		#region Member Vars

		private Index<ActivityCategory, string> _index; 

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="ActivityCategoryCollection"/> object.
		/// </summary>
		public ActivityCategoryCollection( )
			: base( false, true )
		{
			_index = CreateIndexHelper( this );
		} 

		#endregion // Constructor

		#region Methods

		#region Public Methods

		#region FindMatchingItem

		/// <summary>
		/// Finds the ActivityCategory in the collection with the specified category name.
		/// </summary>
		/// <param name="categoryName">Category name to search.</param>
		/// <returns>If a match is found returns the matching ActivityCategory object, otherwise null.</returns>
		public ActivityCategory FindMatchingItem( string categoryName )
		{
			return _index.GetAnyMatchingItem( categoryName );
		}

		#endregion // FindMatchingItem 

		#endregion // Public Methods

		#region Internal Methods

		#region Clone

		internal ActivityCategoryCollection Clone( )
		{
			ActivityCategoryCollection clone = new ActivityCategoryCollection( );

			foreach ( ActivityCategory ii in this )
				clone.Add( ii.Clone( ) );

			return clone;
		}

		#endregion // Clone

		#region CreateIndexHelper

		internal static Index<ActivityCategory, string> CreateIndexHelper( IList<ActivityCategory> source )
		{
			return new Index<ActivityCategory, string>( source, ii => ii.CategoryName, StringComparer.OrdinalIgnoreCase );
		} 

		#endregion // CreateIndexHelper

		#endregion // Internal Methods 

		#endregion // Methods
	}

	#endregion // ActivityCategoryCollection Class
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