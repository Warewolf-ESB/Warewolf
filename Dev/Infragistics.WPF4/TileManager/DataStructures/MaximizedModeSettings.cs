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
using System.Xml;
using Infragistics.Controls.Layouts.Primitives;




namespace Infragistics.Controls.Layouts
{
    /// <summary>
    /// Contains settings that are used to lay out Tiles inside a <see cref="XamTileManager"/> when in maximized mode.
    /// </summary>
    /// <seealso cref="XamTileManager.IsInMaximizedMode"/>
    /// <seealso cref="XamTileManager.NormalModeSettings"/>
     public class MaximizedModeSettings : ModeSettingsBase
    {
        #region Private members
        
        private TileConstraints _maximizedTileConstraints;
        private TileConstraints _minimizedTileConstraints;
        private TileConstraints _minimizedExpandedTileConstraints;

		#endregion //Base class overrides

        #region Constructors

        #endregion //Constructors	
    
        #region Properties

            #region Public Properties

				#region HorizontalTileAreaAlignment

		/// <summary>
		/// Identifies the <see cref="HorizontalTileAreaAlignment"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HorizontalTileAreaAlignmentProperty = DependencyPropertyUtilities.Register("HorizontalTileAreaAlignment",
			typeof(HorizontalAlignment), typeof(MaximizedModeSettings),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.HorizontalAlignmentStretchBox, new PropertyChangedCallback(OnHorizontalTileAreaAlignmentChanged))
			);

		private static void OnHorizontalTileAreaAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CoreUtilities.ValidateEnum(typeof(HorizontalAlignment), e.NewValue);
			
			MaximizedModeSettings instance = d as MaximizedModeSettings;

			instance.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
		}

        /// <summary>
        /// Determines the horizontal alignment of the complete block of visible tiles within the control.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> For <see cref="MaximizedModeSettings"/>, this property applies to the minimized tile area only.</para>
        /// </remarks>
        /// <seealso cref="HorizontalTileAreaAlignmentProperty"/>
       public HorizontalAlignment HorizontalTileAreaAlignment
        {
            get
            {
                return (HorizontalAlignment)this.GetValue(MaximizedModeSettings.HorizontalTileAreaAlignmentProperty);
            }
            set
            {
                this.SetValue(MaximizedModeSettings.HorizontalTileAreaAlignmentProperty, value);
            }
        }

                #endregion //HorizontalTileAreaAlignment

				#region InterTileAreaSpacing

		/// <summary>
		/// Identifies the <see cref="InterTileAreaSpacing"/> dependency property
		/// </summary>
		public static readonly DependencyProperty InterTileAreaSpacingProperty = DependencyPropertyUtilities.Register("InterTileAreaSpacing",
			typeof(double), typeof(MaximizedModeSettings),
			DependencyPropertyUtilities.CreateMetadata(double.NaN, new PropertyChangedCallback(OnSpacingPropertyChanged))
			);

		private static void OnSpacingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ValidateSpacing(e.NewValue);

			MaximizedModeSettings instance = d as MaximizedModeSettings;

			instance.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
		}

        /// <summary>
        /// Gets/sets the amount of spacing between the maximized tile area and the minimized tile area when in maximized mode.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if this property is not set then the <see cref="XamTileManager.InterTileSpacingX"/> or <see cref="XamTileManager.InterTileSpacingY"/> value will be used based on the <see cref="MaximizedTileLocation"/>.</para>
        /// </remarks>
        /// <seealso cref="XamTileManager.InterTileAreaSpacingResolved"/>
        /// <seealso cref="XamTileManager.IsInMaximizedMode"/>
        /// <seealso cref="XamTileManager.MaximizedItems"/>
       public double InterTileAreaSpacing
        {
            get
            {
                return (double)this.GetValue(MaximizedModeSettings.InterTileAreaSpacingProperty);
            }
            set
            {
                this.SetValue(MaximizedModeSettings.InterTileAreaSpacingProperty, value);
            }
        }

                #endregion //InterTileAreaSpacing

				#region InterTileSpacingXMaximized

	   /// <summary>
	   /// Identifies the <see cref="InterTileSpacingXMaximized"/> dependency property
	   /// </summary>
	   public static readonly DependencyProperty InterTileSpacingXMaximizedProperty = DependencyPropertyUtilities.Register("InterTileSpacingXMaximized",
		   typeof(double), typeof(MaximizedModeSettings),
		   DependencyPropertyUtilities.CreateMetadata(double.NaN, new PropertyChangedCallback(OnSpacingPropertyChanged))
		   );

        /// <summary>
        /// Gets/sets the amount of spacing between maximized tiles horizontally.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if this property is not set the <see cref="XamTileManager.InterTileSpacingX"/> setting will be used.</para>
        /// </remarks>
        /// <seealso cref="XamTileManager.InterTileSpacingXMaximizedResolved"/>
        /// <seealso cref="XamTileManager.InterTileSpacingX"/>
        /// <seealso cref="InterTileAreaSpacing"/>
        /// <seealso cref="InterTileSpacingXMinimized"/>
        /// <seealso cref="InterTileSpacingYMinimized"/>
        /// <seealso cref="InterTileSpacingXMaximizedProperty"/>
       public double InterTileSpacingXMaximized
        {
            get
            {
                return (double)this.GetValue(MaximizedModeSettings.InterTileSpacingXMaximizedProperty);
            }
            set
            {
                this.SetValue(MaximizedModeSettings.InterTileSpacingXMaximizedProperty, value);
            }
        }

                #endregion //InterTileSpacingXMaximized

				#region InterTileSpacingXMinimized

	   /// <summary>
	   /// Identifies the <see cref="InterTileSpacingXMinimized"/> dependency property
	   /// </summary>
	   public static readonly DependencyProperty InterTileSpacingXMinimizedProperty = DependencyPropertyUtilities.Register("InterTileSpacingXMinimized",
		   typeof(double), typeof(MaximizedModeSettings),
		   DependencyPropertyUtilities.CreateMetadata(double.NaN, new PropertyChangedCallback(OnSpacingPropertyChanged))
		   );

        /// <summary>
        /// Gets/sets the amount of spacing between minimized tiles horizontally.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if this property is not set the <see cref="XamTileManager.InterTileSpacingX"/> setting will be used.</para>
        /// </remarks>
        /// <seealso cref="XamTileManager.InterTileSpacingXMinimizedResolved"/>
        /// <seealso cref="XamTileManager.InterTileSpacingX"/>
        /// <seealso cref="InterTileSpacingXMaximized"/>
        /// <seealso cref="InterTileSpacingXMinimizedProperty"/>
         public double InterTileSpacingXMinimized
        {
            get
            {
                return (double)this.GetValue(MaximizedModeSettings.InterTileSpacingXMinimizedProperty);
            }
            set
            {
                this.SetValue(MaximizedModeSettings.InterTileSpacingXMinimizedProperty, value);
            }
        }

                #endregion //InterTileSpacingXMinimized

				#region InterTileSpacingYMaximized

		 /// <summary>
		 /// Identifies the <see cref="InterTileSpacingYMaximized"/> dependency property
		 /// </summary>
		 public static readonly DependencyProperty InterTileSpacingYMaximizedProperty = DependencyPropertyUtilities.Register("InterTileSpacingYMaximized",
			 typeof(double), typeof(MaximizedModeSettings),
			 DependencyPropertyUtilities.CreateMetadata(double.NaN, new PropertyChangedCallback(OnSpacingPropertyChanged))
			 );

        /// <summary>
        /// Gets/sets the amount of spacing between maximized tiles vertically.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if this property is not set the <see cref="XamTileManager.InterTileSpacingY"/> setting will be used.</para>
        /// </remarks>
        /// <seealso cref="XamTileManager.InterTileSpacingYMaximizedResolved"/>
        /// <seealso cref="XamTileManager.InterTileSpacingY"/>
        /// <seealso cref="InterTileAreaSpacing"/>
        /// <seealso cref="InterTileSpacingXMinimized"/>
		/// <seealso cref="InterTileSpacingYMinimized"/>
		/// <seealso cref="InterTileSpacingYMaximizedProperty"/>
        public double InterTileSpacingYMaximized
        {
            get
            {
                return (double)this.GetValue(MaximizedModeSettings.InterTileSpacingYMaximizedProperty);
            }
            set
            {
                this.SetValue(MaximizedModeSettings.InterTileSpacingYMaximizedProperty, value);
            }
        }

                #endregion //InterTileSpacingYMaximized

				#region InterTileSpacingYMinimized

		/// <summary>
		/// Identifies the <see cref="InterTileSpacingYMinimized"/> dependency property
		/// </summary>
		public static readonly DependencyProperty InterTileSpacingYMinimizedProperty = DependencyPropertyUtilities.Register("InterTileSpacingYMinimized",
			typeof(double), typeof(MaximizedModeSettings),
			DependencyPropertyUtilities.CreateMetadata(double.NaN, new PropertyChangedCallback(OnSpacingPropertyChanged))
			);

        /// <summary>
        /// Gets/sets the amount of spacing between minimized tiles vertically.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if this property is not set the <see cref="XamTileManager.InterTileSpacingY"/> setting will be used.</para>
        /// </remarks>
        /// <seealso cref="XamTileManager.InterTileSpacingYMinimizedResolved"/>
        /// <seealso cref="XamTileManager.InterTileSpacingY"/>
        /// <seealso cref="InterTileSpacingYMaximized"/>
        /// <seealso cref="InterTileSpacingYMinimizedProperty"/>
        public double InterTileSpacingYMinimized
        {
            get
            {
                return (double)this.GetValue(MaximizedModeSettings.InterTileSpacingYMinimizedProperty);
            }
            set
            {
                this.SetValue(MaximizedModeSettings.InterTileSpacingYMinimizedProperty, value);
            }
        }

                #endregion //InterTileSpacingYMinimized

				#region MaximizedTileLocation



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

         /// <summary>
		/// Identifies the <see cref="MaximizedTileLocation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaximizedTileLocationProperty = DependencyPropertyUtilities.Register("MaximizedTileLocation",
			typeof(MaximizedTileLocation), typeof(MaximizedModeSettings),
			DependencyPropertyUtilities.CreateMetadata(MaximizedTileLocation.Left, new PropertyChangedCallback(OnMaximizedTileLocationChanged))
			);


		private static void OnMaximizedTileLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CoreUtilities.ValidateEnum(typeof(MaximizedTileLocation), e.NewValue);

			MaximizedModeSettings instance = d as MaximizedModeSettings;

			instance.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));

		}

        /// <summary>
        /// Gets/sets where the maximized tiles will be arranged
        /// </summary>
        /// <seealso cref="MaximizedTileLocationProperty"/>
        public MaximizedTileLocation MaximizedTileLocation
        {
            get
            {
                return (MaximizedTileLocation)this.GetValue(MaximizedModeSettings.MaximizedTileLocationProperty);
            }
            set
            {
                this.SetValue(MaximizedModeSettings.MaximizedTileLocationProperty, value);
            }
        }

                #endregion //MaximizedTileLocation

				#region MaximizedTileLayoutOrder

		/// <summary>
		/// Identifies the <see cref="MaximizedTileLayoutOrder"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaximizedTileLayoutOrderProperty = DependencyPropertyUtilities.Register("MaximizedTileLayoutOrder",
			typeof(MaximizedTileLayoutOrder), typeof(MaximizedModeSettings),
			DependencyPropertyUtilities.CreateMetadata(MaximizedTileLayoutOrder.VerticalWithLastTileFill, new PropertyChangedCallback(OnMaximizedTileLayoutOrderChanged))
			);

		private static void OnMaximizedTileLayoutOrderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CoreUtilities.ValidateEnum(typeof(MaximizedTileLayoutOrder), e.NewValue);

			MaximizedModeSettings instance = d as MaximizedModeSettings;

			instance.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
		}

        /// <summary>
        /// Gets/sets how multiple maximized tiles are laid out relative to one another
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note</b>: This property only has meaning if the <see cref="XamTileManager.MaximizedTileLimit"/> is set to a value greater than 1.</para>
        /// </remarks>
        /// <seealso cref="MaximizedTileLayoutOrderProperty"/>
        /// <seealso cref="XamTileManager.MaximizedTileLimit"/>
        public MaximizedTileLayoutOrder MaximizedTileLayoutOrder
        {
            get
            {
                return (MaximizedTileLayoutOrder)this.GetValue(MaximizedModeSettings.MaximizedTileLayoutOrderProperty);
            }
            set
            {
                this.SetValue(MaximizedModeSettings.MaximizedTileLayoutOrderProperty, value);
            }
        }

                #endregion //MaximizedTileLayoutOrder

				#region MaximizedTileConstraints

		/// <summary>
		/// Identifies the <see cref="MaximizedTileConstraints"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaximizedTileConstraintsProperty = DependencyPropertyUtilities.Register("MaximizedTileConstraints",
			typeof(TileConstraints), typeof(MaximizedModeSettings),
			null, new PropertyChangedCallback(OnMaximizedTileConstraintsChanged)
			);

        private static void OnMaximizedTileConstraintsChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            MaximizedModeSettings settings = target as MaximizedModeSettings;

            // unwire the property changed of the old constraints
            if ( settings._maximizedTileConstraints != null )
                settings._maximizedTileConstraints.PropertyChanged -= new PropertyChangedEventHandler(settings.OnMaximizedTileConstraintsPropertyChanged);

            settings._maximizedTileConstraints = e.NewValue as TileConstraints;

            // wire up the property changed of the new constraints
            if ( settings._maximizedTileConstraints != null )
                settings._maximizedTileConstraints.PropertyChanged += new PropertyChangedEventHandler(settings.OnMaximizedTileConstraintsPropertyChanged);
			
			settings.RaisePropertyChangedEvent("MaximizedTileConstraints");
        }


        private void OnMaximizedTileConstraintsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.RaisePropertyChangedEvent("MaximizedTileConstraints");
        }

        /// <summary>
        /// Gets/sets the constraints use for sizing a tile when it's <see cref="XamTile.State"/> is 'Maximized'
        /// </summary>
        /// <seealso cref="MaximizedTileConstraintsProperty"/>
        public TileConstraints MaximizedTileConstraints
        {
            get
            {
                return this._maximizedTileConstraints;
            }
            set
            {
                this.SetValue(MaximizedModeSettings.MaximizedTileConstraintsProperty, value);
            }
        }

                #endregion //MaximizedTileConstraints

				#region MinimizedTileConstraints

		/// <summary>
		/// Identifies the <see cref="MinimizedTileConstraints"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinimizedTileConstraintsProperty = DependencyPropertyUtilities.Register("MinimizedTileConstraints",
			typeof(TileConstraints), typeof(MaximizedModeSettings),
			null, new PropertyChangedCallback(OnMinimizedTileConstraintsChanged)
			);

        private static void OnMinimizedTileConstraintsChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            MaximizedModeSettings settings = target as MaximizedModeSettings;

            // unwire the property changed of the old constraints
            if (settings._minimizedTileConstraints != null)
                settings._minimizedTileConstraints.PropertyChanged -= new PropertyChangedEventHandler(settings.OnMinimizedTileConstraintsPropertyChanged);

            settings._minimizedTileConstraints = e.NewValue as TileConstraints;

            // wire up the property changed of the new constraints
            if (settings._minimizedTileConstraints != null)
                settings._minimizedTileConstraints.PropertyChanged += new PropertyChangedEventHandler(settings.OnMinimizedTileConstraintsPropertyChanged);
            
			settings.RaisePropertyChangedEvent("MinimizedTileConstraints");
        }


        private void OnMinimizedTileConstraintsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.RaisePropertyChangedEvent("MinimizedTileConstraints");
        }

        /// <summary>
        /// Gets/sets the constraints use for sizing a tile when it's <see cref="XamTile.State"/> is 'Minimized'
        /// </summary>
        /// <seealso cref="MinimizedTileConstraintsProperty"/>
        public TileConstraints MinimizedTileConstraints
        {
            get
            {
                return this._minimizedTileConstraints;
            }
            set
            {
                this.SetValue(MaximizedModeSettings.MinimizedTileConstraintsProperty, value);
            }
        }

                #endregion //MinimizedTileConstraints

				#region MinimizedTileExpandButtonVisibility

		/// <summary>
		/// Identifies the <see cref="MinimizedTileExpandButtonVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinimizedTileExpandButtonVisibilityProperty = DependencyPropertyUtilities.Register("MinimizedTileExpandButtonVisibility",
			typeof(Visibility), typeof(MaximizedModeSettings),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.VisibilityVisibleBox, new PropertyChangedCallback(OnMinimizedTileExpandButtonVisibilityChanged))
			);

		private static void OnMinimizedTileExpandButtonVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CoreUtilities.ValidateEnum(typeof(Visibility), e.NewValue);

			MaximizedModeSettings instance = d as MaximizedModeSettings;

			instance.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
		}

        /// <summary>
        /// Gets/sets the default visibility of the expand button in a <see cref="XamTile"/>'s header.
        /// </summary>
        /// <seealso cref="MinimizedTileExpandButtonVisibilityProperty"/>
        /// <seealso cref="MaximizedModeSettings.MinimizedTileExpansionMode"/>
        public Visibility MinimizedTileExpandButtonVisibility
        {
            get
            {
                return (Visibility)this.GetValue(MaximizedModeSettings.MinimizedTileExpandButtonVisibilityProperty);
            }
            set
            {
                this.SetValue(MaximizedModeSettings.MinimizedTileExpandButtonVisibilityProperty, value);
            }
        }

                #endregion //MinimizedTileExpandButtonVisibility

				#region MinimizedExpandedTileConstraints

		/// <summary>
		/// Identifies the <see cref="MinimizedExpandedTileConstraints"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinimizedExpandedTileConstraintsProperty = DependencyPropertyUtilities.Register("MinimizedExpandedTileConstraints",
			typeof(TileConstraints), typeof(MaximizedModeSettings),
			null, new PropertyChangedCallback(OnMinimizedExpandedTileConstraintsChanged)
			);

        private static void OnMinimizedExpandedTileConstraintsChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            MaximizedModeSettings settings = target as MaximizedModeSettings;

            // unwire the property changed of the old constraints
            if (settings._minimizedExpandedTileConstraints != null)
                settings._minimizedExpandedTileConstraints.PropertyChanged -= new PropertyChangedEventHandler(settings.OnMinimizedExpandedTileConstraintsPropertyChanged);

            settings._minimizedExpandedTileConstraints = e.NewValue as TileConstraints;

            // wire up the property changed of the new constraints
            if (settings._minimizedExpandedTileConstraints != null)
                settings._minimizedExpandedTileConstraints.PropertyChanged += new PropertyChangedEventHandler(settings.OnMinimizedExpandedTileConstraintsPropertyChanged);
           
			settings.RaisePropertyChangedEvent("MinimizedExpandedTileConstraints");
        }


        private void OnMinimizedExpandedTileConstraintsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.RaisePropertyChangedEvent("MinimizedExpandedTileConstraints");
        }

        /// <summary>
        /// Gets/sets the constraints use for sizing a tile when it's <see cref="XamTile.State"/> is 'MinimizedExpanded'
        /// </summary>
        /// <seealso cref="MinimizedExpandedTileConstraintsProperty"/>
        public TileConstraints MinimizedExpandedTileConstraints
        {
            get
            {
                return this._minimizedExpandedTileConstraints;
            }
            set
            {
                this.SetValue(MaximizedModeSettings.MinimizedExpandedTileConstraintsProperty, value);
            }
        }

                #endregion //MinimizedExpandedTileConstraints

				#region MinimizedTileExpansionMode

		/// <summary>
		/// Identifies the <see cref="MinimizedTileExpansionMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinimizedTileExpansionModeProperty = DependencyPropertyUtilities.Register("MinimizedTileExpansionMode",
			typeof(MinimizedTileExpansionMode), typeof(MaximizedModeSettings),
			DependencyPropertyUtilities.CreateMetadata(MinimizedTileExpansionMode.AllowMultipleExpandAllInitially, new PropertyChangedCallback(OnMinimizedTileExpansionModeChanged))
			);

		private static void OnMinimizedTileExpansionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CoreUtilities.ValidateEnum(typeof(MinimizedTileExpansionMode), e.NewValue);

			MaximizedModeSettings instance = d as MaximizedModeSettings;

			instance.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));

		}

        /// <summary>
        /// Gets/sets how many minimized tiles may be expanded.
        /// </summary>
        /// <seealso cref="MinimizedTileExpansionModeProperty"/>
        /// <seealso cref="MinimizedTileExpandButtonVisibility"/>
        public MinimizedTileExpansionMode MinimizedTileExpansionMode
        {
            get
            {
                return (MinimizedTileExpansionMode)this.GetValue(MaximizedModeSettings.MinimizedTileExpansionModeProperty);
            }
            set
            {
                this.SetValue(MaximizedModeSettings.MinimizedTileExpansionModeProperty, value);
            }
        }

                #endregion //MinimizedTileExpansionMode

				#region ShowAllMinimizedTiles

		/// <summary>
		/// Identifies the <see cref="ShowAllMinimizedTiles"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShowAllMinimizedTilesProperty = DependencyPropertyUtilities.Register("ShowAllMinimizedTiles",
			typeof(bool), typeof(MaximizedModeSettings),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnShowAllMinimizedTilesChanged))
			);

		private static void OnShowAllMinimizedTilesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			MaximizedModeSettings instance = d as MaximizedModeSettings;

			instance.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
		}

        /// <summary>
        /// Gets/sets whether all minimized tiles are arranged in view
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> even if this property is set to true it is still possible that all the minimized tiles don't fit, e.g. if there isn't enough space to satisfy their minimum size constraints. In this case, scrollbars will appear.</para>
        /// </remarks>
        /// <seealso cref="ShowAllMinimizedTilesProperty"/>
        /// <seealso cref="NormalModeSettings.ShowAllTiles"/>
        /// <seealso cref="NormalModeSettings.TileConstraints"/>
        public bool ShowAllMinimizedTiles
        {
            get
            {
                return (bool)this.GetValue(MaximizedModeSettings.ShowAllMinimizedTilesProperty);
            }
            set
            {
                this.SetValue(MaximizedModeSettings.ShowAllMinimizedTilesProperty, value);
            }
        }

                #endregion //ShowAllMinimizedTiles

				#region ShowTileAreaSplitter







		/// <summary>
		/// Identifies the <see cref="ShowTileAreaSplitter"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShowTileAreaSplitterProperty = DependencyPropertyUtilities.Register("ShowTileAreaSplitter",
			typeof(bool), typeof(MaximizedModeSettings),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnShowTileAreaSplitterChanged))
			);

		private static void OnShowTileAreaSplitterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			MaximizedModeSettings instance = d as MaximizedModeSettings;

			instance.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
		}

        /// <summary>
        /// Gets/sets whether a splitter will be shown between the maximized and minimized tile areas which can be used to control the relative size of the these areas.
        /// </summary>
        /// <seealso cref="ShowTileAreaSplitterProperty"/>
        /// <seealso cref="TileAreaSplitter"/>
        public bool ShowTileAreaSplitter
        {
            get
            {
                return (bool)this.GetValue(MaximizedModeSettings.ShowTileAreaSplitterProperty);
            }
            set
            {
                this.SetValue(MaximizedModeSettings.ShowTileAreaSplitterProperty, value);
            }
        }


                #endregion //ShowTileAreaSplitter

				#region VerticalTileAreaAlignment

		/// <summary>
		/// Identifies the <see cref="VerticalTileAreaAlignment"/> dependency property
		/// </summary>
		public static readonly DependencyProperty VerticalTileAreaAlignmentProperty = DependencyPropertyUtilities.Register("VerticalTileAreaAlignment",
			typeof(VerticalAlignment), typeof(MaximizedModeSettings),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.VerticalAlignmentStretchBox, new PropertyChangedCallback(OnVerticalTileAreaAlignmentChanged))
			);

		private static void OnVerticalTileAreaAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CoreUtilities.ValidateEnum(typeof(VerticalAlignment), e.NewValue);

			MaximizedModeSettings instance = d as MaximizedModeSettings;

			instance.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
		}

        /// <summary>
        /// Determines the vertical alignment of the complete block of visible tiles within the control.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> For <see cref="MaximizedModeSettings"/>, this property applies to the minimized tile area only.</para>
        /// </remarks>
        /// <seealso cref="VerticalTileAreaAlignmentProperty"/>
        public VerticalAlignment VerticalTileAreaAlignment
        {
            get
            {
                return (VerticalAlignment)this.GetValue(MaximizedModeSettings.VerticalTileAreaAlignmentProperty);
            }
            set
            {
                this.SetValue(MaximizedModeSettings.VerticalTileAreaAlignmentProperty, value);
            }
        }

                #endregion //VerticalTileAreaAlignment

            #endregion //Public Properties

            #region Internal Properties

                #region MaximizedTileLocationResolved

        internal MaximizedTileLocation MaximizedTileLocationResolved
        {
            get
            {


#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

                return MaximizedTileLocation;
            }
        }

                #endregion //MaximizedTileLocationResolved

            #endregion //Internal Properties

        #endregion //Properties

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