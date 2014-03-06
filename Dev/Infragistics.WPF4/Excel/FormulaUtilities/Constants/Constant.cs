using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using Infragistics.Documents.Excel.Serialization;

namespace Infragistics.Documents.Excel.FormulaUtilities.Constants
{
	internal abstract class Constant
	{
		// MD 8/18/08 - Excel formula solving
		public abstract object CalcValue { get; }

		// MD 10/22/10 - TFS36696
		// All constants are basically immutable, so we don't need to clone them.
		//public abstract Constant Clone();

		// MD 9/23/09 - TFS19150
		// Every read operation is relatively slow, so the buffer is now cached and passed into this method so we can get values from it.
		//public abstract void Load( BiffRecordStream stream );
		public abstract void Load( BiffRecordStream stream, ref byte[] data, ref int dataIndex );

		public abstract void Save( BiffRecordStream stream );

		// MD 4/6/12 - TFS101506
		public abstract string ToString(CultureInfo culture);

		public abstract byte ConstantCode { get;}

		// MD 9/23/09 - TFS19150
		// Every read operation is relatively slow, so the buffer is now cached and passed into this method so we can get values from it.
		//public static Constant GetNextConstant( BiffRecordStream stream )
		public static Constant GetNextConstant( BiffRecordStream stream, ref byte[] data, ref int dataIndex )
		{
			byte constantCode = stream.ReadByteFromBuffer( ref data, ref dataIndex );

			switch ( constantCode )
			{
				case 0:		return new EmptyConstant();
				case 1:		return new NumberConstant();
				case 2:		return new StringConstant();
				case 4:		return new BooleanConstant();
				case 16:	return new ErrorConstant();

				default:
					Utilities.DebugFail( "Unknown constant type." );
					return null;
			}
		}

		// MD 4/6/12 - TFS101506
		public sealed override string ToString()
		{
			Utilities.DebugFail("The overload which takes a culture should be used.");
			return this.ToString(CultureInfo.CurrentCulture);
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