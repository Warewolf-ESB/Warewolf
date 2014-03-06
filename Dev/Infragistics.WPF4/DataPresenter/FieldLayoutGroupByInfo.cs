using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using Infragistics.Windows.Helpers;
using System.Collections.ObjectModel;
using Infragistics.Windows.Internal;
using System.Diagnostics;
using Infragistics.Collections;

namespace Infragistics.Windows.DataPresenter
{
    /// <summary>
    /// A control used inside the <see cref="GroupByAreaMulti"/> in a <see cref="XamDataPresenter"/>, <see cref="XamDataGrid"/> or <see cref="XamDataCarousel"/> that presents a UI for performing Outlook style Grouping 
    /// for a specific <see cref="FieldLayout"/>s.
    /// </summary>
    /// <seealso cref="GroupByAreaMulti"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DefaultFieldLayout"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.GroupByAreaMode"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.GroupByAreaMulti"/>
    /// <seealso cref="FieldLayout"/>
	[StyleTypedProperty(Property = "FieldLayoutDescriptionTemplate", StyleTargetType = typeof(ContentControl))]
	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CrossBandGrouping, Version = FeatureInfo.Version_9_2)]
    public class FieldLayoutGroupByInfo : DependencyObject
    {
        #region Private Members

        private ObservableCollectionExtended<Field> _groupByFields;
        private GroupByFieldCollection _readOnlyGroupByFields;
        private PropertyValueTracker _recordIndentVersionTracker;

        // JJD 6/3/09 - TFS17949 - added
        private PropertyValueTracker _fieldLayoutVersionTracker;

        #endregion //Private Members	

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldLayoutGroupByInfo"/> class
        /// </summary>
        public FieldLayoutGroupByInfo()
        {
        }

        #endregion //Constructor	

        #region Properties

            #region Public Properties

                #region FieldLayout

        private static readonly DependencyPropertyKey FieldLayoutPropertyKey =
            DependencyProperty.RegisterReadOnly("FieldLayout",
            typeof(FieldLayout), typeof(FieldLayoutGroupByInfo), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="FieldLayout"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FieldLayoutProperty =
            FieldLayoutPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the associated <see cref="FieldLayout"/> (read-only).
        /// </summary>
        /// <seealso cref="FieldLayoutProperty"/>
        //[Description("Returns the associated FieldLayout (read-only).")]
        //[Category("Behavior")]
        [ReadOnly(true)]
        [Bindable(true)]
        public FieldLayout FieldLayout
        {
            get
            {
                return (FieldLayout)this.GetValue(FieldLayoutGroupByInfo.FieldLayoutProperty);
            }
        }

                #endregion //FieldLayout

				#region FieldLayoutDescriptionTemplate

		/// <summary>
		/// Identifies the <see cref="FieldLayoutDescriptionTemplate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FieldLayoutDescriptionTemplateProperty = 
			GroupByAreaMulti.FieldLayoutDescriptionTemplateProperty.AddOwner(typeof(FieldLayoutGroupByInfo));

		/// <summary>
		/// Returns the DataTemplate to use for the ContentControl element that represents the FieldLayout description (read only).
		/// </summary>
		/// <seealso cref="FieldLayoutDescriptionTemplateProperty"/>
		/// <seealso cref="FieldLayoutDescriptionVisibility"/>
		/// <seealso cref="FieldLayout"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.Description"/>
		//[Description("Returns the DataTemplate to use for the ContentControl element that represents the FieldLayout description (read only).")]
		//[Category("Appearance")]
		[Bindable(true)]
		public DataTemplate FieldLayoutDescriptionTemplate
		{
			get
			{
				return (DataTemplate)this.GetValue(FieldLayoutGroupByInfo.FieldLayoutDescriptionTemplateProperty);
			}
			set
			{
				this.SetValue(FieldLayoutGroupByInfo.FieldLayoutDescriptionTemplateProperty, value);
			}
		}

				#endregion //FieldLayoutDescriptionTemplate

                #region FieldLayoutDescriptionVisibility


        private static readonly DependencyPropertyKey FieldLayoutDescriptionVisibilityPropertyKey =
            DependencyProperty.RegisterReadOnly("FieldLayoutDescriptionVisibility",
            typeof(Visibility), typeof(FieldLayoutGroupByInfo), new FrameworkPropertyMetadata(KnownBoxes.VisibilityCollapsedBox));

        /// <summary>
        /// Identifies the <see cref="FieldLayoutDescriptionVisibility"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FieldLayoutDescriptionVisibilityProperty =
            FieldLayoutDescriptionVisibilityPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the visibility of the FieldLayout's Description (read-only)
        /// </summary>
        /// <seealso cref="FieldLayoutDescriptionVisibilityProperty"/>
        //[Description("Gets/sets the visibility of the FieldLayout's Description (read-only)")]
        //[Category("Behavior")]
        [ReadOnly(true)]
        [Bindable(true)]
        public Visibility FieldLayoutDescriptionVisibility
        {
            get
            {
                return (Visibility)this.GetValue(FieldLayoutGroupByInfo.FieldLayoutDescriptionVisibilityProperty);
            }
        }

                #endregion //FieldLayoutDescriptionVisibility

                #region GroupByFields

        /// <summary>
        /// Returns a read-only collection of all <see cref="Field"/>s that this <see cref="Infragistics.Windows.DataPresenter.FieldLayout"/> is grouped by.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [Bindable(true)]
        [ReadOnly(true)]
        public GroupByFieldCollection GroupByFields
        {
            get
            {
                if (this._groupByFields == null)
                {
                    this._groupByFields = new ObservableCollectionExtended<Field>();

                    this._readOnlyGroupByFields = new GroupByFieldCollection(this._groupByFields);
                }

                return this._readOnlyGroupByFields;
            }
        }

                #endregion //GroupByFields	
    
            #endregion //Public Properties	
    
        #endregion //Properties	
 
        #region Methods

            #region Internal Methods

                #region InitializeFieldLayout

        internal void InitializeFieldLayout(FieldLayout fl, bool displayDescription)
        {
            FieldLayout oldFieldLayout = this.FieldLayout;

            if (displayDescription)
                this.SetValue(FieldLayoutGroupByInfo.FieldLayoutDescriptionVisibilityPropertyKey, KnownBoxes.VisibilityVisibleBox);
            else
                this.ClearValue(FieldLayoutGroupByInfo.FieldLayoutDescriptionVisibilityPropertyKey);

            if (fl == oldFieldLayout)
                return;

            if (this._groupByFields != null)
                this._groupByFields.Clear();

            this._recordIndentVersionTracker = null;
            this._fieldLayoutVersionTracker = null;

            if (fl == null)
            {
                this.ClearValue(FieldLayoutGroupByInfo.FieldLayoutPropertyKey);
            }
            else
            {
                this.SetValue(FieldLayoutGroupByInfo.FieldLayoutPropertyKey, fl);

                this._recordIndentVersionTracker = new PropertyValueTracker(fl, "RecordIndentVersion", this.OnGroupByVersionChanged);
                
                // JJD 6/3/09 - TFS17949 - added
                this._fieldLayoutVersionTracker = new PropertyValueTracker(fl, FieldLayout.InternalVersionProperty, this.OnFieldLayoutVersionChanged);

                this.SynchronizeGroupByFields(false);
            }

        }

                #endregion //InitializeFieldLayout

                #region OnGroupByVersionChanged

        internal void SynchronizeGroupByFields(bool reload)
        {
            List<Field> groupedFields = new List<Field>();

            FieldLayout fl = this.FieldLayout;

            if (fl != null)
            {
                int countofGroupByFields = fl.SortedFields.CountOfGroupByFields;

                for (int i = 0; i < countofGroupByFields; i++)
                {
                    Field field = fl.SortedFields[i].Field;

                    Debug.Assert(field != null, "FieldSortDescription should be initialized at this point");

                    if (field != null)
                        groupedFields.Add(field);
                }
            }

            // clear the setters on the old list of fields
            foreach (Field field in this.GroupByFields)
            {
                field.ClearValue(GroupByAreaMulti.IsFirstInListPropertyKey);
                field.ClearValue(GroupByAreaMulti.IsLastInListPropertyKey);
            }

            bool beginUpdateCalled = false;

            if (reload && this._groupByFields.Count > 0)
            {
                beginUpdateCalled = true;
                this._groupByFields.BeginUpdate();
                this._groupByFields.Clear();
            }

            try
            {
                bool areGroupedFieldsTheSame = false;

                int count = groupedFields.Count;

                if (count == this.GroupByFields.Count)
                {
                    areGroupedFieldsTheSame = true;

                    for (int i = 0; i < count; i++)
                    {
                        if (groupedFields[i] != this._groupByFields[i])
                        {
                            areGroupedFieldsTheSame = false;
                            break;
                        }
                    }

                }

                for (int i = 0; i < count; i++)
                {
                    Field field = groupedFields[i];

                    // set the IsFirstInList attached proprty
                    if (i == 0)
                        field.SetValue(GroupByAreaMulti.IsFirstInListPropertyKey, KnownBoxes.TrueBox);
                    else
                        field.ClearValue(GroupByAreaMulti.IsFirstInListPropertyKey);

                    // set the IsLastInList attached proprty
                    if (i == count - 1)
                        field.SetValue(GroupByAreaMulti.IsLastInListPropertyKey, KnownBoxes.TrueBox);
                    else
                        field.ClearValue(GroupByAreaMulti.IsLastInListPropertyKey);
                }

                if (areGroupedFieldsTheSame)
                    return;

                if (!beginUpdateCalled)
                {
                    this._groupByFields.BeginUpdate();
                    beginUpdateCalled = true;
                }

                this._groupByFields.Clear();
                this._groupByFields.AddRange(groupedFields);
            }
            finally
            {
                if (beginUpdateCalled)
                    this._groupByFields.EndUpdate();
            }

        }

                #endregion //OnGroupByVersionChanged

            #endregion //Internal Methods	
    
            #region Private Methods

                // JJD 6/3/09 - TFS17949 - added
                #region OnFieldLayoutVersionChanged

        private void OnFieldLayoutVersionChanged()
        {
            this.SynchronizeGroupByFields(true);
        }

                #endregion //OnGroupByVersionChanged

                #region OnGroupByVersionChanged

        private void OnGroupByVersionChanged()
        {
            this.SynchronizeGroupByFields(false);
        }

                #endregion //OnGroupByVersionChanged

            #endregion //Private Methods	
    
        #endregion //Methods	
    
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