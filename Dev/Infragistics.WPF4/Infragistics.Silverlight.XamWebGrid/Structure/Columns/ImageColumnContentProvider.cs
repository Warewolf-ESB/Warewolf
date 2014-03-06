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
using System.Windows.Data;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// An object that provides content to <see cref="Cell"/> objects that belong to a <see cref="ImageColumn"/>
	/// </summary>
	public class ImageColumnContentProvider : ColumnContentProviderBase
	{
		#region Members

		Image _image;
        bool _heightSet, _widthSet;
		#endregion // Members

		#region Constructor

		/// <summary>
		/// Instantiates a new instance of the <see cref="ImageColumnContentProvider"/>.
		/// </summary>
		public ImageColumnContentProvider()
		{
			this._image = new Image();
			this._image.ImageFailed += new EventHandler<ExceptionRoutedEventArgs>(Image_ImageFailed);    
		}

		#endregion // Constructor

		#region Methods

		#region ResolveDisplayElement

		/// <summary>
		/// Sets up the element that will be displayed in a <see cref="Cell"/>, when the cell is not in edit mode. 
		/// </summary>
		/// <param propertyName="cell">The cell that the display element will be displayed in.</param>
		/// <param propertyName="cellBinding">A <see cref="Binding"/> object that can be applied to the cell. Note: this binding can be null.</param>
		/// <returns>The element that should be displayed.</returns>
        public override FrameworkElement ResolveDisplayElement(Cell cell, Binding cellBinding)
        {
            this.SetUpImageDimensions(cell);
            this.ApplyBindingToDisplayElement(cell, cellBinding);
            return this._image;
        }

		#endregion // ResolveDisplayElement

        #region ApplyBindingToDisplayElement

        /// <summary>
        /// This is where a ColumnContentProvider should apply the Binding to their Display element.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="binding"></param>
        private void ApplyBindingToDisplayElement(Cell cell, Binding binding)
        {
            if (binding != null)
                this._image.SetBinding(Image.SourceProperty, binding);

            SetUpImageDimensions(cell);
            //ImageColumn column = (ImageColumn)cell.Column;

            //if (column.Key != null)
            //{
            //    if (this._image.Source != null)
            //    {
            //        double value = column.ImageHeight;
            //        if (value != 0 && this._image.Height != value)
            //        {
            //            this._image.Height = value;
            //            this._heightSet = true;
            //        }

            //        if (column.ImageWidth != 0)
            //        {
            //            this._image.Width = column.ImageWidth;
            //            this._widthSet = true;
            //        }
            //    }
            //}
        }

        #endregion // ApplyBindingToDisplayElement

		#region AdjustDisplayElement

		/// <summary>
		/// Called during EnsureContent to allow the provider a chance to modify it's display based on the current conditions.
		/// </summary>
		/// <param name="cell"></param>
        public override void AdjustDisplayElement(Cell cell)
        {
            this.SetUpImageDimensions(cell);
            this._image.Measure(new Size(1, 1));
            this._image.InvalidateMeasure();
        }

		#endregion // AdjustDisplayElement

		#region ResolveEditorControl

		/// <summary>
		/// Sets up the edtior control that will be displayed in a <see cref="Cell"/> when the the cell is in edit mode.
		/// </summary>
		/// <param propertyName="cell">The <see cref="Cell"/> entering edit mode.</param>
		/// <param propertyName="editorValue">The value that should be put in the editor.</param>
		/// <param propertyName="availableWidth">The amount of horizontal space available.</param>
		/// <param propertyName="availableHeight">The amound of vertical space available.</param>
		/// <param propertyName="editorBinding">Provides a <see cref="Binding"/> that can be used for setting up the editor.</param>
		/// <returns></returns>
		protected override FrameworkElement ResolveEditorControl(Cell cell, object editorValue, double availableWidth, double availableHeight, Binding editorBinding)
		{
			return null;
		}
		
		#endregion // ResolveEditorControl

		#region ResolveValueFromEditor

		/// <summary>
		/// Resolves the value of the editor control, so that the cell's underlying data can be updated. 
		/// </summary>
		/// <param propertyName="cell">The <see cref="Cell"/> that the editor id being displayed in.</param>
		/// <returns>The value that should be displayed in the cell.</returns>
		public override object ResolveValueFromEditor(Cell cell)
		{
			return null;
		}
		
		#endregion // ResolveValueFromEditor

		#region GenerateGroupByCellContent

		/// <summary>
		/// Used when rendering a GroupByRow, allows for the column content provider to override default behavior and render out a representation of the data.
		/// </summary>
		/// <param name="cell"></param>
		/// <param name="cellBinding"></param>
		/// <returns></returns>
		public override FrameworkElement GenerateGroupByCellContent(GroupByCell cell, Binding cellBinding)
		{
			ImageColumn column = (ImageColumn)cell.ResolveColumn;

			if (column.Key != null)
			{
				this._image.DataContext = cell.Row.Data;

				if (cellBinding != null)
					this._image.SetBinding(Image.SourceProperty, cellBinding);

				this._image.Style = column.ImageStyle;

				if (column.ImageHeight != 0)
					this._image.Height = column.ImageHeight;

				if (column.ImageWidth != 0)
					this._image.Width = column.ImageWidth;
			}

			return this._image;
		}

		#endregion // GenerateGroupByCellContent

		#region ResetContent
		
		/// <summary>
		/// Raised when the cell is recycling to allow the provider a chance to clear any internal members.
		/// </summary>
		public override void ResetContent()
		{
			if (this._image != null)
			{
				this._image.Measure(new Size(1, 1));
			}
			base.ResetContent();
		}
		
		#endregion // ResetContent

        private void SetUpImageDimensions(Cell cell)
        {
            ImageColumn column = (ImageColumn)cell.Column;

            if (column.Key != null)
            {
                // the Style property of the image, must be set, in order to measure it correctly
                // As there might be properties, such as "Stretch" that will have an impact on how its measured.
                ColumnContentProviderBase.SetControlStyle(this._image, column.ImageStyle);                

                //if (this._image.Source != null)
                {
                    double value = column.ImageHeight;
                    if (value != 0)
                    {
                        if (this._image.Height != value || (this._image.Height == value && !this._heightSet))
                        {
                            this._image.Height = value;
                            cell.Row.ActualHeight = 0;
                            this._heightSet = true;
                        }
                    }

                    value = column.ImageWidth;
                    if (value != 0 || (this._image.Width == value && !this._widthSet))
                    {
                        this._image.Width = value;
                        this._widthSet = true;
                    }
                }
                //else
                //{
                //    if (this._heightSet)
                //    {
                //        this._heightSet = false;
                //        this._image.Height = (column.ImageHeight != 0 && !double.IsNaN(column.ImageHeight) && !double.IsInfinity(column.ImageHeight)) ? column.ImageHeight : double.NaN;
                //    }

                //    if (this._widthSet)
                //    {
                //        this._widthSet = false;
                //        this._image.Width = (column.ImageWidth != 0 && !double.IsNaN(column.ImageWidth) && !double.IsInfinity(column.ImageWidth)) ? column.ImageWidth : double.NaN;
                //    }
                //}
            }
        }

		#endregion // Methods

		#region Events

		void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
		{

		}

		#endregion // Events


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