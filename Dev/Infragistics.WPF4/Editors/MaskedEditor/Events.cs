using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Shared;
using Infragistics.Windows;
using Infragistics.Windows.Editors;

namespace Infragistics.Windows.Editors.Events
{
	#region InvalidOperationEventArgs Class

	/// <summary>
	/// InvalidOperationEventArgs class for firing InvalidOperation event.
	/// </summary>
	internal class InvalidOperationEventArgs : System.EventArgs
	{
		private string message;
		private bool beep = true;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public InvalidOperationEventArgs( string message )
		{
			this.message = message;
		}

		/// <summary>
		/// gets the message associated with an illegal operation user was
		/// trying to perform
		/// </summary>
		public string Message
		{
			get
			{
				if ( null == this.message )
					return string.Empty;
				else
					return this.message;
			}
		}

		/// <summary>
		/// you can set this property to false to prevent the masked edit from 
		/// beeping.
		/// </summary>
		public bool Beep
		{
			get
			{
				return this.beep;
			}
			set
			{
				this.beep = value;
			}
		}
	}

	#endregion // InvalidOperationEventArgs Class

	#region InvalidCharEventArgs Class

	/// <summary>
	/// InvalidCharEventArgs class for firing <see cref="XamMaskedEditor.InvalidChar"/> event of the XamMaskedEditor.
	/// </summary>
	/// <remarks>
	/// <seealso cref="XamMaskedEditor.InvalidChar"/>
	/// </remarks>
	public class InvalidCharEventArgs : System.EventArgs
	{
		private char c;
		private bool beep = true;
		private DisplayCharBase dc;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="c">Invalid character that was typed</param>
		/// <param name="dc">The display character placeholder where the invalid character was typed</param>
		public InvalidCharEventArgs( char c, DisplayCharBase dc )
		{
			this.c = c;
			this.dc = dc;
		}

		/// <summary>
		/// Gets the invalid character that the user attempted to input.
		/// </summary>
		public Char Char
		{
			get
			{
				return this.c;
			}
		}

		/// <summary>
		/// Gets the display character instance where the user attempted to enter the invalid character.
		/// </summary>
		public DisplayCharBase DisplayChar
		{
			get
			{
				return this.dc;
			}
		}


		/// <summary>
		/// Gets or sets a value indicating whether the XamMaskedEditor should beep.
		/// </summary>
		public bool Beep
		{
			get
			{
				return this.beep;
			}
			set
			{
				this.beep = value;
			}
		}
	}

	#endregion // InvalidCharEventArgs Class

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