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

namespace Infragistics.Controls.Primitives
{
	/// <summary>
	/// A custom textbox that is embedded within another element and does not provide its own chrome or any state based visual changes.
	/// </summary>

	[DesignTimeVisible(false)]

	public class EmbeddedTextBox : TextBox
	{
		#region Constructor
		static EmbeddedTextBox()
		{

			EmbeddedTextBox.DefaultStyleKeyProperty.OverrideMetadata(typeof(EmbeddedTextBox), new FrameworkPropertyMetadata(typeof(EmbeddedTextBox)));

		}

		/// <summary>
		/// Initializes a new <see cref="EmbeddedTextBox"/>
		/// </summary>
		public EmbeddedTextBox()
		{



			this.AddHandler(CommandManager.CanExecuteEvent, new CanExecuteRoutedEventHandler(this.OnCanExecuteEvent), true);

		}
		#endregion //Constructor


		#region OnCanExecuteEvent

		private void OnCanExecuteEvent(object sender, CanExecuteRoutedEventArgs e)
		{
			// If we disable the undo events, allow the KeyDown event to fire so we can process them manually if we want.
			if (e.OriginalSource == this && e.Handled == false)
			{
				e.CanExecute = true;
				e.ContinueRouting = true;
			}
		}

		#endregion  // OnCanExecuteEvent




#region Infragistics Source Cleanup (Region)





















































#endregion // Infragistics Source Cleanup (Region)

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