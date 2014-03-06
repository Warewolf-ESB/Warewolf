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




using Infragistics.Windows.Controls;


namespace Infragistics.Controls.Schedules.Primitives
{
	#region Internal ComboBoxProxy Class
	internal class ComboBoxProxy
	{
		#region Member Variables




		private ComboBox							_comboBox;


		#endregion //Member Variables

		#region Constructor
		internal ComboBoxProxy(FrameworkElement comboBox)
		{
			CoreUtilities.ValidateNotNull(comboBox);

			// Hook up event listeners.
			comboBox.LostFocus	+=new RoutedEventHandler(comboBox_LostFocus);
			comboBox.KeyUp		+= new KeyEventHandler(comboBox_KeyUp);


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

			this._comboBox = comboBox as ComboBox;
			if (this._comboBox == null)
				throw new ArgumentException(ScheduleUtilities.GetString("LE_MustBeOfType", "ComboBox", "ComboBox"), "comboBox");

			this._comboBox.SelectionChanged	+= new SelectionChangedEventHandler(comboBox_SelectionChanged);
			this._comboBox.DropDownClosed	+= new EventHandler(OnComboDropDownClosed);
			this._comboBox.DropDownOpened	+= new EventHandler(OnComboDropDownOpened);

		}
		#endregion //Constructor

		#region Events

		#region DropDownClosed
		internal event			ProxyDropDownClosedEventHandler DropDownClosed;
		internal delegate void	ProxyDropDownClosedEventHandler(object sender, EventArgs e);
		private void RaiseDropDownClosed()
		{
			if (DropDownClosed != null)
				DropDownClosed(this, EventArgs.Empty);
		}
		#endregion DropDownClosed

		#region DropDownOpened
		internal event			ProxyDropDownOpenedEventHandler DropDownOpened;
		internal delegate void	ProxyDropDownOpenedEventHandler(object sender, EventArgs e);
		private void RaiseDropDownOpened()
		{
			if (DropDownOpened != null)
				DropDownOpened(this, EventArgs.Empty);
		}
		#endregion DropDownOpened

		#region KeyUp
		internal event			KeyEventHandler KeyUp;
		private void RaiseKeyUp(KeyEventArgs e)
		{
			if (KeyUp != null)
				KeyUp(this, e);
		}
		#endregion KeyUp

		#region LostFocus
		internal event			ProxyLostFocusEventHandler LostFocus;
		internal delegate void	ProxyLostFocusEventHandler(object sender, EventArgs e);
		private void RaiseLostFocus()
		{
			if (LostFocus != null)
				LostFocus(this, EventArgs.Empty);
		}
		#endregion LostFocus

		#region SelectionChanged
		internal event			ProxySelectionChangedEventHandler SelectionChanged;
		internal delegate void	ProxySelectionChangedEventHandler(object sender, EventArgs e);
		private void RaiseSelectionChanged()
		{
			if (SelectionChanged != null)
				SelectionChanged(this, EventArgs.Empty);
		}
		#endregion SelectionChanged

		#endregion //Events

		#region Event Handlers

		void comboBox_KeyUp(object sender, KeyEventArgs e)
		{
			this.RaiseKeyUp(e);
		}

		void comboBox_LostFocus(object sender, RoutedEventArgs e)
		{
			this.RaiseLostFocus();
		}

		void OnComboDropDownClosed(object sender, EventArgs e)
		{
			this.RaiseDropDownClosed();
		}

		void OnComboDropDownOpened(object sender, EventArgs e)
		{
			this.RaiseDropDownOpened();
		}







		void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			this.RaiseSelectionChanged();
		}


		#endregion //Event Handlers

		#region Properties

		#region Internal Properties

		#region ComboBoxControl
		internal Control ComboBoxControl
		{

			get
			{

				return this._comboBox;



			}
		}
		#endregion //ComboBoxControl

		#region IsEnabled
		internal bool IsEnabled
		{
			get
			{



				return this._comboBox.IsEnabled;

			}
			set
			{



				this._comboBox.IsEnabled = value;

			}
		}
		#endregion //IsEnabled

		#region Items
		internal IList Items
		{
			get
			{



				return this._comboBox.Items;

			}
		}
		#endregion //Items

		#region ItemsSource
		internal IEnumerable ItemsSource
		{
			get
			{



				return this._comboBox.ItemsSource;

			}
			set
			{



				this._comboBox.ItemsSource = value;

			}
		}
		#endregion //ItemsSource

		#region ItemTemplate
		internal DataTemplate ItemTemplate
		{
			get
			{



				return this._comboBox.ItemTemplate;

			}
			set
			{



				this._comboBox.ItemTemplate = value;

			}
		}
		#endregion //ItemTemplate

		#region SelectedIndex
		internal int SelectedIndex
		{
			get
			{



				return this._comboBox.SelectedIndex;

			}
			set
			{



				this._comboBox.SelectedIndex = value;

			}
		}
		#endregion //SelectedIndex

		#region SelectedItem
		internal object SelectedItem
		{
			get
			{



				return this._comboBox.SelectedItem;

			}
			set
			{



				this._comboBox.SelectedItem = value;

			}
		}
		#endregion //SelectedItem

		#region Text
		internal string Text
		{
			get
			{



				return this._comboBox.Text;

			}
			set
			{



				this._comboBox.Text = value;

			}
		}
		#endregion //Text

		#endregion //Internal Properties

		#endregion //Properties
	}
	#endregion //Internal ComboBoxProxy Class
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