using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Infragistics.Controls.Menus
{
    /// <summary>
    /// The <see cref="XamTagCloudPanel"/> is a the default panel used to display <see cref="XamTagCloudItem"/>'s.
    /// </summary>

    [DesignTimeVisible(false)]

    public class XamTagCloudPanel : Panel
    {
        #region Members

        private TagItemComparer _tagItemComparer;
        List<TagItem> _tagItems;
        List<CloudRow> _rows;
        ObservableCollection<ScaleBreak> _scaleBreaks;

        #endregion // Members

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="XamTagCloudPanel"/> class.
        /// </summary>
        public XamTagCloudPanel()
        {
            this._tagItemComparer = new TagItemComparer();
            _tagItems = new List<TagItem>();

        }
        #endregion //Constructor

        #region Events
        /// <summary>
        /// Occurs when a XamTagCloud cannot display all of its items.
        /// </summary>
        public event EventHandler<XamTagCloudClippedEventArgs> XamTagCloudClipped;
        /// <summary>
        /// Fires the <see cref="XamTagCloudClipped"/> event
        /// </summary>
        protected internal virtual void OnXamTagCloudItemClip(bool cloudClipped)
        {
            if (this.XamTagCloudClipped != null)
                this.XamTagCloudClipped(this, new XamTagCloudClippedEventArgs() { CloudClipped = cloudClipped });
        }

        #endregion Events

        #region Methods
        
        #region Private

        #region Arrange Rows

        private double ArrangeRows(List<CloudRow> rows, Size availableSize)
        {
            double maximumHeight = availableSize.Height;
            double totalHeight = 0;

            double remainingSpace = 0;            
            double spaceBetweenRows = 0;

            double bottomOfGrid = 0;

            foreach (CloudRow r in rows)
            {
                if (this.VerticalContentAlignment != VerticalAlignment.Stretch)
                    totalHeight += r.HeightOfTallestItem + this.ItemSpacing.Top + this.ItemSpacing.Bottom;
                else
                    totalHeight += r.HeightOfTallestItem;
            }
            //Implementing vertical stretch behavior.
            remainingSpace = maximumHeight - totalHeight;
            spaceBetweenRows = remainingSpace /(rows.Count - 1);
			
			// We're adding 1 too many ItemSpacing.Bottom. 
			totalHeight -= this.ItemSpacing.Bottom;

            foreach (CloudRow r in rows)
            {
				// Adjust RowHeight for VerticalContentAlignment
				if (!double.IsInfinity(maximumHeight) && totalHeight < maximumHeight)
				{
                    if (this.VerticalContentAlignment == VerticalAlignment.Center)
                        r.TopOfRow += (availableSize.Height - totalHeight) / 2;
                    else if (this.VerticalContentAlignment == VerticalAlignment.Stretch)
                    {
                        //Implementing vertical stretch behavior.
                        if (spaceBetweenRows > 0 && !double.IsInfinity(spaceBetweenRows))
                        {
                            switch (rows.IndexOf(r))
                            {
                                case 0:
                                    break;
                                default:
                                    if (rows.IndexOf(r) != rows.Count - 1)
                                        r.TopOfRow = bottomOfGrid;
                                    else
                                        r.TopOfRow = maximumHeight - r.HeightOfTallestItem - this.ItemSpacing.Bottom;
                                    break;
                            }
                            bottomOfGrid += r.HeightOfTallestItem + spaceBetweenRows;
                        }
                        else //If something goes wrong default to centering everything.
                            r.TopOfRow += (availableSize.Height - totalHeight) / 2;
                    }
                    else if (this.VerticalContentAlignment == VerticalAlignment.Bottom)
                        r.TopOfRow += (availableSize.Height - totalHeight) - this.ItemSpacing.Bottom;
				}

                r.BottomOfRow = r.TopOfRow + r.HeightOfTallestItem + this.ItemSpacing.Bottom;

                this.ArrangeCells(r, availableSize.Width);
            }
            return totalHeight;

        }

        #endregion //Arrange Rows

        #region Arrange Cells

        private void ArrangeCells(CloudRow row, double maximumWidth)
        {
            FrameworkElement tci;
            double verticalCenterOfCurrentRow = row.TopOfRow + row.HeightOfTallestItem / 2;

            double endOfCurrentRow = 0;

            double totalWidthRequiredByCells = 0;

            //Randomizer for choosing a random top position for a given tag.
            Random randomYPosition = new Random();

            foreach (TagItem t in row.CurrentRowItems)
                totalWidthRequiredByCells += t.Element.DesiredSize.Width * t.ScaledSize;

            foreach (TagItem item in row.CurrentRowItems)
            {
                tci = item.Element as FrameworkElement;
                if (tci != null)
                {
                    double scaledItemHeight = item.Element.DesiredSize.Height * item.ScaledSize;
                    double maximumYPositionOfCurrentItem = row.BottomOfRow - scaledItemHeight - this.ItemSpacing.Bottom;

                    //If no vertical alignment has been set on the items calculate a random y position based on the maximum possible y position for the current item.
                    double randomValidYPosition = row.TopOfRow < maximumYPositionOfCurrentItem ? randomYPosition.Next((int)row.TopOfRow, (int)maximumYPositionOfCurrentItem) : row.TopOfRow;

                    //Save the previous y position so that we don't have to reposition it unless necessary.
                    double previousYPosition = XamTagCloudPanel.GetCurrentYPosition(item.Element);

                    //Calculate the final YPosition of the item.
                    double yPositionResolved = 0;
                    if (tci.VerticalAlignment == VerticalAlignment.Top)
					{
						yPositionResolved = row.TopOfRow;
					}
					else if (tci.VerticalAlignment == VerticalAlignment.Stretch)
					{
						if (previousYPosition < maximumYPositionOfCurrentItem && previousYPosition != 0 && previousYPosition >= row.TopOfRow)
							yPositionResolved = previousYPosition;
						else
							yPositionResolved = randomValidYPosition;
					}
					else if (tci.VerticalAlignment == VerticalAlignment.Bottom)
					{
						yPositionResolved = maximumYPositionOfCurrentItem;
					}
					else
					{
						yPositionResolved = verticalCenterOfCurrentRow - scaledItemHeight / 2;
					}

                    //Implementing vertical stretch behavior.
                    double remainingSpace = maximumWidth - totalWidthRequiredByCells;
                    double spaceBetweenCells = remainingSpace / (row.CurrentRowItems.Count - 1);

                    double scaledItemWidth = item.Element.DesiredSize.Width*item.ScaledSize;
                    //Calculate the final XPosition of the item.
                    double xPositionResolved = item.XPosition;
					if (!double.IsPositiveInfinity(maximumWidth))
					{
						if (this.HorizontalContentAlignment == HorizontalAlignment.Center)
						{
							xPositionResolved = item.XPosition + (maximumWidth - row.LengthOfRow) / 2;
						}
                        else if (this.HorizontalContentAlignment == HorizontalAlignment.Stretch)
                        {
                            //Implementing vertical stretch behavior.
                            if (spaceBetweenCells >= 0 && !double.IsInfinity(spaceBetweenCells))
                            {
                                switch (row.CurrentRowItems.IndexOf(item))
                                {
                                    case 0:
                                        break;
                                    default:
                                        if (row.CurrentRowItems.IndexOf(item) != row.CurrentRowItems.Count - 1)
                                            xPositionResolved = endOfCurrentRow;
                                        else
                                            xPositionResolved = maximumWidth - scaledItemWidth - this.ItemSpacing.Right;
                                        break;
                                }

                                endOfCurrentRow += scaledItemWidth + spaceBetweenCells;
                            }
                            else
                                xPositionResolved = item.XPosition + (maximumWidth - row.LengthOfRow) / 2;

                        }
                        else if (this.HorizontalContentAlignment == HorizontalAlignment.Right)
                        {
                            xPositionResolved = item.XPosition + (maximumWidth - row.LengthOfRow);
                        }
					}

                    //Set the items rectangle.
                    item.Rect = new Rect(xPositionResolved, yPositionResolved, item.Element.DesiredSize.Width, item.Element.DesiredSize.Height);

                    XamTagCloudPanel.SetCurrentYPosition(item.Element, item.Rect.Top);
                }
            }
        }

        #endregion //Arrange Cells

        #endregion //Private

        #region Protected
        /// <summary>
        /// Sets up scale breaks for the <see cref="XamTagCloudPanel"/>.
        /// </summary>
        /// <param name="sb"></param>
        protected internal void SetScaleBreaks(ObservableCollection<ScaleBreak> sb)
        {
            this._scaleBreaks = sb;
        }
        #endregion //Protected

        #endregion //Methods

        #region Overrides

		#region MeasureOverride
		/// <summary>
        /// Provides the behavior for the "measure" pass of the layout. Classes can override this method to define their own measure pass behavior.
        /// </summary>
        /// <param name="availableSize">The available size that this object can give to child objects. Infinity can be specified as a value to indicate that the object will size to whatever content is available.</param>
        /// <returns>
        /// The size that this object determines it needs during layout, based on its calculations of child object allotted sizes.
        /// </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            this._tagItems.Clear();
            this._rows = new List<CloudRow>();

            //Loop through child UIElements and store them along with their weights in a list of tag items.
            foreach (UIElement child in Children)
            {
                double Weight = XamTagCloudPanel.GetWeight(child);

                //Apply scale breaks
                if (this.ScaleBreaks != null)
                {
                    foreach (ScaleBreak sb in this.ScaleBreaks)
                    {
                        if (Weight >= sb.StartWeight && Weight <= sb.EndWeight)
                        {
                            Weight = sb.Weight;
                            break;
                        }
                    }
                }
                _tagItems.Add(new TagItem() { Weight = Weight, Element = child });
            }
            double totalHeight = 0;
            double totalWidth = 0;
            //If we have items to render lets do it.
            if (this._tagItems.Count > 0)
            {
                //Dimensions of current tag.
                double widthOfCurrentItem, heightOfCurrentItem, scaledSizeOfCurrentItem;

                //We need to sort the items by weight to figure out the min and max weights, I want to do this without affecting the order
                //of my original tag list provided by the user.
                List<TagItem> items = new List<TagItem>(this._tagItems);
                items.Sort(this._tagItemComparer);

                double max = this.MaxScale;
                double min = this.MinScale;

                double maxWeight = items[items.Count - 1].Weight;
                double minWeight = items[0].Weight;

                double multiplier = (max - min) / (maxWeight - minWeight);

                //If the multiplyer is infinity or NaN we should just bail, because there's nothing I can do at this point.
                if (Double.IsNaN(multiplier) || Double.IsInfinity(multiplier))
                    multiplier = maxWeight;

                //Create a new row and set it up.
                CloudRow r = new CloudRow();
                r.CurrentRowItems = new List<TagItem>();
                r.TopOfRow = this.ItemSpacing.Top;
                r.BottomOfRow = this.ItemSpacing.Top;

				r.LengthOfRow = this.ItemSpacing.Left;
                r.HeightOfTallestItem = 0;
                r.MaximumRowWidth = availableSize.Width;

                foreach (TagItem item in this._tagItems)
                {
                    //Compute the scale.
                    scaledSizeOfCurrentItem = min + ((maxWeight - (maxWeight - (item.Weight - minWeight))) * multiplier);

                    //If we're using smooth scaling we need to take the log of the scaledSizeOfCurrentItem.
                    if (this.UseSmoothScaling == true)
                    {
                        scaledSizeOfCurrentItem = Math.Log(scaledSizeOfCurrentItem);
                        scaledSizeOfCurrentItem = scaledSizeOfCurrentItem < min ? min : scaledSizeOfCurrentItem;
                    }
                    //Apply the scale.
                    ScaleTransform st = new ScaleTransform();
                    st.ScaleX = scaledSizeOfCurrentItem;
                    st.ScaleY = scaledSizeOfCurrentItem;
                    item.Element.RenderTransform = st;
                    item.ScaledSize = scaledSizeOfCurrentItem;

                    //Measure the element and then calculate it's actual height and width using the scale. Pass in positive infinity here so
                    //the item will size itself, otherwise they will be restricted by the available dimensions.
                    item.Element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    heightOfCurrentItem = item.Element.DesiredSize.Height * scaledSizeOfCurrentItem;
                    widthOfCurrentItem = item.Element.DesiredSize.Width * scaledSizeOfCurrentItem;

                    double bottomOfPreviousRow = r.BottomOfRow;

                    //If the length of the current row is about to exceede the maximum start a new row.
                    if (r.LengthOfRow + widthOfCurrentItem + this.ItemSpacing.Right > r.MaximumRowWidth || r.LengthOfRow == this.ItemSpacing.Left)
                    {
                        if (r.LengthOfRow != this.ItemSpacing.Left)
                        {
							// We need to adjust for the last time we added left ItemSpacing or the row, as it wouldn't apply
							r.LengthOfRow -= this.ItemSpacing.Left;

                            //Take the total width of the previous row first
                            if (r.LengthOfRow > totalWidth)
                                totalWidth = r.LengthOfRow;

                            //Add the old row to the rows collection.
                            this._rows.Add(r);

                            //Create new row
                            r = new CloudRow();
                            r.CurrentRowItems = new List<TagItem>();

                            //Calculate the top position of the new row.
							r.TopOfRow += bottomOfPreviousRow + this.ItemSpacing.Top;

                            r.LengthOfRow = this.ItemSpacing.Left;
                        }

                        //Set the items XPosition.
                        item.XPosition = r.LengthOfRow;

                        //Add our current item to the new row.
                        r.CurrentRowItems.Add(item);

                        //Calculate the new row length, including the item padding.
                        r.LengthOfRow += this.ItemSpacing.Left + widthOfCurrentItem + this.ItemSpacing.Right;

                        //Taking the maximum width of the row, it's possible for this to be infinite.
                        r.MaximumRowWidth = availableSize.Width;

                        //Set the new tallest item height, it'll be whatever the current item is obviously.
                        r.HeightOfTallestItem = heightOfCurrentItem;

                        //Calulate the new bottom of the row.
                        r.BottomOfRow = r.TopOfRow + r.HeightOfTallestItem + ItemSpacing.Bottom;

                        //Increase the total height of the cloud.
                        totalHeight += r.HeightOfTallestItem;

                    }
                    else
                    {
						r.HeightOfTallestItem = Math.Max(heightOfCurrentItem, r.HeightOfTallestItem);

						//Calculate the new row bottom.
                        r.BottomOfRow = r.TopOfRow + r.HeightOfTallestItem + this.ItemSpacing.Bottom;
                        //Take the XPosition of the item to be used later for arranging it.
                        item.XPosition = r.LengthOfRow;
                        //Add our current item to the new row.
                        r.CurrentRowItems.Add(item);

                        //Increase the length of the row.
                        r.LengthOfRow += this.ItemSpacing.Left + widthOfCurrentItem + this.ItemSpacing.Right;
                    }
                }

				// We need to adjust for the last time we added left ItemSpacing or the row, as it wouldn't apply
				r.LengthOfRow -= this.ItemSpacing.Left;

                //Increase our width if the current row is wider than our previous width.
                if (r.LengthOfRow > totalWidth)
                    totalWidth = r.LengthOfRow;


                //Add the row to the rows collection.
                this._rows.Add(new CloudRow(r.CurrentRowItems, r.HeightOfTallestItem, r.TopOfRow, r.LengthOfRow));

                //Increase the total height of the cloud.
                totalHeight = this.ArrangeRows(this._rows, availableSize) + this.ItemSpacing.Top + this.ItemSpacing.Bottom;

            }

            if (totalHeight > availableSize.Height || totalWidth > availableSize.Width)
                this.OnXamTagCloudItemClip(true);
            else
                this.OnXamTagCloudItemClip(false);

            //If the available height and width are not infinity we can use those to set our height and width.
            if (!double.IsInfinity(availableSize.Height) && availableSize.Height > totalHeight)
                totalHeight = availableSize.Height;
            if (!double.IsInfinity(availableSize.Width) && availableSize.Width > totalWidth)
                totalWidth = availableSize.Width;



            return new Size(totalWidth, totalHeight);
		}
		#endregion // MeasureOverride

		#region ArrangeOverride
		/// <summary>
        /// Provides the behavior for the "Arrange" pass of the layout. Classes can override this method to define their own arrange pass behavior.
        /// </summary>
        /// <param name="finalSize">The final area within the parent that this object should use to arrange itself and its children.</param>
        /// <returns>The actual size used.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (TagItem item in this._tagItems)
                item.Element.Arrange(item.Rect);

            RectangleGeometry cliprect = new RectangleGeometry();
            cliprect.Rect = new Rect(this.ItemSpacing.Left, this.ItemSpacing.Top, finalSize.Width, finalSize.Height);
            this.Clip = cliprect;
            return base.ArrangeOverride(new Size(finalSize.Width, finalSize.Height));

		}
		#endregion // ArrangeOverride

		#endregion //Overrides

		#region Properties

		#region Public

		#region NavigateUriProperty

		/// <summary>
        /// Identifies the <see cref="NavigateUriProperty"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty NavigateUriProperty = DependencyProperty.RegisterAttached("NavigateUriProperty", typeof(Uri), typeof(XamTagCloudPanel), null);

        /// <summary>
        /// Sets the <see cref="Uri"/> that should be attached to an item.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetNavigateUri(DependencyObject obj, Uri value)
        {
            obj.SetValue(NavigateUriProperty, value);
        }

        /// <summary>
        /// Gets the <see cref="Uri"/> that should be attached to an item.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Uri GetNavigateUri(DependencyObject obj)
        {
            return (Uri)obj.GetValue(NavigateUriProperty);
        }

        #endregion // NavigateUriProperty

        #region UseSmoothScaling

        /// <summary>
        /// Identifies the <see cref="UseSmoothScaling"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty UseSmoothScalingProperty = DependencyProperty.Register("UseSmoothScaling", typeof(bool), typeof(XamTagCloudPanel), new PropertyMetadata(new PropertyChangedCallback(UseSmoothScalingChanged)));
        /// <summary>
        /// Gets or sets the <see cref="UseSmoothScaling"/> property.
        /// </summary>
        public bool UseSmoothScaling
        {
            get { return (bool)this.GetValue(UseSmoothScalingProperty); }
            set { this.SetValue(UseSmoothScalingProperty, value); }
        }

        private static void UseSmoothScalingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((XamTagCloudPanel)obj).InvalidateMeasure();
        }

        #endregion // UseSmoothScaling

        #region MaxScale

        /// <summary>
        /// Identifies the <see cref="MaxScale"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty MaxScaleProperty = DependencyProperty.Register("MaxScale", typeof(double), typeof(XamTagCloudPanel), new PropertyMetadata(new PropertyChangedCallback(MaxScaleChanged)));
        /// <summary>
        /// Gets or sets the <see cref="MaxScale"/> property.
        /// </summary>
        public double MaxScale
        {
            get { return (double)this.GetValue(MaxScaleProperty); }
            set { this.SetValue(MaxScaleProperty, value); }
        }

        private static void MaxScaleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((XamTagCloudPanel)obj).InvalidateMeasure();
        }

        #endregion // MaxScale

        #region MinScale

        /// <summary>
        /// Identifies the <see cref="MinScale"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty MinScaleProperty = DependencyProperty.Register("MinScale", typeof(double), typeof(XamTagCloudPanel), new PropertyMetadata(new PropertyChangedCallback(MinScaleChanged)));

        /// <summary>
        /// Gets or sets the <see cref="MinScale"/> property.
        /// </summary>
        public double MinScale
        {
            get { return (double)this.GetValue(MinScaleProperty); }
            set { this.SetValue(MinScaleProperty, value); }
        }

        private static void MinScaleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((XamTagCloudPanel)obj).InvalidateMeasure();
        }

        #endregion // MinScale

        #region ItemSpacing

        /// <summary>
        /// Identifies the <see cref="ItemSpacing"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ItemSpacingProperty = DependencyProperty.Register("ItemSpacing", typeof(Thickness), typeof(XamTagCloudPanel), new PropertyMetadata(new PropertyChangedCallback(ItemSpacingChanged)));

        /// <summary>
        /// Gets or sets the <see cref="ItemSpacing"/> property.
        /// </summary>
        public Thickness ItemSpacing
        {
            get { return (Thickness)this.GetValue(ItemSpacingProperty); }
            set { this.SetValue(ItemSpacingProperty, value); }
        }

        private static void ItemSpacingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((XamTagCloudPanel)obj).InvalidateMeasure();
        }

        #endregion // ItemSpacing

        #region ScaleBreaks
        /// <summary>
        /// A collection of <see cref="ScaleBreak"/>'s which are used to define explicit scale values for a given range of scale values.
        /// </summary>
        public ObservableCollection<ScaleBreak> ScaleBreaks
        {
            get { return this._scaleBreaks; }
        }

        #endregion // ScaleBreaks

        #region HorizontalContentAlignment

        /// <summary>
        /// Identifies the <see cref="HorizontalContentAlignment"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HorizontalContentAlignmentProperty = DependencyProperty.Register("HorizontalContentAlignment", typeof(HorizontalAlignment), typeof(XamTagCloudPanel), new PropertyMetadata(new PropertyChangedCallback(HorizontalContentAlignmentChanged)));
        /// <summary>
        /// Gets and sets the <see cref="HorizontalContentAlignment"/> property.
        /// </summary>
        public HorizontalAlignment HorizontalContentAlignment
        {
            get { return (HorizontalAlignment)this.GetValue(HorizontalContentAlignmentProperty); }
            set { this.SetValue(HorizontalContentAlignmentProperty, value); }
        }

        private static void HorizontalContentAlignmentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((XamTagCloudPanel)obj).InvalidateMeasure();
        }

        #endregion // HorizontalContentAlignment

        #region VerticalContentAlignment

        /// <summary>
        /// Identifies the <see cref="VerticalContentAlignment"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty VerticalContentAlignmentProperty = DependencyProperty.Register("VerticalContentAlignment", typeof(VerticalAlignment), typeof(XamTagCloudPanel), new PropertyMetadata(new PropertyChangedCallback(VerticalContentAlignmentChanged)));
        /// <summary>
        /// Gets and sets the <see cref="VerticalContentAlignment"/> property.
        /// </summary>
        public VerticalAlignment VerticalContentAlignment
        {
            get { return (VerticalAlignment)this.GetValue(VerticalContentAlignmentProperty); }
            set { this.SetValue(VerticalContentAlignmentProperty, value); }
        }

        private static void VerticalContentAlignmentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((XamTagCloudPanel)obj).InvalidateMeasure();
        }

        #endregion // VerticalContentAlignment


        #region Weight

        /// <summary>
        /// Identifies the <see cref="WeightProperty"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty WeightProperty = DependencyProperty.RegisterAttached("Weight", typeof(Double), typeof(XamTagCloudPanel), null);

        /// <summary>
        /// Sets the Weight.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetWeight(DependencyObject obj, double value)
        {
            obj.SetValue(WeightProperty, value);
        }


        /// <summary>
        /// Gets the Weight.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static double GetWeight(DependencyObject obj)
        {
            return (double)obj.GetValue(WeightProperty);
        }

        #endregion // Weight

        #endregion //Public

        #region Protected

        #endregion //Protected

        #region Private
        #region CurrentYPosition

        /// <summary>
        /// Identifies the <see cref="CurrentYPositionProperty"/> dependency property. 
        /// </summary>
        private static readonly DependencyProperty CurrentYPositionProperty = DependencyProperty.RegisterAttached("CurrentYPosition", typeof(double), typeof(XamTagCloudPanel), null);

        private static void SetCurrentYPosition(DependencyObject obj, double value)
        {
            obj.SetValue(CurrentYPositionProperty, value);
        }

        private static double GetCurrentYPosition(DependencyObject obj)
        {
            return (double)obj.GetValue(CurrentYPositionProperty);
        }

        #endregion // CurrentYPosition
        #endregion //Private

        #endregion //Properties

        #region Tag Item

        private class TagItem
        {
            public double Weight { get; set; }
            public UIElement Element { get; set; }
            public Rect Rect { get; set; }
            public double XPosition { get; set; }
            public double ScaledSize { get; set; }
        }

        private class TagItemComparer : IComparer<TagItem>
        {
            #region IComparer<TagItem> Members

            public int Compare(TagItem x, TagItem y)
            {
                return x.Weight.CompareTo(y.Weight);
            }

            #endregion
        }
        #endregion //TagItem

        #region Cloud Row
        private class CloudRow
        {
            public CloudRow()
            {

            }

            public CloudRow(List<TagItem> currentRowItems, double heightOfTallestItem, double topOfRow, double lengthOfRow)
            {
                CurrentRowItems = currentRowItems;
                HeightOfTallestItem = heightOfTallestItem;
                TopOfRow = topOfRow;
                LengthOfRow = lengthOfRow;
            }
            public List<TagItem> CurrentRowItems { get; set; }
            public double HeightOfTallestItem { get; set; }
            public double TopOfRow { get; set; }
            public double LengthOfRow { get; set; }
            public double BottomOfRow { get; set; }
            public double MaximumRowWidth { get; set; }
            
        }
        #endregion //Cloud Row
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