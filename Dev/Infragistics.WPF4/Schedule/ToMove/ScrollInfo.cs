using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Infragistics.Collections;
using System.Windows.Automation.Provider;
using System.Windows.Automation.Peers;
using System.Windows.Threading;
using System.Windows.Automation;
using Infragistics.Controls.Schedules;

namespace Infragistics.Controls.Primitives
{
	#region ScrollInfo
	/// <summary>
	/// Exposes scrollbar related settings.
	/// </summary>
	internal class ScrollInfo : INotifyPropertyChanged
		, ISupportPropertyChangeNotifications
	{
		#region Member Variables

		private bool _suspendPropertyChanges;
		private List<string> _suspendedPropertyChanges;
		private PropertyChangeListenerList _propChangeListeners;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ScrollInfo"/>
		/// </summary>
		public ScrollInfo()
		{
			_propChangeListeners = new PropertyChangeListenerList();
		}
		#endregion //Constructor

		#region Properties

		#region ComputedScrollBarIsEnabled
		private bool _computedScrollBarIsEnabled = true;

		/// <summary>
		/// Returns a value indicating the preferred IsEnabled state of the scrollbar.
		/// </summary>
		public bool ComputedScrollBarIsEnabled
		{
			get { return _computedScrollBarIsEnabled; }
			private set
			{
				if (value != _computedScrollBarIsEnabled)
				{
					_computedScrollBarIsEnabled = value;
					this.RaisePropertyChangedEvent("ComputedScrollBarIsEnabled");
				}
			}
		}
		#endregion //ComputedScrollBarIsEnabled

		#region ComputedScrollBarVisibility
		private Visibility _computedScrollBarVisibility = Visibility.Collapsed;

		/// <summary>
		/// Returns a value indicating the preferred visibility of the scrollbar.
		/// </summary>
		public Visibility ComputedScrollBarVisibility
		{
			get { return _computedScrollBarVisibility; }
			private set
			{
				if (value != _computedScrollBarVisibility)
				{
					_computedScrollBarVisibility = value;
					this.RaisePropertyChangedEvent("ComputedScrollBarVisibility");
				}
			}
		}
		#endregion //ComputedScrollBarVisibility

		#region Extent
		private double _extent = 0;

		/// <summary>
		/// Returns the Extent value
		/// </summary>
		public double Extent
		{
			get { return _extent; }
			private set
			{
				if (value != _extent)
				{
					_extent = value;
					this.RaisePropertyChangedEvent("Extent");
				}
			}
		}
		#endregion //Extent

		// AS 4/19/11 TFS73147
		#region IsInUse
		/// <summary>
		/// Indicates if the scroll info is in use.
		/// </summary>
		internal virtual bool IsInUse
		{
			get { return true; }
		}
		#endregion //IsInUse

		#region Maximum
		// AS 11/2/10 TFS58663
		private double _maximum = 0;

		/// <summary>
		/// Returns the Maximum
		/// </summary>
		public double Maximum
		{
			// AS 11/2/10 TFS58663
			// For some double values this results in precision issues so we'll
			// calculate and cache the maximum using decimals when doing the 
			// calculations to try and ensure the precision.
			//
			//get { return this.ScrollableExtent + this.Minimum; }
			get { return _maximum; }
		} 
		#endregion // Maximum

		#region Minimum
		private double _minimum = 0;

		/// <summary>
		/// Returns the Minimum value
		/// </summary>
		public double Minimum
		{
			get { return _minimum; }
			set
			{
				if (value != _minimum)
				{
					Debug.Assert(!double.IsNaN(value) && !double.IsInfinity(value));

					_minimum = value;
					this.RaisePropertyChangedEvent("Minimum");

					this.CalculateMaximum(); // AS 11/2/10 TFS58663
				}
			}
		}
		#endregion //Minimum

		#region Offset
		private double _offset = 0;

		/// <summary>
		/// Returns the Offset value
		/// </summary>
		public double Offset
		{
			get { return _offset; }
			set
			{
				if (value != _offset)
				{
					// ensure its in the scroll range
					value = Math.Max(Math.Min(this.Maximum, value), this.Minimum);

					// AS 2/24/12 TFS102945
					// While debugging I noticed that we could erroneously send a property 
					// change if the value passed in was outside the range so check the 
					// value again.
					//
					if (value != _offset)
					{
						_offset = value;
						this.RaisePropertyChangedEvent("Offset");
					}
				}
			}
		}
		#endregion //Offset

		#region ScrollBarVisibility
		private ScrollBarVisibility _scrollBarVisibility = ScrollBarVisibility.Auto;

		/// <summary>
		/// Returns or sets a value used to determine when the scroll bar should be visible
		/// </summary>
		public ScrollBarVisibility ScrollBarVisibility
		{
			get { return _scrollBarVisibility; }
			set
			{
				if (value != _scrollBarVisibility)
				{
					_scrollBarVisibility = value;
					this.RaisePropertyChangedEvent("ScrollBarVisibility");

					this.VerifyComputedVisibility();
				}
			}
		}
		#endregion //ScrollBarVisibility

		#region ScrollableExtent
		private double _scrollableExtent = 0;

		/// <summary>
		/// Returns the difference between the viewport and extent.
		/// </summary>
		public double ScrollableExtent
		{
			get { return _scrollableExtent; }
			private set
			{
				if (value != _scrollableExtent)
				{
					Debug.Assert(!double.IsNaN(value) && !double.IsInfinity(value));

					_scrollableExtent = value;
					this.RaisePropertyChangedEvent("ScrollableExtent");

					this.CalculateMaximum(); // AS 11/2/10 TFS58663
				}
			}
		}
		#endregion //ScrollableExtent

		#region SmallChange
		private double _smallChange = 1;

		/// <summary>
		/// Returns the SmallChange value
		/// </summary>
		public double SmallChange
		{
			get { return _smallChange; }
			set
			{
				if (value != _smallChange)
				{
					Debug.Assert(!double.IsNaN(value) && !double.IsInfinity(value));

					_smallChange = value;
					this.RaisePropertyChangedEvent("SmallChange");
				}
			}
		}
		#endregion //SmallChange

		#region Viewport
		private double _viewport = 0;

		/// <summary>
		/// Returns the Viewport value
		/// </summary>
		public double Viewport
		{
			get { return _viewport; }
			private set
			{
				if (value != _viewport)
				{
					_viewport = value;
					this.RaisePropertyChangedEvent("Viewport");
				}
			}
		}
		#endregion //Viewport

		#endregion //Properties

		#region Methods

		// AS 11/2/10 TFS58663
		#region CalculateMaximum
		private void CalculateMaximum()
		{
			try
			{
				// use decimal to avoid imprecision
				double max = (double)((decimal)_scrollableExtent + (decimal)_minimum);

				if ( max != _maximum )
				{
					_maximum = max;
					this.RaisePropertyChangedEvent("Maximum");
				}
			}
			catch
			{
				_maximum = _scrollableExtent + _minimum;
			}
		}
		#endregion // CalculateMaximum

		#region GetPercent
		internal double GetPercent()
		{
			return GetPercent(_offset, _minimum, _extent, _viewport);
		}

		internal static double GetPercent(double value, double minimum, double extent, double viewPort)
		{
			if (CoreUtilities.GreaterThan(extent, viewPort))
				return ((value - minimum) / (extent - viewPort)) * 100d;

			return -1d;
		}
		#endregion // GetPercent

		#region GetViewSize
		internal double GetViewSize()
		{
			return GetViewSize(_extent, _viewport);
		}

		internal static double GetViewSize(double extent, double viewPort)
		{
			if (CoreUtilities.LessThan(viewPort, extent))
				return (viewPort / extent) * 100d;

			return 100d;
		}
		#endregion // GetViewSize

		#region Initialize
		/// <summary>
		/// Initializes the state of the scroll info.
		/// </summary>
		/// <param name="viewport">The viewport extent (i.e. the visible area)</param>
		/// <param name="extent">The overall extent (i.e. the size of the content)</param>
		/// <param name="offset">The position within the scrollable area</param>
		public void Initialize(double viewport, double extent, double offset)
		{
			bool suspended = this.SuspendChangeNotifications();

			try
			{
				this.Extent = Math.Max(extent, 0);
				this.Viewport = Math.Max(viewport, 0);
				this.ScrollableExtent = Math.Max(this.Extent - this.Viewport, 0);
				this.Offset = Math.Max(Math.Min(this.Maximum, offset), this.Minimum);

				this.ComputedScrollBarIsEnabled = CoreUtilities.LessThan(this.Viewport, this.Extent);
				this.VerifyComputedVisibility();
			}
			finally
			{
				if (suspended)
					this.ResumeChangeNotifications();
			}
		}
		#endregion //Initialize

		#region OnPropertyChanged
		/// <summary>
		/// Invoked when a property notification should be sent.
		/// </summary>
		/// <param name="propertyName">The string identifying the property being changed</param>
		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handler = this.PropertyChanged;

			if (null != handler)
				handler(this, new PropertyChangedEventArgs(propertyName));

			_propChangeListeners.OnPropertyValueChanged(this, propertyName, null);
		}
		#endregion // OnPropertyChanged

		#region RaisePropertyChangedEvent
		private void RaisePropertyChangedEvent(string propertyName)
		{
			
			if (_suspendPropertyChanges)
			{
				if (_suspendedPropertyChanges == null)
					_suspendedPropertyChanges = new List<string>();

				_suspendedPropertyChanges.Add(propertyName);
				return;
			}

			this.OnPropertyChanged(propertyName);
		}
		#endregion //RaisePropertyChangedEvent

		#region ResumeChangeNotifications
		private void ResumeChangeNotifications()
		{
			Debug.Assert(_suspendPropertyChanges);

			_suspendPropertyChanges = false;
			List<string> changedProps = _suspendedPropertyChanges;
			_suspendedPropertyChanges = null;

			int changeCount = changedProps == null ? 0 : changedProps.Count;

			if (changeCount == 1)
				this.RaisePropertyChangedEvent(changedProps[0]);
			else if (changeCount > 1)
				this.RaisePropertyChangedEvent(string.Empty);
		}
		#endregion //ResumeChangeNotifications

		#region Scroll(double)
		internal void Scroll(double offset)
		{
			this.Offset = Math.Max(this.Minimum, Math.Min(this.Maximum, _offset + offset));
		}
		#endregion // Scroll(double)

		#region Scroll(ScrollAmount)
		internal virtual void Scroll(ScrollAmount scrollAmount)
		{
			double offset = 0;

			switch (scrollAmount)
			{
				case ScrollAmount.SmallDecrement:
				{
					offset = -this.SmallChange;
					break;
				}
				case ScrollAmount.SmallIncrement:
				{
					offset = this.SmallChange;
					break;
				}
				case ScrollAmount.LargeDecrement:
				{
					offset = -this.Viewport;
					break;
				}
				case ScrollAmount.LargeIncrement:
				{
					offset = this.Viewport;
					break;
				}
			}

			this.Scroll(offset);
		}
		#endregion // Scroll(ScrollAmount)

		#region ScrollToPercent
		internal void ScrollToPercent(double percent)
		{
			if (percent < 0 || percent > 100d)
				throw new ArgumentOutOfRangeException();

			if (CoreUtilities.GreaterThan(_extent, _viewport))
			{
				this.Offset = (_extent - _viewport) * percent / 100d + _minimum;
			}
		}
		#endregion // ScrollToPercent

		#region SuspendChangeNotifications
		private bool SuspendChangeNotifications()
		{
			if (_suspendPropertyChanges)
				return false;

			_suspendPropertyChanges = true;
			return true;
		}
		#endregion //SuspendChangeNotifications

		#region VerifyComputedVisibility
		private void VerifyComputedVisibility()
		{
			Visibility vis = Visibility.Collapsed;

			switch (_scrollBarVisibility)
			{
				case System.Windows.Controls.ScrollBarVisibility.Hidden:
				case System.Windows.Controls.ScrollBarVisibility.Disabled:
					break;
				case System.Windows.Controls.ScrollBarVisibility.Auto:
					if (CoreUtilities.LessThan(this.Viewport, this.Extent))
						vis = Visibility.Visible;
					break;
				case System.Windows.Controls.ScrollBarVisibility.Visible:
					vis = Visibility.Visible;
					break;
			}

			this.ComputedScrollBarVisibility = vis;
		}
		#endregion //VerifyComputedVisibility

		#endregion //Methods

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Invoked when the value of a property of the object has changed.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion //INotifyPropertyChanged Members

		#region ISupportPropertyChangeNotifications Implementation

		void ITypedSupportPropertyChangeNotifications<object, string>.AddListener(ITypedPropertyChangeListener<object, string> listener, bool useWeakReference)
		{
			_propChangeListeners.Add(listener, useWeakReference);
		}

		void ITypedSupportPropertyChangeNotifications<object, string>.RemoveListener(ITypedPropertyChangeListener<object, string> listener)
		{
			_propChangeListeners.Remove(listener);
		}

		#endregion // ISupportPropertyChangeNotifications Implementation
	} 
	#endregion // ScrollInfo

	#region ScrollBarInfoMediator
	/// <summary>
	/// Helper class for managing the properties of a ScrollBar and ScrollInfo
	/// </summary>
	internal class ScrollBarInfoMediator : DependencyObject
	{
		#region Member Variables

		private ScrollInfo _scrollInfo;
		private ScrollBar _scrollBar;
		private bool _isInitializing;
		private Action _scrollBarVisibilityAction;

		#endregion // Member Variables

		#region Constructor
		internal ScrollBarInfoMediator(ScrollInfo scrollInfo)
		{
			_scrollInfo = scrollInfo;
			_scrollInfo.PropertyChanged += new PropertyChangedEventHandler(OnScrollInfoPropertyChanged);
		}
		#endregion // Constructor

		#region Properties

		#region IsInitializingScrollBar
		/// <summary>
		/// Returns a boolean indicating if the scrollbar is being initialized based on the scroll info.
		/// </summary>
		public bool IsInitializingScrollBar
		{
			get { return _isInitializing; }
		} 
		#endregion // IsInitializingScrollBar

		#region Offset

		private static readonly DependencyProperty OffsetProperty = DependencyProperty.Register("Offset",
			typeof(double), typeof(ScrollBarInfoMediator), new PropertyMetadata(0d, new PropertyChangedCallback(OnOffsetChanged)));

		private static void OnOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ScrollBarInfoMediator adapter = d as ScrollBarInfoMediator;

			if (adapter._scrollBar != null && !adapter._isInitializing)
			{
				adapter._scrollInfo.Offset = (double)e.NewValue;
			}
		}

		#endregion //Offset

		#region ScrollBar
		internal ScrollBar ScrollBar
		{
			get { return _scrollBar; }
			set
			{
				if (value != _scrollBar)
				{
					_scrollBar = value;

					if (_scrollBar != null)
					{
						// update the properties of the scrollbar with the state of the scrollinfo
						this.InitializeScrollBar();

						// now that the scrollbar is hooked up we can keep a two way binding so our scrollinfo
						// in case the end user changes the scroll offset (i.e. drags the scroll thumb, etc)
						BindingOperations.SetBinding(this, OffsetProperty, new Binding { Source = _scrollBar, Path = new PropertyPath("Value"), Mode = BindingMode.TwoWay });
					}
					else
					{
						this.ClearValue(OffsetProperty);
					}
				}
			}
		}
		#endregion // ScrollBar

		#region ScrollBarVisibilityAction
		internal Action ScrollBarVisibilityAction
		{
			get { return _scrollBarVisibilityAction; }
			set { _scrollBarVisibilityAction = value; }
		}
		#endregion // ScrollBarVisibilityAction

		#region ScrollInfo
		internal ScrollInfo ScrollInfo
		{
			get { return _scrollInfo; }
		}
		#endregion // ScrollInfo

		#endregion // Properties

		#region Methods

		#region InitializeScrollBar
		private void InitializeScrollBar()
		{
			ScrollBar sb = _scrollBar;

			if (sb == null)
				return;

			Debug.Assert(!_isInitializing);

			if (_isInitializing)
				return;

			try
			{
				_isInitializing = true;
				sb.Minimum = _scrollInfo.Minimum;
				sb.SmallChange = _scrollInfo.SmallChange;
				sb.Maximum = _scrollInfo.Maximum;
				sb.ViewportSize = _scrollInfo.Viewport;
				sb.LargeChange = _scrollInfo.Viewport;
				sb.Value = _scrollInfo.Offset;
				sb.IsEnabled = _scrollInfo.ComputedScrollBarIsEnabled;
				this.UpdateScrollBarVisibility();
			}
			finally
			{
				_isInitializing = false;
			}
		}
		#endregion // InitializeScrollBar

		#region OnScrollInfoPropertyChanged
		private void OnScrollInfoPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (_scrollBar != null)
			{
				if (string.IsNullOrEmpty(e.PropertyName))
					this.InitializeScrollBar();
				else
				{
					switch (e.PropertyName)
					{
						case "Minimum":
							_scrollBar.Minimum = _scrollInfo.Minimum;
							break;
						case "Maximum":
						case "ScrollableExtent":
							_scrollBar.Maximum = _scrollInfo.Maximum;
							break;
						case "Viewport":
							_scrollBar.ViewportSize = _scrollInfo.Viewport;
							_scrollBar.LargeChange = _scrollInfo.Viewport;
							break;
						case "Offset":
							_scrollBar.Value = _scrollInfo.Offset;
							break;
						case "ComputedScrollBarIsEnabled":
							_scrollBar.IsEnabled = _scrollInfo.ComputedScrollBarIsEnabled;
							break;
						case "ComputedScrollBarVisibility":
							this.UpdateScrollBarVisibility();
							break;
						case "Extent":
							break;
						case "SmallChange":
							_scrollBar.SmallChange = _scrollInfo.SmallChange;
							break;
						default:
							Debug.Assert(false, "Unexpected property change? " + e.PropertyName);
							break;
					}
				}
			}
		}
		#endregion // OnScrollInfoPropertyChanged

		#region UpdateScrollBarVisibility
		private void UpdateScrollBarVisibility()
		{
			if (_scrollBarVisibilityAction != null)
				_scrollBarVisibilityAction();
			else if (_scrollBar != null)
				_scrollBar.Visibility = _scrollInfo.ComputedScrollBarVisibility;
		}
		#endregion // UpdateScrollBarVisibility

		#endregion // Methods
	} 
	#endregion // ScrollBarInfoMediator

	#region ScrollBarVisibilityCoordinator
	/// <summary>
	/// Helper class for synchronizing/coordinating the visibility of multiple ScrollBarInfoMediator instances. Essentially this 
	/// is used when there are multiple scrollbars in the ui and their Visibility should be tied together.
	/// </summary>
	internal class ScrollBarVisibilityCoordinator
	{
		#region Member Variables

		private DeferredOperation _deferredOperation;
		private List<ScrollBarInfoMediator> _mediators;

		#endregion // Member Variables

		#region Constructor
		internal ScrollBarVisibilityCoordinator()
		{
			_deferredOperation = new DeferredOperation(this.ProcessPendingChanges);
			_mediators = new List<ScrollBarInfoMediator>();
		}
		#endregion // Constructor

		#region Methods

		#region Internal Methods
		internal void Add(ScrollBarInfoMediator mediator)
		{
			_mediators.Add(mediator);
			mediator.ScrollBarVisibilityAction = this.RequestSyncScrollBarVisibility;
			_deferredOperation.StartAsyncOperation();
		}

		internal bool Contains(ScrollBarInfoMediator mediator)
		{
			return _mediators.Contains(mediator);
		}

		internal void Remove(ScrollBarInfoMediator mediator)
		{
			if (_mediators.Remove(mediator))
				mediator.ScrollBarVisibilityAction = null;

			if (_mediators.Count > 0)
				_deferredOperation.StartAsyncOperation();
		}

		internal void StartAsyncVerification()
		{
			_deferredOperation.StartAsyncOperation();
		}
		#endregion // Internal Methods

		#region Private Methods

		#region RequestSyncScrollBarVisibility
		private void RequestSyncScrollBarVisibility()
		{
			_deferredOperation.StartAsyncOperation();
		}
		#endregion // RequestSyncScrollBarVisibility

		#region ProcessPendingChanges
		private void ProcessPendingChanges()
		{
			bool isVisible = false;

			foreach (ScrollBarInfoMediator mediator in _mediators)
			{
				// skip mediators that aren't in use
				if (null == mediator.ScrollBar)
					continue;

				if (mediator.ScrollInfo.ComputedScrollBarVisibility != Visibility.Collapsed)
					isVisible = true;
			}

			Visibility resolvedVis = isVisible ? Visibility.Visible : Visibility.Collapsed;

			foreach (ScrollBarInfoMediator mediator in _mediators)
			{
				var sb = mediator.ScrollBar;

				if (null != sb)
					mediator.ScrollBar.Visibility = resolvedVis;
			}
		}
		#endregion // ProcessPendingChanges

		#endregion // Private Methods

		#endregion // Methods
	} 
	#endregion // ScrollBarVisibilityCoordinator

	#region ElementScrollInfo
	/// <summary>
	/// Custom ScrollInfo class that is associated with a specific element and will invalidate the measure when the scroll position changes.
	/// </summary>
	internal class ElementScrollInfo : ScrollInfo
	{
		#region Member Variables

		private UIElement _element;

		#endregion // Member Variables

		#region Constructor
		internal ElementScrollInfo(UIElement element)
		{
			_element = element;
		}
		#endregion // Constructor

		#region Base class overrides

		// AS 4/19/11 TFS73147
		// Consider the panel no longer in use if the associated panel is collapsed. Note I don't
		// want to check IsVisible as that may not be a valid indicator as it is not synchronously 
		// maintained by the framework as the ancestor's visibility changes.
		//
		#region IsInUse
		/// <summary>
		/// Indicates if the scroll info is in use.
		/// </summary>
		internal override bool IsInUse
		{
			get { return base.IsInUse && _element.Visibility != Visibility.Collapsed; }
		}
		#endregion //IsInUse

		#region OnPropertyChanged
		protected override void OnPropertyChanged(string propertyName)
		{
			if (string.IsNullOrEmpty(propertyName) || propertyName == "Offset")
				_element.InvalidateMeasure();

			base.OnPropertyChanged(propertyName);
		}
		#endregion // OnPropertyChanged

		#endregion // Base class overrides
	} 
	#endregion // ElementScrollInfo

	#region MergedScrollInfo class
	/// <summary>
	/// Custom scroll info that is used to aggregate the values (other than offset) of multiple scroll infos
	/// </summary>
	internal class MergedScrollInfo : ScrollInfo
		, IPropertyChangeListener
	{
		#region Member Variables

		private bool _isSynchronizing;
		private bool _isDirty;
		private WeakSet<ScrollInfo> _sourceScrollInfos;
		private Action _dirtyAction;

		#endregion // Member Variables

		#region Constructor
		internal MergedScrollInfo()
		{
			_sourceScrollInfos = new WeakSet<ScrollInfo>();
		}
		#endregion // Constructor

		#region Base class overrides
		
		#region OnPropertyChanged
		protected override void OnPropertyChanged(string propertyName)
		{
			this.Dirty();

			base.OnPropertyChanged(propertyName);
		}
		#endregion // OnPropertyChanged

		#endregion // Base class overrides

		#region Properties

		#region DirtyAction
		internal Action DirtyAction
		{
			get { return _dirtyAction; }
			set { _dirtyAction = value; }
		}

		#endregion // DirtyAction

		#region IsDirty
		internal bool IsDirty
		{
			get { return _isDirty; }
		} 
		#endregion // IsDirty

		#endregion // Properties

		#region Methods

		#region Internal Methods

		#region Add
		internal void Add(ScrollInfo sourceScrollInfo)
		{
			if (_sourceScrollInfos.Add(sourceScrollInfo))
				((ISupportPropertyChangeNotifications)sourceScrollInfo).AddListener(this, true);
		}
		#endregion // Add

		#region Dirty
		internal void Dirty()
		{
			if (!_isDirty)
			{
				_isDirty = true;

				if (_dirtyAction != null)
					_dirtyAction();
			}
		}
		#endregion // Dirty

		#region Remove
		internal bool Remove(ScrollInfo sourceScrollInfo)
		{
			bool result = _sourceScrollInfos.Remove(sourceScrollInfo);

			if (result)
				((ISupportPropertyChangeNotifications)sourceScrollInfo).RemoveListener(this);

			return result;
		}
		#endregion // Remove

		#region VerifyState
		internal void VerifyState()
		{
			if (!_isDirty)
				return;

			Debug.Assert(!_isSynchronizing);

			_isSynchronizing = true;

			try
			{
				double viewport = 0;
				double extent = 0;

				foreach (ScrollInfo si in _sourceScrollInfos)
				{
					// AS 4/19/11 TFS73147
					if (!si.IsInUse)
						continue;

					viewport = Math.Max(viewport, si.Viewport);
					extent = Math.Max(extent, si.Extent);
				}

				this.Initialize(viewport, extent, this.Offset);

				foreach (ScrollInfo si in _sourceScrollInfos)
				{
					// AS 4/19/11 TFS73147
					if (!si.IsInUse)
						continue;

					si.Offset = this.Offset;
				}
			}
			finally
			{
				_isSynchronizing = false;
				_isDirty = false;
			}
		}
		#endregion // VerifyState

		#endregion // Internal Methods

		#endregion // Methods

		#region ITypedPropertyChangeListener<object,string> Members

		void ITypedPropertyChangeListener<object, string>.OnPropertyValueChanged(object dataItem, string property, object extraInfo)
		{
			// ignore the offset of the attached scroll infos. the main scrollbar drives the position
			// otherwise we will get into situations where we endlessly invalidate
			if (property == "Offset")
				return;

			this.Dirty();
		}

		#endregion
	}
	#endregion // MergedScrollInfo class

	#region ScrollInfoAutomationProvider
	/// <summary>
	/// Automation IScrollProvider that deals with a horizontal and vertical <see cref="ScrollInfo"/>
	/// </summary>
	internal class ScrollInfoAutomationProvider : object
		, IScrollProvider
		, IPropertyChangeListener
	{
		#region Member Variables

		private AutomationPeer _peer;
		private bool _isHorizontallyScrollable;
		private bool _isVerticallyScrollable;
		private double _horzViewSize;
		private double _horzPercent;
		private double _vertViewSize;
		private double _vertPercent;
		private ScrollInfo _horzScrollInfo;
		private ScrollInfo _vertScrollInfo;
		private DispatcherOperation _pendingUpdateOperation;

		#endregion // Member Variables

		#region Constructor
		internal ScrollInfoAutomationProvider(AutomationPeer peer, ScrollInfo horzScrollInfo, ScrollInfo vertScrollInfo)
		{
			CoreUtilities.ValidateNotNull(peer, "peer");
			_peer = peer;

			ScheduleUtilities.ManageListenerHelper(ref _horzScrollInfo, horzScrollInfo, this, true);
			ScheduleUtilities.ManageListenerHelper(ref _vertScrollInfo, vertScrollInfo, this, true);
		}
		#endregion // Constructor

		#region Properties

		#region IsHorizontallyScrollable
		private bool IsHorizontallyScrollable
		{
			get
			{
				ScrollInfo si = _horzScrollInfo;
				return si != null && CoreUtilities.GreaterThan(si.Extent, si.Viewport);
			}
		}
		#endregion // IsHorizontallyScrollable

		#region IsVerticallyScrollable
		private bool IsVerticallyScrollable
		{
			get
			{
				ScrollInfo si = _vertScrollInfo;
				return si != null && CoreUtilities.GreaterThan(si.Extent, si.Viewport);
			}
		}
		#endregion // IsVerticallyScrollable

		#endregion // Properties

		#region Methods

		#region UpdateAutomationInfo
		internal void UpdateAutomationInfo()
		{
			_pendingUpdateOperation = null;

			bool isHorz = this.IsHorizontallyScrollable;
			bool isVert = this.IsVerticallyScrollable;

			RaisePropertyChanged(ScrollPatternIdentifiers.HorizontallyScrollableProperty, ref _isHorizontallyScrollable, isVert);
			RaisePropertyChanged(ScrollPatternIdentifiers.HorizontalViewSizeProperty, ref _horzViewSize, isHorz ? _horzScrollInfo.GetViewSize() : 100d);
			RaisePropertyChanged(ScrollPatternIdentifiers.HorizontalScrollPercentProperty, ref _horzPercent, isHorz ? _horzScrollInfo.GetPercent() : -1d);

			RaisePropertyChanged(ScrollPatternIdentifiers.VerticallyScrollableProperty, ref _isVerticallyScrollable, isVert);
			RaisePropertyChanged(ScrollPatternIdentifiers.VerticalViewSizeProperty, ref _vertViewSize, isVert ? _vertScrollInfo.GetViewSize() : 100d);
			RaisePropertyChanged(ScrollPatternIdentifiers.VerticalScrollPercentProperty, ref _vertPercent, isVert ? _vertScrollInfo.GetPercent() : -1d);
		}
		#endregion // UpdateAutomationInfo

		#region RaisePropertyChanged
		private void RaisePropertyChanged<T>(AutomationProperty property, ref T oldValue, T newValue)
			where T : IEquatable<T>
		{
			if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
			{
				// update the member
				oldValue = newValue;
				_peer.RaisePropertyChangedEvent(property, (object)oldValue, (object)newValue);
			}
		}
		#endregion // RaisePropertyChanged

		#endregion // Methods

		#region IScrollProvider Members

		double IScrollProvider.HorizontalScrollPercent
		{
			get { return this.IsHorizontallyScrollable ? _horzScrollInfo.GetPercent() : -1d; }
		}

		double IScrollProvider.HorizontalViewSize
		{
			get { return this.IsHorizontallyScrollable ? _horzScrollInfo.GetViewSize() : 100d; }
		}

		bool IScrollProvider.HorizontallyScrollable
		{
			get { return this.IsHorizontallyScrollable; }
		}

		void IScrollProvider.Scroll(ScrollAmount horizontalAmount, ScrollAmount verticalAmount)
		{
			if (!_peer.IsEnabled())
				throw new ElementNotEnabledException();

			bool horz = horizontalAmount != ScrollAmount.NoAmount;
			bool vert = verticalAmount != ScrollAmount.NoAmount;

			if (!horz && !vert)
				return;

			if ((horz && !IsHorizontallyScrollable) || (vert && !this.IsVerticallyScrollable))
				throw new InvalidOperationException();

			if (horz)
				_horzScrollInfo.Scroll(horizontalAmount);

			if (vert)
				_vertScrollInfo.Scroll(verticalAmount);
		}

		void IScrollProvider.SetScrollPercent(double horizontalPercent, double verticalPercent)
		{
			if (!_peer.IsEnabled())
				throw new ElementNotEnabledException();

			bool horz = horizontalPercent != -1d;
			bool vert = verticalPercent != -1d;

			if (!horz && !vert)
				return;

			if ((horz && !IsHorizontallyScrollable) || (vert && !this.IsVerticallyScrollable))
				throw new InvalidOperationException();

			ScrollInfo si = horz ? _horzScrollInfo : _vertScrollInfo;
			si.ScrollToPercent(horz ? horizontalPercent : verticalPercent);
		}

		double IScrollProvider.VerticalScrollPercent
		{
			get { return this.IsVerticallyScrollable ? _vertScrollInfo.GetPercent() : -1d; }
		}

		double IScrollProvider.VerticalViewSize
		{
			get { return this.IsVerticallyScrollable ? _vertScrollInfo.GetViewSize() : 100d; }
		}

		bool IScrollProvider.VerticallyScrollable
		{
			get { return this.IsVerticallyScrollable; }
		}

		#endregion //IScrollProvider

		#region ITypedPropertyChangeListener<object,string> Members

		void ITypedPropertyChangeListener<object, string>.OnPropertyValueChanged(object dataItem, string property, object extraInfo)
		{
			if (null == _pendingUpdateOperation)
			{
				_pendingUpdateOperation = _peer.Dispatcher.BeginInvoke(new ScheduleUtilities.MethodInvoker(this.UpdateAutomationInfo), Type.EmptyTypes);
			}
		}

		#endregion //ITypedPropertyChangeListener<object,string> Members
	} 
	#endregion // ScrollInfoAutomationProvider

	// AS 2/24/12 TFS102945
	#region ScrollInfoTouchHelper
	/// <summary>
	/// Helper class for performing scroll operations resulting from touch interactions.
	/// </summary>
	internal class ScrollInfoTouchHelper : Object
		, ISupportScrollHelper
	{
		#region Member Variables

		private bool _isEnabled = true;
		private FrameworkElement _scrollAreaElement;

		private ScrollInfo _horizontalScrollInfo;
		private Func<double> _getFirstItemWidth;
		internal ScrollType HorizontalScrollType;
		internal Func<double, ScrollInfo, double> HorizontalOffsetCoerce;

		private ScrollInfo _verticalScrollInfo;
		private Func<double> _getFirstItemHeight;
		internal ScrollType VerticalScrollType;
		internal Func<double, ScrollInfo, double> VerticalOffsetCoerce;

		private TouchScrollHelper _touchScrollHelper;

		private double _lastOffsetX;
		private double _lastOffsetY;

		private List<Action<ScrollInfoTouchAction>> _pendingActions;
		private Func<FrameworkElement, Point, UIElement, TouchScrollMode> _getScrollModeFromPointCallback;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ScrollInfoTouchHelper"/>
		/// </summary>
		/// <param name="horizontalScrollInfo">The horizontal scroll info. This includes the ScrollInfo that does the scrolling, the type of scrolling done, a function to return the width of the first item.</param>
		/// <param name="verticalScrollInfo">The vertical scroll info. This includes the ScrollInfo that does the scrolling, the type of scrolling done, a function to return the height of the first item.</param>
		/// <param name="getScrollModeFromPointCallback">Optional method used to determine if the specified point is valid for scrolling</param>
		internal ScrollInfoTouchHelper(
			Tuple<ScrollInfo, ScrollType, Func<double>> horizontalScrollInfo,
			Tuple<ScrollInfo, ScrollType, Func<double>> verticalScrollInfo,
			Func<FrameworkElement, Point, UIElement, TouchScrollMode> getScrollModeFromPointCallback = null)
		{
			if (horizontalScrollInfo != null)
			{
				_horizontalScrollInfo = horizontalScrollInfo.Item1;
				HorizontalScrollType = horizontalScrollInfo.Item2;
				_getFirstItemWidth = horizontalScrollInfo.Item3;
			}

			if (verticalScrollInfo != null)
			{
				_verticalScrollInfo = verticalScrollInfo.Item1;
				VerticalScrollType = verticalScrollInfo.Item2;
				_getFirstItemHeight = verticalScrollInfo.Item3;
			}

			_getScrollModeFromPointCallback = getScrollModeFromPointCallback;
		}
		#endregion //Constructor

		#region Properties

		#region IsEnabled
		internal bool IsEnabled
		{
			get { return _isEnabled; }
			set
			{
				if (_isEnabled != value)
				{
					_isEnabled = value;

					InitializeIsManipulationEnabled(_scrollAreaElement, value);
				}
			}
		} 
		#endregion //IsEnabled

		#region ScrollAreaElement
		internal FrameworkElement ScrollAreaElement
		{
			get { return _scrollAreaElement; }
			set
			{
				if (_scrollAreaElement != value)
				{
					InitializeIsManipulationEnabled(_scrollAreaElement, false);

					_scrollAreaElement = value;

					InitializeIsManipulationEnabled(_scrollAreaElement, _isEnabled);

					if (value == null)
					{
						// disable the old one first so it gets unhooked properly
						if (_touchScrollHelper != null)
							_touchScrollHelper.IsEnabled = false;

						_touchScrollHelper = null;
					}
					else
					{
						_touchScrollHelper = new TouchScrollHelper(value, this);
						_touchScrollHelper.IsEnabled = _isEnabled;
					}
				}
			}
		}
		#endregion //ScrollAreaElement

		#region ShouldDeferMouseActions
		internal bool ShouldDeferMouseActions
		{
			get 
			{

				return false;



			}
		}
		#endregion //ShouldDeferMouseActions

		#endregion //Properties

		#region Methods

		#region Public Methods

		#region ClearPendingActions
		public void ClearPendingActions()
		{
			_pendingActions = null;
		}
		#endregion //ClearPendingActions

		#region EnqueuePendingAction
		public void EnqueuePendingAction(Action<ScrollInfoTouchAction> action)
		{
			// don't store anything while we are scrolling/holding/etc
			if (!this.ShouldDeferMouseActions || _touchScrollHelper.CurrentState != TouchState.Pending)
				return;

			if (_pendingActions == null)
				_pendingActions = new List<Action<ScrollInfoTouchAction>>();

			_pendingActions.Add(action);
		}
		#endregion //EnqueuePendingAction

		#endregion //Public Methods

		#region Private Methods

		#region ExecutePending
		private void ExecutePending(ScrollInfoTouchAction state)
		{
			var list = _pendingActions;
			_pendingActions = null;

			if (null != list)
			{
				foreach (var item in list)
					item(state);
			}
		}
		#endregion //ExecutePending

		#region GetMax
		private static double GetMax(ScrollInfo si)
		{
			if (si == null)
				return 0;

			return si.Maximum + -si.Minimum;
		}
		#endregion //GetMax

		#region GetOffset
		private static double GetOffset(ScrollInfo si, double lastOffset)
		{
			if (si == null)
				return 0;

			double currentOffset = si.Offset + -si.Minimum;

			if (Math.Abs(currentOffset - lastOffset) < 1)
				return lastOffset;

			return currentOffset;
		} 
		#endregion //GetOffset

		#region InitializeIsManipulationEnabled
		[Conditional("WPF")]
		private static void InitializeIsManipulationEnabled(FrameworkElement element, bool hook)
		{

			if (element != null)
			{
				// WPF uses the manipulation events which requires that the IsManipulationEnabled to be set but 
				// we should let the developer opt out of it
				var valueSource = DependencyPropertyHelper.GetValueSource(element, UIElement.IsManipulationEnabledProperty); 
				if (valueSource.BaseValueSource == BaseValueSource.Default)
				{
					if (hook)
						element.SetCurrentValue(UIElement.IsManipulationEnabledProperty, KnownBoxes.TrueBox);
					else if (valueSource.IsCurrent)
						element.ClearValue(UIElement.IsManipulationEnabledProperty);
				}
			}

		}
		#endregion //InitializeIsManipulationEnabled

		#region SetOffset
		private static void SetOffset(ScrollInfo si, double offset, Func<double, ScrollInfo, double> coerceOffset, ref double lastOffset)
		{
			if (si != null)
			{
				lastOffset = offset;

				offset += si.Minimum;

				if (null != coerceOffset)
					offset = coerceOffset(offset, si);

				si.Offset = offset;
			}
		} 
		#endregion //SetOffset

		#endregion //Private Methods

		#endregion //Methods

		#region ISupportScrollHelper members
		double ISupportScrollHelper.GetFirstItemHeight()
		{
			return _getFirstItemHeight();
		}

		double ISupportScrollHelper.GetFirstItemWidth()
		{
			return _getFirstItemWidth();
		}

		double ISupportScrollHelper.HorizontalMax
		{
			get { return GetMax(_horizontalScrollInfo); }
		}

		ScrollType ISupportScrollHelper.HorizontalScrollType
		{
			get { return HorizontalScrollType; }
		}

		double ISupportScrollHelper.HorizontalValue
		{
			get { return GetOffset(_horizontalScrollInfo, _lastOffsetX); }
			set { SetOffset(_horizontalScrollInfo, value, this.HorizontalOffsetCoerce, ref _lastOffsetX); }
		}

		void ISupportScrollHelper.InvalidateScrollLayout()
		{
			
		}

		double ISupportScrollHelper.VerticalMax
		{
			get { return GetMax(_verticalScrollInfo); }
		}

		ScrollType ISupportScrollHelper.VerticalScrollType
		{
			get { return VerticalScrollType; }
		}

		double ISupportScrollHelper.VerticalValue
		{
			get { return GetOffset(_verticalScrollInfo, _lastOffsetY); }
			set { SetOffset(_verticalScrollInfo, value, this.VerticalOffsetCoerce, ref _lastOffsetY); }
		}


		void ISupportScrollHelper.OnPanComplete()
		{
		}

		TouchScrollMode ISupportScrollHelper.GetScrollModeFromPoint(Point point, UIElement elementDirectlyOver)
		{
			return _getScrollModeFromPointCallback == null ? TouchScrollMode.Both : _getScrollModeFromPointCallback(_scrollAreaElement, point, elementDirectlyOver);
		}

		void ISupportScrollHelper.OnStateChanged(TouchState newState, TouchState oldState)
		{
			if (oldState == TouchState.Pending)
			{
				// perform actions with true to indicate down
				if (newState == TouchState.Holding)
					this.ExecutePending(ScrollInfoTouchAction.Drag);
				else if (newState == TouchState.NotDown)
					this.ExecutePending(ScrollInfoTouchAction.Click);
				else
					this.ClearPendingActions();
			}
			else
				this.ClearPendingActions();
		}

		#endregion //ISupportScrollHelper members
	} 
	#endregion //ScrollInfoTouchHelper

	// AS 3/7/12 TFS102945
	#region ScrollInfoTouchAction members
	internal enum ScrollInfoTouchAction
	{
		/// <summary>
		/// The touch was initiated and dragged
		/// </summary>
		Drag,

		/// <summary>
		/// The touch was initiated and released
		/// </summary>
		Click,
	} 
	#endregion //ScrollInfoTouchAction members

	#region IScrollInfoProvider
	internal interface IScrollInfoProvider
	{
		ScrollInfo ScrollInfo { get; }
	} 
	#endregion //IScrollInfoProvider
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