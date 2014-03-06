using System;
using System.ComponentModel;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.Reflection;

[assembly: ProvideMetadata(typeof(InfragisticsWPF4.Controls.Interactions.XamFormulaEditor.Design.MetadataStore))]

namespace InfragisticsWPF4.Controls.Interactions.XamFormulaEditor.Design
{
	internal partial class MetadataStore : IProvideAttributeTable
	{
		public AttributeTable AttributeTable
		{
			get
			{
			    bool isVS = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName.Equals("devenv.exe"); 
				AttributeTableBuilder tableBuilder = new AttributeTableBuilder();
				Type t = typeof(Infragistics.Controls.Interactions.XamFormulaEditor);
				Assembly controlAssembly = t.Assembly;

				#region FormulaEditorDialogEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.FormulaEditorDialogEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Dialog",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialogEventArgs_Dialog_Property")),
				    new DisplayNameAttribute("Dialog")				);

				#endregion // FormulaEditorDialogEventArgs Properties

				#region FormulaEditorDialogClosingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.FormulaEditorDialogClosingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FormulaEditorDialogClosingEventArgs Properties

				#region FormulaEditorDialogDisplayingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.FormulaEditorDialogDisplayingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FormulaEditorDialogDisplayingEventArgs Properties

				#region FormulaEditorCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.FormulaEditorCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("FormulaEditorCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType")				);


				tableBuilder.AddCustomAttributes(t, "Key",
					new DescriptionAttribute(SR.GetString("FormulaEditorCommandSource_Key_Property")),
				    new DisplayNameAttribute("Key")				);

				#endregion // FormulaEditorCommandSource Properties

				#region FormulaEditorCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.FormulaEditorCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FormulaEditorCommandBase Properties

				#region DisplayDialogCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.DisplayDialogCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DisplayDialogCommand Properties

				#region XamFormulaEditor Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.XamFormulaEditor");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamFormulaEditorAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamFormulaEditorAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "Formula",
					new DescriptionAttribute(SR.GetString("XamFormulaEditor_Formula_Property")),
				    new DisplayNameAttribute("Formula"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HasSyntaxError",
					new DescriptionAttribute(SR.GetString("XamFormulaEditor_HasSyntaxError_Property")),
				    new DisplayNameAttribute("HasSyntaxError"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SyntaxError",
					new DescriptionAttribute(SR.GetString("XamFormulaEditor_SyntaxError_Property")),
				    new DisplayNameAttribute("SyntaxError"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Target",
					new DescriptionAttribute(SR.GetString("XamFormulaEditor_Target_Property")),
				    new DisplayNameAttribute("Target"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxLineCount",
					new DescriptionAttribute(SR.GetString("XamFormulaEditor_MaxLineCount_Property")),
				    new DisplayNameAttribute("MaxLineCount"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinLineCount",
					new DescriptionAttribute(SR.GetString("XamFormulaEditor_MinLineCount_Property")),
				    new DisplayNameAttribute("MinLineCount"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LocalizedStrings",
					new DescriptionAttribute(SR.GetString("XamFormulaEditor_LocalizedStrings_Property")),
				    new DisplayNameAttribute("LocalizedStrings"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);

				#endregion // XamFormulaEditor Properties

				#region FormulaEditorDialog Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.FormulaEditorDialog");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Formula",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_Formula_Property")),
				    new DisplayNameAttribute("Formula"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HasSyntaxError",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_HasSyntaxError_Property")),
				    new DisplayNameAttribute("HasSyntaxError"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SyntaxError",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_SyntaxError_Property")),
				    new DisplayNameAttribute("SyntaxError"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Target",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_Target_Property")),
				    new DisplayNameAttribute("Target"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FunctionCategories",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_FunctionCategories_Property")),
				    new DisplayNameAttribute("FunctionCategories"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FunctionSearchText",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_FunctionSearchText_Property")),
				    new DisplayNameAttribute("FunctionSearchText"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FunctionSearchType",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_FunctionSearchType_Property")),
				    new DisplayNameAttribute("FunctionSearchType"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FunctionSearchTypes",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_FunctionSearchTypes_Property")),
				    new DisplayNameAttribute("FunctionSearchTypes"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LocalizedStrings",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_LocalizedStrings_Property")),
				    new DisplayNameAttribute("LocalizedStrings"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Operators",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_Operators_Property")),
				    new DisplayNameAttribute("Operators"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CanRedo",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_CanRedo_Property")),
				    new DisplayNameAttribute("CanRedo"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CanUndo",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_CanUndo_Property")),
				    new DisplayNameAttribute("CanUndo"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ClearCommand",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_ClearCommand_Property")),
				    new DisplayNameAttribute("ClearCommand"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "RedoCommand",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_RedoCommand_Property")),
				    new DisplayNameAttribute("RedoCommand"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "UndoCommand",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_UndoCommand_Property")),
				    new DisplayNameAttribute("UndoCommand"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CurrentSyntaxErrorInfo",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_CurrentSyntaxErrorInfo_Property")),
				    new DisplayNameAttribute("CurrentSyntaxErrorInfo"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HasMultipleSyntaxErrors",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_HasMultipleSyntaxErrors_Property")),
				    new DisplayNameAttribute("HasMultipleSyntaxErrors"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MultipleSyntaxErrorsLabel",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_MultipleSyntaxErrorsLabel_Property")),
				    new DisplayNameAttribute("MultipleSyntaxErrorsLabel"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "NextSyntaxErrorCommand",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_NextSyntaxErrorCommand_Property")),
				    new DisplayNameAttribute("NextSyntaxErrorCommand"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "OperandSearchType",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_OperandSearchType_Property")),
				    new DisplayNameAttribute("OperandSearchType"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "OperandSearchTypes",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_OperandSearchTypes_Property")),
				    new DisplayNameAttribute("OperandSearchTypes"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PreviousSyntaxErrorCommand",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_PreviousSyntaxErrorCommand_Property")),
				    new DisplayNameAttribute("PreviousSyntaxErrorCommand"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SyntaxErrorInfos",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_SyntaxErrorInfos_Property")),
				    new DisplayNameAttribute("SyntaxErrorInfos"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Operands",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_Operands_Property")),
				    new DisplayNameAttribute("Operands"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "OperandSearchText",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_OperandSearchText_Property")),
				    new DisplayNameAttribute("OperandSearchText"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CancelCommand",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_CancelCommand_Property")),
				    new DisplayNameAttribute("CancelCommand"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CommitCommand",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_CommitCommand_Property")),
				    new DisplayNameAttribute("CommitCommand"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ShowDialogButtons",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_ShowDialogButtons_Property")),
				    new DisplayNameAttribute("ShowDialogButtons"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "UndoRedoButtonVisibility",
					new DescriptionAttribute(SR.GetString("FormulaEditorDialog_UndoRedoButtonVisibility_Property")),
				    new DisplayNameAttribute("UndoRedoButtonVisibility"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);

				#endregion // FormulaEditorDialog Properties

				#region FunctionCategory Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.FunctionCategory");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Functions",
					new DescriptionAttribute(SR.GetString("FunctionCategory_Functions_Property")),
				    new DisplayNameAttribute("Functions")				);


				tableBuilder.AddCustomAttributes(t, "IsExpanded",
					new DescriptionAttribute(SR.GetString("FunctionCategory_IsExpanded_Property")),
				    new DisplayNameAttribute("IsExpanded")				);


				tableBuilder.AddCustomAttributes(t, "Name",
					new DescriptionAttribute(SR.GetString("FunctionCategory_Name_Property")),
				    new DisplayNameAttribute("Name")				);

				#endregion // FunctionCategory Properties

				#region FunctionInfo Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.FunctionInfo");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Signature",
					new DescriptionAttribute(SR.GetString("FunctionInfo_Signature_Property")),
				    new DisplayNameAttribute("Signature")				);


				tableBuilder.AddCustomAttributes(t, "Description",
					new DescriptionAttribute(SR.GetString("FunctionInfo_Description_Property")),
				    new DisplayNameAttribute("Description")				);


				tableBuilder.AddCustomAttributes(t, "Name",
					new DescriptionAttribute(SR.GetString("FunctionInfo_Name_Property")),
				    new DisplayNameAttribute("Name")				);


				tableBuilder.AddCustomAttributes(t, "Function",
					new DescriptionAttribute(SR.GetString("FunctionInfo_Function_Property")),
				    new DisplayNameAttribute("Function")				);


				tableBuilder.AddCustomAttributes(t, "Dialog",
					new DescriptionAttribute(SR.GetString("FunctionInfo_Dialog_Property")),
				    new DisplayNameAttribute("Dialog")				);

				#endregion // FunctionInfo Properties

				#region FilteredCollection`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.FilteredCollection`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Count",
					new DescriptionAttribute(SR.GetString("FilteredCollection`1_Count_Property")),
				    new DisplayNameAttribute("Count")				);

				#endregion // FilteredCollection`1 Properties

				#region SearchTypeValue Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.SearchTypeValue");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Description",
					new DescriptionAttribute(SR.GetString("SearchTypeValue_Description_Property")),
				    new DisplayNameAttribute("Description")				);


				tableBuilder.AddCustomAttributes(t, "EnumValue",
					new DescriptionAttribute(SR.GetString("SearchTypeValue_EnumValue_Property")),
				    new DisplayNameAttribute("EnumValue")				);

				#endregion // SearchTypeValue Properties

				#region InsertFunctionCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.InsertFunctionCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // InsertFunctionCommand Properties

				#region InsertOperatorCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.InsertOperatorCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // InsertOperatorCommand Properties

				#region OperatorInfo Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.OperatorInfo");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("OperatorInfo_Value_Property")),
				    new DisplayNameAttribute("Value")				);


				tableBuilder.AddCustomAttributes(t, "DisplayText",
					new DescriptionAttribute(SR.GetString("OperatorInfo_DisplayText_Property")),
				    new DisplayNameAttribute("DisplayText")				);

				#endregion // OperatorInfo Properties

				#region ClearFormulaCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.ClearFormulaCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ClearFormulaCommand Properties

				#region RedoFormulaEditCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.RedoFormulaEditCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RedoFormulaEditCommand Properties

				#region UndoFormulaEditCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.UndoFormulaEditCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // UndoFormulaEditCommand Properties

				#region NextSyntaxErrorCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.NextSyntaxErrorCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NextSyntaxErrorCommand Properties

				#region PreviousSyntaxErrorCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.PreviousSyntaxErrorCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PreviousSyntaxErrorCommand Properties

				#region SyntaxErrorInfo Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.SyntaxErrorInfo");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Exception",
					new DescriptionAttribute(SR.GetString("SyntaxErrorInfo_Exception_Property")),
				    new DisplayNameAttribute("Exception")				);

				#endregion // SyntaxErrorInfo Properties

				#region OperandInfo Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.OperandInfo");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Children",
					new DescriptionAttribute(SR.GetString("OperandInfo_Children_Property")),
				    new DisplayNameAttribute("Children")				);


				tableBuilder.AddCustomAttributes(t, "IsExpanded",
					new DescriptionAttribute(SR.GetString("OperandInfo_IsExpanded_Property")),
				    new DisplayNameAttribute("IsExpanded")				);


				tableBuilder.AddCustomAttributes(t, "Name",
					new DescriptionAttribute(SR.GetString("OperandInfo_Name_Property")),
				    new DisplayNameAttribute("Name")				);


				tableBuilder.AddCustomAttributes(t, "IsDataReference",
					new DescriptionAttribute(SR.GetString("OperandInfo_IsDataReference_Property")),
				    new DisplayNameAttribute("IsDataReference")				);


				tableBuilder.AddCustomAttributes(t, "NodeType",
					new DescriptionAttribute(SR.GetString("OperandInfo_NodeType_Property")),
				    new DisplayNameAttribute("NodeType")				);


				tableBuilder.AddCustomAttributes(t, "Signature",
					new DescriptionAttribute(SR.GetString("OperandInfo_Signature_Property")),
				    new DisplayNameAttribute("Signature")				);


				tableBuilder.AddCustomAttributes(t, "IsEnabled",
					new DescriptionAttribute(SR.GetString("OperandInfo_IsEnabled_Property")),
				    new DisplayNameAttribute("IsEnabled")				);


				tableBuilder.AddCustomAttributes(t, "Dialog",
					new DescriptionAttribute(SR.GetString("OperandInfo_Dialog_Property")),
				    new DisplayNameAttribute("Dialog")				);


				tableBuilder.AddCustomAttributes(t, "Reference",
					new DescriptionAttribute(SR.GetString("OperandInfo_Reference_Property")),
				    new DisplayNameAttribute("Reference")				);

				#endregion // OperandInfo Properties

				#region InsertOperandCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.InsertOperandCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // InsertOperandCommand Properties

				#region CancelDialogCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.CancelDialogCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CancelDialogCommand Properties

				#region CommitDialogCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.CommitDialogCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CommitDialogCommand Properties

				#region FormulaEditorTextBox Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.FormulaEditorTextBox");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MaxLineCount",
					new DescriptionAttribute(SR.GetString("FormulaEditorTextBox_MaxLineCount_Property")),
				    new DisplayNameAttribute("MaxLineCount")				);


				tableBuilder.AddCustomAttributes(t, "MinLineCount",
					new DescriptionAttribute(SR.GetString("FormulaEditorTextBox_MinLineCount_Property")),
				    new DisplayNameAttribute("MinLineCount")				);


				tableBuilder.AddCustomAttributes(t, "Text",
					new DescriptionAttribute(SR.GetString("FormulaEditorTextBox_Text_Property")),
				    new DisplayNameAttribute("Text")				);

				#endregion // FormulaEditorTextBox Properties

				#region FormulaElementContentControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.FormulaElementContentControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FormulaElementContentControl Properties

				#region ContextualHelpHost Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.ContextualHelpHost");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ContextualHelpHost Properties

				#region FormulaEditorBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.FormulaEditorBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CanRedo",
					new DescriptionAttribute(SR.GetString("FormulaEditorBase_CanRedo_Property")),
				    new DisplayNameAttribute("CanRedo"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CanUndo",
					new DescriptionAttribute(SR.GetString("FormulaEditorBase_CanUndo_Property")),
				    new DisplayNameAttribute("CanUndo"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Formula",
					new DescriptionAttribute(SR.GetString("FormulaEditorBase_Formula_Property")),
				    new DisplayNameAttribute("Formula"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HasSyntaxError",
					new DescriptionAttribute(SR.GetString("FormulaEditorBase_HasSyntaxError_Property")),
				    new DisplayNameAttribute("HasSyntaxError"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "RedoCommand",
					new DescriptionAttribute(SR.GetString("FormulaEditorBase_RedoCommand_Property")),
				    new DisplayNameAttribute("RedoCommand"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SyntaxError",
					new DescriptionAttribute(SR.GetString("FormulaEditorBase_SyntaxError_Property")),
				    new DisplayNameAttribute("SyntaxError"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Target",
					new DescriptionAttribute(SR.GetString("FormulaEditorBase_Target_Property")),
				    new DisplayNameAttribute("Target"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "UndoCommand",
					new DescriptionAttribute(SR.GetString("FormulaEditorBase_UndoCommand_Property")),
				    new DisplayNameAttribute("UndoCommand"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ShowContextualHelp",
					new DescriptionAttribute(SR.GetString("FormulaEditorBase_ShowContextualHelp_Property")),
				    new DisplayNameAttribute("ShowContextualHelp"),
					new CategoryAttribute(SR.GetString("XamFormulaEditor_Properties"))
				);

				#endregion // FormulaEditorBase Properties

				#region AutoCompleteList Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.AutoCompleteList");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MaxItemsToDisplay",
					new DescriptionAttribute(SR.GetString("AutoCompleteList_MaxItemsToDisplay_Property")),
				    new DisplayNameAttribute("MaxItemsToDisplay")				);

				#endregion // AutoCompleteList Properties

				#region AutoCompleteListItem Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.AutoCompleteListItem");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AutoCompleteListItem Properties

				#region AutoCompleteItemCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.AutoCompleteItemCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AutoCompleteItemCommand Properties

				#region AutoCompleteListStackPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.AutoCompleteListStackPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CanHorizontallyScroll",
					new DescriptionAttribute(SR.GetString("AutoCompleteListStackPanel_CanHorizontallyScroll_Property")),
				    new DisplayNameAttribute("CanHorizontallyScroll")				);


				tableBuilder.AddCustomAttributes(t, "CanVerticallyScroll",
					new DescriptionAttribute(SR.GetString("AutoCompleteListStackPanel_CanVerticallyScroll_Property")),
				    new DisplayNameAttribute("CanVerticallyScroll")				);


				tableBuilder.AddCustomAttributes(t, "ExtentHeight",
					new DescriptionAttribute(SR.GetString("AutoCompleteListStackPanel_ExtentHeight_Property")),
				    new DisplayNameAttribute("ExtentHeight")				);


				tableBuilder.AddCustomAttributes(t, "ExtentWidth",
					new DescriptionAttribute(SR.GetString("AutoCompleteListStackPanel_ExtentWidth_Property")),
				    new DisplayNameAttribute("ExtentWidth")				);


				tableBuilder.AddCustomAttributes(t, "HorizontalOffset",
					new DescriptionAttribute(SR.GetString("AutoCompleteListStackPanel_HorizontalOffset_Property")),
				    new DisplayNameAttribute("HorizontalOffset")				);


				tableBuilder.AddCustomAttributes(t, "ScrollOwner",
					new DescriptionAttribute(SR.GetString("AutoCompleteListStackPanel_ScrollOwner_Property")),
				    new DisplayNameAttribute("ScrollOwner")				);


				tableBuilder.AddCustomAttributes(t, "VerticalOffset",
					new DescriptionAttribute(SR.GetString("AutoCompleteListStackPanel_VerticalOffset_Property")),
				    new DisplayNameAttribute("VerticalOffset")				);


				tableBuilder.AddCustomAttributes(t, "ViewportHeight",
					new DescriptionAttribute(SR.GetString("AutoCompleteListStackPanel_ViewportHeight_Property")),
				    new DisplayNameAttribute("ViewportHeight")				);


				tableBuilder.AddCustomAttributes(t, "ViewportWidth",
					new DescriptionAttribute(SR.GetString("AutoCompleteListStackPanel_ViewportWidth_Property")),
				    new DisplayNameAttribute("ViewportWidth")				);

				#endregion // AutoCompleteListStackPanel Properties
                this.AddCustomAttributes(tableBuilder);
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