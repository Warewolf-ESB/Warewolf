//#define DEBUG_SIZECHANGES


    


using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Controls.Layouts.Primitives;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using Infragistics.Windows.DataPresenter.Internal;

// AS 12/9/08 NA 2009 Vol 1 - Fixed Fields
namespace Infragistics.Windows.DataPresenter
{
    // Enums:
    //
    #region LayoutItemSize
    internal enum LayoutItemSize : int
    {
        // note: the order is important - don't change it
        MinWidth,
        PreferredWidth,
        MaxWidth,
        MinHeight,
        PreferredHeight,
        MaxHeight,
    } 
    #endregion //LayoutItemSize

    #region LayoutManagerType
    internal enum LayoutManagerType
    {
        Header,
        Record,
    } 
    #endregion //LayoutManagerType

    // Interfaces:
    //
    #region ICellPanelLayoutItem
	// AS 10/13/09 NA 2010.1 - CardView
	// We don't need this anymore.
	//
	//internal interface ICellPanelLayoutItem
	//{
	//    VirtualizingDataRecordCellPanel CurrentPanel { get; set; }
	//}
    #endregion //ICellPanelLayoutItem

	// AS 6/1/09
	#region IWrapperLayoutItem
	/// <summary>
	/// Interface implemented by a layout item that wraps another layout item.
	/// </summary>
	internal interface IWrapperLayoutItem
	{
		ILayoutItem InnerLayoutItem { get; }

		double? ExplicitWidth { get; set; }
		double? ExplicitHeight { get; set; }
	}
	#endregion //IWrapperLayoutItem

    // ILayoutItem:
    //
    #region LayoutItem
    internal abstract class LayoutItem : ILayoutItem
		// AS 7/29/09 NA 2009.2 Field Sizing
		, IAutoSizeLayoutItem
    {
        #region Constructor
        protected LayoutItem()
        {
        }
        #endregion //Constructor

		#region Properties

		// AS 4/16/09 TFS16467
		#region Preferred(Width|Height)
		public virtual double PreferredWidth
		{
			get { return this.GetSize(LayoutItemSize.PreferredWidth); }
			// AS 7/29/09 NA 2009.2 Field Sizing
			//set { Debug.Fail("Not implemented/supported for this layout item"); }
		}

		public virtual double PreferredHeight
		{
			get { return this.GetSize(LayoutItemSize.PreferredHeight); }
			// AS 7/29/09 NA 2009.2 Field Sizing
			//set { Debug.Fail("Not implemented/supported for this layout item"); }
		}
		#endregion //Preferred(Width|Height)

		// AS 7/29/09 NA 2009.2 Field Sizing
		#region SizeType
		public virtual ItemSizeType SizeTypeHeight
		{
			get { return ItemSizeType.Explicit; }
		}

		public virtual ItemSizeType SizeTypeWidth
		{
			get { return ItemSizeType.Explicit; }
		}
		#endregion //SizeType

		#endregion //Properties

		#region Methods

		/// <summary>
        /// Returns the specified size for the item
        /// </summary>
        /// <param name="size">Identifies the type of size being requested</param>
        /// <returns>Returns the size for the specified extent</returns>
        public abstract double GetSize(LayoutItemSize size);

        /// <summary>
        /// Returns the visibility of the layout item
        /// </summary>
        /// <returns>Returns a Visibility indicating if the item is visible in the layout.</returns>
        protected abstract Visibility GetVisibility();

        internal static bool IsWidth(LayoutItemSize size)
        {
            return size < LayoutItemSize.MinHeight;
        }

		// AS 7/29/09 NA 2009.2 Field Sizing
		public virtual void SetPreferredWidth(double value, ItemSizeType type)
		{
			Debug.Fail("Not implemented/supported for this layout item");
		}

		public virtual void SetPreferredHeight(double value, ItemSizeType type)
		{
			Debug.Fail("Not implemented/supported for this layout item");
		}
		#endregion //Methods

        #region ILayoutItem Members

        Size ILayoutItem.MaximumSize
        {
            get { return new Size(GetSize(LayoutItemSize.MaxWidth), GetSize(LayoutItemSize.MaxHeight)); }
        }

        Size ILayoutItem.MinimumSize
        {
            get { return new Size(GetSize(LayoutItemSize.MinWidth), GetSize(LayoutItemSize.MinHeight)); }
        }

        Size ILayoutItem.PreferredSize
        {
            get { return new Size(GetSize(LayoutItemSize.PreferredWidth), GetSize(LayoutItemSize.PreferredHeight)); }
        }

        Visibility ILayoutItem.Visibility
        {
            get { return this.GetVisibility(); }
        }

        #endregion //ILayoutItem

		// AS 7/29/09 NA 2009.2 Field Sizing
		#region IAutoSizeLayoutItem Members

		bool IAutoSizeLayoutItem.IsHeightAutoSized
		{
			get { return this.SizeTypeHeight != ItemSizeType.Explicit; }
		}

		bool IAutoSizeLayoutItem.IsWidthAutoSized
		{
			get { return this.SizeTypeWidth != ItemSizeType.Explicit; }
		}

		#endregion //IAutoSizeLayoutItem Members
	} 
    #endregion //LayoutItem

    #region FieldLayoutItemBase
    /// <summary>
    /// Base class for a layout item for a field label/cell.
    /// </summary>
    internal abstract class FieldLayoutItemBase : LayoutItem
    {
        #region Member Variables

        public readonly Field Field;
        public readonly bool IsLabel;

        #endregion //Member Variables

        #region Constructor
        protected FieldLayoutItemBase(Field field, bool isLabel)
            : base()
        {
            GridUtilities.ValidateNotNull(field);
            Field = field;
            IsLabel = isLabel;
        }
        #endregion //Constructor

		#region Base class overrides
		public override string ToString()
		{
			return string.Format("Field={0}, IsLabel={1}, Fixed={2}, SizeTypeW={3}, SizeTypeH={4}", this.Field, this.IsLabel, this.FixedLocation, this.SizeTypeWidth, this.SizeTypeHeight);
		}
		#endregion //Base class overrides

		#region Properties

        public virtual FixedFieldLocation FixedLocation
        {
            get { return this.Field.FixedLocation; }
        }

        // AS 4/16/09 TFS16467
        //public double PreferredWidth
		// AS 7/29/09 NA 2009.2 Field Sizing
		// The base get does what we need and the set was changed to a method.
		//
		//public override double PreferredWidth
		//{
		//    get { return this.GetSize(LayoutItemSize.PreferredWidth); }
		//    set { this.SetPreferredWidth(value); }
		//}

        // AS 4/16/09 TFS16467
        //public double PreferredHeight
		// AS 7/29/09 NA 2009.2 Field Sizing
		// The base get does what we need and the set was changed to a method.
		//
		//public override double PreferredHeight
		//{
		//    get { return this.GetSize(LayoutItemSize.PreferredHeight); }
		//    set { this.SetPreferredHeight(value); }
		//}

		// AS 1/25/10 SizableSynchronized changes
		// Not all records follow the datarecordsizingmode.
		//
		public abstract DataRecordSizingMode RecordSizingMode
		{
			get;
		}

		#endregion //Properties

		#region Methods

		#region SetPreferredWidth
		// AS 7/29/09 NA 2009.2 Field Sizing
		//protected abstract void SetPreferredWidth(double value); 
		public abstract override void SetPreferredWidth(double value, ItemSizeType type);
        #endregion //SetPreferredWidth

        #region SetPreferredHeight
		// AS 7/29/09 NA 2009.2 Field Sizing
		//protected abstract void SetPreferredHeight(double value); 
		public abstract override void SetPreferredHeight(double value, ItemSizeType type);
		#endregion //SetPreferredHeight

        #region OnWidthChanged
        protected void OnWidthChanged()
        {
            this.Field.BumpLayoutItemVersion();
        } 
        #endregion //OnWidthChanged

        #region OnHeightChanged
        protected void OnHeightChanged()
        {
            this.Field.BumpLayoutItemVersion();
        } 
        #endregion //OnHeightChanged

        #region ShouldSynchronize
        internal static bool ShouldSynchronize(LayoutItemSize size, Field field)
        {
            bool isWidth = LayoutItem.IsWidth(size);

            return (isWidth && field.SyncCellLabelWidth) || (isWidth == false && field.SyncCellLabelHeight);
        }
        #endregion //ShouldSynchronize

        #region CombineValues
        internal static double CombineValues(bool sameColumn, FieldLayoutItemBase labelLayoutItem, FieldLayoutItemBase cellLayoutItem, LayoutItemSize size)
        {
            double labelValue = labelLayoutItem.GetSize(size);
            double cellValue = cellLayoutItem.GetSize(size);
            bool isWidth = false;

            if (LayoutItem.IsWidth(size))
                isWidth = true;
            else
                size -= 3;

            switch (size)
            {
                default:
                case LayoutItemSize.MinWidth: // min width/height
                    if (sameColumn != isWidth) // width for label left/right of cell or height for label above/below cell
                    {
                        int offset = isWidth ? 0 : 3;
                        double maxLabel = labelLayoutItem.GetSize((LayoutItemSize)(2 + offset));
                        double prefLabel = labelLayoutItem.GetSize((LayoutItemSize)(1 + offset));

                        // we didn't use to resize such that the label was shrunk
                        labelValue = Math.Max(labelValue, Math.Min(maxLabel, prefLabel));

                        return labelValue + cellValue;
                    }
                    else // minwidth for label above/below cell or minheight for label left/right of cell
                    {
                        return Math.Max(labelValue, cellValue);
                    }
                case LayoutItemSize.PreferredWidth: // pref width/height
                    if (sameColumn != isWidth) // width for label left/right of cell or height for label above/below cell
                    {
                        return labelValue + cellValue;
                    }
                    else // width for label above/below cell or height for label left/right of cell
                    {
                        // prefer the max of the two but constrain that with the min of the max's 
                        LayoutItemSize maxSize = isWidth ? LayoutItemSize.MaxWidth : LayoutItemSize.MaxHeight;
                        double preferred = Math.Min(cellLayoutItem.GetSize(maxSize),
                            Math.Min(labelLayoutItem.GetSize(maxSize), Math.Max(labelValue, cellValue)));
                        return preferred;
                    }
                case LayoutItemSize.MaxWidth: // max width/height
                    if (sameColumn != isWidth) // maxwidth for label left/right of cell or minheight for label above/below cell
                    {
                        // if either is unbounded then there is no bounds
                        if (double.IsPositiveInfinity(labelValue) || double.IsPositiveInfinity(cellValue))
                            return double.PositiveInfinity;

                        return labelValue + cellValue;
                    }
                    else // maxwidth for label above/below cell or maxheight for label left/right of cell
                    {

                        return Math.Min(labelValue, cellValue);
                    }
            }
        }
        #endregion //CombineValues

        #region GetSynchronizedSize
        internal static double GetSynchronizedSize(LayoutItemSize size, Field field)
        {
            // if its height then normal the enum so we can just consider min/max/preferred
            LayoutItemSize normalizedSize = size;

            bool isWidth = false;

            if (LayoutItem.IsWidth(size))
                isWidth = true;
            else
                normalizedSize -= 3;

            LayoutItemSize minSize = isWidth ? LayoutItemSize.MinWidth : LayoutItemSize.MinHeight;
            double minExtent = Math.Max(GetDefaultLabelSize(minSize, field), GetDefaultCellSize(minSize, field));

            LayoutItemSize maxSize = isWidth ? LayoutItemSize.MaxWidth : LayoutItemSize.MaxHeight;
            double maxExtent = Math.Min(GetDefaultLabelSize(maxSize, field), GetDefaultCellSize(maxSize, field));

            // AS 1/26/09
            // To be consistent with the previous versions, we need to 
            // always force a minimum extent of 6 pixels.
            //
            minExtent = Math.Min(maxExtent, Math.Max(FieldLayoutItem.MinimumExtent, minExtent));

            if (normalizedSize == LayoutItemSize.MinWidth)
                return minExtent;

            if (normalizedSize == LayoutItemSize.MaxWidth)
				return Math.Max(maxExtent, minExtent);

            double cellPref = GetDefaultCellSize(size, field);
            double labelPref = GetDefaultLabelSize(size, field);
            double prefExtent;

            if (double.IsNaN(cellPref))
                prefExtent = labelPref;
            else if (double.IsNaN(labelPref))
                prefExtent = cellPref;
            else
                prefExtent = Math.Max(labelPref, cellPref);

			return Math.Max(Math.Min(prefExtent, maxExtent), minExtent);
        }
        #endregion //GetSynchronizedSize

        #region GetDefaultCellSize
        internal static double GetDefaultCellSize(LayoutItemSize size, Field field)
        {
            switch (size)
            {
                case LayoutItemSize.MaxHeight:
                    return field.CellMaxHeightResolved;
                case LayoutItemSize.MinHeight:
                    return field.CellMinHeightResolved;
                case LayoutItemSize.PreferredHeight:
                    return field.GetCellHeightResolvedHelper(true);

                case LayoutItemSize.MaxWidth:
                    return field.CellMaxWidthResolved;
                case LayoutItemSize.MinWidth:
                    return field.CellMinWidthResolved;
                case LayoutItemSize.PreferredWidth:
                    return field.GetCellWidthResolvedHelper(true);

                default:
                    Debug.Fail("Unrecognized size");
                    return double.NaN;
            }
        } 
        #endregion //GetDefaultCellSize

        #region GetDefaultCellPresenterSize
        internal static double GetDefaultCellPresenterSize(LayoutItemSize size, Field field)
        {
            FieldLayout fl = field.Owner;

            if (null != fl)
            {
                FieldLayoutItem labelLayoutItem = field.LabelLayoutItem;
                FieldLayoutItem cellLayoutItem = field.CellLayoutItem;

                switch (field.CellContentAlignmentResolved)
                {
                    case CellContentAlignment.LabelOnly:
                        return labelLayoutItem.GetSize(size);
                    case CellContentAlignment.ValueOnly:
                        return cellLayoutItem.GetSize(size);
                    case CellContentAlignment.LabelAboveValueAlignCenter:
                    case CellContentAlignment.LabelAboveValueAlignLeft:
                    case CellContentAlignment.LabelAboveValueAlignRight:
                    case CellContentAlignment.LabelAboveValueStretch:
                    case CellContentAlignment.LabelBelowValueAlignCenter:
                    case CellContentAlignment.LabelBelowValueAlignLeft:
                    case CellContentAlignment.LabelBelowValueAlignRight:
                    case CellContentAlignment.LabelBelowValueStretch:
                        // if height then add preferred, min, max
                        // if width then max the min, min the max, and max the preferred within the max of the max
                        return CombineValues(true, labelLayoutItem, cellLayoutItem, size);
                    case CellContentAlignment.LabelLeftOfValueAlignBottom:
                    case CellContentAlignment.LabelLeftOfValueAlignMiddle:
                    case CellContentAlignment.LabelLeftOfValueAlignTop:
                    case CellContentAlignment.LabelLeftOfValueStretch:
                    case CellContentAlignment.LabelRightOfValueAlignBottom:
                    case CellContentAlignment.LabelRightOfValueAlignMiddle:
                    case CellContentAlignment.LabelRightOfValueAlignTop:
                    case CellContentAlignment.LabelRightOfValueStretch:
                        // if width then add preferred, min, max
                        // if height then max the min, min the max, and max the preferred within the max of the max
                        return CombineValues(false, labelLayoutItem, cellLayoutItem, size);
                }
            }

            return double.NaN;
        } 
        #endregion //GetDefaultCellPresenterSize

        #region GetDefaultLabelSize
        internal static double GetDefaultLabelSize(LayoutItemSize size, Field field)
        {
            switch (size)
            {
                case LayoutItemSize.MaxHeight:
                    return field.LabelMaxHeightResolved;
                case LayoutItemSize.MinHeight:
                    return field.LabelMinHeightResolved;
                case LayoutItemSize.PreferredHeight:
                    return field.GetLabelHeightResolvedHelper(true, true);
                case LayoutItemSize.MaxWidth:
                    return field.LabelMaxWidthResolved;
                case LayoutItemSize.MinWidth:
                    return field.LabelMinWidthResolved;
                case LayoutItemSize.PreferredWidth:
                    return field.GetLabelWidthResolvedHelper(true, true);
                default:
                    Debug.Fail("Unrecognized size");
                    return double.NaN;
            }
        } 
        #endregion //GetDefaultLabelSize

        #region OutputSizeInfo
        [Conditional("DEBUG")]
        internal static void DebugSizeInfo(object value, string category)
        {
            DebugSizeInfoImpl(value, category);
        }

        [Conditional("DEBUG")]
        protected static void DebugItemSizeInfo(object value, string category)
        {
            DebugSizeInfoImpl(value, "LayoutItem - " + category);
        }

        [Conditional("DEBUG_SIZECHANGES")]
        private static void DebugSizeInfoImpl(object value, string category)
        {
            Debug.WriteLine(value, DateTime.Now.ToString("hh:mm:ss:ffffff") + " " + category);
        }

        #endregion //OutputSizeInfo

        #endregion //Methods
    }
    #endregion //FieldLayoutItemBase

    #region FieldLayoutItem
    /// <summary>
    /// Layout item used to represent a Label/CellValuePresenter in a FieldLayout
    /// </summary>
    internal class FieldLayoutItem : FieldLayoutItemBase,
        IGridBagConstraint
    {
        #region Member Variables

        private double[] _sizes;
        private int _version = -1;
        private Visibility _visibility;
        private GridBagConstraint _gc;
        internal const int CellPresenterFactor = 2;
        internal const int MinimumExtent = 6;

		// AS 7/30/09 NA 2009.2 Field Sizing
		private ItemSizeType _sizeTypeWidth;
		private ItemSizeType _sizeTypeHeight;

        #endregion //Member Variables

        #region Constructor
        internal FieldLayoutItem(Field field, bool isLabel)
            : base(field, isLabel)
        {
            this._sizes = new double[6];
        }
        #endregion //Constructor

        #region Properties

        #region CellPresenterConstraint
		// AS 6/9/09 NA 2009.2 Field Sizing
        //private GridBagConstraint CellPresenterConstraint
        internal GridBagConstraint CellPresenterConstraint
        {
            get
            {
                if (null == this._gc)
                {
                    this._gc = new GridBagConstraint();
                    this.InitializeGC();
                }

                this.VerifyVersion();

                return this._gc;
            }
        }
        #endregion //CellPresenterConstraint

        #endregion //Properties

        #region Methods

        // AS 2/13/09 TFS13988
        #region AdjustPositionForCellPresenter
        internal static void AdjustPositionForCellPresenter(CellContentAlignment alignment, bool isLabel, 
            ref int column, ref int row, ref int rowSpan, ref int columnSpan,
            // AS 2/26/09 CellPresenter Chrome
            // We need to take the cell presenter margin - i.e. the area around the cellpresenterlayoutelement
            // in the CellPresenter - and use only the portion needed based on where the layout item will 
            // be with respect to the other item (i.e. where the label is with respect to the cvp).
            //
            ref Thickness margin)
        {
            Debug.Assert(CellPresenterFactor == 2);

            column *= CellPresenterFactor;
            row *= CellPresenterFactor;

            switch (alignment)
            {
                case CellContentAlignment.LabelAboveValueAlignCenter:
                case CellContentAlignment.LabelAboveValueAlignLeft:
                case CellContentAlignment.LabelAboveValueAlignRight:
                case CellContentAlignment.LabelAboveValueStretch:
                    {
                        // both label and cell span both logical columns
                        columnSpan *= 2;

                        // the cell needs to be on the bottom row
                        if (!isLabel)
                            row += rowSpan;

                        // AS 2/26/09 CellPresenter Chrome
                        if (isLabel)
                            margin.Bottom = 0;
                        else
                            margin.Top = 0;
                        break;
                    }
                case CellContentAlignment.LabelBelowValueAlignCenter:
                case CellContentAlignment.LabelBelowValueAlignLeft:
                case CellContentAlignment.LabelBelowValueAlignRight:
                case CellContentAlignment.LabelBelowValueStretch:
                    {
                        // both label and cell span both logical columns
                        columnSpan *= 2;

                        // the label needs to be on the bottom row
                        if (isLabel)
                            row += rowSpan;

                        // AS 2/26/09 CellPresenter Chrome
                        if (isLabel)
                            margin.Top = 0;
                        else
                            margin.Bottom = 0;
                        break;
                    }
                case CellContentAlignment.LabelLeftOfValueAlignBottom:
                case CellContentAlignment.LabelLeftOfValueAlignMiddle:
                case CellContentAlignment.LabelLeftOfValueAlignTop:
                case CellContentAlignment.LabelLeftOfValueStretch:
                    {
                        // both label and cell span both logical rows
                        rowSpan *= 2;

                        // the cell needs to be on the bottom row
                        if (!isLabel)
                            column += columnSpan;

                        // AS 2/26/09 CellPresenter Chrome
                        if (isLabel)
                            margin.Right = 0;
                        else
                            margin.Left = 0;
                        break;
                    }
                case CellContentAlignment.LabelRightOfValueAlignBottom:
                case CellContentAlignment.LabelRightOfValueAlignMiddle:
                case CellContentAlignment.LabelRightOfValueAlignTop:
                case CellContentAlignment.LabelRightOfValueStretch:
                    {
                        // both label and cell span both logical rows
                        rowSpan *= 2;

                        // the label needs to be on the bottom row
                        if (isLabel)
                            column += columnSpan;

                        // AS 2/26/09 CellPresenter Chrome
                        if (isLabel)
                            margin.Left = 0;
                        else
                            margin.Right = 0;
                        break;
                    }
                case CellContentAlignment.LabelOnly:
                case CellContentAlignment.ValueOnly:
                    // have it fill the area
                    rowSpan *= 2;
                    columnSpan *= 2;
                    break;
            }
        }
        #endregion //AdjustPositionForCellPresenter

        #region GetDefaultSize
        private double GetDefaultSize(LayoutItemSize size)
        {
            // if we're synchronizing then combine the values
            if (ShouldSynchronize(size, this.Field))
                return GetSynchronizedSize(size, this.Field);

            if (this.IsLabel)
                return GetDefaultLabelSize(size, this.Field);

            return GetDefaultCellSize(size, this.Field);
        }
        #endregion //GetDefaultSize

        #region VerifyVersion
		// AS 10/12/09
        //private void VerifyVersion()
		internal void VerifyVersion()
        {
            int fieldVersion = this.Field.LayoutItemVersion;

            if (this._version != fieldVersion)
            {
				this.VerifyVersionImpl(fieldVersion);
			}
		}

		// AS 7/30/09 NA 2009.2 Field Sizing
		// Moved implementation into a helper method.
		//
		private void VerifyVersionImpl(int fieldVersion)
		{
			DebugItemSizeInfo(string.Format("Old:{0}, New:{1}, IsLabel:{2}, Field:{3}", this._version, fieldVersion, this.IsLabel, this.Field), "VerifyVersion Start");
			DebugItemSizeInfo(string.Format("Vis:{0}, MinW:{1}, PrefW:{2}, MaxW:{3}, MinH:{4}, PrefH:{5}, MaxH:{6}",
				this._visibility, this._sizes[0], this._sizes[1], this._sizes[2], this._sizes[3], this._sizes[4], this._sizes[5]), "VerifyVersion Old");

			this._version = fieldVersion;

			for (int i = 0; i < 6; i++)
			{
				this._sizes[i] = this.GetDefaultSize((LayoutItemSize)i);
			}

			// AS 7/30/09 NA 2009.2 Field Sizing
			_sizeTypeHeight = this.Field.GetResizeSize(this.IsLabel, false).Type;
			_sizeTypeWidth = this.Field.GetResizeSize(this.IsLabel, true).Type;

			// AS 1/26/09
			// To be consistent with the previous versions, we need to 
			// always force a minimum extent of 6 pixels.
			//
			double minExtentImposed = MinimumExtent;

			// AS 7/9/09 TFS19237
			FieldLayout fl = this.Field.Owner;
			bool useCellPresenters = null != fl && fl.UseCellPresenters;

			// AS 5/11/09 TFS17356
			// When the labels are with the cells we will not impose a hard minimum on the label. 
			// The minimum will still be imposed on the cells though.
			//
			if (this.IsLabel)
			{
				// AS 7/9/09 TFS19237
				//FieldLayout fl = this.Field.Owner;
				//
				//if (null != fl && fl.UseCellPresenters)
				if (useCellPresenters)
				{
					minExtentImposed = 0d;
				}
			}

			_sizes[(int)LayoutItemSize.MinWidth] = Math.Min(_sizes[(int)LayoutItemSize.MaxWidth], Math.Max(minExtentImposed, _sizes[(int)LayoutItemSize.MinWidth]));
			_sizes[(int)LayoutItemSize.MinHeight] = Math.Min(_sizes[(int)LayoutItemSize.MaxHeight], Math.Max(minExtentImposed, _sizes[(int)LayoutItemSize.MinHeight]));

			this._visibility = this.Field.IsVisibleInCellArea
				? this.Field.VisibilityResolved
				: Visibility.Collapsed;

			if (null != this._gc)
				this.InitializeGC();

			// when labels are with cells we may need to hide the label/cell
			if (this._visibility != Visibility.Collapsed)
			{
				
				
				
				
				
				
				
				
				
				
				
				// AS 7/9/09 TFS19237
				// We need the item to be hidden but not provide constraints that would 
				// affect the scrolling extent at least in the case where the headers 
				// are separate from the cells. E.g. we don't want the height constraints 
				// of a label to affect the height of the labels in a vertically oriented
				// grid.
				//
				//const Visibility Only1Visibility = Visibility.Collapsed;
				//const Visibility Only1Visibility = Visibility.Hidden;
				Visibility Only1Visibility = useCellPresenters ? Visibility.Collapsed : Visibility.Hidden;

				// AS 1/18/10 TFS25729
				bool isConsideredCollapsed = false;

                switch (this.Field.CellContentAlignmentResolved)
                {
                    case CellContentAlignment.ValueOnly:
						if (this.IsLabel)
						{
							this._visibility = Only1Visibility;

							// AS 1/18/10 TFS25729
							isConsideredCollapsed = true;
						}
                        break;
                    case CellContentAlignment.LabelOnly:
						if (!this.IsLabel)
						{
							this._visibility = Only1Visibility;

							// AS 1/18/10 TFS25729
							isConsideredCollapsed = true;
						}
                        break;
                }

				// AS 1/18/10 TFS25729
				//// AS 7/9/09 TFS19237
				//if (null != fl && _visibility == Visibility.Hidden)
				if (null != fl && isConsideredCollapsed && _visibility == Visibility.Hidden)
				{
					if (!fl.IsHorizontal)
					{
						_sizes[(int)LayoutItemSize.MinHeight] = 0;
						_sizes[(int)LayoutItemSize.PreferredHeight] = 0;
						_sizes[(int)LayoutItemSize.MaxHeight] = double.PositiveInfinity;
					}
					else
					{
						_sizes[(int)LayoutItemSize.MinWidth] = 0;
						_sizes[(int)LayoutItemSize.PreferredWidth] = 0;
						_sizes[(int)LayoutItemSize.MaxWidth] = double.PositiveInfinity;
					}
				}
			}

			DebugItemSizeInfo(string.Format("Vis:{0}, MinW:{1}, PrefW:{2}, MaxW:{3}, MinH:{4}, PrefH:{5}, MaxH:{6}",
				this._visibility, this._sizes[0], this._sizes[1], this._sizes[2], this._sizes[3], this._sizes[4], this._sizes[5]), "VerifyVersion New");
			DebugItemSizeInfo(string.Format("IsLabel:{0}, Field:{1}", this.IsLabel, this.Field), "VerifyVersion End");

		}
        #endregion //VerifyVersion

        #region InitializeGC
        private void InitializeGC()
        {
            if (null != this._gc)
            {
                GridBagConstraint gridPos = this.Field.LayoutConstraint;

                // start off with doubling the spans. so if we would normally
                // have had 4 fields in a 2x2 situation, the positions would have
                // been:
                // 0,0 1x1
                // 0,1 1x1
                // 1,0 1x1
                // 1,1 1x1
                // CellA CellB
                // CellC CellD
                //
                // to use this for cell presenters and therefore have a slot for
                // the label and one for the cell, we will double the size so they 
                // will be something like
                // 0,0 2x2
                // 0,2 2x2
                // 2,0 2x2
                // 2,2 2x2
                // then depending on the cellcontentalignment we will position
                // the label & cell within that 2x2 slot. so assuming the labels
                // are above the cells, the logical cells would look something like:
                // LabelA  LabelB
                // CellA   CellB
                // LabelC  LabelD
                // CellC   CellD
                // 

                // since the labels will be with the cells, we want to then
                // adjust where the label/cell 

                #region Refactored
                
#region Infragistics Source Cleanup (Region)






































































#endregion // Infragistics Source Cleanup (Region)

                #endregion //Refactored
                int column = gridPos.Column;
                int row = gridPos.Row;
                int rowSpan = gridPos.RowSpan;
                int columnSpan = gridPos.ColumnSpan;

                // AS 2/26/09 CellPresenter Chrome
                FieldLayout fl = this.Field.Owner;
                Thickness margin = null != fl ? fl.TemplateDataRecordCache.GetCellPresenterMargin(this.Field) : new Thickness();

                AdjustPositionForCellPresenter(this.Field.CellContentAlignmentResolved, this.IsLabel, ref column, ref row, ref rowSpan, ref columnSpan, ref margin);

                this._gc.Column = column;
                this._gc.Row = row;
                this._gc.RowSpan = rowSpan;
                this._gc.ColumnSpan = columnSpan;

                // AS 2/26/09 CellPresenter Chrome
                this._gc.Margin = margin;

				// AS 10/13/09 NA 2010.1 - CardView
				// Pass autofit mode to GetAutoFitWeight.
				//
				AutoFitMode autoFitMode = fl != null ? fl.AutoFitModeResolved : AutoFitMode.Never;

				// AS 10/12/09
				// We should initialize the row/column weigth as we do in the FieldGridBagLayoutManager
				//
				this._gc.ColumnWeight = this.Field.GetAutoFitWeight(!this.IsLabel, true, autoFitMode);
				this._gc.RowWeight = this.Field.GetAutoFitWeight(!this.IsLabel, false, autoFitMode);
			}
        }

        #endregion //InitializeGC

        #endregion //Methods

        #region Base class overrides

        #region GetSize
        public override sealed double GetSize(LayoutItemSize size)
        {
            this.VerifyVersion();

            return this._sizes[(int)size];
        }
        #endregion //GetSize

        #region GetVisibility
        protected override Visibility GetVisibility()
        {
            this.VerifyVersion();
            return this._visibility;
        } 
        #endregion //GetVisibility

		// AS 1/25/10 SizableSynchronized changes
		#region RecordSizingMode
		public override DataRecordSizingMode RecordSizingMode
		{
			get { return this.Field.Owner.DataRecordSizingModeResolved; }
		}
		#endregion //RecordSizingMode

		#region SetPreferredHeight
		// AS 7/29/09 NA 2009.2 Field Sizing
        //protected override void SetPreferredHeight(double value)
		public override void SetPreferredHeight(double value, ItemSizeType type)
        {
            DebugItemSizeInfo(string.Format("Old={0}, New={1}, IsLabel:{2}, Field:{3}", this._sizes[(int)LayoutItemSize.PreferredHeight], value, this.IsLabel, this.Field), "Change Cached PreferredHeight");

            this._sizes[(int)LayoutItemSize.PreferredHeight] = value;

            bool sync = this.Field.SyncCellLabelHeight;

			// AS 7/29/09 NA 2009.2 Field Sizing
			Field.FieldResizeInfo resizeInfo = this.Field.ExplicitResizeInfo;

            if (this.IsLabel || sync)
            {
                DebugItemSizeInfo(string.Format("Old:{0}, New:{1}, IsLabel:{2}, Field:{3}", this.Field.ExplicitResizeInfo.GetSize(true, false), value, this.IsLabel, this.Field), "Explicit Label Height");

				// AS 7/29/09 NA 2009.2 Field Sizing
                //this.Field.ExplicitResizeInfo.LabelHeight = value;
				resizeInfo.SetSize(true, false, new FieldSize(value, type));
            }

            if (!this.IsLabel || sync)
            {
                DebugItemSizeInfo(string.Format("Old:{0}, New:{1}, IsLabel:{2}, Field:{3}", this.Field.ExplicitResizeInfo.GetSize(false, false), value, this.IsLabel, this.Field), "Explicit Cell Height");

				// AS 7/29/09 NA 2009.2 Field Sizing
				//this.Field.ExplicitResizeInfo.CellHeight = value;
				resizeInfo.SetSize(false, false, new FieldSize(value, type));
			}

            this.OnHeightChanged();
        }
        #endregion //SetPreferredHeight

        #region SetPreferredWidth
		// AS 7/29/09 NA 2009.2 Field Sizing
		//protected override void SetPreferredWidth(double value)
		public override void SetPreferredWidth(double value, ItemSizeType type)
        {
            DebugItemSizeInfo(string.Format("Old={0}, New={1}, IsLabel:{2}, Field:{3}", this._sizes[(int)LayoutItemSize.PreferredWidth], value, this.IsLabel, this.Field), "Change Cached PreferredWidth");

            this._sizes[(int)LayoutItemSize.PreferredWidth] = value;

            bool sync = this.Field.SyncCellLabelWidth;

			// AS 7/29/09 NA 2009.2 Field Sizing
			Field.FieldResizeInfo resizeInfo = this.Field.ExplicitResizeInfo;

			if (this.IsLabel || sync)
            {
                DebugItemSizeInfo(string.Format("Old:{0}, New:{1}, IsLabel:{2}, Field:{3}", this.Field.ExplicitResizeInfo.GetSize(true, true), value, this.IsLabel, this.Field), "Explicit Label Width");

				// AS 7/29/09 NA 2009.2 Field Sizing
				//this.Field.ExplicitResizeInfo.LabelWidth = value;
				resizeInfo.SetSize(true, true, new FieldSize(value, type));
			}

            if (!this.IsLabel || sync)
            {
                DebugItemSizeInfo(string.Format("Old:{0}, New:{1}, IsLabel:{2}, Field:{3}", this.Field.ExplicitResizeInfo.GetSize(false, true), value, this.IsLabel, this.Field), "Explicit Cell Width");

				// AS 7/29/09 NA 2009.2 Field Sizing
				//this.Field.ExplicitResizeInfo.CellWidth = value;
				resizeInfo.SetSize(false, true, new FieldSize(value, type));
			}

            this.OnWidthChanged();
        }
        #endregion //SetPreferredWidth

		// AS 7/30/09 NA 2009.2 Field Sizing
		#region SizeType
		public override ItemSizeType SizeTypeHeight
		{
			get
			{
				this.VerifyVersion();
				return _sizeTypeHeight;
			}
		}

		public override ItemSizeType SizeTypeWidth
		{
			get
			{
				this.VerifyVersion();
				return _sizeTypeWidth;
			}
		}
		#endregion //SizeType

		#endregion //Base class overrides

        #region IGridBagConstraint Members

        int IGridBagConstraint.Column
        {
            get { return this.CellPresenterConstraint.Column; }
        }

        int IGridBagConstraint.ColumnSpan
        {
            get { return this.CellPresenterConstraint.ColumnSpan; }
        }

        float IGridBagConstraint.ColumnWeight
        {
            get { return this.CellPresenterConstraint.ColumnWeight; }
        }

        HorizontalAlignment IGridBagConstraint.HorizontalAlignment
        {
            get { return this.CellPresenterConstraint.HorizontalAlignment; }
        }

        Thickness IGridBagConstraint.Margin
        {
            get { return this.CellPresenterConstraint.Margin; }
        }

        int IGridBagConstraint.Row
        {
            get { return this.CellPresenterConstraint.Row; }
        }

        int IGridBagConstraint.RowSpan
        {
            get { return this.CellPresenterConstraint.RowSpan; }
        }

        float IGridBagConstraint.RowWeight
        {
            get { return this.CellPresenterConstraint.RowWeight; }
        }

        VerticalAlignment IGridBagConstraint.VerticalAlignment
        {
            get { return this.CellPresenterConstraint.VerticalAlignment; }
        }

        #endregion //IGridBagConstraint
	}
    #endregion //FieldLayoutItem

    #region CellLayoutItem
    /// <summary>
    /// A proxy FieldLayoutItem that delegates most requests for constraints from its 
    /// wrapped FieldLayoutItem but can have a different preferred width/height. This is 
    /// used for layout items within a specific record/virtualizing record panel.
    /// </summary>
    internal class CellLayoutItem : FieldLayoutItemBase
		// AS 10/13/09 NA 2010.1 - CardView
		//, ICellPanelLayoutItem
		, IWrapperLayoutItem
    {
        #region Member Variables

		// AS 10/13/09 NA 2010.1 - CardView
        //private VirtualizingDataRecordCellPanel _currentPanel;

        private FieldLayoutItemBase _layoutItem;
        private double? _preferredWidth;
        private double? _preferredHeight;
        private bool _forceUnfixed;

		// AS 10/13/09 NA 2010.1 - CardView
		private FieldGridBagLayoutManager _manager;

        #endregion //Member Variables

        #region Constructor
        internal CellLayoutItem(FieldLayoutItemBase layoutItem)
            : base(layoutItem.Field, layoutItem.IsLabel)
        {
            this._layoutItem = layoutItem;
        }
        #endregion //Constructor

        #region Base class overrides

        #region FixedLocation
        public override FixedFieldLocation FixedLocation
        {
            get
            {
                if (_forceUnfixed)
                    return FixedFieldLocation.Scrollable;

                return base.FixedLocation;
            }
        } 
        #endregion //FixedLocation

        #region GetVisibility
        protected override Visibility GetVisibility()
        {
			// AS 1/13/12 TFS73068
			// We need to keep these visible but force their vertical 
			// extents to be 0.
			//
			//// AS 10/13/09 NA 2010.1 - CardView
			//if (null != _manager)
			//{
			//    Record r = _manager.Record;
			//
			//    if (null != r && r.ShouldCollapseCell(this.Field))
			//        return Visibility.Collapsed;
			//}

            return ((ILayoutItem)this._layoutItem).Visibility;
        }
        #endregion //GetVisibility

        #region GetSize
        public override double GetSize(LayoutItemSize size)
        {
            
            switch (size)
            {
                case LayoutItemSize.PreferredWidth:
                    if (null != this._preferredWidth)
                        return this._preferredWidth.Value;
                    break;
                case LayoutItemSize.PreferredHeight:
                    if (null != this._preferredHeight)
                        return this._preferredHeight.Value;

                    double extent;
					// AS 10/13/09 NA 2010.1 - CardView
                    //if (null != this._currentPanel && this._currentPanel.TryGetPreferredExtent(this, !this.IsLabel, false, out extent))
					VirtualizingDataRecordCellPanel currentPanel = _manager != null ? _manager.CellPanel : null;

                    if (null != currentPanel && currentPanel.TryGetPreferredExtent(this, !this.IsLabel, false, out extent))
                    {
                        DebugItemSizeInfo(string.Format("Extent Returned={0}, Base Item Size={1}, IsLabel:{2}, Field:{3}", extent, this._layoutItem.GetSize(size), this.IsLabel, this.Field), "Wrapper PreferredHeight from VirtPanel");

                        return extent;
                    }
                    break;
				case LayoutItemSize.MaxHeight:
				case LayoutItemSize.MinHeight:
					// AS 1/13/12 TFS73068
					// We need the collapsed cells to be visible but occupy no vertical space
					// so the logical row can be collapsed.
					//
					if (null != _manager)
					{
						Record r = _manager.Record;

						if (null != r && r.ShouldCollapseCell(this.Field))
							return 0;
					}
					return this._layoutItem.GetSize(size);
				default:
                    return this._layoutItem.GetSize(size);
            }

            return this._layoutItem.GetSize(size);
        }
        #endregion //GetSize

		// AS 1/25/10 SizableSynchronized changes
		#region RecordSizingMode
		public override DataRecordSizingMode RecordSizingMode
		{
			get
			{
				if (_manager != null)
					return _manager.RecordSizingMode;

				return this.InnerLayoutItem.RecordSizingMode;
			}
		}
		#endregion //RecordSizingMode

		#region SetPreferred
		// AS 7/29/09 NA 2009.2 Field Sizing
		//protected override void SetPreferredHeight(double value)
		public override void SetPreferredHeight(double value, ItemSizeType type)
        {
            FieldLayout fl = this.Field.Owner;

			// AS 1/25/10 SizableSynchronized changes
			//if (fl == null || fl.IsHorizontal || !GridUtilities.IsVariableHeightRecordMode(fl.DataRecordSizingModeResolved))
            if (fl == null || fl.IsHorizontal || !GridUtilities.IsVariableHeightRecordMode(this.RecordSizingMode))
				// AS 7/29/09 NA 2009.2 Field Sizing
				//this._layoutItem.PreferredHeight = value;
                this._layoutItem.SetPreferredHeight(value, type);
            else
            {
                DebugItemSizeInfo(string.Format("Old={0}, New={1}, IsLabel:{2}, Field:{3}", this._preferredHeight, value, this.IsLabel, this.Field), "Change Wrapper PreferredHeight");

                this._preferredHeight = value;
                this.OnHeightChanged();
            }
        }

		// AS 7/29/09 NA 2009.2 Field Sizing
		//protected override void SetPreferredWidth(double value)
		public override void SetPreferredWidth(double value, ItemSizeType type)
        {
            FieldLayout fl = this.Field.Owner;

			// AS 1/25/10 SizableSynchronized changes
			//if (fl == null || !fl.IsHorizontal || !GridUtilities.IsVariableHeightRecordMode(fl.DataRecordSizingModeResolved))
            if (fl == null || !fl.IsHorizontal || !GridUtilities.IsVariableHeightRecordMode(this.RecordSizingMode))
				// AS 7/29/09 NA 2009.2 Field Sizing
				//this._layoutItem.PreferredWidth = value;
				this._layoutItem.SetPreferredWidth(value, type);
            else
            {
                DebugItemSizeInfo(string.Format("Old={0}, New={1}, IsLabel:{2}, Field:{3}", this._preferredWidth, value, this.IsLabel, this.Field), "Change Wrapper PreferredWidth");

                this._preferredWidth = value;
                this.OnWidthChanged();
            }
        }
        #endregion //SetPreferred

		// AS 7/29/09 NA 2009.2 Field Sizing
		#region SizeType
		public override ItemSizeType SizeTypeHeight
		{
			get
			{
				return _layoutItem.SizeTypeHeight;
			}
		}

		public override ItemSizeType SizeTypeWidth
		{
			get
			{
				return _layoutItem.SizeTypeWidth;
			}
		}
		#endregion //SizeType

        #endregion //Base class overrides

        #region Properties

        #region CurrentPanel
		// AS 10/13/09 NA 2010.1 - CardView
		// Since we will have a reference back to the owning manager we 
		// don't need to have each item keep a reference to the panel. We 
		// can just keep that on the layout manager itself.
		//
		//VirtualizingDataRecordCellPanel ICellPanelLayoutItem.CurrentPanel
		//{
		//    get { return this._currentPanel; }
		//    set { this._currentPanel = value; }
		//}
        #endregion //CurrentPanel

        #region ForceUnfixed
        internal bool ForceUnfixed
        {
            get { return _forceUnfixed; }
            set { _forceUnfixed = value; }
        } 
        #endregion //ForceUnfixed

        #region InnerLayoutItem
        internal FieldLayoutItemBase InnerLayoutItem
        {
            get { return this._layoutItem; }
        } 
        #endregion //InnerLayoutItem

		// AS 10/13/09 NA 2010.1 - CardView
		#region LayoutManager
		internal FieldGridBagLayoutManager LayoutManager
		{
			get { return _manager; }
			set { _manager = value; }
		} 
		#endregion //LayoutManager

        #endregion //Properties

        #region Methods

        #region InitializeFrom
        internal void InitializeFrom(CellLayoutItem srcItem)
        {
            Debug.Assert(null != srcItem);

            this._preferredHeight = srcItem._preferredHeight;
            this._preferredWidth = srcItem._preferredWidth;
        }
        #endregion //InitializeFrom

        #endregion //Methods

		#region IWrapperLayoutItem Members

		ILayoutItem IWrapperLayoutItem.InnerLayoutItem
		{
			get { return _layoutItem; }
		}

		double? IWrapperLayoutItem.ExplicitWidth
		{
			get
			{
				return _preferredWidth;
			}
			set
			{
				if (!object.Equals(_preferredWidth, value))
				{
					_preferredWidth = value;
					this.OnWidthChanged();
				}
			}
		}

		double? IWrapperLayoutItem.ExplicitHeight
		{
			get
			{
				return _preferredHeight;
			}
			set
			{
				if (!object.Equals(_preferredHeight, value))
				{
					_preferredHeight = value;
					this.OnWidthChanged();
				}
			}
		}

		#endregion
	} 
    #endregion //CellLayoutItem

    #region GridDefinitionLayoutItem
    internal class GridDefinitionLayoutItem : LayoutItem, IGridBagConstraint
    {
        #region Member Variables

        private DefinitionBase _rowCol;
        private int _originX;
        private int _originY;
        private float _weightX;
        private float _weightY;
        private int _span;
        private static readonly Thickness EmptyThickness = new Thickness();

        // AS 4/16/09 TFS16467
        // The preferred width/height for the layout item needs to be settable
        // so that the size can be adjusted when resized via the ui. To that end
        // instead of returning a value based on the associated definition we will
        // now cache that when the layout item is created and allow it to be modified.
        //
        //
        private double _preferredX;
        private double _preferredY;

        #endregion //Member Variables

        #region Constructor
        internal GridDefinitionLayoutItem(RowDefinition row, int originY, bool usingCellPresenters) :
            this(row, 0, originY, usingCellPresenters)
        {
        }

        internal GridDefinitionLayoutItem(ColumnDefinition col, int originX, bool usingCellPresenters) :
            this(col, originX, 0, usingCellPresenters)
        {
        }

        private GridDefinitionLayoutItem(DefinitionBase rowCol, int originX, int originY, bool usingCellPresenters)
        {
            // if we're using cell presenters we also need to offset the origins as we 
            // do for the cell presenter constraints
            if (usingCellPresenters)
            {
                originX *= 2;
                originY *= 2;
            }

            this._rowCol = rowCol;
            RowDefinition row = _rowCol as RowDefinition;
            ColumnDefinition col = _rowCol as ColumnDefinition;
            this._originX = originX;
            this._originY = originY;
            this._span = usingCellPresenters ? 2 : 1;

            if (col != null)
            {
                this._weightX = col.Width.IsStar ? (float)col.Width.Value : 0f;

                // AS 4/16/09 TFS16467
                _preferredX = col.Width.IsAbsolute ? col.Width.Value : 0d;
            }
            else
            {
                this._weightY = row.Height.IsStar ? (float)row.Height.Value : 0f;

                // AS 4/16/09 TFS16467
                _preferredY = row.Height.IsAbsolute ? row.Height.Value : 0d;
            }
        }
        #endregion //Constructor

        #region Properties
        // AS 4/16/09 TFS16467
        public override double PreferredWidth
        {
            get { return _preferredX; }
			// AS 7/29/09 NA 2009.2 Field Sizing
            //set { _preferredX = value; }
        }

        // AS 4/16/09 TFS16467
        public override double PreferredHeight
        {
            get { return _preferredY; }
			// AS 7/29/09 NA 2009.2 Field Sizing
			//set { _preferredY = value; }
        }
        #endregion //Properties

        #region Methods
        #region IsItemNeeded
        internal static bool IsItemNeeded(RowDefinition row)
        {
            return row.Height.IsAuto == false ||
                row.MinHeight != 0 ||
                !double.IsPositiveInfinity(row.MaxHeight);
        }

        internal static bool IsItemNeeded(ColumnDefinition col)
        {
            return col.Width.IsAuto == false ||
                col.MinWidth != 0 ||
                !double.IsPositiveInfinity(col.MaxWidth);
        } 
    	#endregion //IsItemNeeded 

    	#endregion //Methods

        #region Base class overrides
        public override double GetSize(LayoutItemSize size)
        {
            RowDefinition row = _rowCol as RowDefinition;
            ColumnDefinition col = _rowCol as ColumnDefinition;

            switch (size)
            {
                case LayoutItemSize.MinWidth:
                    return col != null ? col.MinWidth : 0d;
                case LayoutItemSize.MaxWidth:
                    return col != null ? col.MaxWidth : double.PositiveInfinity;
                case LayoutItemSize.PreferredWidth:
                    // AS 4/16/09 TFS16467
                    //return col != null && col.Width.IsAbsolute ? col.Width.Value : 0d;
                    return this.PreferredWidth;

                case LayoutItemSize.MinHeight:
                    return row != null ? row.MinHeight : 0d;
                case LayoutItemSize.MaxHeight:
                    return row != null ? row.MaxHeight : double.PositiveInfinity;
                case LayoutItemSize.PreferredHeight:
                    // AS 4/16/09 TFS16467
                    //return row != null ? row.Height.Value : 0d;
                    return this.PreferredHeight;

                default:
                    Debug.Fail("Unexpected size");
                    return 0d;
            }
        }

        protected override Visibility GetVisibility()
        {
            return Visibility.Hidden;
        }

		// AS 7/29/09 NA 2009.2 Field Sizing
		public override void SetPreferredHeight(double value, ItemSizeType type)
		{
			_preferredY = value;
		}

		// AS 7/29/09 NA 2009.2 Field Sizing
		public override void SetPreferredWidth(double value, ItemSizeType type)
		{
			_preferredX = value;
		}
        #endregion //Base class overrides

        #region IGridBagConstraint Members

        int IGridBagConstraint.Column
        {
            get { return _originX; }
        }

        int IGridBagConstraint.ColumnSpan
        {
            get { return _span; }
        }

        float IGridBagConstraint.ColumnWeight
        {
            get { return _weightX; }
        }

        HorizontalAlignment IGridBagConstraint.HorizontalAlignment
        {
            get { return HorizontalAlignment.Left; }
        }

        Thickness IGridBagConstraint.Margin
        {
            get { return EmptyThickness; }
        }

        int IGridBagConstraint.Row
        {
            get { return _originY; }
        }

        int IGridBagConstraint.RowSpan
        {
            get { return _span; }
        }

        float IGridBagConstraint.RowWeight
        {
            get { return _weightY; }
        }

        VerticalAlignment IGridBagConstraint.VerticalAlignment
        {
            get { return VerticalAlignment.Top; }
        }

        #endregion //IGridBagConstraint
    } 
    #endregion //GridDefinitionLayoutItem

    #region ItemLayoutInfoLayoutItem
    // AS 2/2/09
    // When calculating the resulting layout for a splitter drag, we need to be able to use 
    // the resulting constraints (i.e. Column/Row) so we needed a derived layout item that 
    // would use an ItemLayoutInfo to provide those values.
    //
    /// <summary>
    /// Custom CellLayoutItem whose Row/Column origins are obtained from a given ItemLayoutInfo instance.
    /// </summary>
    internal class ItemLayoutInfoLayoutItem : CellLayoutItem,
        IGridBagConstraint
    {
        #region Member Variables

        private ItemLayoutInfo _layoutInfo;
        private IGridBagConstraint _fieldConstraint;

        // AS 2/26/09 CellPresenter Chrome
        private Thickness? _margin;

        #endregion //Member Variables

        #region ItemLayoutInfoLayoutItem
        internal ItemLayoutInfoLayoutItem(FieldLayoutItemBase layoutItem, IGridBagConstraint baseConstaint, ItemLayoutInfo layoutInfo)
            : base(layoutItem)
        {
            GridUtilities.ValidateNotNull(layoutInfo);

            // AS 2/13/09 TFS13988
            // We cannot use the layout info as is since it will be based on the 
            // actual row/column information. The layout item may be used to 
            // represent a cellpresenter in which case the origins and spans 
            // should be adjusted. Instead of manipulating the layout info we're
            // given, we'll clone it and adjust it in the same way we had been
            // in the FieldLayoutItem.
            //
            //_layoutInfo = layoutInfo;
            _layoutInfo = layoutInfo.Clone();

            FieldLayout fl = this.Field.Owner;
            Debug.Assert(null != fl);

            if (null != fl && fl.UseCellPresenters)
            {
                // AS 2/13/09 TFS13988
                int row = _layoutInfo.Row;
                int column = _layoutInfo.Column;
                int rowSpan = _layoutInfo.RowSpan;
                int columnSpan = _layoutInfo.ColumnSpan;

                // AS 2/26/09 CellPresenter Chrome
                Thickness margin = fl.TemplateDataRecordCache.GetCellPresenterMargin(this.Field);

                FieldLayoutItem.AdjustPositionForCellPresenter(this.Field.CellContentAlignmentResolved, this.IsLabel, ref column, ref row, ref rowSpan, ref columnSpan, ref margin);

                _layoutInfo.Row = row;
                _layoutInfo.RowSpan = rowSpan;
                _layoutInfo.Column = column;
                _layoutInfo.ColumnSpan = columnSpan;

                // AS 2/26/09 CellPresenter Chrome
                _margin = margin;
            }
                

            _fieldConstraint = baseConstaint ?? layoutItem.Field.LayoutConstraint;
        }
        #endregion //ItemLayoutInfoLayoutItem

        #region IGridBagConstraint Members

        int IGridBagConstraint.Column
        {
            get { return _layoutInfo.Column; }
        }

        int IGridBagConstraint.ColumnSpan
        {
            get { return _layoutInfo.ColumnSpan; }
        }

        float IGridBagConstraint.ColumnWeight
        {
            get { return _fieldConstraint.ColumnWeight; }
        }

        HorizontalAlignment IGridBagConstraint.HorizontalAlignment
        {
            get { return _fieldConstraint.HorizontalAlignment; }
        }

        Thickness IGridBagConstraint.Margin
        {
            // AS 2/26/09 CellPresenter Chrome
            //get { return _fieldConstraint.Margin; }
            get { return _margin ?? _fieldConstraint.Margin; }
        }

        int IGridBagConstraint.Row
        {
            get { return _layoutInfo.Row; }
        }

        int IGridBagConstraint.RowSpan
        {
            get { return _layoutInfo.RowSpan; }
        }

        float IGridBagConstraint.RowWeight
        {
            get { return _fieldConstraint.RowWeight; }
        }

        VerticalAlignment IGridBagConstraint.VerticalAlignment
        {
            get { return _fieldConstraint.VerticalAlignment; }
        }

        #endregion //IGridBagConstraint Members
    } 
    #endregion //ItemLayoutInfoLayoutItem

	#region WrapperLayoutItem
	internal class WrapperLayoutItem : LayoutItem
		, IWrapperLayoutItem
	{
		#region Member Variables

		private LayoutItem _layoutItem;
		private double? _preferredWidth;
		private double? _preferredHeight;

		#endregion //Member Variables

		#region Constructor
		internal WrapperLayoutItem(LayoutItem layoutItem)
		{
			GridUtilities.ValidateNotNull(layoutItem);
			_layoutItem = layoutItem;
		}
		#endregion //Constructor

		#region Base class overrides

		#region PreferredWidth|Height
		// AS 7/29/09 NA 2009.2 Field Sizing
		// The base will handle the Get impl and the set was changed to a method.
		//
		//public override double PreferredHeight
		//{
		//    get
		//    {
		//        return this.GetSize(LayoutItemSize.PreferredHeight);
		//    }
		//    set
		//    {
		//        _preferredHeight = value;
		//    }
		//}

		//public override double PreferredWidth
		//{
		//    get
		//    {
		//        return this.GetSize(LayoutItemSize.PreferredWidth);
		//    }
		//    set
		//    {
		//        _preferredWidth = value;
		//    }
		//}
		#endregion //PreferredWidth|Height

		#region GetSize
		public override double GetSize(LayoutItemSize size)
		{
			switch (size)
			{
				case LayoutItemSize.PreferredWidth:
					if (null != _preferredWidth)
						return _preferredWidth.Value;
					break;
				case LayoutItemSize.PreferredHeight:
					if (null != _preferredHeight)
						return _preferredHeight.Value;
					break;
			}

			return _layoutItem.GetSize(size);
		}
		#endregion //GetSize

		#region GetVisibility
		protected override Visibility GetVisibility()
		{
			return ((ILayoutItem)_layoutItem).Visibility;
		}
		#endregion //GetVisibility

		// AS 7/30/09 NA 2009.2 Field Sizing
		#region SetPreferred
		public override void SetPreferredHeight(double value, ItemSizeType type)
		{
			_preferredHeight = value;
		}

		// AS 7/29/09 NA 2009.2 Field Sizing
		public override void SetPreferredWidth(double value, ItemSizeType type)
		{
			_preferredWidth = value;
		} 
		#endregion //SetPreferred

		#endregion //Base class overrides

		#region Properties
		public LayoutItem BaseItem
		{
			get { return _layoutItem; }
		}
		#endregion //Properties

		#region IWrapperLayoutItem Members

		ILayoutItem IWrapperLayoutItem.InnerLayoutItem
		{
			get { return _layoutItem; }
		}

		double? IWrapperLayoutItem.ExplicitWidth
		{
			get
			{
				return _preferredWidth;
			}
			set
			{
				if (!object.Equals(_preferredWidth, value))
				{
					_preferredWidth = value;
				}
			}
		}

		double? IWrapperLayoutItem.ExplicitHeight
		{
			get
			{
				return _preferredHeight;
			}
			set
			{
				if (!object.Equals(_preferredHeight, value))
				{
					_preferredHeight = value;
				}
			}
		}

		#endregion
	}
	#endregion //WrapperLayoutItem

	// ILayoutContainer:
    //
    #region CalcSizeLayoutContainer
    internal class CalcSizeLayoutContainer : ILayoutContainer
    {
        #region Member Variables

        internal static readonly ILayoutContainer Instance = new CalcSizeLayoutContainer();

        #endregion //Member Variables

        #region Constructor
        private CalcSizeLayoutContainer()
        {
        }
        #endregion //Constructor

        #region ILayoutContainer

        Rect ILayoutContainer.GetBounds(object containerContext)
        {
            Debug.Assert(containerContext is Rect);
            return (Rect)containerContext;
        }

        void ILayoutContainer.PositionItem(ILayoutItem item, Rect rect, object containerContext)
        {
        }

        #endregion //ILayoutContainer
    } 
    #endregion //CalcSizeLayoutContainer

    #region FieldRectsLayoutContainer
    internal class FieldRectsLayoutContainer : ILayoutContainer
    {
        #region Member Variables

        private Dictionary<ILayoutItem, Rect> _positions;
        private Vector _nearFixedFieldOffset;
        private Vector _scrollableFieldOffset;
        private Vector _farFixedFieldOffset;

        #endregion //Member Variables

        #region Constructor
        internal FieldRectsLayoutContainer()
        {
            this._positions = new Dictionary<ILayoutItem, Rect>();
        }
        #endregion //Constructor

        #region Properties
        internal Rect this[ILayoutItem item]
        {
            get
            {
                Rect r;
                if (false == this._positions.TryGetValue(item, out r))
                    r = new Rect();

                return r;
            }
            // AS 2/26/09 CellPresenter Chrome
            set
            {
                this._positions[item] = value;
            }
        } 
        #endregion //Properties

        #region Methods
        internal void Reset(Vector nearFixedFieldOffset, Vector scrollableFieldOffset, Vector farFixedFieldOffset)
        {
            this._positions.Clear();
            this._nearFixedFieldOffset = nearFixedFieldOffset;
            this._scrollableFieldOffset = scrollableFieldOffset;
            this._farFixedFieldOffset = farFixedFieldOffset;
        } 
        #endregion //Methods

        #region ILayoutContainer Members

        Rect ILayoutContainer.GetBounds(object containerContext)
        {
            return (Rect)containerContext;
        }

        void ILayoutContainer.PositionItem(ILayoutItem item, Rect rect, object containerContext)
        {
            FieldLayoutItemBase fieldItem = item as FieldLayoutItemBase;

            if (null != fieldItem)
            {
                switch (fieldItem.FixedLocation)
                {
                    case FixedFieldLocation.FixedToNearEdge:
                        rect.Offset(_nearFixedFieldOffset);
                        break;
                    case FixedFieldLocation.FixedToFarEdge:
                        rect.Offset(_farFixedFieldOffset);
                        break;
                    case FixedFieldLocation.Scrollable:
                        rect.Offset(_scrollableFieldOffset);
                        break;
                }

                this._positions[item] = rect;
            }
        }

        #endregion //ILayoutContainer
    } 
    #endregion //FieldRectsLayoutContainer

    // IGridBagConstraint:
    //
    #region GridBagConstraint
    internal class GridBagConstraint : IGridBagConstraint
    {
        #region Constructor
        public GridBagConstraint()
        {
            this.RowSpan = 1;
            this.ColumnSpan = 1;
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;
        } 
        #endregion //Constructor

        #region Properties

        public int Column { get; set; }
        public int ColumnSpan { get; set; }
        public float ColumnWeight { get; set; }
        public int Row { get; set; }
        public int RowSpan { get; set; }
        public float RowWeight { get; set; }
        public Thickness Margin { get; set; }
        public HorizontalAlignment HorizontalAlignment { get; set; }
        public VerticalAlignment VerticalAlignment { get; set; }

        #endregion //Properties

        #region IGridBagConstraint Members

        int IGridBagConstraint.Column
        {
            get { return this.Column; }
        }

        int IGridBagConstraint.ColumnSpan
        {
            get { return this.ColumnSpan; }
        }

        float IGridBagConstraint.ColumnWeight
        {
            get { return this.ColumnWeight; }
        }

        HorizontalAlignment IGridBagConstraint.HorizontalAlignment
        {
            get { return this.HorizontalAlignment; }
        }

        Thickness IGridBagConstraint.Margin
        {
            get { return this.Margin; }
        }

        int IGridBagConstraint.Row
        {
            get { return this.Row; }
        }

        int IGridBagConstraint.RowSpan
        {
            get { return this.RowSpan; }
        }

        float IGridBagConstraint.RowWeight
        {
            get { return this.RowWeight; }
        }

        VerticalAlignment IGridBagConstraint.VerticalAlignment
        {
            get { return this.VerticalAlignment; }
        }

        #endregion //IGridBagConstraint
    } 
    #endregion //GridBagConstraint

    // GridBagLayoutManager:
    //
    #region FieldGridBagLayoutManager
    internal class FieldGridBagLayoutManager : GridBagLayoutManager
    {
        #region Member Variables

        private FieldLayout _fieldLayout;
        private int _layoutItemVersion = -2;
        private int _layoutManagerVersion = -2;
        private LayoutManagerType _type;
        private FieldGridBagLayoutManager _sourceManager;
        private Dictionary<FieldLabelKey, FieldLayoutItemBase> _fieldItemMap;
        internal static readonly object InfiniteRect = new Rect(0, 0, double.PositiveInfinity, double.PositiveInfinity);
        private LayoutInfo _layoutInfo;

		// AS 10/13/09 NA 2010.1 - CardView
		private Record _record;
		private VirtualizingDataRecordCellPanel _cellPanel;

		// AS 11/29/10 TFS60418
		// Cache information to avoid having the GridBagLayoutManager do calculations that would 
		// be the same otherwise.
		//
		private Dictionary<Rect, GridBagLayoutItemDimensionsCollection> _cachedDimensions;
		internal int InvalidateLayoutCount = 1;
		private Size? _cachedPreferredSize;
		private Size? _cachedMinSize;
		private Size? _cachedMaxSize;
		private DataRecordSizingMode? _cachedSizingMode;

		#endregion //Member Variables

        #region Constructor
        internal FieldGridBagLayoutManager(FieldLayout fieldLayout, LayoutManagerType type)
        {
            GridUtilities.ValidateNotNull(fieldLayout);
            this._fieldLayout = fieldLayout;
            this._type = type;
            this._fieldItemMap = new Dictionary<FieldLabelKey, FieldLayoutItemBase>();

			// AS 11/29/10 TFS60418
			this._cachedDimensions = new Dictionary<Rect, GridBagLayoutItemDimensionsCollection>();
        }
        #endregion //Constructor

		#region Base class overrides
		
		#region InvalidateLayout
		public override void InvalidateLayout()
		{
			// AS 11/29/10 TFS60418
			InvalidateLayoutCount++;
			_cachedDimensions.Clear();
			_cachedMaxSize = _cachedMinSize = _cachedPreferredSize = null;
			_cachedSizingMode = null;

			base.InvalidateLayout();
		}
		#endregion //InvalidateLayout

		#endregion //Base class overrides

		#region Properties

		// AS 10/13/09 NA 2010.1 - CardView
		#region CellPanel
		internal VirtualizingDataRecordCellPanel CellPanel
		{
			get { return _cellPanel; }
		}
		#endregion //CellPanel

		#region FieldLayout
        internal FieldLayout FieldLayout
        {
            get { return this._fieldLayout; }
        }
        #endregion //FieldLayout 

		// AS 1/25/10 SizableSynchronized changes
		#region RecordSizingMode
		public DataRecordSizingMode RecordSizingMode
		{
			get
			{
				// AS 11/29/10 TFS60418
				// Moved to a helper method so we can calculate it elsewhere. Also since 
				// we use this often and the state doesn't change at least while the layout 
				// is valid, we can just cache the results.
				//
				//Record record = _record;
				//
				//if (record == null && _cellPanel != null)
				//{
				//    // AS 10/21/10 TFS26331
				//    if (_cellPanel.IsHeaderArea)
				//        return DataRecordSizingMode.SizedToContentAndFixed;
				//
				//    record = _cellPanel.Record;
				//}
				//
				//DataRecordSizingMode sizingMode = _fieldLayout.DataRecordSizingModeResolved;
				//
				//FilterRecord fr = record as FilterRecord;
				//
				//if (null != fr && sizingMode == DataRecordSizingMode.SizableSynchronized)
				//    return DataRecordSizingMode.IndividuallySizable;
				//
				//return sizingMode;
				if (_cachedSizingMode == null)
				{
					_cachedSizingMode = this.CalculateSizingMode();
				}

				return _cachedSizingMode.Value;
			}
		} 
		#endregion //RecordSizingMode

		// AS 10/13/09 NA 2010.1 - CardView
		#region Record
		internal Record Record
		{
			get
			{
				return _record;
			}
			set
			{
				Debug.Assert(_sourceManager != null, "This is meant for the lm of a record/vdrcp");
				_record = value;

				// AS 11/29/10 TFS60418
				_cachedSizingMode = null;
			}
		} 
		#endregion //Record

        #region Type
        internal LayoutManagerType Type
        {
            get { return this._type; }
        } 
        #endregion //Type

        #endregion //Properties

        #region Methods

        #region CalculateCellPresenterRects
        internal void CalculateCellPresenterRects(FieldRectsLayoutContainer lcRects, Size containerSize, Rect[] cellRects)
        {
            for (int i = 0; i < cellRects.Length; i++)
                cellRects[i] = new Rect();

            Rect emptyRect = new Rect();

            this.LayoutContainer(lcRects, new Rect(containerSize));

            // AS 2/26/09 CellPresenter Chrome
            TemplateDataRecordCache recordCache = _fieldLayout.TemplateDataRecordCache;

            #region Merge Cell & Label Rects for CellPresenter
            foreach (ILayoutItem item in this.LayoutItems)
            {
                FieldLayoutItemBase fli = item as FieldLayoutItemBase;
                Debug.Assert(null != fli);

                if (null != fli && ((ILayoutItem)fli).Visibility != Visibility.Collapsed)
                {
                    Rect itemRect = lcRects[fli];

                    IGridBagConstraint gc = (IGridBagConstraint)this.LayoutItems.GetConstraint(fli);

                    // AS 2/26/09 CellPresenter Chrome
                    // When using CellPresenters, we have a layout item for the CVP and another for 
                    // the label. Those elements will ultimately be within the CellPresenterLayoutElement
                    // of the CellPresenter element that the VDRCP will be creating/positioning. Therefore 
                    // we had to have the layout item for the label & cell include the portion of the 
                    // chrome around the CellPresenterLayoutElement so the CP would be the right size.
                    // This loop however is meant to store the actual position of the CVP/Label with 
                    // respect to the CP so for the cellRects we need to remove the margin of the item.
                    // And we also need to store a rect relative to the CP back in the FieldRectsLayoutContainer
                    //

                    // the rects we store should be where the element will be within
                    // the layout element so we need to adjust for the margins around
                    // the cellpresenterlayoutelement around the cvp/label
                    Thickness cpMargin = recordCache.GetCellPresenterMargin(fli.Field);
                    Rect adjustedRect = itemRect;
                    adjustedRect.Offset(-cpMargin.Left, -cpMargin.Top);
                    lcRects[fli] = adjustedRect;

                    // AS 2/26/09 CellPresenter Chrome
                    // remove the margin since we are trying to calculate
                    // the size/rect of the cell presenter and the item 
                    // represents the cvp/lable within the cell presenter
                    // 
                    Thickness margin = gc.Margin;
                    itemRect.Offset(-margin.Left, -margin.Top);
					// AS 6/15/12 TFS112011
                    //itemRect.Width += margin.Left + margin.Right;
                    //itemRect.Height += margin.Top + margin.Bottom;
					itemRect.Width = Math.Max(itemRect.Width + margin.Left + margin.Right, 0);
					itemRect.Height = Math.Max(itemRect.Height + margin.Top + margin.Bottom, 0);

                    if (!itemRect.Equals(emptyRect))
                    {
                        int cellIndex = fli.Field.TemplateCellIndex;
                        Rect currentRect = cellRects[cellIndex];

                        // if we don't have one for this cell/label then use the
                        // specified one
                        if (currentRect.Equals(emptyRect))
                            currentRect = itemRect;
                        else // otherwise union them together
                        {
                            // AS 3/24/09 Optimization
                            //currentRect.Union(itemRect);
                            currentRect = GridUtilities.Union(ref currentRect, ref itemRect);
                        }

                        cellRects[cellIndex] = currentRect;
                    }
                }
            }
            #endregion //Merge Cell & Label Rects for CellPresenter
        } 
        #endregion //CalculateCellPresenterRects

        #region Calculate(Max|Min|Pref)Size
        internal Size CalculateMaximumSize()
        {
			// AS 11/29/10 TFS60418
			//return CalculateMaximumSize(CalcSizeLayoutContainer.Instance, InfiniteRect);
			if (_cachedMaxSize == null)
				_cachedMaxSize = CalculateMaximumSize(CalcSizeLayoutContainer.Instance, InfiniteRect);

			return _cachedMaxSize.Value;
        }

        internal Size CalculateMinimumSize()
        {
			// AS 11/29/10 TFS60418
			//return CalculateMinimumSize(CalcSizeLayoutContainer.Instance, InfiniteRect);
			if (_cachedMinSize == null)
				_cachedMinSize = CalculateMinimumSize(CalcSizeLayoutContainer.Instance, InfiniteRect);

			return _cachedMinSize.Value;
		}

        internal Size CalculatePreferredSize()
        {
			// AS 11/29/10 TFS60418
			//return CalculatePreferredSize(CalcSizeLayoutContainer.Instance, InfiniteRect);
			if (_cachedPreferredSize == null)
				_cachedPreferredSize = CalculatePreferredSize(CalcSizeLayoutContainer.Instance, InfiniteRect);

			return _cachedPreferredSize.Value;
		} 
        #endregion //Calculate(Max|Min|Pref)Size

		// AS 11/29/10 TFS60418
		#region CalculateSizingMode
		internal DataRecordSizingMode CalculateSizingMode()
		{
			Record record = _record;

			if (record == null && _cellPanel != null)
				record = _cellPanel.Record;

			return CalculateSizingMode(record, _fieldLayout, _cellPanel != null ? _cellPanel.IsHeaderArea : _type == LayoutManagerType.Header);
		}

		internal static DataRecordSizingMode CalculateSizingMode(Record record, FieldLayout fieldLayout, bool isHeaderArea)
		{
			// AS 11/29/10 TFS60418
			// We may end up asking for this on the label's layout manager which 
			// won't have a context of a record or panel.
			//
			if (isHeaderArea)
				return DataRecordSizingMode.SizedToContentAndFixed;

			DataRecordSizingMode sizingMode = fieldLayout.DataRecordSizingModeResolved;

			FilterRecord fr = record as FilterRecord;

			if (null != fr && sizingMode == DataRecordSizingMode.SizableSynchronized)
				return DataRecordSizingMode.IndividuallySizable;

			return sizingMode;
		}
		#endregion //CalculateSizingMode

		#region Clone
		internal FieldGridBagLayoutManager Clone()
        {
            return Clone(null);
        }

        internal FieldGridBagLayoutManager Clone(LayoutInfo layoutInfo)
        {
            // we need a layout manager whose layout items know how to get the preferred 
            // size from our local elements for non-virtualized fields and delegate to
            // the field's layoutitems for the rest
            FieldGridBagLayoutManager newLm = GridUtilities.CreateGridBagLayoutManager(this._fieldLayout, this._type);

            // hold a reference to the original so we can refresh when the 
            // layout manager updates
            newLm._sourceManager = this;

            // AS 2/2/09
            // Added a LayoutInfo member that can be used to override the constraints that 
            // will be used for the layout items instead of the field's LayoutConstraint.
            //
            newLm._layoutInfo = layoutInfo;

            return newLm;
        } 
        #endregion //Clone

		// AS 6/29/09 NA 2009.2 Field Sizing
 		#region ConvertLayoutItems
		internal static IList<ILayoutItem> ConvertLayoutItems(IList<FieldLayoutItemBase> items)
		{
			if (items == null)
				return null;

			ILayoutItem[] newList = new ILayoutItem[items.Count];

			for (int i = 0, count = items.Count; i < count; i++)
				newList[i] = items[i];

			return newList;
		} 
		#endregion //ConvertLayoutItems

		#region CreateLayoutInfo
        // AS 2/2/09
        // When calculating the layout that will result from a splitter drag operation
        // we need to get a LayoutInfo object since it has the logic for calculating
        // the new origins when the fixed location of fields have been changed. If 
        // we don't have a _dragFieldLayoutInfo on the fieldlayout then we need to 
        // generate one based on the current layout since we don't cache the original
        // autogenerated layout.
        //
        /// <summary>
        /// Creates a <see cref="LayoutInfo"/> based on the items and constraints.
        /// </summary>
        /// <param name="useFieldConstraint">True to use the Field's LayoutConstraint which will 
        /// contain the actual grid positions and not the adjusted ones that would be used if 
        /// cell presenters are being used.</param>
        internal LayoutInfo CreateLayoutInfo(bool useFieldConstraint)
        {
            LayoutInfo li = new LayoutInfo(_fieldLayout);

            foreach (ILayoutItem item in this.LayoutItems)
            {
                FieldLayoutItemBase fieldItem = item as FieldLayoutItemBase;

                if (null != fieldItem)
                {
                    Field field = fieldItem.Field;
                    ItemLayoutInfo itemInfo;

                    IGridBagConstraint gcField = useFieldConstraint
                        ? field.LayoutConstraint
                        : this.LayoutItems.GetConstraint(fieldItem) as IGridBagConstraint;

                    if (!li.TryGetValue(field, out itemInfo))
                    {
                        itemInfo = new ItemLayoutInfo(gcField.Column, gcField.Row, gcField.ColumnSpan, gcField.RowSpan);
                        itemInfo._fixedLocation = fieldItem.FixedLocation;
						// SSP 8/24/09 - NAS9.2 Field chooser - TFS19140
						// 
                        //itemInfo._isCollapsed = field.IsVisibleInCellArea == false;
						itemInfo.Visibility = field.VisibilityInCellArea;

                        li[field] = itemInfo;
                    }
                    else if (!useFieldConstraint)
                    {
                        // if we have an item for the field then this must be a cell presenter 
                        // situation in which case we need to aggregate the row/column info
                        int newCol = Math.Min(itemInfo.Column, gcField.Column);
                        itemInfo.ColumnSpan = Math.Max(itemInfo.Column + itemInfo.ColumnSpan, gcField.Column + gcField.ColumnSpan) - newCol;
                        itemInfo.Column = newCol;

                        int newRow = Math.Min(itemInfo.Row, gcField.Row);
                        itemInfo.RowSpan = Math.Max(itemInfo.Row + itemInfo.RowSpan, gcField.Row + gcField.RowSpan) - newRow;
                        itemInfo.Row = newRow;
                    }
                }
            }

            return li;
        }
        #endregion //CreateLayoutInfo

		// AS 5/29/09 NA 2009.2 Undo/Redo
		#region CreateUndoSnapshot
		internal void CreateUndoSnapshot(bool resizeInXAxis, Record targetRecord)
		{
			bool isHorizontal = _fieldLayout.IsHorizontal;
			DataPresenterBase dp = _fieldLayout.DataPresenter;

			Debug.Assert(null != dp);

			if (dp == null || !dp.IsUndoEnabled)
				return;

			if (targetRecord == null)
			{
				FieldResizeInfoAction action = FieldResizeInfoAction.Create(_fieldLayout, resizeInXAxis);

				if (null != action)
					dp.History.AddUndoActionInternal(action);
			}
			else
			{
				Debug.Assert(resizeInXAxis == isHorizontal, "Expected logical row height changes only");

				ResizeRecordAction action = ResizeRecordAction.Create(targetRecord, resizeInXAxis);

				if (null != action)
					dp.History.AddUndoActionInternal(action);
			}
		}
		#endregion //CreateUndoSnapshot

		#region GetPreferredExtent
        internal double GetPreferredExtent(int origin, int span, bool column)
        {
            if (this._fieldLayout.UseCellPresenters)
            {
                origin *= 2;
                span *= 2;
            }

            Size preferredSize = this.CalculatePreferredSize();
			// AS 11/29/10 TFS60418
			// Changed to use the Cached version.
			//
			GridBagLayoutItemDimensionsCollection dims = GetLayoutItemDimensionsCached(CalcSizeLayoutContainer.Instance, new Rect(preferredSize));
            
            double[] extents = column ? dims.ColumnDims : dims.RowDims;
            Debug.Assert(origin >= 0 && origin < extents.Length);
            Debug.Assert(span > 0 && (origin + span) <= extents.Length);

            return extents[origin + span] - extents[origin];
        }
        #endregion //GetPreferredExtent

        #region GetLayoutItem
        internal FieldLayoutItemBase GetLayoutItem(Field field, bool isLabel)
        {
            FieldLayoutItemBase item = null;
            FieldLabelKey key = new FieldLabelKey(field, isLabel);

            this._fieldItemMap.TryGetValue(key, out item);

            return item;
        } 
        #endregion //GetLayoutItem

		// AS 11/29/10 TFS60418
		// There is some overhead with asking the grid bag to calculate the dimensions
		// for a given layout so if the layout has not been invalidated and the same 
		// rect is being provided we can use the last set of dimensions.
		//
		#region GetLayoutItemDimensionsCached
		internal GridBagLayoutItemDimensionsCollection GetLayoutItemDimensionsCached(ILayoutContainer layoutContainer, object containerContext)
		{
			var rect = layoutContainer.GetBounds(containerContext);
			GridBagLayoutItemDimensionsCollection dims;

			if (_cachedDimensions.TryGetValue(rect, out dims))
				return dims;

			dims = base.GetLayoutItemDimensions(layoutContainer, containerContext);
			_cachedDimensions[rect] = dims;

			return dims;
		} 
		#endregion //GetLayoutItemDimensionsCached

        #region Initialize
        private void Initialize()
        {
            bool useCellPresenters = this._fieldLayout.UseCellPresenters;
            this.LayoutItems.Clear();
            bool isLabel = this._type == LayoutManagerType.Header;

            for (int i = 0, count = this._fieldLayout.Fields.Count; i < count; i++)
            {
                Field field = this._fieldLayout.Fields[i];

                if (field.IsInLayout == false)
                    continue;

                if (isLabel || useCellPresenters)
                    this.LayoutItems.Add(field.LabelLayoutItem, field.LabelLayoutConstraint);

                if (!isLabel)
                    this.LayoutItems.Add(field.CellLayoutItem, field.CellLayoutConstraint);
            }

            this.InitializeItemMap();

            foreach (GridDefinitionLayoutItem colDef in this._fieldLayout.TemplateDataRecordCache.GridColumnLayoutItems)
                this.LayoutItems.Add(colDef, colDef);

            foreach (GridDefinitionLayoutItem rowDef in this._fieldLayout.TemplateDataRecordCache.GridRowLayoutItems)
                this.LayoutItems.Add(rowDef, rowDef);
        } 
        #endregion //Initialize

        #region InitializeFrom
        private void InitializeFrom(FieldGridBagLayoutManager sourceManager)
        {
            // make sure the source is up to date before copying its items
            sourceManager.VerifyLayout();

            // try to reuse the olditems so that if a field is moved we 
            // keep the record height of a sized record
            Dictionary<ILayoutItem, ILayoutItem> _oldItems = new Dictionary<ILayoutItem, ILayoutItem>();

			// AS 2/24/10 TFS28409
			// Since we recycle the layout items, we may need to reset/clear the 
			// explicit sizing if the new mode doesn't support it.
			//
			bool isResizable = false;

			switch (this.RecordSizingMode)
			{
				case DataRecordSizingMode.IndividuallySizable:
				case DataRecordSizingMode.SizedToContentAndIndividuallySizable:
					isResizable = true;
					break;
			}

            for (int i = 0, count = this.LayoutItems.Count; i < count; i++)
            {
				// AS 6/1/09
				// Since we have multiple types of wrapper layout items I created
				// an interface we can use to get to the underlying layuot item.
				//
				//CellLayoutItem cellItem = this.LayoutItems[i] as CellLayoutItem;
				//
				//if (cellItem != null)
				//{
				//    _oldItems[cellItem.InnerLayoutItem] = cellItem;
				//
				//    // clear the forced unfixed state if there is one
				//    cellItem.ForceUnfixed = false;
				//}
				IWrapperLayoutItem wrapperItem = this.LayoutItems[i] as IWrapperLayoutItem;

				if (null != wrapperItem)
				{
					_oldItems[wrapperItem.InnerLayoutItem] = this.LayoutItems[i];

					CellLayoutItem cellItem = wrapperItem as CellLayoutItem;

					if (null != cellItem)
						cellItem.ForceUnfixed = false;

					// AS 2/24/10 TFS28409
					if (!isResizable)
					{
						wrapperItem.ExplicitWidth = null;
						wrapperItem.ExplicitHeight = null;
					}
				}
			}

            this.LayoutItems.Clear();

            for (int i = 0, count = sourceManager.LayoutItems.Count; i < count; i++)
            {
                ILayoutItem sourceItem = sourceManager.LayoutItems[i];
                IGridBagConstraint gc = sourceManager.LayoutItems.GetConstraint(sourceItem) as IGridBagConstraint;

				
#region Infragistics Source Cleanup (Region)















































#endregion // Infragistics Source Cleanup (Region)

				ILayoutItem destItem;

				// if we didn't have a wrapper for the item, then create one now
				if (false == _oldItems.TryGetValue(sourceItem, out destItem))
				{
					FieldLayoutItemBase sourceFieldItem = sourceItem as FieldLayoutItemBase;

					if (null != sourceFieldItem)
					{
						ItemLayoutInfo layoutItemInfo = null;

						if (null != _layoutInfo)
						{
							layoutItemInfo = _layoutInfo[sourceFieldItem.Field];
							Debug.Assert(null != layoutItemInfo);
						}

						// AS 10/13/09 NA 2010.1 - CardView
						// Store a reference to the owning manager on the celllayoutitem.
						//
						//if (null == layoutItemInfo)
						//    destItem = new CellLayoutItem(sourceFieldItem);
						//else
						//    destItem = new ItemLayoutInfoLayoutItem(sourceFieldItem, gc, layoutItemInfo);
						CellLayoutItem cellItem;

						if (null == layoutItemInfo)
							cellItem = new CellLayoutItem(sourceFieldItem);
						else
							cellItem = new ItemLayoutInfoLayoutItem(sourceFieldItem, gc, layoutItemInfo);

						cellItem.LayoutManager = this;
						destItem = cellItem;
					}
					else
					{
						Debug.Assert(sourceItem is GridDefinitionLayoutItem);
						// AS 5/29/09
						// We need to wrap the item in a layout item in case its preferred width/height be changed.
						//
						//destItem = sourceItem;
						if (sourceItem is LayoutItem)
							destItem = new WrapperLayoutItem((LayoutItem)sourceItem);
						else
							destItem = sourceItem;
					}
				}

				// if the item is a gridbag constraint then we will use that to 
				// provide the constraint information
				if (destItem is IGridBagConstraint)
					gc = (IGridBagConstraint)destItem;

                this.LayoutItems.Add(destItem, gc);
            }

            this.InitializeItemMap();
        }

        #endregion //InitializeFrom

        #region InitializeItemMap
        private void InitializeItemMap()
        {
            this._fieldItemMap.Clear();

            for (int i = 0, count = this.LayoutItems.Count; i < count; i++)
            {
                FieldLayoutItemBase fieldItem = this.LayoutItems[i] as FieldLayoutItemBase;

                if (null != fieldItem)
                    this._fieldItemMap.Add(new FieldLabelKey(fieldItem), fieldItem);
            }
        }
        #endregion //InitializeItemMap

        #region InitializePanelReference
        internal void InitializePanelReference(VirtualizingDataRecordCellPanel owningPanel)
        {
			// AS 10/13/09 NA 2010.1 - CardView
			// Rather than spend the time iterating the collection and 
			// setting a refernce on each item each item now has a reference
			// to this layoutmanager (which doesn't change) so we just need to 
			// store the reference on the layout manager and have the item come 
			// back to it if it needs the panel.
			//
			//foreach (ILayoutItem item in this.LayoutItems)
			//{
			//    ICellPanelLayoutItem cellItem = item as ICellPanelLayoutItem;
			//
			//    if (null != cellItem)
			//        cellItem.CurrentPanel = owningPanel;
			//}
			_cellPanel = owningPanel;

			// AS 11/29/10 TFS60418
			_cachedSizingMode = null;
        }
        #endregion //InitializePanelReference

		// AS 6/29/09 NA 2009.2 Field Sizing
		#region ResizeItems
		internal bool ResizeItems(Dictionary<ILayoutItem, Size> newExtents, bool resizeInXAxis, Record targetRecord, bool addToUndo, ItemSizeType sizeType, Dictionary<ILayoutItem, bool> autoSizeStates)
		{
			if (newExtents == null || newExtents.Count == 0)
				return false;

			//  e.g. if View==Vert & resizeInXAxis & (!UseCellPresenters || Above/Below) OR
			//          View==Horz & !resizeInXAxis & (!UseCellPresenters || Left/Right)
			FieldLayoutItemBase.DebugSizeInfo("", "Resize Extents Start");



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


			// AS 5/29/09 NA 2009.2 Undo/Redo
			if (addToUndo)
			{
				// if a resize operation has occurred snapshot the layout info
				// before the resize is performed.
				if (newExtents != null && newExtents.Count > 0)
				{
					// note we cannot rely on the actual new extents handed
					// back to determine what will change since some items 
					// may not have been changed in the first move when 
					// dragging is immediate but may be changed subsequent
					this.CreateUndoSnapshot(resizeInXAxis, targetRecord);
				}
			}

			foreach (KeyValuePair<ILayoutItem, Size> item in newExtents)
			{
				
#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)

				LayoutItem actualItem = item.Key as LayoutItem;






				if (null != actualItem)
				{
					ItemSizeType itemSizeType = sizeType;
					bool isAutoSizeItem;

					if (!autoSizeStates.TryGetValue(actualItem, out isAutoSizeItem))
					{
						// if we're not told by the layout then we should keep the existing size type
						itemSizeType = resizeInXAxis ? actualItem.SizeTypeWidth : actualItem.SizeTypeHeight;
					}
					else
					{
						if (!isAutoSizeItem)
							itemSizeType = ItemSizeType.Explicit;
						else if (itemSizeType == ItemSizeType.ExplicitAutoSize)
						{
							FieldLayoutItemBase fieldItem = actualItem as FieldLayoutItemBase;
							Debug.Assert(null != fieldItem);

							// if this is an explicit autosize (e.g. the user double clicks on the right 
							// edge of the field) and the field has a width of auto, then we want it to 
							// return to auto mode. therefore a subsequent change to the width property 
							// should take precedence
							if (null != fieldItem && fieldItem.Field.GetWidthOrHeight(resizeInXAxis).IsAnyAuto)
								itemSizeType = ItemSizeType.AutoMode;
						}
					}

					if (resizeInXAxis)
					{
						actualItem.SetPreferredWidth(item.Value.Width, itemSizeType);
					}
					else
					{
						actualItem.SetPreferredHeight(item.Value.Height, itemSizeType);
					}
				}
			}

			FieldLayoutItemBase.DebugSizeInfo("", "Resize Extents End");

			return true;
		}
		#endregion //ResizeItems

		// AS 6/15/09 NA 2009.2 Field Sizing
		#region ShouldPreventResizeField
		internal bool ShouldPreventResizeField(Field field, bool resizeLabel, bool resizeInXAxis)
		{
			if (null != _sourceManager)
				return _sourceManager.ShouldPreventResizeField(field, resizeLabel, resizeInXAxis);
			else
			{
				// record resizing is not being tested here...
				if (resizeInXAxis == _fieldLayout.IsHorizontal)
					return false;

				float[] weights = resizeInXAxis ? this.ColumnWeights : this.RowWeights;
				Debug.Assert(null != weights);

				if (weights == null)
					return false;

				int weightColumn = -1;

				for(int i = 0; i < weights.Length; i++)
				{
					if (weights[i] > 0f)
					{
						// if there are multiple columns with weights we can resize
						if (weightColumn >= 0)
							return false;

						weightColumn = i;
					}
				}

				// if there are no weights we can resize any
				if (weightColumn < 0)
					return false;

				FieldLayoutItemBase layoutItem = this.GetLayoutItem(field, resizeLabel);
				Debug.Assert(null != layoutItem);

				if (null != layoutItem)
				{
					IGridBagConstraint gc = this.LayoutItems.GetConstraint(layoutItem) as IGridBagConstraint;
					int lastCol = (resizeInXAxis ? gc.Column + gc.ColumnSpan : gc.Row + gc.RowSpan) - 1;

					return lastCol == weightColumn;
				}
			}

			return false;
		}
		#endregion //ShouldPreventResizeField

		// AS 6/9/09 NA 2009.2 Field Sizing
		#region UpdateWeights
		/// <summary>
		/// Helper method to reset the weights of the constraint objects based on the values of the fields.
		/// </summary>
		private void UpdateWeights()
		{
			bool isHorz = _fieldLayout.IsHorizontal;

			// AS 10/9/09 NA 2010.1 - CardView
			// Pass autofit mode to GetAutoFitWeight.
			//
			AutoFitMode autoFitMode = _fieldLayout.AutoFitModeResolved;

			foreach (Field fl in _fieldLayout.Fields)
			{
				UpdateWeights(fl,
					fl.GetAutoFitWeight(false, true, autoFitMode),
					fl.GetAutoFitWeight(false, false, autoFitMode),
					fl.GetAutoFitWeight(true, true, autoFitMode),
					fl.GetAutoFitWeight(true, false, autoFitMode));
			}

			if (autoFitMode == AutoFitMode.ExtendLastField)
			{
				LayoutItemsCollection items = this.LayoutItems;
				int maxOrigin = -1;
				FieldLayoutItemBase lastFieldItem = null;
				bool hasWeight = false;

				foreach (ILayoutItem item in items)
				{
					if (item.Visibility == Visibility.Collapsed)
						continue;

					IGridBagConstraint gc = items.GetConstraint(item) as IGridBagConstraint;

					float weight = !isHorz ? gc.ColumnWeight : gc.RowWeight;

					if (weight > 0f)
					{
						hasWeight = true;
						break;
					}

					// the last field could span multiple logical columns and 
					// so we could have a definition as the layout item.
					if (item is GridDefinitionLayoutItem)
						continue;

					Debug.Assert(item is FieldLayoutItemBase);

					int origin = !isHorz ? gc.Column : gc.Row;

					if (origin > maxOrigin)
					{
						maxOrigin = origin;
						lastFieldItem = item as FieldLayoutItemBase;
					}
				}

				// if no item has a weight in the column orientation then 
				// we can provide a default to the last one
				if (!hasWeight && null != lastFieldItem)
				{
					Field f = lastFieldItem.Field;

					UpdateWeights(f,
						isHorz ? f.GetAutoFitWeight(false, true) : 1,
						isHorz ? 1 : f.GetAutoFitWeight(false, false),
						isHorz ? f.GetAutoFitWeight(true, true) : 1,
						isHorz ? 1 : f.GetAutoFitWeight(true, false));
				}
			}
		}

		private static void UpdateWeights(Field f, float labelWeightX, float labelWeightY, float cellWeightX, float cellWeightY)
		{
			UpdateWeights(f.LayoutConstraint, Math.Max(labelWeightX, cellWeightX), Math.Max(labelWeightY, cellWeightY));
			UpdateWeights(f.CellLayoutItem.CellPresenterConstraint, cellWeightX, cellWeightY);
			UpdateWeights(f.LabelLayoutItem.CellPresenterConstraint, labelWeightX, labelWeightY);
		}

		private static void UpdateWeights(GridBagConstraint gc, float x, float y)
		{
			Debug.Assert(!float.IsNaN(x) && !float.IsNaN(y));
			gc.ColumnWeight = x;
			gc.RowWeight = y;
		}
		#endregion //UpdateWeights

        #region VerifyLayout
        internal void VerifyLayout()
        {
            if (this._layoutManagerVersion != this._fieldLayout.LayoutManagerVersion)
            {
                if (null != this._sourceManager)
                {
                    this.InitializeFrom(this._sourceManager);
                }
                else
                {
                    Debug.Assert(null == _layoutInfo, "The _layoutInfo is only used within the InitializeFrom. We're not expecting to have a layoutinfo for the base layoutmanager");

                    this.Initialize();
                }

                this._layoutManagerVersion = this._fieldLayout.LayoutManagerVersion;

				// AS 6/9/09 NA 2009.2 Field Sizing
				if (null == _sourceManager)
					this.UpdateWeights();

                this.InvalidateLayout();
            }

			// AS 11/29/10 TFS60418
			// The LayoutItemVersion of the field layout may not get bumped
			// because we don't want to dirty the cache of all fields so we'll 
			// use the GridColumnWidthVersion instead.
			//
			//if (this._layoutItemVersion != this._fieldLayout.LayoutItemVersion)
			if (this._layoutItemVersion != _fieldLayout.GridColumnWidthVersion)
            {
				// AS 11/29/10 TFS60418
				//this._layoutItemVersion = this._fieldLayout.LayoutItemVersion;
				_layoutItemVersion = _fieldLayout.GridColumnWidthVersion;
				
                this.InvalidateLayout();
            }
        }

        #endregion //VerifyLayout

        #endregion //Methods

        #region FieldLabelKey struct
        private struct FieldLabelKey : IEquatable<FieldLabelKey>
        {
            internal Field Field;
            internal bool IsLabel;

            internal FieldLabelKey(Field field, bool isLabel)
            {
                Field = field;
                IsLabel = isLabel;
            }

            internal FieldLabelKey(FieldLayoutItemBase layoutItem)
                : this(layoutItem.Field, layoutItem.IsLabel)
            {
            }

            public override bool Equals(object obj)
            {
                return obj is FieldLabelKey
                    ? this.Equals((FieldLabelKey)obj)
                    : false;
            }

            public override int GetHashCode()
            {
                return this.Field.GetHashCode();
            }

            #region IEquatable<FieldLabelKey> Members

            public bool Equals(FieldLabelKey other)
            {
                return other.Field == this.Field &&
                    other.IsLabel == this.IsLabel;
            }

            #endregion
        } 
        #endregion //FieldLabelKey struct
	} 
    #endregion //FieldGridBagLayoutManager

    #region CPGridBagLayoutManager
    /// <summary>
    /// Layout manager used by a CellPresenter for measuring/arranging its Label/Cell.
    /// </summary>
    internal class CPGridBagLayoutManager : GridBagLayoutManager, ILayoutContainer
    {
        #region Member Variables

        private CellPresenterLayoutElementBase _cp;
        private CPLayoutItem _label;
        private CPLayoutItem _cell;
        private Rect _cellRect;
        private Rect _labelRect;
        private Field _field;
		// JJD 08/16/10 - TFS26331 - added
		private Size _constrainedSize;

        #endregion //Member Variables

        #region Constructor

		// JJD 08/16/10 - TFS26331 - added constrainedSize
        
        internal CPGridBagLayoutManager(Field field, Size constrainedSize)
        {
            GridUtilities.ValidateNotNull(field);
            this._field = field;
			this._constrainedSize = constrainedSize; // JJD 08/16/10 - TFS26331 - added constrainedSize
            this._label = new CPLayoutItem(this, true);
            this._cell = new CPLayoutItem(this, false);
        }
        #endregion //Constructor

        #region Properties
        internal Field Field
        {
            get { return this._field; }
        }

		// JJD 08/16/10 - TFS26331 - added
		internal Size ConstrainedSize
		{
			get { return this._constrainedSize; }
			set
			{
				if (!GridUtilities.AreClose(this._constrainedSize.Width, value.Width) ||
					 !GridUtilities.AreClose(this._constrainedSize.Height, value.Height))
				{
					this._constrainedSize = value;
					this.InvalidateLayout();
				}
			}
		}

        internal Rect CellRect
        {
            get { return this._cellRect; }
        }

        internal Rect LabelRect
        {
            get { return this._labelRect; }
        }
        #endregion //Properties

        #region Methods

        #region Initialize
        internal void Initialize(CellPresenterLayoutElementBase cp, Rect availableRect, Rect preferredCellRect, Rect preferredLabelRect)
        {
            this._cp = cp;

            this.LayoutItems.Clear();

            if (null != cp)
            {
                _label.PreferredRect = preferredLabelRect;
                _cell.PreferredRect = preferredCellRect;

                // AS 2/26/09 CellPresenter Chrome
                // Since the Label & Cell layout item's GridBagConstraint returns 
                // a margin for the space within the CellPresenter but around the 
                // CellPresenterLayoutElement, we need to have these custom layout 
                // items implement IGridBagConstraint and return an empty margin
                // since this manager is used to calculate the size/layout of the 
                // CVP & LabelPresenter within the CellPresenterLayoutElement.
                //
                //this.LayoutItems.Add(this._label, this.Field.LabelLayoutItem);
                //this.LayoutItems.Add(this._cell, this.Field.CellLayoutItem);
                this.LayoutItems.Add(this._label, this._label);
                this.LayoutItems.Add(this._cell, this._cell);

				// AS 10/12/09
				// Since the FieldGridBagLayoutManager manipulates the weights for an autofit
				// we should make sure the layout is up to date before using the constraints in 
				// the CPLayoutItem.
				//
				if (null != _field.Owner)
					_field.Owner.CellLayoutManager.VerifyLayout();

				Size preferredSize = this.CalculatePreferredSize(CalcSizeLayoutContainer.Instance, availableRect);

                // we want the arranged rects to have valid coordinates so 
                // if we were given infinite width and/or height then use
                // the preferred size for that extent
                if (double.IsPositiveInfinity(availableRect.Width))
                    availableRect.Width = preferredSize.Width;

                if (double.IsPositiveInfinity(availableRect.Height))
                    availableRect.Height = preferredSize.Height;

                this.LayoutContainer(this, availableRect);
            }
            else
            {
                this._cellRect = this._labelRect = new Rect();
            }
        }
        #endregion //Initialize

        #endregion //Methods

        #region ILayoutContainer

        Rect ILayoutContainer.GetBounds(object containerContext)
        {
            return (Rect)containerContext;
        }

        void ILayoutContainer.PositionItem(ILayoutItem item, Rect rect, object containerContext)
        {
            if (item == this._cell)
                this._cellRect = rect;
            else
            {
                Debug.Assert(item == this._label);
                this._labelRect = rect;
            }
        }

        #endregion //ILayoutContainer

        #region CPLayoutItem
        private class CPLayoutItem : ILayoutItem,
            // AS 2/26/09 CellPresenter Chrome
            // See the Initialize method of the CPGridBagLayoutManager for details.
            //
            IGridBagConstraint
        {
            #region Member Variables

            private CPGridBagLayoutManager _owner;
            private bool _isLabel;
            private Rect _preferredRect;

            #endregion //Member Variables

            #region Constructor
            internal CPLayoutItem(CPGridBagLayoutManager owner, bool isLabel)
            {
                this._owner = owner;
                this._isLabel = isLabel;
            }
            #endregion //Constructor

            #region Properties
            // AS 2/26/09 CellPresenter Chrome
            private IGridBagConstraint BaseConstraint
            {
                get
                {
                    Field field = this._owner.Field;

                    return this._isLabel
                        ? field.LabelLayoutItem
                        : field.CellLayoutItem;
                }
            }

            private ILayoutItem BaseLayoutItem
            {
                get
                {
                    Field field = this._owner.Field;

                    return this._isLabel
                        ? field.LabelLayoutItem
                        : field.CellLayoutItem;
                }
            }

            private FrameworkElement Element
            {
                get
                {
                    if (this._owner._cp == null)
                        return null;

                    return _isLabel
                        ? _owner._cp.LabelPresenter
                        : _owner._cp.CellValuePresenter;
                }
            }

            internal Rect PreferredRect
            {
                get { return _preferredRect; }
                set { _preferredRect = value; }
            }
            #endregion //Properties

            #region ILayoutItem

            Size ILayoutItem.MaximumSize
            {
                get 
                { 
                    return this.BaseLayoutItem.MaximumSize; 
                }
            }

            Size ILayoutItem.MinimumSize
            {
                get { return this.BaseLayoutItem.MinimumSize; }
            }

            Size ILayoutItem.PreferredSize
            {
                get
                {
                    if (_preferredRect.IsEmpty == false)
                        return _preferredRect.Size;

                    FrameworkElement fe = this.Element;

                    if (null == fe)
                        return new Size();

					// AS 10/20/10 TFS26331
					// In addition to Joe's changes, when the sizing mode is synchronized 
					// or fixed then we use the size of the shared grid bag in which case 
					// we need to get the constraint for the label (if one has been set).
					//
					// JJD 08/16/10 - TFS26331
					// Use constrained size for the measure
					//fe.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
					Size constraint = this._owner.ConstrainedSize;

					if (_isLabel)
					{
						if (_owner._cp.Record is TemplateDataRecord)
						{
							double width = _owner._field.GetLabelWidthResolvedHelper(false, false);

							if (!double.IsNaN(width))
								constraint.Width = width;
						}
					}

                    fe.Measure(constraint);
                    return fe.DesiredSize;
                }
            }

            Visibility ILayoutItem.Visibility
            {
                get { return this.BaseLayoutItem.Visibility; }
            }

            #endregion //ILayoutItem

            // AS 2/26/09 CellPresenter Chrome
            #region IGridBagConstraint

            int IGridBagConstraint.Column
            {
                get { return this.BaseConstraint.Column; }
            }

            int IGridBagConstraint.ColumnSpan
            {
                get { return this.BaseConstraint.ColumnSpan; }
            }

            float IGridBagConstraint.ColumnWeight
            {
                get { return this.BaseConstraint.ColumnWeight; }
            }

            HorizontalAlignment IGridBagConstraint.HorizontalAlignment
            {
                get { return this.BaseConstraint.HorizontalAlignment; }
            }

            Thickness IGridBagConstraint.Margin
            {
                get { return new Thickness(); }
            }

            int IGridBagConstraint.Row
            {
                get { return this.BaseConstraint.Row; }
            }

            int IGridBagConstraint.RowSpan
            {
                get { return this.BaseConstraint.RowSpan; }
            }

            float IGridBagConstraint.RowWeight
            {
                get { return this.BaseConstraint.RowWeight; }
            }

            VerticalAlignment IGridBagConstraint.VerticalAlignment
            {
                get { return this.BaseConstraint.VerticalAlignment; }
            }

            #endregion //IGridBagConstraint
        }
        #endregion //CPLayoutItem
    } 
    #endregion //CPGridBagLayoutManager
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