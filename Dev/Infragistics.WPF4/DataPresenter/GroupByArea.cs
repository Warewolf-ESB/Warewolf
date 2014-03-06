using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Text;
using System.Diagnostics;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Runtime.CompilerServices;
using Infragistics.Windows.Themes;
using Infragistics.Windows.DataPresenter.Events;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.DataPresenter;
using Infragistics.Windows.Helpers;
using System.Windows.Threading;

namespace Infragistics.Windows.DataPresenter
{
    #region GroupByArea Class

    /// <summary>
	/// A control used by the <see cref="XamDataPresenter"/>, <see cref="XamDataGrid"/> and <see cref="XamDataCarousel"/> for managing and displaying a list of fields that 
    /// are available for grouping and a list of fields that are already grouped.  It also provides support for managing the contents of the lists as fields are dragged and
    /// dropped between the lists.
    /// </summary>
	/// <remarks>
	/// <p class="body">The default style for the GroupByArea provides a UI for grouping data records based on <see cref="Field"/>s defined in the <see cref="DataPresenterBase.DefaultFieldLayout"/>.  Since there is only one 
	/// <see cref="DataPresenterBase.DefaultFieldLayout"/>, the GroupByArea can only provide a grouping UI for that <see cref="FieldLayout"/>.  By default, the <see cref="DataPresenterBase.DefaultFieldLayout"/>
    /// is set to the FieldLayout associated with the first DataRecord encountered.  When bound to flat homogeneous data this is not an issue since there is only one <see cref="FieldLayout"/>.  Keep in mind that if
	/// the control is bound to non-homogeneous data or hierarchical data the grouping UI will only show the <see cref="Field"/>s from the <see cref="DataPresenterBase.DefaultFieldLayout"/>.</p>
    /// <p class="body">The GroupByArea keeps track of its expanded and collapsed state and exposes an IsExpanded property that styles can trigger of off to collapse the elements in the style's Template.
    /// The default style for the GroupByArea contains a Template that exposes a UI for expanding and collapsing the control.</p>
    /// <p class="note"><b>Note: </b>The GroupByArea contains the following Template Parts (Template Parts are elements with specific names and types that the control expects to
    /// find in its visual tree.  They are essential to the operation of the control.  If you replace the control's Template you should be sure to include elements with the required
    /// names and types):
    ///		<ul>
    ///			<li>Name: PART_AvailableFieldLabelsArea  Type: FrameworkElement - Specifies the area where the <see cref="AvailableFieldLabels"/> list resides</li>
    ///			<li>Name: PART_GroupedFieldLabelsArea  Type: FrameworkElement - Specifies the area where the <see cref="GroupedFieldLabels"/> list resides</li>
    ///			<li>Name: PART_InsertionPoint  Type: FrameworkElement - Specifies the element that marks the insertion point when <see cref="GroupByAreaFieldLabel"/>s are dragged and dropped</li>
    ///		</ul>
    /// </p>
	/// <p class="note"><b>Note: </b>The GroupByArea is automatically created by the <see cref="XamDataPresenter"/>, <see cref="XamDataGrid"/> and <see cref="XamDataCarousel"/> controls when needed.
	/// It is not intended for use outside of these controls and you should never need to create it directly.</p>
	/// <p class="body">Refer to the <a href="xamDataPresenter_About_Grouping.html">About Grouping</a> topic in the Developer's Guide for an explanation of how this property is used.</p>
	/// </remarks>
	/// <seealso cref="DataPresenterBase.DefaultFieldLayout"/>
	/// <seealso cref="Field"/>
	/// <seealso cref="XamDataPresenter"/>
	/// <seealso cref="XamDataGrid"/>
	/// <seealso cref="XamDataCarousel"/>
	/// <seealso cref="DataPresenterBase.GroupByArea"/>
	[TemplatePart(Name = "PART_AvailableFieldLabelsArea", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_GroupedFieldLabelsArea", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_InsertionPoint", Type = typeof(FrameworkElement))]
	//[Description("A control used by the XamDataPresenter, XamDataGrid and XamDataCarousel that manages and displays a list of fields that are available for grouping and a list of fields that are already grouped.  It also provides support for dragging and dropping fields between the lists.")]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class GroupByArea : GroupByAreaBase
    {
        #region Member Variables

        private FrameworkElement    _groupedFieldLabelsArea;
        private FrameworkElement    _availableFieldLabelsArea;
		private FieldLayout			_fieldLayoutWired;

        private int                 _currentFieldLabelDragElementInsertAtIndex = -1;
        private Vector              _currentFieldLabelDragElementStartOffset;
        private bool                _isFieldLabelDragOverGroupedListArea;
        private bool                _isFieldLabelDragOverAvailableListArea;

        private bool                _ignoreSortedFieldsChange;
        private int                 _lastGroupedFieldsCount;

		// AS 11/29/10 TFS60418
		private DispatcherOperation _reinitializeOperation;

		#endregion Member Variables

        #region Constructors

        static GroupByArea()
        {
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(GroupByArea), new FrameworkPropertyMetadata(typeof(GroupByArea)));
        }

		/// <summary>
		/// Constructor provided to allow creation in design tools for template and style editing.
		/// </summary>
        public GroupByArea() : this(null)
        {
			this.IsExpanded = true;
        }

        internal GroupByArea(DataPresenterBase dataPresenter) : base(dataPresenter)
        {
		}

        #endregion //Constructors

        #region Base Class Overrides

			#region OnApplyTemplate
    
		/// <summary>
		/// Called when the template is applied
		/// </summary>
    	public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this.VerifyAvailableFieldLabelsArea();
			this.VerifyGroupedFieldLabelsArea();
		}

   			#endregion //OnApplyTemplate	

			#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="GroupByArea"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="Infragistics.Windows.Automation.Peers.DataPresenter.GroupByAreaAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new GroupByAreaAutomationPeer(this);
		}
			#endregion //OnCreateAutomationPeer
    
            #region OnStyleVersionNumberChanged

        internal override void OnStyleVersionNumberChanged()
        {
            base.OnStyleVersionNumberChanged();

			// AS 11/29/10 TFS60418
			if (!this.IsActive)
				return;

			// JM 05-04-09 - Since OnFieldLayoutInitialized will only get called once for a FieldLayout, 
			//				 call OnFieldLayoutInitialized explicitly here if the current DefaultFieldLayout
			//				 does not match the last one we 'wired up' in OnFieldLayoutInitialized.
			if (this._fieldLayoutWired != null &&
				this._fieldLayoutWired != this.DataPresenter.DefaultFieldLayout)
				this.OnFieldLayoutInitialized();
			

            if (this.DataPresenter != null &&
                this.DataPresenter.DefaultFieldLayout != null &&
                this.AvailableFieldLabels != null)
            {
                this.RefreshAvailableFieldLabels(this.DataPresenter.DefaultFieldLayout,
                                                        this.AvailableFieldLabels,
                                                        true);
                this.RefreshGroupedFieldLabels(this.DataPresenter.DefaultFieldLayout,
                                                    this.GroupedFieldLabels,
                                                    true);

				// JM 05-01-09 - The ExpanderDecorator handles this automatically
                
                // reflect the existing state
                //this.IsExpanded = false;

				// JM 05-05-09 - Fire the OnShowPrompts/OnHidePrompts events as appropriate so that any EventTriggers in the
				//				 new Style's GroupByArea template are fired.
				int groupFieldLabelsCount = this.GroupedFieldLabels			== null ||
											this.GroupedFieldLabels.Count	== 0 ? 0 : this.GroupedFieldLabels.Count;
				if (groupFieldLabelsCount == 0)
					this.OnShowPrompts();
				else
					this.OnHidePrompts();
			}
		}

            #endregion //OnStyleVersionNumberChanged	

			#region OnVisualParentChanged
		/// <summary>
		/// Invoked when the visual parent of the element has been changed.
		/// </summary>
		/// <param name="oldParent">The previous visual parent</param>
		protected override void OnVisualParentChanged(DependencyObject oldParent)
		{
			// AS 11/29/10 TFS60418
			if (_reinitializeOperation == null)
				_reinitializeOperation = this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new GridUtilities.MethodDelegate(this.ReinitializeAsync));

			base.OnVisualParentChanged(oldParent);
		}
			#endregion //OnVisualParentChanged

        #endregion //Base Class Overrides

        #region Properties

            #region Public Properties

                #region AvailableFieldLabels

        private static readonly DependencyPropertyKey AvailableFieldLabelsPropertyKey =
            DependencyProperty.RegisterReadOnly("AvailableFieldLabels",
            typeof(GroupByAreaFieldLabelCollection), typeof(GroupByArea), new FrameworkPropertyMetadata());

        /// <summary>
        /// Identifies the 'AvailableFieldLabels' dependency property
        /// </summary>
        public static readonly DependencyProperty AvailableFieldLabelsProperty =
            AvailableFieldLabelsPropertyKey.DependencyProperty;

        /// <summary>
		/// Returns a collection of <see cref="GroupByAreaFieldLabel"/>s which represent the <see cref="Field"/>s currently available for grouping.
        /// </summary>
		/// <remarks>
		/// <p class="body">This collection is automatically populated with <see cref="GroupByAreaFieldLabel"/>s for each un-grouped field in the <see cref="DataPresenterBase.DefaultFieldLayout"/></p>
		/// <p class="body">The <see cref="GroupByAreaFieldLabel"/>s in this collection are derived from <see cref="System.Windows.Controls.Primitives.Thumb"/> and represent <see cref="Field"/>s
		/// available for grouping.  The default style for the <see cref="GroupByArea"/> contains a <see cref="System.Windows.Controls.ListBox"/> which is bound to this collection.  Since the elements in the
		/// collection are derived from <see cref="System.Windows.Controls.Primitives.Thumb"/> they automatically support drag and drop.  When the <see cref="GroupByArea"/> creates these elements it 
		/// listens to the drag events and automatically groups/ungroups the field represented by the <see cref="GroupByAreaFieldLabel"/> being dragged and manages the contents of the AvailableFieldLabels and 
        /// GroupedFieldLabels collections.</p>
		/// <p class="body">If you replace the Template for the <see cref="GroupByArea"/> you can bind to this collection to take advantage of this same functionality.</p>
		/// </remarks>
		/// <seealso cref="GroupByArea"/>
		/// <seealso cref="Field"/>
		/// <seealso cref="GroupByAreaFieldLabel"/>
		/// <seealso cref="System.Windows.Controls.Primitives.Thumb"/>
		//[Description("Returns a collection of all GroupByAreaFieldLabels which represent the Fields currently available for grouping.")]
        //[Category("Behavior")]
		public GroupByAreaFieldLabelCollection AvailableFieldLabels
        {
            get { return this.GetValue(GroupByArea.AvailableFieldLabelsProperty) as GroupByAreaFieldLabelCollection; }
        }

                #endregion //AvailableFieldLabels

                #region GroupedFieldLabels

        private static readonly DependencyPropertyKey GroupedFieldLabelsPropertyKey =
            DependencyProperty.RegisterReadOnly("GroupedFieldLabels",
            typeof(GroupByAreaFieldLabelCollection), typeof(GroupByArea), new FrameworkPropertyMetadata());

        /// <summary>
        /// Identifies the 'GroupedFieldLabels' dependency property
        /// </summary>
        public static readonly DependencyProperty GroupedFieldLabelsProperty =
            GroupedFieldLabelsPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a collection of <see cref="GroupByAreaFieldLabel"/>s which represent the <see cref="Field"/>s currently being used to group the data.
		/// </summary>
		/// <remarks>
		/// <p class="body">This collection is automatically populated with <see cref="GroupByAreaFieldLabel"/>s for each grouped field in the <see cref="DataPresenterBase.DefaultFieldLayout"/></p>
		/// <p class="body">The <see cref="GroupByAreaFieldLabel"/>s in this collection are derived from <see cref="System.Windows.Controls.Primitives.Thumb"/> and represent <see cref="Field"/>s
		/// currently being used to group the data.  The default style for the <see cref="GroupByArea"/> contains a <see cref="System.Windows.Controls.ListBox"/> which is bound to this collection.  Since the elements in the
		/// collection are derived from <see cref="System.Windows.Controls.Primitives.Thumb"/> they automatically support drag and drop.  When the <see cref="GroupByArea"/> creates these elements it 
        /// listens to the drag events and automatically groups/ungroups the field represented by the <see cref="GroupByAreaFieldLabel"/> being dragged and manages the contents of the AvailableFieldLabels and 
        /// GroupedFieldLabels collections.</p>
		/// <p class="body">If you replace the template for the <see cref="GroupByArea"/> you can bind to this collection to take advantage of this same functionality.</p>
		/// </remarks>
		/// <seealso cref="GroupByArea"/>
		/// <seealso cref="Field"/>
		/// <seealso cref="GroupByAreaFieldLabel"/>
		/// <seealso cref="System.Windows.Controls.Primitives.Thumb"/>
		//[Description("Returns a collection of all GroupByAreaFieldLabels which represent the Fields currently being used to group the data.")]
		//[Category("Behavior")]
        public GroupByAreaFieldLabelCollection GroupedFieldLabels
        {
            get { return this.GetValue(GroupByArea.GroupedFieldLabelsProperty) as GroupByAreaFieldLabelCollection; }
        }

                #endregion //GroupedFieldLabels

            #endregion Public Properties

            #region Internal Properties

				#region GetDefaultGroupByFieldLabelStyle

		internal Style GetDefaultGroupByFieldLabelStyle()
		{
			return this.TryFindResource(typeof(GroupByAreaFieldLabel)) as Style;
		}

				#endregion //GetDefaultGroupByFieldLabelStyle	

            #endregion Internal Properties

            #region Private Properties

				// JM 10-7-10 TFS56631 - Added
				#region FieldLayoutTemplateVersion

		/// <summary>
		/// Identifies the <see cref="FieldLayoutTemplateVersion"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FieldLayoutTemplateVersionProperty = DependencyProperty.Register("FieldLayoutTemplateVersion",
			typeof(int), typeof(GroupByArea), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnFieldLayoutTemplateVersionChanged)));

		private static void OnFieldLayoutTemplateVersionChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			GroupByArea gba = target as GroupByArea;

			// AS 11/29/10 TFS60418
			if (!gba.IsActive)
				return;

			gba.RefreshAvailableFieldLabels(gba.DataPresenter.DefaultFieldLayout, gba.AvailableFieldLabels, true);
			gba.RefreshGroupedFieldLabels(gba.DataPresenter.DefaultFieldLayout, gba.GroupedFieldLabels, true);
		}

		private int FieldLayoutTemplateVersion
		{
			get
			{
				return (int)this.GetValue(GroupByArea.FieldLayoutTemplateVersionProperty);
			}
			set
			{
				this.SetValue(GroupByArea.FieldLayoutTemplateVersionProperty, value);
			}
		}

				#endregion //FieldLayoutTemplateVersion

				// AS 11/29/10 TFS60418
				// The groupbyarea is hooked up to the DataPresenter even if its not being used - e.g. 
				// if the GroupByAreaMulti is used which it is by default. However, it was still receiving 
				// and processing fieldlayout change notifications.
				//
				#region IsActive
		private bool IsActive
		{
			get { return VisualTreeHelper.GetParent(this) != null; }
		}
				#endregion //IsActive

			#endregion Private Properties

        #endregion Properties

        #region Methods

			#region Private Methods

                #region CreateGroupByAreaFieldLabelFromField

		// SSP 7/17/07 BR22919
		// Added forGroupedFieldsArea parameter.
		// 
        //private GroupByAreaFieldLabel CreateGroupByAreaFieldLabelFromField(Field field, Style defaultStyle)
		private GroupByAreaFieldLabel CreateGroupByAreaFieldLabelFromField( Field field, Style defaultStyle, bool forGroupedFieldsArea )
        {
            GroupByAreaFieldLabel groupByAreaFieldLabel = new GroupByAreaFieldLabel(this, field, defaultStyle);

            // Bind the label's StyleVersion property to the GroupByArea's StyleVersion property
            Binding binding     = new Binding("StyleVersionNumber");
            binding.Mode        = BindingMode.OneWay;
            binding.Source      = this;
            groupByAreaFieldLabel.SetBinding(GroupByAreaFieldLabel.StyleVersionNumberProperty, binding);

			// SSP 7/17/07 BR22919
			// Added forGroupedFieldsArea parameter. If a field is grouped, we always want to
			// display its caption in the area where we display grouped fields, even if that field
			// is hidden. Therefore don't bind the Visibility of the grouped field label to the
			// VisibilityResolved of the field since the field's VisibilityResolved will be
			// Collapsed if the FieldSettings' CellVisibilityWhenGrouped is set to Collapsed.
			// 
			if ( ! forGroupedFieldsArea )
			{
				// JJD 5/01/07
				// Bind to the field's visibility property
				// Bind the label's StyleVersion property to the GroupByArea's StyleVersion property
				binding = new Binding( "VisibilityResolved" );
				binding.Mode = BindingMode.OneWay;
				binding.Source = field;
				groupByAreaFieldLabel.SetBinding( GroupByAreaFieldLabel.VisibilityProperty, binding );
			}

            // Listen to the dragging events.
            groupByAreaFieldLabel.DragStarted   += new DragStartedEventHandler(OnGroupByAreaFieldLabelDragStarted);
            groupByAreaFieldLabel.DragDelta     += new DragDeltaEventHandler(OnGroupByAreaFieldLabelDragDelta);
            groupByAreaFieldLabel.DragCompleted += new DragCompletedEventHandler(OnGroupByAreaFieldLabelDragCompleted);

            return groupByAreaFieldLabel;
        }

                #endregion CreateGroupByAreaFieldLabelFromField

				#region InitializeAvailableFieldLabelsCollection

        private void InitializeAvailableFieldLabelsCollection()
        {
			if (this.DataPresenter == null || 
				this.DataPresenter.DefaultFieldLayout == null)
                return;


            if (this.GetValue(GroupByArea.AvailableFieldLabelsProperty) == null)
            {
                FieldLayout						fieldLayout				= this.DataPresenter.DefaultFieldLayout;
                GroupByAreaFieldLabelCollection fieldLabelsCollection	= GroupByAreaFieldLabelCollection.Create();

                this.SetValue(GroupByArea.AvailableFieldLabelsPropertyKey, fieldLabelsCollection);

                this.RefreshAvailableFieldLabels(fieldLayout, fieldLabelsCollection, true);
            }
        }

			 #endregion //InitializeAvailableFieldLabelsCollection

				#region InitializeGroupedFieldLabelsCollection

        private void InitializeGroupedFieldLabelsCollection()
        {
			if (this.DataPresenter == null || 
				this.DataPresenter.DefaultFieldLayout == null)
                return;


            if (this.GetValue(GroupByArea.GroupedFieldLabelsProperty) == null)
            {
                FieldLayout						fieldLayout				= this.DataPresenter.DefaultFieldLayout;
                GroupByAreaFieldLabelCollection fieldLabelsCollection	= GroupByAreaFieldLabelCollection.Create();

                this.SetValue(GroupByArea.GroupedFieldLabelsPropertyKey, fieldLabelsCollection);
                fieldLabelsCollection.CollectionChanged += new NotifyCollectionChangedEventHandler(OnGroupedFieldsChanged);

                this.RefreshGroupedFieldLabels(fieldLayout, fieldLabelsCollection, true);
            }
        }

				#endregion //InitializeAvailableFieldLabelsCollection
    
				#region OnThemeChanged

		internal override void OnThemeChanged(object sender, RoutedPropertyChangedEventArgs<string> e)
		{
            base.OnThemeChanged(sender, e);

			// AS 11/29/10 TFS60418
			if (!this.IsActive)
				return;

			this.RefreshAvailableFieldLabels(this.DataPresenter.DefaultFieldLayout, this.AvailableFieldLabels, true);
			this.RefreshGroupedFieldLabels(this.DataPresenter.DefaultFieldLayout, this.GroupedFieldLabels, true);


			// JM 05-05-09 - Fire the OnShowPrompts/OnHidePrompts events as appropriate so that any EventTriggers in the
			//				 new Theme's GroupByArea template are fired.
			int groupFieldLabelsCount = this.GroupedFieldLabels			== null ||
										this.GroupedFieldLabels.Count	== 0 ? 0 : this.GroupedFieldLabels.Count;
			if (groupFieldLabelsCount == 0)
				this.OnShowPrompts();
			else
				this.OnHidePrompts();
		}

				#endregion //OnThemeChanged	
    
				#region OnFieldLayoutInitialized

		internal override void OnFieldLayoutInitialized()
		{
            base.OnFieldLayoutInitialized();

			// Make sure that a default field layout has been established.  This may not have happened yet
			// depending on timing.
			if (this.DataPresenter == null || this.DataPresenter.DefaultFieldLayout == null)
				return;

			// AS 11/29/10 TFS60418
			if (!this.IsActive)
				return;

			// first make sure the labels collections are allocated
			this.InitializeAvailableFieldLabelsCollection();
			this.InitializeGroupedFieldLabelsCollection();

			// then refresh the collections
			this.RefreshAvailableFieldLabels(this.DataPresenter.DefaultFieldLayout, this.AvailableFieldLabels, true);
			this.RefreshGroupedFieldLabels(this.DataPresenter.DefaultFieldLayout, this.GroupedFieldLabels, true);

			this.InitializeGroupedFieldLabelsCollection();
			this.InitializeAvailableFieldLabelsCollection();

			// JJD  2/24/07
			// Only hook the event the 1st time thru or if the default layout changes
			// Listen to the SortedFields collection changed notification
			if (this._fieldLayoutWired != this.DataPresenter.DefaultFieldLayout)
			{
				// JJD  2/24/07
				// unhook from the last one
				if ( this._fieldLayoutWired != null )
					this._fieldLayoutWired.SortedFields.CollectionChanged -= new NotifyCollectionChangedEventHandler(OnSortedFieldsChanged);

				this._fieldLayoutWired = this.DataPresenter.DefaultFieldLayout;

				if ( this._fieldLayoutWired != null )
					this._fieldLayoutWired.SortedFields.CollectionChanged += new NotifyCollectionChangedEventHandler(OnSortedFieldsChanged);
			}

			// JM 10-7-10 TFS56631 
			Binding binding = new Binding("TemplateVersion");
			binding.Source	= this._fieldLayoutWired;
			binding.Mode	= BindingMode.OneWay;
			this.SetBinding(GroupByArea.FieldLayoutTemplateVersionProperty, binding);
		}

				#endregion //OnFieldLayoutInitialized

				#region OnExpanderBarAreaClick

		private void OnExpanderBarAreaMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.IsExpanded = !this.IsExpanded;
        }

				#endregion //OnExpanderBarAreaClick

				#region OnGroupByAreaFieldLabelDragCompleted

        void OnGroupByAreaFieldLabelDragCompleted(object sender, DragCompletedEventArgs e)
        {
            // Get a reference to the label being dragged.
            GroupByAreaFieldLabel groupByAreaFieldLabelBeingDragged = sender as GroupByAreaFieldLabel;

            Debug.Assert(groupByAreaFieldLabelBeingDragged != null);
            if (groupByAreaFieldLabelBeingDragged == null)
                return;


            // Reset our dragging flag.
			this.SetValue(GroupByArea.FieldLabelDragInProgressPropertyKey, KnownBoxes.FalseBox);


            // Remove the drag element.
			this.RemoveVisualChild(this.CurrentFieldLabelDragElement);
			this.CurrentFieldLabelDragElement = null;


            // Hide the insertion point if it is currently highlighted.
            if (this.IsDragInsertionPointHighlighted)
                this.OnHideInsertionPoint();


            // Set a flag that tells us whether we are grouping or ungrouping.
            bool actionIsGrouping	= (groupByAreaFieldLabelBeingDragged.Field.IsGroupBy == false && this._isFieldLabelDragOverGroupedListArea == true);
            bool actionIsReGrouping = (groupByAreaFieldLabelBeingDragged.Field.IsGroupBy == true && this._isFieldLabelDragOverGroupedListArea == true);
            bool actionIsUnGrouping = (groupByAreaFieldLabelBeingDragged.Field.IsGroupBy == true && this._isFieldLabelDragOverGroupedListArea == false);

            // set a flag to bypass sorted fields changed notifications
            this._ignoreSortedFieldsChange = true;

			Style defaultFieldLabelStyle = this.GetDefaultGroupByFieldLabelStyle();

			// JM 04-17-09 - CrossBand grouping feature
			// Refactor and move the code that updates the groupings and fires grouping events into the new GroupingHelper class so that the 
			// functionality can be shared by the new GroupByAreaMulti.
			#region old code
			//FieldLayout fl = this.DataPresenter.DefaultFieldLayout;
			//FieldSortDescriptionCollection sortedFields = fl.SortedFields;
			//List<FieldSortDescription> groups = new List<FieldSortDescription>(sortedFields.CountOfGroupByFields + 1);

			//// JJD 3/2/07
			//// Initialize a the list of existing groupby fields used to support raising the grouping events
			//for (int i = 0; i < sortedFields.CountOfGroupByFields; i++)
			//{
			//    if ( sortedFields[i].IsGroupBy )
			//        groups.Add(sortedFields[i]);
			//}
			#endregion //old code
			// AS 6/1/09 NA 2009.2 Undo/Redo
			//GroupingHelper groupingHelper = new GroupingHelper(this, this.DataPresenter.DefaultFieldLayout);
			GroupingHelper groupingHelper = new GroupingHelper(this.DataPresenter.DefaultFieldLayout);

			bool cancel = false;

            try
            {
				// JJD 5/9/07 - BR22685
				// Call the IsOkToScroll method 1st which will attempt to exit edit mode
				// and return false if that gets cancelled
				IViewPanelInfo viewPanelInfo = this.DataPresenter as IViewPanelInfo;

				if (viewPanelInfo != null &&
					viewPanelInfo.IsOkToScroll() == false)
				{
					cancel = true;
					return;
				}

                if (actionIsReGrouping)
                {
                    if (this.GroupedFieldLabels.IndexOf(groupByAreaFieldLabelBeingDragged) != this._currentFieldLabelDragElementInsertAtIndex)
                    {
                        if (this.GroupedFieldLabels.IndexOf(groupByAreaFieldLabelBeingDragged) < this._currentFieldLabelDragElementInsertAtIndex)
                            this._currentFieldLabelDragElementInsertAtIndex--;

                        if (this._currentFieldLabelDragElementInsertAtIndex > -1 &&
                            this._currentFieldLabelDragElementInsertAtIndex != this.GroupedFieldLabels.IndexOf(groupByAreaFieldLabelBeingDragged) &&
                            this._currentFieldLabelDragElementInsertAtIndex <= this.GroupedFieldLabels.Count)
                        {
							// JM 04-17-09 - CrossBand grouping feature
							// Refactor and move the code that updates the groupings and fires grouping events into the new GroupingHelper class so that the 
							// functionality can be shared by the new GroupByAreaMulti.
							#region old code
							//// JJD 3/2/07
							//// adjust the list of existing groupby fields used to support raising the grouping events
							//foreach (FieldSortDescription fieldSortDescription in groups)
							//{
							//    if (fieldSortDescription.Field == groupByAreaFieldLabelBeingDragged.Field)
							//    {
							//        // Remove from old position.
							//        groups.Remove(fieldSortDescription);

							//        // Insert at new position.
							//        groups.Insert(this._currentFieldLabelDragElementInsertAtIndex, fieldSortDescription);
							//        break;
							//    }
							//}

							//// JJD 3/2/07
							//// raise the before grouping event
							//// if canceled then return
							//if (!this.RaiseGroupingEventHelper(fl, groups))
							//{
							//    cancel = true;
							//    return;
							//}
							#endregion //old code
							if (false == groupingHelper.ProcessTryReGroup(groupByAreaFieldLabelBeingDragged.Field, this._currentFieldLabelDragElementInsertAtIndex))
							{
								cancel = true;
								return;
							}

                            // Remove the field from its old position in the GroupedList
                            this.GroupedFieldLabels.InternalRemove(groupByAreaFieldLabelBeingDragged, false);

                            // Insert the field into its new position in the Grouped list.
                            this.GroupedFieldLabels.InternalInsert(this._currentFieldLabelDragElementInsertAtIndex, groupByAreaFieldLabelBeingDragged, true);

                            groupByAreaFieldLabelBeingDragged.OnAddedToGroupedList();

							// JM 04-17-09 - CrossBand grouping feature
							// Refactor and move the code that updates the groupings and fires grouping events into the new GroupingHelper class so that the 
							// functionality can be shared by the new GroupByAreaMulti.
							#region old code
							//// JJD 1/29/09
							//// Since we will be making multiple updates call BeginUpdate
							//sortedFields.BeginUpdate();

							//try
							//{
							//    // Change the field's position in the list of sorted fields to trigger the actual grouping.
							//    foreach (FieldSortDescription fieldSortDescription in sortedFields)
							//    {
							//        if (fieldSortDescription.Field == groupByAreaFieldLabelBeingDragged.Field)
							//        {
							//            // Remove from old position.
							//            sortedFields.Remove(fieldSortDescription);

							//            // Insert at new position.
							//            sortedFields.Insert(this._currentFieldLabelDragElementInsertAtIndex, fieldSortDescription);
							//            break;
							//        }
							//    }
							//}
							//finally
							//{
							//    // JJD 1/29/09
							//    // Call EndUpdate since we called BeginUpdate above
							//    sortedFields.EndUpdate();
							//}
							#endregion //old code
							groupingHelper.ProcessApplyReGroup(groupByAreaFieldLabelBeingDragged.Field, this._currentFieldLabelDragElementInsertAtIndex);
                        }
                    }
                }
                else if (actionIsGrouping)
                {
                    // If the drag element was not dropped in a valid area, make the original label visible and exit.
					// JM 10-14-08 TFS8190 - Should be looking at GroupedFieldLabels.Count here.
					if (this._currentFieldLabelDragElementInsertAtIndex < 0 ||
                        //this._currentFieldLabelDragElementInsertAtIndex > this.AvailableFieldLabels.Count)
                        this._currentFieldLabelDragElementInsertAtIndex > this.GroupedFieldLabels.Count)
                    {
                        cancel = true;
                        return;
                    }

					// JM 04-17-09 - CrossBand grouping feature
					// Refactor and move the code that updates the groupings and fires grouping events into the new GroupingHelper class so that the 
					// functionality can be shared by the new GroupByAreaMulti.
					#region old code
					//// Add the field to the list of sorted fields to trigger the actual grouping.  (If it is
					//// already in the sorted fields collection remove it first then re-add it)
					//FieldSortDescription oldfieldSortDescription = this.GetFieldSortDescriptionForField(groupByAreaFieldLabelBeingDragged.Field, groups);
					//if (oldfieldSortDescription != null)
					//    groups.Remove(oldfieldSortDescription);

					//FieldSortDescription fieldSortDescription = new FieldSortDescription();
					//fieldSortDescription.Field = groupByAreaFieldLabelBeingDragged.Field;
					//fieldSortDescription.Direction = ListSortDirection.Ascending;
					//// JM 10-16-08 [BR35076  TFS6401] Preserve the sort direction specified on the Field (if any)
					//if (sortedFields.Contains(fieldSortDescription.Field))
					//    fieldSortDescription.Direction = sortedFields[fieldSortDescription.Field].Direction;
					//fieldSortDescription.IsGroupBy = true;
                    
					//groups.Insert(this._currentFieldLabelDragElementInsertAtIndex, fieldSortDescription);

					//// JJD 3/2/07
					//// raise the before grouping event
					//// if canceled then return
					//if (!this.RaiseGroupingEventHelper(fl, groups))
					//{
					//    cancel = true;
					//    return;
					//}
					#endregion //old code
					if (false == groupingHelper.ProcessTryGrouping(groupByAreaFieldLabelBeingDragged.Field, this._currentFieldLabelDragElementInsertAtIndex))
					{
						cancel = true;
						return;
					}

                    // Add the field to the grouped list and fire the appropriate events.
                    groupByAreaFieldLabelBeingDragged.OnRemovedFromAvailableList();

					// JM 10-14-08 TFS8190 - Change to actually remove the GroupByAreaFieldLabel from the 'available' list when it is grouped.
					this.UnhookFieldLabelDragEventListeners(groupByAreaFieldLabelBeingDragged);
					this.AvailableFieldLabels.InternalRemove(groupByAreaFieldLabelBeingDragged, true);

					// SSP 7/17/07 BR22919
					// Added forGroupedFieldsArea parameter.
					// 
                    //GroupByAreaFieldLabel newGroupByAreaFieldLabel = this.CreateGroupByAreaFieldLabelFromField(groupByAreaFieldLabelBeingDragged.Field, defaultFieldLabelStyle);
					GroupByAreaFieldLabel newGroupByAreaFieldLabel = this.CreateGroupByAreaFieldLabelFromField( groupByAreaFieldLabelBeingDragged.Field, defaultFieldLabelStyle, true );

                    this.GroupedFieldLabels.InternalInsert(this._currentFieldLabelDragElementInsertAtIndex, newGroupByAreaFieldLabel, true);
                    newGroupByAreaFieldLabel.OnAddedToGroupedList();


					// JM 04-17-09 - CrossBand grouping feature
					// Refactor and move the code that updates the groupings and fires grouping events into the new GroupingHelper class so that the 
					// functionality can be shared by the new GroupByAreaMulti.
					#region old code
					//// Add the field to the list of sorted fields to trigger the actual grouping.  (If it is
					//// already in the sorted fields collection remove it first then re-add it)
					//oldfieldSortDescription = this.GetFieldSortDescriptionForField(newGroupByAreaFieldLabel.Field, sortedFields);
					//if (oldfieldSortDescription != null)
					//{
					//    // JJD 1/29/09
					//    // Since we will be making multiple updates call BeginUpdate
					//    sortedFields.BeginUpdate();

					//    sortedFields.Remove(oldfieldSortDescription);
					//}

					//try
					//{
					//    sortedFields.Insert(this._currentFieldLabelDragElementInsertAtIndex, fieldSortDescription);
					//}
					//finally
					//{
					//    // JJD 1/29/09
					//    // If we called BeginUpdate above call EndUpdate
					//    if (oldfieldSortDescription != null)
					//        sortedFields.EndUpdate();
					//}
					#endregion //old code
					groupingHelper.ProcessApplyGrouping(groupByAreaFieldLabelBeingDragged.Field, this._currentFieldLabelDragElementInsertAtIndex);
				}
                else if (actionIsUnGrouping)
                {
					// JM 04-17-09 - CrossBand grouping feature
					// Refactor and move the code that updates the groupings and fires grouping events into the new GroupingHelper class so that the 
					// functionality can be shared by the new GroupByAreaMulti.
					#region old code
					//// JJD 3/2/07
					//// Get the field from the list
					//FieldSortDescription oldfieldSortDescription = this.GetFieldSortDescriptionForField(groupByAreaFieldLabelBeingDragged.Field, groups);

					//Debug.Assert(oldfieldSortDescription != null);

					//if (oldfieldSortDescription == null)
					//{
					//    cancel = true;
					//    return;
					//}

					//// JJD 3/2/07
					//// Remove the field from the list
					//groups.Remove(oldfieldSortDescription);

					//// JJD 3/2/07
					//// raise the before grouping event
					//// if canceled then return
					//if (!this.RaiseGroupingEventHelper(fl, groups))
					//{
					//    cancel = true;
					//    return;
					//}
					#endregion //old code
					if (false == groupingHelper.ProcessTryUnGroup(groupByAreaFieldLabelBeingDragged.Field))
					{
						cancel = true;
						return;
					}

                    // Remove the field from the GroupedList
                    this.GroupedFieldLabels.InternalRemove(groupByAreaFieldLabelBeingDragged, true);
                    this.UnhookFieldLabelDragEventListeners(groupByAreaFieldLabelBeingDragged);

                    // reset the flag
                    this._ignoreSortedFieldsChange = false;


					// JM 02-05-09 TFS13537 - Move this foreach block of code after the next foreach block.  We need to do this because of the change made in the Grouping scenario above [JM 10-14-08 TFS8190]
					//						  which now actually removes GroupByAreaFieldLabels from the Available list when they are grouped.  Since the GroupByAreaFieldLabel for the 
					//						  field being dragged has not been added to the Available list yet, we never find the groupByAreaFieldLabelBeingDragged in the following
					//						  block of code and therefore never fire the OnAddedToAvailableList event.  The GroupByAreaFieldLabel actually gets added back into the 
					//						  Available list by the next foreach block as a result of updating the list of sorted fields.
					// Fire the OnAddedToAvailableList event
					//foreach (GroupByAreaFieldLabel availableGroupByAreaFieldLabel in this.AvailableFieldLabels)
					//{
					//    if (availableGroupByAreaFieldLabel.Field == groupByAreaFieldLabelBeingDragged.Field)
					//    {
					//        availableGroupByAreaFieldLabel.OnAddedToAvailableList();
					//        break;
					//    }
					//}

					//if (sortedFields.Count > 0)
					if (groupingHelper.SortedFields.Count > 0)
                    {
						// JM 05-04-09 - Move the calls to BeginUpdate/EndUpdate to the ProcessApplyUngroup
						//				 routine to simplify the use of the routine instead of requiring all 
						//				 callers to call BeginUpdate/EndUpdate
						// JJD 1/29/09
                        // Since we will be making multiple updates call BeginUpdate
                        //sortedFields.BeginUpdate();
						//groupingHelper.SortedFields.BeginUpdate();

						// JM 04-17-09 - CrossBand grouping feature
						// Refactor and move the code that updates the groupings and fires grouping events into the new GroupingHelper class so that the 
						// functionality can be shared by the new GroupByAreaMulti.
						#region old code
						//// Remove the field from the list of sorted fields to trigger the actual grouping.
						//foreach (FieldSortDescription fieldSortDescription in sortedFields)
						//{
						//    if (fieldSortDescription.Field == groupByAreaFieldLabelBeingDragged.Field)
						//    {
						//        sortedFields.Remove(fieldSortDescription);
						//        break;
						//    }
						//}
						#endregion //old code
						groupingHelper.ProcessApplyUnGroup(groupByAreaFieldLabelBeingDragged.Field);

						// JM 05-04-09 - Don't call EndUpdate here (see note above about moving BeingUpdate/EndUpdate
						//				 calls to the ProcessApplyUngroup routine.
                        // JJD 1/29/09
                        // Call EndUpdate since we called BeginUpdate above
                        //sortedFields.EndUpdate();
						//groupingHelper.SortedFields.EndUpdate();

						// JM 02-05-09 TFS13537 - Moved from above.
						// Fire the OnAddedToAvailableList event
						foreach (GroupByAreaFieldLabel availableGroupByAreaFieldLabel in this.AvailableFieldLabels)
						{
							if (availableGroupByAreaFieldLabel.Field == groupByAreaFieldLabelBeingDragged.Field)
							{
								availableGroupByAreaFieldLabel.OnAddedToAvailableList();
								break;
							}
						}
                    }
                }

                else
                {
                    if (groupByAreaFieldLabelBeingDragged.Field.IsGroupBy == false)
                    {
                        cancel = true;
                        return;
                    }
                }
            }
            finally
            {
                // reset the flag
                this._ignoreSortedFieldsChange = false;

				// [BR19838 3-8-07 JM] Always reset the opacity, not just when cancel = true.
				groupByAreaFieldLabelBeingDragged.Opacity = 1.0;

				if (cancel == false)
				{
					// JM 04-17-09 - CrossBand grouping feature
					//FieldSortDescription[] newGroupsArray = new FieldSortDescription[groups.Count];
					//groups.CopyTo(newGroupsArray);
					//this.DataPresenter.RaiseGrouped(new GroupedEventArgs(fl, newGroupsArray));
					// AS 6/1/09 NA 2009.2 Undo/Redo
					// Use the existing helper method on the GroupingHelper class.
					//
					//FieldSortDescription[] newGroupsArray = new FieldSortDescription[groupingHelper.Groups.Count];
					//groupingHelper.Groups.CopyTo(newGroupsArray);
					//this.DataPresenter.RaiseGrouped(new GroupedEventArgs(groupingHelper.FieldLayout, newGroupsArray));
					groupingHelper.RaiseGroupedEventHelper();
				}

            }
        }

			 #endregion //OnGroupByAreaFieldLabelDragCompleted
    
				#region OnGroupByAreaFieldLabelDragDelta

        void OnGroupByAreaFieldLabelDragDelta(object sender, DragDeltaEventArgs e)
        {
            // Get a reference to the label being dragged.
            GroupByAreaFieldLabel groupByAreaFieldLabel = sender as GroupByAreaFieldLabel;

            Debug.Assert(groupByAreaFieldLabel != null);
            if (groupByAreaFieldLabel == null)
                return;


            // Set the drag element's TranslateTransform offset properties to reflect the new delta.
            this.CurrentFieldLabelDragElementTransform.X = e.HorizontalChange;
            this.CurrentFieldLabelDragElementTransform.Y = e.VerticalChange;


            // See if we are over a grouped label element, and if so, let it know so it can show/hide insertion points.
            Point pointInOriginalFieldLabelCoords	= new Point(e.HorizontalChange + this._currentFieldLabelDragElementStartOffset.X,
                                                                e.VerticalChange + this._currentFieldLabelDragElementStartOffset.Y);
            Point pointInGroupByAreaCoords			= Utilities.TransformPointToAncestorCoordinates(pointInOriginalFieldLabelCoords, groupByAreaFieldLabel, this);


            // Set flags indicating whether we are over the Grouped area or the Available area.
            this.SetDragPositionFlags(pointInGroupByAreaCoords);


            // Show/hide/position the insertion point.
            //
            // If we are not currently over the GroupedListArea, the insertion does not need to be shown.  If the insertion
            // point is currently highlighted, hide it.
            double yOffset = 0;
            if (this._isFieldLabelDragOverGroupedListArea == false)
            {
                if (this.IsDragInsertionPointHighlighted == true)
                    this.OnHideInsertionPoint();

                this._currentFieldLabelDragElementInsertAtIndex = -1;
                this.LastDragInsertionPointOffset				= new Vector(double.PositiveInfinity, double.PositiveInfinity);
            }
            // If there are no entries in the Grouped list yet, set the insertion point at the beginning of the list.
            else if (this.GroupedFieldLabels.Count < 1)
            {
                Vector newOffset = new Vector(0, yOffset);
                if (newOffset != this.LastDragInsertionPointOffset)
                {
                    this.InsertionPoint.RenderTransform	= new TranslateTransform(newOffset.X, newOffset.Y);
                    this.OnShowInsertionPoint();
                    this.LastDragInsertionPointOffset		= new Vector(newOffset.X, newOffset.Y);

                    this._currentFieldLabelDragElementInsertAtIndex = 0;
                }
            }
            // Loop through all entries in the Grouped list and find the first entry where the current drag position is to the
            // left of the middle of the label and place the insertion point just before that label.
            // If no such label is found, place the insertion point after the last element.
            else
            {
                bool insertionPointSet = false;
                foreach (GroupByAreaFieldLabel fieldLabel in this.GroupedFieldLabels)
                {
                    Rect fieldLabelRect = fieldLabel.RectInGroupByAreaCoordinates;

                    if (pointInGroupByAreaCoords.X < (fieldLabelRect.Left + (fieldLabelRect.Width / 2)))
                    {
                        // Get the insertion point width.  Treat polygon elements differently because their ActualWidth
                        // property returns some wierd huge number
                        double insertionPointWidth = this.InsertionPoint.ActualWidth;
                        if (this.InsertionPoint is System.Windows.Shapes.Polygon)
                            insertionPointWidth = ((System.Windows.Shapes.Polygon)this.InsertionPoint).RenderedGeometry.Bounds.Width - 3;

                        Vector newOffset = new Vector(fieldLabelRect.X - insertionPointWidth, yOffset);
                        if (newOffset != this.LastDragInsertionPointOffset)
                        {
                            //Debug.WriteLine("Using newOffset: " + newOffset.ToString() + ", fieldLabelrect: " + fieldLabelRect.ToString() + ", InsertionPoint width: " + insertionPointWidth.ToString());
                            if (this.LastDragInsertionPointOffset.X != double.PositiveInfinity)
                                this.OnHideInsertionPoint();

                            this.InsertionPoint.RenderTransform			= new TranslateTransform(newOffset.X, newOffset.Y);
                            this.OnShowInsertionPoint();
                            this._currentFieldLabelDragElementInsertAtIndex = this.GroupedFieldLabels.IndexOf(fieldLabel);
                            this.LastDragInsertionPointOffset				= newOffset;
                        }

                        insertionPointSet = true;

                        break;
                    }
                }


                // If we did not set an insertion point in the above loop, set one now after the last field label.
                if (insertionPointSet == false)
                {
                    Rect fieldLabelRect = this.GroupedFieldLabels[this.GroupedFieldLabels.Count - 1].RectInGroupByAreaCoordinates;
                    Vector newOffset = new Vector(fieldLabelRect.X + fieldLabelRect.Width, yOffset);
                    if (newOffset != this.LastDragInsertionPointOffset)
                    {
                        if (this.LastDragInsertionPointOffset.X != double.PositiveInfinity)
                            this.OnHideInsertionPoint();

                        this.InsertionPoint.RenderTransform			= new TranslateTransform(newOffset.X, newOffset.Y);
                        this.OnShowInsertionPoint();
                        this._currentFieldLabelDragElementInsertAtIndex = this.GroupedFieldLabels.Count;
                        this.LastDragInsertionPointOffset				= newOffset;
                    }
                }
            }
        }

			#endregion //OnGroupByAreaFieldLabelDragDelta

				#region OnGroupByAreaFieldLabelDragStarted

        void OnGroupByAreaFieldLabelDragStarted(object sender, DragStartedEventArgs e)
        {
            // Get a reference to the label being dragged.
            GroupByAreaFieldLabel groupByAreaFieldLabel = sender as GroupByAreaFieldLabel;

            Debug.Assert(groupByAreaFieldLabel != null);
            if (groupByAreaFieldLabel == null)
                return;


            // Create a Visualbrush from the label being dragged and apply it to the background of the element being dragged.
            VisualBrush vb									= new VisualBrush(groupByAreaFieldLabel);
            this.CurrentFieldLabelDragElement.Fill			= vb;

            this.CurrentFieldLabelDragElementTransform.X	= 0;
            this.CurrentFieldLabelDragElementTransform.Y	= 0;


            // Position the element being dragged over the field label and make it visible.
            this.CurrentFieldLabelDragElement.Arrange(new Rect(Utilities.TransformPointToAncestorCoordinates(new Point(0, 0), groupByAreaFieldLabel, this),
                                                               new Size(groupByAreaFieldLabel.ActualWidth, groupByAreaFieldLabel.ActualHeight)));
            this.CurrentFieldLabelDragElement.Visibility = Visibility.Visible;


            // Change the appearance of the field label.
            groupByAreaFieldLabel.Opacity = 0.3;


            // Set some flags and save some values.
            this.LastDragInsertionPointOffset				= new Vector(double.PositiveInfinity, double.PositiveInfinity);
            this._currentFieldLabelDragElementStartOffset	= new Vector(e.HorizontalOffset, e.VerticalOffset);
            this._currentFieldLabelDragElementInsertAtIndex = -1;
            this.IsDragInsertionPointHighlighted			= false;
			this.SetValue(GroupByArea.FieldLabelDragInProgressPropertyKey, KnownBoxes.TrueBox);
        }

				#endregion //OnGroupByAreaFieldLabelDragStarted

				#region OnGroupedFieldsChanged

        void OnGroupedFieldsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.GroupedFieldLabels.Count == 0 && this._lastGroupedFieldsCount > 0)
            {
                this.OnShowPrompts();
                this._lastGroupedFieldsCount = this.GroupedFieldLabels.Count;
            }
            else if (this.GroupedFieldLabels.Count > 0 && this._lastGroupedFieldsCount == 0)
            {
                this.OnHidePrompts();
                this._lastGroupedFieldsCount = this.GroupedFieldLabels.Count;
            }
        }

				#endregion //OnGroupedFieldsChanged

				#region OnSortedFieldsChanged

        void OnSortedFieldsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this._ignoreSortedFieldsChange == false)
            {
				// AS 11/29/10 TFS60418
				if (!this.IsActive)
					return;

				this.RefreshAvailableFieldLabels(this.DataPresenter.DefaultFieldLayout, this.AvailableFieldLabels, true);
                this.RefreshGroupedFieldLabels(this.DataPresenter.DefaultFieldLayout, this.GroupedFieldLabels, true);
            }
        }

				#endregion //OnSortedFieldsChanged

				#region RefreshAvailableFieldLabels

        private void RefreshAvailableFieldLabels(FieldLayout fieldLayout, GroupByAreaFieldLabelCollection availableFieldLabels, bool notifyCollectionChanged)
        {

			Style defaultStyle = this.GetDefaultGroupByFieldLabelStyle();

            this.UnhookAllFieldLabelDragEventListeners(true, false);
            availableFieldLabels.InternalClear();

            // JJD 6/12/08 
            // Check for a null fieldLayout. This can happen if you clear the DataSource, then clear the FieldLayouts collection
            // and finally change the Theme property
            if (fieldLayout != null)
            {
                foreach (Field field in fieldLayout.Fields)
                {
                    if (field.AllowGroupByResolved == false)
                        continue;

                    if (!field.IsVisibleInCellArea)
                        continue;

					// JM 10-14-08 TFS8190 - Now that we are actually removing GroupByAreaFieldLabels from the 'available' list when they are grouped,
					//						 don't add them here on a refresh.
					if (field.IsGroupBy)
						continue;

                    // SSP 7/17/07 BR22919
                    // Added forGroupedFieldsArea parameter.
                    // 
                    //GroupByAreaFieldLabel groupByAreaFieldLabel = this.CreateGroupByAreaFieldLabelFromField(field, defaultStyle);
                    GroupByAreaFieldLabel groupByAreaFieldLabel = this.CreateGroupByAreaFieldLabelFromField(field, defaultStyle, false);

					// JM 10-14-08 TFS8190 - Don't need to do this since we are no longer adding grouped fields to the 'available' list (we now exit above if the field is grouped).
					//if (field.IsGroupBy)
					//	groupByAreaFieldLabel.LayoutTransform = new ScaleTransform(.000001, 1);

                    availableFieldLabels.InternalAdd(groupByAreaFieldLabel);
                }
            }

            if (notifyCollectionChanged)
                availableFieldLabels.InternalReset();
        }

				#endregion //RefreshAvailableFieldLabels

				#region RefreshGroupedFieldLabels

        private void RefreshGroupedFieldLabels(FieldLayout fieldLayout, GroupByAreaFieldLabelCollection groupedFieldLabels, bool notifyCollectionChanged)
        {
            Style defaultStyle = this.GetDefaultGroupByFieldLabelStyle();
			this.UnhookAllFieldLabelDragEventListeners(false, true);
            groupedFieldLabels.InternalClear();

            // JJD 6/12/08 
            // Check for a null fieldLayout. This can happen if you clear the DataSource, then clear the FieldLayouts collection
            // and finally change the Theme property
            if (fieldLayout != null)
            {
                foreach (FieldSortDescription fsd in fieldLayout.SortedFields)
                {
                    if (fsd.IsGroupBy)
                        // SSP 7/17/07 BR22919
                        // Added forGroupedFieldsArea parameter.
                        // 
                        //groupedFieldLabels.InternalAdd(this.CreateGroupByAreaFieldLabelFromField(fsd.Field, defaultStyle));
                        groupedFieldLabels.InternalAdd(this.CreateGroupByAreaFieldLabelFromField(fsd.Field, defaultStyle, true));
                }
            }

            if (notifyCollectionChanged)
                groupedFieldLabels.InternalReset();
        }

				#endregion //RefreshGroupedFieldLabels

				// AS 11/29/10 TFS60418
				#region ReinitializeAsync
		private void ReinitializeAsync()
		{
			_reinitializeOperation = null;

			if (!this.IsActive)
				return;

			this.OnFieldLayoutInitialized();
		} 
				#endregion //ReinitializeAsync

				#region SetDragPositionFlags

        private void SetDragPositionFlags(Point pointInGroupByAreaCoords)
        {
            Rect groupedListAreaRect					= new Rect(Utilities.TransformPointToAncestorCoordinates(new Point(0, 0), this._groupedFieldLabelsArea, this), new Size(this._groupedFieldLabelsArea.ActualWidth, this._groupedFieldLabelsArea.ActualHeight));
            this._isFieldLabelDragOverGroupedListArea	= groupedListAreaRect.Contains(pointInGroupByAreaCoords);

            Rect availableListAreaRect					= new Rect(Utilities.TransformPointToAncestorCoordinates(new Point(0, 0), this._availableFieldLabelsArea, this), new Size(this._availableFieldLabelsArea.ActualWidth, this._availableFieldLabelsArea.ActualHeight));
            this._isFieldLabelDragOverAvailableListArea = availableListAreaRect.Contains(pointInGroupByAreaCoords);
        }

				#endregion SetDragPositionFlags

				#region UnhookFieldLabelDragEventListeners

        private void UnhookFieldLabelDragEventListeners(GroupByAreaFieldLabel groupByAreaFieldLabel)
        {
            groupByAreaFieldLabel.DragStarted	-= new DragStartedEventHandler(OnGroupByAreaFieldLabelDragStarted);
            groupByAreaFieldLabel.DragDelta		-= new DragDeltaEventHandler(OnGroupByAreaFieldLabelDragDelta);
            groupByAreaFieldLabel.DragCompleted -= new DragCompletedEventHandler(OnGroupByAreaFieldLabelDragCompleted);
        }

				#endregion UnhookFieldLabelDragEventListeners

				#region UnhookAllFieldLabelDragEventListeners

        private void UnhookAllFieldLabelDragEventListeners(bool unhookAvailableFieldLabels, bool unhookGroupedFieldLabels)
        {
            if (unhookAvailableFieldLabels)
            {
                foreach (GroupByAreaFieldLabel groupByAreaFieldLabel in this.AvailableFieldLabels)
                    this.UnhookFieldLabelDragEventListeners(groupByAreaFieldLabel);
            }

            if (unhookGroupedFieldLabels)
            {
                foreach (GroupByAreaFieldLabel groupByAreaFieldLabel in this.GroupedFieldLabels)
                    this.UnhookFieldLabelDragEventListeners(groupByAreaFieldLabel);
            }
        }

				#endregion UnhookAllFieldLabelDragEventListeners

				#region VerifyAvailableFieldLabelsArea

		private void VerifyAvailableFieldLabelsArea()
		{
			DependencyObject area = base.GetTemplateChild("PART_AvailableFieldLabelsArea");

			if (area == null)
				return;

			FrameworkElement fe = area as FrameworkElement;
			if (fe == null)
				throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_4", "FrameworkElement", "PART_AvailableFieldLabelsArea", "GroupByArea", area.GetType( ).Name ) );

			this._availableFieldLabelsArea = fe;
		}

				#endregion //VerifyAvailableFieldLabelsArea	

				#region VerifyGroupedFieldLabelsArea

		private void VerifyGroupedFieldLabelsArea()
		{
			DependencyObject area = base.GetTemplateChild("PART_GroupedFieldLabelsArea");

			if (area == null)
				return;

			FrameworkElement fe = area as FrameworkElement;
			if (fe == null)
				throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_4", "FrameworkElement", "PART_GroupedFieldLabelsArea", "GroupByArea", area.GetType( ).Name ) );

			this._groupedFieldLabelsArea = fe;
		}

				#endregion //VerifyGroupedFieldLabelsArea	
    
			#endregion Private Methods

			#region Static Methods

			#endregion Static Methods

        #endregion Methods
    }

    #endregion GroupByArea Class

    #region GroupByAreaFieldLabel Class

    /// <summary>
    /// Used to represent a <see cref="Field"/> in the group by area.
    /// </summary>
	/// <remarks>
	/// <p class="body"><see cref="GroupByAreaFieldLabel"/> is derived from <see cref="System.Windows.Controls.Primitives.Thumb"/> and represents a <see cref="Field"/>
    /// in the <see cref="DataPresenterBase.DefaultFieldLayout"/>.  The default style for the <see cref="GroupByArea"/> contains a 
    /// <see cref="System.Windows.Controls.ListBox"/> which is bound to a collection of GroupByAreaFieldLabels.  Since the elements in the
	/// collection are derived from <see cref="System.Windows.Controls.Primitives.Thumb"/> they automatically support drag and drop.  When the <see cref="GroupByArea"/> creates these elements it 
	/// listens to the drag events and automatically groups/ungroups the field represented by the GroupByAreaFieldLabel being dragged.</p>
	/// <p class="note"><b>Note: </b>GroupByAreaFieldLabels are automatically created by the <see cref="GroupByArea"/> when needed.
	/// They are not intended for use outside of these controls and you should never need to create one directly.</p>
	/// </remarks>
	/// <seealso cref="GroupByArea"/>
    /// <seealso cref="GroupByAreaFieldLabelCollection"/>
	/// <seealso cref="DataPresenterBase.DefaultFieldLayout"/>
	/// <seealso cref="Field"/>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class GroupByAreaFieldLabel : Thumb
    {
        #region Member Variables

        private GroupByArea			_groupByArea = null;

        #endregion //Member Variables

        #region Constructor

		static GroupByAreaFieldLabel()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(GroupByAreaFieldLabel), new FrameworkPropertyMetadata(typeof(GroupByAreaFieldLabel)));
		}

		internal GroupByAreaFieldLabel(GroupByArea groupByArea, Field field, Style defaultStyle)
        {
            this._groupByArea = groupByArea;
            this.SetValue(FieldPropertyKey, field);

			// initialize the style
			this.SetStyle(defaultStyle);
		}

        #endregion //Constructor

		#region Base class overrides

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="GroupByAreaFieldLabel"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="Infragistics.Windows.Automation.Peers.DataPresenter.GroupByAreaFieldLabelAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new GroupByAreaFieldLabelAutomationPeer(this);
		}
		#endregion //OnCreateAutomationPeer

		#endregion //Base class overrides

        #region Properties

			#region Public Properties

				#region Field

        private static readonly DependencyPropertyKey FieldPropertyKey =
            DependencyProperty.RegisterReadOnly("Field",
            typeof(Field), typeof(GroupByAreaFieldLabel), new FrameworkPropertyMetadata());

        /// <summary>
        /// Identifies the 'Field' dependency property
        /// </summary>
        public static readonly DependencyProperty FieldProperty =
            FieldPropertyKey.DependencyProperty;

        /// <summary>
		/// The actual <see cref="Infragistics.Windows.DataPresenter.Field"/> associated with this <see cref="GroupByAreaFieldLabel"/>. (read-only).
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.Field"/>
		/// <seealso cref="GroupByAreaFieldLabel"/>
        //[Description("The associated field (read-only)")]
        //[Category("Behavior")]
        public Field Field
        {
            get { return (Field)this.GetValue(GroupByAreaFieldLabel.FieldProperty); }
        }

				#endregion //Field

			#endregion Public Properties

			#region Internal Properties

				#region RectInGroupByAreaCoordinates

        internal Rect RectInGroupByAreaCoordinates
        {
            get
            {
                Point location = Utilities.TransformPointToAncestorCoordinates(new Point(0, 0),
                                                                                   this,
                                                                                   this._groupByArea);
                if (location.X < 0)
                    location.X += 0;

                location = this.TranslatePoint(new Point(0, 0), this._groupByArea);

                if (location.X < 0)
                    location.X += 0;

                return new Rect(location, new Size(this.ActualWidth, this.ActualHeight));
            }
        }

				#endregion RectInGroupByAreaCoordinates

				#region StyleVersionNumber

        internal static readonly DependencyProperty StyleVersionNumberProperty = DependencyProperty.Register("StyleVersionNumber",
            typeof(int), typeof(GroupByAreaFieldLabel), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(StyleVersionNumberChanged)));

        private static void StyleVersionNumberChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            GroupByAreaFieldLabel groupByAreaFieldLabel = target as GroupByAreaFieldLabel;

			if (groupByAreaFieldLabel != null && 
				groupByAreaFieldLabel._groupByArea != null)
                groupByAreaFieldLabel.SetStyle(groupByAreaFieldLabel._groupByArea.GetDefaultGroupByFieldLabelStyle());
        }

        internal int StyleVersionNumber
        {
            get { return (int)this.GetValue(GroupByAreaFieldLabel.StyleVersionNumberProperty); }
            set { this.SetValue(GroupByAreaFieldLabel.StyleVersionNumberProperty, value); }
        }

				#endregion //StyleVersionNumber

			#endregion Internal Properties

        #endregion Properties

		#region Methods

		#region SetStyle

		private void SetStyle( Style defaultStyle )
		{
			Style style = null;

			// If a style selector was supplied, use it.
			if (this._groupByArea.DataPresenter.GroupByAreaFieldLabelStyleSelector != null)
				style = this._groupByArea.DataPresenter.GroupByAreaFieldLabelStyleSelector.SelectStyle(this.Field, this);


			// If a style was supplied, use it.
			if (style == null)
				style = this._groupByArea.DataPresenter.GroupByAreaFieldLabelStyle;

			// either set or clear the property
			if (style == null)
			{
				if (defaultStyle != null)
					this.SetValue(GroupByAreaFieldLabel.StyleProperty, defaultStyle);
				else
					this.ClearValue(GroupByAreaFieldLabel.StyleProperty);
			}
			else
				this.SetValue(GroupByAreaFieldLabel.StyleProperty, style);

		}

		#endregion //SetFieldLabelStyle

		#endregion //Methods	
    
        #region Events

			#region AddedToAvailableList

        /// <summary>
        /// Event ID for the 'AddedToAvailableList' routed event
        /// </summary>
        public static readonly RoutedEvent AddedToAvailableListEvent =
            EventManager.RegisterRoutedEvent("AddedToAvailableList", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GroupByAreaFieldLabel));

		/// <summary>
		/// Occurs when the <see cref="GroupByAreaFieldLabel"/> is added to the <see cref="GroupByArea.AvailableFieldLabels"/> collection.
		/// </summary>
		/// <see cref="GroupByAreaFieldLabel"/>
		/// <see cref="GroupByArea.AvailableFieldLabels"/>
		internal protected virtual void OnAddedToAvailableList()
        {
            RoutedEventArgs args = new RoutedEventArgs(GroupByAreaFieldLabel.AddedToAvailableListEvent, this);
            this.RaiseEvent(args);
        }

        /// <summary>
		/// Occurs when the <see cref="GroupByAreaFieldLabel"/> is added to the <see cref="GroupByArea.AvailableFieldLabels"/> collection.
        /// </summary>
		/// <see cref="GroupByAreaFieldLabel"/>
		/// <see cref="GroupByArea.AvailableFieldLabels"/>
        //[Description("Occurs when the GroupByAreaFieldLabel is added to the AvailableFieldLabels collection.")]
        //[Category("Behavior")]
        public event RoutedEventHandler AddedToAvailableList
        {
            add
            {
                base.AddHandler(GroupByAreaFieldLabel.AddedToAvailableListEvent, value);
            }
            remove
            {
                base.RemoveHandler(GroupByAreaFieldLabel.AddedToAvailableListEvent, value);
            }
        }

			#endregion //AddedToAvailableList

			#region AddedToGroupedList

        /// <summary>
        /// Event ID for the 'AddedToGroupedList' routed event
        /// </summary>
        public static readonly RoutedEvent AddedToGroupedListEvent =
            EventManager.RegisterRoutedEvent("AddedToGroupedList", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GroupByAreaFieldLabel));

		/// <summary>
		/// Occurs when the <see cref="GroupByAreaFieldLabel"/> is added to the <see cref="GroupByArea.GroupedFieldLabels"/> collection.
		/// </summary>
		/// <see cref="GroupByAreaFieldLabel"/>
		/// <see cref="GroupByArea.GroupedFieldLabels"/>
		internal protected virtual void OnAddedToGroupedList()
        {
            RoutedEventArgs args = new RoutedEventArgs(GroupByAreaFieldLabel.AddedToGroupedListEvent, this);
            this.RaiseEvent(args);
        }

		/// <summary>
		/// Occurs when the <see cref="GroupByAreaFieldLabel"/> is added to the <see cref="GroupByArea.GroupedFieldLabels"/> collection.
		/// </summary>
		/// <see cref="GroupByAreaFieldLabel"/>
		/// <see cref="GroupByArea.GroupedFieldLabels"/>
		//[Description("Occurs when the GroupByAreaFieldLabel is added to the GroupedFieldLabels collection.")]
        //[Category("Behavior")]
        public event RoutedEventHandler AddedToGroupedList
        {
            add
            {
                base.AddHandler(GroupByAreaFieldLabel.AddedToGroupedListEvent, value);
            }
            remove
            {
                base.RemoveHandler(GroupByAreaFieldLabel.AddedToGroupedListEvent, value);
            }
        }

			#endregion //AddedToGroupedList

			#region RemovedFromAvailableList

        /// <summary>
        /// Event ID for the 'RemovedFromAvailableList' routed event
        /// </summary>
        public static readonly RoutedEvent RemovedFromAvailableListEvent =
            EventManager.RegisterRoutedEvent("RemovedFromAvailableList", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GroupByAreaFieldLabel));

		/// <summary>
		/// Occurs when the <see cref="GroupByAreaFieldLabel"/> is removed from the <see cref="GroupByArea.AvailableFieldLabels"/> collection.
		/// </summary>
		/// <see cref="GroupByAreaFieldLabel"/>
		/// <see cref="GroupByArea.AvailableFieldLabels"/>
		internal protected virtual void OnRemovedFromAvailableList()
        {
            RoutedEventArgs args = new RoutedEventArgs(GroupByAreaFieldLabel.RemovedFromAvailableListEvent, this);
            this.RaiseEvent(args);
        }

		/// <summary>
		/// Occurs when the <see cref="GroupByAreaFieldLabel"/> is removed from the <see cref="GroupByArea.AvailableFieldLabels"/> collection.
		/// </summary>
		/// <see cref="GroupByAreaFieldLabel"/>
		/// <see cref="GroupByArea.AvailableFieldLabels"/>
		//[Description("Occurs when the GroupByAreaFieldLabel is removed from the AvailableFieldLabels collection.")]
        //[Category("Behavior")]
        public event RoutedEventHandler RemovedFromAvailableList
        {
            add
            {
                base.AddHandler(GroupByAreaFieldLabel.RemovedFromAvailableListEvent, value);
            }
            remove
            {
                base.RemoveHandler(GroupByAreaFieldLabel.RemovedFromAvailableListEvent, value);
            }
        }

			#endregion //RemovedFromAvailableList

        #endregion //Events
    }

    #endregion GroupByAreaFieldLabel Class

    #region GroupByAreaFieldLabelCollection Class

    /// <summary>
    /// A read-only collection of <see cref="GroupByAreaFieldLabel"/>s
    /// </summary>
	/// <remarks>
    /// <p class="body">The <see cref="GroupByArea"/> exposes 2 properties of this type: <see cref="GroupByArea.AvailableFieldLabels"/> and <see cref="GroupByArea.GroupedFieldLabels"/>.</p>
	/// <p class="note"><b>Note: </b>The GroupByAreaFieldLabelCollection is automatically created by the <see cref="GroupByArea"/> when needed.
	/// It is not intended for use outside of these controls and you should never need to create it directly.</p>
	/// </remarks>
	/// <seealso cref="GroupByArea"/>
    /// <seealso cref="GroupByAreaFieldLabel"/>
	/// <seealso cref="GroupByArea.AvailableFieldLabels"/>
	/// <seealso cref="GroupByArea.GroupedFieldLabels"/>
	public class GroupByAreaFieldLabelCollection : ReadOnlyCollection<GroupByAreaFieldLabel>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Member Variables

        private List<GroupByAreaFieldLabel>			_list = null;
        private NotifyCollectionChangedEventHandler _collectionChangedHandler = null;
        private PropertyChangedEventHandler			_propertyChangedHandler;

        #endregion //Member Variables

        #region Constructor

        internal static GroupByAreaFieldLabelCollection Create()
        {
            return new GroupByAreaFieldLabelCollection(new List<GroupByAreaFieldLabel>());
        }

        internal GroupByAreaFieldLabelCollection(List<GroupByAreaFieldLabel> list)
            : base(list)
        {
            this._list = list;
        }

        #endregion Constructor

        #region Properties

			#region InternalList

        internal List<GroupByAreaFieldLabel> InternalList { get { return this._list; } }

			#endregion //InternalList

        #endregion Properties

        #region Methods

			#region Protected Methods

				#region OnCollectionChanged

        /// <summary>
        /// Raises the CollectionChanged event
        /// </summary>
        internal protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            if (this._collectionChangedHandler != null)
            {
				// AS 7/2/07
				// IList<T> isn't an IList so checking/casting for this type really won't treat it as a list for the action.
				//
                //if (item is IList<GroupByAreaFieldLabel>)
                //    this._collectionChangedHandler(this, new NotifyCollectionChangedEventArgs(action, (IList<GroupByAreaFieldLabel>)item, index));
				Debug.Assert(item is IList<GroupByAreaFieldLabel> == false, "This isn't an IList but its a list of items!");
                if (item is System.Collections.IList)
                    this._collectionChangedHandler(this, new NotifyCollectionChangedEventArgs(action, (System.Collections.IList)item, index));
                else
                    this._collectionChangedHandler(this, new NotifyCollectionChangedEventArgs(action, item, index));
            }

        }

				#endregion //OnCollectionChanged

				#region OnPropertyChanged

        /// <summary>
        /// Called when a property changes.
        /// </summary>
        /// <param name="propertyName"></param>
        internal protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this._propertyChangedHandler != null)
                this._propertyChangedHandler(this, new PropertyChangedEventArgs(propertyName));
        }

				#endregion //OnPropertyChanged

			#endregion Protected Methods

			#region Internal Methods

				#region InternalAdd

        internal int InternalAdd(GroupByAreaFieldLabel groupByAreaLabel, bool notifyCollectionChanged)
        {
            int index = this.InternalAdd(groupByAreaLabel);

            if (notifyCollectionChanged)
                this.InternalReset();

            return index;
        }

        internal int InternalAdd(GroupByAreaFieldLabel groupByAreaLabel)
        {
            return this.InternalInsert(this._list.Count, groupByAreaLabel);
        }

				#endregion //InternalAdd

				#region InternalClear

        internal void InternalClear(bool notifyCollectionChanged)
        {
            this._list.Clear();

            if (notifyCollectionChanged)
                this.InternalReset();
        }

        internal void InternalClear()
        {
            this._list.Clear();
        }

				#endregion //InternalClear

				#region InternalInsert

        internal int InternalInsert(int index, GroupByAreaFieldLabel groupByAreaLabel, bool notifyCollectionChanged)
        {
            this.InternalInsert(index, groupByAreaLabel);

            if (notifyCollectionChanged)
                this.InternalReset();

            return index;
        }

        internal int InternalInsert(int index, GroupByAreaFieldLabel groupByAreaLabel)
        {
            this._list.Insert(index, groupByAreaLabel);

            return index;
        }

				#endregion //InternalInsert

				#region InternalRemove

        internal void InternalRemove(GroupByAreaFieldLabel groupByAreaLabel, bool notifyCollectionChanged)
        {
            this.InternalRemove(groupByAreaLabel);

            if (notifyCollectionChanged)
                this.InternalReset();
        }

        internal void InternalRemove(GroupByAreaFieldLabel groupByAreaLabel)
        {
            this._list.Remove(groupByAreaLabel);
        }

				#endregion //InternalRemove

				#region InternalReset

        internal void InternalReset()
        {
            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(NotifyCollectionChangedAction.Reset, null, -1);
        }

				#endregion //InternalReset

			#endregion //Internal Methods

        #endregion Methods

        #region INotifyCollectionChanged Members

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                this._collectionChangedHandler = System.Delegate.Combine(this._collectionChangedHandler, value) as NotifyCollectionChangedEventHandler;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                this._collectionChangedHandler = System.Delegate.Remove(this._collectionChangedHandler, value) as NotifyCollectionChangedEventHandler;
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                this._propertyChangedHandler = System.Delegate.Combine(this._propertyChangedHandler, value) as PropertyChangedEventHandler;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                this._propertyChangedHandler = System.Delegate.Remove(this._propertyChangedHandler, value) as PropertyChangedEventHandler;
            }
        }

        #endregion
    }

    #endregion GroupByAreaFieldLabelCollection Class

	#region GroupByAreaFieldListBox

	/// <summary>
    /// A System.Windows.Controls.ListBox that contains the collection of <see cref="GroupByArea.AvailableFieldLabels"/> or <see cref="GroupByArea.GroupedFieldLabels"/> for the 
    /// <see cref="GroupByArea"/> of a <see cref="DataPresenterBase"/> derived control such as <see cref="XamDataGrid"/>, <see cref="XamDataCarousel"/> or <see cref="XamDataPresenter"/>.
	/// </summary>
	/// <seealso cref="GroupByArea.AvailableFieldLabels"/>
	/// <seealso cref="GroupByArea.GroupedFieldLabels"/>
	/// <seealso cref="GroupByArea"/>
    /// <seealso cref="XamDataGrid"/>
    /// <seealso cref="XamDataCarousel"/>
    /// <seealso cref="XamDataPresenter"/>
    //[Description("A listBox that contains the list of AvailableFieldLabels or GroupedFieldLabels for the GroupByArea of a XamDataPresenter, XamDataGrid or XamDataCarousel.")]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class GroupByAreaFieldListBox : ListBox
	{
		static GroupByAreaFieldListBox()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(GroupByAreaFieldListBox), new FrameworkPropertyMetadata(typeof(GroupByAreaFieldListBox)));
		}

		#region Base class overrides

			// [JM 04-27-07] Return false to force the ItemsControl base class to create a wrapper
			// for each item in the list so that the VirtualizingStackPanel used by the listbox will
			// viritualize items.
			#region IsItemItsOwnContainerOverride

		/// <summary>
		/// Determines if the specified item is (or is eligible to be) its own container.
		/// </summary>
		/// <param name="item">The item to check.</param>
		/// <returns>True if the item serves as its own container and does not need one provided for it, false to have the listbox geberate a wrapper.</returns>
		protected override bool IsItemItsOwnContainerOverride(object item)
		{
			return false;
		}

			#endregion //IsItemItsOwnContainerOverride

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="GroupByAreaFieldListBox"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="Infragistics.Windows.Automation.Peers.DataPresenter.GroupByAreaFieldListBoxAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new GroupByAreaFieldListBoxAutomationPeer(this);
		}
				#endregion //OnCreateAutomationPeer

		// JM 10-23-09 TFS23784 Added.
		#region PrepareContainerForItemOverride

		/// <summary>
		/// Prepares the container to 'host' the item.
		/// </summary>
		/// <param name="element">The container that wraps the item.</param>
		/// <param name="item">The data item that is wrapped.</param>
		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			base.PrepareContainerForItemOverride(element, item);
			// JM 10-26-09 TFS24192 - Check for null Field
			//if (item is GroupByAreaFieldLabel && element is FrameworkElement)
			if (item is GroupByAreaFieldLabel && element is FrameworkElement && ((GroupByAreaFieldLabel)item).Field != null)
				((FrameworkElement)element).SetValue(AutomationProperties.NameProperty, ((GroupByAreaFieldLabel)item).Field.Name);
		}

		#endregion //PrepareContainerForItemOverride

		#endregion //Base class overrides
	}

	#endregion //GroupByAreaFieldListBox
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