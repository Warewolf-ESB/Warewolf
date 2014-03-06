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
	#region ResolvedActivityCategoryCollection Class

	internal class ResolvedActivityCategoryCollection : ObservableCollectionExtended<ActivityCategory>, IPropertyChangeListener
	{
		private bool _includeAllSupportedCategories;
		private IEnumerable<ActivityCategory> _defaultCategories;
		private Index<ActivityCategory, string> _defaultCategoriesIndex;
		private ActivityBase _activity;

		public ResolvedActivityCategoryCollection(
			IEnumerable<ActivityCategory> defaultCategories, ActivityBase activity, bool includeAllSupportedCategories )
			: this( defaultCategories, null, activity, includeAllSupportedCategories )
		{
		}

		internal ResolvedActivityCategoryCollection( IEnumerable<ActivityCategory> defaultCategories,
			Index<ActivityCategory, string> defaultCategoriesIndex, ActivityBase activity, bool includeAllSupportedCategories )
			: base( false, true )
		{
			_defaultCategories = defaultCategories;
			_activity = activity;
			_includeAllSupportedCategories = includeAllSupportedCategories;
			_defaultCategoriesIndex = defaultCategoriesIndex;

			ScheduleUtilities.AddListenerHelper( defaultCategories, this, true );
			ScheduleUtilities.AddListenerHelper( activity, this, true );
			ScheduleUtilities.AddListenerHelper( activity.OwningResource, this, true );

			this.InitializeItems( );
		}

		private ActivityCategory FindDefaultCategory( string categoryId )
		{
			if ( null != _defaultCategoriesIndex )
				return _defaultCategoriesIndex.GetAnyMatchingItem( categoryId );

			return null != _defaultCategories ? ( from ii in _defaultCategories where ii.CategoryName == categoryId select ii ).FirstOrDefault( ) : null;
		}

		internal IEnumerable<ActivityCategory> GetCategories( ActivityBase activity )
		{
			string categories = activity.Categories;

			string[] arr = !string.IsNullOrEmpty( categories ) ? categories.Split( ',' ) : null;
			if ( null != arr && arr.Length > 0 )
			{
				List<ActivityCategory> list = null;

				Resource resource = activity.OwningResource;
				Debug.Assert( null != resource, "Activity without owning resource." );
				var customCategories = null != resource ? resource.CustomActivityCategories : null;

				for ( int i = 0; i < arr.Length; i++ )
				{
					string categoryId = arr[i];
					categoryId = null != categoryId ? categoryId.Trim( ) : null;
					if ( !string.IsNullOrEmpty( categoryId ) )
					{
						ActivityCategory category = null != customCategories ? customCategories.FindMatchingItem( categoryId ) : null;
						if ( null == category )
							category = this.FindDefaultCategory( categoryId );

						if ( null != category )
						{
							if ( null == list )
								list = new List<ActivityCategory>( );

							list.Add( category );
						}
					}
				}

				return list;
			}

			return null;
		}

		private void InitializeItems( )
		{
			IEnumerable<ActivityCategory> items;

			if ( _includeAllSupportedCategories )
			{
				List<ActivityCategory> list = new List<ActivityCategory>( );

				var owner = _activity.OwningResource;
				Debug.Assert( null != owner );
				var customCategories = null != owner ? owner.CustomActivityCategories : null;
				if ( null != customCategories )
					list.AddRange( customCategories );

				// Add default categories. Filter out ones that are part of custom categories.
				// 
				if (_defaultCategories != null)
					list.AddRange(from ii in _defaultCategories where null == customCategories || null == customCategories.FindMatchingItem(ii.CategoryName) select ii);

				items = list;
			}
			else
			{
				items = this.GetCategories( _activity );
			}

			ScheduleUtilities.ReplaceCollectionContents( this, items );
		}

		public void OnPropertyValueChanged( object sender, string property, object extraInfo )
		{
			bool reeval = false;

			if ( _activity == sender && "Categories" == property )
			{
				if ( !_includeAllSupportedCategories )
					reeval = true;
			}
			else if ( sender == _defaultCategories
				|| sender is Resource && "CustomActivityCategories" == property
				|| sender is IEnumerable<ActivityCategory> )
			{
				reeval = true;
			}

			if ( reeval )
			{
				this.InitializeItems( );
			}
		}
	} 

	#endregion // ResolvedActivityCategoryCollection Class
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