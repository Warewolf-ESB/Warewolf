using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Markup;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Infragistics.Windows;

namespace Infragistics.Windows.Themes
{
	#region DataPresenterResourceSet<T> base class

	/// <summary>
	/// Abstract base class used to supply style resources for a specific look for DataPresenters.
	/// </summary>
	/// <typeparam name="T">A <see cref="ResourceSetLocator"/> derived class that must have a public parameterless constructor.</typeparam>
	public abstract class DataPresenterResourceSet<T> : ResourceSet where T : ResourceSetLocator, new()
	{
		#region Constants

		// AS 11/6/07 ThemeGroupingName
		//static internal readonly string GroupingName = "DataPresenter";
		internal const string GroupingName = "DataPresenter";

		#endregion //Constants

		#region Base class overrides

		#region Grouping

		/// <summary>
		/// Returns the grouping that the resources are applied to (read-only)
		/// </summary>
		/// <remarks>
		/// Examples: DataPresenterBase, Editors, DataPresenterBase, WPF etc.
		/// </remarks>
		public override string Grouping
		{
			get { return Location.Grouping; }
		}

		#endregion //Grouping

		#region Theme

		/// <summary>
		/// Returns the name of the look (read-only)
		/// </summary>
		public override string Theme
		{
			get
			{
				return Location.Theme;
			}
		}

		#endregion //Theme

		#region Resources

		/// <summary>
		/// Returns the ResourceDictionary containing the associated styles (read-only)
		/// </summary>
		public override ResourceDictionary Resources
		{
			get
			{
				//return ResourcesInternal;
				ResourceDictionary rd = ResourcesInternal;

				// JJD 7/23/07 - ResourceWasher support
				// Call VerifyResources after the initial load so that we can delay the hydrating
				// of the resources by a ResourceWasher until this theme is actually used
				this.VerifyResources();

				return rd;
			}
		}

		#endregion //Resources

		#endregion //Base class overrides

		#region Static Properties

		#region Private Propeties

		#region Location

		private static ResourceSetLocator g_Location;

		// AS 5/7/08
		/// <summary>
		/// Returns the <see cref="ResourceSetLocator"/> that describes the theme information for the resource set.
		/// </summary>
		//private static ResourceSetLocator Location
		public static ResourceSetLocator Location
		{
			get
			{
				if (g_Location == null)
					g_Location = new T();

				return g_Location;
			}
		}

		#endregion //Location

		#region ResourcesInternal

		private static ResourceDictionary g_ResourcesInternal;

		private static ResourceDictionary ResourcesInternal
		{
			get
			{
				if (g_ResourcesInternal == null)
				{
					g_ResourcesInternal = Utilities.CreateResourceSetDictionary(Location.Assembly, Location.ResourcePath);
				}

				return g_ResourcesInternal;
			}
		}

		#endregion //ResourcesInternal

		#endregion //Private Propeties

		#region Public Properties

		// JM NA 10.1 CardView
		#region CardHeaderPresenter

		private static Style g_CardHeaderPresenter;

		/// <summary>
		/// The style for the <see cref="CardHeaderPresenter"/> in <see cref="Infragistics.Windows.DataPresenter.CardView"/>of a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style CardHeaderPresenter
		{
			get
			{
				if (g_CardHeaderPresenter == null)
					g_CardHeaderPresenter = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.CardHeaderPresenter)) as Style;
				
				return g_CardHeaderPresenter;
			}
		}

		#endregion //CardHeaderPresenter

		// JM NA 10.1 CardView
		#region CardViewCard

		private static Style g_CardViewCard;

		/// <summary>
		/// The style for the <see cref="Infragistics.Windows.DataPresenter.CardViewCard"/> in <see cref="Infragistics.Windows.DataPresenter.CardView"/>of a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style CardViewCard
		{
			get
			{
				if (g_CardViewCard == null)
					g_CardViewCard = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.CardViewCard)) as Style;

				return g_CardViewCard;
			}
		}

		#endregion //CardViewCard


		#region CarouselBreadcrumbControl

		private static Style g_CarouselBreadcrumbControl;

		/// <summary>
		/// The style for a CarouselBreadcrumbControl.
		/// </summary>
		public static Style CarouselBreadcrumbControl
		{
			get
			{
				if (g_CarouselBreadcrumbControl == null)
					g_CarouselBreadcrumbControl = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.CarouselBreadcrumbControl)) as Style;

				return g_CarouselBreadcrumbControl;
			}
		}

				#endregion //CarouselBreadcrumbControl

		#region CarouselBreadcrumb

		private static Style g_CarouselBreadcrumb;

		/// <summary>
		/// The style for a CarouselBreadcrumb.
		/// </summary>
		public static Style CarouselBreadcrumb
		{
			get
			{
				if (g_CarouselBreadcrumb == null)
					g_CarouselBreadcrumb = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.CarouselBreadcrumb)) as Style;

				return g_CarouselBreadcrumb;
			}
		}

		#endregion //CarouselBreadcrumb

		#region CarouselItem

		private static Style g_CarouselItem;

		/// <summary>
		/// The style for item wrappers used in carousel format of a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style CarouselItem
		{
			get
			{
				if (g_CarouselItem == null)
					g_CarouselItem = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.CarouselItem)) as Style;

				return g_CarouselItem;
			}
		}

		#endregion //CarouselItem

        #region FieldDragIndicator

        private static Style g_FieldDragIndicator;

		/// <summary>
		/// The style for a cell in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style FieldDragIndicator
		{
			get
			{
                if (g_FieldDragIndicator == null)
                    g_FieldDragIndicator = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.FieldDragIndicator)) as Style;

                return g_FieldDragIndicator;
			}
		}

        #endregion //FieldDragIndicator

        // JJD 1/12/09 - NA 2009 vol 1 - record filtering
		#region CustomFilterSelectionControl

		private static Style g_CustomFilterSelectionControl;

		/// <summary>
        /// The style for a <see cref="Infragistics.Windows.DataPresenter.CustomFilterSelectionControl"/> 
		/// </summary>
		public static Style CustomFilterSelectionControl
		{
			get
			{
				if (g_CustomFilterSelectionControl == null)
					g_CustomFilterSelectionControl = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.CustomFilterSelectionControl)) as Style;

				return g_CustomFilterSelectionControl;
			}
		}

		#endregion //CustomFilterSelectionControl

        // JJD 1/12/09 - NA 2009 vol 1 - record filtering
		#region FilterButton

		private static Style g_FilterButton;

		/// <summary>
        /// The style for a <see cref="Infragistics.Windows.DataPresenter.FilterButton"/> 
		/// </summary>
		public static Style FilterButton
		{
			get
			{
				if (g_FilterButton == null)
					g_FilterButton = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.FilterButton)) as Style;

				return g_FilterButton;
			}
		}

		#endregion //FilterButton

        // JJD 1/12/09 - NA 2009 vol 1 - fixed fields
		#region FixedFieldButton

		private static Style g_FixedFieldButton;

		/// <summary>
        /// The style for a <see cref="Infragistics.Windows.DataPresenter.FixedFieldButton"/> 
		/// </summary>
		public static Style FixedFieldButton
		{
			get
			{
				if (g_FixedFieldButton == null)
					g_FixedFieldButton = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.FixedFieldButton)) as Style;

				return g_FixedFieldButton;
			}
		}

		#endregion //FixedFieldButton

        // JJD 6/10/09 - NA 2009 vol 2 - RecordFixing
		#region FixedRecordButton

		private static Style g_FixedRecordButton;

		/// <summary>
        /// The style for a <see cref="Infragistics.Windows.DataPresenter.FixedRecordButton"/> 
		/// </summary>
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
        public static Style FixedRecordButton
		{
			get
			{
				if (g_FixedRecordButton == null)
					g_FixedRecordButton = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.FixedRecordButton)) as Style;

				return g_FixedRecordButton;
			}
		}

		#endregion //FixedRecordButton



        #region CellPresenter

        private static Style g_Cell;

		/// <summary>
		/// The style for a cell in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style CellPresenter
		{
			get
			{
				if (g_Cell == null)
					g_Cell = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.CellPresenter)) as Style;

				return g_Cell;
			}
		}

		#endregion //CellPresenter

		#region CellValuePresenter

		private static Style g_CellValue;

		/// <summary>
		/// The style for a cell in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style CellValuePresenter
		{
			get
			{
				if (g_CellValue == null)
					g_CellValue = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.CellValuePresenter)) as Style;

				return g_CellValue;
			}
		}

		#endregion //CellValuePresenter
        
		#region DataRecordPresenter

		private static Style g_DataRecordPresenter;

		/// <summary>
		/// The style for a record in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style DataRecordPresenter
		{
			get
			{
				if (g_DataRecordPresenter == null)
					g_DataRecordPresenter = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.DataRecordPresenter)) as Style;

				return g_DataRecordPresenter;
			}
		}

		#endregion //DataRecordPresenter

		#region DataRecordCellArea

		private static Style g_DataRecordCellArea;

		/// <summary>
		/// The style for a record expander in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style DataRecordCellArea
		{
			get
			{
				if (g_DataRecordCellArea == null)
					g_DataRecordCellArea = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.DataRecordCellArea)) as Style;

				return g_DataRecordCellArea;
			}
		}

		#endregion //DataRecordCellArea

		#region ExpandableFieldRecordPresenter

		private static Style g_ExpandableFieldRecordPresenter;

		/// <summary>
		/// The style for an expandable field record in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style ExpandableFieldRecordPresenter
		{
			get
			{
				if (g_ExpandableFieldRecordPresenter == null)
					g_ExpandableFieldRecordPresenter = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.ExpandableFieldRecordPresenter)) as Style;

				return g_ExpandableFieldRecordPresenter;
			}
		}

		#endregion //ExpandableFieldRecordPresenter

		#region ExpandedCellPresenter

		private static Style g_ExpandedCellPresenter;

		/// <summary>
		/// The style for an expanded cell in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style ExpandedCellPresenter
		{
			get
			{
				if (g_ExpandedCellPresenter == null)
					g_ExpandedCellPresenter = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.ExpandedCellPresenter)) as Style;

				return g_ExpandedCellPresenter;
			}
		}

		#endregion //ExpandedCellPresenter

		#region FieldChooser

		// SSP 6/23/09 - NAS9.2 Field Chooser
		// 

		private static Style g_FieldChooser;

		/// <summary>
		/// The style for a <see cref="Infragistics.Windows.DataPresenter.FieldChooser"/> 
		/// </summary>
		public static Style FieldChooser
		{
			get
			{
				if ( g_FieldChooser == null )
					g_FieldChooser = ResourceSet.GetSealedResource( ResourcesInternal, typeof( Infragistics.Windows.DataPresenter.FieldChooser ) ) as Style;

				return g_FieldChooser;
			}
		}

		#endregion // FieldChooser

		// JJD 10/27/11 - Added
		#region FieldMenuItem

		private static Style g_FieldMenuItem;

		/// <summary>
		/// The style for a FieldMenuItem in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style FieldMenuItem
		{
			get
			{
				if (g_FieldMenuItem == null)
					g_FieldMenuItem = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.FieldMenuItem)) as Style;

				return g_FieldMenuItem;
			}
		}

		#endregion //FieldMenuItem

        // JJD 1/12/09 - NA 2009 vol 1 - record filtering
		#region FilterCellValuePresenter

		private static Style g_FilterCellValuePresenter;

		/// <summary>
        /// The style for a <see cref="Infragistics.Windows.DataPresenter.FilterCellValuePresenter"/> 
		/// </summary>
		public static Style FilterCellValuePresenter
		{
			get
			{
				if (g_FilterCellValuePresenter == null)
					g_FilterCellValuePresenter = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.FilterCellValuePresenter)) as Style;

				return g_FilterCellValuePresenter;
			}
		}

		#endregion //FilterCellValuePresenter
        
        // AS 1/7/09 NA 2009 Vol 1 - Fixed Fields
		#region FixedFieldSplitter

		private static Style g_FixedFieldSplitter;

		/// <summary>
		/// The style for an splitter used to fix/unfix <see cref="Infragistics.Windows.DataPresenter.Field"/> instances in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style FixedFieldSplitter
		{
			get
			{
				if (g_FixedFieldSplitter == null)
					g_FixedFieldSplitter = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.FixedFieldSplitter)) as Style;

				return g_FixedFieldSplitter;
			}
		}

		#endregion //FixedFieldSplitter
        
		#region LabelPresenter

		private static Style g_FieldLabel;

		/// <summary>
		/// The style for a label in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style LabelPresenter
		{
			get
			{
				if (g_FieldLabel == null)
					g_FieldLabel = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.LabelPresenter)) as Style;

				return g_FieldLabel;
			}
		}

		#endregion //LabelPresenter

		#region GroupByArea

		private static Style g_GroupByArea;

		/// <summary>
		/// The style for the GroupByArea of a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style GroupByArea
		{
			get
			{
				if (g_GroupByArea == null)
					g_GroupByArea = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.GroupByArea)) as Style;

				return g_GroupByArea;
			}
		}

		#endregion //GroupByArea

		#region GroupByAreaFieldLabel

		private static Style g_GroupByAreaFieldLabel;

		/// <summary>
		/// The style for a cell in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style GroupByAreaFieldLabel
		{
			get
			{
				if (g_GroupByAreaFieldLabel == null)
					g_GroupByAreaFieldLabel = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.GroupByAreaFieldLabel)) as Style;

				return g_GroupByAreaFieldLabel;
			}
		}

		#endregion //GroupByAreaFieldLabel

		#region GroupByAreaFieldListBox

		private static Style g_GroupByAreaFieldListBox;

		/// <summary>
		/// The style for listboxes in the GroupByArea of a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style GroupByAreaFieldListBox
		{
			get
			{
				if (g_GroupByAreaFieldListBox == null)
					g_GroupByAreaFieldListBox = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.GroupByAreaFieldListBox)) as Style;

				return g_GroupByAreaFieldListBox;
			}
		}

		#endregion //GroupByAreaFieldListBox

        // JJD 4/15/09 - NA 2009 vol2 - Cross band grouping
		#region GroupByAreaMulti

		private static Style g_GroupByAreaMulti;

		/// <summary>
		/// The style for the <see cref="GroupByAreaMulti"/> used in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style GroupByAreaMulti
		{
			get
			{
				if (g_GroupByAreaMulti == null)
					g_GroupByAreaMulti = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.GroupByAreaMulti)) as Style;

				return g_GroupByAreaMulti;
			}
		}

		#endregion //GroupByAreaMulti

		#region GroupByRecordPresenter

		private static Style g_GroupByRecordPresenter;

		/// <summary>
		/// The style for groupby records in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style GroupByRecordPresenter
		{
			get
			{
				if (g_GroupByRecordPresenter == null)
					g_GroupByRecordPresenter = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.GroupByRecordPresenter)) as Style;

				return g_GroupByRecordPresenter;
			}
		}

		#endregion //GroupByRecordPresenter

		#region GroupBySummariesPresenter

		
		

		private static Style g_GroupBySummariesPresenter;

		/// <summary>
		/// The style for the element that's displayed inside each group-by record to display that group's summaries.
		/// </summary>
		public static Style GroupBySummariesPresenter
		{
			get
			{
				if ( g_GroupBySummariesPresenter == null )
					g_GroupBySummariesPresenter = ResourceSet.GetSealedResource( ResourcesInternal, typeof( Infragistics.Windows.DataPresenter.GroupBySummariesPresenter ) ) as Style;

				return g_GroupBySummariesPresenter;
			}
		}

		#endregion // GroupBySummariesPresenter

		#region HeaderPresenter

		private static Style g_HeaderPresenter;

		/// <summary>
		/// The style for the header area in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style HeaderPresenter
		{
			get
			{
				if (g_HeaderPresenter == null)
					g_HeaderPresenter = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.HeaderPresenter)) as Style;

				return g_HeaderPresenter;
			}
		}

		#endregion //HeaderPresenter

		#region HeaderLabelArea

		private static Style g_HeaderLabelArea;

		/// <summary>
		/// The style for the area that contains the labels in the header of a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style HeaderLabelArea
		{
			get
			{
				if (g_HeaderLabelArea == null)
					g_HeaderLabelArea = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.HeaderLabelArea)) as Style;

				return g_HeaderLabelArea;
			}
		}

		#endregion //HeaderLabelArea

		#region HeaderPrefixArea

		private static Style g_HeaderPrefixArea;

		/// <summary>
		/// The style for the header area in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style HeaderPrefixArea
		{
			get
			{
				if (g_HeaderPrefixArea == null)
					g_HeaderPrefixArea = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.HeaderPrefixArea)) as Style;

				return g_HeaderPrefixArea;
			}
		}

		#endregion //HeaderPrefixArea

		#region RecordExportStatusControl

		private static Style g_RecordExportStatusControl;

		/// <summary>
		/// The style for a RecordExportStatusControl in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style RecordExportStatusControl
		{
			get
			{
				if (g_RecordExportStatusControl == null)
					g_RecordExportStatusControl = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.RecordExportStatusControl)) as Style;

				return g_RecordExportStatusControl;
			}
		}

		#endregion //RecordExportStatusControl

		// JJD 10/27/11 - Added
		#region RecordFilterTreeControl

		private static Style g_RecordFilterTreeControl;

		/// <summary>
		/// The style for a RecordFilterTreeControl in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style RecordFilterTreeControl
		{
			get
			{
				if (g_RecordFilterTreeControl == null)
					g_RecordFilterTreeControl = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.RecordFilterTreeControl)) as Style;

				return g_RecordFilterTreeControl;
			}
		}

		#endregion //RecordFilterTreeControl

		#region RecordListControl

		private static Style g_RecordListControl;

		/// <summary>
		/// The style for a RecordListControl in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style RecordListControl
		{
			get
			{
				if (g_RecordListControl == null)
					g_RecordListControl = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.RecordListControl)) as Style;

				return g_RecordListControl;
			}
		}

		#endregion //RecordListControl

		#region RecordScrollTip

		private static Style g_RecordScrollTip;

		/// <summary>
		/// The style for a <see cref="RecordScrollTip"/> of a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style RecordScrollTip
		{
			get
			{
				if (g_RecordScrollTip == null)
					g_RecordScrollTip = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.RecordScrollTip)) as Style;

				return g_RecordScrollTip;
			}
		}

		#endregion //RecordScrollTip

		#region RecordSelector

		private static Style g_RecordSelector;

		/// <summary>
		/// The style for a <see cref="Infragistics.Windows.DataPresenter.RecordSelector"/> in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style RecordSelector
		{
			get
			{
				if (g_RecordSelector == null)
					g_RecordSelector = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.RecordSelector)) as Style;

				return g_RecordSelector;
			}
		}

		#endregion //RecordSelector

		#region SummaryButton

		
		

		private static Style g_SummaryButton;

		/// <summary>
		/// The style for the summary button that's displayed inside each field label, which when clicked upon displays
		/// the summary selection user interface.
		/// </summary>
		public static Style SummaryButton
		{
			get
			{
				if ( g_SummaryButton == null )
					g_SummaryButton = ResourceSet.GetSealedResource( ResourcesInternal, typeof( Infragistics.Windows.DataPresenter.SummaryButton ) ) as Style;

				return g_SummaryButton;
			}
		}

		#endregion // SummaryButton

		#region SummaryCalculatorSelectionControl

		
		

		private static Style g_SummaryCalculatorSelectionControl;

		/// <summary>
		/// The style for the control that's displayed to let the user select summaries for a field in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>.
		/// </summary>
		public static Style SummaryCalculatorSelectionControl
		{
			get
			{
				if ( g_SummaryCalculatorSelectionControl == null )
					g_SummaryCalculatorSelectionControl = ResourceSet.GetSealedResource( ResourcesInternal, typeof( Infragistics.Windows.DataPresenter.SummaryCalculatorSelectionControl ) ) as Style;

				return g_SummaryCalculatorSelectionControl;
			}
		}

		#endregion // SummaryCalculatorSelectionControl

		#region SummaryCellPresenter

		
		

		private static Style g_SummaryCellPresenter;

		/// <summary>
		/// The style for summary cells in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style SummaryCellPresenter
		{
			get
			{
				if ( g_SummaryCellPresenter == null )
					g_SummaryCellPresenter = ResourceSet.GetSealedResource( ResourcesInternal, typeof( Infragistics.Windows.DataPresenter.SummaryCellPresenter ) ) as Style;

				return g_SummaryCellPresenter;
			}
		}

		#endregion // SummaryCellPresenter

		#region SummaryRecordCellArea

		
		

		private static Style g_SummaryRecordCellArea;

		/// <summary>
		/// The style for element that contains summary cells that are aligned with the field labels in <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>.
		/// </summary>
		public static Style SummaryRecordCellArea
		{
			get
			{
				if ( g_SummaryRecordCellArea == null )
					g_SummaryRecordCellArea = ResourceSet.GetSealedResource( ResourcesInternal, typeof( Infragistics.Windows.DataPresenter.SummaryRecordCellArea ) ) as Style;

				return g_SummaryRecordCellArea;
			}
		}

		#endregion // SummaryRecordCellArea

		#region SummaryRecordContentArea

		
		

		private static Style g_SummaryRecordContentArea;

		/// <summary>
		/// The style for element that contains summary record's contents in <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>.
		/// </summary>
		public static Style SummaryRecordContentArea
		{
			get
			{
				if ( g_SummaryRecordContentArea == null )
					g_SummaryRecordContentArea = ResourceSet.GetSealedResource( ResourcesInternal, typeof( Infragistics.Windows.DataPresenter.SummaryRecordContentArea ) ) as Style;

				return g_SummaryRecordContentArea;
			}
		}

		#endregion // SummaryRecordContentArea

		#region SummaryRecordHeaderPresenter

		
		

		private static Style g_SummaryRecordHeaderPresenter;

		/// <summary>
		/// The style for element that contains summary record's contents in <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>.
		/// </summary>
		public static Style SummaryRecordHeaderPresenter
		{
			get
			{
				if ( g_SummaryRecordHeaderPresenter == null )
					g_SummaryRecordHeaderPresenter = ResourceSet.GetSealedResource( ResourcesInternal, typeof( Infragistics.Windows.DataPresenter.SummaryRecordHeaderPresenter ) ) as Style;

				return g_SummaryRecordHeaderPresenter;
			}
		}

		#endregion // SummaryRecordHeaderPresenter

		#region SummaryRecordPrefixArea

		
		

		private static Style g_SummaryRecordPrefixArea;

		/// <summary>
		/// The style for the element placed in the area of summary record where record selectors for data records go in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>.
		/// </summary>
		public static Style SummaryRecordPrefixArea
		{
			get
			{
				if ( g_SummaryRecordPrefixArea == null )
					g_SummaryRecordPrefixArea = ResourceSet.GetSealedResource( ResourcesInternal, typeof( Infragistics.Windows.DataPresenter.SummaryRecordPrefixArea ) ) as Style;

				return g_SummaryRecordPrefixArea;
			}
		}

		#endregion // SummaryRecordPrefixArea

		#region SummaryRecordPresenter

		
		

		private static Style g_SummaryRecordPresenter;

		/// <summary>
		/// The style for summary records in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style SummaryRecordPresenter
		{
			get
			{
				if ( g_SummaryRecordPresenter == null )
					g_SummaryRecordPresenter = ResourceSet.GetSealedResource( ResourcesInternal, typeof( Infragistics.Windows.DataPresenter.SummaryRecordPresenter ) ) as Style;

				return g_SummaryRecordPresenter;
			}
		}

		#endregion // SummaryRecordPresenter

		#region SummaryResultPresenter

		
		

		private static Style g_SummaryResultPresenter;

		/// <summary>
		/// The style for summary results in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> 
		/// </summary>
		public static Style SummaryResultPresenter
		{
			get
			{
				if ( g_SummaryResultPresenter == null )
					g_SummaryResultPresenter = ResourceSet.GetSealedResource( ResourcesInternal, typeof( Infragistics.Windows.DataPresenter.SummaryResultPresenter ) ) as Style;

				return g_SummaryResultPresenter;
			}
		}

		#endregion // SummaryResultPresenter

		#region SummaryResultsPresenter

		
		

		private static Style g_SummaryResultsPresenter;

		/// <summary>
		/// The style for element that contains one or more summary results in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>.
		/// </summary>
		public static Style SummaryResultsPresenter
		{
			get
			{
				if ( g_SummaryResultsPresenter == null )
					g_SummaryResultsPresenter = ResourceSet.GetSealedResource( ResourcesInternal, typeof( Infragistics.Windows.DataPresenter.SummaryResultsPresenter ) ) as Style;

				return g_SummaryResultsPresenter;
			}
		}

		#endregion // SummaryResultsPresenter

		// JM NA 10.1 CardView
		#region XamDataCards

		private static Style g_XamDataCards;

		/// <summary>
		/// The style used for the <see cref="Infragistics.Windows.DataPresenter.XamDataCards"/>  control. 
		/// </summary>
		public static Style XamDataCards
		{
			get
			{
				if (g_XamDataCards == null)
					g_XamDataCards = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.XamDataCards)) as Style;

				return g_XamDataCards;
			}
		}

		#endregion //XamDataCards


		#region XamDataCarousel

		private static Style g_XamDataCarousel;

		/// <summary>
		/// The style used for the <see cref="Infragistics.Windows.DataPresenter.XamDataCarousel"/>  control. 
		/// </summary>
		public static Style XamDataCarousel
		{
			get
			{
				if (g_XamDataCarousel == null)
					g_XamDataCarousel = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.XamDataCarousel)) as Style;

				return g_XamDataCarousel;
			}
		}

				#endregion //XamDataCarousel

		#region XamDataPresenter

		private static Style g_XamDataPresenter;

		/// <summary>
		/// The style used for the <see cref="Infragistics.Windows.DataPresenter.XamDataPresenter"/>  control. 
		/// </summary>
		public static Style XamDataPresenter
		{
			get
			{
				if (g_XamDataPresenter == null)
					g_XamDataPresenter = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.XamDataPresenter)) as Style;

				return g_XamDataPresenter;
			}
		}

		#endregion //XamDataPresenter


		#region XamDataGrid

		private static Style g_XamDataGrid;

		/// <summary>
		/// The style used for the <see cref="Infragistics.Windows.DataPresenter.XamDataGrid"/>  control. 
		/// </summary>
		public static Style XamDataGrid
		{
			get
			{
				if (g_XamDataGrid == null)
					g_XamDataGrid = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.DataPresenter.XamDataGrid)) as Style;

				return g_XamDataGrid;
			}
		}

		#endregion //XamDataGrid

		#endregion //Public Properties

		#endregion //Static Properties
	}

	#endregion //DataPresenterResourceSet<T> base class

	#region DataPresenterGeneric

	/// <summary>
	/// Class used to supply style resources for the Generic look for a DataPresenters
	/// </summary>
	public class DataPresenterGeneric : DataPresenterResourceSet<DataPresenterGeneric.Locator>
	{

		#region Instance static property

		private static DataPresenterGeneric g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterGeneric Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterGeneric();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameGeneric; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\DataPresenter\DataPresenterGeneric_Express.xaml;ResourceSets\DataPresenter\DataPresenterGeneric.xaml"; } }

		}
	}

	#endregion //DataPresenterGeneric

	#region DataPresenterOnyx

	/// <summary>
	/// Class used to supply style resources for the Onyx look for a DataPresenters
	/// </summary>
	public class DataPresenterOnyx : DataPresenterResourceSet<DataPresenterOnyx.Locator>
	{

		#region Instance static property

		private static DataPresenterOnyx g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterOnyx Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterOnyx();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameOnyx; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterOnyx.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\DataPresenter\DataPresenterOnyx_Express.xaml;ResourceSets\DataPresenter\DataPresenterOnyx.xaml"; } }

		}
	}

	#endregion //DataPresenterOnyx

	#region DataPresenterAero

	/// <summary>
	/// Class used to supply style resources for the Aero look for a DataPresenters
	/// </summary>
	public class DataPresenterAero : DataPresenterResourceSet<DataPresenterAero.Locator>
	{

		#region Instance static property

		private static DataPresenterAero g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterAero Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterAero();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameAero; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterAero.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\DataPresenter\DataPresenterAero_Express.xaml;ResourceSets\DataPresenter\DataPresenterAero.xaml"; } }

		}
	}

	#endregion //DataPresenterAero

	#region DataPresenterRoyale

	/// <summary>
	/// Class used to supply style resources for the Royale look for a DataPresenters
	/// </summary>
	public class DataPresenterRoyale : DataPresenterResourceSet<DataPresenterRoyale.Locator>
	{

		#region Instance static property

		private static DataPresenterRoyale g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterRoyale Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterRoyale();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameRoyale; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterRoyale.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\DataPresenter\DataPresenterRoyale_Express.xaml;ResourceSets\DataPresenter\DataPresenterRoyale.xaml"; } }

		}
	}

	#endregion //DataPresenterRoyale

	#region DataPresenterRoyaleStrong

	/// <summary>
	/// Class used to supply style resources for the RoyaleStrong look for a DataPresenters
	/// </summary>
	public class DataPresenterRoyaleStrong : DataPresenterResourceSet<DataPresenterRoyaleStrong.Locator>
	{

		#region Instance static property

		private static DataPresenterRoyaleStrong g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterRoyaleStrong Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterRoyaleStrong();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameRoyaleStrong; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterRoyaleStrong.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\DataPresenter\DataPresenterRoyaleStrong_Express.xaml;ResourceSets\DataPresenter\DataPresenterRoyaleStrong.xaml"; } }

		}
	}

	#endregion //DataPresenterRoyaleStrong

	#region DataPresenterLunaNormal

	/// <summary>
	/// Class used to supply style resources for the LunaNormal look for a DataPresenters
	/// </summary>
	public class DataPresenterLunaNormal : DataPresenterResourceSet<DataPresenterLunaNormal.Locator>
	{

		#region Instance static property

		private static DataPresenterLunaNormal g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterLunaNormal Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterLunaNormal();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameLunaNormal; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterLunaNormal.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\DataPresenter\DataPresenterLunaNormal_Express.xaml;ResourceSets\DataPresenter\DataPresenterLunaNormal.xaml"; } }

		}
	}

	#endregion //DataPresenterLunaNormal

	#region DataPresenterLunaOlive

	/// <summary>
	/// Class used to supply style resources for the LunaOlive look for a DataPresenters
	/// </summary>
	public class DataPresenterLunaOlive : DataPresenterResourceSet<DataPresenterLunaOlive.Locator>
	{

		#region Instance static property

		private static DataPresenterLunaOlive g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterLunaOlive Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterLunaOlive();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameLunaOlive; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterLunaOlive.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\DataPresenter\DataPresenterLunaOlive_Express.xaml;ResourceSets\DataPresenter\DataPresenterLunaOlive.xaml"; } }

		}
	}

	#endregion //DataPresenterLunaOlive

	#region DataPresenterLunaSilver

	/// <summary>
	/// Class used to supply style resources for the LunaSilver look for a DataPresenters
	/// </summary>
	public class DataPresenterLunaSilver : DataPresenterResourceSet<DataPresenterLunaSilver.Locator>
	{

		#region Instance static property

		private static DataPresenterLunaSilver g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterLunaSilver Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterLunaSilver();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameLunaSilver; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterLunaSilver.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\DataPresenter\DataPresenterLunaSilver_Express.xaml;ResourceSets\DataPresenter\DataPresenterLunaSilver.xaml"; } }

		}
	}

	#endregion //DataPresenterLunaSilver

	#region DataPresenterOffice2k7Blue

	/// <summary>
	/// Class used to supply style resources for the Lite look for a DataPresenters
	/// </summary>
	public class DataPresenterOffice2k7Blue : DataPresenterResourceSet<DataPresenterOffice2k7Blue.Locator>
	{

		#region Instance static property

		private static DataPresenterOffice2k7Blue g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterOffice2k7Blue Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterOffice2k7Blue();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameOffice2k7Blue; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterOffice2k7Blue.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\DataPresenter\DataPresenterOffice2k7Blue_Express.xaml;ResourceSets\DataPresenter\DataPresenterOffice2k7Blue.xaml"; } }

		}
	}

	#endregion //DataPresenterOffice2k7Blue

	#region DataPresenterOffice2k7Black

	/// <summary>
	/// Class used to supply style resources for the Lite look for a DataPresenters
	/// </summary>
	public class DataPresenterOffice2k7Black : DataPresenterResourceSet<DataPresenterOffice2k7Black.Locator>
	{

		#region Instance static property

		private static DataPresenterOffice2k7Black g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterOffice2k7Black Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterOffice2k7Black();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameOffice2k7Black; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterOffice2k7Black.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\DataPresenter\DataPresenterOffice2k7Black_Express.xaml;ResourceSets\DataPresenter\DataPresenterOffice2k7Black.xaml"; } }

		}
	}

	#endregion //DataPresenterOffice2k7Black

	#region DataPresenterOffice2k7Silver

	/// <summary>
	/// Class used to supply style resources for the Lite look for a DataPresenters
	/// </summary>
	public class DataPresenterOffice2k7Silver : DataPresenterResourceSet<DataPresenterOffice2k7Silver.Locator>
	{

		#region Instance static property

		private static DataPresenterOffice2k7Silver g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterOffice2k7Silver Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterOffice2k7Silver();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameOffice2k7Silver; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterOffice2k7Silver.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\DataPresenter\DataPresenterOffice2k7Silver_Express.xaml;ResourceSets\DataPresenter\DataPresenterOffice2k7Silver.xaml"; } }

		}
	}

	#endregion //DataPresenterOffice2k7Silver

	// JJD 8/30/10 - Added Office 2010 Blue theme
	#region DataPresenterOffice2010Blue

	/// <summary>
	/// Class used to supply style resources for the Lite look for a DataPresenters
	/// </summary>
	public class DataPresenterOffice2010Blue : DataPresenterResourceSet<DataPresenterOffice2010Blue.Locator>
	{

		#region Instance static property

		private static DataPresenterOffice2010Blue g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterOffice2010Blue Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterOffice2010Blue();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameOffice2010Blue; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterOffice2010Blue.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\DataPresenter\DataPresenterOffice2010Blue_Express.xaml;ResourceSets\DataPresenter\DataPresenterOffice2010Blue.xaml"; } }

		}
	}

	#endregion //DataPresenterOffice2010Blue

	#region DataPresenterWashBaseDark

	/// <summary>
	/// Class used to supply style resources for the Lite look for a DataPresenters
	/// </summary>
	public class DataPresenterWashBaseDark : DataPresenterResourceSet<DataPresenterWashBaseDark.Locator>
	{

		#region Instance static property

		private static DataPresenterWashBaseDark g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterWashBaseDark Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterWashBaseDark();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameWashBaseDark; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterWashBaseDark.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DataPresenter\DataPresenterWashBaseDark.xaml"; } }
		}
	}

	#endregion //DataPresenterWashBaseDark

	#region DataPresenterWashBaseLight

	/// <summary>
	/// Class used to supply style resources for the Lite look for a DataPresenters
	/// </summary>
	public class DataPresenterWashBaseLight : DataPresenterResourceSet<DataPresenterWashBaseLight.Locator>
	{

		#region Instance static property

		private static DataPresenterWashBaseLight g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterWashBaseLight Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterWashBaseLight();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameWashBaseLight; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterWashBaseLight.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DataPresenter\DataPresenterWashBaseLight.xaml"; } }
		}
	}

	#endregion //DataPresenterWashBaseLight


    #region DataPresenterPrintBasic

    /// <summary>
    /// Class used to supply style resources for the Print look for a DataPresenters
    /// </summary>
    public class DataPresenterPrintBasic : DataPresenterResourceSet<DataPresenterPrintBasic.Locator>
    {

        #region Instance static property

        private static DataPresenterPrintBasic g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static DataPresenterPrintBasic Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new DataPresenterPrintBasic();

                return g_Instance;
            }
        }

        #endregion //Instance static property

        /// <summary>
        /// Identifies the location of a specific theme grouping
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public class Locator : ResourceSetLocator
        {
            /// <summary>The assembly containg the resource set</summary>
            public override Assembly Assembly { get { return this.GetType().Assembly; } }
            /// <summary>The name of the theme</summary>
            public override string Theme { get { return ThemeManager.ThemeNamePrintBasic; } }
            /// <summary>The name of the grouping</summary>
            public override string Grouping { get { return DataPresenterPrintBasic.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\DataPresenter\DataPresenterPrintBasic.xaml"; } }
        }
    }
    #endregion //DataPresenterPrintBasic


	// JJD 10/29/10 - Added IGTheme
	#region DataPresenterIGTheme

	/// <summary>
	/// Class used to supply style resources for the Lite look for a DataPresenters
	/// </summary>
	public class DataPresenterIGTheme : DataPresenterResourceSet<DataPresenterIGTheme.Locator>
	{

		#region Instance static property

		private static DataPresenterIGTheme g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterIGTheme Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterIGTheme();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameIGTheme; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterIGTheme.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\DataPresenter\DataPresenterIGTheme_Express.xaml;ResourceSets\DataPresenter\DataPresenterIGTheme.xaml"; } }

		}
	}

	#endregion //DataPresenterIGTheme

	// JJD 02/16/12 - Added Metro Theme
	#region DataPresenterMetro

	/// <summary>
	/// Class used to supply style resources for the Metro look for a DataPresenters
	/// </summary>
	public class DataPresenterMetro : DataPresenterResourceSet<DataPresenterMetro.Locator>
	{

		#region Instance static property

		private static DataPresenterMetro g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterMetro Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterMetro();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameMetro; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterMetro.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\DataPresenter\DataPresenterMetro.xaml"; } }

		}
	}

	#endregion //DataPresenterMetro

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