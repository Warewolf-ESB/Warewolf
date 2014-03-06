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

	#region FieldChooserFilter Class

	/// <summary>
	/// Used for specifying a group of fields.
	/// </summary>
	/// <remarks>
	/// <see cref="FieldChooserFilter.Filter"/> property is used to create the group by filtering
	/// fields into a subset of fields. The fields for which the Filter predicate evaluates 
	/// to true will be included in the field group.
	/// </remarks>
	/// <seealso cref="FieldChooser.FieldFilters"/>
	[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
	public class FieldChooserFilter : PropertyChangeNotifier
	{
		#region Member Vars

		private string _description;
		private Predicate<Field> _filter;
		private object _toolTip;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="FieldChooser"/>.
		/// </summary>
		public FieldChooserFilter( )
		{
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="FieldChooser"/>.
		/// </summary>
		public FieldChooserFilter( string description, Predicate<Field> filter, string toolTip )
		{
			_description = description;
			_filter = filter;
			_toolTip = toolTip;
		}

		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region Description

		/// <summary>
		/// Description of the field group that's displayed in the field groups selector.
		/// </summary>
		//[Description( "Specifies the description of category or subgroup of fields this filter will form." )]
		//[Category( "Data" )]
		[Bindable( true )]
		public string Description
		{
			get
			{
				return _description;
			}
			set
			{
				if ( _description != value )
				{
					_description = value;
					this.RaisePropertyChangedEvent( "Description" );
				}
			}
		}

		#endregion // Description

		#region Filter

		/// <summary>
		/// Specifies the predicate that filters the fields to form a subgroup of fields.
		/// </summary>
		//[Description( "Used to filter fields to form a subgroup or subcategory of fields." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public Predicate<Field> Filter
		{
			get
			{
				return _filter;
			}
			set
			{
				if ( _filter != value )
				{
					_filter = value;
					this.RaisePropertyChangedEvent( "Filter" );
				}
			}
		}

		#endregion // Filter

		#region ToolTip

		/// <summary>
		/// Gets or sets the tool-tip that's displayed for the field group.
		/// </summary>
		//[Description( "Tool-tip that's displayed for the filter." )]
		//[Category( "Data" )]
		[Bindable( true )]
		public object ToolTip
		{
			get
			{
				return _toolTip;
			}
			set
			{
				if ( _toolTip != value )
				{
					_toolTip = value;
					this.RaisePropertyChangedEvent( "ToolTip" );
				}
			}
		}

		#endregion // ToolTip

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Private/Internal Methods

		#region GetMatchingFields

		internal IEnumerable<Field> GetMatchingFields( FieldLayout fl )
		{
			return this.GetMatchingFields( FieldChooserGroup.DefaultFieldFilter.Filter( fl.Fields ) );
		}

		internal IEnumerable<Field> GetMatchingFields( IEnumerable<Field> fields ) 
		{
			return null != _filter
				? GridUtilities.Filter<Field>( fields, _filter )
				: fields;
		}

		#endregion // GetMatchingFields

		#endregion // Private/Internal Methods

		#endregion // Methods
	}

	#endregion // FieldChooserFilter Class

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