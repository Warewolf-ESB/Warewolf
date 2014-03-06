using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Markup;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace Infragistics.Windows.Helpers
{
	#region ValueRangeConverterStopBase abstract base class

	/// <summary>
	/// Abstract class for value range converter stops
	/// </summary>
	abstract public class ValueRangeConverterStopBase : DependencyObjectNotifier, IComparable
	{
        #region Base class overrides

            #region OnPropertyChanged

        /// <summary>
        /// Called when a property has been changed
        /// </summary>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == OffsetProperty)
                this.RaisePropertyChangedEvent(e.Property.Name);
        }

            #endregion //OnPropertyChanged

        #endregion //Base class overrides
        
        #region Offset

        /// <summary>
		/// Identifies the 'Offset' dependency property
		/// </summary>
		public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register("Offset",
				typeof(double), typeof(ValueRangeConverterStopBase), new FrameworkPropertyMetadata(0.0d), new ValidateValueCallback(ValidateOffsetValue));

		private static bool ValidateOffsetValue(object value)
		{
			if (!(value is double))
				return false;

			double dbl = (double)value;

			if (dbl < 0.0d ||
				 dbl > 1.0d)
				return false;

			return true;
		}

		/// <summary>
		/// The percentage offset from 0 to 1.
		/// </summary>
		//[Description("The percentage offset from 0 to 1.")]
		//[Category("Behavior")]
		public double Offset
		{
			get
			{
				return (double)this.GetValue(ValueRangeConverterStopBase.OffsetProperty);
			}
			set
			{
				this.SetValue(ValueRangeConverterStopBase.OffsetProperty, value);
			}
		}

		#endregion //Offset

		#region IComparable Members

		/// <summary>
		/// Compares 2 values
		/// </summary>
		/// <param name="obj">value to compare</param>
		/// <returns>Returns 0 if values are equal, -1 if the passed in value is less than this value or 1 if its greater.</returns>
		int IComparable.CompareTo(object obj)
		{
			ValueRangeConverterStopBase stop = obj as ValueRangeConverterStopBase;

			Debug.Assert(stop != null);

			// can only compare stops to each other
			if (stop == null)
			{
				if (obj is double)
					return this.Offset.CompareTo(obj);

				return 1;
			}

			// compare the values 
			return this.Offset.CompareTo(stop.Offset);

		}

		#endregion
	}

	#endregion //ValueRangeConverterStopBase abstract base class	
    
	#region ValueRangeStopCollection<T> abstract base class

	/// <summary>
	/// Abstract base class used by <see cref="ValueRangeConverterBase"/> derived classes to hold their stops.
	/// </summary>
	public abstract class ValueRangeStopCollection<T> : ObservableCollection<T> where T : ValueRangeConverterStopBase
	{
		#region Private Members

		private bool _sortRequired;

		#endregion //Private Members

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		public ValueRangeStopCollection()
		{
			this.Initialize();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public ValueRangeStopCollection(List<T> list)
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
					throw new NotSupportedException( SR.GetString( "LE_NotSupportedException_7", this.GetType( ).Name ) );
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
			this.PropertyChanged += new PropertyChangedEventHandler(ValueRangeStopCollection_PropertyChanged);
			this.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ValueRangeStopCollection_CollectionChanged);
		}

				#endregion //Initialize

				#region Item_PropertyChanged

		private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this.OnCollectionChanged( new System.Collections.Specialized.NotifyCollectionChangedEventArgs( System.Collections.Specialized.NotifyCollectionChangedAction.Reset, null, -1));
		}

				#endregion //Item_PropertyChanged

				#region ValueRangeStopCollection_CollectionChanged

		private void ValueRangeStopCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			this._sortRequired = true;
		}

				#endregion //ValueRangeStopCollection_CollectionChanged

				#region ValueRangeStopCollection_PropertyChanged

		private void ValueRangeStopCollection_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this._sortRequired = true;
		}

				#endregion //ValueRangeStopCollection_PropertyChanged

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

	#endregion //ValueRangeStopCollection<T> abstract base class	
    
	#region ValueRangeConverterBase abstract base class

	/// <summary>
	/// Abstract base class for value range converters
	/// </summary>
	abstract public class ValueRangeConverterBase : ValueConverterBase
	{
		#region Base class overrides

			#region OnPropertyChanged

		/// <summary>
		/// Called when a property has been changed
		/// </summary>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

			if (e.Property == StartValueProperty || e.Property == EndValueProperty)
			{
				this.VerifyStopTypes();
				this.RaisePropertyChangedEvent(e.Property.Name);
			}
        }

			#endregion //OnPropertyChanged

		#endregion //Base class overrides

		#region Properties
    
    		#region EndValue

		/// <summary>
		/// Identifies the 'EndValue' dependency property
		/// </summary>
		public static readonly DependencyProperty EndValueProperty = DependencyProperty.Register("EndValue",
				typeof(ValueBase), typeof(ValueRangeConverterBase), new FrameworkPropertyMetadata());

		/// <summary>
		/// Gets/sets the end of the range
		/// </summary>
		//[Description("Gets/sets the end of the range")]
		//[Category("Behavior")]
		public ValueBase EndValue
		{
			get
			{
                return (ValueBase)this.GetValue(ValueRangeConverterBase.EndValueProperty);
			}
			set
			{
				this.SetValue(ValueRangeConverterBase.EndValueProperty, value);
			}
		}

			#endregion //EndValue

			#region StartValue

		/// <summary>
		/// Identifies the 'StartValue' dependency property
		/// </summary>
		public static readonly DependencyProperty StartValueProperty = DependencyProperty.Register("StartValue",
				typeof(ValueBase), typeof(ValueRangeConverterBase), new FrameworkPropertyMetadata());

		/// <summary>
		/// Gets/sets the start of the range
		/// </summary>
		//[Description("Gets/sets the start of the range")]
		//[Category("Behavior")]
		public ValueBase StartValue
		{
			get
			{
				return (ValueBase)this.GetValue(ValueRangeConverterBase.StartValueProperty);
			}
			set
			{
				this.SetValue(ValueRangeConverterBase.StartValueProperty, value);
			}
		}

			#endregion //StartValue

   		#endregion //Properties	
    
		#region Methods

			#region Protected Methods
    
				#region VerifyStopTypes

		/// <summary>
		/// Throws a NotSupportedException if the srat end end value types are different 
		/// </summary>
		/// <exception cref="NotSupportedException">The StartValue and EndValue must be of the same type</exception>
        protected virtual void VerifyStopTypes()
		{
			ValueBase start = this.StartValue;
			ValueBase end = this.EndValue;

			if (start != null && end != null &&
				start.GetType() != end.GetType())
				throw new NotSupportedException( SR.GetString( "LE_NotSupportedException_8", this.GetType( ).Name ) );
		}

   				#endregion //VerifyStopTypes	
    
   			#endregion //Protected Methods	
        
	   	#endregion //Methods	
        
	}

	#endregion //ValueRangeConverterBase abstract base class	
        
	#region Derived classes for Brush target 

	#region ValueToBrushRangeStop

	/// <summary>
	/// Represents a stop in a <see cref="ValueToBrushRangeConverter"/>'s <see cref="ValueToBrushRangeConverter.Stops"/> collection.
	/// </summary>
	public class ValueToBrushRangeStop : ValueRangeConverterStopBase
	{
        #region Base class overrides

            #region OnPropertyChanged

        /// <summary>
        /// Called when a property has been changed
        /// </summary>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == ValueToBrushRangeStop.BrushProperty)
                this.RaisePropertyChangedEvent(e.Property.Name);
        }

            #endregion //OnPropertyChanged

        #endregion //Base class overrides
        
        #region Brush

		/// <summary>
		/// Identifies the 'Brush' dependency property
		/// </summary>
		public static readonly DependencyProperty BrushProperty = DependencyProperty.Register("Brush",
				typeof(Brush), typeof(ValueToBrushRangeStop), new FrameworkPropertyMetadata());

		/// <summary>
		/// The corresponding brush value
		/// </summary>
		//[Description("The corresponding brush value")]
		//[Category("Behavior")]
		public Brush Brush
		{
			get
			{
				return (Brush)this.GetValue(ValueToBrushRangeStop.BrushProperty);
			}
			set
			{
				this.SetValue(ValueToBrushRangeStop.BrushProperty, value);
			}
		}

		#endregion //Brush
	}

	#endregion //ValueToBrushRangeStop	

	#region ValueToBrushRangeStopCollection

	/// <summary>
	/// A collection of <see cref="ValueToBrushRangeStop"/>s exposed by the <see cref="ValueToBrushRangeConverter"/> class.
	/// </summary>
	public class ValueToBrushRangeStopCollection : ValueRangeStopCollection<ValueToBrushRangeStop>
	{
	}

	#endregion //ValueToBrushRangeStopCollection	

	#region ValueToBrushRangeConverter

	/// <summary>
	/// A converter that converts a value into a Brush
	/// </summary>
	public class ValueToBrushRangeConverter : ValueRangeConverterBase
	{
		#region Private Members

		private ValueToBrushRangeStopCollection _stops;

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
				throw new NotSupportedException( SR.GetString( "LE_NotSupportedException_9", targetType.Name ) );

			// if we don't have any stops in the collection or if the value is null then return the default brush 
			if (value == null ||
				value is DBNull ||
				this.StartValue == null ||
				this.EndValue == null ||
				this.Stops.Count == 0)
				return this.DefaultBrush;

			// first make sure the stop collection is sorted
			this._stops.Sort();

			ValueToBrushRangeStop previousStop = null;

			try
			{
				double percentOfTotalRange = this.StartValue.CalculatePercentInRange(value, this.EndValue, culture);

				foreach (ValueToBrushRangeStop stop in this._stops)
				{
					if (stop.Offset > percentOfTotalRange)
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

									double rangeBetweenStops = stop.Offset - previousStop.Offset;

									Debug.Assert (rangeBetweenStops >= 0);

									if ( rangeBetweenStops <= 0 )
										return previousStop.Brush;

									double percentOfRangeBetweenStops = (percentOfTotalRange - previousStop.Offset ) / rangeBetweenStops;

									if (percentOfRangeBetweenStops <= 0)
										return previousStop.Brush;

									if (percentOfRangeBetweenStops >= 1)
										return stop.Brush;

									Color fromColor = fromBrush.Color;
									Color toColor = toBrush.Color;

									Byte aValue = this.InterpolateColorValue(fromColor.A, toColor.A, percentOfRangeBetweenStops);
									Byte rValue = this.InterpolateColorValue(fromColor.R, toColor.R, percentOfRangeBetweenStops);
									Byte gValue = this.InterpolateColorValue(fromColor.G, toColor.G, percentOfRangeBetweenStops);
									Byte bValue = this.InterpolateColorValue(fromColor.B, toColor.B, percentOfRangeBetweenStops);

									return new SolidColorBrush(Color.FromArgb(aValue, rValue, gValue, bValue));
								}
							}
						}

						return stop.Brush;
					}

					previousStop = stop;
				}

				if (previousStop != null)
					return previousStop.Brush;
			}
			catch
			{ 
			}

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
			typeof(Brush), typeof(ValueToBrushRangeConverter), new FrameworkPropertyMetadata(Brushes.Black));

		/// <summary>
		/// Gets/sets the brush to use for values that can't be converted.
		/// </summary>
		//[Description("Gets/sets the brush to use for values that can't be converted.")]
		//[Category("Behavior")]
		public Brush DefaultBrush
		{
			get
			{
				return (Brush)this.GetValue(ValueToBrushRangeConverter.DefaultBrushProperty);
			}
			set
			{
				this.SetValue(ValueToBrushRangeConverter.DefaultBrushProperty, value);
			}
		}

			#endregion //DefaultBrush

			#region Stops

		/// <summary>
		/// Gets/sets the collection of <see cref="ValueToBrushRangeStop"/>s.
		/// </summary>
		public ValueToBrushRangeStopCollection Stops
		{
			get
			{
				if (this._stops == null)
				{
					this._stops = new ValueToBrushRangeStopCollection();

					// listen for changes
					this._stops.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnStopsCollectionChanged);
				}

				return this._stops;
			}
			set
			{
				if (value != this._stops)
				{
					// unhook from old collection
					if ( this._stops != null )
						this._stops.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnStopsCollectionChanged);

					this._stops = value;

					// listen for changes
					if ( this._stops != null )
						this._stops.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnStopsCollectionChanged);
					
                    this.RaisePropertyChangedEvent("Stops");
				}
			}
		}

		private void OnStopsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			this.VerifyStopTypes();
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

	#endregion //ValueToBrushRangeConverter	        
    
	#endregion //Derived classes for Brush target 
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