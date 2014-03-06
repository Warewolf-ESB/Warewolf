using System;
using System.ComponentModel;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.Reflection;

[assembly: ProvideMetadata(typeof(InfragisticsWPF4.DataVisualization.Design.MetadataStore))]

namespace InfragisticsWPF4.DataVisualization.Design
{
	internal partial class MetadataStore : IProvideAttributeTable
	{
		public AttributeTable AttributeTable
		{
			get
			{
			    bool isVS = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName.Equals("devenv.exe"); 
				AttributeTableBuilder tableBuilder = new AttributeTableBuilder();
				Type t = typeof(Infragistics.Controls.XamZoombar);
				Assembly controlAssembly = t.Assembly;

				#region ObjectConverter Properties
				t = controlAssembly.GetType("Infragistics.ObjectConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ObjectConverter Properties

				#region FastReflectionHelper Properties
				t = controlAssembly.GetType("Infragistics.FastReflectionHelper");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "PropertyName",
					new DescriptionAttribute(SR.GetString("FastReflectionHelper_PropertyName_Property")),
				    new DisplayNameAttribute("PropertyName")				);


				tableBuilder.AddCustomAttributes(t, "UseTraditionalReflection",
					new DescriptionAttribute(SR.GetString("FastReflectionHelper_UseTraditionalReflection_Property")),
				    new DisplayNameAttribute("UseTraditionalReflection")				);


				tableBuilder.AddCustomAttributes(t, "Invalid",
					new DescriptionAttribute(SR.GetString("FastReflectionHelper_Invalid_Property")),
				    new DisplayNameAttribute("Invalid")				);

				#endregion // FastReflectionHelper Properties

				#region RectUtil Properties
				t = controlAssembly.GetType("Infragistics.RectUtil");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RectUtil Properties

				#region GeometryUtil Properties
				t = controlAssembly.GetType("Infragistics.GeometryUtil");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "UnitNone",
					new DescriptionAttribute(SR.GetString("GeometryUtil_UnitNone_Property")),
				    new DisplayNameAttribute("UnitNone")				);


				tableBuilder.AddCustomAttributes(t, "UnitBubble",
					new DescriptionAttribute(SR.GetString("GeometryUtil_UnitBubble_Property")),
				    new DisplayNameAttribute("UnitBubble")				);


				tableBuilder.AddCustomAttributes(t, "UnitTriangle",
					new DescriptionAttribute(SR.GetString("GeometryUtil_UnitTriangle_Property")),
				    new DisplayNameAttribute("UnitTriangle")				);


				tableBuilder.AddCustomAttributes(t, "UnitPyramid",
					new DescriptionAttribute(SR.GetString("GeometryUtil_UnitPyramid_Property")),
				    new DisplayNameAttribute("UnitPyramid")				);


				tableBuilder.AddCustomAttributes(t, "UnitSquare",
					new DescriptionAttribute(SR.GetString("GeometryUtil_UnitSquare_Property")),
				    new DisplayNameAttribute("UnitSquare")				);


				tableBuilder.AddCustomAttributes(t, "UnitDiamond",
					new DescriptionAttribute(SR.GetString("GeometryUtil_UnitDiamond_Property")),
				    new DisplayNameAttribute("UnitDiamond")				);


				tableBuilder.AddCustomAttributes(t, "UnitPentagon",
					new DescriptionAttribute(SR.GetString("GeometryUtil_UnitPentagon_Property")),
				    new DisplayNameAttribute("UnitPentagon")				);


				tableBuilder.AddCustomAttributes(t, "UnitHexagon",
					new DescriptionAttribute(SR.GetString("GeometryUtil_UnitHexagon_Property")),
				    new DisplayNameAttribute("UnitHexagon")				);


				tableBuilder.AddCustomAttributes(t, "UnitTetragram",
					new DescriptionAttribute(SR.GetString("GeometryUtil_UnitTetragram_Property")),
				    new DisplayNameAttribute("UnitTetragram")				);


				tableBuilder.AddCustomAttributes(t, "UnitPentagram",
					new DescriptionAttribute(SR.GetString("GeometryUtil_UnitPentagram_Property")),
				    new DisplayNameAttribute("UnitPentagram")				);


				tableBuilder.AddCustomAttributes(t, "UnitHexagram",
					new DescriptionAttribute(SR.GetString("GeometryUtil_UnitHexagram_Property")),
				    new DisplayNameAttribute("UnitHexagram")				);


				tableBuilder.AddCustomAttributes(t, "UnitThermometer",
					new DescriptionAttribute(SR.GetString("GeometryUtil_UnitThermometer_Property")),
				    new DisplayNameAttribute("UnitThermometer")				);


				tableBuilder.AddCustomAttributes(t, "UnitHourglass",
					new DescriptionAttribute(SR.GetString("GeometryUtil_UnitHourglass_Property")),
				    new DisplayNameAttribute("UnitHourglass")				);


				tableBuilder.AddCustomAttributes(t, "UnitTube",
					new DescriptionAttribute(SR.GetString("GeometryUtil_UnitTube_Property")),
				    new DisplayNameAttribute("UnitTube")				);


				tableBuilder.AddCustomAttributes(t, "UnitRaindrop",
					new DescriptionAttribute(SR.GetString("GeometryUtil_UnitRaindrop_Property")),
				    new DisplayNameAttribute("UnitRaindrop")				);


				tableBuilder.AddCustomAttributes(t, "UnitSmiley",
					new DescriptionAttribute(SR.GetString("GeometryUtil_UnitSmiley_Property")),
				    new DisplayNameAttribute("UnitSmiley")				);

				#endregion // GeometryUtil Properties

				#region SmartPlacer Properties
				t = controlAssembly.GetType("Infragistics.SmartPlacer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Bounds",
					new DescriptionAttribute(SR.GetString("SmartPlacer_Bounds_Property")),
				    new DisplayNameAttribute("Bounds")				);


				tableBuilder.AddCustomAttributes(t, "Overlap",
					new DescriptionAttribute(SR.GetString("SmartPlacer_Overlap_Property")),
				    new DisplayNameAttribute("Overlap")				);


				tableBuilder.AddCustomAttributes(t, "Fade",
					new DescriptionAttribute(SR.GetString("SmartPlacer_Fade_Property")),
				    new DisplayNameAttribute("Fade")				);

				#endregion // SmartPlacer Properties

				#region MarkBase Properties
				t = controlAssembly.GetType("Infragistics.MarkBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Stroke",
					new DescriptionAttribute(SR.GetString("MarkBase_Stroke_Property")),
				    new DisplayNameAttribute("Stroke")				);


				tableBuilder.AddCustomAttributes(t, "Fill",
					new DescriptionAttribute(SR.GetString("MarkBase_Fill_Property")),
				    new DisplayNameAttribute("Fill")				);


				tableBuilder.AddCustomAttributes(t, "StrokeThickness",
					new DescriptionAttribute(SR.GetString("MarkBase_StrokeThickness_Property")),
				    new DisplayNameAttribute("StrokeThickness")				);


				tableBuilder.AddCustomAttributes(t, "TickMarkSize",
					new DescriptionAttribute(SR.GetString("MarkBase_TickMarkSize_Property")),
				    new DisplayNameAttribute("TickMarkSize")				);

				#endregion // MarkBase Properties

				#region FastItemsSourceEventArgs Properties
				t = controlAssembly.GetType("Infragistics.FastItemsSourceEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Action",
					new DescriptionAttribute(SR.GetString("FastItemsSourceEventArgs_Action_Property")),
				    new DisplayNameAttribute("Action")				);


				tableBuilder.AddCustomAttributes(t, "Position",
					new DescriptionAttribute(SR.GetString("FastItemsSourceEventArgs_Position_Property")),
				    new DisplayNameAttribute("Position")				);


				tableBuilder.AddCustomAttributes(t, "Count",
					new DescriptionAttribute(SR.GetString("FastItemsSourceEventArgs_Count_Property")),
				    new DisplayNameAttribute("Count")				);


				tableBuilder.AddCustomAttributes(t, "PropertyName",
					new DescriptionAttribute(SR.GetString("FastItemsSourceEventArgs_PropertyName_Property")),
				    new DisplayNameAttribute("PropertyName")				);

				#endregion // FastItemsSourceEventArgs Properties

				#region DataMappingPair Properties
				t = controlAssembly.GetType("Infragistics.DataMappingPair");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "InternalName",
					new DescriptionAttribute(SR.GetString("DataMappingPair_InternalName_Property")),
				    new DisplayNameAttribute("InternalName")				);


				tableBuilder.AddCustomAttributes(t, "ExternalName",
					new DescriptionAttribute(SR.GetString("DataMappingPair_ExternalName_Property")),
				    new DisplayNameAttribute("ExternalName")				);

				#endregion // DataMappingPair Properties

				#region DataMapping Properties
				t = controlAssembly.GetType("Infragistics.DataMapping");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DataMapping Properties

				#region StripeGroupBase Properties
				t = controlAssembly.GetType("Infragistics.StripeGroupBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Stroke",
					new DescriptionAttribute(SR.GetString("StripeGroupBase_Stroke_Property")),
				    new DisplayNameAttribute("Stroke")				);


				tableBuilder.AddCustomAttributes(t, "StrokeThickness",
					new DescriptionAttribute(SR.GetString("StripeGroupBase_StrokeThickness_Property")),
				    new DisplayNameAttribute("StrokeThickness")				);


				tableBuilder.AddCustomAttributes(t, "Style",
					new DescriptionAttribute(SR.GetString("StripeGroupBase_Style_Property")),
				    new DisplayNameAttribute("Style")				);


				tableBuilder.AddCustomAttributes(t, "Fill",
					new DescriptionAttribute(SR.GetString("StripeGroupBase_Fill_Property")),
				    new DisplayNameAttribute("Fill")				);


				tableBuilder.AddCustomAttributes(t, "Unit",
					new DescriptionAttribute(SR.GetString("StripeGroupBase_Unit_Property")),
				    new DisplayNameAttribute("Unit")				);


				tableBuilder.AddCustomAttributes(t, "Width",
					new DescriptionAttribute(SR.GetString("StripeGroupBase_Width_Property")),
				    new DisplayNameAttribute("Width")				);

				#endregion // StripeGroupBase Properties

				#region TypeUtil Properties
				t = controlAssembly.GetType("Infragistics.TypeUtil");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TypeUtil Properties

				#region RectChangedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.RectChangedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "OldRect",
					new DescriptionAttribute(SR.GetString("RectChangedEventArgs_OldRect_Property")),
				    new DisplayNameAttribute("OldRect")				);


				tableBuilder.AddCustomAttributes(t, "NewRect",
					new DescriptionAttribute(SR.GetString("RectChangedEventArgs_NewRect_Property")),
				    new DisplayNameAttribute("NewRect")				);

				#endregion // RectChangedEventArgs Properties

				#region RectChangedEventHandler Properties
				t = controlAssembly.GetType("Infragistics.RectChangedEventHandler");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RectChangedEventHandler Properties

				#region Platform Properties
				t = controlAssembly.GetType("Infragistics.Platform");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // Platform Properties

				#region TimeSpanConverter Properties
				t = controlAssembly.GetType("Infragistics.TimeSpanConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TimeSpanConverter Properties

				#region SolidBrushCollectionConverter Properties
				t = controlAssembly.GetType("Infragistics.SolidBrushCollectionConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SolidBrushCollectionConverter Properties

				#region Flattener Properties
				t = controlAssembly.GetType("Infragistics.Flattener");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // Flattener Properties

				#region AxisBase Properties
				t = controlAssembly.GetType("Infragistics.AxisBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AutoRange",
					new DescriptionAttribute(SR.GetString("AxisBase_AutoRange_Property")),
				    new DisplayNameAttribute("AutoRange")				);


				tableBuilder.AddCustomAttributes(t, "ScrollScale",
					new DescriptionAttribute(SR.GetString("AxisBase_ScrollScale_Property")),
				    new DisplayNameAttribute("ScrollScale")				);


				tableBuilder.AddCustomAttributes(t, "ScrollPosition",
					new DescriptionAttribute(SR.GetString("AxisBase_ScrollPosition_Property")),
				    new DisplayNameAttribute("ScrollPosition")				);

				#endregion // AxisBase Properties

				#region StringFormatConverter Properties
				t = controlAssembly.GetType("Infragistics.StringFormatConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // StringFormatConverter Properties

				#region TransformUtil Properties
				t = controlAssembly.GetType("Infragistics.TransformUtil");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TransformUtil Properties

				#region PriorityQueue`1 Properties
				t = controlAssembly.GetType("Infragistics.PriorityQueue`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Count",
					new DescriptionAttribute(SR.GetString("PriorityQueue`1_Count_Property")),
				    new DisplayNameAttribute("Count")				);

				#endregion // PriorityQueue`1 Properties

				#region ArrayUtil Properties
				t = controlAssembly.GetType("Infragistics.ArrayUtil");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ArrayUtil Properties

				#region FastItemsSource Properties
				t = controlAssembly.GetType("Infragistics.FastItemsSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Count",
					new DescriptionAttribute(SR.GetString("FastItemsSource_Count_Property")),
				    new DisplayNameAttribute("Count")				);


				tableBuilder.AddCustomAttributes(t, "Dispatcher",
					new DescriptionAttribute(SR.GetString("FastItemsSource_Dispatcher_Property")),
				    new DisplayNameAttribute("Dispatcher")				);


				tableBuilder.AddCustomAttributes(t, "ItemsSource",
					new DescriptionAttribute(SR.GetString("FastItemsSource_ItemsSource_Property")),
				    new DisplayNameAttribute("ItemsSource")				);

				#endregion // FastItemsSource Properties

				#region BrushCollection Properties
				t = controlAssembly.GetType("Infragistics.BrushCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "InterpolationMode",
					new DescriptionAttribute(SR.GetString("BrushCollection_InterpolationMode_Property")),
				    new DisplayNameAttribute("InterpolationMode")				);

				#endregion // BrushCollection Properties

				#region ShapeUtil Properties
				t = controlAssembly.GetType("Infragistics.ShapeUtil");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ShapeUtil Properties

				#region DateTimeConverter Properties
				t = controlAssembly.GetType("Infragistics.DateTimeConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DateTimeConverter Properties

				#region Clipper Properties
				t = controlAssembly.GetType("Infragistics.Clipper");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Target",
					new DescriptionAttribute(SR.GetString("Clipper_Target_Property")),
				    new DisplayNameAttribute("Target")				);


				tableBuilder.AddCustomAttributes(t, "IsClosed",
					new DescriptionAttribute(SR.GetString("Clipper_IsClosed_Property")),
				    new DisplayNameAttribute("IsClosed")				);

				#endregion // Clipper Properties

				#region StringFormatter Properties
				t = controlAssembly.GetType("Infragistics.StringFormatter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "FormatString",
					new DescriptionAttribute(SR.GetString("StringFormatter_FormatString_Property")),
				    new DisplayNameAttribute("FormatString")				);


				tableBuilder.AddCustomAttributes(t, "CompiledFormatString",
					new DescriptionAttribute(SR.GetString("StringFormatter_CompiledFormatString_Property")),
				    new DisplayNameAttribute("CompiledFormatString")				);


				tableBuilder.AddCustomAttributes(t, "PropertyNames",
					new DescriptionAttribute(SR.GetString("StringFormatter_PropertyNames_Property")),
				    new DisplayNameAttribute("PropertyNames")				);

				#endregion // StringFormatter Properties

				#region StringFormatUtil Properties
				t = controlAssembly.GetType("Infragistics.StringFormatUtil");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // StringFormatUtil Properties

				#region MathUtil Properties
				t = controlAssembly.GetType("Infragistics.MathUtil");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MathUtil Properties

				#region StripeBase Properties
				t = controlAssembly.GetType("Infragistics.StripeBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Stroke",
					new DescriptionAttribute(SR.GetString("StripeBase_Stroke_Property")),
				    new DisplayNameAttribute("Stroke")				);


				tableBuilder.AddCustomAttributes(t, "StrokeThickness",
					new DescriptionAttribute(SR.GetString("StripeBase_StrokeThickness_Property")),
				    new DisplayNameAttribute("StrokeThickness")				);


				tableBuilder.AddCustomAttributes(t, "Fill",
					new DescriptionAttribute(SR.GetString("StripeBase_Fill_Property")),
				    new DisplayNameAttribute("Fill")				);

				#endregion // StripeBase Properties

				#region InteractionHelper Properties
				t = controlAssembly.GetType("Infragistics.InteractionHelper");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Control",
					new DescriptionAttribute(SR.GetString("InteractionHelper_Control_Property")),
				    new DisplayNameAttribute("Control")				);


				tableBuilder.AddCustomAttributes(t, "IsFocused",
					new DescriptionAttribute(SR.GetString("InteractionHelper_IsFocused_Property")),
				    new DisplayNameAttribute("IsFocused")				);


				tableBuilder.AddCustomAttributes(t, "IsMouseOver",
					new DescriptionAttribute(SR.GetString("InteractionHelper_IsMouseOver_Property")),
				    new DisplayNameAttribute("IsMouseOver")				);


				tableBuilder.AddCustomAttributes(t, "IsReadOnly",
					new DescriptionAttribute(SR.GetString("InteractionHelper_IsReadOnly_Property")),
				    new DisplayNameAttribute("IsReadOnly")				);


				tableBuilder.AddCustomAttributes(t, "IsPressed",
					new DescriptionAttribute(SR.GetString("InteractionHelper_IsPressed_Property")),
				    new DisplayNameAttribute("IsPressed")				);


				tableBuilder.AddCustomAttributes(t, "ClickCount",
					new DescriptionAttribute(SR.GetString("InteractionHelper_ClickCount_Property")),
				    new DisplayNameAttribute("ClickCount")				);

				#endregion // InteractionHelper Properties

				#region PathFigureUtil Properties
				t = controlAssembly.GetType("Infragistics.PathFigureUtil");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PathFigureUtil Properties

				#region MatrixUtil Properties
				t = controlAssembly.GetType("Infragistics.MatrixUtil");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MatrixUtil Properties

				#region ColorUtil Properties
				t = controlAssembly.GetType("Infragistics.ColorUtil");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ColorUtil Properties

				#region ColorConverter Properties
				t = controlAssembly.GetType("Infragistics.ColorConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ColorConverter Properties

				#region BrushUtil Properties
				t = controlAssembly.GetType("Infragistics.BrushUtil");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // BrushUtil Properties

				#region CultureInfoHelper Properties
				t = controlAssembly.GetType("Infragistics.CultureInfoHelper");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CultureToUse",
					new DescriptionAttribute(SR.GetString("CultureInfoHelper_CultureToUse_Property")),
				    new DisplayNameAttribute("CultureToUse")				);

				#endregion // CultureInfoHelper Properties

				#region XamDock Properties
				t = controlAssembly.GetType("Infragistics.Controls.XamDock");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamDVSharedAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamDVSharedAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "LayoutPriority",
					new DescriptionAttribute(SR.GetString("XamDock_LayoutPriority_Property")),
				    new DisplayNameAttribute("LayoutPriority"),
					new CategoryAttribute(SR.GetString("XamDock_Properties"))
				);

				#endregion // XamDock Properties

				#region InputContext Properties
				t = controlAssembly.GetType("Infragistics.InputContext");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ViewMousePosition",
					new DescriptionAttribute(SR.GetString("InputContext_ViewMousePosition_Property")),
				    new DisplayNameAttribute("ViewMousePosition")				);


				tableBuilder.AddCustomAttributes(t, "DocMousePosition",
					new DescriptionAttribute(SR.GetString("InputContext_DocMousePosition_Property")),
				    new DisplayNameAttribute("DocMousePosition")				);


				tableBuilder.AddCustomAttributes(t, "Key",
					new DescriptionAttribute(SR.GetString("InputContext_Key_Property")),
				    new DisplayNameAttribute("Key")				);

				#endregion // InputContext Properties

				#region InteractiveControl Properties
				t = controlAssembly.GetType("Infragistics.InteractiveControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "LastInput",
					new DescriptionAttribute(SR.GetString("InteractiveControl_LastInput_Property")),
				    new DisplayNameAttribute("LastInput")				);


				tableBuilder.AddCustomAttributes(t, "Canvas",
					new DescriptionAttribute(SR.GetString("InteractiveControl_Canvas_Property")),
				    new DisplayNameAttribute("Canvas")				);


				tableBuilder.AddCustomAttributes(t, "CurrentTool",
					new DescriptionAttribute(SR.GetString("InteractiveControl_CurrentTool_Property")),
				    new DisplayNameAttribute("CurrentTool")				);


				tableBuilder.AddCustomAttributes(t, "MouseDownTools",
					new DescriptionAttribute(SR.GetString("InteractiveControl_MouseDownTools_Property")),
				    new DisplayNameAttribute("MouseDownTools")				);


				tableBuilder.AddCustomAttributes(t, "MouseMoveTools",
					new DescriptionAttribute(SR.GetString("InteractiveControl_MouseMoveTools_Property")),
				    new DisplayNameAttribute("MouseMoveTools")				);

				#endregion // InteractiveControl Properties

				#region DefaultTool Properties
				t = controlAssembly.GetType("Infragistics.DefaultTool");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DefaultTool Properties

				#region Tool Properties
				t = controlAssembly.GetType("Infragistics.Tool");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "View",
					new DescriptionAttribute(SR.GetString("Tool_View_Property")),
				    new DisplayNameAttribute("View")				);


				tableBuilder.AddCustomAttributes(t, "LastInput",
					new DescriptionAttribute(SR.GetString("Tool_LastInput_Property")),
				    new DisplayNameAttribute("LastInput")				);

				#endregion // Tool Properties

				#region Range Properties
				t = controlAssembly.GetType("Infragistics.Controls.Range");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Minimum",
					new DescriptionAttribute(SR.GetString("Range_Minimum_Property")),
				    new DisplayNameAttribute("Minimum")				);


				tableBuilder.AddCustomAttributes(t, "Maximum",
					new DescriptionAttribute(SR.GetString("Range_Maximum_Property")),
				    new DisplayNameAttribute("Maximum")				);


				tableBuilder.AddCustomAttributes(t, "Scale",
					new DescriptionAttribute(SR.GetString("Range_Scale_Property")),
				    new DisplayNameAttribute("Scale")				);


				tableBuilder.AddCustomAttributes(t, "Scroll",
					new DescriptionAttribute(SR.GetString("Range_Scroll_Property")),
				    new DisplayNameAttribute("Scroll")				);

				#endregion // Range Properties

				#region XamZoombar Properties
				t = controlAssembly.GetType("Infragistics.Controls.XamZoombar");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamDVSharedAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamDVSharedAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "PreviewContentSize",
					new DescriptionAttribute(SR.GetString("XamZoombar_PreviewContentSize_Property")),
				    new DisplayNameAttribute("PreviewContentSize"),
					new CategoryAttribute(SR.GetString("XamZoombar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalPreviewContent",
					new DescriptionAttribute(SR.GetString("XamZoombar_HorizontalPreviewContent_Property")),
				    new DisplayNameAttribute("HorizontalPreviewContent"),
				    new TypeConverterAttribute(typeof(StringConverter))
,
					new CategoryAttribute(SR.GetString("XamZoombar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "VerticalPreviewContent",
					new DescriptionAttribute(SR.GetString("XamZoombar_VerticalPreviewContent_Property")),
				    new DisplayNameAttribute("VerticalPreviewContent"),
				    new TypeConverterAttribute(typeof(StringConverter))
,
					new CategoryAttribute(SR.GetString("XamZoombar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalScaleLeftStyle",
					new DescriptionAttribute(SR.GetString("XamZoombar_HorizontalScaleLeftStyle_Property")),
				    new DisplayNameAttribute("HorizontalScaleLeftStyle"),
					new CategoryAttribute(SR.GetString("XamZoombar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalScaleRightStyle",
					new DescriptionAttribute(SR.GetString("XamZoombar_HorizontalScaleRightStyle_Property")),
				    new DisplayNameAttribute("HorizontalScaleRightStyle"),
					new CategoryAttribute(SR.GetString("XamZoombar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalThumbStyle",
					new DescriptionAttribute(SR.GetString("XamZoombar_HorizontalThumbStyle_Property")),
				    new DisplayNameAttribute("HorizontalThumbStyle"),
					new CategoryAttribute(SR.GetString("XamZoombar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalScrollLeftStyle",
					new DescriptionAttribute(SR.GetString("XamZoombar_HorizontalScrollLeftStyle_Property")),
				    new DisplayNameAttribute("HorizontalScrollLeftStyle"),
					new CategoryAttribute(SR.GetString("XamZoombar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalScrollRightStyle",
					new DescriptionAttribute(SR.GetString("XamZoombar_HorizontalScrollRightStyle_Property")),
				    new DisplayNameAttribute("HorizontalScrollRightStyle"),
					new CategoryAttribute(SR.GetString("XamZoombar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "VerticalScaleTopStyle",
					new DescriptionAttribute(SR.GetString("XamZoombar_VerticalScaleTopStyle_Property")),
				    new DisplayNameAttribute("VerticalScaleTopStyle"),
					new CategoryAttribute(SR.GetString("XamZoombar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "VerticalScaleBottomStyle",
					new DescriptionAttribute(SR.GetString("XamZoombar_VerticalScaleBottomStyle_Property")),
				    new DisplayNameAttribute("VerticalScaleBottomStyle"),
					new CategoryAttribute(SR.GetString("XamZoombar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "VerticalThumbStyle",
					new DescriptionAttribute(SR.GetString("XamZoombar_VerticalThumbStyle_Property")),
				    new DisplayNameAttribute("VerticalThumbStyle"),
					new CategoryAttribute(SR.GetString("XamZoombar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "VerticalScrollTopStyle",
					new DescriptionAttribute(SR.GetString("XamZoombar_VerticalScrollTopStyle_Property")),
				    new DisplayNameAttribute("VerticalScrollTopStyle"),
					new CategoryAttribute(SR.GetString("XamZoombar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "VerticalScrollBottomStyle",
					new DescriptionAttribute(SR.GetString("XamZoombar_VerticalScrollBottomStyle_Property")),
				    new DisplayNameAttribute("VerticalScrollBottomStyle"),
					new CategoryAttribute(SR.GetString("XamZoombar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Minimum",
					new DescriptionAttribute(SR.GetString("XamZoombar_Minimum_Property")),
				    new DisplayNameAttribute("Minimum"),
					new CategoryAttribute(SR.GetString("XamZoombar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Maximum",
					new DescriptionAttribute(SR.GetString("XamZoombar_Maximum_Property")),
				    new DisplayNameAttribute("Maximum"),
					new CategoryAttribute(SR.GetString("XamZoombar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Range",
					new DescriptionAttribute(SR.GetString("XamZoombar_Range_Property")),
				    new DisplayNameAttribute("Range"),
					new CategoryAttribute(SR.GetString("XamZoombar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SmallChange",
					new DescriptionAttribute(SR.GetString("XamZoombar_SmallChange_Property")),
				    new DisplayNameAttribute("SmallChange"),
					new CategoryAttribute(SR.GetString("XamZoombar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LargeChange",
					new DescriptionAttribute(SR.GetString("XamZoombar_LargeChange_Property")),
				    new DisplayNameAttribute("LargeChange"),
					new CategoryAttribute(SR.GetString("XamZoombar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Orientation",
					new DescriptionAttribute(SR.GetString("XamZoombar_Orientation_Property")),
				    new DisplayNameAttribute("Orientation"),
					new CategoryAttribute(SR.GetString("XamZoombar_Properties"))
				);

				#endregion // XamZoombar Properties

				#region ZoomChangedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.ZoomChangedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "NewRange",
					new DescriptionAttribute(SR.GetString("ZoomChangedEventArgs_NewRange_Property")),
				    new DisplayNameAttribute("NewRange")				);

				#endregion // ZoomChangedEventArgs Properties

				#region ZoomChangeEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.ZoomChangeEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "NewRange",
					new DescriptionAttribute(SR.GetString("ZoomChangeEventArgs_NewRange_Property")),
				    new DisplayNameAttribute("NewRange")				);

				#endregion // ZoomChangeEventArgs Properties

				#region DraggingTool Properties
				t = controlAssembly.GetType("Infragistics.Controls.DraggingTool");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DraggingTool Properties

				#region ResizingTool Properties
				t = controlAssembly.GetType("Infragistics.Controls.ResizingTool");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ResizingTool Properties

				#region TrackbarTool Properties
				t = controlAssembly.GetType("Infragistics.Controls.TrackbarTool");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TrackbarTool Properties

				#region HalfEdge Properties
				t = controlAssembly.GetType("Infragistics.Silverlight.HalfEdge");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Beg",
					new DescriptionAttribute(SR.GetString("HalfEdge_Beg_Property")),
				    new DisplayNameAttribute("Beg")				);


				tableBuilder.AddCustomAttributes(t, "End",
					new DescriptionAttribute(SR.GetString("HalfEdge_End_Property")),
				    new DisplayNameAttribute("End")				);

				#endregion // HalfEdge Properties

				#region HalfEdgeCollection Properties
				t = controlAssembly.GetType("Infragistics.Silverlight.HalfEdgeCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Count",
					new DescriptionAttribute(SR.GetString("HalfEdgeCollection_Count_Property")),
				    new DisplayNameAttribute("Count")				);

				#endregion // HalfEdgeCollection Properties

				#region Triangle Properties
				t = controlAssembly.GetType("Infragistics.Silverlight.Triangle");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "v0",
					new DescriptionAttribute(SR.GetString("Triangle_v0_Property")),
				    new DisplayNameAttribute("v0")				);


				tableBuilder.AddCustomAttributes(t, "v1",
					new DescriptionAttribute(SR.GetString("Triangle_v1_Property")),
				    new DisplayNameAttribute("v1")				);


				tableBuilder.AddCustomAttributes(t, "v2",
					new DescriptionAttribute(SR.GetString("Triangle_v2_Property")),
				    new DisplayNameAttribute("v2")				);

				#endregion // Triangle Properties

				#region TriangleMesh Properties
				t = controlAssembly.GetType("Infragistics.Silverlight.TriangleMesh");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "TriangleList",
					new DescriptionAttribute(SR.GetString("TriangleMesh_TriangleList_Property")),
				    new DisplayNameAttribute("TriangleList")				);


				tableBuilder.AddCustomAttributes(t, "EdgeCollection",
					new DescriptionAttribute(SR.GetString("TriangleMesh_EdgeCollection_Property")),
				    new DisplayNameAttribute("EdgeCollection")				);


				tableBuilder.AddCustomAttributes(t, "Points",
					new DescriptionAttribute(SR.GetString("TriangleMesh_Points_Property")),
				    new DisplayNameAttribute("Points")				);

				#endregion // TriangleMesh Properties

				#region ErrorMessageDisplayingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.ErrorMessageDisplayingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ErrorMessage",
					new DescriptionAttribute(SR.GetString("ErrorMessageDisplayingEventArgs_ErrorMessage_Property")),
				    new DisplayNameAttribute("ErrorMessage")				);

				#endregion // ErrorMessageDisplayingEventArgs Properties

				#region SafeSetters Properties
				t = controlAssembly.GetType("Infragistics.SafeSetters");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SafeSetters Properties

				#region SafeSetterCollection Properties
				t = controlAssembly.GetType("Infragistics.SafeSetterCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SafeSetterCollection Properties

				#region SafeSetter Properties
				t = controlAssembly.GetType("Infragistics.SafeSetter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("SafeSetter_Value_Property")),
				    new DisplayNameAttribute("Value")				);


				tableBuilder.AddCustomAttributes(t, "ValueAsXamlString",
					new DescriptionAttribute(SR.GetString("SafeSetter_ValueAsXamlString_Property")),
				    new DisplayNameAttribute("ValueAsXamlString")				);


				tableBuilder.AddCustomAttributes(t, "PropertyName",
					new DescriptionAttribute(SR.GetString("SafeSetter_PropertyName_Property")),
				    new DisplayNameAttribute("PropertyName")				);

				#endregion // SafeSetter Properties

				#region GroupBy Properties
				t = controlAssembly.GetType("Infragistics.GroupBy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ItemsSource",
					new DescriptionAttribute(SR.GetString("GroupBy_ItemsSource_Property")),
				    new DisplayNameAttribute("ItemsSource")				);


				tableBuilder.AddCustomAttributes(t, "KeyMemberPath",
					new DescriptionAttribute(SR.GetString("GroupBy_KeyMemberPath_Property")),
				    new DisplayNameAttribute("KeyMemberPath")				);


				tableBuilder.AddCustomAttributes(t, "ValueMemberPath",
					new DescriptionAttribute(SR.GetString("GroupBy_ValueMemberPath_Property")),
				    new DisplayNameAttribute("ValueMemberPath")				);


				tableBuilder.AddCustomAttributes(t, "GroupMemberPath",
					new DescriptionAttribute(SR.GetString("GroupBy_GroupMemberPath_Property")),
				    new DisplayNameAttribute("GroupMemberPath")				);

				#endregion // GroupBy Properties

				#region SurfaceViewer Properties
				t = controlAssembly.GetType("Infragistics.Controls.SurfaceViewer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DragStroke",
					new DescriptionAttribute(SR.GetString("SurfaceViewer_DragStroke_Property")),
				    new DisplayNameAttribute("DragStroke"),
					new CategoryAttribute(SR.GetString("SurfaceViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DragStrokeThickness",
					new DescriptionAttribute(SR.GetString("SurfaceViewer_DragStrokeThickness_Property")),
				    new DisplayNameAttribute("DragStrokeThickness"),
					new CategoryAttribute(SR.GetString("SurfaceViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DragStrokeDashArray",
					new DescriptionAttribute(SR.GetString("SurfaceViewer_DragStrokeDashArray_Property")),
				    new DisplayNameAttribute("DragStrokeDashArray"),
					new CategoryAttribute(SR.GetString("SurfaceViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PreviewBrush",
					new DescriptionAttribute(SR.GetString("SurfaceViewer_PreviewBrush_Property")),
				    new DisplayNameAttribute("PreviewBrush"),
					new CategoryAttribute(SR.GetString("SurfaceViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "WorldRect",
					new DescriptionAttribute(SR.GetString("SurfaceViewer_WorldRect_Property")),
				    new DisplayNameAttribute("WorldRect"),
					new CategoryAttribute(SR.GetString("SurfaceViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "WindowRect",
					new DescriptionAttribute(SR.GetString("SurfaceViewer_WindowRect_Property")),
				    new DisplayNameAttribute("WindowRect"),
					new CategoryAttribute(SR.GetString("SurfaceViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PreviewRect",
					new DescriptionAttribute(SR.GetString("SurfaceViewer_PreviewRect_Property")),
				    new DisplayNameAttribute("PreviewRect"),
					new CategoryAttribute(SR.GetString("SurfaceViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ViewportRect",
					new DescriptionAttribute(SR.GetString("SurfaceViewer_ViewportRect_Property")),
				    new DisplayNameAttribute("ViewportRect"),
					new CategoryAttribute(SR.GetString("SurfaceViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "NavigationSettings",
					new DescriptionAttribute(SR.GetString("SurfaceViewer_NavigationSettings_Property")),
				    new DisplayNameAttribute("NavigationSettings")				);


				tableBuilder.AddCustomAttributes(t, "MinimumZoomLevel",
					new DescriptionAttribute(SR.GetString("SurfaceViewer_MinimumZoomLevel_Property")),
				    new DisplayNameAttribute("MinimumZoomLevel"),
					new CategoryAttribute(SR.GetString("SurfaceViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaximumZoomLevel",
					new DescriptionAttribute(SR.GetString("SurfaceViewer_MaximumZoomLevel_Property")),
				    new DisplayNameAttribute("MaximumZoomLevel"),
					new CategoryAttribute(SR.GetString("SurfaceViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ZoomLevel",
					new DescriptionAttribute(SR.GetString("SurfaceViewer_ZoomLevel_Property")),
				    new DisplayNameAttribute("ZoomLevel"),
					new CategoryAttribute(SR.GetString("SurfaceViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "OverviewPlusDetailPaneStyle",
					new DescriptionAttribute(SR.GetString("SurfaceViewer_OverviewPlusDetailPaneStyle_Property")),
				    new DisplayNameAttribute("OverviewPlusDetailPaneStyle"),
					new CategoryAttribute(SR.GetString("SurfaceViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalOverviewPlusDetailPaneAlignment",
					new DescriptionAttribute(SR.GetString("SurfaceViewer_HorizontalOverviewPlusDetailPaneAlignment_Property")),
				    new DisplayNameAttribute("HorizontalOverviewPlusDetailPaneAlignment"),
					new CategoryAttribute(SR.GetString("SurfaceViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "VerticalOverviewPlusDetailPaneAlignment",
					new DescriptionAttribute(SR.GetString("SurfaceViewer_VerticalOverviewPlusDetailPaneAlignment_Property")),
				    new DisplayNameAttribute("VerticalOverviewPlusDetailPaneAlignment"),
					new CategoryAttribute(SR.GetString("SurfaceViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "OverviewPlusDetailPaneVisibility",
					new DescriptionAttribute(SR.GetString("SurfaceViewer_OverviewPlusDetailPaneVisibility_Property")),
				    new DisplayNameAttribute("OverviewPlusDetailPaneVisibility"),
					new CategoryAttribute(SR.GetString("SurfaceViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "OverviewPlusDetailPane",
					new DescriptionAttribute(SR.GetString("SurfaceViewer_OverviewPlusDetailPane_Property")),
				    new DisplayNameAttribute("OverviewPlusDetailPane"),
					new CategoryAttribute(SR.GetString("SurfaceViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ZoomLevelDisplayText",
					new DescriptionAttribute(SR.GetString("SurfaceViewer_ZoomLevelDisplayText_Property")),
				    new DisplayNameAttribute("ZoomLevelDisplayText")				);


				tableBuilder.AddCustomAttributes(t, "DefaultInteraction",
					new DescriptionAttribute(SR.GetString("SurfaceViewer_DefaultInteraction_Property")),
				    new DisplayNameAttribute("DefaultInteraction")				);

				#endregion // SurfaceViewer Properties

				#region StackPool`1 Properties
				t = controlAssembly.GetType("Infragistics.StackPool`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DeferDisactivate",
					new DescriptionAttribute(SR.GetString("StackPool`1_DeferDisactivate_Property")),
				    new DisplayNameAttribute("DeferDisactivate")				);


				tableBuilder.AddCustomAttributes(t, "ActiveCount",
					new DescriptionAttribute(SR.GetString("StackPool`1_ActiveCount_Property")),
				    new DisplayNameAttribute("ActiveCount")				);


				tableBuilder.AddCustomAttributes(t, "InactiveCount",
					new DescriptionAttribute(SR.GetString("StackPool`1_InactiveCount_Property")),
				    new DisplayNameAttribute("InactiveCount")				);


				tableBuilder.AddCustomAttributes(t, "Create",
					new DescriptionAttribute(SR.GetString("StackPool`1_Create_Property")),
				    new DisplayNameAttribute("Create")				);


				tableBuilder.AddCustomAttributes(t, "Deactivate",
					new DescriptionAttribute(SR.GetString("StackPool`1_Deactivate_Property")),
				    new DisplayNameAttribute("Deactivate")				);


				tableBuilder.AddCustomAttributes(t, "Activate",
					new DescriptionAttribute(SR.GetString("StackPool`1_Activate_Property")),
				    new DisplayNameAttribute("Activate")				);


				tableBuilder.AddCustomAttributes(t, "Destroy",
					new DescriptionAttribute(SR.GetString("StackPool`1_Destroy_Property")),
				    new DisplayNameAttribute("Destroy")				);

				#endregion // StackPool`1 Properties

				#region PropertyChangedEventArgs`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.PropertyChangedEventArgs`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "OldValue",
					new DescriptionAttribute(SR.GetString("PropertyChangedEventArgs`1_OldValue_Property")),
				    new DisplayNameAttribute("OldValue")				);


				tableBuilder.AddCustomAttributes(t, "NewValue",
					new DescriptionAttribute(SR.GetString("PropertyChangedEventArgs`1_NewValue_Property")),
				    new DisplayNameAttribute("NewValue")				);

				#endregion // PropertyChangedEventArgs`1 Properties

				#region NavigationSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.NavigationSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowPan",
					new DescriptionAttribute(SR.GetString("NavigationSettings_AllowPan_Property")),
				    new DisplayNameAttribute("AllowPan")				);


				tableBuilder.AddCustomAttributes(t, "AllowZoom",
					new DescriptionAttribute(SR.GetString("NavigationSettings_AllowZoom_Property")),
				    new DisplayNameAttribute("AllowZoom")				);

				#endregion // NavigationSettings Properties

				#region XamOverviewPlusDetailPane Properties
				t = controlAssembly.GetType("Infragistics.Controls.XamOverviewPlusDetailPane");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("CommonSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("CommonSupportingControlsAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "Immediate",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_Immediate_Property")),
				    new DisplayNameAttribute("Immediate"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "World",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_World_Property")),
				    new DisplayNameAttribute("World"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "WorldStyle",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_WorldStyle_Property")),
				    new DisplayNameAttribute("WorldStyle"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Window",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_Window_Property")),
				    new DisplayNameAttribute("Window"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "WindowStyle",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_WindowStyle_Property")),
				    new DisplayNameAttribute("WindowStyle"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Preview",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_Preview_Property")),
				    new DisplayNameAttribute("Preview"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PreviewStyle",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_PreviewStyle_Property")),
				    new DisplayNameAttribute("PreviewStyle"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Mode",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_Mode_Property")),
				    new DisplayNameAttribute("Mode"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ShrinkToThumbnail",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_ShrinkToThumbnail_Property")),
				    new DisplayNameAttribute("ShrinkToThumbnail"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SurfaceViewer",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_SurfaceViewer_Property")),
				    new DisplayNameAttribute("SurfaceViewer"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PreviewCanvas",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_PreviewCanvas_Property")),
				    new DisplayNameAttribute("PreviewCanvas"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PreviewViewportdRect",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_PreviewViewportdRect_Property")),
				    new DisplayNameAttribute("PreviewViewportdRect"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Viewport",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_Viewport_Property")),
				    new DisplayNameAttribute("Viewport"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ZoomTo100ButtonVisibility",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_ZoomTo100ButtonVisibility_Property")),
				    new DisplayNameAttribute("ZoomTo100ButtonVisibility"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ScaleToFitButtonVisibility",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_ScaleToFitButtonVisibility_Property")),
				    new DisplayNameAttribute("ScaleToFitButtonVisibility"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "InteractionStatesToolVisibility",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_InteractionStatesToolVisibility_Property")),
				    new DisplayNameAttribute("InteractionStatesToolVisibility"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ZoomTo100ButtonToolTip",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_ZoomTo100ButtonToolTip_Property")),
				    new DisplayNameAttribute("ZoomTo100ButtonToolTip"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ScaleToFitButtonToolTip",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_ScaleToFitButtonToolTip_Property")),
				    new DisplayNameAttribute("ScaleToFitButtonToolTip"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DragPanButtonToolTip",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_DragPanButtonToolTip_Property")),
				    new DisplayNameAttribute("DragPanButtonToolTip"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DragZoomButtonToolTip",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_DragZoomButtonToolTip_Property")),
				    new DisplayNameAttribute("DragZoomButtonToolTip"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ZoomInButtonToolTip",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_ZoomInButtonToolTip_Property")),
				    new DisplayNameAttribute("ZoomInButtonToolTip"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ZoomOutButtonToolTip",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_ZoomOutButtonToolTip_Property")),
				    new DisplayNameAttribute("ZoomOutButtonToolTip"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DefaultInteractionButtonToolTip",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_DefaultInteractionButtonToolTip_Property")),
				    new DisplayNameAttribute("DefaultInteractionButtonToolTip"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ZoomLevelLargeChange",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_ZoomLevelLargeChange_Property")),
				    new DisplayNameAttribute("ZoomLevelLargeChange"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsZoomable",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_IsZoomable_Property")),
				    new DisplayNameAttribute("IsZoomable"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "UpdatingSliderRanges",
					new DescriptionAttribute(SR.GetString("XamOverviewPlusDetailPane_UpdatingSliderRanges_Property")),
				    new DisplayNameAttribute("UpdatingSliderRanges"),
					new CategoryAttribute(SR.GetString("XamOverviewPlusDetailPane_Properties"))
				);

				#endregion // XamOverviewPlusDetailPane Properties

				#region SelectedCollectionBase`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.SelectedCollectionBase`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SelectedCollectionBase`1 Properties

				#region AggregateValueSource Properties
				t = controlAssembly.GetType("Infragistics.Collections.AggregateValueSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ItemsSource",
					new DescriptionAttribute(SR.GetString("AggregateValueSource_ItemsSource_Property")),
				    new DisplayNameAttribute("ItemsSource")				);


				tableBuilder.AddCustomAttributes(t, "ValueMemberPath",
					new DescriptionAttribute(SR.GetString("AggregateValueSource_ValueMemberPath_Property")),
				    new DisplayNameAttribute("ValueMemberPath")				);

				#endregion // AggregateValueSource Properties

				#region AggregateValueCollection Properties
				t = controlAssembly.GetType("Infragistics.Collections.AggregateValueCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ItemsSources",
					new DescriptionAttribute(SR.GetString("AggregateValueCollection_ItemsSources_Property")),
				    new DisplayNameAttribute("ItemsSources")				);

				#endregion // AggregateValueCollection Properties

				#region OpenStreetMapTileSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Maps.OpenStreetMapTileSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // OpenStreetMapTileSource Properties

				#region MapTileSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Maps.MapTileSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MapTileSource Properties

				#region XamMultiScaleTileSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Maps.XamMultiScaleTileSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ImageWidth",
					new DescriptionAttribute(SR.GetString("XamMultiScaleTileSource_ImageWidth_Property")),
				    new DisplayNameAttribute("ImageWidth"),
					new CategoryAttribute(SR.GetString("XamMultiScaleTileSource_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ImageHeight",
					new DescriptionAttribute(SR.GetString("XamMultiScaleTileSource_ImageHeight_Property")),
				    new DisplayNameAttribute("ImageHeight"),
					new CategoryAttribute(SR.GetString("XamMultiScaleTileSource_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TileWidth",
					new DescriptionAttribute(SR.GetString("XamMultiScaleTileSource_TileWidth_Property")),
				    new DisplayNameAttribute("TileWidth"),
					new CategoryAttribute(SR.GetString("XamMultiScaleTileSource_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TileHeight",
					new DescriptionAttribute(SR.GetString("XamMultiScaleTileSource_TileHeight_Property")),
				    new DisplayNameAttribute("TileHeight"),
					new CategoryAttribute(SR.GetString("XamMultiScaleTileSource_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TileOverlap",
					new DescriptionAttribute(SR.GetString("XamMultiScaleTileSource_TileOverlap_Property")),
				    new DisplayNameAttribute("TileOverlap"),
					new CategoryAttribute(SR.GetString("XamMultiScaleTileSource_Properties"))
				);

				#endregion // XamMultiScaleTileSource Properties

				#region CloudMadeTileSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Maps.CloudMadeTileSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Key",
					new DescriptionAttribute(SR.GetString("CloudMadeTileSource_Key_Property")),
				    new DisplayNameAttribute("Key")				);


				tableBuilder.AddCustomAttributes(t, "Parameter",
					new DescriptionAttribute(SR.GetString("CloudMadeTileSource_Parameter_Property")),
				    new DisplayNameAttribute("Parameter")				);

				#endregion // CloudMadeTileSource Properties

				#region DesignServices Properties
				t = controlAssembly.GetType("Infragistics.Controls.Maps.DesignServices");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsInDesignModeStatic",
					new DescriptionAttribute(SR.GetString("DesignServices_IsInDesignModeStatic_Property")),
				    new DisplayNameAttribute("IsInDesignModeStatic")				);

				#endregion // DesignServices Properties

				#region XamMultiScaleImage Properties
				t = controlAssembly.GetType("Infragistics.Controls.Maps.XamMultiScaleImage");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ContentPresenter",
					new DescriptionAttribute(SR.GetString("XamMultiScaleImage_ContentPresenter_Property")),
				    new DisplayNameAttribute("ContentPresenter"),
					new CategoryAttribute(SR.GetString("XamMultiScaleImage_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Source",
					new DescriptionAttribute(SR.GetString("XamMultiScaleImage_Source_Property")),
				    new DisplayNameAttribute("Source"),
					new CategoryAttribute(SR.GetString("XamMultiScaleImage_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ViewportOrigin",
					new DescriptionAttribute(SR.GetString("XamMultiScaleImage_ViewportOrigin_Property")),
				    new DisplayNameAttribute("ViewportOrigin"),
					new CategoryAttribute(SR.GetString("XamMultiScaleImage_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ViewportWidth",
					new DescriptionAttribute(SR.GetString("XamMultiScaleImage_ViewportWidth_Property")),
				    new DisplayNameAttribute("ViewportWidth"),
					new CategoryAttribute(SR.GetString("XamMultiScaleImage_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DeferralHandler",
					new DescriptionAttribute(SR.GetString("XamMultiScaleImage_DeferralHandler_Property")),
				    new DisplayNameAttribute("DeferralHandler"),
					new CategoryAttribute(SR.GetString("XamMultiScaleImage_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "UseSprings",
					new DescriptionAttribute(SR.GetString("XamMultiScaleImage_UseSprings_Property")),
				    new DisplayNameAttribute("UseSprings"),
					new CategoryAttribute(SR.GetString("XamMultiScaleImage_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SpringsEasingFunction",
					new DescriptionAttribute(SR.GetString("XamMultiScaleImage_SpringsEasingFunction_Property")),
				    new DisplayNameAttribute("SpringsEasingFunction"),
					new CategoryAttribute(SR.GetString("XamMultiScaleImage_Properties"))
				);

				#endregion // XamMultiScaleImage Properties

				#region PointCollectionUtil Properties
				t = controlAssembly.GetType("Infragistics.Controls.PointCollectionUtil");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PointCollectionUtil Properties

				#region BingMapsTileSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Maps.BingMapsTileSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "TilePath",
					new DescriptionAttribute(SR.GetString("BingMapsTileSource_TilePath_Property")),
				    new DisplayNameAttribute("TilePath")				);


				tableBuilder.AddCustomAttributes(t, "SubDomains",
					new DescriptionAttribute(SR.GetString("BingMapsTileSource_SubDomains_Property")),
				    new DisplayNameAttribute("SubDomains")				);


				tableBuilder.AddCustomAttributes(t, "CultureName",
					new DescriptionAttribute(SR.GetString("BingMapsTileSource_CultureName_Property")),
				    new DisplayNameAttribute("CultureName")				);

				#endregion // BingMapsTileSource Properties

				#region DoubleAnimator Properties
				t = controlAssembly.GetType("Infragistics.DoubleAnimator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "TransitionProgress",
					new DescriptionAttribute(SR.GetString("DoubleAnimator_TransitionProgress_Property")),
				    new DisplayNameAttribute("TransitionProgress")				);


				tableBuilder.AddCustomAttributes(t, "IntervalMilliseconds",
					new DescriptionAttribute(SR.GetString("DoubleAnimator_IntervalMilliseconds_Property")),
				    new DisplayNameAttribute("IntervalMilliseconds")				);

				#endregion // DoubleAnimator Properties

				#region Numeric Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.Util.Numeric");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // Numeric Properties

				#region LeastSquaresFit Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.Util.LeastSquaresFit");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // LeastSquaresFit Properties

				#region SRProvider Properties
				t = controlAssembly.GetType("Infragistics.SRProvider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "OPD_DefaultInteraction",
					new DescriptionAttribute(SR.GetString("SRProvider_OPD_DefaultInteraction_Property")),
				    new DisplayNameAttribute("OPD_DefaultInteraction")				);


				tableBuilder.AddCustomAttributes(t, "OPD_ScaleToFit",
					new DescriptionAttribute(SR.GetString("SRProvider_OPD_ScaleToFit_Property")),
				    new DisplayNameAttribute("OPD_ScaleToFit")				);


				tableBuilder.AddCustomAttributes(t, "OPD_ZoomTo100",
					new DescriptionAttribute(SR.GetString("SRProvider_OPD_ZoomTo100_Property")),
				    new DisplayNameAttribute("OPD_ZoomTo100")				);


				tableBuilder.AddCustomAttributes(t, "OPD_ScaleToFit_SeriesViewer",
					new DescriptionAttribute(SR.GetString("SRProvider_OPD_ScaleToFit_SeriesViewer_Property")),
				    new DisplayNameAttribute("OPD_ScaleToFit_SeriesViewer")				);

				#endregion // SRProvider Properties

				#region SmartPlaceableWrapper`1 Properties
				t = controlAssembly.GetType("Infragistics.SmartPlaceableWrapper`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "NoWiggle",
					new DescriptionAttribute(SR.GetString("SmartPlaceableWrapper`1_NoWiggle_Property")),
				    new DisplayNameAttribute("NoWiggle")				);


				tableBuilder.AddCustomAttributes(t, "Element",
					new DescriptionAttribute(SR.GetString("SmartPlaceableWrapper`1_Element_Property")),
				    new DisplayNameAttribute("Element")				);


				tableBuilder.AddCustomAttributes(t, "ElementLocationResult",
					new DescriptionAttribute(SR.GetString("SmartPlaceableWrapper`1_ElementLocationResult_Property")),
				    new DisplayNameAttribute("ElementLocationResult")				);


				tableBuilder.AddCustomAttributes(t, "OriginalLocation",
					new DescriptionAttribute(SR.GetString("SmartPlaceableWrapper`1_OriginalLocation_Property")),
				    new DisplayNameAttribute("OriginalLocation")				);


				tableBuilder.AddCustomAttributes(t, "Opacity",
					new DescriptionAttribute(SR.GetString("SmartPlaceableWrapper`1_Opacity_Property")),
				    new DisplayNameAttribute("Opacity")				);


				tableBuilder.AddCustomAttributes(t, "SmartPosition",
					new DescriptionAttribute(SR.GetString("SmartPlaceableWrapper`1_SmartPosition_Property")),
				    new DisplayNameAttribute("SmartPosition")				);

				#endregion // SmartPlaceableWrapper`1 Properties

				#region Extensions Properties
				t = controlAssembly.GetType("Infragistics.Extensions");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // Extensions Properties

				#region XamDockAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.XamDockAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Dock",
					new DescriptionAttribute(SR.GetString("XamDockAutomationPeer_Dock_Property")),
				    new DisplayNameAttribute("Dock"),
					new CategoryAttribute(SR.GetString("XamDockAutomationPeer_Properties"))
				);

				#endregion // XamDockAutomationPeer Properties

				#region TrendCalculators Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.TrendCalculators");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TrendCalculators Properties

				#region RearrangedList`1 Properties
				t = controlAssembly.GetType("Infragistics.RearrangedList`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Count",
					new DescriptionAttribute(SR.GetString("RearrangedList`1_Count_Property")),
				    new DisplayNameAttribute("Count")				);


				tableBuilder.AddCustomAttributes(t, "IsReadOnly",
					new DescriptionAttribute(SR.GetString("RearrangedList`1_IsReadOnly_Property")),
				    new DisplayNameAttribute("IsReadOnly")				);

				#endregion // RearrangedList`1 Properties

				#region CursorTypeConverter Properties
				t = controlAssembly.GetType("Infragistics.CursorTypeConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CursorTypeConverter Properties

				#region WidgetAttribute Properties
				t = controlAssembly.GetType("System.Runtime.CompilerServices.WidgetAttribute");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // WidgetAttribute Properties

				#region MainWidgetAttribute Properties
				t = controlAssembly.GetType("System.Runtime.CompilerServices.MainWidgetAttribute");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MainWidgetAttribute Properties

				#region SuppressWidgetMemberAttribute Properties
				t = controlAssembly.GetType("System.Runtime.CompilerServices.SuppressWidgetMemberAttribute");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SuppressWidgetMemberAttribute Properties

				#region WidgetDefaultStringAttribute Properties
				t = controlAssembly.GetType("System.Runtime.CompilerServices.WidgetDefaultStringAttribute");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("WidgetDefaultStringAttribute_Value_Property")),
				    new DisplayNameAttribute("Value")				);

				#endregion // WidgetDefaultStringAttribute Properties

				#region WidgetDefaultNumberAttribute Properties
				t = controlAssembly.GetType("System.Runtime.CompilerServices.WidgetDefaultNumberAttribute");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("WidgetDefaultNumberAttribute_Value_Property")),
				    new DisplayNameAttribute("Value")				);

				#endregion // WidgetDefaultNumberAttribute Properties

				#region WidgetDefaultBooleanAttribute Properties
				t = controlAssembly.GetType("System.Runtime.CompilerServices.WidgetDefaultBooleanAttribute");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("WidgetDefaultBooleanAttribute_Value_Property")),
				    new DisplayNameAttribute("Value")				);

				#endregion // WidgetDefaultBooleanAttribute Properties

				#region SuppressWidgetMemberCopyAttribute Properties
				t = controlAssembly.GetType("System.Runtime.CompilerServices.SuppressWidgetMemberCopyAttribute");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SuppressWidgetMemberCopyAttribute Properties

				#region WidgetModuleAttribute Properties
				t = controlAssembly.GetType("System.Runtime.CompilerServices.WidgetModuleAttribute");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Name",
					new DescriptionAttribute(SR.GetString("WidgetModuleAttribute_Name_Property")),
				    new DisplayNameAttribute("Name")				);

				#endregion // WidgetModuleAttribute Properties

				#region WidgetModuleParentAttribute Properties
				t = controlAssembly.GetType("System.Runtime.CompilerServices.WidgetModuleParentAttribute");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Name",
					new DescriptionAttribute(SR.GetString("WidgetModuleParentAttribute_Name_Property")),
				    new DisplayNameAttribute("Name")				);

				#endregion // WidgetModuleParentAttribute Properties

				#region WidgetIgnoreDependsAttribute Properties
				t = controlAssembly.GetType("System.Runtime.CompilerServices.WidgetIgnoreDependsAttribute");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Name",
					new DescriptionAttribute(SR.GetString("WidgetIgnoreDependsAttribute_Name_Property")),
				    new DisplayNameAttribute("Name")				);

				#endregion // WidgetIgnoreDependsAttribute Properties

				#region DontObfuscateAttribute Properties
				t = controlAssembly.GetType("System.Runtime.CompilerServices.DontObfuscateAttribute");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DontObfuscateAttribute Properties

				#region WeakAttribute Properties
				t = controlAssembly.GetType("System.Runtime.CompilerServices.WeakAttribute");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // WeakAttribute Properties

				#region GroupingBase Properties
				t = controlAssembly.GetType("Infragistics.GroupingBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Key",
					new DescriptionAttribute(SR.GetString("GroupingBase_Key_Property")),
				    new DisplayNameAttribute("Key")				);

				#endregion // GroupingBase Properties

				#region VisualStates Properties
				t = controlAssembly.GetType("Infragistics.VisualStates");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // VisualStates Properties

				#region PolySimplification Properties
				t = controlAssembly.GetType("Infragistics.PolySimplification");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PolySimplification Properties

				#region EncodingsCollection Properties
				t = controlAssembly.GetType("Infragistics.EncodingsCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // EncodingsCollection Properties

				#region ImageStreamValidEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Maps.ImageStreamValidEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Stream",
					new DescriptionAttribute(SR.GetString("ImageStreamValidEventArgs_Stream_Property")),
				    new DisplayNameAttribute("Stream")				);

				#endregion // ImageStreamValidEventArgs Properties

				#region ImageStreamValidEventHandler Properties
				t = controlAssembly.GetType("Infragistics.Controls.Maps.ImageStreamValidEventHandler");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ImageStreamValidEventHandler Properties

				#region XamZoombarAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.XamZoombarAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Zoombar",
					new DescriptionAttribute(SR.GetString("XamZoombarAutomationPeer_Zoombar_Property")),
				    new DisplayNameAttribute("Zoombar"),
					new CategoryAttribute(SR.GetString("XamZoombarAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontallyScrollable",
					new DescriptionAttribute(SR.GetString("XamZoombarAutomationPeer_HorizontallyScrollable_Property")),
				    new DisplayNameAttribute("HorizontallyScrollable"),
					new CategoryAttribute(SR.GetString("XamZoombarAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalScrollPercent",
					new DescriptionAttribute(SR.GetString("XamZoombarAutomationPeer_HorizontalScrollPercent_Property")),
				    new DisplayNameAttribute("HorizontalScrollPercent"),
					new CategoryAttribute(SR.GetString("XamZoombarAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalViewSize",
					new DescriptionAttribute(SR.GetString("XamZoombarAutomationPeer_HorizontalViewSize_Property")),
				    new DisplayNameAttribute("HorizontalViewSize"),
					new CategoryAttribute(SR.GetString("XamZoombarAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "VerticallyScrollable",
					new DescriptionAttribute(SR.GetString("XamZoombarAutomationPeer_VerticallyScrollable_Property")),
				    new DisplayNameAttribute("VerticallyScrollable"),
					new CategoryAttribute(SR.GetString("XamZoombarAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "VerticalScrollPercent",
					new DescriptionAttribute(SR.GetString("XamZoombarAutomationPeer_VerticalScrollPercent_Property")),
				    new DisplayNameAttribute("VerticalScrollPercent"),
					new CategoryAttribute(SR.GetString("XamZoombarAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "VerticalViewSize",
					new DescriptionAttribute(SR.GetString("XamZoombarAutomationPeer_VerticalViewSize_Property")),
				    new DisplayNameAttribute("VerticalViewSize"),
					new CategoryAttribute(SR.GetString("XamZoombarAutomationPeer_Properties"))
				);

				#endregion // XamZoombarAutomationPeer Properties
				return tableBuilder.CreateTable();
			}
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