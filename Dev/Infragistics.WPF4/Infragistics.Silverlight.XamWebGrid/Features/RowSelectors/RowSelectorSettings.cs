using System.Windows;
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// An object that contains settings for using RowSelectorSettings on the <see cref="XamGrid"/>
	/// </summary>
	[StyleTypedProperty(Property = "Style", StyleTargetType = typeof(RowSelectorCellControl))]
	[StyleTypedProperty(Property = "HeaderStyle", StyleTargetType = typeof(RowSelectorHeaderCellControl))]
	[StyleTypedProperty(Property = "FooterStyle", StyleTargetType = typeof(RowSelectorFooterCellControl))]
	public class RowSelectorSettings : VisualSettingsBase
	{
		#region Constructor

		/// <summary>
		/// Creates a new instance of the <see cref="RowSelectorSettings"/> object.
		/// </summary>
		public RowSelectorSettings()
		{
			this.Visibility = Visibility.Collapsed;
		}

		#endregion // Constructor

		#region EnableRowNumbering

		/// <summary>
        /// Identifies the <see cref="EnableRowNumbering"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty EnableRowNumberingProperty = DependencyProperty.Register("EnableRowNumbering", typeof(bool), typeof(RowSelectorSettings), new PropertyMetadata(true, new PropertyChangedCallback(EnableRowNumberingChanged)));

        /// <summary>
        /// Gets / sets if the RowSelectors should show row numbers.
        /// </summary>
        public bool EnableRowNumbering
        {
            get { return (bool)this.GetValue(EnableRowNumberingProperty); }
            set { this.SetValue(EnableRowNumberingProperty, value); }
        }

        private static void EnableRowNumberingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            RowSelectorSettings settingsOverride = (RowSelectorSettings)obj;
            settingsOverride.OnPropertyChanged("EnableRowNumbering");
        }

        #endregion // EnableRowNumbering 

		#region HeaderStyle

		/// <summary>
		/// Identifies the <see cref="HeaderStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty HeaderStyleProperty = DependencyProperty.Register("HeaderStyle", typeof(Style), typeof(RowSelectorSettings), new PropertyMetadata(new PropertyChangedCallback(HeaderStyleChanged)));

		/// <summary>
		/// Gets/sets the <see cref="Style"/> that will be used on the <see cref="RowSelectorHeaderCellControl"/> objects of the <see cref="ColumnLayout"/>.
		/// </summary>
		public Style HeaderStyle
		{
			get { return (Style)this.GetValue(HeaderStyleProperty); }
			set { this.SetValue(HeaderStyleProperty, value); }
		}

		private static void HeaderStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			RowSelectorSettings settings = (RowSelectorSettings)obj;
			if (settings.Grid != null)
				settings.Grid.ResetPanelRows();
			settings.OnPropertyChanged("HeaderStyle");
		}

		#endregion // Style

		#region FooterStyle

		/// <summary>
		/// Identifies the <see cref="FooterStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FooterStyleProperty = DependencyProperty.Register("FooterStyle", typeof(Style), typeof(RowSelectorSettings), new PropertyMetadata(new PropertyChangedCallback(FooterStyleChanged)));

		/// <summary>
		/// Gets/sets the <see cref="Style"/> that will be used on the <see cref="RowSelectorFooterCellControl"/> objects of the <see cref="ColumnLayout"/>.
		/// </summary>
		public Style FooterStyle
		{
			get { return (Style)this.GetValue(FooterStyleProperty); }
			set { this.SetValue(FooterStyleProperty, value); }
		}

		private static void FooterStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			RowSelectorSettings settings = (RowSelectorSettings)obj;
			if (settings.Grid != null)
				settings.Grid.ResetPanelRows();
			settings.OnPropertyChanged("FooterStyle");
		}

		#endregion // Style

		#region RowNumberingSeed

		/// <summary>
		/// Identifies the <see cref="RowNumberingSeed"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty RowNumberingSeedProperty = DependencyProperty.Register("RowNumberingSeed", typeof(int), typeof(RowSelectorSettings), new PropertyMetadata(1, new PropertyChangedCallback(RowNumberingSeedChanged)));

		/// <summary>
		/// Gets / sets the value that will be used to start the row numbering on each <see cref="ColumnLayout"/> in the <see cref="XamGrid"/>.
		/// </summary>
		public int RowNumberingSeed
		{
			get { return (int)this.GetValue(RowNumberingSeedProperty); }
			set { this.SetValue(RowNumberingSeedProperty, value); }
		}

		private static void RowNumberingSeedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			RowSelectorSettings settingsOverride = (RowSelectorSettings)obj;
			settingsOverride.OnPropertyChanged("RowNumberingSeed");
		}

		#endregion // RowNumberingSeed
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