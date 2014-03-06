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

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A class that controls the base editor settings for an object on the <see cref="XamGrid"/>.
	/// </summary>
	public class EditingSettingsBase : SettingsBase 
	{				
		#region IsF2EditingEnabled

		/// <summary>
		/// Identifies the <see cref="IsF2EditingEnabled"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty IsF2EditingEnabledProperty = DependencyProperty.Register("IsF2EditingEnabled", typeof(bool), typeof(EditingSettingsBase), new PropertyMetadata(true, new PropertyChangedCallback(IsF2EditingEnabledChanged)));

		/// <summary>
		/// Gets/Sets if pressing the F2 key will cause the ActiveCell to enter edit mode.
		/// </summary>
		public bool IsF2EditingEnabled
		{
			get { return (bool)this.GetValue(IsF2EditingEnabledProperty); }
			set { this.SetValue(IsF2EditingEnabledProperty, value); }
		}

		private static void IsF2EditingEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			EditingSettingsBase settings = (EditingSettingsBase)obj;
			settings.OnPropertyChanged("IsF2EditingEnabled");
		}

		#endregion // IsF2EditingEnabled

		#region IsEnterKeyEditingEnabled

		/// <summary>
		/// Identifies the <see cref="IsEnterKeyEditingEnabled"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty IsEnterKeyEditingEnabledProperty = DependencyProperty.Register("IsEnterKeyEditingEnabled", typeof(bool), typeof(EditingSettingsBase), new PropertyMetadata(false, new PropertyChangedCallback(IsEnterKeyEditingEnabledChanged)));

		/// <summary>
		/// Gets/Sets if pressing the Enter key will cause the ActiveCell to enter edit mode.
		/// </summary>
		public bool IsEnterKeyEditingEnabled
		{
			get { return (bool)this.GetValue(IsEnterKeyEditingEnabledProperty); }
			set { this.SetValue(IsEnterKeyEditingEnabledProperty, value); }
		}

		private static void IsEnterKeyEditingEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			EditingSettingsBase settings = (EditingSettingsBase)obj;
			settings.OnPropertyChanged("IsEnterKeyEditingEnabled");
		}

		#endregion // IsEnterKeyEditingEnabled

		#region IsMouseActionEditingEnabled

		/// <summary>
		/// Identifies the <see cref="IsMouseActionEditingEnabled"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty IsMouseActionEditingEnabledProperty = DependencyProperty.Register("IsMouseActionEditingEnabled", typeof(MouseEditingAction), typeof(EditingSettingsBase), new PropertyMetadata(MouseEditingAction.DoubleClick, new PropertyChangedCallback(IsMouseActionEditingEnabledChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="MouseEditingAction"/> that can cause a <see cref="Cell"/> to enter edit mode.
		/// </summary>
		public MouseEditingAction IsMouseActionEditingEnabled
		{
			get { return (MouseEditingAction)this.GetValue(IsMouseActionEditingEnabledProperty); }
			set { this.SetValue(IsMouseActionEditingEnabledProperty, value); }
		}

		private static void IsMouseActionEditingEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			EditingSettingsBase settings = (EditingSettingsBase)obj;
			settings.OnPropertyChanged("IsMouseActionEditingEnabled");
		}

		#endregion // IsMouseActionEditingEnabled

		#region IsOnCellActiveEditingEnabled

		/// <summary>
		/// Identifies the <see cref="IsOnCellActiveEditingEnabled"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty IsOnCellActiveEditingEnabledProperty = DependencyProperty.Register("IsOnCellActiveEditingEnabled", typeof(bool), typeof(EditingSettingsBase), new PropertyMetadata(false, new PropertyChangedCallback(IsOnCellActiveEditingEnabledChanged)));

		/// <summary>
		/// Gets/Sets if a <see cref="Cell"/> will enter edit mode when it becomes active.
		/// </summary>
		public bool IsOnCellActiveEditingEnabled
		{
			get { return (bool)this.GetValue(IsOnCellActiveEditingEnabledProperty); }
			set { this.SetValue(IsOnCellActiveEditingEnabledProperty, value); }
		}

		private static void IsOnCellActiveEditingEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			EditingSettingsBase settings = (EditingSettingsBase)obj;
			settings.OnPropertyChanged("IsOnCellActiveEditingEnabled");
		}

		#endregion // IsOnCellActiveEditingEnabled
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