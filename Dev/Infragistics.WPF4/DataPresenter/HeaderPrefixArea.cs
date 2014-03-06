using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
//using System.Windows.Events;
using System.Windows.Media.Animation;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Markup;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Data;
using System.Globalization;
using System.Text;
using Infragistics.Shared;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Selection;
using System.Windows.Threading;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.DataPresenter
{
	#region RecordPrefixArea Class

	/// <summary>
	/// A base class for <see cref="HeaderPrefixArea"/> and <see cref="SummaryRecordPrefixArea"/> controls.
	/// Used by records that do not display record selectors to occupy the area where record selecotrs would go.
	/// </summary>
	//[Description( "Used by records that do not display record selectors to occupy the area where record selecotrs would go." )]
	public abstract class RecordPrefixArea : ContentControl
	{
		#region Member Variables

		private int _cachedVersion;
		private bool _versionInitialized;
		private Record _record;
		private StyleSelectorHelperBase _styleSelectorHelper;

		#endregion Member Variables

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="RecordPrefixArea"/> class
		/// </summary>
		public RecordPrefixArea( )
		{
			_styleSelectorHelper = this.CreateStyleSelectorHelper( );
		}

		#endregion Constructors

		#region Base class overrides

		#region OnPropertyChanged

		/// <summary>
		/// Called when a property is changed.
		/// </summary>
		protected override void OnPropertyChanged( DependencyPropertyChangedEventArgs e )
		{
			base.OnPropertyChanged( e );

			if ( e.Property == DataContextProperty )
			{
				Record record = e.NewValue as Record;

				if ( record != this._record )
				{
					if ( this._record != null )
						this._record = null;
					else
						this._record = record;

					this.SetValue( RecordPropertyKey, this._record );
				}
			}
			else if ( e.Property == InternalVersionProperty )
			{
				this.InitializeVersionInfo( );
			}
		}

		#endregion //OnPropertyChanged

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region FieldLayout

		/// <summary>
		/// Identifies the 'FieldLayout' dependency property
		/// </summary>
		public static readonly DependencyProperty FieldLayoutProperty = DependencyProperty.Register( "FieldLayout",
				  typeof( FieldLayout ), typeof( RecordPrefixArea ), new FrameworkPropertyMetadata( new PropertyChangedCallback( OnFieldLayoutChanged ) ) );

		private static void OnFieldLayoutChanged( DependencyObject target, DependencyPropertyChangedEventArgs e )
		{
			RecordPrefixArea rs = target as RecordPrefixArea;

			if ( rs != null )
			{
				rs._cachedFieldLayout = e.NewValue as FieldLayout;
				rs.InitializeVersionInfo( );
            }
		}

		private FieldLayout _cachedFieldLayout = null;

		/// <summary>
		/// Returns the associated field layout
		/// </summary>
		//[Description( "Returns the associated field layout" )]
		//[Category( "Data" )]
		public FieldLayout FieldLayout
		{
			get
			{
				return this._cachedFieldLayout;
			}
			set
			{
				this.SetValue( FieldLayoutProperty, value );
			}
		}

		#endregion //FieldLayout

		#region Orientation

		private static readonly DependencyPropertyKey OrientationPropertyKey =
			DependencyProperty.RegisterReadOnly( "Orientation",
			typeof( Orientation ), typeof( RecordPrefixArea ), new FrameworkPropertyMetadata( Orientation.Vertical ) );

		/// <summary>
		/// Identifies the <see cref="Orientation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OrientationProperty =
			OrientationPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the orientation (vertical/horizontal) of the RecordPrefixAreas in the containing Panel.
		/// </summary>
		/// <seealso cref="OrientationProperty"/>
		//[Description( "Returns the orientation (vertical/horizontal) of the HeaderPrefixAreas in the containing Panel." )]
		//[Category( "Appearance" )]
		public Orientation Orientation
		{
			get { return (Orientation)this.GetValue( OrientationProperty ); }
		}

		#endregion //Orientation

		#region Record

		private static readonly DependencyPropertyKey RecordPropertyKey =
			DependencyProperty.RegisterReadOnly( "Record",
			typeof( Record ), typeof( RecordPrefixArea ), new FrameworkPropertyMetadata( ) );

		/// <summary>
		/// Identifies the 'Record' dependency property
		/// </summary>
		public static readonly DependencyProperty RecordProperty =
			RecordPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the associated record inside a DataPresenterBase (read-only)
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b>Since <see cref="Record"/> is an abstract base class for <see cref="DataRecord"/>, <see cref="GroupByRecord"/> and <see cref="ExpandableFieldRecord"/> you may have to cast this property to the appropiate derived class to access specific properties, e.g. the <see cref="DataRecord"/>'s <see cref="DataRecord.Cells"/> collection.</para>
		/// </remarks>
		/// <seealso cref="DataRecord"/>
		/// <seealso cref="GroupByRecord"/>
		/// <seealso cref="ExpandableFieldRecord"/>
		//[Description("Returns the associated record inside a DataPresenterBase (read-only)")]
		//[Category("Data")]
		public Record Record
		{
			get
			{
				return this._record;
			}
		}

		#endregion //Record

		#endregion //Public Properties

		#region Internal Properties

		#region InternalVersion

		internal static readonly DependencyProperty InternalVersionProperty = DependencyProperty.Register( "InternalVersion",
			typeof( int ), typeof( RecordPrefixArea ), new FrameworkPropertyMetadata( 0 ) );

		internal int InternalVersion
		{
			get
			{
				return (int)this.GetValue( InternalVersionProperty );
			}
			set
			{
				this.SetValue( InternalVersionProperty, value );
			}
		}

		#endregion //InternalVersion

		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

		#region Private/Internal Methods

		#region CreateStyleSelectorHelper

		
		
		/// <summary>
		/// Creates an instance of StyleSelectorHelperBase derived class for the element.
		/// </summary>
		/// <returns>The created style selector helper.</returns>
		internal abstract StyleSelectorHelperBase CreateStyleSelectorHelper( );

		#endregion // CreateStyleSelectorHelper

		#region InitializeVersionInfo

		private void InitializeVersionInfo( )
		{
			if ( this._cachedFieldLayout != null &&
				this._cachedFieldLayout.DataPresenter != null )
			{
				if ( this._cachedFieldLayout.StyleGenerator != null )
				{
					int version = this.InternalVersion;

					if ( this._cachedVersion != version )
					{
						this._cachedVersion = version;

						if ( this._versionInitialized == true )
							this._styleSelectorHelper.InvalidateStyle( );

						this.SetValue( OrientationPropertyKey, KnownBoxes.FromValue( this.FieldLayout.StyleGenerator.LogicalOrientation ) );
					}

					this._versionInitialized = true;
				}
			}
		}

		#endregion //InitializeVersionInfo

        #endregion // Private/Internal Methods

        #endregion //Methods
    }

	#endregion // RecordPrefixArea Class

	#region HeaderPrefixArea Class

	/// <summary>
	/// A control that is placed in the header area and sized to match the dimensions of the RecordSelectors so that the LabelPresenters in the HeaderLabelArea line up with the cells in each record.
	/// </summary>
	//[Description("A control that is placed in the header area and sized to match the dimensions of the RecordSelectors so that the LabelPresenters in the HeaderLabelArea line up with the cells in each record.")]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class HeaderPrefixArea : RecordPrefixArea
	{

		#region Constructors

		static HeaderPrefixArea()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(HeaderPrefixArea), new FrameworkPropertyMetadata(typeof(HeaderPrefixArea)));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HeaderPrefixArea"/> class
		/// </summary>
		public HeaderPrefixArea()
		{
		}

		#endregion Constructors

		#region Base class overrides

		#region CreateStyleSelectorHelper

		
		
		/// <summary>
		/// Creates an instance of StyleSelectorHelperBase derived class for the element.
		/// </summary>
		/// <returns>The created style selector helper.</returns>
		internal override StyleSelectorHelperBase CreateStyleSelectorHelper( )
		{
			return new StyleSelectorHelper( this );
		}

		#endregion // CreateStyleSelectorHelper

		#region ToString

		/// <summary>
		/// Returns a string representation of the object
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("HeaderPrefixArea: ");

			if (this.Record != null)
				sb.Append(this.Record.ToString());

			return sb.ToString();
		}

		#endregion //ToString

		#endregion //Base class overrides

		#region StyleSelectorHelper private class

		private class StyleSelectorHelper : StyleSelectorHelperBase
		{
			private HeaderPrefixArea _rs;

			internal StyleSelectorHelper(HeaderPrefixArea rs) : base(rs)
			{
				this._rs = rs;
			}

			/// <summary>
			/// The style to be used as the source of a binding (read-only)
			/// </summary>
			public override Style Style
			{
				get
				{
					if (this._rs == null)
						return null;

					FieldLayout fl = this._rs.FieldLayout;

					if (fl != null)
					{
						DataPresenterBase dp = fl.DataPresenter;

						if (dp != null)
							return dp.InternalHeaderPrefixAreaStyleSelector.SelectStyle(this._rs.DataContext, this._rs);
					}

					return null;
				}
			}
		}

		#endregion //StyleSelectorHelper private class
	}

	#endregion // HeaderPrefixArea Class
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