using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;
using System.IO;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.OBJRecords
{
	// http://msdn.microsoft.com/en-us/library/dd924428(v=office.12).aspx
	internal class ObjFmla
	{
		private ObjectParsedFormula fmla;
		private PictFmlaEmbedInfo embedInfo;

		private ObjFmla() { }

		public Formula GetFormula()
		{
			if (this.fmla == null)
				return null;

			return this.fmla.GetFormula();
		}

		public static ObjFmla Load(Biff8RecordStream stream)
		{
			ObjFmla result = new ObjFmla();

			ushort cbFmla = stream.ReadUInt16();
			Debug.Assert(cbFmla % 2 == 0, "The length must be even.");

			long startPosition = stream.Position;

			if (cbFmla > 0)
			{
				result.fmla = ObjectParsedFormula.Load(stream);
				Formula formula = result.GetFormula();
				if (formula != null &&
					formula.PostfixTokenList.Count >= 1 &&
					formula.PostfixTokenList[0] is TblToken)
				{
					result.embedInfo = PictFmlaEmbedInfo.Load(stream);
				}
			}

			int remaining = cbFmla - (int)(stream.Position - startPosition);
			stream.ReadBytes(remaining);

			return result;
		}

		public void IterateFormulas(Worksheet owningWorksheet, Workbook.IterateFormulasCallback callback)
		{
			Formula formula = this.GetFormula();
			if (formula != null)
				callback(owningWorksheet, formula);
		}

		// MD 2/28/12 - 12.1 - Table Support
		//public void ResolveReferences(WorkbookSerializationManager manager)
		//{
		//    Formula formula = this.GetFormula();
		//    if (formula != null)
		//        formula.ResolveReferences(manager);
		//}

		public void Save(Biff8RecordStream stream)
		{
			Formula formula = this.GetFormula();
			bool saveEmbedInfo = 
				formula != null &&
				formula.PostfixTokenList.Count >= 1 &&
				formula.PostfixTokenList[0] is TblToken;

			if (saveEmbedInfo && this.embedInfo == null)
			{
				Utilities.DebugFail("The embedInfo field must not be null when the first ptg of the formula is a Tbl token.");
				return;
			}

			stream.Write((ushort)0); // temp cbFmla;

			if (this.fmla == null)
				return;

			long start = stream.Position;
			this.fmla.Save(stream);

			if (saveEmbedInfo)
				this.embedInfo.Save(stream);

			long end = stream.Position;
			long cbFmla = end - start;
			if (cbFmla % 4 != 0)
			{
				int padding = (int)(4 - (cbFmla % 4));
				stream.Write(new byte[padding]);
				cbFmla += padding;
			}

			stream.Position = start - 2;
			stream.Write((ushort)cbFmla); // cbFmla;

			stream.Seek(cbFmla, SeekOrigin.Current);
		}

		public PictFmlaEmbedInfo EmbedInfo
		{
			get { return this.embedInfo; }
		}

		public ObjectParsedFormula Fmla
		{
			get { return this.fmla; }
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