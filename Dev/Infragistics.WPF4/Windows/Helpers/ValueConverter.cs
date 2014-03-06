using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Markup;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace Infragistics.Windows.Helpers
{
    #region ValueConversionMode enumeration

    /// <summary>
    /// Determines how target values are calculated betwen stops of a <see cref="ValueConverterBase"/> or <see cref="ValueRangeConverterBase"/> derived class.  
    /// </summary>
    public enum ValueConversionMode
    {
        /// <summary>
        /// Calculate an interpolated target value based on the source value relative to the values specified on the adjacent stops.
        /// </summary>
        InterpolateBetweenStops = 0,

        /// <summary>
        /// Use the value specified on the nearest stop.
        /// </summary>
        NearestStop = 1
    }

    #endregion //ValueConversionMode enumeration	
    
	#region ValueConverterStopBase abstract class

	/// <summary>
	/// Abstract class for value converter stops
	/// </summary>
	abstract public class ValueConverterStopBase : DependencyObjectNotifier, IComparable
	{
		#region Value

		/// <summary>
		/// Identifies the 'Value' dependency property
		/// </summary>
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value",
				typeof(ValueBase), typeof(ValueConverterStopBase), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnValueChanged)));

		private static void OnValueChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			ValueConverterStopBase control = target as ValueConverterStopBase;

			if (control != null)
			{
				control._cachedValue = (ValueBase)(e.NewValue);
				control.RaisePropertyChangedEvent("Value");
			}
		}

		private ValueBase _cachedValue = null;

		/// <summary>
		/// The source value
		/// </summary>
		//[Description("The source value")]
		//[Category("Behavior")]
		public ValueBase Value
		{
			get
			{
				return this._cachedValue;
			}
			set
			{
				this.SetValue(ValueConverterStopBase.ValueProperty, value);
			}
		}

		#endregion //Value

		#region IComparable Members

		/// <summary>
		/// Compares 2 values
		/// </summary>
		/// <param name="obj">value to compare</param>
		/// <returns>Returns 0 if values are equal, -1 if the passed in value is less than this value or 1 if its greater.</returns>
		int IComparable.CompareTo(object obj)
		{
			ValueConverterStopBase stop = obj as ValueConverterStopBase;

			Debug.Assert(stop != null);

			// can only compare stops to each other
			if (stop == null)
				return this.Value.CompareTo(obj);

			// compare the values 
			return this.Value.CompareTo(stop.Value);

		}

		#endregion
	}

	#endregion //ValueConverterStopBase abstract class	
    
	#region ValueStopCollection<T> abstract base class

	/// <summary>
	/// Abstract base class used by <see cref="ValueConverterBase"/> derived classes to hold their stops.
	/// </summary>
	public abstract class ValueStopCollection<T> : ObservableCollection<T> where T : ValueConverterStopBase
	{
		#region Private Members

		private bool _sortRequired;

		#endregion //Private Members

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		public ValueStopCollection()
		{
			this.Initialize();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public ValueStopCollection(List<T> list)
			: base(list)
		{
			this.Initialize();
		}

		#endregion //Constructors

		#region Base class overrides

		#region ClearItems

		/// <summary>
		/// Called to clear the collection of all items
		/// </summary>
		protected override void ClearItems()
		{
			foreach (T stop in this)
			{
				// stop listening for property change notifications on the stop
				stop.PropertyChanged -= new PropertyChangedEventHandler(Item_PropertyChanged);
			}

			base.Clear();
		}

		#endregion //ClearItems

		#region InsertItem

		/// <summary>
		/// Called to insert an item in the collection
		/// </summary>
		/// <param name="index">The index where the item should be inserted.</param>
		/// <param name="item">The item to indert.</param>
		protected override void InsertItem(int index, T item)
		{
			if (this.Count > 0)
			{
				// ensure that all the stops are of the same type
				if (item.GetType() != this[0].GetType())
					throw new NotSupportedException(SR.GetString("LE_NotSupportedException_4", this.GetType().Name));
			}

			base.InsertItem(index, item);

			// listen for a property change on the stop
			item.PropertyChanged += new PropertyChangedEventHandler(Item_PropertyChanged);
		}

		#endregion //InsertItem

		#region RemoveItem

		/// <summary>
		/// Called to remove an item from the collection
		/// </summary>
		/// <param name="index">The index of the item to remove.</param>
		protected override void RemoveItem(int index)
		{
			// stop listening for property change notifications on the old stop
			this[index].PropertyChanged -= new PropertyChangedEventHandler(Item_PropertyChanged);

			base.RemoveItem(index);
		}

		#endregion //RemoveItem

		#region SetItem

		/// <summary>
		/// Called to replace an item in the collection at a specific index.
		/// </summary>
		/// <param name="index">The index of the item to replace.</param>
		/// <param name="item">The new item</param>
		protected override void SetItem(int index, T item)
		{
			// stop listening for property change notifications on the old stop
			T oldValue = this[index];
			if (oldValue != null)
				oldValue.PropertyChanged -= new PropertyChangedEventHandler(Item_PropertyChanged);

			base.SetItem(index, item);

			// start listening for property change notifications on the new stop
			item.PropertyChanged -= new PropertyChangedEventHandler(Item_PropertyChanged);
		}

		#endregion //SetItem

		#endregion //Base class overrides

		#region Methods

		#region Private Methods

		#region Initialize

		private void Initialize()
		{
			// liset for any changes so we can know if a sort is required
			this.PropertyChanged += new PropertyChangedEventHandler(ValueStopCollection_PropertyChanged);
			this.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ValueStopCollection_CollectionChanged);
		}

		#endregion //Initialize

		#region Item_PropertyChanged

		private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this.OnCollectionChanged( new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset, null, -1));
		}

		#endregion //Item_PropertyChanged

		#region ValueStopCollection_CollectionChanged

		private void ValueStopCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			this._sortRequired = true;
		}

		#endregion //ValueStopCollection_CollectionChanged

		#region ValueStopCollection_PropertyChanged

		private void ValueStopCollection_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this._sortRequired = true;
		}

		#endregion //ValueStopCollection_PropertyChanged

		#endregion //Private Methods

			#region Internal methods

				#region Sort

		/// <summary>
		/// Called to sort the items in the collection
		/// </summary>
		internal void Sort()
		{
			if (this._sortRequired == false || this.Count < 2)
				return;

			T[] t = new T[this.Count];


			Utilities.SortMerge(t, System.Collections.Comparer.Default);
			this._sortRequired = false;
		}

				#endregion //Sort

			#endregion //Internal Methods

		#endregion //Methods
	}

	#endregion //ValueStopCollection<T> abstract base class	

	#region ValueConverterBase abstract base class

	/// <summary>
	/// Abstract base class for value converters
	/// </summary>
	abstract public class ValueConverterBase : DependencyObjectNotifier, IValueConverter
	{
		#region Base class overrides

			#region OnPropertyChanged

		/// <summary>
		/// Called when a property has been changed
		/// </summary>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

			if (e.Property == ConversionModeProperty)
				this.RaisePropertyChangedEvent(e.Property.Name);
        }

			#endregion //OnPropertyChanged

		#endregion //Base class overrides

		#region ConversionMode

		/// <summary>
		/// Identifies the 'ConversionMode' dependency property
		/// </summary>
		public static readonly DependencyProperty ConversionModeProperty = DependencyProperty.Register("ConversionMode",
			typeof(ValueConversionMode), typeof(ValueConverterBase), new FrameworkPropertyMetadata(ValueConversionMode.InterpolateBetweenStops));

		/// <summary>
		/// Gets/sets how values are calculated
		/// </summary>
		//[Description("Gets/sets how values are calculated")]
		//[Category("Behavior")]
		public ValueConversionMode ConversionMode
		{
			get
			{
				return (ValueConversionMode)this.GetValue(ValueConverterBase.ConversionModeProperty);
			}
			set
			{
				this.SetValue(ValueConverterBase.ConversionModeProperty, value);
			}
		}

		#endregion //ConversionMode

		#region IValueConverter Members

		/// <summary>
		/// Converts a value to a target type
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="targetType">The type to convert to.</param>
		/// <param name="parameter">A conversion parameter</param>
		/// <param name="culture">The culture to use when doing the conversion.</param>
		/// <returns></returns>
		public abstract object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture);

		/// <summary>
		/// Converts back - not supported
		/// </summary>
		public virtual object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException(SR.GetString("LE_NotSupportedException_5"));
		}

		#endregion
	}

	#endregion //ValueConverterBase abstract base class	
    
	#region Derived classes for Brush target 

	#region ValueToBrushStop

	/// <summary>
	/// Represents a stop in a <see cref="ValueToBrushConverter"/>'s <see cref="ValueToBrushConverter.Stops"/> collection.
	/// </summary>
	public class ValueToBrushStop : ValueConverterStopBase
	{
		#region Brush

		/// <summary>
		/// Identifies the 'Brush' dependency property
		/// </summary>
		public static readonly DependencyProperty BrushProperty = DependencyProperty.Register("Brush",
				typeof(Brush), typeof(ValueToBrushStop), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnBrushChanged)));

		private static void OnBrushChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			ValueToBrushStop control = target as ValueToBrushStop;

			if (control != null)
                control._cachedBrush = (Brush)(e.NewValue);
		}

		private Brush _cachedBrush = null;

		/// <summary>
		/// The corresponding brush value
		/// </summary>
		//[Description("The corresponding brush value")]
		//[Category("Behavior")]
		public Brush Brush
		{
			get
			{
				return this._cachedBrush;
			}
			set
			{
				this.SetValue(ValueToBrushStop.BrushProperty, value);
			}
		}

		#endregion //Brush
	}

	#endregion //ValueToBrushStop	
    
	#region ValueToBrushStopCollection

	/// <summary>
	/// A collection of <see cref="ValueToBrushStop"/>s exposed by the <see cref="ValueToBrushConverter"/> class.
	/// </summary>
	public class ValueToBrushStopCollection : ValueStopCollection<ValueToBrushStop>
	{
	}

	#endregion //ValueToBrushStopCollection	
    
	#endregion //Derived classes for Brush target 


	#region ValueToBrushConverter

	/// <summary>
	/// A converter that converts a value into a Brush
	/// </summary>
	public class ValueToBrushConverter : ValueConverterBase
	{
		#region Private Members

		private ValueToBrushStopCollection _stops;

		#endregion //Private Members

		#region Base class overrides

			#region Convert

		/// <summary>
		/// Converts a value to a target type
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="targetType">The type to convert to.</param>
		/// <param name="parameter">A conversion parameter</param>
		/// <param name="culture">The culture to use when doing the conversion.</param>
		/// <returns></returns>
		public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (typeof(Brush) != targetType)
				throw new NotSupportedException( SR.GetString( "LE_NotSupportedException_6", targetType.Name ) );

			// if we don't have any stops in the collection or if the value is null then return the default brush 
			if (value == null ||
				value is DBNull ||
				this.Stops.Count == 0)
				return this.DefaultBrush;

			// first make sure the stop collection is sorted
			this._stops.Sort();

			ValueToBrushStop previousStop = null;

			try
			{
				foreach (ValueToBrushStop stop in this._stops)
				{
					if (stop.Value.CompareTo(value) > 0)
					{
						if (previousStop != null)
						{
							switch (this.ConversionMode)
							{
								case ValueConversionMode.NearestStop:
									return previousStop.Brush;

								case ValueConversionMode.InterpolateBetweenStops:
								{
									SolidColorBrush fromBrush = previousStop.Brush as SolidColorBrush;
									SolidColorBrush toBrush = stop.Brush as SolidColorBrush;

									// since we can only interpolate if the brushes are solid
									// color brushes, if either one is not just return the 
									// previous stop brush
									if (fromBrush == null || toBrush == null)
										return previousStop.Brush;

									double percentOfRange = previousStop.Value.CalculatePercentInRange(value, stop.Value, culture);

									if (percentOfRange <= 0)
										return previousStop.Brush;

									if (percentOfRange >= 1)
										return stop.Brush;

									Color fromColor = fromBrush.Color;
									Color toColor = toBrush.Color;

									Byte aValue = this.InterpolateColorValue(fromColor.A, toColor.A, percentOfRange);
									Byte rValue = this.InterpolateColorValue(fromColor.R, toColor.R, percentOfRange);
									Byte gValue = this.InterpolateColorValue(fromColor.G, toColor.G, percentOfRange);
									Byte bValue = this.InterpolateColorValue(fromColor.B, toColor.B, percentOfRange);

									return new SolidColorBrush(Color.FromArgb(aValue, rValue, gValue, bValue));
								}
							}
						}

						return stop.Brush;
					}

					previousStop = stop;
				}
			}
			catch
			{
				return this.DefaultBrush;
			}


			if (previousStop != null)
				return previousStop.Brush;

			return this.DefaultBrush;
		}

			#endregion //Convert

			#region OnPropertyChanged

		/// <summary>
		/// Called when a property has been changed
		/// </summary>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

			if (e.Property == ConversionModeProperty ||
				e.Property == DefaultBrushProperty)
				this.RaisePropertyChangedEvent(e.Property.Name);
		}

			#endregion //OnPropertyChanged

		#endregion //Base class overrides

		#region Properties

			#region DefaultBrush

		/// <summary>
		/// Identifies the 'DefaultBrush' dependency property
		/// </summary>
		public static readonly DependencyProperty DefaultBrushProperty = DependencyProperty.Register("DefaultBrush",
			typeof(Brush), typeof(ValueToBrushConverter), new FrameworkPropertyMetadata(Brushes.Black));

		/// <summary>
		/// Gets/sets the brush to use for values that can't be converted.
		/// </summary>
		//[Description("Gets/sets the brush to use for values that can't be converted.")]
		//[Category("Behavior")]
		public Brush DefaultBrush
		{
			get
			{
				return (Brush)this.GetValue(ValueToBrushConverter.DefaultBrushProperty);
			}
			set
			{
				this.SetValue(ValueToBrushConverter.DefaultBrushProperty, value);
			}
		}

			#endregion //DefaultBrush

			#region Stops

		/// <summary>
		/// Gets/sets the collection of <see cref="ValueToBrushStop"/>s.
		/// </summary>
		public ValueToBrushStopCollection Stops
		{
			get
			{
				if (this._stops == null)
					this._stops = new ValueToBrushStopCollection();

				return this._stops;
			}
			set
			{
				if (value != this._stops)
				{
					this._stops = value;
					this.RaisePropertyChangedEvent("Stops");
				}
			}
		}

			#endregion //Stops

		#endregion //Properties

		#region Methods

			#region Private Methods

				#region InterpolateColorValue

		private Byte InterpolateColorValue(Byte from, Byte to, double percentOfRange)
		{
			// if the values are the same just return that value
			if (from == to)
				return from;

			if (to > from)
				return (Byte)(((to - from) * percentOfRange) + from);
			else
				return (Byte)(from - ((from - to) * percentOfRange));
		}

				#endregion //InterpolateColorValue

			#endregion //Private Methods

		#endregion //Methods
	}

	#endregion //ValueToBrushConverter
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