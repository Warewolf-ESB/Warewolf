using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using Infragistics.Windows.Reporting;

namespace Infragistics.Windows.Controls
{
	/// <summary>
	/// An image class that will automatically change the image when its <see cref="UIElement.IsEnabled"/> is false. The element 
	/// can automatically generate one or an image can be provided via the <see cref="DisabledSource"/> property.
	/// </summary>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class AutoDisabledImage : Image
	{
		#region Member Variables

		private ImageSource _lastImageSource;

		#endregion //Member Variables

		#region Constructor
		static AutoDisabledImage()
		{
			Image.SourceProperty.OverrideMetadata(typeof(AutoDisabledImage), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnSourceChanged), new CoerceValueCallback(CoerceSource)));
			FrameworkElement.IsEnabledProperty.OverrideMetadata(typeof(AutoDisabledImage), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsEnabledChanged)));
			FrameworkElement.OpacityMaskProperty.OverrideMetadata(typeof(AutoDisabledImage), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceOpacityMask)));
			FrameworkElement.OpacityProperty.OverrideMetadata(typeof(AutoDisabledImage), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceOpacity)));
		}

		/// <summary>
		/// Initializes a new <see cref="AutoDisabledImage"/>
		/// </summary>
		public AutoDisabledImage()
		{
		}
		#endregion //Constructor

		#region Properties

		#region DisabledSource

		/// <summary>
		/// Identifies the <see cref="DisabledSource"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DisabledSourceProperty = DependencyProperty.Register("DisabledSource",
			typeof(ImageSource), typeof(AutoDisabledImage), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnDisabledSourceChanged)));

		private static void OnDisabledSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// if the image is disabled then update the source
			if (false.Equals(d.GetValue(FrameworkElement.IsEnabledProperty)))
				((AutoDisabledImage)d).VerifySource();
		}

		/// <summary>
		/// Returns or sets the <see cref="ImageSource"/> to use as the source for the image when the <see cref="UIElement.IsEnabled"/> is false.
		/// </summary>
		/// <seealso cref="DisabledSourceProperty"/>
		//[Description("Returns or sets the 'ImageSource' to use as the source for the image when the 'IsEnabled' is false.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public ImageSource DisabledSource
		{
			get
			{
				return (ImageSource)this.GetValue(AutoDisabledImage.DisabledSourceProperty);
			}
			set
			{
				this.SetValue(AutoDisabledImage.DisabledSourceProperty, value);
			}
		}

		#endregion //DisabledSource

		#endregion //Properties

		#region Methods

		#region CoerceOpacity
		private static object CoerceOpacity(DependencyObject d, object newValue)
		{
			AutoDisabledImage img = d as AutoDisabledImage;

			// if there is no mask explicitly set and the element is disabled...
			if (img.IsInitialized &&
				false == img.IsEnabled &&
				img.DisabledSource == null)
			{
				return 0.5d;
			}

			return newValue;
		}
		#endregion //CoerceOpacity

		#region CoerceOpacityMask
		private static object CoerceOpacityMask(DependencyObject d, object newValue)
		{
			AutoDisabledImage img = d as AutoDisabledImage;

			// if there is no mask explicitly set and the element is disabled...
			if (img.IsInitialized &&
				newValue == null &&
				img._lastImageSource != null &&
				img.DisabledSource == null &&
				false == img.IsEnabled)
			{
				ImageBrush brush = new ImageBrush(img._lastImageSource);
				return brush;
			}

			return newValue;
		}
		#endregion //CoerceOpacityMask

		#region CoerceSource
		private static object CoerceSource(DependencyObject d, object newValue)
		{
			AutoDisabledImage img = d as AutoDisabledImage;
			img._lastImageSource = newValue as ImageSource;

			// if there is an image and the image is disabled...
			if (img.IsInitialized && false == img.IsEnabled)
			{
				// see if there is a specific disabled image specified
				object disabledImg = img.DisabledSource;

				// if so, use that
				if (disabledImg != null)
					return disabledImg;

				BitmapSource newBitmap = newValue as BitmapSource;

				if (newBitmap != null)
				{
					// otherwise create one
					FormatConvertedBitmap bitmap = new FormatConvertedBitmap(newBitmap, PixelFormats.Gray32Float, null, 1d);

					// AS 6/17/11
					// The FCB may not be able to be frozen depending on the source. It's best not to freeze the 
					// bitmap unless the source is frozne and the FCB can be frozen.
					//
					//bitmap.Freeze();
					if (newBitmap.IsFrozen && bitmap.CanFreeze)
						bitmap.Freeze();

					return bitmap;
				}
			}

			return newValue;
		}
		#endregion //CoerceSource

		#region OnIsEnabledChanged
		private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			AutoDisabledImage img = d as AutoDisabledImage;

			if (img.IsInitialized)
				img.VerifySource();
		}
		#endregion //OnIsEnabledChanged

		#region OnSourceChanged
		private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			AutoDisabledImage img = d as AutoDisabledImage;

			img.CoerceValue(Image.OpacityProperty);
			img.CoerceValue(Image.OpacityMaskProperty);
		} 
		#endregion //OnSourceChanged

		#region VerifySource
		private void VerifySource()
		{
			this.CoerceValue(Image.SourceProperty);

			// AS 6/17/11 TFS77993
			this.CoerceValue(Image.OpacityProperty);
		}
		#endregion //VerifySource

		#endregion //Methods

		#region Base class overrides

		#region OnInitialized
		/// <summary>
		/// Invoked when the element is initialized.
		/// </summary>
		/// <param name="e">Event arguments</param>
		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			// we need to delay the coersion of the values. if we do it within the properties before
			// its initialized or even synchronously here then the image is sometimes corrupted within
			// the display. i noticed this with certain png images in the ribbon when the tool was disabled
			// whem the app started. disabling thereafter seemed to work ok even when the coersion was 
			// synchronous
            // JJD 3/29/08 - added support for printing.
            // We can't do asynchronous operations during a print

            if (ReportSection.GetIsInReport(this))
                this.VerifySource();
            else

			    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new MethodInvoker(this.VerifySource));
		}

		private delegate void MethodInvoker();
		#endregion //OnInitialized

		#endregion //Base class overrides
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