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

	#region FieldChooserEntry Class

	/// <summary>
	/// Represents a field in a FieldChooser.
	/// </summary>
	/// <seealso cref="FieldChooser"/>
	[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
	public class FieldChooserEntry : PropertyChangeNotifier
	{
		#region Member Vars

		private Field _field;
		private bool _isVisible;
		private bool _isSelected;

		private FieldChooser _fieldChooser;
		private PropertyValueTracker _fieldVisibilityTracker;

		// SSP 1/12/10 TFS25122
		// 
		private PropertyValueTracker _fieldActualPositionTracker;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="field">Associated field</param>
		public FieldChooserEntry( Field field )
		{
			GridUtilities.ValidateNotNull( field );

			_field = field;
			_isVisible = IsFieldVisible( field );

			_fieldVisibilityTracker = new PropertyValueTracker( _field, "VisibilityResolved", this.OnFieldVisibilityChanged );

			// SSP 1/12/10 TFS25122
			// If the FieldDisplayOrder is SameAsDataPresenter then we need to re-sort the fields in the field chooser
			// whenever a field's position is changed, for example via drag-and-drop in the data presenter.
			// 
			_fieldActualPositionTracker = new PropertyValueTracker( _field, "ActualPosition", this.OnFieldActualPositionChanged );
		}

		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region Field

		/// <summary>
		/// Gets the associated field.
		/// </summary>
		public Field Field
		{
			get
			{
				return _field;
			}
		}

		#endregion // Field

		#region IsSelected

		/// <summary>
		/// Indicates if the associated field is selected in the FieldChooser.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// When a field label is clicked in the FieldChooser, it gets selected. It becomes the pivot
		/// item for keyboard navigation purposes. Only one field can be selected at a time.
		/// </para>
		/// </remarks>
		public bool IsSelected
		{
			get
			{
				return _isSelected;
			}
		}

		#endregion // IsSelected

		#region IsVisible

		/// <summary>
		/// Gets or sets a value indicating whether the field is visible in the data presenter.
		/// </summary>
		public bool IsVisible
		{
			get
			{
				return _isVisible;
			}
			set
			{
				if ( _isVisible != value )
				{
					FieldDragManager.ChangeFieldVisibilityViaFieldChooser( _field, value );

					_isVisible = IsFieldVisible( _field );

					this.RaisePropertyChangedEvent( "IsVisible" );
				}
			}
		}

		#endregion // IsVisible

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Private/Internal Methods

		#region InitializeFieldChooser

		internal void InitializeFieldChooser( FieldChooser fieldChooser )
		{
			_fieldChooser = fieldChooser;
		}

		#endregion // InitializeFieldChooser

		#region InternalSetIsSelected

		/// <summary>
		/// Sets the IsSelected property value.
		/// </summary>
		/// <param name="isSelected">New IsSelected property value.</param>
		internal void InternalSetIsSelected( bool isSelected )
		{
			if ( _isSelected != isSelected )
			{
				_isSelected = isSelected;

				this.RaisePropertyChangedEvent( "IsSelected" );
			}
		}

		#endregion // InternalSetIsSelected

		#region IsFieldVisible

		private static bool IsFieldVisible( Field field )
		{
			return Visibility.Visible == field.VisibilityResolved;
		}

		#endregion // IsFieldVisible

		#region OnFieldActualPositionChanged

		// SSP 1/12/10 TFS25122
		// If the FieldDisplayOrder is SameAsDataPresenter then we need to re-sort the fields in the field chooser
		// whenever a field's position is changed, for example via drag-and-drop in the data presenter.
		// 
		private void OnFieldActualPositionChanged( )
		{
			if ( null != _fieldChooser && FieldChooserDisplayOrder.SameAsDataPresenter == _fieldChooser.FieldDisplayOrder )
			{
				_fieldChooser.ReInitializeCurrentFieldsAsync( );
			}
		}

		#endregion // OnFieldActualPositionChanged

		#region OnFieldVisibilityChanged

		private void OnFieldVisibilityChanged( )
		{
			bool newIsVisible = IsFieldVisible( _field );
			if ( newIsVisible != _isVisible )
			{
				_isVisible = newIsVisible;
				this.RaisePropertyChangedEvent( "IsVisible" );
				
				if ( null != _fieldChooser )
					_fieldChooser.OnFieldVisibilityChanged( _field );
			}
		}

		#endregion // OnFieldVisibilityChanged

		#endregion // Private/Internal Methods

		#endregion // Methods

	}

	#endregion // FieldChooserEntry Class

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