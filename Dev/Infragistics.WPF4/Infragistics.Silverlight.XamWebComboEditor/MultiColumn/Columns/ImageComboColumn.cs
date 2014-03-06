using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Editors.Primitives;
using System.Windows;

namespace Infragistics.Controls.Editors
{
	/// <summary>
	/// A <see cref="ComboColumn"/> that uses <see cref="System.Windows.Controls.Image"/> elements to represent data.
	/// </summary>

	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_MultiColumnCombo, Version = FeatureInfo.Version_11_2)]

	public class ImageComboColumn : ComboColumn
	{
		#region Members

		Style _imageStyle;

		#endregion // Members

		#region Overrides

		#region GenerateContentProvider

		/// <summary>
		/// Generates a new <see cref="Primitives.CheckboxComboColumnContentProvider"/> that will be used to generate conent for <see cref="ComboCellBase"/> objects for this <see cref="CheckboxComboColumn"/>.
		/// </summary>
		/// <returns></returns>
		protected internal override Primitives.ComboColumnContentProviderBase GenerateContentProvider()
		{
			return new ImageComboColumnContentProvider();
		}
		#endregion // GenerateContentProvider

		#endregion // Overrides

		#region Properties

		#region ImageStyle

		/// <summary>
		/// Gets/sets the Style that will be applied to every <see cref="System.Windows.Controls.Image"/> in this <see cref="ImageComboColumn"/>.
		/// </summary>
		public Style ImageStyle
        {
            get
            {
                return this._imageStyle;
            }
            set
            {
                if (this._imageStyle != value)
                {
                    this._imageStyle = value;
                    this.OnPropertyChanged("ImageStyle");
                }
            }
        }

		#endregion // ImageStyle

		#region ImageHeight

		/// <summary>
		/// Identifies the <see cref="ImageHeight"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ImageHeightProperty = DependencyProperty.Register("ImageHeight", typeof(double), typeof(ImageComboColumn), new PropertyMetadata(new PropertyChangedCallback(ImageHeightChanged)));

		/// <summary>
		/// Gets / sets a height to all images in an <see cref="ImageComboColumn"/>
		/// </summary>
		public double ImageHeight
		{
			get { return (double)this.GetValue(ImageHeightProperty); }
			set { this.SetValue(ImageHeightProperty, value); }
		}

		private static void ImageHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ImageComboColumn col = (ImageComboColumn)obj;
			col.OnPropertyChanged("ImageHeight");
		}

		#endregion // ImageHeight

		#region ImageWidth

		/// <summary>
		/// Identifies the <see cref="ImageWidth"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ImageWidthProperty = DependencyProperty.Register("ImageWidth", typeof(double), typeof(ImageComboColumn), new PropertyMetadata(new PropertyChangedCallback(ImageWidthChanged)));

		/// <summary>
		/// Gets / sets a width to all images in an <see cref="ImageComboColumn"/>
		/// </summary>
		public double ImageWidth
		{
			get { return (double)this.GetValue(ImageWidthProperty); }
			set { this.SetValue(ImageWidthProperty, value); }
		}

		private static void ImageWidthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ImageComboColumn col = (ImageComboColumn)obj;
			col.OnPropertyChanged("ImageWidth");
		}

		#endregion // ImageWidth

		#endregion // Properties
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