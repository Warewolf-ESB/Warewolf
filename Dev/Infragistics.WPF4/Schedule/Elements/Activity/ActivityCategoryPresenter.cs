using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Infragistics.AutomationPeers;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Custom element that represents a <see cref="ActivityCategory"/>
	/// </summary>

	[InfragisticsFeature(FeatureName = "ActivityCategories", Version = "11.1")]

	public class ActivityCategoryPresenter : Control
		, IPropertyChangeListener
	{
		#region Member Variables

		internal static Color DarkForegroundColor	= Color.FromArgb(0xff, 0x6D, 0x6D, 0x6D);
		internal static Color NullColorBorder		= Color.FromArgb(0xff, 0x6D, 0x6D, 0x6D);
		internal static Color NullColorBackground	= Colors.White;

		#endregion // Member Variables

		#region Constructor
		static ActivityCategoryPresenter()
		{

			ActivityCategoryPresenter.DefaultStyleKeyProperty.OverrideMetadata(typeof(ActivityCategoryPresenter), new FrameworkPropertyMetadata(typeof(ActivityCategoryPresenter)));

		}

		/// <summary>
		/// Initializes a new <see cref="ActivityCategoryPresenter"/>
		/// </summary>
		public ActivityCategoryPresenter()
		{



		}
		#endregion //Constructor

        #region Base class overrides

        #region OnCreateAutomationPeer
        /// <summary>
        /// Returns an automation peer that exposes the <see cref="ActivityCategoryPresenter"/> to UI Automation.
        /// </summary>
        /// <returns>A <see cref="ActivityCategoryPresenterAutomationPeer"/></returns>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new ActivityCategoryPresenterAutomationPeer(this);
        }
        #endregion // OnCreateAutomationPeer

        #endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region Category

		/// <summary>
		/// Identifies the <see cref="Category"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CategoryProperty = DependencyPropertyUtilities.Register("Category",
			typeof(ActivityCategory), typeof(ActivityCategoryPresenter),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnCategoryChanged))
			);

		private static void OnCategoryChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ActivityCategoryPresenter instance = (ActivityCategoryPresenter)d;

			var oldValue = e.OldValue as ActivityCategory;
			var newValue = e.NewValue as ActivityCategory;

			if ( oldValue != null )
				ScheduleUtilities.RemoveListener(oldValue, instance);

			if ( newValue != null )
				ScheduleUtilities.AddListener(newValue, instance, false);

			instance.InitializeBrushProperties();
		}

		/// <summary>
		/// Returns or sets the category that the element represents
		/// </summary>
		/// <seealso cref="CategoryProperty"/>
		public ActivityCategory Category
		{
			get
			{
				return (ActivityCategory)this.GetValue(ActivityCategoryPresenter.CategoryProperty);
			}
			set
			{
				this.SetValue(ActivityCategoryPresenter.CategoryProperty, value);
			}
		}

		#endregion //Category

		#region ComputedBackground

		private static readonly DependencyPropertyKey ComputedBackgroundPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedBackground",
			typeof(Brush), typeof(ActivityCategoryPresenter), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedBackground"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedBackgroundProperty = ComputedBackgroundPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the preferred background for the element based on the <see cref="ActivityCategory.Color"/>
		/// </summary>
		/// <seealso cref="ComputedBackgroundProperty"/>
		public Brush ComputedBackground
		{
			get
			{
				return (Brush)this.GetValue(ActivityCategoryPresenter.ComputedBackgroundProperty);
			}
			private set
			{
				this.SetValue(ActivityCategoryPresenter.ComputedBackgroundPropertyKey, value);
			}
		}

		#endregion //ComputedBackground

		#region ComputedBorderBrush

		private static readonly DependencyPropertyKey ComputedBorderBrushPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedBorderBrush",
			typeof(Brush), typeof(ActivityCategoryPresenter), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedBorderBrush"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedBorderBrushProperty = ComputedBorderBrushPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the preferred border for the element based on the <see cref="ActivityCategory.Color"/>
		/// </summary>
		/// <seealso cref="ComputedBorderBrushProperty"/>
		public Brush ComputedBorderBrush
		{
			get
			{
				return (Brush)this.GetValue(ActivityCategoryPresenter.ComputedBorderBrushProperty);
			}
			private set
			{
				this.SetValue(ActivityCategoryPresenter.ComputedBorderBrushPropertyKey, value);
			}
		}

		#endregion //ComputedBorderBrush

		#region ComputedForeground

		private static readonly DependencyPropertyKey ComputedForegroundPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedForeground",
			typeof(Brush), typeof(ActivityCategoryPresenter), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedForeground"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedForegroundProperty = ComputedForegroundPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the preferred foreground for the element based on the <see cref="ActivityCategory.Color"/>
		/// </summary>
		/// <seealso cref="ComputedForegroundProperty"/>
		public Brush ComputedForeground
		{
			get
			{
				return (Brush)this.GetValue(ActivityCategoryPresenter.ComputedForegroundProperty);
			}
			private set
			{
				this.SetValue(ActivityCategoryPresenter.ComputedForegroundPropertyKey, value);
			}
		}

		#endregion //ComputedForeground

		#region NameVisibility

		/// <summary>
		/// Identifies the <see cref="NameVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty NameVisibilityProperty = DependencyPropertyUtilities.Register("NameVisibility",
			typeof(Visibility), typeof(ActivityCategoryPresenter),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.VisibilityVisibleBox)
			);

		/// <summary>
		/// Returns or sets a value indicating whether the <see cref="ActivityCategory.CategoryName"/> should be displayed.
		/// </summary>
		/// <seealso cref="NameVisibilityProperty"/>
		public Visibility NameVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ActivityCategoryPresenter.NameVisibilityProperty);
			}
			set
			{
				this.SetValue(ActivityCategoryPresenter.NameVisibilityProperty, value);
			}
		}

		#endregion //NameVisibility

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Private Methods

		#region InitializeBrushProperties
		private void InitializeBrushProperties()
		{
			ActivityCategory category = this.Category;

			Brush background, border, foreground;

			if ( category == null || category.Color == null )
			{
				background = ScheduleUtilities.GetBrush(NullColorBackground);
				border = ScheduleUtilities.GetBrush(NullColorBorder);
				foreground = ScheduleUtilities.GetBrush(DarkForegroundColor);
			}
			else
			{
				Color color = category.Color.Value;

				background = ScheduleUtilities.GetBrush(color);
				border = ScheduleUtilities.GetActivityCategoryBrush(color, ActivityCategoryBrushId.Border);
				foreground = ScheduleUtilities.GetBrush(ScheduleUtilities.CalculateForeground(color, Colors.White, DarkForegroundColor));
			}

			this.ComputedBackground = background;
			this.ComputedBorderBrush = border;
			this.ComputedForeground = foreground;
		}
		#endregion // InitializeBrushProperties

		#endregion // Private Methods

		#endregion // Methods

		#region ITypedPropertyChangeListener<object,string> Members

		void ITypedPropertyChangeListener<object, string>.OnPropertyValueChanged( object dataItem, string property, object extraInfo )
		{
			if ( dataItem is ActivityCategory )
			{
				if ( string.IsNullOrEmpty(property) || property == "Color" )
					this.InitializeBrushProperties();
			}
		}

		#endregion //ITypedPropertyChangeListener<object,string> Members
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