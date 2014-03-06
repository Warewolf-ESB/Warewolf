using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;





using Infragistics;
using Infragistics.Controls.Layouts.Primitives;
using System.ComponentModel;
using System.Windows.Media;
using System.Diagnostics;


	using System.Windows.Interop;


using System.Windows.Input;
using Infragistics.Collections;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using Infragistics.AutomationPeers;
using System.Windows.Automation.Peers;
using System.Collections;
using Infragistics.Windows.Internal;

namespace Infragistics.Controls.Layouts
{
	/// <summary>
	/// A <see cref="System.Windows.Controls.ContentControl"/> derived element that represents a tile inside a <see cref="XamTileManager"/>.
	/// </summary>
	/// <remarks>
	/// <para class="note"><b>Note:</b> even though this class derives from ContentControl it exposes all of the properties and functionality of a HeaderedContentControl. 
	/// The reason the class doesn't derive from HeaderedContentControl is because the base PrepareItemContainer logic in ItemsControl assumes that 
	/// the item in the Items collection should be set as its Header, likewise the <see cref="ItemsControl.ItemTemplate"/> property should be set as the 
	/// HeaderTemplate. For tiles this is not the intent, instead the item should be the Tile's Content and the
	/// <b>ItemsControl.ItemTemplate</b> property should be used to initialize the ContentTemplate property.</para>
	/// </remarks>
	[TemplateVisualState(Name = VisualStateUtilities.StateNormal, GroupName = VisualStateUtilities.GroupCommon)]
	[TemplateVisualState(Name = VisualStateUtilities.StateMouseOver, GroupName = VisualStateUtilities.GroupCommon)]
	[TemplateVisualState(Name = VisualStateUtilities.StateDisabled, GroupName = VisualStateUtilities.GroupCommon)]

	[TemplateVisualState(Name = VisualStateUtilities.StateDragging, GroupName = VisualStateUtilities.GroupDrag)]
	[TemplateVisualState(Name = VisualStateUtilities.StateNotDragging, GroupName = VisualStateUtilities.GroupDrag)]
	[TemplateVisualState(Name = VisualStateUtilities.StateSwapTarget, GroupName = VisualStateUtilities.GroupDrag)]

	[TemplateVisualState(Name = VisualStateUtilities.StateMaximized, GroupName = VisualStateUtilities.GroupMinimized)]
	[TemplateVisualState(Name = VisualStateUtilities.StateMinimized, GroupName = VisualStateUtilities.GroupMinimized)]
	[TemplateVisualState(Name = VisualStateUtilities.StateMinimizedExpanded, GroupName = VisualStateUtilities.GroupMinimized)]
	[TemplateVisualState(Name = VisualStateUtilities.StateNotMinimized, GroupName = VisualStateUtilities.GroupMinimized)]

	[TemplatePart(Name = TransitionCanvas, Type = typeof(Canvas))]
	public class XamTile : ContentControl, IResizableElement, ICommandTarget, ITypedPropertyChangeListener<object, string>
	{
		#region Private Members

		const string TransitionCanvas = "TransitionCanvas";

		private XamTileManager _owningManager;
		private WeakReference _headerPresenter;

		private bool _isGenerated;
		private bool _isSynchonizingFromItemInfo;
		private bool _isInitialized;
		private bool _isMouseOver;
		private bool _isInResolveState;
		private bool _isMaximized;
		private bool _hasVisualStateGroups;
		private bool _isOnwerWired;








		// JJD 11//8/11 - TFS29248/TFS95683 - added
		private bool _wasMaximized;

		private TransformGroup		_transformGroup;
		private TranslateTransform	_translateTransform;
		private ScaleTransform		_scaleTransform;

		private Canvas _transitionCanvas;
		private Image _transitionImage;
		private ScaleTransform _transitionImageScaleTransform;





		#endregion //Private Members	

		#region Constructors

		static XamTile()
		{

			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamTile), new FrameworkPropertyMetadata(typeof(XamTile)));
			UIElement.IsEnabledProperty.OverrideMetadata(typeof(XamTile), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)));
			ContentControl.ContentTemplateProperty.OverrideMetadata(typeof(XamTile), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnContentTemplateChanged)));

		}

		/// <summary>
		/// Instantiates a new instance of <see cref="XamTile"/>
		/// </summary>
		public XamTile()
		{





		}

		#endregion //Constructors	
    
		#region Base class overrides

		#region LogicalChildren


		/// <summary>
		/// Gets an enumerator that can iterate the logical children of this element.
		/// </summary>
		protected override IEnumerator LogicalChildren
		{
			get
			{
				// In WPF ContentControl's base implementation of this property will return the
				// content as a logical child. However, if the XamTule was a generated
				// container then the content is already a logical child of the
				// XamTileControl
				if (_isGenerated)
					return EmptyEnumerator.Instance;

				return base.LogicalChildren;
			}
		}


		#endregion //LogicalChildren	

		#region OnContentChanged


		/// <summary>
		/// Called when the content has changed
		/// </summary>
		/// <param name="oldContent">the old content</param>
		/// <param name="newContent">the new content</param>
		protected override void OnContentChanged(object oldContent, object newContent)
		{
			// In WPF, ContentControl's base implementation of this method will add the 
			// content as a logical child. However, if the XamTule was a generated
			// container then the content is already a logical child of the
			// XamTileControl
			if (_isGenerated)
				return;

			base.OnContentChanged(oldContent, newContent);
		}


		#endregion //OnContentChanged	
    
		#region OnCreateAutomationPeer

		/// <summary>
		/// Returns an automation peer that exposes the <see cref="XamTile"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="XamTileAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new XamTileAutomationPeer(this);
		}

		#endregion //OnCreateAutomationPeer	

		#region OnApplyTemplate

		/// <summary>
		/// Invoked when the template has been applied to the element.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();






			_transitionCanvas = this.GetTemplateChild(TransitionCanvas) as Canvas;

			if (_owningManager == null)
			{
				XamTileManager tm = PresentationUtilities.GetVisualAncestor<XamTileManager>(this, null);

				if (tm != null)
					this.Initialize(tm);
			}

			this.ResolveContentTemplate();


			this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);



			this.UpdateVisualStates(false);
		}

		#endregion //OnApplyTemplate

		#region OnInitialized


		/// <summary>
		/// Overridden. Raises the <see cref="FrameworkElement.Initialized"/> event. This method is invoked when the <see cref="FrameworkElement.IsInitialized"/> is set to true.
		/// </summary>
		/// <param name="e">Event arguments</param>
		protected override void OnInitialized(EventArgs e)



		{

			base.OnInitialized(e);


			if (_isInitialized)
				return;

			_isInitialized = true;

		}
		#endregion //OnInitialized

		#region OnMouseEnter

		/// <summary>
		/// Invoked when an unhandled Mouse.MouseEnter attached event is raised on this element
		/// </summary>
		/// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> that contains the event data</param>
		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			_isMouseOver = true;

			this.UpdateVisualStates();

		}

		#endregion //OnMouseEnter

		#region OnMouseLeave

		/// <summary>
		/// Invoked when an unhandled Mouse.MouseLeave attached event is raised on this element
		/// </summary>
		/// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> that contains the event data</param>
		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			_isMouseOver = false;

			this.UpdateVisualStates();

		}

		#endregion //OnMouseLeave	



#region Infragistics Source Cleanup (Region)






































































































































#endregion // Infragistics Source Cleanup (Region)


		#endregion //Base class overrides	
        
		#region Properties

		#region Public Properties

		#region AllowMaximize

		/// <summary>
		/// Identifies the <see cref="AllowMaximize"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowMaximizeProperty = DependencyPropertyUtilities.Register("AllowMaximize",
			typeof(bool?), typeof(XamTile),
			// JJD 1/6/12 - TFS98921 - added change callback
			//null, null
			null, new PropertyChangedCallback(OnAllowMaximizeChanged)
			);

		// JJD 1/6/12 - TFS98921 - added
		private static void OnAllowMaximizeChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamTile tile = target as XamTile;

			if (tile._isInitialized)
				tile.SetResolvedVisibilityProperties();
		}

		/// <summary>
		/// Gets/sets whether this tile can be maximized by the user.
		/// </summary>
		/// <seealso cref="AllowMaximizeProperty"/>
		//[Description("Gets/sets whether this tile can be maximized by the user.")]
		//[Category("TilesControl Properties")]





		public bool? AllowMaximize
		{
			get
			{
				return (bool?)this.GetValue(XamTile.AllowMaximizeProperty);
			}
			set
			{
				this.SetValue(XamTile.AllowMaximizeProperty, value);
			}
		}

		#endregion //AllowMaximize

		// JJD 1/6/12 - TFS98921
		// Changed AllowMaximizeResolved to a dependency property since we are now binding to it 
		// from inside the TileHeaderPresenter template 
		#region AllowMaximizeResolved

		private static readonly DependencyPropertyKey AllowMaximizeResolvedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("AllowMaximizeResolved",
			typeof(bool), typeof(XamTile), KnownBoxes.TrueBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="AllowMaximizeResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowMaximizeResolvedProperty = AllowMaximizeResolvedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the resolved value as to whether this tile can be maximized (read-only).
		/// </summary>
		/// <seealso cref="AllowMaximizeProperty"/>
		/// <seealso cref="MaximizedModeSettings.MinimizedTileExpansionMode"/>
		/// <seealso cref="AllowMaximizeResolvedProperty"/>
		public bool AllowMaximizeResolved
		{
			get
			{
				// JJD 1/6/12 - TFS98921
				// Changed AllowMaximizeResolved to a dependency property since we are now binding to it 
				// from inside the TileHeaderPresenter template 
				//bool? allow = this.AllowMaximize;

				//if (allow.HasValue)
				//    return allow.Value;

				//XamTileManager tm = this.Manager;

				//if (tm != null)
				//    return tm.MaximizedTileLimit > 0;

				//return false;
				return (bool)this.GetValue(XamTile.AllowMaximizeResolvedProperty);
			}
		}

		#endregion //AllowMaximizeResolved

		#region ContentTemplateMaximized

		/// <summary>
		/// Identifies the <see cref="ContentTemplateMaximized"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ContentTemplateMaximizedProperty = DependencyPropertyUtilities.Register("ContentTemplateMaximized",
			typeof(DataTemplate), typeof(XamTile),
			null, new PropertyChangedCallback(OnContentTemplateMaximizedChanged)
			);

		private static void OnContentTemplateMaximizedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamTile tile = target as XamTile;


			if (tile.IsLoaded && tile.State == TileState.Maximized)



				tile.ResolveContentTemplate();
		}
		/// <summary>
		/// Gets/sets the ContentTemplate that will be used when the Tile's State is 'Maximized'.
		/// </summary>
		/// <seealso cref="ContentTemplateMaximizedProperty"/>
		//[Description("Gets/sets the ContentTemplate that will be used when the Tile's State is 'Maximized'.")]
		//[Category("TilesControl Properties")]
		public DataTemplate ContentTemplateMaximized
		{
			get
			{
				return (DataTemplate)this.GetValue(XamTile.ContentTemplateMaximizedProperty);
			}
			set
			{
				this.SetValue(XamTile.ContentTemplateMaximizedProperty, value);
			}
		}

		#endregion //ContentTemplateMaximized

		#region ContentTemplateMinimized

		/// <summary>
		/// Identifies the <see cref="ContentTemplateMinimized"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ContentTemplateMinimizedProperty = DependencyPropertyUtilities.Register("ContentTemplateMinimized",
			typeof(DataTemplate), typeof(XamTile),
			null, new PropertyChangedCallback(OnContentTemplateMinimizedChanged)
			);

		private static void OnContentTemplateMinimizedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamTile tile = target as XamTile;


			if (tile.IsLoaded && tile.State == TileState.Minimized)



				tile.ResolveContentTemplate();
		}
		/// <summary>
		/// Gets/sets the ContentTemplate that will be used when the Tile's State is 'Minimized'.
		/// </summary>
		/// <seealso cref="ContentTemplateMinimizedProperty"/>
		//[Description("Gets/sets the ContentTemplate that will be used when the Tile's State is 'Minimized'.")]
		//[Category("TilesControl Properties")]
		public DataTemplate ContentTemplateMinimized
		{
			get
			{
				return (DataTemplate)this.GetValue(XamTile.ContentTemplateMinimizedProperty);
			}
			set
			{
				this.SetValue(XamTile.ContentTemplateMinimizedProperty, value);
			}
		}

		#endregion //ContentTemplateMinimized

		#region ContentTemplateMinimizedExpanded

		/// <summary>
		/// Identifies the <see cref="ContentTemplateMinimizedExpanded"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ContentTemplateMinimizedExpandedProperty = DependencyPropertyUtilities.Register("ContentTemplateMinimizedExpanded",
			typeof(DataTemplate), typeof(XamTile),
			null, new PropertyChangedCallback(OnContentTemplateMinimizedExpandedChanged)
			);

		private static void OnContentTemplateMinimizedExpandedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamTile tile = target as XamTile;


			if (tile.IsLoaded && tile.State == TileState.MinimizedExpanded)



				tile.ResolveContentTemplate();
		}
		/// <summary>
		/// Gets/sets the ContentTemplate that will be used when the Tile's State is 'MinimizedExpanded'.
		/// </summary>
		/// <seealso cref="ContentTemplateMinimizedExpandedProperty"/>
		//[Description("Gets/sets the ContentTemplate that will be used when the Tile's State is 'MinimizedExpanded'.")]
		//[Category("TilesControl Properties")]
		public DataTemplate ContentTemplateMinimizedExpanded
		{
			get
			{
				return (DataTemplate)this.GetValue(XamTile.ContentTemplateMinimizedExpandedProperty);
			}
			set
			{
				this.SetValue(XamTile.ContentTemplateMinimizedExpandedProperty, value);
			}
		}

		#endregion //ContentTemplateMinimizedExpanded

		#region ContentTemplateResolved

		private static readonly DependencyPropertyKey ContentTemplateResolvedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ContentTemplateResolved",
			typeof(DataTemplate), typeof(XamTile), null, null);

		private void ResolveContentTemplate()
		{
			DataTemplate stateTemplate = null;

			switch (this.State)
			{
				case TileState.Maximized:
					stateTemplate = this.ContentTemplateMaximized;
					break;

				case TileState.Minimized:
					stateTemplate = this.ContentTemplateMinimized;
					break;

				case TileState.MinimizedExpanded:
					stateTemplate = this.ContentTemplateMinimizedExpanded;
					break;

			}

			if (stateTemplate == null)
				stateTemplate = this.ContentTemplate;

			this.SetValue(ContentTemplateResolvedPropertyKey, stateTemplate);
		}

		/// <summary>
		/// Identifies the read-only <see cref="ContentTemplateResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ContentTemplateResolvedProperty = ContentTemplateResolvedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the ContentTemplate to use based on the current <see cref="State"/> of the tile
		/// </summary>
		/// <seealso cref="ContentTemplateResolvedProperty"/>
		public DataTemplate ContentTemplateResolved
		{
			get
			{
				return (DataTemplate)this.GetValue(XamTile.ContentTemplateResolvedProperty);
			}
		}

		#endregion //ContentTemplateResolved

		#region ContentVisibility

		private static readonly DependencyPropertyKey ContentVisibilityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ContentVisibility",
			typeof(Visibility), typeof(XamTile), KnownBoxes.VisibilityVisibleBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="ContentVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ContentVisibilityProperty = ContentVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the visibility of the content (read-only)
		/// </summary>
		/// <value>Returns 'Visible' unless the <see cref="State"/> is 'Minimized' and a tempate for this state has not been provided thru the <see cref="ContentTemplateMinimized"/> property.</value>
		/// <remarks>
		/// <para class="note"><b>Note:</b> This property is used for binding to the ContentPesenter's Visibility inside the tile's template.</para>
		/// </remarks>
		/// <seealso cref="ContentTemplateMinimized"/>
		/// <seealso cref="XamTileManager.ItemTemplateMinimized"/>
		/// <seealso cref="ContentVisibilityProperty"/>
		[Bindable(true)]
		public Visibility ContentVisibility
		{
			get
			{
				return (Visibility)this.GetValue(XamTile.ContentVisibilityProperty);
			}
		}

		#endregion //ContentVisibility

		#region CloseAction

		/// <summary>
		/// Identifies the <see cref="CloseAction"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CloseActionProperty = DependencyPropertyUtilities.Register("CloseAction",
			typeof(TileCloseAction), typeof(XamTile),
			DependencyPropertyUtilities.CreateMetadata(TileCloseAction.Default, new PropertyChangedCallback(OnCloseActionChanged))
			);

		private static void OnCloseActionChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamTile tile = target as XamTile;

			// JJD 2/22/10 - TFS27935
			// Set the resolved visibility properties
			if (tile._isInitialized)
				tile.SetResolvedVisibilityProperties();
		}

		/// <summary>
		/// Gets/sets what happens when this tile is closed.
		/// </summary>
		/// <seealso cref="CloseActionProperty"/>
		/// <seealso cref="XamTileManager.TileCloseAction"/>
		/// <seealso cref="CloseButtonVisibility"/>
		/// <seealso cref="CloseButtonVisibilityResolved"/>
		//[Description("Gets/sets what happens when this tile is closed.")]
		//[Category("TilesControl Properties")]
		
		
		
		public TileCloseAction CloseAction
		{
			get
			{
				return (TileCloseAction)this.GetValue(XamTile.CloseActionProperty);
			}
			set
			{
				this.SetValue(XamTile.CloseActionProperty, value);
			}
		}

		#endregion //CloseAction
		
		// JJD 1/6/12 - TFS98921
		// Changed CloseActionResolved to a dependency property since we are now binding to it 
		// from inside the TileHeaderPresenter template 
		#region CloseActionResolved

		private static readonly DependencyPropertyKey CloseActionResolvedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("CloseActionResolved",
			typeof(TileCloseAction), typeof(XamTile), TileCloseAction.Default, null);

		/// <summary>
		/// Identifies the read-only <see cref="CloseActionResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CloseActionResolvedProperty = CloseActionResolvedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the resolved value as to what happens when this tile is closed (read-only).
		/// </summary>
		/// <seealso cref="CloseActionProperty"/>
		/// <seealso cref="CloseActionResolvedProperty"/>
		/// <seealso cref="XamTileManager.TileCloseAction"/>
		[ReadOnly(true)]
		public TileCloseAction CloseActionResolved
		{
			get
			{
				// JJD 1/6/12 - TFS98921
				// Changed CloseActionResolved to a dependency property since we are now binding to it 
				// from inside the TileHeaderPresenter template 
				//TileCloseAction action = this.CloseAction;

				//if (action != TileCloseAction.Default)
				//    return action;

				//if (_owningManager == null)
				//    return TileCloseAction.DoNothing;

				//action = _owningManager.TileCloseAction;

				//if (action != TileCloseAction.Default)
				//    return action;

				//return TileCloseAction.DoNothing;
				return (TileCloseAction)this.GetValue(XamTile.CloseActionResolvedProperty);
			}
		}

		#endregion //CloseActionResolved

		#region CloseButtonVisibility

		/// <summary>
		/// Identifies the <see cref="CloseButtonVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CloseButtonVisibilityProperty = DependencyPropertyUtilities.Register("CloseButtonVisibility",
			typeof(Visibility?), typeof(XamTile),
			null, new PropertyChangedCallback(OnCloseButtonVisibilityChanged)
			);

		private static void OnCloseButtonVisibilityChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamTile tile = target as XamTile;

			// JJD 2/22/10 - TFS27935
			// Set the resolved visibility properties
			if (tile._isInitialized)
				tile.SetResolvedVisibilityProperties();
		}

		/// <summary>
		/// Gets/sets the visibility of the close button in the header area
		/// </summary>
		/// <seealso cref="XamTileManager.TileCloseAction"/>
		/// <seealso cref="CloseAction"/>
		/// <seealso cref="CloseButtonVisibilityProperty"/>
		/// <seealso cref="CloseButtonVisibilityResolved"/>
		//[Description("Gets/sets the visibility of the close button in the header area")]
		//[Category("TilesControl Properties")]





		public Visibility? CloseButtonVisibility
		{
			get
			{
				return (Visibility?)this.GetValue(XamTile.CloseButtonVisibilityProperty);
			}
			set
			{
				this.SetValue(XamTile.CloseButtonVisibilityProperty, value);
			}
		}

		#endregion //CloseButtonVisibility

		#region CloseButtonVisibilityResolved

		private static readonly DependencyPropertyKey CloseButtonVisibilityResolvedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("CloseButtonVisibilityResolved",
			typeof(Visibility), typeof(XamTile),
			KnownBoxes.VisibilityCollapsedBox,
			new PropertyChangedCallback(OnCloseButtonVisibilityResolvedChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="CloseButtonVisibilityResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CloseButtonVisibilityResolvedProperty = CloseButtonVisibilityResolvedPropertyKey.DependencyProperty;

		private static void OnCloseButtonVisibilityResolvedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamTile instance = (XamTile)d;
			instance.InvalidateMeasure();

			if (instance.Panel != null)
				instance.Panel.InvalidateMeasure();
		}

		/// <summary>
		/// Determines the visibility of the close button in the TileHeaderPresenter (read-only)
		/// </summary>
		/// <seealso cref="CloseButtonVisibilityResolvedProperty"/>
		/// <seealso cref="XamTileManager.TileCloseAction"/>
		/// <seealso cref="CloseAction"/>
		/// <seealso cref="CloseButtonVisibility"/>
		//[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		public Visibility CloseButtonVisibilityResolved
		{
			get
			{
				return (Visibility)this.GetValue(XamTile.CloseButtonVisibilityResolvedProperty);
			}
		}

		#endregion //CloseButtonVisibilityResolved

		#region ExpandButtonVisibility

		/// <summary>
		/// Identifies the <see cref="ExpandButtonVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ExpandButtonVisibilityProperty = DependencyPropertyUtilities.Register("ExpandButtonVisibility",
			typeof(Visibility?), typeof(XamTile),
			null, new PropertyChangedCallback(OnExpandButtonVisibilityChanged)
			);

		private static void OnExpandButtonVisibilityChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamTile tile = target as XamTile;

			// Make sure to re-evaluate the ExpandButtonVisibilityResolved property
			tile.SetResolvedVisibilityProperties();
		}

		/// <summary>
		/// Gets/sets the visibility of the expand toggle button in the header area
		/// </summary>
		/// <seealso cref="XamTileManager.IsInMaximizedMode"/>
		/// <seealso cref="ExpandButtonVisibilityResolved"/>
		/// <seealso cref="ExpandButtonVisibilityProperty"/>
		//[Description("Gets/sets the visibility of the expand toggle button in the header area")]
		//[Category("TilesControl Properties")]





		public Visibility? ExpandButtonVisibility
		{
			get
			{
				return (Visibility?)this.GetValue(XamTile.ExpandButtonVisibilityProperty);
			}
			set
			{
				this.SetValue(XamTile.ExpandButtonVisibilityProperty, value);
			}
		}

		#endregion //ExpandButtonVisibility

		#region ExpandButtonVisibilityResolved

		private static readonly DependencyPropertyKey ExpandButtonVisibilityResolvedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ExpandButtonVisibilityResolved",
			typeof(Visibility), typeof(XamTile), KnownBoxes.VisibilityCollapsedBox, null);


		/// <summary>
		/// Identifies the <see cref="ExpandButtonVisibilityResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ExpandButtonVisibilityResolvedProperty =
			ExpandButtonVisibilityResolvedPropertyKey.DependencyProperty;

		/// <summary>
		/// Determines the visibility of the expand button in the TileHeaderPresenter (read-only)
		/// </summary>
		/// <seealso cref="ExpandButtonVisibilityResolvedProperty"/>
		/// <seealso cref="ExpandButtonVisibility"/>
		//[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		public Visibility ExpandButtonVisibilityResolved
		{
			get
			{
				return (Visibility)this.GetValue(XamTile.ExpandButtonVisibilityResolvedProperty);
			}
		}

		#endregion //ExpandButtonVisibilityResolved

		#region HasImage

		private static readonly DependencyPropertyKey HasImagePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("HasImage",
			typeof(bool), typeof(XamTile), KnownBoxes.FalseBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="HasImage"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasImageProperty = HasImagePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating if the <see cref="Image"/> property has been set.
		/// </summary>
		/// <seealso cref="HasImageProperty"/>
		/// <seealso cref="Image"/>
		//[Description("Returns a boolean indicating if the Image property has been set.")]
		//[Category("TilesControl Properties")] 
		[Bindable(true)]
		[ReadOnly(true)]
		//[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool HasImage
		{
			get
			{
				return (bool)this.GetValue(XamTile.HasImageProperty);
			}
		}

		#endregion //HasImage

		#region HasHeader

		private static readonly DependencyPropertyKey HasHeaderPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("HasHeader",
			typeof(bool), typeof(XamTile), KnownBoxes.FalseBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="HasHeader"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasHeaderProperty = HasHeaderPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating if the <see cref="Header"/> property has been set.
		/// </summary>
		/// <seealso cref="HasHeaderProperty"/>
		/// <seealso cref="Header"/>
		//[Description("Returns a boolean indicating if the Header property has been set.")]
		//[Category("TilesControl Properties")] 
		[Bindable(true)]
		[ReadOnly(true)]
		//[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool HasHeader
		{
			get
			{
				return (bool)this.GetValue(XamTile.HasHeaderProperty);
			}
		}

		#endregion //HasHeader

		#region Header

		/// <summary>
		/// Identifies the <see cref="Header"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HeaderProperty = DependencyPropertyUtilities.Register("Header",
			typeof(object), typeof(XamTile),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnHeaderChanged))
			);

		private static void OnHeaderChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamTile tile = target as XamTile;

			tile.SetValue(HasHeaderPropertyKey, KnownBoxes.FromValue(e.NewValue != null));

			tile.OnHeaderChanged(e.OldValue, e.NewValue);
		}

		/// <summary>
		/// Gets/sets an object that will appear in the header of this tile.
		/// </summary>
		/// <value>A header object or the default value of null.</value>
		/// <seealso cref="HeaderProperty"/>
		//[Description("Gets/sets an object that will appear in the header of this tile.")]
		//[Category("Content")]
		[Bindable(true)]
		//[Localizability(LocalizationCategory.Label)]
		public object Header
		{
			get
			{
				return (object)this.GetValue(XamTile.HeaderProperty);
			}
			set
			{
				this.SetValue(XamTile.HeaderProperty, value);
			}
		}

		#endregion //Header

		#region HeaderTemplate

		/// <summary>
		/// Identifies the <see cref="HeaderTemplate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HeaderTemplateProperty = DependencyPropertyUtilities.Register("HeaderTemplate",
			typeof(DataTemplate), typeof(XamTile),
			null, null
			);

		/// <summary>
		/// Gets/sets the template used to display the content of the tile's header.
		/// </summary>
		/// <seealso cref="HeaderTemplateProperty"/>
		//[Description("Gets/sets the template used to display the content of the tile's header.")]
		//[Category("Content")]
		[Bindable(true)]
		public DataTemplate HeaderTemplate
		{
			get
			{
				return (DataTemplate)this.GetValue(XamTile.HeaderTemplateProperty);
			}
			set
			{
				this.SetValue(XamTile.HeaderTemplateProperty, value);
			}
		}

		#endregion //HeaderTemplate

		#region Image

		/// <summary>
		/// Identifies the <see cref="Image"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ImageProperty = DependencyPropertyUtilities.Register("Image",
			typeof(ImageSource), typeof(XamTile),
			null, new PropertyChangedCallback(OnImageChanged)
			);

		private static void OnImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamTile instance = d as XamTile;

			if (e.NewValue == null)
				d.ClearValue(XamTile.HasImagePropertyKey);
			else
				d.SetValue(XamTile.HasImagePropertyKey, KnownBoxes.TrueBox);

			instance.InvalidateMeasure();

			if (instance.Panel != null)
				instance.Panel.InvalidateMeasure();
		}

		/// <summary>
		/// The image used in the tile's header/>.
		/// </summary>
		/// <seealso cref="ImageProperty"/>
		/// <seealso cref="HasImage"/>
		//[Description("The image used to in the tile's header.")]
		//[Category("TilesControl Properties")] 
		[Bindable(true)]
		public ImageSource Image
		{
			get
			{
				return (ImageSource)this.GetValue(XamTile.ImageProperty);
			}
			set
			{
				this.SetValue(XamTile.ImageProperty, value);
			}
		}

		#endregion //Image

		#region IsClosed

		/// <summary>
		/// Identifies the <see cref="IsClosed"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsClosedProperty = DependencyPropertyUtilities.Register("IsClosed",
			typeof(bool), typeof(XamTile),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsClosedChanged))
			);

		private static void OnIsClosedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamTile tile = (XamTile)d;

			tile.SetValue(VisibilityProperty, true == (bool)e.NewValue ? Visibility.Collapsed : Visibility.Visible);

			// make sure the ItemInfo object reflects this change
			if (tile._owningManager != null && !tile._isSynchonizingFromItemInfo)
			{
				tile._owningManager.SyncItemInfo(tile, false);

				// JJD 4/19/11 - TFS58732
				// Call OnTileIsClosedChanged which will try to maintain relative positioning
				// in the maximized list when maximized tiles are closed/unclosed
				
				
				
				//if (tile.IsClosed == false)
				//    panel.InvalidateMeasure();
				tile._owningManager.OnTileIsClosedChanged(tile);
			}
		}

		/// <summary>
		/// Returns or sets whether this tile is closed.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if true then this tile's Visibility will be coerced to 'Collapsed'</para>
		/// </remarks>
		/// <seealso cref="IsClosedProperty"/>
		/// <seealso cref="CloseAction"/>"/>
		public bool IsClosed
		{
			get
			{
				return (bool)this.GetValue(XamTile.IsClosedProperty);
			}
			set
			{
				this.SetValue(XamTile.IsClosedProperty, value);
			}
		}

		#endregion //IsClosed

		#region IsExpandedWhenMinimized

		/// <summary>
		/// Identifies the <see cref="IsExpandedWhenMinimized"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsExpandedWhenMinimizedProperty = DependencyPropertyUtilities.Register("IsExpandedWhenMinimized",
			typeof(bool?), typeof(XamTile),
			null, new PropertyChangedCallback(OnIsExpandedWhenMinimizedChanged)
			);


		private static void OnIsExpandedWhenMinimizedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamTile tile = target as XamTile;

			// make sure the ItemInfo object reflects this change
			if (!tile._isSynchonizingFromItemInfo)
			{
				if (tile._owningManager != null)
				{
					tile._owningManager.SyncItemInfo(tile, false);

					// Resolve the state if the tile is currently minimized
					switch (tile.State)
					{
						case TileState.Minimized:
						case TileState.MinimizedExpanded:
							tile.ResolveState();
							
							// bump the layout version so the new state get reflected in the ui
							tile._owningManager.BumpLayoutVersion();
							break;
					}

					tile.SetContentVisibility();
				}
			}
		}

		/// <summary>
		/// Gets/sets whether this tile is expanded when it is minimized
		/// </summary>
		/// <seealso cref="IsExpandedWhenMinimizedProperty"/>
		//[Description("Gets/sets whether this tile is expanded when it is minimized")]
		//[Category("TilesControl Properties")]
		[TypeConverter(typeof(System.Windows.NullableBoolConverter))]
		public bool? IsExpandedWhenMinimized
		{
			get
			{
				return (bool?)this.GetValue(XamTile.IsExpandedWhenMinimizedProperty);
			}
			set
			{
				this.SetValue(XamTile.IsExpandedWhenMinimizedProperty, value);
			}
		}

		#endregion //IsExpandedWhenMinimized

		#region IsMaximized

		/// <summary>
		/// Identifies the <see cref="IsMaximized"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsMaximizedProperty = DependencyPropertyUtilities.Register("IsMaximized",
			typeof(bool), typeof(XamTile),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsMaximizedChanged))
			);

		private static void OnIsMaximizedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamTile instance = (XamTile)d;
			
			instance._isMaximized = (bool)e.NewValue;

			XamTileManager tm = instance._owningManager;

			if (tm != null && tm._settingStateOfTile != instance)
			{
				if (instance._isMaximized != (instance.State == TileState.Maximized))
					tm.ExecuteTileCommandImpl(instance, TileCommandType.ToggleMaximized, null);

				instance.ResolveState();
			}
		}

		/// <summary>
		/// Returns or sets whether this tile's <see cref="State"/> is maximized.
		/// </summary>
		/// <seealso cref="State"/>
		/// <seealso cref="IsMaximizedProperty"/>
		public bool IsMaximized
		{
			get
			{
				return _isMaximized;
			}
			set
			{
				this.SetValue(XamTile.IsMaximizedProperty, value);
			}
		}

		#endregion //IsMaximized

		#region MaximizeButtonVisibility

		/// <summary>
		/// Identifies the <see cref="MaximizeButtonVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaximizeButtonVisibilityProperty = DependencyPropertyUtilities.Register("MaximizeButtonVisibility",
			typeof(Visibility?), typeof(XamTile),
			null, new PropertyChangedCallback(OnMaximizeButtonVisibilityChanged)
			);

		private static void OnMaximizeButtonVisibilityChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamTile tile = target as XamTile;

			
			// Make sure to re-evaluate the MaximizeButtonVisibilityResolved property
			tile.SetResolvedVisibilityProperties();
		}

		/// <summary>
		/// Gets/sets the visibility of the maximize toggle button in the header area
		/// </summary>
		/// <seealso cref="XamTileManager.MaximizedTileLimit"/>
		/// <seealso cref="MaximizeButtonVisibilityProperty"/>
		//[Description("Gets/sets the visibility of the maximize toggle button in the header area")]
		//[Category("TilesControl Properties")]





		public Visibility? MaximizeButtonVisibility
		{
			get
			{
				return (Visibility?)this.GetValue(XamTile.MaximizeButtonVisibilityProperty);
			}
			set
			{
				this.SetValue(XamTile.MaximizeButtonVisibilityProperty, value);
			}
		}

		#endregion //MaximizeButtonVisibility

		#region MaximizeButtonVisibilityResolved

		private static readonly DependencyPropertyKey MaximizeButtonVisibilityResolvedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("MaximizeButtonVisibilityResolved",
			typeof(Visibility), typeof(XamTile),
			KnownBoxes.VisibilityCollapsedBox,
			new PropertyChangedCallback(OnMaximizeButtonVisibilityResolvedChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="MaximizeButtonVisibilityResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaximizeButtonVisibilityResolvedProperty = MaximizeButtonVisibilityResolvedPropertyKey.DependencyProperty;

		private static void OnMaximizeButtonVisibilityResolvedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamTile instance = (XamTile)d;

			instance.InvalidateMeasure();
			if (instance.Panel != null)
				instance.Panel.InvalidateMeasure();
		}

		/// <summary>
		/// Determines the visibility of the maximize button in the TileHeaderPresenter (read-only)
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note</b>: if <see cref="ModeSettingsBase"/>.<see cref="XamTileManager.MaximizedTileLimit"/> is set to zero then this property will be set to 'Collapsed'. 
		/// Unless the <see cref="MaximizeButtonVisibility"/> is set.</para>
		/// </remarks>
		/// <seealso cref="MaximizeButtonVisibilityResolvedProperty"/>
		/// <seealso cref="MaximizeButtonVisibility"/>
		//[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		public Visibility MaximizeButtonVisibilityResolved
		{
			get
			{
				return (Visibility)this.GetValue(XamTile.MaximizeButtonVisibilityResolvedProperty);
			}
		}

		#endregion //MaximizeButtonVisibilityResolved



#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)



		#region SaveInLayout

		/// <summary>
		/// Identifies the <see cref="SaveInLayout"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SaveInLayoutProperty = DependencyPropertyUtilities.Register("SaveInLayout",
			typeof(bool), typeof(XamTile),
			KnownBoxes.TrueBox, null
			);

		/// <summary>
		/// Returns or sets a boolean indicating if the tile should be included in a saved layout.
		/// </summary>
		/// <seealso cref="SaveInLayoutProperty"/>
		//[Description("Indicates if the tile should be included in a saved layout.")]
		//[Category("TilesControl Properties")] 
		[Bindable(true)]
		public bool SaveInLayout
		{
			get
			{
				return (bool)this.GetValue(XamTile.SaveInLayoutProperty);
			}
			set
			{
				this.SetValue(XamTile.SaveInLayoutProperty, value);
			}
		}

		#endregion //SaveInLayout

		#region State

		private static readonly DependencyPropertyKey StatePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("State",
			typeof(TileState), typeof(XamTile),
			TileState.Normal,
			new PropertyChangedCallback(OnStateChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="State"/> dependency property
		/// </summary>
		public static readonly DependencyProperty StateProperty = StatePropertyKey.DependencyProperty;

		private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamTile tile = (XamTile)d;
			TileAreaPanel panel = tile.Panel;
			XamTileManager tm = tile.Manager;

			if (tm != null)
			{
				// JJD 11//8/11 - TFS29248/TFS95683 
				// keep track of whether we are transitioning from a maximized state
				tile._wasMaximized = object.Equals(e.OldValue, TileState.Maximized);

				if (tm.IsInitializedInternal &&
					!tile._isSynchonizingFromItemInfo)
				{
					TileState newState = (TileState)e.NewValue;
					TileState oldState = (TileState)e.OldValue;

					// if we will be shrinking the tile and we are animating the change
					// then take a snapshot of the tile while it was large to make
					// the animation look good
					// JJD 2/26/90 - TFS28590
					// Always take the snapshot because we can't always assume the one
					// state will be larger than another. Later we may release
					// the snapshot inside the UpdateTrasform method if the new size is 
					// larger than the old size 
					// since we don't need it anymore
					if (tile._transitionImage == null &&
					    panel.ShouldAnimate)
					    //IsOldStateLarger(newState, oldState))
					    tile.SnapshotImage();

					// resolve the template to pick up any state specific template setting
					tile.ResolveContentTemplate();

					// make sure the ItemInfo object reflects this change
					tm.SyncItemInfo(tile, false);

					// JJD 1/5/10 - TFS25900
					// If we aren't being called from within the panel's ChangeTileState method
					// then we need to call it so everyone's state gets synchronized 
					// properly
					if (tm._settingStateOfTile == null)
					{
						tm.ChangeTileState(tile, newState, oldState);

						// if the state is not what we are expecting (e.g. from an event being cancelled)
						// then just return so we don't raise an erroneous TileStateChanged event below
						if (newState != tile.State)
						{
							tile.SynchIsExpandedFromState();
							return;
						}
					}

					tile.SynchIsExpandedFromState();

					
					// Make sure the resolved visibility props are in sync with the new state
					tile.SetResolvedVisibilityProperties();

					// see if an automation peer was created
					XamTileAutomationPeer peer = FrameworkElementAutomationPeer.FromElement(tile) as XamTileAutomationPeer;

					if (peer != null)
					{
						// get the automation peer for the associated item
						TileItemAutomationPeer itemPeer = peer.EventsSource as TileItemAutomationPeer;

						// verify the expand collapse state
						if (itemPeer != null)
							itemPeer.ItemInfo.VerifyExpandCollapseStateOnPeer();
					}

					// raise the after event
					tm.OnTileStateChanged(new TileStateChangedEventArgs(tile, tm.ItemFromTile(tile), newState, oldState));
				}
			}

			tile.SetContentVisibility();

            tile.UpdateVisualStates();

		}

		/// <summary>
		/// Returns the current state of the tile (read-only)
		/// </summary>
		/// <seealso cref="StateProperty"/>
		public TileState State
		{
			get
			{
				return (TileState)this.GetValue(XamTile.StateProperty);
			}
		}

		#endregion //State

		#endregion //Public Properties	

		#region Internal Properties
		
		#region HasHwndHost

		internal bool HasHwndHost
		{
			get
			{
				HwndHostTracker hhi = HwndHostTracker.GetHwndHost(this);
				return null != hhi && hhi.HasHwndHost;
			}
		}

		#endregion //HasHwndHost

		#region HeaderPresenter

		internal TileHeaderPresenter HeaderPresenter
		{
			get
			{
				if (this._headerPresenter == null)
					return null;

				TileHeaderPresenter thp = CoreUtilities.GetWeakReferenceTargetSafe(this._headerPresenter) as TileHeaderPresenter;

				if (thp != null &&
					PresentationUtilities.GetTemplatedParent( thp ) != this)
				{
					this._headerPresenter = null;
					thp = null;
				}

				return thp;
			}
		}

		#endregion //HeaderPresenter
    
		#region Manager

		internal XamTileManager Manager { get { return _owningManager; } }

		#endregion //Manager

		#region IsExpandedWhenMinimizedResolved

		internal bool IsExpandedWhenMinimizedResolved
		{
			get
			{
				return TileUtilities.GetIsExpandedWhenMinimizedHelper(this._owningManager, this, null, this.IsExpandedWhenMinimized);
			}
		}


		#endregion //IsExpandedWhenMinimizedResolved	
		
		#region IsGenerated

		internal bool IsGenerated { get { return _isGenerated; } }

		#endregion //IsGenerated	
    
		#region Panel

		internal TileAreaPanel Panel { get { return _owningManager != null ? _owningManager.Panel : null; } }

		#endregion //Panel	

		#endregion //Internal Properties	
        
		#endregion //Properties

		#region Methods

		#region Public Methods

		#region ExecuteCommand

		/// <summary>
		/// Executes the specified <see cref="TileCommandType"/>.
		/// </summary>
		/// <param name="command">The Command to execute.</param>
		/// <param name="parameter">An optional parameter.</param>
		/// <param name="sourceElement">The source of the command</param>
		/// <returns>True if command was executed, false if canceled.</returns>
		/// <seealso cref="TileCommandType"/>
		public bool ExecuteCommand(TileCommandType command, object parameter, FrameworkElement sourceElement)
		{
			return this.ExecuteCommandImpl(command, parameter, sourceElement);
		}

		private bool ExecuteCommandImpl(TileCommandType commandType, object parameter, FrameworkElement sourceElement)
		{
			// Make sure the minimal control state exists to execute the command.
			if (this.CanExecuteCommand(commandType, sourceElement) == false)
				return false;

			if (_owningManager != null)
			{
				_owningManager.ExecuteTileCommandImpl(this, commandType, parameter);
				return true;
			}

			return false;
		}

		#endregion //ExecuteCommandImpl

		#endregion //Public Methods	
		
		#region Protected Methods

		#region OnHeaderChanged

		/// <summary>
		/// Called when the <see cref="Header"/> property value changes.
		/// </summary>
		/// <param name="oldHeader">The old header value</param>
		/// <param name="newHeader">The new header value</param>
		protected virtual void OnHeaderChanged(object oldHeader, object newHeader)
		{

			base.RemoveLogicalChild(oldHeader);
			base.AddLogicalChild(newHeader);

		}

		#endregion //OnHeaderChanged
		
		#region VisualState... Methods

        /// <summary>
        /// Called to set the VisualStates of the editor
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected virtual void SetVisualState(bool useTransitions)
        {
            // Set Common state
            if (this.IsEnabled == false)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateDisabled, useTransitions);
            else
            if (this._isMouseOver)
                VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);
            
            // Set drag state
            if (XamTileManager.GetIsDragging(this))
                VisualStateManager.GoToState(this, VisualStateUtilities.StateDragging, useTransitions);
            else
            if (XamTileManager.GetIsSwapTarget(this))
                VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateSwapTarget, VisualStateUtilities.StateNotDragging);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateNotDragging, useTransitions);

            string minstate = null;

            // set minimized state
            switch (this.State)
            {
                case TileState.Maximized:
                    minstate = VisualStateUtilities.StateMaximized;
                    break;
                case TileState.Minimized:
                    minstate = VisualStateUtilities.StateMinimized;
                    break;
                case TileState.MinimizedExpanded:
                    minstate = VisualStateUtilities.StateMinimizedExpanded;
                    break;
                case TileState.Normal:
                    minstate = VisualStateUtilities.StateNotMinimized;
                    break;
            }

            if (minstate != null)
                VisualStateManager.GoToState(this, minstate, useTransitions);

        }

        internal static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamTile tile = target as XamTile;

            if (tile != null)
                tile.UpdateVisualStates();
        }

         /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        internal protected void UpdateVisualStates()
        {
            this.UpdateVisualStates(true);
        }

        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        internal protected void UpdateVisualStates(bool useTransitions)
        {
            if (false == this._hasVisualStateGroups)
                return;


            if (!this.IsLoaded)
                useTransitions = false;





            this.SetVisualState(useTransitions);

            TileHeaderPresenter thp = this.HeaderPresenter;

            if (thp != null)
                thp.UpdateVisualStates(useTransitions);
        }

		#endregion //VisualState... Methods


		#endregion //Protected Methods

		#region Internal Methods

		#region CanExecuteCommand

		internal bool CanExecuteCommand(TileCommandType command, FrameworkElement sourceElement)
		{
			switch (command)
			{
				case TileCommandType.Close:
					return this.CloseActionResolved != TileCloseAction.DoNothing;
				
				case TileCommandType.ToggleMaximized:
					return this.AllowMaximizeResolved;
				
				case TileCommandType.ToggleMinimizedExpansion:
					{
						switch (this.State)
						{
							case TileState.Minimized:
							case TileState.MinimizedExpanded:
								return true;
						}
					}
					break;
			}

			return false;
		}

		#endregion //CanExecuteCommand	

		#region Initialize

		internal void Initialize(XamTileManager owningManager)
		{
			if (_owningManager == owningManager)
				return;

			// unwire the old owner before we set the _owningManager member below
			this.UnwireOwner();

			_owningManager = owningManager;

			if (_owningManager != null)
			{
				// wire the new owner
				this.WireOwner();

				// If the tile is maximized we need to set its State property
				// before we call SyncItemInfo
				if (_isMaximized)
					this.SetValue(StatePropertyKey, TileState.Maximized);

				// Make sure the item info's state is synched up with the tiles
				_owningManager.SyncItemInfo(this, true);
				
				// Call resolve make sure state is set appropriately
				if (!_isMaximized)
					this.ResolveState();

				this.ResolveContentTemplate();
				this.SetResolvedVisibilityProperties();
			}
		}

		#endregion //Initialize	

		#region InitializeHeaderPresenter

		internal void InitializeHeaderPresenter(TileHeaderPresenter headerPreseenter)
		{
			if (headerPreseenter == null)
				this._headerPresenter = null;
			else
				this._headerPresenter = new WeakReference(headerPreseenter);
		}

		#endregion //InitializeHeaderPresenter	

		#region InitializeIsGenerated

		internal void InitializeIsGenerated()
		{
			this._isGenerated = true;
		}

		#endregion //InitializeIsGenerated	
    
		#region OnAnimationEnded

		internal void OnAnimationEnded()
		{
			this.ReleaseSnapshotImage();
		}

		#endregion //OnAnimationEnded	

		#region ResolveState

		internal void ResolveState()
		{
			if (this._isInResolveState)
				return;

			_isInResolveState = true;

			try
			{
				TileState state;

				if (_owningManager == null)
					state = TileState.Normal;
				else
				{
					if (_owningManager.IsInMaximizedMode)
					{
						if (_isMaximized)
							state = TileState.Maximized;
						else
							state = this.IsExpandedWhenMinimizedResolved ? TileState.MinimizedExpanded : TileState.Minimized;
					}
					else
					{
						//Debug.Assert(_isMaximized == false, "Tile state can't be maximized if XamTileManager's IsInMaximizedMode is false");
						state = TileState.Normal;
					}
				}

				this.SetValue(StatePropertyKey, state);
			}
			finally
			{
				_isInResolveState = false;
			}

		}

		#endregion //ResolveState

		#region SetContentVisibility

		internal void SetContentVisibility()
		{
			// set the visibility of the content
			if (this.State == TileState.Minimized && this.ContentTemplateMinimized == null)
				this.SetValue(ContentVisibilityPropertyKey, KnownBoxes.VisibilityCollapsedBox);
			else
				this.ClearValue(ContentVisibilityPropertyKey);
		}

		#endregion //SetContentVisibility

		#region SynchStateFromInfo

		internal void SynchStateFromInfo(ItemTileInfo info)
		{
			if (this._isSynchonizingFromItemInfo)
				return;

			this._isSynchonizingFromItemInfo = true;

			try
			{
			    this.IsClosed = info.IsClosed;
			    this.IsExpandedWhenMinimized = info.IsExpandedWhenMinimized;
				this.IsMaximized = info.IsMaximized;
			    this.ResolveState();

			    this.SetResolvedVisibilityProperties();

			    this.ResolveContentTemplate();
			}
			finally
			{
			    this._isSynchonizingFromItemInfo = false;
			}

		}

		#endregion //SynchStateFromInfo	
            
		#region UpdateTransform

		internal void UpdateTransform(Rect original, Rect current, Rect target, Vector offset, double resizeFactor, bool calledFromArrange)
		{
			//Debug.WriteLine(string.Format("Tile.UpdateTransform - original {0}, current: {1} - target {2}: - offset: {3}, from arrange: {4}, content: {5}", 
			//    original, current, target, offset, calledFromArrange, this.Content));


			if (this._transformGroup == null ||
				(this._translateTransform != null && this._translateTransform.IsSealed) ||
				(this._scaleTransform != null && this._scaleTransform.IsSealed))



			{
				this._transformGroup = new TransformGroup();
				this._translateTransform = new TranslateTransform();
				this._scaleTransform = new ScaleTransform();
				this._transformGroup.Children.Add(this._translateTransform);
				this._transformGroup.Children.Add(this._scaleTransform);

				this.SetValue(RenderTransformProperty, _transformGroup);
			}


			Rect rect = Rect.Offset(current, offset);
			Rect targetRect = Rect.Offset(target, offset);


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


			// JJD 2/11/10 - TFS26775 
			// Check if we have any descendant hwndhosts, if so we don't want to 
			// animate the size so set the rect's size to the targetRect's size

			bool hasHwndHost = this.HasHwndHost;



			if (hasHwndHost)
			{
				rect.Width = targetRect.Width;
				rect.Height = targetRect.Height;
			}

			this._translateTransform.X = rect.X;
			this._translateTransform.Y = rect.Y;

			double scaleX = rect.Width / Math.Max(targetRect.Width, 1);
			double scaleY = rect.Height / Math.Max(targetRect.Height, 1);

			
			// Determine the scale factor to use in both dimensions
			double scaleFactorToUse;

			if (TileUtilities.AreClose(scaleX, 1) ||
				TileUtilities.AreClose(scaleY, 1))
				scaleFactorToUse = 1;
			else
				scaleFactorToUse = Math.Min(scaleX, scaleY);

			if (this._transitionImage != null )
			{
				// JJD 2/23/10 - TFS27400
				// If the rect is the same as the target rect and we are being called
				// from arrange then release the snapshot image since there will be no
				// need for an animation and our OnAnimationEnded method may never be called.
				if (calledFromArrange == true)
				{
					// JJD 11//8/11 - TFS29248/TFS95683 
					// Call ShouldRetainSnapshot method. If it returns false then release the snapshot image
					//if (rect == targetRect )
					if (!this.ShouldRetainSnapshot(rect, targetRect))
					{
						this.ReleaseSnapshotImage();
					}
				}
				else
					if (calledFromArrange == false &&
						rect.Width <= 1.1 * targetRect.Width &&
						rect.Height <= 1.1 * targetRect.Height &&
						this.Panel.IsAnimationInProgress)
						this.ReleaseSnapshotImage();
				
				
				
				if (this._transitionImage != null)
				{
					
					// Determine the larger of the 2 factors for use in both dimensions
					
					
					double snapshotScaleX = rect.Width / Math.Max((original.Width * scaleX), 1);
					double snapshotScaleY = rect.Height / Math.Max((original.Height * scaleY), 1);
					double snapshotScaleFactor = Math.Max(snapshotScaleX, snapshotScaleY);

					this._transitionImageScaleTransform.ScaleX = snapshotScaleFactor;
					this._transitionImageScaleTransform.ScaleY = snapshotScaleFactor;

					
					// In the case where the new state is minimized and we aren't going to show content
					// animate the opacity quickly to zero to get rid of the old content's image before it gets
					// to the end of the animation. This avoids some potentially ugly results when the
					// tile is stretched vertically
					if (calledFromArrange == false && this.State == TileState.Minimized && this.ContentTemplate == null)
						this._transitionImage.Opacity = Math.Min(1.0d, Math.Max(1.0 - (resizeFactor * 2), 0d));
				}
			}

			
			// Use the same scale factor for both dimensions
			
			
			this._scaleTransform.ScaleX = scaleFactorToUse;
			this._scaleTransform.ScaleY = scaleFactorToUse;

			this._scaleTransform.CenterX = rect.X;
			this._scaleTransform.CenterY = rect.Y;

			// JJD 2/11/10 - TFS26775 
			// If we aren't called from arrange and we have an hwnd host we need to trigger an
			// UpdateLayout (since HwndHost wires this event to know when to re-position its hwnd).
			// We can do this by invalidating our arrange and then calling UpdateLayout to process
			// it synchronously.
			if (hasHwndHost && !calledFromArrange)
			{
				this.InvalidateArrange();
				this.UpdateLayout();
			}
		}

		#endregion //UpdateTransform

		#region UnwireOwner

		internal void UnwireOwner()
		{
			if (_owningManager != null && true == _isOnwerWired)
			{
				_owningManager.PropChangeListeners.Remove(this);
				_isOnwerWired = false;
			}
		}

		#endregion //UnwireOwner	
    
		#region WireOwner

		internal void WireOwner()
		{
			if (_owningManager != null && false == _isOnwerWired)
			{
				_owningManager.PropChangeListeners.Add(this, true);
				_isOnwerWired = true;
			}
		}

		#endregion //WireOwner	
    
		#endregion //Internal Methods	
		
		#region Private Methods

		#region OnContentTemplateChanged


		private static void OnContentTemplateChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamTile tile = target as XamTile;

			if (tile != null)
				tile.ResolveContentTemplate();
		}







		#endregion //OnContentTemplateChanged	
    		
		#region ReleaseSnapshotImage

		private void ReleaseSnapshotImage()
		{
			
			if (this._transitionImage == null)
			    return;

			if (this._transitionCanvas != null)
			{
				this._transitionCanvas.Children.Remove(_transitionImage);
				_transitionCanvas.SetValue(VisibilityProperty, KnownBoxes.VisibilityCollapsedBox);
			}

			this._transitionImage.Source = null;
			this._transitionImageScaleTransform = null;
			this._transitionImage = null;
		}

		#endregion //ReleaseSnapshotImage	

		#region SetResolvedVisibilityProperties

		private void SetResolvedVisibilityProperties()
		{
			this.SetContentVisibility();
    
			// JJD 1/6/12 - TFS98921
			// Set the AllowMaximizeResolved dependency property
			#region AllowMaximizeResolved

			bool? allow = this.AllowMaximize;

			if (!allow.HasValue)
			{
				if (_owningManager != null && _owningManager.MaximizedTileLimit > 0)
					allow = true;
				else
					allow = false;
			}

			this.SetValue(XamTile.AllowMaximizeResolvedPropertyKey, KnownBoxes.FromValue(allow.Value));

			#endregion //AllowMaximizeResolved	
    			
			// JJD 1/6/12 - TFS98921
			// Set the CloseActionResolved dependency property 
			// NOte: this must be set before CloseButtonVisibility below
			#region CloseActionResolved

			TileCloseAction action = this.CloseAction;

			if (action == TileCloseAction.Default)
			{
				if (_owningManager != null)
					action = _owningManager.TileCloseAction;

				if (action == TileCloseAction.Default)
					action = TileCloseAction.DoNothing;
			}

			this.SetValue(XamTile.CloseActionResolvedPropertyKey, action);

			#endregion //CloseActionResolved

			#region CloseButtonVisibility

			Visibility? vis = this.CloseButtonVisibility;

			if (!vis.HasValue)
			{
				if (this.CloseActionResolved == TileCloseAction.DoNothing)
					vis = Visibility.Collapsed;
				else
					vis = Visibility.Visible;
			}

			this.SetValue(CloseButtonVisibilityResolvedPropertyKey, KnownBoxes.FromValue(vis.Value));

			#endregion //CloseButtonVisibility	
    
			#region ExpandButtonVisibility

			switch (this.State)
			{
				case TileState.Maximized:
				case TileState.Normal:
					
					// Always set the ExpandButtonVisibilityResolved property to 
					// 'Collapsed' when  not minimized
					vis = Visibility.Collapsed;
					break;
				default:
					{
						vis = this.ExpandButtonVisibility;

						if (!vis.HasValue)
						{
							if (this._owningManager != null)
								vis = this._owningManager.MaximizedModeSettingsSafe.MinimizedTileExpandButtonVisibility;
							else
								vis = Visibility.Visible;
						}
						break;
					}
			}

			this.SetValue(ExpandButtonVisibilityResolvedPropertyKey, vis.Value);

			#endregion //ExpandButtonVisibility	
    
			#region MaximizeButtonVisibility

			vis = this.MaximizeButtonVisibility;

			if (!vis.HasValue)
			{

				if (_owningManager != null && _owningManager.MaximizedTileLimit == 0)
					vis = Visibility.Collapsed;
				else
					vis = Visibility.Visible;
			}

			this.SetValue(MaximizeButtonVisibilityResolvedPropertyKey, KnownBoxes.FromValue(vis.Value));

			#endregion //MaximizeButtonVisibility	
		}

		#endregion //SetResolvedVisibilityProperties	

		// JJD 11//8/11 - TFS29248/TFS95683 - added
		#region ShouldRetainSnapshot

		private bool ShouldRetainSnapshot(Rect rect, Rect targetRect)
		{
			
			
			
			// JJD 11//8/11 - TFS29248/TFS95683 - added
			// if the rects are equal then we don't need the snapshot
			if (rect == targetRect)
			    return false;

			if (this.State == TileState.Maximized)
			{
				// if we are going to the maximized state and the width or
				// height isn't growing then don't use a snapshot image.
				// Otherwise font's can get distorted
				if (targetRect.Width < rect.Width ||
					targetRect.Height < rect.Height)
					return false;
			}
			else
			{
				// if we are going from the maximized state and the width or
				// height isn't shrinking then don't use a snapshot image.
				// Otherwise font's can get distorted
				if (this._wasMaximized)
				{
					if (targetRect.Width > rect.Width ||
						targetRect.Height > rect.Height)
						return false;
				}
			}

			return true;
		}

		#endregion //ShouldRetainSnapshot	
            
		#region SnapshotImage

		internal void SnapshotImage()
		{
			this.ReleaseSnapshotImage();

			if (_transitionCanvas == null)
				return;

			Size size = new Size(this.ActualWidth, this.ActualHeight);

			TileAreaPanel panel = this.Panel;

			// JJD 11/23/10 - TFS60272
			// Make sure not to create an image greater than twice the size of the _panel
			if (panel != null)
			{
				Size szTC = new Size(panel.ActualWidth, panel.ActualHeight);

				if (szTC.Width > 0)
					size.Width = Math.Min(size.Width, szTC.Width * 2);

				if (szTC.Height > 0)
					size.Height = Math.Min(size.Height, szTC.Height * 2);
			}

			// If the image is > than 40mb then don't create it
			if (size.Width < 1 || size.Height < 1 || size.Width * size.Height > 40000000)
				return;


			// Check if we have any descendant hwnd hosts, if so we don't want to 
			// do any animations
			if (this.HasHwndHost)
				return;

			RenderTargetBitmap bmp = new RenderTargetBitmap((int)(size.Width),
				(int)(size.Height),
				96, 96, PixelFormats.Default);

			if (VisualTreeHelper.GetChildrenCount(this) > 0)
				bmp.Render((Visual)VisualTreeHelper.GetChild(this, 0));
			else
				bmp.Render(this);

			bmp.Freeze();







			_transitionImage = new Image();
			_transitionImageScaleTransform = new ScaleTransform();
			_transitionImage.RenderTransform = this._transitionImageScaleTransform;
			Canvas.SetLeft(_transitionImage, 0);
			Canvas.SetTop(_transitionImage, 0);
			_transitionImage.Width = size.Width;
			_transitionImage.Height = size.Height;
			_transitionImage.Opacity = 1;
			_transitionImage.Stretch = Stretch.None;
			_transitionImage.Source = bmp;

			_transitionCanvas.Children.Add(_transitionImage);

			_transitionImage.InvalidateMeasure();
			_transitionImage.InvalidateArrange();

			_transitionCanvas.SetValue(VisibilityProperty, KnownBoxes.VisibilityVisibleBox);
		}

		#endregion //SnapshotImage	

		#region SynchIsExpandedFromState

		private void SynchIsExpandedFromState()
		{

			// If the state matches the default then set the
			// IsExpandedWhenMinimized  property to null
			if (this._owningManager != null)
			{
				switch (this.State)
				{
					case TileState.Minimized:
						if (this._owningManager.IsExpandedWhenMinimizedDefault)
							this.IsExpandedWhenMinimized = false;
						else
							this.IsExpandedWhenMinimized = null;
						break;
					case TileState.MinimizedExpanded:
						if (this._owningManager.IsExpandedWhenMinimizedDefault)
							this.IsExpandedWhenMinimized = null;
						else
							this.IsExpandedWhenMinimized = true;
						break;
				}
			}
		}

		#endregion //SynchIsExpandedFromState	
    
		#endregion //Private Methods	
    
		#endregion //Methods

		#region ITypedPropertyChangeListener<object,string> Members

		void ITypedPropertyChangeListener<object, string>.OnPropertyValueChanged(object dataItem, string property, object extraInfo)
		{
			if (property == "LayoutVersion")
			{
				this.ResolveState();
				this.ResolveContentTemplate();
				this.SetResolvedVisibilityProperties();
				this.SynchIsExpandedFromState();
			}
		}

		#endregion
		
		#region HwndHostTracker internal class


		internal class HwndHostTracker
		{
			private XamTile _owner;
			private WeakList<HwndHost> _hosts;

			internal HwndHostTracker(XamTile tile)
			{
				this._owner = tile;
				this._hosts = new WeakList<HwndHost>();
			}
			#region Properties

			#region HasHwndHost
			internal bool HasHwndHost
			{
				get
				{
					bool hasHwndHost = false;

					if (this._hosts.Count > 0)
						hasHwndHost = true;

					return hasHwndHost;
				}
			}

			#endregion //HasHwndHost

			// We need to maintain a list of the hwndhosts within the pane. When we have one 
			// or more then the content needs to be hidden when the pane is unpinned and not 
			// displayed within the flyout.
			//
			#region HwndHost
			internal static readonly DependencyProperty HwndHostProperty
				= DependencyProperty.RegisterAttached("HwndHost", typeof(HwndHostTracker), typeof(HwndHostTracker),
					new FrameworkPropertyMetadata((HwndHostTracker)null,
						FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior |
						FrameworkPropertyMetadataOptions.Inherits,
						new PropertyChangedCallback(OnHwndHostChanged)));

			internal static HwndHostTracker GetHwndHost(DependencyObject d)
			{
				return (HwndHostTracker)d.GetValue(HwndHostProperty);
			}

			internal static void SetHwndHost(DependencyObject d, HwndHostTracker value)
			{
				d.SetValue(HwndHostProperty, value);
			}

			/// <summary>
			/// Handles changes to the HwndHost property.
			/// </summary>
			private static void OnHwndHostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
			{
				HwndHost host = d as HwndHost;

				if (null != host)
				{
					HwndHostTracker oldInfo = e.OldValue as HwndHostTracker;
					HwndHostTracker newInfo = e.NewValue as HwndHostTracker;

					if (null != oldInfo)
						oldInfo.Remove(host);

					if (null != newInfo)
						newInfo.Add(host);
				}

			}
			#endregion //HwndHost


			#endregion //Properties


			#region Methods

			#region Add/Remove(HwndHost)
			internal void Add(HwndHost host)
			{
				this._hosts.Add(host);
			}

			internal void Remove(HwndHost host)
			{
				this._hosts.Remove(host);
				this._hosts.Compact();
			}

			#endregion //Add/Remove(HwndHost)

			#region GetHwndHosts

			internal IEnumerable<HwndHost> GetHwndHosts()
			{
				return this._hosts;
			}

			#endregion //GetHwndHosts

			#endregion //Methods
		}

		#endregion //HwndHostTracker internal class

		#region ICommandTarget Members

		object ICommandTarget.GetParameter(CommandSource source)
		{
			return source is TileCommandSource ? this : null;
		}

		bool ICommandTarget.SupportsCommand(ICommand command)
		{
			return command is TileCommand;
		}

		#endregion

		#region IResizableElement Members

		object IResizableElement.ResizeContext
		{
			get { return this; }
		}

		#endregion
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