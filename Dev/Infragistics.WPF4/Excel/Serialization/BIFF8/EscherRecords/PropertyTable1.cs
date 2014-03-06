using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;




using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords
{
	internal class PropertyTable1 : PropertyTableBase
	{
		public PropertyTable1( WorksheetShape shape )
			: base( shape.DrawingProperties1 ) { }

		public PropertyTable1( List<PropertyTableBase.PropertyValue> properties )
			: base( properties ) { }

		public PropertyTable1( byte version, ushort instance, uint recordLength )
			: base( version, instance, recordLength ) { }

		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			base.Load( manager );

			WorksheetShape shape = (WorksheetShape)manager.ContextStack[ typeof( WorksheetShape ) ];

			if ( shape != null )
			{
				shape.DrawingProperties1 = this.Properties;

				// MD 8/23/11 - TFS84306
				this.ParseLoadedPropertyValues(manager, shape);
			}
			else
			{
				manager.Workbook.DrawingProperties1 = this.Properties;
			}
		}

		// MD 8/23/11 - TFS84306
		#region ParseColor

		private static Color? ParseColor(BIFF8WorkbookSerializationManager manager, PropertyValue colorPropertyValue, PropertyValue opacityPropertyValue)
		{
			if (colorPropertyValue != null)
			{
				byte alpha = 255;
				if (opacityPropertyValue != null)
				{
					double opactiy = Utilities.FromFixedPoint16_16Value((uint)opacityPropertyValue.Value);
					alpha = (byte)MathUtilities.MidpointRoundingAwayFromZero(opactiy * 255);
				}

				uint value = (uint)colorPropertyValue.Value;

				if ((value & 0xFF000000) == 0)
				{
					// If the high order byte is not set, it is a normal RGB value. 
					byte rValue = (byte)(value & 0x000000FF);
					byte gValue = (byte)((value & 0x0000FF00) >> 8);
					byte bValue = (byte)((value & 0x00FF0000) >> 16);

					Color solidColor = Color.FromArgb(alpha, rValue, gValue, bValue);
					return solidColor;
				}
				else
				{
					// MD 9/20/11 - TFS86085
					byte highOrderByte = (byte)(value >> 24);
					ColorIndex colorIndex = (ColorIndex)highOrderByte;

					switch (colorIndex)
					{
						case ColorIndex.SchemeIndex:
							int paletteIndex = (int)(value & 0x00FFFFFF);
							// MD 1/16/12 - 12.1 - Cell Format Updates
							//return manager.Workbook.Palette[paletteIndex];
							return manager.Workbook.Palette.GetColorAtAbsoluteIndex(paletteIndex);

						case ColorIndex.SysIndex:
							{
								// MD 7/2/12 - TFS115692
								// Added some loading logic for this case.
								SysIndex sysIndex = (SysIndex)(value & 0x00FFFFFF);
								switch (sysIndex)
								{
									case SysIndex.SystemColorButtonFace: return Utilities.SystemColorsInternal.ButtonFaceColor;
									case SysIndex.SystemColorWindowText: return Utilities.SystemColorsInternal.WindowTextColor;
									case SysIndex.SystemColorMenu: return Utilities.SystemColorsInternal.MenuColor;
									case SysIndex.SystemColorHighlight: return Utilities.SystemColorsInternal.HighlightColor;
									case SysIndex.SystemColorHighlightText: return Utilities.SystemColorsInternal.HighlightTextColor;
									case SysIndex.SystemColorCaptionText: return Utilities.SystemColorsInternal.ActiveCaptionTextColor;
									case SysIndex.SystemColorActiveCaption: return Utilities.SystemColorsInternal.ActiveCaptionColor;
									case SysIndex.SystemColorButtonHighlight: return Utilities.SystemColorsInternal.ButtonHighlightColor;
									case SysIndex.SystemColorButtonShadow: return Utilities.SystemColorsInternal.ButtonShadowColor;
									case SysIndex.SystemColorButtonText: return Utilities.SystemColorsInternal.ControlTextColor; 
									case SysIndex.SystemColorGrayText: return Utilities.SystemColorsInternal.GrayTextColor;
									case SysIndex.SystemColorInactiveCaption: return Utilities.SystemColorsInternal.InactiveCaptionColor;
									case SysIndex.SystemColorInactiveCaptionText: return Utilities.SystemColorsInternal.InactiveCaptionTextColor;
									case SysIndex.SystemColorInfoBackground: return Utilities.SystemColorsInternal.InfoColor;
									case SysIndex.SystemColorInfoText: return Utilities.SystemColorsInternal.InfoTextColor;
									case SysIndex.SystemColorMenuText: return Utilities.SystemColorsInternal.MenuTextColor;
									case SysIndex.SystemColorScrollbar: return Utilities.SystemColorsInternal.ScrollBarColor;
									case SysIndex.SystemColorWindow: return Utilities.SystemColorsInternal.WindowColor;
									case SysIndex.SystemColorWindowFrame: return Utilities.SystemColorsInternal.WindowFrameColor;
									case SysIndex.SystemColor3DLight: return Utilities.SystemColorsInternal.ControlLightColor;  

									default:
										Utilities.DebugFail("Don't know how to handle this SysIndex: " + sysIndex);
										break;
								}
							}
							break;

						default:
							Utilities.DebugFail("Don't know how to handle this yet.");
							break;
					}
				}
			}

			return null;
		}

		#endregion  // ParseColor

		// MD 8/23/11 - TFS84306
		#region ParseLoadedPropertyValues

		private void ParseLoadedPropertyValues(BIFF8WorkbookSerializationManager manager, WorksheetShape shape)
		{
			PropertyValue fillStyleColorValue = null;
			PropertyValue fillStyleOpacityValue = null;

			// MD 7/5/12 - TFS115686
			PropertyValue fillStyleNoFillHitTest = null;

			PropertyValue lineStyleColorValue = null;
			PropertyValue lineStyleOpacityValue = null;

			// MD 7/2/12 - TFS115692
			PropertyValue lineStyleNoLineDrawDash = null;

			// MD 8/7/12 - TFS115692
			WorksheetShapeWithText shapeWithText = shape as WorksheetShapeWithText;

			for(int i = 0; i < this.Properties.Count; i++)
			{
				PropertyValue propertyValue = this.Properties[i];
				
				switch (propertyValue.PropertyType)
				{
					// MD 7/24/12 - TFS115693
					// We now handle the TransformRotation property
					case PropertyType.TransformRotation:
						shape.Rotation = Utilities.FromFixedPoint16_16Value((uint)propertyValue.Value);
						break;

					// MD 8/7/12 - TFS115692
					case PropertyType.TextLeft:
						Debug.Assert(shapeWithText != null, "This property should be written out on a shape with text.");
						if (shapeWithText != null)
							shapeWithText.LeftMargin = Utilities.EMUToTwips((int)(uint)propertyValue.Value);
						break;

					// MD 8/7/12 - TFS115692
					case PropertyType.TextTop:
						Debug.Assert(shapeWithText != null, "This property should be written out on a shape with text.");
						if (shapeWithText != null)
							shapeWithText.TopMargin = Utilities.EMUToTwips((int)(uint)propertyValue.Value);
						break;

					// MD 8/7/12 - TFS115692
					case PropertyType.TextRight:
						Debug.Assert(shapeWithText != null, "This property should be written out on a shape with text.");
						if (shapeWithText != null)
							shapeWithText.RightMargin = Utilities.EMUToTwips((int)(uint)propertyValue.Value);
						break;

					// MD 8/7/12 - TFS115692
					case PropertyType.TextBottom:
						Debug.Assert(shapeWithText != null, "This property should be written out on a shape with text.");
						if (shapeWithText != null)
							shapeWithText.BottomMargin = Utilities.EMUToTwips((int)(uint)propertyValue.Value);
						break;

					case PropertyType.FillStyleColor:
						fillStyleColorValue = propertyValue;
						break;

					case PropertyType.FillStyleOpacity:
						fillStyleOpacityValue = propertyValue;
						break;

					// MD 7/5/12 - TFS115686
					case PropertyType.FillStyleNoFillHitTest:
						fillStyleNoFillHitTest = propertyValue;
						break;

					case PropertyType.LineStyleColor:
						lineStyleColorValue = propertyValue;
						break;

					case PropertyType.LineStyleOpacity:
						lineStyleOpacityValue = propertyValue;
						break;

					// MD 7/2/12 - TFS115692
					case PropertyType.LineStyleNoLineDrawDash:
						lineStyleNoLineDrawDash = propertyValue;
						break;
				}
			}

			Color? backgroundColor = PropertyTable1.ParseColor(manager, fillStyleColorValue, fillStyleOpacityValue);
			if (backgroundColor.HasValue)
			{
				shape.Fill = ShapeFill.FromColor(backgroundColor.Value);
			}
			// MD 7/5/12 - TFS115686
			// If the bit is turned on that the shape is filled, we should use a default color.
			else if (fillStyleNoFillHitTest != null && fillStyleNoFillHitTest.Value is uint && ((uint)fillStyleNoFillHitTest.Value & 0x10) == 0x10)
			{
				shape.Fill = ShapeFill.FromColor(Utilities.SystemColorsInternal.WindowColor);
			}

			Color? lineColor = PropertyTable1.ParseColor(manager, lineStyleColorValue, lineStyleOpacityValue);
			if (lineColor.HasValue)
			{
				shape.Outline = ShapeOutline.FromColor(lineColor.Value);
			}
			// MD 7/2/12 - TFS115692
			// If the bit is turned on that a line is present, we should use a default color.
			else if (lineStyleNoLineDrawDash != null && lineStyleNoLineDrawDash.Value is uint && ((uint)lineStyleNoLineDrawDash.Value & 0x08) == 0x08)
			{
				shape.Outline = ShapeOutline.FromColor(Utilities.SystemColorsInternal.WindowTextColor);
			}
		}

		#endregion  // ParseLoadedPropertyValues

		public override EscherRecordType Type
		{
			get { return EscherRecordType.PropertyTable1; }
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