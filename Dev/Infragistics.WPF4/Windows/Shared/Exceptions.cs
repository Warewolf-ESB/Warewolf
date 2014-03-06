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
using System.Globalization;
using Infragistics.Controls;

namespace Infragistics
{
	#region InvalidEventNameException
	/// <summary>
	/// An <see cref="Exception"/> that is thrown when and invalid event name is given to a <see cref="CommandSource"/>
	/// </summary>
	public class InvalidEventNameException : Exception
	{
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidEventNameException"/> class.
		/// </summary>
		public InvalidEventNameException()
			: base()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidEventNameException"/> class.
		/// </summary>
		/// <param name="eventName"></param>
		/// <param name="elementType"></param>
		public InvalidEventNameException(string eventName, Type elementType)
			: base(string.Format(CultureInfo.CurrentCulture, SR.GetString("InvalidEventName"), "\"" + eventName + "\"", elementType.ToString()))
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidEventNameException"/> class.
		/// </summary>
		/// <param name="message">The message that should be displayed.</param>
		public InvalidEventNameException(string message):base(message)
			
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidEventNameException"/> class.
		/// </summary>
		/// <param name="message">The message that should be displayed.</param>
		/// <param name="innerException">An inner exception.</param>
		public InvalidEventNameException(string message, Exception innerException)
			: base(message, innerException)
		{

		}

		#endregion // Constructor

	}
	#endregion // InvalidEventNameException
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