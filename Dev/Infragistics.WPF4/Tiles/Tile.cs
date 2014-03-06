using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Markup;
using System.Diagnostics;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Shapes;

using Infragistics.Windows.Selection;
using Infragistics.Windows.Licensing;
using Infragistics.Windows.Controls.Events;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Commands;
using Infragistics.Windows.Virtualization;
using System.Xml;
using Infragistics.Controls.Layouts.Primitives;
using Infragistics.Windows.Resizing;
using Infragistics.Windows.Internal;
using Infragistics.Windows.Tiles.Events;
using Infragistics.Windows.Themes;
using System.Windows.Media.Imaging;
using Infragistics.Windows.Automation.Peers.Tiles;
using System.Windows.Automation.Peers;
using System.Windows.Interop;
using Infragistics.Collections;

namespace Infragistics.Windows.Tiles
{
    #region Tile class

    /// <summary>
    /// A <see cref="System.Windows.Controls.ContentControl"/> derived element that represents a tile inside a <see cref="XamTilesControl"/>.
    /// </summary>
    /// <remarks>
    /// <para class="note"><b>Note:</b> even though this class derives from ContentControl it exposes all of the properties and functionality of a <see cref="HeaderedContentControl"/>. 
    /// The reason the class doesn't derive from HeaderedContentControl is because the base PrepareItemContainer logic in ItemsControl assumes that 
    /// the item in the Items collection should be set as its Header, likewise the <see cref="ItemsControl.ItemTemplate"/> property should be set as the 
    /// HeaderTemplate. For tiles this is not the intent, instead the item should be the Tile's Content and the
    /// <b>ItemsControl.ItemTemplate</b> property should be used to initialize the ContentTemplate property.</para>
    /// </remarks>
    //[Description("A ContentControl derived element that represents a tile inside a XamTilesControl.")]
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]

    // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateNormal,               GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateMouseOver,            GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateDisabled,             GroupName = VisualStateUtilities.GroupCommon)]

    [TemplateVisualState(Name = VisualStateUtilities.StateDragging,             GroupName = VisualStateUtilities.GroupDrag)]
    [TemplateVisualState(Name = VisualStateUtilities.StateNotDragging,          GroupName = VisualStateUtilities.GroupDrag)]
    [TemplateVisualState(Name = VisualStateUtilities.StateSwapTarget,           GroupName = VisualStateUtilities.GroupDrag)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateMaximized,            GroupName = VisualStateUtilities.GroupMinimized)]
    [TemplateVisualState(Name = VisualStateUtilities.StateMinimized,            GroupName = VisualStateUtilities.GroupMinimized)]
    [TemplateVisualState(Name = VisualStateUtilities.StateMinimizedExpanded,    GroupName = VisualStateUtilities.GroupMinimized)]
    [TemplateVisualState(Name = VisualStateUtilities.StateNotMinimized,         GroupName = VisualStateUtilities.GroupMinimized)]


    [DesignTimeVisible(false)] // JJD 06/04/10 - TFS32695 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
    public class Tile : ContentControl, IResizableElement
    {
        #region Private Members

        private XamTilesControl         _tilesControl;
        private TilesPanel              _panel;
        private TransformGroup          _transformGroup;
        private TranslateTransform      _translateTransform;
        private ScaleTransform          _scaleTransform;
        private PropertyValueTracker    _tracker;
        private TileConstraints         _constraints;
        private TileConstraints         _constraintsMaximized;
        private TileConstraints         _constraintsMinimized;
        private TileConstraints         _constraintsMinimizedExpanded;
        private WeakReference           _headerPresenter;
        private ImageBrush              _snapshotImageBrush;
        private Rectangle               _snapshotImage;
        private ScaleTransform          _snapshotImageScaleTransform;
        private bool                    _isSynchonizingFromItemInfo;
        private bool                    _isInCoerceState;

		// JJD 11//8/11 - TFS29248/TFS95683 - added
		private bool					_wasMaximized;


        // JJD 4/23/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;

 
        #endregion //Private Members

        #region Constants

        internal static readonly object NormalBox               = TileState.Normal;
        internal static readonly object MaximizedBox            = TileState.Maximized;
        internal static readonly object MinimizedBox            = TileState.Minimized;
        internal static readonly object MinimizedExpandedBox    = TileState.MinimizedExpanded;

        #endregion //Constants	
    
        #region Constructor

        static Tile()
        {
            if (XamTilesControl.s_HeaderStringFormatProperty != null)
                s_HeaderStringFormatProperty = XamTilesControl.s_HeaderStringFormatProperty.AddOwner(typeof(Tile));
            else
                s_HeaderStringFormatProperty = DependencyProperty.Register("HeaderStringFormat",
                    typeof(string), typeof(Tile), new FrameworkPropertyMetadata(null));

            CommandBinding commandClose = new CommandBinding(CloseCommand,
                Tile.ExecuteCommandHandler, Tile.CanExecuteCommandHandler);
            CommandManager.RegisterClassCommandBinding(typeof(Tile), commandClose);

            CommandBinding commandToggleMaximized = new CommandBinding(ToggleMaximizedCommand,
                Tile.ExecuteCommandHandler, Tile.CanExecuteCommandHandler);
            CommandManager.RegisterClassCommandBinding(typeof(Tile), commandToggleMaximized);

            CommandBinding commandToggleMinimizedExpansion = new CommandBinding(ToggleMinimizedExpansionCommand,
                Tile.ExecuteCommandHandler, Tile.CanExecuteCommandHandler);
            CommandManager.RegisterClassCommandBinding(typeof(Tile), commandToggleMinimizedExpansion);

            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(Tile), new FrameworkPropertyMetadata(typeof(Tile)));

            ContentControl.ContentTemplateProperty.OverrideMetadata(typeof(Tile), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceContentTemplate)));
            FrameworkElement.RenderTransformProperty.OverrideMetadata(typeof(Tile), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnRenderTransformChanged), new CoerceValueCallback(CoerceRenderTransform)));
            FrameworkElement.VisibilityProperty.OverrideMetadata(typeof(Tile), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceVisibility)));

            // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
            UIElement.IsEnabledProperty.OverrideMetadata(typeof(Tile), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)));

        }

        /// <summary>
        /// Instantiates a new instance of <see cref="Tile"/>
        /// </summary>
        public Tile()
        {

            // JJD 2/11/10 - TFS26775
            // Added tarcker for descendaant hwndhosts
            HwndHostTracker.SetHwndHost(this, new HwndHostTracker(this));
        }

        #endregion //Constructor	

        #region Commands

        /// <summary>
        /// Command used to close the Tile.
        /// </summary>
        static public readonly RoutedUICommand CloseCommand = new RoutedUICommand("Close", "Close", typeof(Tile));

        /// <summary>
        /// Command used to toggle the <see cref="State"/> of a Tile to and from 'Maximized'.
        /// </summary>
        static public readonly RoutedUICommand ToggleMaximizedCommand = new RoutedUICommand("ToggleMaximized", "ToggleMaximized", typeof(Tile));

        /// <summary>
        /// Command used to toggle the <see cref="State"/> of a Tile to and from 'MinimizedExpanded'.
        /// </summary>
        static public readonly RoutedUICommand ToggleMinimizedExpansionCommand = new RoutedUICommand("ToggleMinimizedExpansion", "ToggleMinimizedExpansion", typeof(Tile));

        #endregion //Commands	
    
        #region CommandHandlers

			#region CanExecuteCommandHandler

		private static void CanExecuteCommandHandler(object sender, CanExecuteRoutedEventArgs e)
		{
            Tile tile = sender as Tile;

			e.CanExecute = false;

			if (tile != null)
			{
                if (e.Command == CloseCommand)
                {
                    e.CanExecute = tile.CloseActionResolved != TileCloseAction.DoNothing;
                    e.Handled = true;
                }
                else
                if (e.Command == ToggleMaximizedCommand)
                {
                    e.CanExecute = tile.AllowMaximizeResolved;
                    e.Handled = true;
                }
                else
                if (e.Command == ToggleMinimizedExpansionCommand)
                {
                    switch (tile.State)
                    {
                        case TileState.Minimized:
                        case TileState.MinimizedExpanded:
                            e.CanExecute = true;
                            break;
                        default:
                            e.CanExecute = false;
                            break;
                    }
                    e.Handled = true;
                }
			}
		}

			#endregion //CanExecuteCommandHandler	

			#region ExecuteCommandHandler

		private static void ExecuteCommandHandler(object sender, ExecutedRoutedEventArgs e)
		{
            Tile tile = sender as Tile;
			if (tile != null && tile.Panel != null)
				tile.Panel.ExecuteTileCommandImpl(tile, e.Command as RoutedCommand, e.Parameter, e.Source, e.OriginalSource);
		}

			#endregion //ExecuteCommandHandler

		#endregion //CommandHandlers

        #region Base class overrides

            #region GetVisualChild

        /// <summary>
        /// Gets the visual child at a specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the specific child visual.</param>
        /// <returns>The visual child at the specified index.</returns>
        protected override Visual GetVisualChild(int index)
        {
            if (this._snapshotImage == null ||
                 index < base.VisualChildrenCount)
                return base.GetVisualChild(index);

            return this._snapshotImage;

        }

            #endregion //GetVisualChild	

            #region LogicalChildren
    
        /// <summary>
        /// Gets an enumerator for the tile's logical child elements.
        /// </summary>
        /// <value>An enumerator.</value>
        protected override IEnumerator LogicalChildren
        {
	        get 
	        { 
                object header = this.Header;

                if ( header == null )
                    return base.LogicalChildren;

                return new MultiSourceEnumerator( new SingleItemEnumerator(header), base.LogicalChildren );
	        }
        }

   	        #endregion //LogicalChildren	

            #region OnApplyTemplate

        /// <summary>
        /// Called when the template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();


            // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);


        }

            #endregion //OnApplyTemplate	
    
            #region OnCreateAutomationPeer

        /// <summary>
        /// Returns an automation peer that exposes the <see cref="Tile"/> to UI Automation.
        /// </summary>
        /// <returns>A <see cref="TileAutomationPeer"/></returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new TileAutomationPeer(this);
        }

            #endregion //OnCreateAutomationPeer	

            #region OnInitialized

        /// <summary>
        /// Called after the control has been initialized.
        /// </summary>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            XamTilesControl tilesControl = this.Parent as XamTilesControl;

            // JJD 2/06/10 - TFS27262 
            // if the logical parent is a XamTilesControl call 
            // InitializeTilesControl so we can make sure the maximizedItems
            // collection gets synched up
            if (tilesControl != null)
                this.InitializeTilesControl(tilesControl);

            this.CoerceValue(ContentTemplateProperty);

            this.SetResolvedVisibilityProperties();

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

            // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
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

            // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }

            #endregion //OnMouseLeave	

            #region OnPropertyChanged

        /// <summary>
        /// Called when a property changes.
        /// </summary>
        /// <param name="e">A DependencyPropertyChangedEventArgs instance that contains information about the property that changed.</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);


            DependencyProperty dp = e.Property;

            if (dp == TilesPanelBase.IsDraggingProperty ||
                 dp == TilesPanelBase.IsSwapTargetProperty)
                this.UpdateVisualStates();

        }

            #endregion //OnPropertyChanged	
    
            #region OnVisualParentChanged

        /// <summary>
        /// Called when the visiual parent as changed
        /// </summary>
        /// <param name="oldParent"></param>
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
			// JJD 4/8/11 - TFS71762
			// Don't blindly clear the tracker here since it will be maintained properly inside
			// the InitializePanel method that is called below.
            //this._tracker = null;

            base.OnVisualParentChanged(oldParent);

            this.InitializePanel(VisualTreeHelper.GetParent(this) as TilesPanel);

        }

            #endregion //OnVisualParentChanged	
 
            #region VisualChildrenCount

        /// <summary>
        /// Returns the total numder of visual children (read-only).
        /// </summary>
        protected override int VisualChildrenCount
        {
            get
            {
                int count = base.VisualChildrenCount;

                if (this._snapshotImage != null)
                    count++;

                return count;
            }
        }

            #endregion //VisualChildrenCount

        #endregion //Base class overrides	
    
        #region Events

        #endregion //Events	
   
        #region Properties

            #region Public Properties

                #region AllowMaximize

        /// <summary>
        /// Identifies the <see cref="AllowMaximize"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AllowMaximizeProperty = DependencyProperty.Register("AllowMaximize",
            typeof(bool?), typeof(Tile), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets/sets whether this tile can be maximized by the user.
        /// </summary>
        /// <seealso cref="AllowMaximizeProperty"/>
        //[Description("Gets/sets whether this tile can be maximized by the user.")]
        //[Category("TilesControl Properties")]
        [TypeConverter(typeof(System.Windows.NullableBoolConverter))]
        public bool? AllowMaximize
        {
            get
            {
                return (bool?)this.GetValue(Tile.AllowMaximizeProperty);
            }
            set
            {
                this.SetValue(Tile.AllowMaximizeProperty, value);
            }
        }

                #endregion //AllowMaximize

                #region AllowMaximizeResolved

        /// <summary>
        /// Returns the resolved value as to whether this tile can be maximized.
        /// </summary>
        /// <seealso cref="AllowMaximizeProperty"/>
        /// <seealso cref="MaximizedModeSettings.MinimizedTileExpansionMode"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public bool AllowMaximizeResolved
        {
            get
            {
                bool? allow = this.AllowMaximize;

                if (allow.HasValue)
                    return allow.Value;

                TilesPanel panel = this.Panel;

                if (panel != null)
                    return panel.MaximizedTileLimit > 0;

                return false;
            }
        }

                #endregion //AllowMaximizeResolved

		        #region Column

		/// <summary>
		/// Identifies the Column dependency property.
		/// </summary>
		public static readonly DependencyProperty ColumnProperty = TilesPanel.ColumnProperty.AddOwner(typeof(Tile));

		/// <summary>
		/// Gets/sets the value of the Column, -1 indicates that the tile will be positioned relative to the previous tile in the panel. The default value is 0.
		/// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property is ignored unless <see cref="XamTilesControl"/>.<see cref="XamTilesControl.NormalModeSettings"/>.<see cref="Infragistics.Windows.Tiles.NormalModeSettings.TileLayoutOrder"/> is set to 'UseExplicitRowColumnOnTile'.</para>
        /// </remarks>
        //[Description("Gets/sets the value of the Column, -1 indicates that the tile will be positioned relative to the previous tile in the panel.")]
        //[Category("TilesControl Properties")]
        public int Column
        {
            get
            {
                return (int)this.GetValue(Tile.ColumnProperty);
            }
            set
            {
                this.SetValue(Tile.ColumnProperty, value);
            }
		}

		        #endregion // Column

		        #region ColumnSpan

		/// <summary>
		/// Identifies the ColumnSpan dependency property.
		/// </summary>
		public static readonly DependencyProperty ColumnSpanProperty = TilesPanel.ColumnSpanProperty.AddOwner(typeof(Tile));

		/// <summary>
        /// Gets/sets the value of the ColumnSpan, 0 indicates that the tile will occupy the remainder of the space in its logical column. Default value is 1.
		/// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property is ignored unless <see cref="XamTilesControl"/>.<see cref="XamTilesControl.NormalModeSettings"/>.<see cref="Infragistics.Windows.Tiles.NormalModeSettings.TileLayoutOrder"/> is set to 'UseExplicitRowColumnOnTile'.</para>
        /// </remarks>
        //[Description("Gets/sets the value of the ColumnSpan, 0 indicates that the tile will occupy the remainder of the space in its logical column.")]
        //[Category("TilesControl Properties")]
        public int ColumnSpan
        {
            get
            {
                return (int)this.GetValue(Tile.ColumnSpanProperty);
            }
            set
            {
                this.SetValue(Tile.ColumnSpanProperty, value);
            }
		}

		        #endregion // ColumnSpan

		        #region ColumnWeight

		/// <summary>
		/// Identifies the ColumnWeight dependency property.
		/// </summary>
		public static readonly DependencyProperty ColumnWeightProperty = TilesPanel.ColumnWeightProperty.AddOwner(typeof(Tile));

		/// <summary>
        /// Gets/sets how any extra width will be distributed among tiles. The default value is 1.
		/// </summary>
        //[Description("Gets/sets how any extra width will be distributed among tiles. The default value is 1.")]
        //[Category("TilesControl Properties")]
        public float ColumnWeight
        {
            get
            {
                return (float)this.GetValue(Tile.ColumnWeightProperty);
            }
            set
            {
                this.SetValue(Tile.ColumnWeightProperty, value);
            }
		}

		        #endregion // ColumnWeight

                #region Constraints

        /// <summary>
        /// Identifies the <see cref="Constraints"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ConstraintsProperty 
            = TilesPanel.ConstraintsProperty.AddOwner(typeof(Tile), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnConstraintsChanged)));

        private static void OnConstraintsChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            Tile tile = target as Tile;

            // unwire the property changed of the old constraints
            if (tile._constraints != null)
                tile._constraints.PropertyChanged -= new PropertyChangedEventHandler(tile.OnConstraintsPropertyChanged);

            tile._constraints = e.NewValue as TileConstraints;

            // wire up the property changed of the new constraints
            if (tile._constraints != null)
                tile._constraints.PropertyChanged += new PropertyChangedEventHandler(tile.OnConstraintsPropertyChanged);

            tile.ProcessConstraintsChanged();
        }


        private void OnConstraintsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.ProcessConstraintsChanged();
        }

        /// <summary>
        /// Gets/sets the size constraints for this tile when its <see cref="State"/> is 'Normal'.
        /// </summary>
        /// <seealso cref="ConstraintsProperty"/>
        //[Description("Gets/sets the size constraints for this tile when its State is 'Normal'.")]
        //[Category("TilesControl Properties")]
        public TileConstraints Constraints
        {
            get
            {
                return this._constraints;
            }
            set
            {
                this.SetValue(Tile.ConstraintsProperty, value);
            }
        }

                #endregion //Constraints

                #region ConstraintsMaximized

        /// <summary>
        /// Identifies the <see cref="ConstraintsMaximized"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ConstraintsMaximizedProperty 
            = TilesPanel.ConstraintsMaximizedProperty.AddOwner(typeof(Tile), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnConstraintsMaximizedChanged)));

        private static void OnConstraintsMaximizedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            Tile tile = target as Tile;

            // unwire the property changed of the old constraints
            if (tile._constraintsMaximized != null)
                tile._constraintsMaximized.PropertyChanged -= new PropertyChangedEventHandler(tile.OnConstraintsPropertyChanged);

            tile._constraintsMaximized = e.NewValue as TileConstraints;

            // wire up the property changed of the new constraints
            if (tile._constraintsMaximized != null)
                tile._constraintsMaximized.PropertyChanged += new PropertyChangedEventHandler(tile.OnConstraintsPropertyChanged);

            tile.ProcessConstraintsChanged();
        }

        /// <summary>
        /// Gets/sets the size constraints for this tile when its <see cref="State"/> is 'Maximized'.
        /// </summary>
        /// <seealso cref="ConstraintsMaximizedProperty"/>
        //[Description("Gets/sets the size constraints for this tile when its State is 'Maximized'.")]
        //[Category("TilesControl Properties")]
        public TileConstraints ConstraintsMaximized
        {
            get
            {
                return this._constraintsMaximized;
            }
            set
            {
                this.SetValue(Tile.ConstraintsMaximizedProperty, value);
            }
        }

                #endregion //ConstraintsMaximized

                #region ConstraintsMinimized

        /// <summary>
        /// Identifies the <see cref="ConstraintsMinimized"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ConstraintsMinimizedProperty 
            = TilesPanel.ConstraintsMinimizedProperty.AddOwner(typeof(Tile), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnConstraintsMinimizedChanged)));

        private static void OnConstraintsMinimizedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            Tile tile = target as Tile;

            // unwire the property changed of the old constraints
            if (tile._constraintsMinimized != null)
                tile._constraintsMinimized.PropertyChanged -= new PropertyChangedEventHandler(tile.OnConstraintsPropertyChanged);

            tile._constraintsMinimized = e.NewValue as TileConstraints;
 
            // wire up the property changed of the new constraints
            if (tile._constraintsMinimized != null)
                tile._constraintsMinimized.PropertyChanged += new PropertyChangedEventHandler(tile.OnConstraintsPropertyChanged);

            tile.ProcessConstraintsChanged();
        }

        /// <summary>
        /// Gets/sets the size constraints for this tile when its <see cref="State"/> is 'Minimized'.
        /// </summary>
        /// <seealso cref="ConstraintsMinimizedProperty"/>
        //[Description("Gets/sets the size constraints for this tile when its State is 'Minimized'.")]
        //[Category("TilesControl Properties")]
        public TileConstraints ConstraintsMinimized
        {
            get
            {
                return this._constraintsMinimized;
            }
            set
            {
                this.SetValue(Tile.ConstraintsMinimizedProperty, value);
            }
        }

                #endregion //ConstraintsMinimized

                #region ConstraintsMinimizedExpanded

        /// <summary>
        /// Identifies the <see cref="ConstraintsMinimizedExpanded"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ConstraintsMinimizedExpandedProperty 
            = TilesPanel.ConstraintsMinimizedExpandedProperty.AddOwner(typeof(Tile), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnConstraintsMinimizedExpandedChanged)));

        private static void OnConstraintsMinimizedExpandedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            Tile tile = target as Tile;

            // unwire the property changed of the old constraints
            if (tile._constraintsMinimizedExpanded != null)
                tile._constraintsMinimizedExpanded.PropertyChanged -= new PropertyChangedEventHandler(tile.OnConstraintsPropertyChanged);

            tile._constraintsMinimizedExpanded = e.NewValue as TileConstraints;
 
            // wire up the property changed of the new constraints
            if (tile._constraintsMinimizedExpanded != null)
                tile._constraintsMinimizedExpanded.PropertyChanged += new PropertyChangedEventHandler(tile.OnConstraintsPropertyChanged);

            tile.ProcessConstraintsChanged();
        }

        /// <summary>
        /// Gets/sets the size constraints for this tile when its <see cref="State"/> is 'MinimizedExpanded'.
        /// </summary>
        /// <seealso cref="ConstraintsMinimizedExpandedProperty"/>
        //[Description("Gets/sets the size constraints for this tile when its State is 'MinimizedExpanded'.")]
        //[Category("TilesControl Properties")]
        public TileConstraints ConstraintsMinimizedExpanded
        {
            get
            {
                return this._constraintsMinimizedExpanded;
            }
            set
            {
                this.SetValue(Tile.ConstraintsMinimizedExpandedProperty, value);
            }
        }

                #endregion //ConstraintsMinimizedExpanded

                #region ContentTemplateMaximized

        /// <summary>
        /// Identifies the <see cref="ContentTemplateMaximized"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ContentTemplateMaximizedProperty = DependencyProperty.Register("ContentTemplateMaximized",
            typeof(DataTemplate), typeof(Tile), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnContentTemplateMaximizedChanged)));

        private static void OnContentTemplateMaximizedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            Tile tile = target as Tile;

            if (tile.IsLoaded && tile.State == TileState.Maximized)
                tile.CoerceValue(ContentTemplateProperty);
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
                return (DataTemplate)this.GetValue(Tile.ContentTemplateMaximizedProperty);
            }
            set
            {
                this.SetValue(Tile.ContentTemplateMaximizedProperty, value);
            }
        }

                #endregion //ContentTemplateMaximized

                #region ContentTemplateMinimized

        /// <summary>
        /// Identifies the <see cref="ContentTemplateMinimized"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ContentTemplateMinimizedProperty = DependencyProperty.Register("ContentTemplateMinimized",
            typeof(DataTemplate), typeof(Tile), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnContentTemplateMinimizedChanged)));

        private static void OnContentTemplateMinimizedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            Tile tile = target as Tile;

            if (tile.IsLoaded && tile.State == TileState.Minimized)
                tile.CoerceValue(ContentTemplateProperty);
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
                return (DataTemplate)this.GetValue(Tile.ContentTemplateMinimizedProperty);
            }
            set
            {
                this.SetValue(Tile.ContentTemplateMinimizedProperty, value);
            }
        }

                #endregion //ContentTemplateMinimized

                #region ContentTemplateMinimizedExpanded

        /// <summary>
        /// Identifies the <see cref="ContentTemplateMinimizedExpanded"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ContentTemplateMinimizedExpandedProperty = DependencyProperty.Register("ContentTemplateMinimizedExpanded",
            typeof(DataTemplate), typeof(Tile), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnContentTemplateMinimizedExpandedChanged)));

        private static void OnContentTemplateMinimizedExpandedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            Tile tile = target as Tile;

            if (tile.IsLoaded && tile.State == TileState.MinimizedExpanded)
                tile.CoerceValue(ContentTemplateProperty);
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
                return (DataTemplate)this.GetValue(Tile.ContentTemplateMinimizedExpandedProperty);
            }
            set
            {
                this.SetValue(Tile.ContentTemplateMinimizedExpandedProperty, value);
            }
        }

                #endregion //ContentTemplateMinimizedExpanded

                #region ContentVisibility

        private static readonly DependencyPropertyKey ContentVisibilityPropertyKey =
            DependencyProperty.RegisterReadOnly("ContentVisibility",
            typeof(Visibility), typeof(Tile), new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox));

        /// <summary>
        /// Identifies the <see cref="ContentVisibility"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ContentVisibilityProperty =
            ContentVisibilityPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the visibility of the content (read-only)
        /// </summary>
        /// <value>Returns 'Visible' unless the <see cref="State"/> is 'Minimized' and a tempate for this state has not been provided thru the <see cref="ContentTemplateMinimized"/> property.</value>
        /// <remarks>
        /// <para class="note"><b>Note:</b> This property is used for binding to the ContentPesenter's Visibility inside the tile's template.</para>
        /// </remarks>
        /// <seealso cref="ContentTemplateMinimized"/>
        /// <seealso cref="XamTilesControl.ItemTemplateMinimized"/>
        /// <seealso cref="ContentVisibilityProperty"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Bindable(true)]
        public Visibility ContentVisibility
        {
            get
            {
                return (Visibility)this.GetValue(Tile.ContentVisibilityProperty);
            }
        }

                #endregion //ContentVisibility

                #region CloseAction

        /// <summary>
        /// Identifies the <see cref="CloseAction"/> dependency property
        /// </summary>
        public static readonly DependencyProperty CloseActionProperty = DependencyProperty.Register("CloseAction",
            
            
            typeof(TileCloseAction), typeof(Tile), new FrameworkPropertyMetadata(TileCloseAction.Default, new PropertyChangedCallback(OnCloseActionChanged)));

        private static void OnCloseActionChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            Tile tile = target as Tile;

            // JJD 2/22/10 - TFS27935
            // Set the resolved visibility properties
            if (tile.IsInitialized)
                tile.SetResolvedVisibilityProperties();
        }

        /// <summary>
        /// Gets/sets what happens when this tile is closed.
        /// </summary>
        /// <seealso cref="CloseActionProperty"/>
        /// <seealso cref="XamTilesControl.TileCloseAction"/>
        /// <seealso cref="CloseButtonVisibility"/>
        /// <seealso cref="CloseButtonVisibilityResolved"/>
        //[Description("Gets/sets what happens when this tile is closed.")]
        //[Category("TilesControl Properties")]
        
        
        
        public TileCloseAction CloseAction
        {
            get
            {
                return (TileCloseAction)this.GetValue(Tile.CloseActionProperty);
            }
            set
            {
                this.SetValue(Tile.CloseActionProperty, value);
            }
        }

                #endregion //CloseAction

                #region CloseActionResolved

        /// <summary>
        /// Returns the resolved value as to what happens when this tile is closed.
        /// </summary>
        /// <seealso cref="CloseActionProperty"/>
        /// <seealso cref="XamTilesControl.TileCloseAction"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public TileCloseAction CloseActionResolved
        {
            get
            {

                TileCloseAction action = this.CloseAction;

                if (action != TileCloseAction.Default)
                    return action;

                TilesPanel panel = this.Panel;

                if (panel == null)
                    return TileCloseAction.DoNothing;

                action = panel.TileCloseAction;

                if (action != TileCloseAction.Default)
                    return action;

                return TileCloseAction.DoNothing;
            }
        }

                #endregion //CloseActionResolved

                #region CloseButtonVisibility

        /// <summary>
        /// Identifies the <see cref="CloseButtonVisibility"/> dependency property
        /// </summary>
        public static readonly DependencyProperty CloseButtonVisibilityProperty = DependencyProperty.Register("CloseButtonVisibility",
            typeof(Visibility?), typeof(Tile), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnCloseButtonVisibilityChanged)));

        private static void OnCloseButtonVisibilityChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            Tile tile = target as Tile;

            // JJD 2/22/10 - TFS27935
            // Set the resolved visibility properties
            if (tile.IsInitialized)
                tile.SetResolvedVisibilityProperties();
        }

        /// <summary>
        /// Gets/sets the visibility of the close button in the header area
        /// </summary>
        /// <seealso cref="XamTilesControl.TileCloseAction"/>
        /// <seealso cref="CloseAction"/>
        /// <seealso cref="CloseButtonVisibilityProperty"/>
        /// <seealso cref="CloseButtonVisibilityResolved"/>
        //[Description("Gets/sets the visibility of the close button in the header area")]
        //[Category("TilesControl Properties")]
        [TypeConverter(typeof(NullableConverter<Visibility>))]
        public Visibility? CloseButtonVisibility
        {
            get
            {
                return (Visibility?)this.GetValue(Tile.CloseButtonVisibilityProperty);
            }
            set
            {
                this.SetValue(Tile.CloseButtonVisibilityProperty, value);
            }
        }

                #endregion //CloseButtonVisibility

                #region CloseButtonVisibilityResolved

        private static readonly DependencyPropertyKey CloseButtonVisibilityResolvedPropertyKey =
            DependencyProperty.RegisterReadOnly("CloseButtonVisibilityResolved",
            typeof(Visibility), typeof(Tile), new FrameworkPropertyMetadata(KnownBoxes.VisibilityCollapsedBox, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        /// <summary>
        /// Identifies the <see cref="CloseButtonVisibilityResolved"/> dependency property
        /// </summary>
        public static readonly DependencyProperty CloseButtonVisibilityResolvedProperty =
            CloseButtonVisibilityResolvedPropertyKey.DependencyProperty;

        /// <summary>
        /// Determines the visibility of the close button in the TileHeaderPresenter (read-only)
        /// </summary>
        /// <seealso cref="CloseButtonVisibilityResolvedProperty"/>
        /// <seealso cref="XamTilesControl.TileCloseAction"/>
        /// <seealso cref="CloseAction"/>
        /// <seealso cref="CloseButtonVisibility"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public Visibility CloseButtonVisibilityResolved
        {
            get
            {
                return (Visibility)this.GetValue(Tile.CloseButtonVisibilityResolvedProperty);
            }
        }

                #endregion //CloseButtonVisibilityResolved

                #region ExpandButtonVisibility

        /// <summary>
        /// Identifies the <see cref="ExpandButtonVisibility"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ExpandButtonVisibilityProperty = DependencyProperty.Register("ExpandButtonVisibility",
            typeof(Visibility?), typeof(Tile), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnExpandButtonVisibilityChanged)));

        private static void OnExpandButtonVisibilityChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            Tile tile = target as Tile;

            // JJD 2/08/10 - TFS27327
            // Make sure to re-evaluate the ExpandButtonVisibilityResolved property
            tile.SetResolvedVisibilityProperties();
        }

        /// <summary>
        /// Gets/sets the visibility of the expand toggle button in the header area
        /// </summary>
        /// <seealso cref="XamTilesControl.IsInMaximizedMode"/>
        /// <seealso cref="ExpandButtonVisibilityResolved"/>
        /// <seealso cref="ExpandButtonVisibilityProperty"/>
        //[Description("Gets/sets the visibility of the expand toggle button in the header area")]
        //[Category("TilesControl Properties")]
        [TypeConverter(typeof(NullableConverter<Visibility>))]
        public Visibility? ExpandButtonVisibility
        {
            get
            {
                return (Visibility?)this.GetValue(Tile.ExpandButtonVisibilityProperty);
            }
            set
            {
                this.SetValue(Tile.ExpandButtonVisibilityProperty, value);
            }
        }

                #endregion //ExpandButtonVisibility

                #region ExpandButtonVisibilityResolved

        private static readonly DependencyPropertyKey ExpandButtonVisibilityResolvedPropertyKey =
            DependencyProperty.RegisterReadOnly("ExpandButtonVisibilityResolved",
            typeof(Visibility), typeof(Tile), new FrameworkPropertyMetadata(KnownBoxes.VisibilityCollapsedBox));


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
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public Visibility ExpandButtonVisibilityResolved
        {
            get
            {
                return (Visibility)this.GetValue(Tile.ExpandButtonVisibilityResolvedProperty);
            }
        }

                #endregion //ExpandButtonVisibilityResolved

                #region HasImage

        private static readonly DependencyPropertyKey HasImagePropertyKey =
			DependencyProperty.RegisterReadOnly("HasImage",
			typeof(bool), typeof(Tile), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="HasImage"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasImageProperty =
			HasImagePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating if the <see cref="Image"/> property has been set.
		/// </summary>
		/// <seealso cref="HasImageProperty"/>
		/// <seealso cref="Image"/>
		//[Description("Returns a boolean indicating if the Image property has been set.")]
		//[Category("TilesControl Properties")] 
		[Bindable(true)]
		[ReadOnly(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool HasImage
		{
			get
			{
				return (bool)this.GetValue(Tile.HasImageProperty);
			}
		}

		        #endregion //HasImage

                #region HasHeader

        private static readonly DependencyPropertyKey HasHeaderPropertyKey =
			DependencyProperty.RegisterReadOnly("HasHeader",
			typeof(bool), typeof(Tile), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="HasHeader"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasHeaderProperty =
			HasHeaderPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating if the <see cref="Header"/> property has been set.
		/// </summary>
		/// <seealso cref="HasHeaderProperty"/>
		/// <seealso cref="Header"/>
		//[Description("Returns a boolean indicating if the Header property has been set.")]
		//[Category("TilesControl Properties")] 
		[Bindable(true)]
		[ReadOnly(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool HasHeader
		{
			get
			{
				return (bool)this.GetValue(Tile.HasHeaderProperty);
			}
		}

		        #endregion //HasHeader

                #region Header

        /// <summary>
        /// Identifies the <see cref="Header"/> dependency property
        /// </summary>
        public static readonly DependencyProperty HeaderProperty = HeaderedContentControl.HeaderProperty.AddOwner(typeof(Tile),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnHeaderChanged)));

        private static void OnHeaderChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            Tile tile = target as Tile;

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
        [Localizability(LocalizationCategory.Label)]
        public object Header
        {
            get
            {
                return (object)this.GetValue(Tile.HeaderProperty);
            }
            set
            {
                this.SetValue(Tile.HeaderProperty, value);
            }
        }

                #endregion //Header

                #region HeaderStringFormat

        private static DependencyProperty s_HeaderStringFormatProperty;

        /// <summary>
        /// Identifies the <see cref="HeaderStringFormat"/> dependency property
        /// </summary>
        public static DependencyProperty HeaderStringFormatProperty { get { return s_HeaderStringFormatProperty; } }


        /// <summary>
        /// Gets/sets the format of the tile header
        /// </summary>
        /// <seealso cref="HeaderStringFormatProperty"/>
        //[Description("Gets/sets the format of the tile header")]
        //[Category("Control")]
        [Bindable(true)]
        public string HeaderStringFormat
        {
            get
            {
                return (string)this.GetValue(Tile.HeaderStringFormatProperty);
            }
            set
            {
                this.SetValue(Tile.HeaderStringFormatProperty, value);
            }
        }

                #endregion //HeaderStringFormat
	
                #region HeaderTemplate

        /// <summary>
        /// Identifies the <see cref="HeaderTemplate"/> dependency property
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateProperty = HeaderedContentControl.HeaderTemplateProperty.AddOwner(typeof(Tile));

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
                return (DataTemplate)this.GetValue(Tile.HeaderTemplateProperty);
            }
            set
            {
                this.SetValue(Tile.HeaderTemplateProperty, value);
            }
        }

                #endregion //HeaderTemplate

                #region HeaderTemplateSelector

        /// <summary>
        /// Identifies the <see cref="HeaderTemplateSelector"/> dependency property
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateSelectorProperty = HeaderedContentControl.HeaderTemplateSelectorProperty.AddOwner(typeof(Tile));

        /// <summary>
        /// Gets/sets a selector to allow th application writer to assign different header templates for each tile.
        /// </summary>
        /// <value>A DataTemplateSelector object or the default value of null.</value>
        /// <seealso cref="HeaderTemplateSelectorProperty"/>
        //[Description("Gets/sets a selector to allow th application writer to assign different header templates for each tile.")]
        //[Category("Content")]
        [Bindable(true)]
        public DataTemplateSelector HeaderTemplateSelector
        {
            get
            {
                return (DataTemplateSelector)this.GetValue(Tile.HeaderTemplateSelectorProperty);
            }
            set
            {
                this.SetValue(Tile.HeaderTemplateSelectorProperty, value);
            }
        }

                #endregion //HeaderTemplateSelector
    
		        #region Image

		/// <summary>
		/// Identifies the <see cref="Image"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image",
			typeof(ImageSource), typeof(Tile), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentMeasure, new PropertyChangedCallback(OnImageChanged)));

		private static void OnImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
            if ( e.NewValue == null )
                d.ClearValue(Tile.HasImagePropertyKey);
            else
			    d.SetValue(Tile.HasImagePropertyKey, KnownBoxes.TrueBox);
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
				return (ImageSource)this.GetValue(Tile.ImageProperty);
			}
			set
			{
				this.SetValue(Tile.ImageProperty, value);
			}
		}

		        #endregion //Image

                #region IsClosed

        /// <summary>
        /// Identifies the <see cref="IsClosed"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsClosedProperty = DependencyProperty.Register("IsClosed",
            typeof(bool), typeof(Tile), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsClosedChanged)));

        private static void OnIsClosedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            Tile tile = target as Tile;

            tile.CoerceValue(VisibilityProperty);

            // make sure the ItemInfo object reflects this change
            
            
            if (!tile._isSynchonizingFromItemInfo)
                //tile.Panel.SyncItemInfo(tile);
            {
                TilesPanel panel = tile.Panel;

                
                // If the panel member is null then get it from the tiles control
                if ( panel == null && tile._tilesControl != null )
                    panel = tile._tilesControl.Panel;

                if (panel != null)
                {
                    panel.SyncItemInfo(tile, false);

					// JJD 4/19/11 - TFS58732
					// Call OnTileIsClosedChanged which will try to maintain relative positioning
					// in the maximized list when maximized tiles are closed/unclosed
					
					
					
					//if (tile.IsClosed == false)
					//    panel.InvalidateMeasure();
					panel.OnTileIsClosedChanged(tile);
                }
            }
        }

        /// <summary>
        /// Gets/sets whether this tile is closed.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if true then this tile's Visibility will be coerced to 'Collapsed'</para>
        /// </remarks>
        /// <seealso cref="IsClosedProperty"/>
        /// <seealso cref="CloseAction"/>"/>
        //[Description("Gets/sets whether this tile is closed.")]
        //[Category("TilesControl Properties")]
        public bool IsClosed
        {
            get
            {
                return (bool)this.GetValue(Tile.IsClosedProperty);
            }
            set
            {
                this.SetValue(Tile.IsClosedProperty, value);
            }
        }

                #endregion //IsClosed

                #region IsDragging

        /// <summary>
        /// Identifies the <see cref="IsDragging"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsDraggingProperty = TilesPanelBase.IsDraggingProperty.AddOwner(typeof(Tile));

        /// <summary>
        /// Returns whether this tile is currently being dragged (read-only)
        /// </summary>
        /// <seealso cref="IsDraggingProperty"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        [Bindable(true)]
         public bool IsDragging
        {
            get
            {
                return (bool)this.GetValue(Tile.IsDraggingProperty);
            }
        }

                #endregion //IsDragging

                #region IsExpandedWhenMinimized

        /// <summary>
        /// Identifies the <see cref="IsExpandedWhenMinimized"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsExpandedWhenMinimizedProperty = DependencyProperty.Register("IsExpandedWhenMinimized",
            typeof(bool?), typeof(Tile), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnIsExpandedWhenMinimizedChanged)));

        private static void OnIsExpandedWhenMinimizedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            Tile tile = target as Tile;

            // make sure the ItemInfo object reflects this change
            if (!tile._isSynchonizingFromItemInfo)
            {
                TilesPanel panel = tile.Panel;

                if (panel != null)
                {
                    panel.SyncItemInfo(tile, false);

                    
                    // Coerce the state if the tile is currently minimized
                    switch (tile.State)
                    {
                        case TileState.Minimized:
                        case TileState.MinimizedExpanded:
                            
                            
                            
                            tile.CoerceState();
                            break;
                    }

                    tile.SetContentVisibility();
                }
            }
        }

        /// <summary>
        /// Gets/sets whether this tile is expanded when it is minimized
        /// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if this property is set to true on multiple tiles but <see cref="MaximizedModeSettings"/>.<see cref="MaximizedModeSettings.MinimizedTileExpansionMode"/> is set to 'AllowOne' then the last tile processed will be the only expanded tile.
		/// Also note that when swapping tiles via a drag operation this setting can be optionally swapped between the source tile and the target tile based on the <see cref="TileSwappingEventArgs.SwapIsExpandedWhenMinimized"/> property on the <see cref="TileSwappingEventArgs"/> class.</para>
		/// </remarks>
        /// <seealso cref="IsExpandedWhenMinimizedProperty"/>
		/// <seealso cref="MaximizedModeSettings.MinimizedTileExpansionMode"/>
		/// <seealso cref="TileSwappingEventArgs.SwapIsExpandedWhenMinimized"/>
        //[Description("Gets/sets whether this tile is expanded when it is minimized")]
        //[Category("TilesControl Properties")]
        [TypeConverter(typeof(System.Windows.NullableBoolConverter))]
        public bool? IsExpandedWhenMinimized
        {
            get
            {
                return (bool?)this.GetValue(Tile.IsExpandedWhenMinimizedProperty);
            }
            set
            {
                this.SetValue(Tile.IsExpandedWhenMinimizedProperty, value);
            }
        }

                #endregion //IsExpandedWhenMinimized

                #region IsSwapTarget

        /// <summary>
        /// Identifies the <see cref="IsSwapTarget"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsSwapTargetProperty = TilesPanelBase.IsSwapTargetProperty.AddOwner(typeof(Tile));

        /// <summary>
        /// Returns whether another tile is being dragged over this tile and if released will swap positions with this tile (read-only)
        /// </summary>
        /// <seealso cref="IsSwapTargetProperty"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        [Bindable(true)]
        public bool IsSwapTarget
        {
            get
            {
                return (bool)this.GetValue(Tile.IsSwapTargetProperty);
            }
        }

                #endregion //IsSwapTarget

                #region MaximizeButtonVisibility

        /// <summary>
        /// Identifies the <see cref="MaximizeButtonVisibility"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MaximizeButtonVisibilityProperty = DependencyProperty.Register("MaximizeButtonVisibility",
            typeof(Visibility?), typeof(Tile), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnMaximizeButtonVisibilityChanged)));

        private static void OnMaximizeButtonVisibilityChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            Tile tile = target as Tile;

            
            // Make sure to re-evaluate the MaximizeButtonVisibilityResolved property
            tile.SetResolvedVisibilityProperties();
        }

        /// <summary>
        /// Gets/sets the visibility of the maximize toggle button in the header area
        /// </summary>
        /// <seealso cref="XamTilesControl.MaximizedTileLimit"/>
        /// <seealso cref="MaximizeButtonVisibilityProperty"/>
        //[Description("Gets/sets the visibility of the maximize toggle button in the header area")]
        //[Category("TilesControl Properties")]
        [TypeConverter(typeof(NullableConverter<Visibility>))]
        public Visibility? MaximizeButtonVisibility
        {
            get
            {
                return (Visibility?)this.GetValue(Tile.MaximizeButtonVisibilityProperty);
            }
            set
            {
                this.SetValue(Tile.MaximizeButtonVisibilityProperty, value);
            }
        }

                #endregion //MaximizeButtonVisibility

                #region MaximizeButtonVisibilityResolved

        private static readonly DependencyPropertyKey MaximizeButtonVisibilityResolvedPropertyKey =
            DependencyProperty.RegisterReadOnly("MaximizeButtonVisibilityResolved",
            typeof(Visibility), typeof(Tile), new FrameworkPropertyMetadata(KnownBoxes.VisibilityCollapsedBox, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentMeasure));


        /// <summary>
        /// Identifies the <see cref="MaximizeButtonVisibilityResolved"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MaximizeButtonVisibilityResolvedProperty =
            MaximizeButtonVisibilityResolvedPropertyKey.DependencyProperty;

        /// <summary>
        /// Determines the visibility of the maximize button in the TileHeaderPresenter (read-only)
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note</b>: if <see cref="ModeSettingsBase"/>.<see cref="XamTilesControl.MaximizedTileLimit"/> is set to zero then this property will be set to 'Collapsed'. 
        /// Unless the <see cref="MaximizeButtonVisibility"/> is set.</para>
        /// </remarks>
        /// <seealso cref="MaximizeButtonVisibilityResolvedProperty"/>
        /// <seealso cref="MaximizeButtonVisibility"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public Visibility MaximizeButtonVisibilityResolved
        {
            get
            {
                return (Visibility)this.GetValue(Tile.MaximizeButtonVisibilityResolvedProperty);
            }
        }

                #endregion //MaximizeButtonVisibilityResolved

		        #region Row

		/// <summary>
		/// Identifies the Row dependency property.
		/// </summary>
		public static readonly DependencyProperty RowProperty = TilesPanel.RowProperty.AddOwner(typeof(Tile));

		/// <summary>
        /// Gets/sets the value of the Row. A value of -1 indicates that the tile will be positioned relative to the previous tile in the panel. The default value is 0.
		/// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property is ignored unless <see cref="XamTilesControl"/>.<see cref="XamTilesControl.NormalModeSettings"/>.<see cref="Infragistics.Windows.Tiles.NormalModeSettings.TileLayoutOrder"/> is set to 'UseExplicitRowColumnOnTile'.</para>
        /// </remarks>
        //[Description("Gets/sets the value of the Row, -1 indicates that the tile will be positioned relative to the previous tile in the panel.")]
        //[Category("TilesControl Properties")]
        public int Row
        {
            get
            {
                return (int)this.GetValue(Tile.RowProperty);
            }
            set
            {
                this.SetValue(Tile.RowProperty, value);
            }
		}

		        #endregion // Row

		        #region RowSpan

		/// <summary>
		/// Identifies the RowSpan dependency property.
		/// </summary>
		public static readonly DependencyProperty RowSpanProperty = TilesPanel.RowSpanProperty.AddOwner(typeof(Tile));

		/// <summary>
        /// Gets/sets the value of the RowSpan, 0 indicates that the tile will occupy the remainder of the space in its logical row. Default value is 1.
        /// </summary>        
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property is ignored unless <see cref="XamTilesControl"/>.<see cref="XamTilesControl.NormalModeSettings"/>.<see cref="Infragistics.Windows.Tiles.NormalModeSettings.TileLayoutOrder"/> is set to 'UseExplicitRowColumnOnTile'.</para>
        /// </remarks>
        //[Description("Gets/sets the value of the RowSpan, 0 indicates that the tile will occupy the remainder of the space in its logical row.")]
        //[Category("TilesControl Properties")]
        public int RowSpan
        {
            get
            {
                return (int)this.GetValue(Tile.RowSpanProperty);
            }
            set
            {
                this.SetValue(Tile.RowSpanProperty, value);
            }
		}

		        #endregion // RowSpan

		        #region RowWeight

		/// <summary>
		/// Identifies the RowWeight dependency property.
		/// </summary>
		public static readonly DependencyProperty RowWeightProperty = TilesPanel.RowWeightProperty.AddOwner(typeof(Tile));

		/// <summary>
        /// Gets/sets how any extra height will be distributed among tiles. The default value is 1.
		/// </summary>
        //[Description("Gets/sets how any extra height will be distributed among tiles. The default value is 1.")]
        //[Category("TilesControl Properties")]
        public float RowWeight
        {
            get
            {
                return (float)this.GetValue(Tile.RowWeightProperty);
            }
            set
            {
                this.SetValue(Tile.RowWeightProperty, value);
            }
		}

		        #endregion // RowWeight

		        #region SaveInLayout

		/// <summary>
		/// Identifies the <see cref="SaveInLayout"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SaveInLayoutProperty = DependencyProperty.Register("SaveInLayout",
			typeof(bool), typeof(Tile), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

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
				return (bool)this.GetValue(Tile.SaveInLayoutProperty);
			}
			set
			{
				this.SetValue(Tile.SaveInLayoutProperty, value);
			}
		}

		        #endregion //SaveInLayout

		        #region SerializationId

		/// <summary>
		/// Identifies the <see cref="SerializationId"/> dependency property
		/// </summary>
        public static readonly DependencyProperty SerializationIdProperty = TilesPanel.SerializationIdProperty.AddOwner(typeof(Tile));

		/// <summary>
		/// Returns/sets a string that is stored to idenify the tile when the layout is saved.
		/// </summary>
		/// <remarks>
        /// <p class="body">The SerializationId property is not used by the Tile itself. Instead, the value of the 
		/// property is saved along with the layout when using the <see cref="XamTilesControl.SaveLayout(System.IO.Stream)"/> method. 
        /// This property can be used to save information that you can use to re-create an item when the layout is loaded if 
        /// the associated tile is not already loaded thru the <see cref="XamTilesControl.LoadingItemMapping"/> event. 
        /// The SerializationId can be used to identify what type of content 
		/// that you want to create. For example, this can be set to the name of the file so that you can load the file when 
		/// the layout is loaded.</p>
		/// </remarks>
		/// <seealso cref="SerializationIdProperty"/>
		//[Description("A string that is stored with the tile when the layout is saved.")]
        //[Category("TilesControl Properties")] 
		[Bindable(true)]
		public string SerializationId
		{
			get
			{
                return (string)this.GetValue(Tile.SerializationIdProperty);
			}
			set
			{
                this.SetValue(Tile.SerializationIdProperty, value);
			}
		}

		        #endregion //SerializationId
    
                #region State

        /// <summary>
        /// Identifies the <see cref="State"/> dependency property
        /// </summary>
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State",
            typeof(TileState), typeof(Tile), new FrameworkPropertyMetadata(NormalBox, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange, new PropertyChangedCallback(OnStateChanged), new CoerceValueCallback(CoerceState)));

        private static void OnStateChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            Tile tile = target as Tile;
            TilesPanel panel = tile.Panel;
			
			// JJD 11//8/11 - TFS29248/TFS95683 
			// keep track of whether we are transitioning from a maximized state
			tile._wasMaximized = object.Equals(e.OldValue, TileState.Maximized);

            if (panel == null )
            {
                XamTilesControl tc = tile.Parent as XamTilesControl;

                // JJD 2/06/10 - TFS27262 
                // Since the panel hasn't been initialized yet call 
                // InitializeTilesControl so we can make sure the maximizedItems
                // collection gets synched up
                if (tc != null)
                    tile.InitializeTilesControl(tc);
            }
            else     
            if (panel != null &&
				 
				 
				 
				 
				 panel.WasLoaded &&
                !tile._isSynchonizingFromItemInfo )
            {
                TileState newState = (TileState)e.NewValue;
                TileState oldState = (TileState)e.OldValue;

                // if we will be shrinking the tile and we are animating the change
                // then take a snapshot of the tile while it was large to make
                // the animation look good
                // JJD 2/26/90 - TFS28590
                // Always take the snapshot becuase we can't always assume the one
                // state will be larger than another. Later we may release
                // the snapshot inside the UpdateTrasform method if the new size is 
                // larger than the old size 
                // since we don't need it anymore
                if (tile._snapshotImage == null &&
                    panel.ShouldAnimate )
                    //IsOldStateLarger(newState, oldState))
                    tile.SnapshotImage();

                // coerce the template to pich up any state specific template setting
                tile.CoerceValue(ContentTemplateProperty);
            
                // make sure the ItemInfo object reflects this change
                panel.SyncItemInfo(tile, false);

                // JJD 1/5/10 - TFS25900
                // If we aren't being called from within the panel's ChangeTileState method
                // then we need to call ChangeTileStateHelper so everyone's state gets synchonized 
                // properly
                if (panel._isSettingTileState == false)
                {
                    panel.ChangeTileStateHelper(tile, newState, oldState);

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

                // raise the after event
                panel.RaiseTileStateChanged(new TileStateChangedEventArgs(tile, panel.ItemFromTile(tile), newState, oldState));
            }

            tile.SetContentVisibility();


            // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
            tile.UpdateVisualStates();



        }

        private static object CoerceState(DependencyObject target, object value)
        {
            Tile tile = target as Tile;

            if (tile != null)
            {
                bool oldIsInFlag = tile._isInCoerceState;

                // JJD 2/13/10 - TFS27558
                // Set a flag s we know we are in the coerce state logic
                tile._isInCoerceState = true;

                try
                {
                    TileState state = (TileState)value;
                    TilesPanel panel = tile.Panel;

                    if (panel != null)
                    {
                        if (panel.IsInMaximizedMode)
                        {
                            switch (state)
                            {
                                case TileState.Minimized:
                                case TileState.MinimizedExpanded:
                                    {
                                        if (panel._isSettingTileState == true)
                                            return state;

                                        // JJD 2/13/10 - TFS27558
                                        // If the state doesn't match the existing state
                                        // and the existing state is one of the minimized states
                                        // that means that the state is being set externally
                                        // and we need to return it as is.
                                        TileState existingState = tile.State;

                                        if (existingState != state)
                                        {
                                            switch (existingState)
                                            {
                                                case TileState.Minimized:
                                                case TileState.MinimizedExpanded:
                                                    // JJD 2/13/10 - TFS27558
                                                    // Set the IsExpandedWhenMinimized based on the new state
                                                    tile.IsExpandedWhenMinimized = state == TileState.MinimizedExpanded;
                                                    return state;
                                            }
                                        }
                                    }
                                    break;
                            }

                            if (state != TileState.Maximized)
                                return tile.IsExpandedWhenMinimizedResolved ? TileState.MinimizedExpanded : TileState.Minimized;
                        }
                        else
                        {
                            if (state == TileState.Maximized)
                                return state;

                            return NormalBox;
                        }
                    }
                }
                finally
                {
                    // JJD 2/13/10 - TFS27558
                    // restore the _isInCoerceState flag
                    tile._isInCoerceState = oldIsInFlag;
                }
            }

            return value;
        }

        /// <summary>
        /// Gets/sets the state of the Tile
        /// </summary>
        /// <seealso cref="StateProperty"/>
        //[Description("Gets/sets the state of the Tile")]
        //[Category("TilesControl Properties")]
        public TileState State
        {
            get
            {
                return (TileState)this.GetValue(Tile.StateProperty);
            }
            set
            {
                this.SetValue(Tile.StateProperty, value);
            }
        }

                #endregion //State

            #endregion //Public Properties

            #region Internal Properties

                // JJD 2/11/10 - TFS26775 - added
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

                TileHeaderPresenter thp = Utilities.GetWeakReferenceTargetSafe(this._headerPresenter) as TileHeaderPresenter;

                if (thp != null &&
                    thp.TemplatedParent != this)
                {
                    this._headerPresenter = null;
                    thp = null;
                }

                return thp;
            }
        }

                #endregion //HeaderPresenter	

                #region IsExpandedWhenMinimizedResolved

        internal bool IsExpandedWhenMinimizedResolved
        {
            get
            {
                return TilesPanel.GetIsExpandedWhenMinimizedHelper(this._panel, this, null, this.IsExpandedWhenMinimized);
            }
        }

 
                #endregion //IsExpandedWhenMinimizedResolved	

                #region Panel

        internal TilesPanel Panel
        {
            get
            {
                return this._panel;
            }
        }

                #endregion //Panel
	
                #region TilesControl

        internal XamTilesControl TilesControl
        {
            get
            {
                if ( this._tilesControl != null )
                    return this._tilesControl;

                if (this._panel != null && this._panel.TilesControl != null)
                   this.InitializeTilesControl( this._panel.TilesControl );

                return this._tilesControl;
            }
        }

                #endregion //TilesControl

            #endregion //Internal Properties	

            #region Private Properties

            #endregion //Private Properties	
            
        #endregion //Properties

        #region Methods

            #region Internal Methods

                #region OnAnimationEnded

        internal void OnAnimationEnded()
        {
            this.ReleaseSnapshotImage();
        }

                #endregion //OnAnimationEnded	

                #region InitializeHeaderPresenter

        internal void InitializeHeaderPresenter(TileHeaderPresenter headerPreseenter)
        {
            if (headerPreseenter == null)
                this._headerPresenter = null;
            else
                this._headerPresenter = new WeakReference(headerPreseenter);
        }

                #endregion //InitializeHeaderPresenter	

                #region InitializePanel

        // JJD 2/06/10 - TFS27262 - added
        internal void InitializePanel(TilesPanel panel)
        {
            if (panel == this._panel)
            {
                // JJD 2/10/10 - TFS27473
                // Make sure the cached panel is not null
                if ( this._panel != null &&
                     this._tilesControl == null &&
                     this._panel.TilesControl != null)
                    this.InitializeTilesControl(this._panel.TilesControl);
                return;
            }

            this._tracker = null;

            this._panel = panel;

            if (this._panel != null)
            {
                // JJD 3/25/10 - TFS29388
                // Moved below until after the tile has been synced up with the info object's state
                //this.OnLayoutVersionChanged();

                this._tracker = new PropertyValueTracker(this._panel, TilesPanel.LayoutVersionProperty, new PropertyValueTracker.PropertyValueChangedHandler(OnLayoutVersionChanged));

				// JJD 8/17/10 - TFS35319
				// if the state is not normal then make sure the item info state is synced to the tile
				if (this.State != TileState.Normal ||
					 // JJD 10/15/10 - TFS48188
					 // Only sync the item info if IsExpandedWhenMinimized has been set and it resolves to tru
					 //this.IsExpandedWhenMinimizedResolved)
					 ( this.IsExpandedWhenMinimized.HasValue &&
						this.IsExpandedWhenMinimizedResolved))
				{
					this._panel.SyncItemInfo(this, true);
				}

                this._panel.SyncTileFromItemInfo(this);

                // JJD 3/25/10 - TFS29388
                // Moved from above so that the tile has now been synced up with the info object's state
                this.OnLayoutVersionChanged();
            }
        }

                #endregion //InitializeTilesControl	

                #region InitializeTilesControl

        // JJD 2/06/10 - TFS27262 - added
        internal void InitializeTilesControl(XamTilesControl tilesControl)
        {
            this._tilesControl = tilesControl;

            // JJD 2/06/10 - TFS27262 
            // if the tile is in the Items collection and it is maximized
            // make sure it is also in the MaximizedItems collection
            if (this._tilesControl != null)
            {
                ItemInfo info = this._tilesControl.GetItemInfo(this);

                if (info != null)
                {
                    if (this.State == TileState.Maximized &&
                        !this._tilesControl.MaximizedItemsInternal.Contains(this))
                    {
                        this._tilesControl.MaximizedItemsInternal.Add(this);

                        
                        
                        
                        
                        
                        this._tilesControl.SetValue(TilesPanel.IsInMaximizedModePropertyKey, KnownBoxes.TrueBox);
                    }

                    info.SyncFromTileState(this);
                }
            }
        }

                #endregion //InitializeTilesControl	

                #region SynchStateFromInfo

        internal void SynchStateFromInfo(ItemInfo info)
        {
            if (this._isSynchonizingFromItemInfo)
                return;

            this._isSynchonizingFromItemInfo = true;

            try
            {
                this.IsClosed = info.IsClosed;
                this.IsExpandedWhenMinimized = info.IsExpandedWhenMinimized;

                TileState state;
                if (info.IsMaximized)
                    state = TileState.Maximized;
                else
                {
                    TilesPanel panel = this.Panel;

                    if (panel != null &&
                         panel.IsInMaximizedMode)
                    {
                        if (info.IsExpandedWhenMinimizedResolved)
                            state = TileState.MinimizedExpanded;
                        else
                            state = TileState.Minimized;
                    }
                    else
                        state = TileState.Normal;
                }

                this.State = state;

                this.SetResolvedVisibilityProperties();

                this.CoerceValue(ContentTemplateProperty);
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

			// JM 05-24-10 - NA 10.2 SmartTag
			//if (this._transformGroup == null)
			if (this._transformGroup == null												|| 
				(this._translateTransform	!= null && this._translateTransform.IsSealed)	||
				(this._scaleTransform		!= null && this._scaleTransform.IsSealed))
            {
                this._transformGroup = new TransformGroup();
                this._translateTransform = new TranslateTransform();
                this._scaleTransform = new ScaleTransform();
                this._transformGroup.Children.Add(this._translateTransform);
                this._transformGroup.Children.Add(this._scaleTransform);
                
                this.CoerceValue(RenderTransformProperty);
            }

            Rect rect = Rect.Offset(current, offset);
            Rect targetRect = Rect.Offset(target, offset);

            // JJD 2/11/10 - TFS26775 
            // Check if we have any descendant hwndhosts, if so we don't want to 
            // animate the size so set the rect's size to the targetRect's size
            bool hasHwndHost = this.HasHwndHost;
            if (hasHwndHost)
                rect.Size = targetRect.Size;

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

            if (this._snapshotImage != null)
            {
                // JJD 2/23/10 - TFS27400
                // If the rect is the same as the target rect and we are being called
                // from arrange then release the snapshot image since there will be no
                // need for an animation and our OnAnimationEnded method may never be called.
                if (calledFromArrange == true )
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
                
                
                
                if (this._snapshotImage != null)
                {
                    
                    // Determine the larger of the 2 factors for use in both dimensions
                    
                    
                    double snapshotScaleX = rect.Width /  Math.Max((original.Width * scaleX), 1);
                    double snapshotScaleY = rect.Height /  Math.Max((original.Height * scaleY), 1);
                    double snapshotScaleFactor = Math.Max(snapshotScaleX, snapshotScaleY);

                    this._snapshotImageScaleTransform.ScaleX = snapshotScaleFactor;
                    this._snapshotImageScaleTransform.ScaleY = snapshotScaleFactor;

                    
                    // In the case where the new state is minimized and we aren't going to show content
                    // animate the opacity quickly to zero to get rid of the old content's image before it gets
                    // to the end of the animation. This avoids some potentially ugly results when the
                    // tile is stretched vertically
					if ( calledFromArrange == false && this.State == TileState.Minimized && this.ContentTemplate == null)
                        this._snapshotImageBrush.Opacity = Math.Min(1.0d, Math.Max(1.0 - (resizeFactor * 2), 0d));
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

            #endregion //Internal Methods	

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


        // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
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
            if (this.IsMouseOver)
                VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);
            
            // Set drag state
            if (TilesPanelBase.GetIsDragging(this))
                VisualStateManager.GoToState(this, VisualStateUtilities.StateDragging, useTransitions);
            else
            if (TilesPanelBase.GetIsSwapTarget(this))
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

        // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
        internal static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            Tile tile = target as Tile;

            if (tile != null)
                tile.UpdateVisualStates();
        }

        // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
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
        
            #region Private Methods

                #region CoerceContentTemplate

        private static object CoerceContentTemplate(DependencyObject target, object value)
        {
            Tile tile = target as Tile;
 
            DataTemplate stateTemplate = null;

            switch (tile.State)
            {
                case TileState.Maximized:
                    stateTemplate = tile.ContentTemplateMaximized;
                    break;

                case TileState.Minimized:
                    stateTemplate = tile.ContentTemplateMinimized;
                    break;

                case TileState.MinimizedExpanded:
                    stateTemplate = tile.ContentTemplateMinimizedExpanded;
                    break;

            }

            if (stateTemplate != null)
                return stateTemplate;

            return value;
        }

                #endregion //CoerceContentTemplate

                #region CoerceRenderTransform

        private static object CoerceRenderTransform(DependencyObject target, object value)
        {
            Tile tile = target as Tile;

            Transform explicitTransform = value as Transform;

            // they didn't specify a tranform then return our transform group
            if ( explicitTransform == null )
                return tile._transformGroup;

            // if we don't have a group then return the explicit transform
            if (tile._transformGroup == null)
                return explicitTransform;

            // if our group already containes the explicit transform
            // then return the group
            if( tile._transformGroup.Children.Contains(explicitTransform) )
                return tile._transformGroup;

            tile._transformGroup = new TransformGroup();

            if (tile._translateTransform != null)
                tile._transformGroup.Children.Add(tile._translateTransform);

            if (tile._scaleTransform != null)
                tile._transformGroup.Children.Add(tile._scaleTransform);

            tile._transformGroup.Children.Add(explicitTransform);

            return tile._transformGroup;
        }

                #endregion //CoerceRenderTransform

                // JJD 2/13/10 - TFS27558 - added
                #region CoerceState

        private void CoerceState()
        {
            // JJD 2/13/10 - TFS27558
            // If we are already in the coerce then bail
            if (this._isInCoerceState)
                return;

            this.CoerceValue(StateProperty);
        }

                #endregion //CoerceState	
    
                #region CoerceVisibility

        private static object CoerceVisibility(DependencyObject target, object value)
        {
            Tile tile = target as Tile;

            if (tile.IsClosed)
                return KnownBoxes.VisibilityCollapsedBox;

            return value;
        }

                #endregion //CoerceVisibility

                #region IsOldStateLarger

        // JJD 2/26/90 - TFS28590 - no longer needed
        //private static bool IsOldStateLarger(TileState newState, TileState oldState)
        //{
        //    switch (oldState)
        //    {
        //        case TileState.Maximized:
        //            return true;

        //        case TileState.Minimized:
        //            return false;

        //        case TileState.MinimizedExpanded:
        //            return newState == TileState.Minimized;

        //        default:
        //        case TileState.Normal:
        //            return newState != TileState.Maximized;
        //    }
        //}

                #endregion //IsOldStateLarger   	
    
                #region OnLayoutVersionChanged

        private void OnLayoutVersionChanged()
        {
            
            
            
            this.CoerceState();
            this.CoerceValue(ContentTemplateProperty);
            this.SetResolvedVisibilityProperties();
            this.SynchIsExpandedFromState();
        }

                #endregion //OnLayoutVersionChanged	

                #region OnRenderTransformChanged

        private static void OnRenderTransformChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            // We need to Coerce the RenderTransformProperty when it gets set to null
            // since the only time that should happen is if someone calls
            // ClearValue. Calling CoerceValue will cause our callback to be called.
            if (e.NewValue == null)
                target.CoerceValue(RenderTransformProperty);
        }

                #endregion //OnRenderTransformChanged	
    
                #region ProcessConstraintsChanged

        private void ProcessConstraintsChanged()
        {
            if (this.IsInitialized && this.IsLoaded &&
                this.IsVisible)
            {
                this.InvalidateMeasure();

                if (this._panel != null)
                    this._panel.InvalidateMeasure();
            }
        }

                #endregion //ProcessConstraintsChanged	
    
                #region ReleaseSnapshotImage

        private void ReleaseSnapshotImage()
        {
            if (this._snapshotImage == null)
                return;

            Rectangle img = this._snapshotImage;

            this._snapshotImage = null;
            this._snapshotImageBrush = null;
            this._snapshotImageScaleTransform = null;

            this.RemoveVisualChild(img);
        }

                #endregion //ReleaseSnapshotImage	

                #region SetContentVisibility

        internal void SetContentVisibility()
        {
            // set the visibility of the content
            if ( this.State == TileState.Minimized && this.ContentTemplateMinimized == null )
                this.SetValue(ContentVisibilityPropertyKey, KnownBoxes.VisibilityCollapsedBox);
            else
                this.ClearValue(ContentVisibilityPropertyKey);
        }

                #endregion //SetContentVisibility	

                #region SetResolvedVisibilityProperties

        private void SetResolvedVisibilityProperties()
        {
            this.SetContentVisibility();

            Visibility? vis = this.CloseButtonVisibility;

            if (!vis.HasValue)
            {
                if (this.CloseActionResolved == TileCloseAction.DoNothing)
                    vis = Visibility.Collapsed;
                else
                    vis = Visibility.Visible;
            }

            SetValue(CloseButtonVisibilityResolvedPropertyKey, KnownBoxes.FromValue(vis.Value));

            switch (this.State)
            {
                case TileState.Maximized:
                case TileState.Normal:
                    
                    // Always set the ExpandButtonVisibilityResolved property to 
                    // 'Collapsed' when  not miminized
                    vis = Visibility.Collapsed;
                    break;
                default:
                    {
                        vis = this.ExpandButtonVisibility;

                        if (!vis.HasValue)
                        {
                            if (this._panel != null)
                                vis = this._panel.MaximizedModeSettingsSafe.MinimizedTileExpandButtonVisibility;
                            else
                                vis = Visibility.Visible;
                        }
                        break;
                    }
            }

            SetValue(ExpandButtonVisibilityResolvedPropertyKey, KnownBoxes.FromValue(vis.Value));

            vis = this.MaximizeButtonVisibility;

            if (!vis.HasValue)
            {
                TilesPanel panel = this.Panel;

                if (panel != null && panel.MaximizedTileLimit == 0)
                    vis = Visibility.Collapsed;
                else
                    vis = Visibility.Visible;
            }

            SetValue(MaximizeButtonVisibilityResolvedPropertyKey, KnownBoxes.FromValue(vis.Value));
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

            Size size = new Size(this.ActualWidth, this.ActualHeight);

			// JJD 11/23/10 - TFS60272
			// Make sure not to create an image greater than twice the size of the _panel
			if (this._panel != null)
			{
				Size szTC = new Size(_panel.ActualWidth, _panel.ActualHeight);

				if (szTC.Width > 0)
					size.Width = Math.Min(size.Width, szTC.Width * 2);

				if (szTC.Height > 0)
					size.Height = Math.Min(size.Height, szTC.Height * 2);
			}

			// JJD 11/23/10 - TFS60272
			// If the image is > than 40mb then don't create it
            //if (size.Width < 1 || size.Height < 1 || base.VisualChildrenCount < 1 )
            if (size.Width < 1 || size.Height < 1 || base.VisualChildrenCount < 1 
				|| size.Width * size.Height > 40000000)
                return;

            // JJD 2/11/10 - TFS26775 
            // Check if we have any descendant hwndhosts, if so we don't want to 
            // do any animations
            if (this.HasHwndHost)
                return;

            RenderTargetBitmap bmp = new RenderTargetBitmap((int)(size.Width),
                (int)(size.Height),
                96, 96, PixelFormats.Default);

            bmp.Render(base.GetVisualChild(0));
            //bmp.Freeze();

            this._snapshotImageBrush = new ImageBrush();
            this._snapshotImageBrush.ImageSource = bmp;

            this._snapshotImage = new Rectangle();
            this._snapshotImage.Width               = size.Width;
            this._snapshotImage.Height              = size.Height;
            this._snapshotImage.Fill                = this._snapshotImageBrush;
            this._snapshotImage.IsHitTestVisible    = false;


            this._snapshotImageScaleTransform = new ScaleTransform();
            this._snapshotImage.RenderTransform = this._snapshotImageScaleTransform;

            this.AddVisualChild(this._snapshotImage);

            this._snapshotImage.Measure(size);
            this._snapshotImage.Arrange(new Rect(size));
        }

                #endregion //SnapshotImage	
        
                #region SynchIsExpandedFromState

        private void SynchIsExpandedFromState()
        {

            
            // Only synch the IsExpanded property if we have a panel since
            // the panel can tell us waht the default value is.
            // If the state matches the default then set the
            // IsExpandedWhenMinimized  property to null
            if (this._panel != null)
            {
                switch (this.State)
                {
                    case TileState.Minimized:
                        
                        
                        if (this._panel.IsExpandedWhenMinimizedDefault)
                            this.IsExpandedWhenMinimized = false;
                        else
                            this.IsExpandedWhenMinimized = null;
                        break;
                    case TileState.MinimizedExpanded:
                        
                        
                        if (this._panel.IsExpandedWhenMinimizedDefault)
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
            
        #region IResizableElement Members

        object IResizableElement.ResizeContext
        {
            get { return this; }
        }

        #endregion

        // JJD 2/11/10 - TFS26775 - added
        #region HwndHostTracker internal class

        internal class HwndHostTracker
        {
            private Tile _owner;
            private WeakList<HwndHost> _hosts;

            internal HwndHostTracker(Tile tile)
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
    }

    #endregion //Tile class	
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