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

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
	/// A control that contains one or more <see cref="LabelPresenter"/> instances (i.e. column headers) in the <see cref="DataPresenterBase"/> derived controls that display a separate header area such as in <see cref="XamDataGrid"/>.  It is used primarily for styling the area around the LabelPresenters.
	/// </summary>
	//[Description("A control that contains one or more 'LabelPresenter' instances (i.e. column headers) in the 'DataPresenterBase' derived controls that display a separate header area such as in 'XamDataGrid'.  It is used primarily for styling the area around the LabelPresenters.")]
    // AS 1/15/09 NA 2009 Vol 1 - Fixed Fields
    //public class HeaderLabelArea : ContentControl
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class HeaderLabelArea : RecordCellAreaBase
	{
		#region Member Variables

		private int _cachedVersion;
		private bool _versionInitialized;
		private StyleSelectorHelper _styleSelectorHelper;

		#endregion Member Variables

		#region Constructors

		static HeaderLabelArea()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(HeaderLabelArea), new FrameworkPropertyMetadata(typeof(HeaderLabelArea)));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HeaderLabelArea"/> class
		/// </summary>
		public HeaderLabelArea()
		{
			// initialize the styleSelectorHelper
			this._styleSelectorHelper = new StyleSelectorHelper(this);
		}

		#endregion Constructors

		#region Base class overrides

			#region OnPropertyChanged

		/// <summary>
		/// Called when a property is changed.
		/// </summary>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			if (e.Property == InternalVersionProperty)
			{
				this.InitializeVersionInfo();
			}
		}

			#endregion //OnPropertyChanged

			#region ToString

		/// <summary>
		/// Returns a string representation of the object
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("HeaderLabelArea: ");

			if (this.FieldLayout != null)
				sb.Append(this.FieldLayout.ToString());

			return sb.ToString();
		}

			#endregion //ToString

		#endregion //Base class overrides

		#region Properties

			#region Public Properties

				#region FieldLayout

		/// <summary>
		/// Identifies the 'FieldLayout' dependency property
		/// </summary>
		public static readonly DependencyProperty FieldLayoutProperty = DependencyProperty.Register("FieldLayout",
				  typeof(FieldLayout), typeof(HeaderLabelArea), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFieldLayoutChanged)));

		private static void OnFieldLayoutChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			HeaderLabelArea hla = target as HeaderLabelArea;

			if (hla != null)
			{
				hla._cachedFieldLayout = e.NewValue as FieldLayout;
				hla.InitializeVersionInfo();
			}
		}

		private FieldLayout _cachedFieldLayout = null;

		/// <summary>
		/// Returns the associated field layout
		/// </summary>
		//[Description("Returns the associated field layout")]
		//[Category("Behavior")]
		public FieldLayout FieldLayout
		{
			get
			{
				return this._cachedFieldLayout;
			}
			set
			{
				this.SetValue(HeaderLabelArea.FieldLayoutProperty, value);
			}
		}

				#endregion //FieldLayout

				#region Orientation

		private static readonly DependencyPropertyKey OrientationPropertyKey =
			DependencyProperty.RegisterReadOnly("Orientation",
			typeof(Orientation), typeof(HeaderLabelArea), new FrameworkPropertyMetadata(Orientation.Vertical));

		/// <summary>
		/// Identifies the <see cref="Orientation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OrientationProperty =
			OrientationPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the orientation (vertical/horizontal) of the HeaderLabelAreas in the containing Panel.
		/// </summary>
		/// <seealso cref="OrientationProperty"/>
		//[Description("Returns the orientation (vertical/horizontal) of the HeaderLabelAreas in the containing Panel.")]
		//[Category("Appearance")]
		public Orientation Orientation
		{
			get { return (Orientation)this.GetValue(HeaderLabelArea.OrientationProperty); }
		}

				#endregion //Orientation

			#endregion //Public Properties

			#region Internal Properties

				#region InternalVersion

		internal static readonly DependencyProperty InternalVersionProperty = DependencyProperty.Register("InternalVersion",
			typeof(int), typeof(HeaderLabelArea), new FrameworkPropertyMetadata(0));

		internal int InternalVersion
		{
			get
			{
				return (int)this.GetValue(HeaderLabelArea.InternalVersionProperty);
			}
			set
			{
				this.SetValue(HeaderLabelArea.InternalVersionProperty, value);
			}
		}

				#endregion //InternalVersion

			#endregion //Internal Properties

		#endregion //Properties

		#region Methods

			#region Private Methods

				#region InitializeVersionInfo

		private void InitializeVersionInfo()
		{
			if (this._cachedFieldLayout != null &&
				this._cachedFieldLayout.DataPresenter != null)
			{
				if (this._cachedFieldLayout.StyleGenerator != null)
				{
					int version = this.InternalVersion;

					if (this._cachedVersion != version)
					{
						this._cachedVersion = version;

						if (this._versionInitialized == true)
							this._styleSelectorHelper.InvalidateStyle();

						this.SetValue(HeaderLabelArea.OrientationPropertyKey, KnownBoxes.FromValue(this.FieldLayout.StyleGenerator.LogicalOrientation));
					}

					this._versionInitialized = true;
				}
			}
		}

				#endregion //InitializeVersionInfo

			#endregion //Private Methods

		#endregion //Methods

		#region StyleSelectorHelper private class

		private class StyleSelectorHelper : StyleSelectorHelperBase
		{
			private HeaderLabelArea _hla;

			internal StyleSelectorHelper(HeaderLabelArea hla) : base (hla)
			{
				this._hla = hla;
			}

			/// <summary>
			/// The style to be used as the source of a binding (read-only)
			/// </summary>
			public override Style Style
			{
				get
				{
					if (this._hla == null)
						return null;

					FieldLayout fl = this._hla.FieldLayout;

					if (fl != null)
					{
						DataPresenterBase dp = fl.DataPresenter;

						if (dp != null)
							return dp.InternalHeaderLabelAreaStyleSelector.SelectStyle(this._hla.FieldLayout, this._hla);
					}

					return null;
				}
			}
		}

		#endregion //StyleSelectorHelper private class
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