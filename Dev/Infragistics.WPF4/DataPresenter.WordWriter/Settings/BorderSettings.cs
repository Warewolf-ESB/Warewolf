using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using Infragistics.Documents.Word;

namespace Infragistics.Windows.DataPresenter.WordWriter
{
	/// <summary>
	/// Exposes border settings for table cells when exporting to Word
	/// </summary>
	/// <seealso cref="WordTableSettingsBase.BorderSettings"/>
	/// <seealso cref="WordTableCellSettings.BorderSettings"/>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class WordBorderSettings : DependencyObject
	{
		#region Member Variables

		private Color _color;
		private TableBorderStyle? _style;
		private DeviceUnitLength? _width; 

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="WordBorderSettings"/>
		/// </summary>
		public WordBorderSettings()
		{
		}
		#endregion //Constructor

		#region Properties

		#region Public Properties

		#region Color

		/// <summary>
		/// Identifies the <see cref="Color"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color",
			typeof(Color), typeof(WordBorderSettings), new FrameworkPropertyMetadata(WordExporter.EmptyColor, new PropertyChangedCallback(OnColorChanged)));

		private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((WordBorderSettings)d)._color = (Color)e.NewValue;
		}

		/// <summary>
		/// Returns or sets the color of the borders
		/// </summary>
		/// <seealso cref="ColorProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.TableBorderProperties.Color"/>
		[Bindable(true)]
		public Color Color
		{
			get
			{
				return _color;
			}
			set
			{
				this.SetValue(WordBorderSettings.ColorProperty, value);
			}
		}

		#endregion //Color

		#region Style

		/// <summary>
		/// Identifies the <see cref="Style"/> dependency property
		/// </summary>
		public static readonly DependencyProperty StyleProperty = DependencyProperty.Register("Style",
			typeof(TableBorderStyle?), typeof(WordBorderSettings), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnStyleChanged)));

		private static void OnStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((WordBorderSettings)d)._style = (TableBorderStyle?)e.NewValue;
		}

		/// <summary>
		/// Returns or sets the style of the borders
		/// </summary>
		/// <seealso cref="StyleProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.TableBorderProperties.Style"/>
		[Bindable(true)]
		public TableBorderStyle? Style
		{
			get
			{
				return _style;
			}
			set
			{
				this.SetValue(WordBorderSettings.StyleProperty, value);
			}
		}

		#endregion //Style

		#region Width

		/// <summary>
		/// Identifies the <see cref="Width"/> dependency property
		/// </summary>
		public static readonly DependencyProperty WidthProperty = DependencyProperty.Register("Width",
			typeof(DeviceUnitLength?), typeof(WordBorderSettings), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnWidthChanged)));

		private static void OnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((WordBorderSettings)d)._width = (DeviceUnitLength?)e.NewValue;
		}

		/// <summary>
		/// Returns or sets the width of the border in twips.
		/// </summary>
		/// <seealso cref="WidthProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.TableBorderProperties.Width"/>
		[Bindable(true)]
		public DeviceUnitLength? Width
		{
			get
			{
				return _width;
			}
			set
			{
				this.SetValue(WordBorderSettings.WidthProperty, value);
			}
		}

		#endregion //Width

		#endregion //Public Properties

		#region Internal Properties

		#region HasValues
		internal bool HasValues
		{
			get
			{
				return _style != null ||
					_width != null ||
					_color != WordExporter.EmptyColor;
			}
		}
		#endregion //HasValues

		#endregion //Internal Properties
		#endregion //Properties

		#region Methods

		#region Initialize
		internal void Initialize(WordDocumentWriter writer, TableBorderProperties borderProperties)
		{
			if (null != borderProperties)
			{
				UnitOfMeasurement unit = writer.Unit;

				borderProperties.Color = WordExporter.ResolveProperty(_color, borderProperties.Color);
				borderProperties.Style = _style ?? borderProperties.Style;
				borderProperties.Width = WordExporter.ConvertUnits(_width, 5f, 320f, unit) ?? borderProperties.Width;
			}
		}
		#endregion //Initialize

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