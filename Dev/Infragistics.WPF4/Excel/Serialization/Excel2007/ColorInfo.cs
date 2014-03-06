using System;
using System.Collections.Generic;
using System.Text;





using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007
{





    internal class ColorInfo
    {
        #region Private Members

        private ExcelDefaultableBoolean auto = ExcelDefaultableBoolean.Default;
        private uint? indexed = null;
        private Color rgb = Utilities.ColorEmpty;
        private uint? theme = null;
        private double tint = 0.0;

		// MD 1/19/11 - TFS62268
		// Keep a reference to the related font data object, if this ColorInfo is used for font data.
		private WorkbookFontData fontDataObject;

        #endregion Private Members

		#region Base Class Overrides

		// MD 3/4/12 - 12.1 - Table Support
		#region Equals

		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(this, obj))
				return true;

			ColorInfo other = obj as ColorInfo;
			if (other == null)
				return false;

			return (this.auto == other.auto &&
				this.indexed == other.indexed &&
				this.rgb.Equals(other.rgb) &&
				this.theme == other.theme &&
				this.tint == other.tint);
		}

		#endregion // Equals

		// MD 3/4/12 - 12.1 - Table Support
		#region GetHashCode

		public override int GetHashCode()
		{
			return
				this.auto.GetHashCode() ^
				this.indexed.GetHashCode() ^
				this.rgb.GetHashCode() ^
				this.theme.GetHashCode() ^
				this.tint.GetHashCode();
		}

		#endregion // GetHashCode

		#endregion // Base Class Overrides

        #region Properties

        #region Auto






        public ExcelDefaultableBoolean Auto 
        {
            get { return this.auto; }
            set { this.auto = value; }
        }

        #endregion Auto

        #region Indexed






        public uint? Indexed
        {
            get { return this.indexed; }
            set { this.indexed = value; }
        }

        #endregion Indexed

        #region RGB






        public Color RGB
        {
            get { return this.rgb; }
            set { this.rgb = value; }
        }

        #endregion RGB

        #region Theme






        public uint? Theme
        {
            get { return this.theme; }
            set { this.theme = value; }
        }

        #endregion Theme

        #region Tint






        public double Tint
        {
            get { return this.tint; }
            set { this.tint = value; }
        }

        #endregion Tint

        #region IsDefault






        public bool IsDefault
        {
            get
            {
				return (this.auto == ExcelDefaultableBoolean.Default &&
                        this.indexed == null &&
                        this.rgb == Utilities.ColorEmpty &&
                        this.theme == null &&
                        this.tint == 0.0);
            }
        }

        #endregion IsDefault

		// MD 1/19/11 - TFS62268
		#region FontDataObject

		public WorkbookFontData FontDataObject
		{
			get { return this.fontDataObject; }
			set { this.fontDataObject = value; }
		}

		#endregion // FontDataObject

        #endregion Properties

        #region Methods

		// MD 1/17/12 - 12.1 - Cell Format Updates
		#region CreateColorInfo

		public static ColorInfo CreateColorInfo(WorkbookSerializationManager manager, WorkbookColorInfo workbookColorInfo, ColorableItem item)
		{
			ColorInfo colorInfo = new ColorInfo();

			if (workbookColorInfo != null)
			{
				if (workbookColorInfo.IsAutomatic)
					colorInfo.Auto = ExcelDefaultableBoolean.True;

				if (workbookColorInfo.Color.HasValue)
				{
					Color color = workbookColorInfo.Color.Value;

					if (Utilities.ColorIsSystem(color))
						colorInfo.Indexed = (uint)manager.Workbook.Palette.FindIndex(color, item);
					else
						colorInfo.RGB = color;
				}

				if (workbookColorInfo.ThemeColorType.HasValue)
					colorInfo.Theme = (uint)workbookColorInfo.ThemeColorType.Value;

				if (workbookColorInfo.Tint.HasValue)
					colorInfo.Tint = workbookColorInfo.Tint.Value;
			}

			return colorInfo;
		}

		#endregion // CreateColorInfo

		// MD 3/4/12 - 12.1 - Table Support
		// Removed and replaced with Equals and GetHashCode overrides.
		#region Removed

		//#region HasSameData

		//internal static bool HasSameData(ColorInfo color1, ColorInfo color2)
		//{
		//    // 09/18/08 CDS - Assume a default ColorInfo object is equivalent to null as ColorInfo objects
		//    // are sometimes created by default, and sometimes created only when properties are going to be set.
		//    color1 = (color1 != null && color1.IsDefault) ? null : color1;
		//    color2 = (color2 != null && color2.IsDefault) ? null : color2;

		//    if (ReferenceEquals(color1, null) &&
		//        ReferenceEquals(color2, null))
		//        return true;
		//    if (ReferenceEquals(color1, null) ||
		//        ReferenceEquals(color2, null))
		//        return false;

		//    return (color1.auto == color2.auto &&
		//        color1.indexed == color2.indexed &&
		//        color1.rgb.Equals(color2.rgb) &&
		//        color1.theme == color2.theme &&
		//        color1.tint == color2.tint);
		//}

		//#endregion HasSameData

		#endregion // Removed

		// MD 1/24/12 - 12.1 - Cell Format Updates
		// This is no longer needed.
		#region Removed

		//#region ResolveColor

		//// MD 11/29/11 - TFS96205
		////internal Color ResolveColor( Excel2007WorkbookSerializationManager manager )
		//internal Color ResolveColor(WorkbookSerializationManager manager)
		//{
		//    // MD 4/29/11 - TFS73906
		//    // We no longer need an overload with the isForFont flag.
		////    // MD 9/2/08 - Styles
		////    // Moved all code to the new overload
		////    return this.ResolveColor( manager, false );
		////}
		////
		////// MD 9/2/08 - Styles
		////// Added new overload because the processing is sllightly different for fonts
		////internal Color ResolveColor( Excel2007WorkbookSerializationManager manager, bool isForFont )
		////{
		//    Color baseColor = Utilities.ColorEmpty;
		//    if (manager != null)
		//    {
		//        if (this.theme != null &&
		//            // MD 11/29/11
		//            // Found while fixing TFS96205
		//            // This is an off by one error.
		//            //this.theme <= manager.ThemeColors.Count)
		//            // MD 1/16/12 - 12.1 - Cell Format Updates
		//            //this.theme < manager.ThemeColors.Count)
		//            this.theme < manager.Workbook.ThemeColors.Length)
		//        {
		//            // MD 1/18/12 - 12.1 - Cell Format Updates
		//            #region Old Code

		//            //// MD 4/29/11 - TFS73906
		//            //// It looks like the 1st and 2nd and 3rd and 4th theme colors are swapped, and not just for fonts either.
		//            ////// 8/25/08 - CDS
		//            ////// The index passed is 1-based, so we need to subtract 1 when accessing the color list
		//            //////baseColor = manager.ThemeColors[(int)this.theme];
		//            ////// MD 9/2/08 - Styles
		//            ////// The theme index is actually 0-based, but the Window and WindowText system colors seem to be reversed for fonts.
		//            //////baseColor = manager.ThemeColors[(int)this.theme - 1];
		//            ////baseColor = manager.ThemeColors[ (int)this.theme ];
		//            ////
		//            ////// MD 9/2/08 - Styles
		//            ////// The Window and WindowText system colors seem to be reversed for fonts.
		//            ////if ( isForFont )
		//            ////{
		//            ////    if ( baseColor == Utilities.SystemColorsWindow )
		//            ////        baseColor = Utilities.SystemColorsWindowText;
		//            ////    else if (baseColor == Utilities.SystemColorsWindowText)
		//            ////        baseColor = Utilities.SystemColorsWindow;
		//            ////}
		//            //uint actualTheme;
		//            //switch (this.theme.Value)
		//            //{
		//            //    case 0:
		//            //        actualTheme = 1;
		//            //        break;

		//            //    case 1:
		//            //        actualTheme = 0;
		//            //        break;

		//            //    case 2:
		//            //        actualTheme = 3;
		//            //        break;

		//            //    case 3:
		//            //        actualTheme = 2;
		//            //        break;

		//            //    default:
		//            //        actualTheme = this.theme.Value;
		//            //        break;
		//            //}

		//            //// MD 1/16/12 - 12.1 - Cell Format Updates
		//            ////baseColor = manager.ThemeColors[(int)actualTheme];
		//            //baseColor = manager.Workbook.ThemeColors[(int)actualTheme];

		//            #endregion // Old Code
		//            baseColor = manager.Workbook.ThemeColors[(int)this.theme];
		//        }
		//        else if (this.indexed != null)
		//        {
		//            // 8/28/08 CDS - Use the Palette off of the workbook
		//            //baseColor = manager.RetrieveIndexedColor((int)this.indexed);
		//            // MD 1/16/12 - 12.1 - Cell Format Updates
		//            //baseColor = manager.Workbook.Palette[(int)this.indexed];
		//            baseColor = manager.Workbook.Palette.GetColorAtAbsoluteIndex((int)this.indexed);
		//        }
		//        else
		//            baseColor = this.rgb;
		//    }

		//    // MD 9/2/08 - Styles
		//    // If the tint is 0, return the base color so if the color is a system color, we don't lose the system color info on the struct.
		//    if ( this.tint == 0 )
		//        return baseColor;

		//    return Utilities.ApplyTint(baseColor, this.tint);
		//}

		//#endregion ResolveColor

		#endregion // Removed

		// MD 1/16/12 - 12.1 - Cell Format Updates
		#region ResolveColorInfo

		internal WorkbookColorInfo ResolveColorInfo(WorkbookSerializationManager manager)
		{
			if (this.IsDefault)
				return null;

			if (this.Auto == ExcelDefaultableBoolean.True)
				return WorkbookColorInfo.Automatic;

			double? resolvedTint = null;
			if (this.tint != 0)
				resolvedTint = this.tint;

			WorkbookColorInfo colorInfo;

			Color baseColor = Utilities.ColorEmpty;
			if (manager != null)
			{
				if (this.theme != null &&
					this.theme < manager.Workbook.ThemeColors.Length)
				{
					colorInfo = new WorkbookColorInfo(null, (WorkbookThemeColorType)this.theme.Value, resolvedTint, true);
				}
				else if (this.indexed != null)
				{
					colorInfo = new WorkbookColorInfo(manager.Workbook.Palette.GetColorAtAbsoluteIndex((int)this.indexed), null, resolvedTint, true);
				}
				else if (this.rgb != Utilities.ColorEmpty && this.rgb.A == 255)
				{
					colorInfo = new WorkbookColorInfo(this.rgb, null, resolvedTint, true);
				}
				else
				{
					return null;
				}
			}
			else
			{
				Utilities.DebugFail("This is unexpected.");
				return WorkbookColorInfo.Automatic;
			}

			return colorInfo;
		}

		#endregion ResolveColorInfo

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