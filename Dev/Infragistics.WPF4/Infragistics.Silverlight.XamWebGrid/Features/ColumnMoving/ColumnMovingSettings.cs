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
	/// An object that contains settings for using ColumnMovingSettings on the <see cref="XamGrid"/>
	/// </summary>
	public class ColumnMovingSettings : SettingsBase
	{
		#region IndicatorStyle

		/// <summary>
		/// Identifies the <see cref="IndicatorStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty IndicatorStyleProperty = DependencyProperty.Register("IndicatorStyle", typeof(Style), typeof(ColumnMovingSettings), new PropertyMetadata(new PropertyChangedCallback(IndicatorStyleChanged)));

		/// <summary>
		/// Gets/sets the <see cref="Style"/> that will be used to identify where a <see cref="Column"/> will be moved.
		/// </summary>
		public Style IndicatorStyle
		{
			get { return (Style)this.GetValue(IndicatorStyleProperty); }
			set { this.SetValue(IndicatorStyleProperty, value); }
		}

		private static void IndicatorStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnMovingSettings settings = (ColumnMovingSettings)obj;
			settings.OnPropertyChanged("IndicatorStyle");
		}

		#endregion // IndicatorStyle

		#region AllowColumnMoving

		/// <summary>
		/// Identifies the <see cref="AllowColumnMoving"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowColumnMovingTypeProperty = DependencyProperty.Register("AllowColumnMoving", typeof(ColumnMovingType), typeof(ColumnMovingSettings), new PropertyMetadata(ColumnMovingType.Disabled, new PropertyChangedCallback(AllowColumnMovingChanged)));

		/// <summary>
		/// Gets/sets how column moving will work for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		public ColumnMovingType AllowColumnMoving
		{
			get { return (ColumnMovingType)this.GetValue(AllowColumnMovingTypeProperty); }
			set { this.SetValue(AllowColumnMovingTypeProperty, value); }
		}

		private static void AllowColumnMovingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnMovingSettings settings = (ColumnMovingSettings)obj;
			settings.OnPropertyChanged("AllowColumnMoving");
		}

		#endregion // AllowColumnMoving

		#region EasingFunction

		/// <summary>
		/// Identifies the <see cref="EasingFunction"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty EasingFunctionProperty = DependencyProperty.Register("EasingFunction", typeof(EasingFunctionBase), typeof(ColumnMovingSettings), new PropertyMetadata(new SineEase() { EasingMode = EasingMode.EaseInOut }, new PropertyChangedCallback(EasingFunctionChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="EasingFunctionBase"/> that will be used to animate the immediate moving of columns.
		/// </summary>
		public EasingFunctionBase EasingFunction
		{
			get { return (EasingFunctionBase)this.GetValue(EasingFunctionProperty); }
			set { this.SetValue(EasingFunctionProperty, value); }
		}

		private static void EasingFunctionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnMovingSettings settings = (ColumnMovingSettings)obj;
			settings.OnPropertyChanged("EasingFunction");
		}

		#endregion // EasingFunction 
				
		#region AnimationDuration

		/// <summary>
		/// Identifies the <see cref="AnimationDuration"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AnimationDurationProperty = DependencyProperty.Register("AnimationDuration", typeof(int), typeof(ColumnMovingSettings), new PropertyMetadata(8, new PropertyChangedCallback(AnimationDurationChanged)));

		/// <summary>
		/// Gets/sets the duration of the animation of moving a column immediately. 
		/// </summary>
		public int AnimationDuration
		{
			get { return (int)this.GetValue(AnimationDurationProperty); }
			set { this.SetValue(AnimationDurationProperty, value); }
		}

		private static void AnimationDurationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnMovingSettings settings = (ColumnMovingSettings)obj;
			settings.OnPropertyChanged("AnimationDuration");
		}

		#endregion // AnimationDuration 
				        
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