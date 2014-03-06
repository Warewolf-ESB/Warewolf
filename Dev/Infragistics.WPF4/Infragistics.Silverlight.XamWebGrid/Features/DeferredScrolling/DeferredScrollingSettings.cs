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
using System.ComponentModel;
using System.Windows.Controls.Primitives;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// An object that contains settings for using DeferredScrolling on the <see cref="XamGrid"/>
	/// </summary>
	public class DeferredScrollingSettings : SettingsBase
	{
		#region Properties
			
		#region AllowDeferredScrolling
        
        /// <summary>
        /// Identifies the <see cref="AllowDeferredScrolling"/> dependency property. 
        /// </summary>
		public static readonly DependencyProperty AllowDeferredScrollingProperty = DependencyProperty.Register("AllowDeferredScrolling", typeof(DeferredScrollingType), typeof(DeferredScrollingSettings), new PropertyMetadata(DeferredScrollingType.None, new PropertyChangedCallback(AllowDeferredScrollingChanged)));

		/// <summary>
		/// Gets/Sets whether DeferredScrolling is enabled. 
		/// </summary>
		public DeferredScrollingType AllowDeferredScrolling
		{
			get { return (DeferredScrollingType)this.GetValue(AllowDeferredScrollingProperty); }
			set { this.SetValue(AllowDeferredScrollingProperty, value); }
		}

		private static void AllowDeferredScrollingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{ 

		}

		#endregion // AllowDeferredScrolling 
				
		#region DeferredScrollTemplate

		/// <summary>
		/// Identifies the <see cref="DeferredScrollTemplate"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty DeferredScrollTemplateProperty = DependencyProperty.Register("DeferredScrollTemplate", typeof(DataTemplate), typeof(DeferredScrollingSettings), null);

		/// <summary>
		/// Gets/Sets the <see cref="DataTemplate"/> that will be displayed while scrolling when deferred scrolling is enabled. 
		/// </summary>
		/// <remarks>
		/// Note: the DataContext of the <see cref="DataTemplate"/> will be a <see cref="ScrollTipInfo"/>.
		/// </remarks>
		public DataTemplate DeferredScrollTemplate
		{
			get { return (DataTemplate)this.GetValue(DeferredScrollTemplateProperty); }
			set { this.SetValue(DeferredScrollTemplateProperty, value); }
		}

		#endregion // DeferredScrollTemplate

		#region DefaultColumnKey

		/// <summary>
		/// Identifies the <see cref="DefaultColumnKey"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty DefaultColumnKeyProperty = DependencyProperty.Register("DefaultColumnKey", typeof(string), typeof(DeferredScrollingSettings), new PropertyMetadata(null, new PropertyChangedCallback(DefaultColumnKeyChanged)));

		/// <summary>
		/// Gets/Sets the key of the <see cref="Column"/> that data will be pulled from when the <see cref="DeferredScrollingSettings.DeferredScrollTemplate"/> property isn't set for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		public string DefaultColumnKey
		{
			get { return (string)this.GetValue(DefaultColumnKeyProperty); }
			set { this.SetValue(DefaultColumnKeyProperty, value); }
		}

		private static void DefaultColumnKeyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			DeferredScrollingSettings settings = (DeferredScrollingSettings)obj;
			settings.OnPropertyChanged("DefaultColumnKey");
		}

		#endregion // DefaultColumnKey 

		#region GroupByDeferredScrollTemplate

		/// <summary>
		/// Identifies the <see cref="GroupByDeferredScrollTemplate"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty GroupByDeferredScrollTemplateProperty = DependencyProperty.Register("GroupByDeferredScrollTemplate", typeof(DataTemplate), typeof(DeferredScrollingSettings), new PropertyMetadata(null, new PropertyChangedCallback(GroupByDeferredScrollTemplateChanged)));

		/// <summary>
		/// Gets/Sets  <see cref="DataTemplate"/> that will be displayed while scrolling through GroupBy rows for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		/// <remarks>
		/// Note: the DataContext of the <see cref="DataTemplate"/> will be a <see cref="ScrollTipInfo"/>.
		/// </remarks>
		public DataTemplate GroupByDeferredScrollTemplate
		{
			get { return (DataTemplate)this.GetValue(GroupByDeferredScrollTemplateProperty); }
			set { this.SetValue(GroupByDeferredScrollTemplateProperty, value); }
		}

		private static void GroupByDeferredScrollTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			DeferredScrollingSettings settings = (DeferredScrollingSettings)obj;
			settings.OnPropertyChanged("GroupByDeferredScrollTemplate");
		}

		#endregion // GroupByDeferredScrollTemplate 

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