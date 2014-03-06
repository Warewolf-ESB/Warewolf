using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using System.Diagnostics;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Infragistics.Shared;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Internal;
using Infragistics.Windows.DataPresenter.Internal;
using Infragistics.Windows.Reporting;
using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.Editors;
using System.Windows.Markup;
using System.ComponentModel;
using System.Collections.ObjectModel;




namespace Infragistics.Windows.DataPresenter
{

	#region FieldChooserGroup Class

	/// <summary>
	/// A class that represents an entry in the field-group selector's drop-down list in the <see cref="FieldChooser"/>.
	/// </summary>
	/// <seealso cref="FieldChooser.FieldGroups"/>
	/// <seealso cref="FieldChooser"/>
	[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
	public class FieldChooserGroup : PropertyChangeNotifier
	{
		#region Nested Data Structures

		internal class DefaultFieldFilter : GridUtilities.IMeetsCriteria
		{
			public bool MeetsCriteria( object item )
			{
				Field field = item as Field;
				if ( null != field && ! field.IsExpandableResolved && AllowFieldHiding.Never != field.AllowHidingResolved )
					return true;

				return false;
			}

			public static IEnumerable<Field> Filter( IEnumerable<Field> fields )
			{
				return GridUtilities.Filter<Field>( fields, new DefaultFieldFilter( ) );
			}
		}

		#endregion // Nested Data Structures

		#region Member Vars

		private FieldLayout _fieldLayout;
		private FieldChooserFilter _fieldFilter;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fieldLayout">Field-layout whose fields are represented by this field group.</param>
		/// <param name="fieldFilter">Used to create a subgroup of fields.</param>
		public FieldChooserGroup( FieldLayout fieldLayout, FieldChooserFilter fieldFilter )
		{
			_fieldLayout = fieldLayout;
			_fieldFilter = fieldFilter;
		}

		#endregion // Constructor

		#region Base Overrides

		#region Equals

		/// <summary>
		/// Overridden. Checks to see if the specified object is equal to this object.
		/// </summary>
		/// <param name="obj">Object to test for equality against this object.</param>
		/// <returns>True if the specified object is equal to this object. False otherwise.</returns>
		public override bool Equals( object obj )
		{
			FieldChooserGroup group = obj as FieldChooserGroup;
			if ( null != group )
			{
				if ( group._fieldFilter == _fieldFilter
					&& group._fieldLayout == _fieldLayout )
					return true;
			}

			return false;
		}

		#endregion // Equals

		#region GetHashCode

		/// <summary>
		/// Overridden. Returns the hash code of this object.
		/// </summary>
		/// <returns>Hash code of this object.</returns>
		public override int GetHashCode( )
		{
			return ( null != _fieldLayout ? _fieldLayout.GetHashCode( ) : 0 )
				^ ( null != _fieldFilter ? _fieldFilter.GetHashCode( ) : 0 );
		}

		#endregion // GetHashCode

		#region ToString

		// SSP 7/27/09 TFS19771
		// Apparently the auto-mation requires this or unique automation id.
		// 
		/// <summary>
		/// Overridden. Returns a string representation of this object.
		/// </summary>
		/// <returns>String object</returns>
		public override string ToString( )
		{
			string flName = null != _fieldLayout
				? _fieldLayout.Description
				: string.Empty;

			string filterName = null != _fieldFilter
				? _fieldFilter.Description
				: string.Empty;

			if ( filterName.Length > 0 )
				return flName + " - " + filterName;
			else
				return flName;
		}

		#endregion // ToString

		#endregion // Base Overrides

		#region Properties

		#region Public Properties

		#region FieldFilter

		/// <summary>
		/// Returns the associated <see cref="FieldChooserFilter"/> if any which filters the 
		/// field-layout's fields into a subgroup of fields.
		/// </summary>
		//[Description( "Used to filter fields to form the subgroup of fields." )]
		//[Category( "Data" )]
		[Bindable( true )]		
		public FieldChooserFilter FieldFilter
		{
			get
			{
				return _fieldFilter;
			}
		}

		#endregion // FieldFilter

		#region FieldLayout

		/// <summary>
		/// Returns the associated field layout.
		/// </summary>
		//[Description( "Associated FieldLayout." )]
		//[Category( "Data" )]
		[Bindable( true )]		
		public FieldLayout FieldLayout
		{
			get
			{
				return _fieldLayout;
			}
		}

		#endregion // FieldLayout

		#region HasFieldFilter

		/// <summary>
		/// Indicates if <see cref="FieldFilter"/> property returns a non-null value.
		/// </summary>
		//[Description( "Indicates if FieldFilter property value is non-null." )]
		//[Category( "Data" )]
		[Bindable( true )]		
		public bool HasFieldFilter
		{
			get
			{
				return null != _fieldFilter;
			}
		}

		#endregion // HasFieldFilter

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Private/Internal Methods

		#region GetMatchingFields

		internal IEnumerable<Field> GetMatchingFields( )
		{
			IEnumerable<Field> fields = null;

			if ( null != _fieldLayout )
			{
				fields = DefaultFieldFilter.Filter( _fieldLayout.Fields );

				if ( null != _fieldFilter )
					fields = _fieldFilter.GetMatchingFields( fields );
			}

			return fields;
		}

		#endregion // GetMatchingFields

		#endregion // Private/Internal Methods

		#endregion // Methods
	}

	#endregion // FieldChooserGroup Class

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