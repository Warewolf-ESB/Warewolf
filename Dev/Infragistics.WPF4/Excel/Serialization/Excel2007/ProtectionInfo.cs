using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Documents.Excel.Serialization.Excel2007
{





    internal class ProtectionInfo
    {
        #region Members

        private bool hidden = ProtectionInfo.DEFAULT_HIDDEN;
        private bool locked = ProtectionInfo.DEFAULT_LOCKED;

        #endregion Members

        #region Constants

        internal const bool DEFAULT_HIDDEN = false;
        internal const bool DEFAULT_LOCKED = true;

        #endregion Constants

		// MD 1/8/12 - 12.1 - Cell Format Updates
		#region Base Class Overrides

		#region Equals

		public override bool Equals(object obj)
		{
			return ProtectionInfo.HasSameData(this, obj as ProtectionInfo);
		}

		#endregion // Equals

		// MD 1/9/12 - 12.1 - Cell Format Updates
		#region GetHashCode

		public override int GetHashCode()
		{
			int hashCode = 0;

			hashCode ^= Convert.ToInt32(this.hidden);
			hashCode ^= Convert.ToInt32(this.locked) << 1;

			return hashCode;
		}

		#endregion  // GetHashCode

		#endregion // Base Class Overrides

        #region Properties

        #region Hidden






        public bool Hidden
        {
            get { return this.hidden; }
            set { this.hidden = value; }
        }

        #endregion Hidden

        #region Locked






        public bool Locked
        {
            get { return this.locked; }
            set { this.locked = value; }
        }

        #endregion Locked

        #region IsDefault






        public bool IsDefault
        {
            get
            {
                return (this.hidden == ProtectionInfo.DEFAULT_HIDDEN &&
                    this.locked == ProtectionInfo.DEFAULT_LOCKED);
            }
        }


        #endregion IsDefault

        #endregion Properties

        #region Methods

		// MD 12/21/11 - 12.1 - Table Support
		// Moved this code from the FormatInfo.CreateWorksheetCellFormatData so it can be used in other places.
		#region ApplyTo

		internal void ApplyTo(WorksheetCellFormatData formatData)
		{
			formatData.Locked = (this.Locked) ? ExcelDefaultableBoolean.True : ExcelDefaultableBoolean.False;
		}

		#endregion // ApplyTo

		// MD 12/30/11 - 12.1 - Table Support
		#region CreateProtectionInfo

		public static ProtectionInfo CreateProtectionInfo(WorksheetCellFormatData formatData)
		{
			// MD 1/8/12 - 12.1 - Cell Format Updates
			// Moved all code to a new override.
			return ProtectionInfo.CreateProtectionInfo(formatData, false);
		}

		// MD 1/8/12 - 12.1 - Cell Format Updates
		// Added a new override.
		public static ProtectionInfo CreateProtectionInfo(WorksheetCellFormatData formatData, bool forceCreate)
		{
			// MD 1/8/12 - 12.1 - Cell Format Updates
			// If we don't need to force create and the locked value is default, return null.
			ExcelDefaultableBoolean locked = formatData.LockedResolved;
			if (forceCreate == false && locked == ExcelDefaultableBoolean.True)
				return null;

			ProtectionInfo protectionInfo = new ProtectionInfo();
			protectionInfo.Locked = (locked == ExcelDefaultableBoolean.True);
			return protectionInfo;
		}

		#endregion // CreateProtectionInfo

		#region HasSameData

		internal static bool HasSameData(ProtectionInfo prot1, ProtectionInfo prot2)
		{
			if (ReferenceEquals(prot1, null) &&
				ReferenceEquals(prot2, null))
				return true;
			if (ReferenceEquals(prot1, null) ||
				ReferenceEquals(prot2, null))
				return false;
			return (prot1.hidden == prot2.hidden &&
				prot1.locked == prot2.locked);
		}

		#endregion // HasSameData

        #endregion Methods
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