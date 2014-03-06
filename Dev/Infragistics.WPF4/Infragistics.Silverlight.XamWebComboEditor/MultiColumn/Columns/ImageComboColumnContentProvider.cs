using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;

namespace Infragistics.Controls.Editors.Primitives
{
	/// <summary>
	/// A class used to provide content to a <see cref="ComboCellBase"/> object for a particular <see cref="ImageComboColumn"/>.
	/// </summary>

	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_MultiColumnCombo, Version = FeatureInfo.Version_11_2)]

	public class ImageComboColumnContentProvider : ComboColumnContentProviderBase
	{
        #region Members

		Image _image;
		bool _heightSet, _widthSet;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Instantiates a new instance of the <see cref="CheckboxComboColumnContentProvider"/>.
		/// </summary>
		public ImageComboColumnContentProvider()
		{
			this._image = new Image();
		}

		#endregion // Constructor

		#region Methods

		#region AdjustDisplayElement

		/// <summary>
		/// Called during EnsureContent to allow the provider a chance to modify it's display based on the current conditions.
		/// </summary>
		/// <param name="cell"></param>
		public override void AdjustDisplayElement(ComboCellBase cell)
		{
			ImageComboColumn column = (ImageComboColumn)cell.Column;

			if (column.Key != null)
			{
				// the Style property of the image, must be set, in order to measure it correctly
				// As there might be properties, such as "Stretch" that will have an impact on how its measured.
				if (this._image.Style != column.ImageStyle)
					this._image.Style = column.ImageStyle;

                 // In SL, the ImageSource won't get set until it is measured, so lets do a preliminary measure to get the imageSource if there is one.
                if (this._image.Source == null && cell.Control != null && this._image.DesiredSize.Width == 0 && this._image.DesiredSize.Height == 0)
                {
                    cell.Control.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    cell.Control.Measure(new Size(1, 1));
                }

				if (this._image.Source != null)
				{
					double value = column.ImageHeight;
					if (value != 0)
					{
						if (this._image.Height != value)
						{
							this._image.Height = value;
							this._heightSet = true;
						}
					}

					value = column.ImageWidth;
					if (value != 0)
					{
						this._image.Width = value;
						this._widthSet = true;
					}
				}
				else
				{
					if (this._heightSet)
					{
						this._heightSet = false;
						this._image.Height = double.NaN;
					}

					if (this._widthSet)
					{
						this._widthSet = false;
						this._image.Width = double.NaN;
					}
				}

				this._image.Measure(new Size(1, 1));
			}
		}

		#endregion // AdjustDisplayElement

		#region ResolveDisplayElement

		/// <summary>
		/// Sets up the element that will be displayed in a <see cref="ComboCellBase"/>. 
		/// </summary>
		/// <param propertyName="cell">The cell that the display element will be displayed in.</param>
		/// <param propertyName="cellBinding">A <see cref="Binding"/> object that can be applied to the cell. Note: this binding can be null.</param>
		/// <returns>The element that should be displayed.</returns>
		public override FrameworkElement ResolveDisplayElement(ComboCellBase cell, Binding cellBinding)
		{
			this._image.IsHitTestVisible = false;

			if (cellBinding != null)
				this._image.SetBinding(Image.SourceProperty, cellBinding);

			ImageComboColumn column = (ImageComboColumn)cell.Column;
			if (column.Key != null)
			{
				if (this._image.Source != null)
				{
					double value = column.ImageHeight;
					if (value != 0 && this._image.Height != value)
					{
						this._image.Height = value;
						this._heightSet = true;
					}

					if (column.ImageWidth != 0)
					{
						this._image.Width = column.ImageWidth;
						this._widthSet = true;
					}
				}
			}

			return this._image;
		}

		#endregion // ResolveDisplayElement

		#endregion // Methods
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