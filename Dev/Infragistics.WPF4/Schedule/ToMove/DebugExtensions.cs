



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
using System.Diagnostics;

namespace Infragistics
{
	internal static class DebugHelper
	{
		private static int _indent = 0;

		[Conditional("DEBUG")]
		public static void Indent()
		{
			_indent++;
		}

		[Conditional("DEBUG")]
		public static void Unindent()
		{
			Debug.Assert(_indent > 0);
			_indent--;
		}

		[Conditional("DEBUG_LAYOUT")]
		public static void DebugLayout(FrameworkElement element, string category, string message, params object[] messageArgs)
		{
			DebugLayout(element, false, false, category, message, messageArgs);
		}

		[Conditional("DEBUG_LAYOUT")]
		public static void DebugLayout(FrameworkElement element, bool unindentBefore, bool indentAfter, string category, string message, params object[] messageArgs)
		{
			string templatedParent = null;


			if (null != element && null != element.TemplatedParent)
				templatedParent = element.TemplatedParent.GetHashCode().ToString();

			if (message == null)
				message = string.Empty;

			if (unindentBefore)
				Unindent();

			Debug.WriteLine(string.Format("{0}[{1} - {2}:{3} TP:{4}] {5} - {6}", 
				new string(' ', _indent * 3),
				DateTime.Now.ToString("hh:mm:ss:ffffff"),
				element.GetType().Name,
				element.GetHashCode(), 
				templatedParent, 
				category, 
				string.Format(message, messageArgs)));

			if (indentAfter)
				Indent();
		}
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