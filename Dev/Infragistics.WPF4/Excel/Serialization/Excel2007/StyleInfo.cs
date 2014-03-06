using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Documents.Excel.Serialization.Excel2007
{





    internal class StyleInfo
    {

        #region Members

        private int builtinId = -1;
        private bool customBuiltin = false;
        private bool hidden = false;
        // 8/27/08 CDS
        // OutlineStyle gets converted to a byte where 0 is the default
        //private int outlineStyle = -1;
        private int outlineStyle = 0;
        private string name = string.Empty;
        private int xfId = -1;

        #endregion Members

		// MD 12/31/11 - 12.1 - Cell Format Updates
		#region Constructor

		public StyleInfo() { }

		// MD 1/10/12 - 12.1 - Cell Format Updates
		// The manager is now needed by the constructor.
		//public StyleInfo(WorkbookStyle style)
		public StyleInfo(WorkbookSerializationManager manager, WorkbookStyle style, bool isHidden)
		{
			this.Name = style.Name;

			// MD 1/10/12 - 12.1 - Cell Format Updates
			// We no longer cache format indexes because we can easily get them at save time.
			//this.CellStyleXfId = style.StyleFormatInternal.IndexInXfsCollection;
			this.CellStyleXfId = manager.GetStyleFormatIndex(style);

			WorkbookBuiltInStyle builtInStyle = style as WorkbookBuiltInStyle;
			if (builtInStyle != null)
			{
				this.OutlineStyle = Convert.ToInt32(builtInStyle.OutlineLevel);
				this.BuiltinId = (int)builtInStyle.Type;
				this.CustomBuiltin = builtInStyle.IsCustomized;

				// MD 2/4/12 - 12.1 - Cell Format Updates
				this.Hidden = isHidden;
			}
		}

		#endregion // Constructor

        #region Properties

        #region BuiltinId






        public int BuiltinId
        {
            get { return this.builtinId; }
            set { this.builtinId = value; }
        }

        #endregion BuiltinId

        #region CustomBuiltin






        public bool CustomBuiltin
        {
            get { return this.customBuiltin; }
            set { this.customBuiltin = value; }
        }

        #endregion CustomBuiltin

        #region Hidden






        public bool Hidden
        {
            get { return this.hidden; }
            set { this.hidden = value; }
        }

        #endregion Hidden

        #region OutlineStyle







        public int OutlineStyle
        {
            get { return this.outlineStyle; }
            set { this.outlineStyle = value; }
        }

        #endregion OutlineStyle

        #region Name






        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        #endregion Name

        #region CellStyleXfId






        public int CellStyleXfId
        {
            get { return this.xfId; }
            set { this.xfId = value; }
        }

        #endregion CellStyleXfId

        #endregion Properties

        #region Methods

        internal static bool HasSameData(StyleInfo style1, StyleInfo style2)
        {
            if (ReferenceEquals(style1, null) &&
                ReferenceEquals(style2, null))
                return true;
            if (ReferenceEquals(style1, null) ||
                ReferenceEquals(style2, null))
                return false;
            return (style1.builtinId == style2.builtinId &&
                style1.customBuiltin == style2.customBuiltin &&
                style1.hidden == style2.hidden &&
                style1.name == style2.name &&
                style1.outlineStyle == style2.outlineStyle &&
                style1.xfId == style2.xfId);
        }

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