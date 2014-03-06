using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// A custom panel for creating bars to represent a collection of <see cref="ActivityCategory"/> instances.
	/// </summary>

	[InfragisticsFeature(FeatureName = "ActivityCategories", Version = "11.1")]

	public class ActivityCategoryBarPanel : Panel
		, IPropertyChangeListener
	{
		#region Member Variables

		private List<ActivityCategoryPresenter> _presenters;
		private Dictionary<ActivityCategory, ActivityCategoryPresenter> _bars; 

		#endregion // Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ActivityCategoryBarPanel"/>
		/// </summary>
		public ActivityCategoryBarPanel()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override System.Windows.Size ArrangeOverride( System.Windows.Size finalSize )
		{
			Rect rect = new Rect();

			if ( null != _presenters )
			{
				for(int i = 0, count = _presenters.Count; i < count; i++)
				{
					// add a pixel of inter-item spacing
					if ( i > 0 )
						rect.Width++;

					UIElement element = _presenters[i];
					Size desired = element.DesiredSize;

					Rect elementRect = new Rect(rect.Right, 0, desired.Width, finalSize.Height);
					element.Arrange(elementRect);
					rect.Union(elementRect);
				}
			}

			return new Size(rect.Width, rect.Height);
		}
		#endregion //ArrangeOverride

		#region HasLogicalOrientation

		/// <summary>
		/// Returns true to indicate that the panel supports arranging the children in a single orientation.
		/// </summary>
		protected override bool HasLogicalOrientation
		{
			get
			{
				return true;
			}
		}

		#endregion //HasLogicalOrientation

		#region LogicalOrientation

		/// <summary>
		/// Returns the default orientation in which the children are arranged.
		/// </summary>
		protected override Orientation LogicalOrientation
		{
			get
			{
				return Orientation.Horizontal;
			}
		}

		#endregion //LogicalOrientation

		#region MeasureOverride
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride( Size availableSize )
		{
			var categories = this.Categories;
			bool hasCategories = categories != null && ScheduleUtilities.HasItems(this.FilterCategories(categories));
			Dictionary<ActivityCategory, ActivityCategoryPresenter> newBars = null;
			Dictionary<ActivityCategory, ActivityCategoryPresenter> oldBars = _bars;
			List<ActivityCategoryPresenter> availablePresenters = null;

			if ( hasCategories )
			{
				newBars = new Dictionary<ActivityCategory,ActivityCategoryPresenter>();

				if (oldBars != null)
				{
					// see what we already had elements for and establish what we will have for reuse
					foreach ( ActivityCategory category in this.FilterCategories(categories) )
					{
						ActivityCategoryPresenter presenter;

						if (oldBars.TryGetValue(category, out presenter))
						{
							oldBars.Remove(category);
							newBars.Add(category, presenter);
						}
					}

					if ( oldBars.Count > 0 )
					{
						availablePresenters = new List<ActivityCategoryPresenter>();

						foreach ( var presenter in oldBars.Values )
							availablePresenters.Add(presenter);
					}
				}

				if ( _presenters == null )
					_presenters = new List<ActivityCategoryPresenter>();
				else
					_presenters.Clear();

				// build the list of presenters
				foreach ( ActivityCategory category in this.FilterCategories(categories) )
				{
					ActivityCategoryPresenter presenter;

					if (!newBars.TryGetValue(category, out presenter))
					{
						if ( null != availablePresenters && availablePresenters.Count > 0 )
						{
							int index = availablePresenters.Count - 1;
							presenter = availablePresenters[index];
							availablePresenters.RemoveAt(index);
						}
						else
						{
							presenter = new ActivityCategoryPresenter();
							presenter.SetValue(ActivityCategoryPresenter.NameVisibilityProperty, KnownBoxes.VisibilityCollapsedBox);
							this.Children.Add(presenter);
						}

						presenter.Category = category;

						newBars[category] = presenter;
					}

					_presenters.Add(presenter);
				}

				// remove any presenters no longer being used
				if ( null != availablePresenters )
				{
					foreach ( var presenter in availablePresenters )
						this.Children.Remove(presenter);
				}
			}
			else if ( _presenters != null )
			{
				// if we had presenters but we don't need them then 
				_presenters.Clear();
				this.Children.Clear();
			}

			// cache the new dictionary of bars
			_bars = newBars;

			Debug.Assert((_presenters == null && this.Children.Count == 0) || (_presenters != null && _presenters.Count == this.Children.Count), "Out of sync");

			Size desiredSize = new Size();

			if ( null != _presenters && _presenters.Count > 0 )
			{
				Size measureSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

				foreach ( ActivityCategoryPresenter presenter in _presenters )
				{
					presenter.Measure(measureSize);
					Size elementSize = presenter.DesiredSize;

					desiredSize.Width += elementSize.Width;
					desiredSize.Height = Math.Max(desiredSize.Height, elementSize.Height);
				}

				desiredSize.Width += _presenters.Count - 1;
			}

			return desiredSize;
		}
		#endregion //MeasureOverride

		#endregion // Base class overrides

		#region Properties

		#region Categories

		/// <summary>
		/// Identifies the <see cref="Categories"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CategoriesProperty = DependencyPropertyUtilities.Register("Categories",
			typeof(IEnumerable<ActivityCategory>), typeof(ActivityCategoryBarPanel),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnCategoriesChanged))
			);

		private static void OnCategoriesChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ActivityCategoryBarPanel instance = (ActivityCategoryBarPanel)d;

			var oldNotifier = e.OldValue as ISupportPropertyChangeNotifications;
			var newNotifier = e.NewValue as ISupportPropertyChangeNotifications;

			if ( oldNotifier != null )
				oldNotifier.RemoveListener(instance);

			if ( newNotifier != null )
				newNotifier.AddListener(instance, true);

			instance.InvalidateMeasure();
		}

		/// <summary>
		/// Returns or sets the collection of categories to be displayed.
		/// </summary>
		/// <seealso cref="CategoriesProperty"/>
		public IEnumerable<ActivityCategory> Categories
		{
			get
			{
				return (IEnumerable<ActivityCategory>)this.GetValue(ActivityCategoryBarPanel.CategoriesProperty);
			}
			set
			{
				this.SetValue(ActivityCategoryBarPanel.CategoriesProperty, value);
			}
		}

		#endregion //Categories

		// JJD 3/4/11 - added HideFirstCategory property
		#region HideFirstCategory

		/// <summary>
		/// Identifies the <see cref="HideFirstCategory"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HideFirstCategoryProperty = DependencyPropertyUtilities.Register("HideFirstCategory",
			typeof(bool), typeof(ActivityCategoryBarPanel),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnHideFirstCategoryChanged))
			);

		private static void OnHideFirstCategoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityCategoryBarPanel instance = (ActivityCategoryBarPanel)d;

			instance.InvalidateMeasure();
		}

		/// <summary>
		/// Returns or sets whether to hide the first category. Defaults to true.
		/// </summary>
		/// <seealso cref="HideFirstCategoryProperty"/>
		public bool HideFirstCategory
		{
			get
			{
				return (bool)this.GetValue(ActivityCategoryBarPanel.HideFirstCategoryProperty);
			}
			set
			{
				this.SetValue(ActivityCategoryBarPanel.HideFirstCategoryProperty, value);
			}
		}

		#endregion //HideFirstCategory

		#endregion // Properties

		#region Methods

		#region Private Methods

		#region FilterCategories
		private IEnumerable<ActivityCategory> FilterCategories( IEnumerable<ActivityCategory> categories )
		{
			if ( categories != null )
			{
				int includableCount = 0;

				foreach ( var category in categories )
				{
					// skip those with no color
					if ( category.Color == null )
						continue;

					includableCount++;

					// JJD 3/4/11 - added HideFirstCategory property
					if (includableCount > 1 || this.HideFirstCategory == false)
						yield return category;
				}
			}
		}
		#endregion // FilterCategories

		#endregion // Private Methods

		#endregion // Methods

		#region ITypedPropertyChangeListener<object,string> Members

		void ITypedPropertyChangeListener<object, string>.OnPropertyValueChanged( object dataItem, string property, object extraInfo )
		{
			if ( dataItem is ActivityCategory )
			{
				if ( string.IsNullOrEmpty(property) || property == "Color" )
					this.InvalidateMeasure();
			}
			else if ( dataItem is IEnumerable<ActivityCategory> )
			{
				this.InvalidateMeasure();
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