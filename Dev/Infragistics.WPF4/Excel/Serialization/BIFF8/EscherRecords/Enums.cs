using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords
{
	internal enum BLIPRecordType : ushort
	{
		Unknwon =	0x0000,
		WMF =		0x0216,
		EMF =		0x03D4,
		PICT =		0x0542,
		PNG =		0x06E0,
		JPEG =		0x046A,
		DIB =		0x07A8,
		CMYKJPEG =	0x06E2,
		TIFF =		0x06E4,
		Client =	0x0800
	}

	internal enum BLIPType : byte
	{
		Error = 0,
		Unknown = 1,
		EMF = 2,
		WMF = 3,
		PICT = 4,
		JPEG = 5,
		PNG = 6,
		DIB = 7,
		TIFF = 17,
		CMYKJPEG = 18,
		FirstClient = 32,
		LastClient = 255
	}

	internal enum BLIPUsage : byte
	{
		Default = 0,
		Texture = 1,
		Max = 255
	}

	internal enum EscherRecordType : ushort
	{
		DrawingGroupContainer		= 0xF000,
		BLIPStoreContainer			= 0xF001,
		DrawingContainer			= 0xF002,
		GroupContainer				= 0xF003,
		ShapeContainer				= 0xF004,
		SolverContainer				= 0xF005,
		DrawingGroup				= 0xF006,
		BLIPStoreEntry				= 0xF007,
		Drawing						= 0xF008,
		GroupShape					= 0xF009,
		Shape						= 0xF00A,
		PropertyTable1				= 0xF00B,
		Textbox						= 0xF00C,
		ClientTextbox				= 0xF00D,
		Anchor						= 0xF00E,
		ChildAnchor					= 0xF00F,
		ClientAnchor				= 0xF010,
		ClientData					= 0xF011,
		ConnectorRule				= 0xF012,
		AlignRule					= 0xF013,
		ArcRule						= 0xF014,
		ClientRule					= 0xF015,
		ClassID						= 0xF016,
		CalloutRule					= 0xF017,
		BLIPMin						= 0xF018,
		
		BLIPMax						= 0xF117,
		Regroup						= 0xF118,
		Selections					= 0xF119,
		ColorMRU					= 0xF11A,
		DeletedPSPL					= 0xF11D,
		SplitMenuColors				= 0xF11E,
		OLEObject					= 0xF11F,
		ColorScheme					= 0xF120,
		PropertyTable2				= 0xF121,
		PropertyTable3				= 0xF122,
	}

	internal enum PropertyType : ushort
	{
		// http://msdn.microsoft.com/en-us/library/dd949750(v=office.12)
		TransformRotation					= 4,

		ProtectionLockAgainstGrouping		= 127,

		// http://msdn.microsoft.com/en-us/library/dd947446(v=office.12)
		TextID								= 128,
		// http://msdn.microsoft.com/en-us/library/dd953234(v=office.12)
		TextLeft							= 129,
		// http://msdn.microsoft.com/en-us/library/dd925068(v=office.12)
		TextTop								= 130,
		// http://msdn.microsoft.com/en-us/library/dd906782(v=office.12)
		TextRight							= 131,
		// http://msdn.microsoft.com/en-us/library/dd772858(v=office.12)
		TextBottom							= 132,
		// http://msdn.microsoft.com/en-us/library/dd948575(v=office.12)
		TextAnchorText						= 135,
		// http://msdn.microsoft.com/en-us/library/dd920734(v=office.12)
		TextDirection						= 139,
		// http://msdn.microsoft.com/en-us/library/dd950905(v=office.12)
		TextFitToShape						= 191,

		BLIPId								= 260,
		BLIPName							= 261,
		BLIPPictureActive					= 319,

		GeometryRight						= 322,
		GeometryBottom						= 323,
		GeometryShapePath					= 324,
		GeometryVertices					= 325,
		GeometrySegmentInfo					= 326,
		GeometryConnectionSites				= 337,
		GeometryConnectionSitesDirection	= 338,
		GeometryAdjustHandles				= 341,
		GeometryGuides						= 342,
		GeometryInscribe					= 343,
		GeometryTypeOfConnectionSites		= 344,
		GeometryFragments					= 345,			// MD 8/26/11 - TFS84024
		GeometryFillOK						= 383,

		FillStyleColor						= 385,
		FillStyleOpacity					= 386,			// MD 8/23/11 - TFS84306
		FillStyleBackColor					= 387,
		FillStyleColorModification			= 389,
		FillStyleShadeColors				= 407,			// MD 8/26/11 - TFS84024

		// http://msdn.microsoft.com/en-us/library/dd909380(v=office.12).aspx
		FillStyleNoFillHitTest				= 447,

		LineStyleColor						= 448,
		LineStyleOpacity					= 449,			// MD 8/23/11 - TFS84306
		LineStyleColorModification			= 451,
		LineStyleWidth						= 459,
		LineStyleDashStyle					= 463,			// MD 8/26/11 - TFS84024

		// http://msdn.microsoft.com/en-us/library/dd951605(v=office.12).aspx
		LineStyleNoLineDrawDash				= 511,

		ShadowColor							= 513,
		ShadowColorModification				= 515,
		ShadowObscured						= 575,

		ShapeBackground						= 831,

		GroupShapeName						= 896,
		GroupShapeDescription				= 897,
		GroupShapeWrapPolygonVertices		= 899,			// MD 8/26/11 - TFS84024
		GroupTableRowProperties				= 928,			// MD 8/26/11 - TFS84024

		
		
		Office2007Data						= 937,			// MD 8/23/11 - TFS84306

		// Not sure if this property is named correctly, but its undocumented
		ExtendedProperties					= 959,

		DiagramRelationTable				= 1284,			// MD 8/26/11 - TFS84024
		DiagramConstrainBounds				= 1288,			// MD 8/26/11 - TFS84024

		LineLeftStyleDashStyle				= 1349,			// MD 8/26/11 - TFS84024

		LineTopStyleDashStyle				= 1423,			// MD 8/26/11 - TFS84024

		LineRightStyleDashStyle				= 1487,			// MD 8/26/11 - TFS84024

		LineBottomStyleDashStyle			= 1551,			// MD 8/26/11 - TFS84024

		LineColumnStyleDashStyle			= 1615,			// MD 8/26/11 - TFS84024

		ClipVertices						= 1728,			// MD 8/26/11 - TFS84024
		ClipSegmentsInfo					= 1729,			// MD 8/26/11 - TFS84024
	}

	// MD 9/20/11 - TFS86085
	internal enum ColorIndex : byte
	{
		PaletteIndex = 0x01,
		PaletteRGB = 0x02,
		SystemRGB = 0x04,
		SchemeIndex = 0x08,
		SysIndex = 0x10,
	}

	// MD 9/20/11 - TFS86085
	internal enum SysIndex
	{
		SystemColorButtonFace,          // COLOR_BTNFACE
		SystemColorWindowText,          // COLOR_WINDOWTEXT
		SystemColorMenu,                // COLOR_MENU
		SystemColorHighlight,           // COLOR_HIGHLIGHT
		SystemColorHighlightText,       // COLOR_HIGHLIGHTTEXT
		SystemColorCaptionText,         // COLOR_CAPTIONTEXT
		SystemColorActiveCaption,       // COLOR_ACTIVECAPTION
		SystemColorButtonHighlight,     // COLOR_BTNHIGHLIGHT
		SystemColorButtonShadow,        // COLOR_BTNSHADOW
		SystemColorButtonText,          // COLOR_BTNTEXT
		SystemColorGrayText,            // COLOR_GRAYTEXT
		SystemColorInactiveCaption,     // COLOR_INACTIVECAPTION
		SystemColorInactiveCaptionText, // COLOR_INACTIVECAPTIONTEXT
		SystemColorInfoBackground,      // COLOR_INFOBK
		SystemColorInfoText,            // COLOR_INFOTEXT
		SystemColorMenuText,            // COLOR_MENUTEXT
		SystemColorScrollbar,           // COLOR_SCROLLBAR
		SystemColorWindow,              // COLOR_WINDOW
		SystemColorWindowFrame,         // COLOR_WINDOWFRAME
		SystemColor3DLight,             // COLOR_3DLIGHT
		SystemColorMax,                 // Count of system colors

		FillColor = 0xF0,				// Use the fillColor property
		LineOrFillColor,				// Use the line color only if there is a line
		LineColor,						// Use the lineColor property
		ShadowColor,					// Use the shadow color
		This,							// Use this color (only valid as described below)
		FillBackColor,					// Use the fillBackColor property
		LineBackColor,					// Use the lineBackColor property
		FillThenLine,					// Use the fillColor unless no fill and line
		IndexMask = 0xFF,				// Extract the color index

		ProcessMask = 0xFFFF00,			// All the processing bits
		ModificationMask = 0x0F00,		// Just the function
		ModFlagMask = 0xF000,			// Just the additional flags
		Darken = 0x0100,				// Darken color by parameter/255
		Lighten = 0x0200,				// Lighten color by parameter/255
		Add = 0x0300,					// Add grey level RGB(param,param,param)
		Subtract = 0x0400,				// Subtract grey level RGB(p,p,p)
		ReverseSubtract = 0x0500,		// Subtract from grey level RGB(p,p,p)

		

		BlackWhite = 0x0600,			// Black if < uParam, else white (>=)
		Invert = 0x2000,				// Invert color (at the *end*)
		Invert128 = 0x4000,				// Invert by toggling the top bit
		Gray = 0x8000,					// Make the color gray (before the above!)
		BParamMask = 0xFF0000,			// Parameter used as above
		BParamShift = 16,				// To extract the parameter value
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