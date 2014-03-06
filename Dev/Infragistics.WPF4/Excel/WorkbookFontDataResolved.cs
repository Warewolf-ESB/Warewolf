using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using Infragistics.Documents.Excel.Serialization.BIFF8;




using System.Drawing;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 1/8/12 - 12.1 - Cell Format Updates
	internal sealed class WorkbookFontDataResolved :
		IWorkbookFont
	{
		#region Member Variables

		private WorksheetCellFormatProxy _proxy;

		#endregion // Member Variables

		#region Constructor

		public WorkbookFontDataResolved(WorksheetCellFormatProxy proxy)
		{
			_proxy = proxy;
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region Equals

		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(this, obj))
				return true;

			WorkbookFontDataResolved other = obj as WorkbookFontDataResolved;
			if (other == null)
				return false;

			return _proxy.Equals(other._proxy);
		}

		#endregion // Equals

		#region GetHashCode

		public override int GetHashCode()
		{
			return _proxy.GetHashCode();
		}

		#endregion // GetHashCode

		#endregion // Base Class Overrides

		#region Interfaces

		#region IWorkbookFont Members

		void IWorkbookFont.SetFontFormatting(IWorkbookFont source)
		{
			this.ThrowOnSet();
		}

		ExcelDefaultableBoolean IWorkbookFont.Bold
		{
			get
			{
				return _proxy.Element.FontBoldResolved;
			}
			set
			{
				this.ThrowOnSet();
			}
		}

		Color IWorkbookFont.Color
		{
			get
			{
				return _proxy.Element.FontColorInfoResolved.GetResolvedColor(_proxy.Element.Workbook);
			}
			set
			{
				this.ThrowOnSet();
			}
		}

		WorkbookColorInfo IWorkbookFont.ColorInfo
		{
			get
			{
				return _proxy.Element.FontColorInfoResolved;
			}
			set
			{
				this.ThrowOnSet();
			}
		}

		int IWorkbookFont.Height
		{
			get
			{
				return _proxy.Element.FontHeightResolved;
			}
			set
			{
				this.ThrowOnSet();
			}
		}

		ExcelDefaultableBoolean IWorkbookFont.Italic
		{
			get
			{
				return _proxy.Element.FontItalicResolved;
			}
			set
			{
				this.ThrowOnSet();
			}
		}

		string IWorkbookFont.Name
		{
			get
			{
				return _proxy.Element.FontNameResolved;
			}
			set
			{
				this.ThrowOnSet();
			}
		}

		ExcelDefaultableBoolean IWorkbookFont.Strikeout
		{
			get
			{
				return _proxy.Element.FontStrikeoutResolved;
			}
			set
			{
				this.ThrowOnSet();
			}
		}

		FontSuperscriptSubscriptStyle IWorkbookFont.SuperscriptSubscriptStyle
		{
			get
			{
				return _proxy.Element.FontSuperscriptSubscriptStyleResolved;
			}
			set
			{
				this.ThrowOnSet();
			}
		}

		FontUnderlineStyle IWorkbookFont.UnderlineStyle
		{
			get
			{
				return _proxy.Element.FontUnderlineStyleResolved;
			}
			set
			{
				this.ThrowOnSet();
			}
		}

		#endregion

		#endregion // Interfaces

		#region Methods

		#region ThrowOnSet

		private void ThrowOnSet()
		{
			throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_ResolvedFormatCannotBeModified"));
		}

		#endregion // ThrowOnSet

		#endregion // Methods
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