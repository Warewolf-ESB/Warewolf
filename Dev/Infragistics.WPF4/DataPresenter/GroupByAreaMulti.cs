using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Infragistics.Windows.Helpers;
using System.Windows.Media;
using Infragistics.Windows.Automation.Peers.DataPresenter;
using System.Diagnostics;
using System.Windows.Data;
using Infragistics.Windows.Internal;
using Infragistics.Collections;

namespace Infragistics.Windows.DataPresenter
{
	#region GroupByAreaMulti Class

	/// <summary>
	/// A control used by the <see cref="XamDataPresenter"/>, <see cref="XamDataGrid"/> and <see cref="XamDataCarousel"/> that presents a UI for performing Outlook style Grouping 
	/// across multiple <see cref="FieldLayout"/>s.
	/// </summary>
	/// <remarks>
	/// <p class="body">The <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.GroupByAreaMode"/> property determines whether the XamDataPresenter uses the original <see cref="GroupByArea"/> control as the UI for Outlook style grouping of the
	/// <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DefaultFieldLayout"/> only or whether it uses this GroupByAreaMulti control for Outlook style grouping across multiple <see cref="FieldLayout"/>s.</p>
	/// </remarks>
	/// <seealso cref="GroupByArea"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DefaultFieldLayout"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.GroupByAreaMode"/>
	/// <seealso cref="FieldLayout"/>
    [TemplatePart(Name = "PART_InsertionPoint", Type = typeof(FrameworkElement))]
	[StyleTypedProperty(Property = "FieldLayoutDescriptionTemplate", StyleTargetType = typeof(ContentControl))]
	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CrossBandGrouping, Version = FeatureInfo.Version_9_2)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class GroupByAreaMulti : GroupByAreaBase
	{
        #region Private Members

        private ObservableCollectionExtended<FieldLayoutGroupByInfo> _fieldLayoutInfos;
        private FieldLayoutGroupByInfoCollection _readOnlyFieldLayoutInfos;

        private readonly static object DefaultFieldLayoutOffsetXFull = 24d;
        private readonly static object DefaultFieldLayoutOffsetXCompact = 6d;
        private readonly static object DefaultFieldLayoutOffsetYFull = 2d;
        private readonly static object DefaultFieldLayoutOffsetYCompact = 0d;
        private readonly static object DefaultFieldOffsetX = 6d;
        private readonly static object DefaultFieldOffsetY = 0d;

        #endregion //Private Members	
    
        #region Constructors

        static GroupByAreaMulti()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(GroupByAreaMulti), new FrameworkPropertyMetadata(typeof(GroupByAreaMulti)));
        }

		/// <summary>
		/// Constructor provided to allow creation in design tools for template and style editing.
		/// </summary>
        public GroupByAreaMulti() : base(null)
        {
			this.IsExpanded = true;
        }

        #endregion //Constructors

        #region Base class overrides

            #region InitializeDataPresenter

        internal override void InitializeDataPresenter(DataPresenterBase dataPresenter)
        {
            base.InitializeDataPresenter(dataPresenter);

			if (dataPresenter == null)
			{
				if (this._fieldLayoutInfos != null)
				{
					this._fieldLayoutInfos.Clear();
				}

				BindingOperations.ClearBinding(this, GroupByAreaMulti.OverallSortVersionProperty);
			}
			else
			{
				this.SetBinding(GroupByAreaMulti.OverallSortVersionProperty, Utilities.CreateBindingObject(DataPresenterBase.OverallSortVersionProperty, BindingMode.OneWay, dataPresenter));
			}
        }

            #endregion //InitializeDataPresenter	

            #region OnCreateAutomationPeer
        /// <summary>
        /// Returns an automation peer that exposes the <see cref="GroupByAreaMulti"/> to UI Automation.
        /// </summary>
        /// <returns>A <see cref="Infragistics.Windows.Automation.Peers.DataPresenter.GroupByAreaMultiAutomationPeer"/></returns>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new GroupByAreaMultiAutomationPeer(this);
        }
            #endregion //OnCreateAutomationPeer

			#region OnApplyTemplate
    
		/// <summary>
		/// Called when the template is applied
		/// </summary>
    	public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

            // JJD 9/22/09 - TFS18119 
            // added the calledFromApplyTemplate param
            this.SynchronizeFieldLayouts(true);
		}

   			#endregion //OnApplyTemplate	
    
            #region OnStyleVersionNumberChanged

        internal override void OnStyleVersionNumberChanged()
        {
            base.OnStyleVersionNumberChanged();

            // JJD 9/22/09 - TFS18119 
            // added the calledFromApplyTemplate param
            this.SynchronizeFieldLayouts(false);
        }

            #endregion //OnStyleVersionNumberChanged	

            #region OnVisualParentChanged

        /// <summary>
        /// Called when the visual parent has changed
        /// </summary>
        /// <param name="oldParent"></param>
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

			if (VisualTreeHelper.GetParent(this) == null)
			{
				this.ClearFieldLayoutInfo();
			}
			else
			{
				// JJD 4/26/11 - TFS24326
				// If the groupbyarea is being re-displayed we need to call
				// SynchronizeFieldLayouts since we called ClearFieldLayoutInfo
				// above when the area was hidden
				if (this.IsInitialized)
					this.SynchronizeFieldLayouts(false);
			}
        }

            #endregion //OnVisualParentChanged	

        #endregion //Base class overrides	
    
		#region Properties

            #region Public Properties

                #region ConnectorLinePen

        /// <summary>
        /// Identifies the <see cref="ConnectorLinePen"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ConnectorLinePenProperty = DependencyProperty.Register("ConnectorLinePen",
            typeof(Pen), typeof(GroupByAreaMulti), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets/sets the Pen that will be used for drawing connector lines between fields
        /// </summary>
        /// <seealso cref="ConnectorLinePenProperty"/>
        //[Description("Gets/sets the Pen that will be used for drawing connector lines between fields")]
        //[Category("Appearance")]
        public Pen ConnectorLinePen
        {
            get
            {
                return (Pen)this.GetValue(GroupByAreaMulti.ConnectorLinePenProperty);
            }
            set
            {
                this.SetValue(GroupByAreaMulti.ConnectorLinePenProperty, value);
            }
        }

                #endregion //ConnectorLinePen

				#region FieldLayoutDescriptionTemplate

		/// <summary>
		/// Identifies the <see cref="FieldLayoutDescriptionTemplate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FieldLayoutDescriptionTemplateProperty = DependencyProperty.Register("FieldLayoutDescriptionTemplate",
			typeof(DataTemplate), typeof(GroupByAreaMulti), new FrameworkPropertyMetadata((DataTemplate)null));

		/// <summary>
		/// Returns/sets the DataTemplate to use for the FieldLayout description element.
		/// </summary>
		/// <seealso cref="FieldLayoutDescriptionTemplateProperty"/>
		/// <seealso cref="FieldLayoutDescriptionTemplate"/>
		/// <seealso cref="FieldLayout"/>
		/// <seealso cref="FieldLayout.Description"/>
		//[Description("Returns/sets the DataTemplate to use for the FieldLayout description element.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public DataTemplate FieldLayoutDescriptionTemplate
		{
			get
			{
				return (DataTemplate)this.GetValue(GroupByAreaMulti.FieldLayoutDescriptionTemplateProperty);
			}
			set
			{
				this.SetValue(GroupByAreaMulti.FieldLayoutDescriptionTemplateProperty, value);
			}
		}

				#endregion //FieldLayoutDescriptionTemplate

                #region FieldLayoutGroupByInfos

        /// <summary>
        /// Returns a read-only collection of <see cref="FieldLayoutGroupByInfo"/>s, each representing a specific <see cref="FieldLayout"/>.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [Bindable(true)]
        [ReadOnly(true)]
        public FieldLayoutGroupByInfoCollection FieldLayoutGroupByInfos
        {
            get
            {
                if (this._fieldLayoutInfos == null)
                {
                    this._fieldLayoutInfos = new ObservableCollectionExtended<FieldLayoutGroupByInfo>();
                    this._readOnlyFieldLayoutInfos = new FieldLayoutGroupByInfoCollection(this._fieldLayoutInfos);
                }

                return this._readOnlyFieldLayoutInfos;
            }
        }

                #endregion //FieldLayoutGroupByInfos

                #region FieldLayoutOrientation

        private static readonly DependencyPropertyKey FieldLayoutOrientationPropertyKey =
            DependencyProperty.RegisterReadOnly("FieldLayoutOrientation",
            typeof(Orientation), typeof(GroupByAreaMulti), new FrameworkPropertyMetadata(KnownBoxes.OrientationHorizontalBox));

        /// <summary>
        /// Identifies the <see cref="FieldLayoutOrientation"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FieldLayoutOrientationProperty =
            FieldLayoutOrientationPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets/sets how fieldLayout information is orientated (read-only)
        /// </summary>
        /// <seealso cref="FieldLayoutOrientationProperty"/>
        //[Description("Gets/sets how fieldLayout information is orientated (read-only)")]
        //[Category("Behavior")]
        public Orientation FieldLayoutOrientation
        {
            get
            {
                return (Orientation)this.GetValue(GroupByAreaMulti.FieldLayoutOrientationProperty);
            }
        }

                #endregion //FieldLayoutOrientation

                #region FieldLayoutOffsetX

        /// <summary>
        /// Identifies the <see cref="FieldLayoutOffsetX"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FieldLayoutOffsetXProperty = DependencyProperty.Register("FieldLayoutOffsetX",
            typeof(double), typeof(GroupByAreaMulti), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnFieldLayoutOffsetChanged), new CoerceValueCallback(CoerceFieldLayoutOffsetX)),
            new ValidateValueCallback(ValidateOffsetAllowNegative));

        private static object CoerceFieldLayoutOffsetX(DependencyObject target, object value)
        {
            GroupByAreaMulti gba = target as GroupByAreaMulti;

            if (gba != null )
            {
                if (double.IsNaN((double)value))
                {
                    DataPresenterBase dp = gba.DataPresenter;

                    if (dp != null)
                    {
                        if (dp.GroupByAreaMode == GroupByAreaMode.MultipleFieldLayoutsCompact)
                            return DefaultFieldLayoutOffsetXCompact;
                        else
                            return DefaultFieldLayoutOffsetXFull;
                    }
                }
            }

            return value;
        }

        private static void OnFieldLayoutOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            GroupByAreaMulti gba = target as GroupByAreaMulti;

            if (gba != null)
                gba.CalculateOffsets();
        }

        /// <summary>
        /// Gets or sets the amount to offset between <see cref="FieldLayoutGroupByInfo"/>s in the X dimension.
        /// </summary>
        /// <remarks>
        /// <para class="body">This setting will be used to calculate the correct <see cref="OffsetProperty"/> that will be set of each <see cref="FieldLayoutGroupByInfo"/>. This Offset is bound to the Margin property of the root element of the FieldLayoutGroupByInfo's DataTempate.</para>
        /// <para class="note"><b>Note:</b> only positive values, neagtive values, 0 or NaN are allowed.</para>
        /// </remarks>
        /// <seealso cref="FieldLayoutOffsetXProperty"/>
        /// <exception cref="ArgumentOutOfRangeException">If set to infinity.</exception>
        //[Description("Gets or sets the amount to offset between FieldLayoutGroupByInfos in the X dimension.")]
        //[Category("Appearance")]
        public double FieldLayoutOffsetX
        {
            get
            {
                return (double)this.GetValue(GroupByAreaMulti.FieldLayoutOffsetXProperty);
            }
            set
            {
                this.SetValue(GroupByAreaMulti.FieldLayoutOffsetXProperty, value);
            }
        }

                #endregion //FieldLayoutOffsetX

                #region FieldLayoutOffsetY

        /// <summary>
        /// Identifies the <see cref="FieldLayoutOffsetY"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FieldLayoutOffsetYProperty = DependencyProperty.Register("FieldLayoutOffsetY",
             typeof(double), typeof(GroupByAreaMulti), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnFieldLayoutOffsetChanged), new CoerceValueCallback(CoerceFieldLayoutOffsetY)),
            new ValidateValueCallback(ValidateOffset));


        private static object CoerceFieldLayoutOffsetY(DependencyObject target, object value)
        {
            GroupByAreaMulti gba = target as GroupByAreaMulti;

            if (gba != null )
            {
                if (double.IsNaN((double)value))
                {
                    DataPresenterBase dp = gba.DataPresenter;

                    if (dp != null)
                    {
                        if (dp.GroupByAreaMode == GroupByAreaMode.MultipleFieldLayoutsCompact)
                            return DefaultFieldLayoutOffsetYCompact;
                        else
                            return DefaultFieldLayoutOffsetYFull;
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// Gets or sets the amount to offset between <see cref="FieldLayoutGroupByInfo"/>s in the Y dimension.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> only positive values, 0 or NaN are allowed.</para>
        /// </remarks>
        /// <seealso cref="FieldLayoutOffsetXProperty"/>
        /// <exception cref="ArgumentOutOfRangeException">If set to infinity or a negative value.</exception>
        //[Description("Gets or sets the amount to offset between FieldLayoutGroupByInfos in the Y dimension.")]
        //[Category("Appearance")]
        public double FieldLayoutOffsetY
        {
            get
            {
                return (double)this.GetValue(GroupByAreaMulti.FieldLayoutOffsetYProperty);
            }
            set
            {
                this.SetValue(GroupByAreaMulti.FieldLayoutOffsetYProperty, value);
            }
        }

                #endregion //FieldLayoutOffsetY

                #region FieldOffsetX

        /// <summary>
        /// Identifies the <see cref="FieldOffsetX"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FieldOffsetXProperty = DependencyProperty.Register("FieldOffsetX",
            typeof(double), typeof(GroupByAreaMulti), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnFieldOffsetChanged), new CoerceValueCallback(CoerceFieldOffsetX)),
            new ValidateValueCallback(ValidateOffsetAllowNegative));

        private static object CoerceFieldOffsetX(DependencyObject target, object value)
        {
            GroupByAreaMulti gba = target as GroupByAreaMulti;

            if (gba != null)
            {
                if (double.IsNaN((double)value))
                {
                    return DefaultFieldOffsetX;
                }
            }

            return value;
        }

        private static void OnFieldOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            GroupByAreaMulti gba = target as GroupByAreaMulti;

            if (gba != null)
                gba.CalculateOffsets();
        }

        /// <summary>
        /// Gets or sets the amount to offset between <see cref="Field"/>s within a <see cref="FieldLayout"/> in the X dimension.
        /// </summary>
        /// <remarks>
        /// <para class="body">This setting will be used to calculate the correct <see cref="OffsetProperty"/> that will be set of each <see cref="Field"/>. This Offset is bound to the Margin property of the root element of the Field's DataTempate.</para>
        /// <para class="note"><b>Note:</b> only positive values, negative values, 0 or Nan are allowed.</para>
        /// </remarks>
        /// <seealso cref="FieldOffsetXProperty"/>        
        /// <exception cref="ArgumentOutOfRangeException">If set to infinity.</exception>
        //[Description("Gets or sets the amount to offset between Fields within a FieldLayout in the X dimension.")]
        //[Category("Appearance")]
        public double FieldOffsetX
        {
            get
            {
                return (double)this.GetValue(GroupByAreaMulti.FieldOffsetXProperty);
            }
            set
            {
                this.SetValue(GroupByAreaMulti.FieldOffsetXProperty, value);
            }
        }

                #endregion //FieldOffsetX

                #region FieldOffsetY

        /// <summary>
        /// Identifies the <see cref="FieldOffsetY"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FieldOffsetYProperty = DependencyProperty.Register("FieldOffsetY",
            typeof(double), typeof(GroupByAreaMulti), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnFieldOffsetChanged), new CoerceValueCallback(CoerceFieldOffsetY)),
            new ValidateValueCallback(ValidateOffset));

        private static object CoerceFieldOffsetY(DependencyObject target, object value)
        {
            GroupByAreaMulti gba = target as GroupByAreaMulti;

            if (gba != null)
            {
                if (double.IsNaN((double)value))
                {
                    return DefaultFieldOffsetY;
                }
            }

            return value;
        }

        /// <summary>
        /// Gets or sets the amount to offset between <see cref="Field"/>s within a <see cref="FieldLayout"/> in the Y dimension.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> only positive values, 0 or NaN are allowed.</para>
        /// </remarks>
        /// <seealso cref="FieldOffsetXProperty"/>
        /// <exception cref="ArgumentOutOfRangeException">If set to infinity or a negative value.</exception>
        //[Description("Gets or sets the amount to offset between Fields within a FieldLayout in the Y dimension.")]
        //[Category("Appearance")]
        public double FieldOffsetY
        {
            get
            {
                return (double)this.GetValue(GroupByAreaMulti.FieldOffsetYProperty);
            }
            set
            {
                this.SetValue(GroupByAreaMulti.FieldOffsetYProperty, value);
            }
        }

                #endregion //FieldOffsetY

                #region HasOffsetX

        private static readonly DependencyPropertyKey HasOffsetXPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("HasOffsetX",
            typeof(bool), typeof(GroupByAreaMulti), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        /// <summary>
        /// Identifies the HasOffsetX" attached readonly dependency property
        /// </summary>
        /// <remarks>
        /// <para class="body">This property is useful to trigger off in both the template for <see cref="Field"/>s and <see cref="FieldLayoutGroupByInfo"/>s. 
        /// </para>
        /// </remarks>
        /// <seealso cref="GetHasOffsetX(DependencyObject)"/>
        /// <seealso cref="GetHasOffsetY(DependencyObject)"/>
        /// <seealso cref="HasOffsetYProperty"/>
        public static readonly DependencyProperty HasOffsetXProperty =
            HasOffsetXPropertyKey.DependencyProperty;


        /// <summary>
        /// Gets the value of the 'HasOffsetX' attached readonly property
        /// </summary>
        /// <remarks>
        /// <para class="body">This property is useful to trigger off in both the template for <see cref="Field"/>s and <see cref="FieldLayoutGroupByInfo"/>s. 
        /// </para>
        /// </remarks>
        /// <seealso cref="HasOffsetXProperty"/>
        /// <seealso cref="GetHasOffsetY(DependencyObject)"/>
        public static bool GetHasOffsetX(DependencyObject d)
        {
            return (bool)d.GetValue(GroupByAreaMulti.HasOffsetXProperty);
        }

                #endregion //HasOffsetX

                #region HasOffsetY

        private static readonly DependencyPropertyKey HasOffsetYPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("HasOffsetY",
            typeof(bool), typeof(GroupByAreaMulti), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        /// <summary>
        /// Identifies the HasOffsetY" attached readonly dependency property
        /// </summary>
        /// <remarks>
        /// <para class="body">This property is useful to trigger off in both the template for <see cref="Field"/>s and <see cref="FieldLayoutGroupByInfo"/>s. 
        /// </para>
        /// </remarks>
        /// <seealso cref="GetHasOffsetY(DependencyObject)"/>
        /// <seealso cref="GetHasOffsetX(DependencyObject)"/>
        /// <seealso cref="HasOffsetXProperty"/>
        public static readonly DependencyProperty HasOffsetYProperty =
            HasOffsetYPropertyKey.DependencyProperty;


        /// <summary>
        /// Gets the value of the 'HasOffsetY' attached readonly property
        /// </summary>
        /// <remarks>
        /// <para class="body">This property is useful to trigger off in both the template for <see cref="Field"/>s and <see cref="FieldLayoutGroupByInfo"/>s. 
        /// </para>
        /// </remarks>
        /// <seealso cref="HasOffsetYProperty"/>
        /// <seealso cref="GetHasOffsetX(DependencyObject)"/>
        public static bool GetHasOffsetY(DependencyObject d)
        {
            return (bool)d.GetValue(GroupByAreaMulti.HasOffsetYProperty);
        }

                #endregion //HasOffsetY

                #region IsConnectorLineTarget

        /// <summary>
        /// Identifies the IsConnectorLineTarget attached dependency property which is used to identify the element within the visual tree of the <see cref="LabelPresenter"/> and/or the <see cref="FieldLayoutGroupByInfo"/> DataTemplate where the connector lines will be drawn to or from.
        /// </summary>
        /// <seealso cref="ConnectorLinePen"/>
        /// <seealso cref="GetIsConnectorLineTarget"/>
        /// <seealso cref="SetIsConnectorLineTarget"/>
        public static readonly DependencyProperty IsConnectorLineTargetProperty = DependencyProperty.RegisterAttached("IsConnectorLineTarget",
            typeof(bool), typeof(GroupByAreaMulti), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        /// <summary>
        /// Gets the value of the 'IsConnectorLineTarget' attached property which is used to identify the element within the visual tree of the <see cref="LabelPresenter"/> and/or the <see cref="FieldLayoutGroupByInfo"/> DataTemplate where the connector lines will be drawn to or from.
        /// </summary>
        /// <seealso cref="ConnectorLinePen"/>
        /// <seealso cref="IsConnectorLineTargetProperty"/>
        /// <seealso cref="SetIsConnectorLineTarget"/>
        public static bool GetIsConnectorLineTarget(DependencyObject d)
        {
            return (bool)d.GetValue(GroupByAreaMulti.IsConnectorLineTargetProperty);
        }

        /// <summary>
        /// Sets the value of the 'IsConnectorLineTarget' attached property
        /// </summary>
        /// <seealso cref="IsConnectorLineTargetProperty"/>
        /// <seealso cref="GetIsConnectorLineTarget"/>
        public static void SetIsConnectorLineTarget(DependencyObject d, bool value)
        {
            d.SetValue(GroupByAreaMulti.IsConnectorLineTargetProperty, value);
        }

                #endregion //IsConnectorLineTarget

                #region IsFirstInList

        internal static readonly DependencyPropertyKey IsFirstInListPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("IsFirstInList",
            typeof(bool), typeof(GroupByAreaMulti), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        /// <summary>
        /// Identifies the IsFirstInList" attached readonly dependency property
        /// </summary>
        /// <remarks>
        /// <para class="body">This property is useful to trigger off in both the template for <see cref="Field"/>s and <see cref="FieldLayoutGroupByInfo"/>s. 
        /// </para>
        /// </remarks>
        /// <seealso cref="GetIsFirstInList(DependencyObject)"/>
        /// <seealso cref="GetIsLastInList(DependencyObject)"/>
        /// <seealso cref="IsLastInListProperty"/>
        public static readonly DependencyProperty IsFirstInListProperty =
            IsFirstInListPropertyKey.DependencyProperty;


        /// <summary>
        /// Gets the value of the 'IsFirstInList' attached readonly property
        /// </summary>
        /// <remarks>
        /// <para class="body">This property is useful to trigger off in both the template for <see cref="Field"/>s and <see cref="FieldLayoutGroupByInfo"/>s. 
        /// </para>
        /// </remarks>
        /// <seealso cref="IsFirstInListProperty"/>
        /// <seealso cref="IsLastInListProperty"/>
        /// <seealso cref="GetIsLastInList(DependencyObject)"/>
        public static bool GetIsFirstInList(DependencyObject d)
        {
            return (bool)d.GetValue(GroupByAreaMulti.IsFirstInListProperty);
        }

                #endregion //IsFirstInList

                #region IsLastInList

        internal static readonly DependencyPropertyKey IsLastInListPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("IsLastInList",
            typeof(bool), typeof(GroupByAreaMulti), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        /// <summary>
        /// Identifies the IsLastInList" attached readonly dependency property
        /// </summary>
        /// <remarks>
        /// <para class="body">This property is useful to trigger off in both the template for <see cref="Field"/>s and <see cref="FieldLayoutGroupByInfo"/>s. 
        /// </para>
        /// </remarks>
        /// <seealso cref="GetIsLastInList(DependencyObject)"/>
        /// <seealso cref="GetIsFirstInList(DependencyObject)"/>
        /// <seealso cref="IsFirstInListProperty"/>
        public static readonly DependencyProperty IsLastInListProperty =
            IsLastInListPropertyKey.DependencyProperty;


        /// <summary>
        /// Gets the value of the 'IsLastInList' attached readonly property
        /// </summary>
        /// <remarks>
        /// <para class="body">This property is useful to trigger off in both the template for <see cref="Field"/>s and <see cref="FieldLayoutGroupByInfo"/>s. 
        /// </para>
        /// </remarks>
        /// <seealso cref="IsLastInListProperty"/>
        /// <seealso cref="GetIsFirstInList(DependencyObject)"/>
        public static bool GetIsLastInList(DependencyObject d)
        {
            return (bool)d.GetValue(GroupByAreaMulti.IsLastInListProperty);
        }

                #endregion //IsLastInList

                #region Offset

        private static readonly DependencyPropertyKey OffsetPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("Offset",
            typeof(Thickness), typeof(GroupByAreaMulti), new FrameworkPropertyMetadata(new Thickness()));

        /// <summary>
        /// Identifies the Offset" attached readonly dependency property
        /// </summary>
        /// <remarks>
        /// <para class="body">This property is used to bind the Margin property in the DataTemplates for <see cref="FieldLayoutGroupByInfo"/> and <see cref="Field"/>. 
        /// </para>
        /// <para class="body">For <b>FieldLayoutGroupByInfo</b>s it is calculated based on the <see cref="FieldLayoutOffsetX"/> and <see cref="FieldLayoutOffsetY"/> settings.</para>
        /// <para class="body">For <b>Field</b>s it is calculated based on the <see cref="FieldOffsetX"/> and <see cref="FieldOffsetY"/> settings.</para>
        /// </remarks>
        /// <seealso cref="GetOffset"/>
        /// <seealso cref="FieldOffsetX"/>
        /// <seealso cref="FieldOffsetY"/>
        /// <seealso cref="FieldLayoutOffsetX"/>
        /// <seealso cref="FieldLayoutOffsetY"/>
        public static readonly DependencyProperty OffsetProperty =
            OffsetPropertyKey.DependencyProperty;


        /// <summary>
        /// Gets the value of the 'Offset' attached readonly property
        /// </summary>
        /// <remarks>
        /// <para class="body">This property is used to bind the Margin property in the DataTemplates for <see cref="FieldLayoutGroupByInfo"/> and <see cref="Field"/>. 
        /// </para>
        /// <para class="body">For <b>FieldLayoutGroupByInfo</b>s it is calculated based on the <see cref="FieldLayoutOffsetX"/> and <see cref="FieldLayoutOffsetY"/> settings.</para>
        /// <para class="body">For <b>Field</b>s it is calculated based on the <see cref="FieldOffsetX"/> and <see cref="FieldOffsetY"/> settings.</para>
        /// </remarks>
        /// <seealso cref="OffsetProperty"/>
        /// <seealso cref="FieldLayoutOffsetX"/>
        /// <seealso cref="FieldLayoutOffsetY"/>
        /// <seealso cref="FieldOffsetX"/>
        /// <seealso cref="FieldOffsetY"/>
        public static Thickness GetOffset(DependencyObject d)
        {
            return (Thickness)d.GetValue(GroupByAreaMulti.OffsetProperty);
        }

                #endregion //Offset

            #endregion //Public Properties	
       
			#region Internal Properties

				#region OverallSortVersion

		internal static readonly DependencyProperty OverallSortVersionProperty = DependencyProperty.Register("OverallSortVersion",
			typeof(int), typeof(GroupByAreaMulti), new FrameworkPropertyMetadata((int)0, new PropertyChangedCallback(OnOverallSortVersionChanged)));

		private static void OnOverallSortVersionChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
            // JJD 9/22/09 - TFS18119 
            // added the calledFromApplyTemplate param
            ((GroupByAreaMulti)target).SynchronizeFieldLayouts(false);
		}

				#endregion //OverallSortVersion

			#endregion //Internal Properties

		#endregion //Properties

		#region Methods

		    #region Internal Methods

		    #endregion //Internal Methods

		    #region Private Methods

		        #region CalculateOffsets

		private void CalculateOffsets()
        {
            DataPresenterBase dp = this.DataPresenter;

            if (dp == null)
                return;

            int count = this._fieldLayoutInfos != null ? this._fieldLayoutInfos.Count : 0;

            if (count == 0)
                return;

            bool isVertical = this.FieldLayoutOrientation == Orientation.Vertical;

            this.CoerceValue(FieldLayoutOffsetXProperty);
            this.CoerceValue(FieldLayoutOffsetYProperty);
            this.CoerceValue(FieldOffsetXProperty);
            this.CoerceValue(FieldOffsetYProperty);

            double fieldOffsetX = this.FieldOffsetX;
            double fieldOffsetY = this.FieldOffsetY;
            double fieldLayoutOffsetX = this.FieldLayoutOffsetX;
            double fieldLayoutOffsetY = this.FieldLayoutOffsetY;
            
            bool hasFieldOffsetX = GetHasOffset( fieldOffsetX );
            bool hasFieldOffsetY = GetHasOffset( fieldOffsetY );
            bool hasFieldLayoutOffsetX = GetHasOffset( fieldLayoutOffsetX );
            bool hasFieldLayoutOffsetY = GetHasOffset( fieldLayoutOffsetY );
            
            double cumulativeFieldLayoutOffsetY = 0;

            // Loop over all the FieldLayoutGroupByInfo to initialize their margins 
            for (int i = 0; i < count; i++)
            {
                FieldLayoutGroupByInfo gbi = this._fieldLayoutInfos[i];

                // determine the xoffset for the fieldlayout 
                double offsetX = i == 0
                        ? 0
                        : isVertical
                            ? fieldLayoutOffsetX * gbi.FieldLayout.NestingDepth
                            : fieldLayoutOffsetX;

                double offsetY = cumulativeFieldLayoutOffsetY;

                if (i > 0)
                    offsetY += fieldLayoutOffsetY;

                // set the Offset property
                gbi.SetValue(OffsetPropertyKey, new Thickness(offsetX, offsetY, 0, 0));

                // set the IsFirstInList attached proprty
                if (i == 0)
                    gbi.SetValue(IsFirstInListPropertyKey, KnownBoxes.TrueBox);
                else
                    gbi.ClearValue(IsFirstInListPropertyKey);

                // set the IsLastInList attached proprty
                if ( i == count - 1 )
                    gbi.SetValue(IsLastInListPropertyKey, KnownBoxes.TrueBox);
                else
                    gbi.ClearValue(IsLastInListPropertyKey);

                // set the HasOffsetX property
                if (hasFieldLayoutOffsetX)
                    gbi.SetValue(HasOffsetXPropertyKey, KnownBoxes.TrueBox);
                else
                    gbi.ClearValue(HasOffsetXPropertyKey);

                // set the HasOffsetY property
                if (hasFieldLayoutOffsetY)
                    gbi.SetValue(HasOffsetYPropertyKey, KnownBoxes.TrueBox);
                else
                    gbi.ClearValue(HasOffsetYPropertyKey);

                // in compact mode we want to keep track of the cumulative Y offset to use
                // for the indent margin
                if (!isVertical)
                    cumulativeFieldLayoutOffsetY = offsetY;

                offsetX = fieldOffsetX;
                offsetY = 0;

                int fieldCount = gbi.GroupByFields.Count;

                // loop over the fields setting their offsets
                for(int j = 0; j < fieldCount; j++)
                {
                    Field field = gbi.GroupByFields[j];

                    // set the Offset property
                    field.SetValue(OffsetPropertyKey, new Thickness(offsetX, offsetY, 0, 0));

                    // set the HasOffsetX property
                    if (hasFieldOffsetX)
                        field.SetValue(HasOffsetXPropertyKey, KnownBoxes.TrueBox);
                    else
                        field.ClearValue(HasOffsetXPropertyKey);

                    // set the HasOffsetY property
                    if (hasFieldOffsetY)
                        field.SetValue(HasOffsetYPropertyKey, KnownBoxes.TrueBox);
                    else
                        field.ClearValue(HasOffsetYPropertyKey);

                    // increment the Y offset since it must be cumulative
                    offsetY += fieldOffsetY;
                    offsetX = fieldOffsetX;
                }

            }
        }

                #endregion //CalculateOffsets
    
                #region ClearFieldLayoutInfo

        private void ClearFieldLayoutInfo()
        {
            if (this._fieldLayoutInfos == null || this._fieldLayoutInfos.Count == 0)
                return;

            foreach (FieldLayoutGroupByInfo fli in this._fieldLayoutInfos)
                fli.InitializeFieldLayout(null, false);

            this._fieldLayoutInfos.Clear();

        }

                #endregion //ClearFieldLayoutInfo	
  
                #region CompareFieldLayouts

        private static int CompareFieldLayouts(FieldLayout x, FieldLayout y)
        {
            if (x == y)
                return 0;

            if (x == null)
                return -1;

            if (y == null)
                return 1;

            FieldLayout rootX = x.RootFieldLayout;
            FieldLayout rootY = y.RootFieldLayout;

            // if the root layouts don't match then compare the root indices
            if (rootX != rootY)
            {
                // Compare fieldlayouts by their index which would normally
                // be by the order that the records were encountered. This is
                // also something that can be controlled by the application developer
                return rootX.Index < rootY.Index ? -1 : 1;
            }

			// If both field layouts have the same root, and one of them IS the root, then it sorts first.
			if (x == rootX)
				return -1;
			if (y == rootY)
				return 1;

            int originalNestingDepthX, nestingDepthX;
            int originalNestingDepthY, nestingDepthY;
            
            originalNestingDepthX = nestingDepthX = x.NestingDepth;
            originalNestingDepthY = nestingDepthY = y.NestingDepth;

            // initalize the sibling layout stack variables to the x and y layouts 
            FieldLayout siblingLayoutX = x;
            FieldLayout siblingLayoutY = y;

            // walk up the parent chain looking for a common ancestor (which defines sibling layouts)
            while (siblingLayoutY != null &&
                    siblingLayoutX != null &&
                    siblingLayoutX.ParentFieldLayout != siblingLayoutY.ParentFieldLayout)
            {
                if (nestingDepthX < nestingDepthY)
                {
                    siblingLayoutY = siblingLayoutY.ParentFieldLayout;
                    nestingDepthY--;
                }
                else
                {
                    siblingLayoutX = siblingLayoutX.ParentFieldLayout;
                    nestingDepthX--;
                }
            }

            Debug.Assert(siblingLayoutY != null, "The ancestor layout should not be null");
            Debug.Assert(siblingLayoutY.ParentFieldLayout == siblingLayoutX.ParentFieldLayout, "We should have a common parent at this point");
            Debug.Assert(siblingLayoutY.ParentFieldLayout != null, "The common parent should not be null");

            if (siblingLayoutX == null)
                return -1;

            if (siblingLayoutY == null)
                return 1;

            // get the parent fields of the sibling layouts
            Field parentFieldX = siblingLayoutX.ParentField;
            Field parentFieldY = siblingLayoutY.ParentField;

            // if we don't have either one of them or if they both
            // have the same parent field then compare by the sibling layout indices
            if (parentFieldX == null ||
                parentFieldY == null ||
                parentFieldX == parentFieldY)
            {
                if (originalNestingDepthX != originalNestingDepthY)
                    return originalNestingDepthX < originalNestingDepthY ? -1 : 1;

                return siblingLayoutX.Index < siblingLayoutY.Index ? -1 : 1;
            }

            return parentFieldX.Index < parentFieldY.Index ? -1 : 1;

        }

                #endregion //CompareFieldLayouts	

                #region GetHasOffset

        private static bool GetHasOffset(double value)
        {
            return (value < -0.5d || value > 0.5d);
        }

                #endregion //GetHasOffset	
    
				#region SynchronizeFieldLayouts

        // JJD 9/22/09 - TFS18119 
        // added the calledFromApplyTemplate param
		private void SynchronizeFieldLayouts(bool calledFromApplyTemplate)
        {
            DataPresenterBase dp = this.DataPresenter;

            if (dp == null|| !dp.IsInitialized)
                return;

            switch (dp.GroupByAreaMode)
            {
                case GroupByAreaMode.DefaultFieldLayoutOnly:
                    this.ClearFieldLayoutInfo();
                    return;

                case GroupByAreaMode.MultipleFieldLayoutsCompact:
                    this.SetValue(FieldLayoutOrientationPropertyKey, KnownBoxes.OrientationHorizontalBox);
                    break;

                case GroupByAreaMode.MultipleFieldLayoutsFull:
                default:
                    this.SetValue(FieldLayoutOrientationPropertyKey, KnownBoxes.OrientationVerticalBox);
                    break;
            }

            bool isCompactMode = dp.GroupByAreaMode == GroupByAreaMode.MultipleFieldLayoutsCompact;

            List<FieldLayout> fieldLayouts = new List<FieldLayout>();

            FieldLayout defaultFieldLayout = dp.DefaultFieldLayout;

            foreach (FieldLayout fl in dp.FieldLayouts)
            {
                bool addFieldLayout = false;

                if (fl.ParentFieldLayoutKey == null)
                {
                    // for root level bands we only add ones where we have encountered a record
                    // or if it is the default layout.
                    addFieldLayout = (fl.IsInitialRecordLoaded || fl == defaultFieldLayout);
                }
                else
                {
                    bool haveGroupFieldsBeenEncountered = fl.HasGroupBySortFields;

                    FieldLayout ancestorLayout = fl.ParentFieldLayout;

                    // Only allocate the ancestor fieldlayout when we
                    // encounter a fieldlayout that has groupby fields either on this
                    // fieldlayout or anywhere up its ancestor chain
                    Stack<FieldLayout> ancestorsToAdd = haveGroupFieldsBeenEncountered ? new Stack<FieldLayout>() : null;

                    while (ancestorLayout != null)
                    {
                        if (haveGroupFieldsBeenEncountered == false)
                        {
                            haveGroupFieldsBeenEncountered = ancestorLayout.HasGroupBySortFields;
                            ancestorsToAdd = new Stack<FieldLayout>();
                        }

                        if (haveGroupFieldsBeenEncountered)
                            ancestorsToAdd.Push(ancestorLayout);

                        FieldLayout grandparent = ancestorLayout.ParentFieldLayout;

                        if (grandparent == null)
                        {
                            // Only add this fieldlayout if its root level ancestor
                            // has a record loaded and its ParentFieldLayoutKey is null or
                            // if it is the default layout
                            addFieldLayout = ancestorLayout == defaultFieldLayout ||
                                            (ancestorLayout.IsInitialRecordLoaded &&
                                             ancestorLayout.ParentFieldLayoutKey == null);
                            break;
                        }

                        // walk up the ancestor chain
                        ancestorLayout = grandparent;
                    }

                    // only add ancestor layouts if this field layout is an add potential
                    // and we are not in compact mode
                    if (addFieldLayout == true && ancestorsToAdd != null && !isCompactMode)
                    {
                        while (ancestorsToAdd.Count > 0)
                        {
                            ancestorLayout = ancestorsToAdd.Pop();

                            // only add field layouts that are not already in the list
                            if (!fieldLayouts.Contains(ancestorLayout))
                                fieldLayouts.Add(ancestorLayout);
                        }
                    }
                }

                // even if the addFieldLayout flag is set only add field layouts 
                // that have group by fields and that are not already in the list
                if (addFieldLayout == true &&
                    fl.HasGroupBySortFields &&
                    !fieldLayouts.Contains(fl))
                {
                    fieldLayouts.Add(fl);
                }
            }

            int count = fieldLayouts.Count;

            int oldCount = this.FieldLayoutGroupByInfos.Count;

            // sort the fieldlayout collection
            fieldLayouts.Sort(CompareFieldLayouts);

            if (count == oldCount)
            {
                bool areFieldLayoutsTheSame = true;

                for (int i = 0; i < count; i++)
                {
                    if (fieldLayouts[i] != this._fieldLayoutInfos[i].FieldLayout)
                    {
                        areFieldLayoutsTheSame = false;
                        break;
                    }
                }

                if (areFieldLayoutsTheSame)
                {
                    // makes sure the field collections are in sync before calculating 
                    // the offsets
                    foreach (FieldLayoutGroupByInfo info in this.FieldLayoutGroupByInfos)
                        info.SynchronizeGroupByFields(false);

                    this.CalculateOffsets();

                    // JJD 9/22/09 - TFS18119 
                    // check the calledFromApplyTemplate param 
                    // if true then hiide the prompts if there are no grouped field
                    if (calledFromApplyTemplate)
                    {
                        if (oldCount != 0)
                            this.OnHidePrompts();
                    }
                    return;
                }
            }

            FieldLayoutGroupByInfo[] oldLayoutInfos = oldCount > 0 ? new FieldLayoutGroupByInfo[oldCount] : null;

            if ( oldLayoutInfos != null )
                this._fieldLayoutInfos.CopyTo(oldLayoutInfos, 0);

            this._fieldLayoutInfos.BeginUpdate();

            this._fieldLayoutInfos.Clear();

            for (int i = 0; i < count; i++)
            {
                FieldLayout fl = fieldLayouts[i];

                FieldLayoutGroupByInfo fli = null;

                for (int j = 0; j < oldCount; j++)
                {
                    FieldLayoutGroupByInfo fliOld = oldLayoutInfos[j];

                    if (fliOld != null && fl == fliOld.FieldLayout)
                    {
                        fli = fliOld;

                        oldLayoutInfos[j] = null;
                        break;
                    }
                }

                if (fli == null)
                {
                    fli = new FieldLayoutGroupByInfo();
					BindingOperations.SetBinding(fli, 
												FieldLayoutGroupByInfo.FieldLayoutDescriptionTemplateProperty, 
												Utilities.CreateBindingObject(GroupByAreaMulti.FieldLayoutDescriptionTemplateProperty, BindingMode.OneWay, this));
                }

                fli.InitializeFieldLayout(fl, count > 1 || fl.ParentFieldLayout != null);

                this._fieldLayoutInfos.Add(fli);
            }

            this.CalculateOffsets();

            this._fieldLayoutInfos.EndUpdate();

            // JJD 9/22/09 - TFS18119 
            // check the calledFromApplyTemplate param 
            // if true then allways show or hide the prompts based on the
            // new count (ignoring the old count)
            if (calledFromApplyTemplate)
            {
                if (this._fieldLayoutInfos.Count == 0)
                    this.OnShowPrompts();
                else
                    this.OnHidePrompts();
            }
            else
            {
                // if the count is going to or from zero then raise
                // the appropriate event to show/hide the prompts
                if (oldCount == 0)
                {
                    if (this._fieldLayoutInfos.Count > 0)
                        this.OnHidePrompts();
                }
                else
                {
                    if (this._fieldLayoutInfos.Count == 0)
                        this.OnShowPrompts();
                }
            }
		}

                #endregion //SynchronizeFieldLayouts

                #region ValidateOffset

                private static bool ValidateOffset(object value)
                {
                    if (!(value is double))
                        return false;

                    double dval = (double)value;

                    if (double.IsNaN(dval))
                        return true;

                    if (double.IsInfinity(dval) ||
                        double.IsPositiveInfinity(dval) ||
                        double.IsNegativeInfinity(dval))
                        return false;

                    // Call ValidateNonNegative which will throw an exception if
                    // the value is negative
                    GridUtilities.ValidateNonNegative(dval);

                    return true;
                }

                #endregion //ValidateOffset

                #region ValidateOffsetAllowNegative

                private static bool ValidateOffsetAllowNegative(object value)
                {
                    if (!(value is double))
                        return false;

                    double dval = (double)value;

                    if (double.IsNaN(dval))
                        return true;

                    if (double.IsInfinity(dval) ||
                        double.IsPositiveInfinity(dval) ||
                        double.IsNegativeInfinity(dval))
                        return false;

                    return true;
                }

                #endregion //ValidateOffsetAllowNegative

            #endregion //Private Methods

        #endregion //Methods

    }

	#endregion //GroupByAreaMulti Class
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