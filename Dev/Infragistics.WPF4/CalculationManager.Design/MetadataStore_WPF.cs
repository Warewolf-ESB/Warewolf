using System;
using System.ComponentModel;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.Reflection;

[assembly: ProvideMetadata(typeof(InfragisticsWPF4.Calculations.XamCalculationManager.Design.MetadataStore))]

namespace InfragisticsWPF4.Calculations.XamCalculationManager.Design
{
	internal partial class MetadataStore : IProvideAttributeTable
	{
		public AttributeTable AttributeTable
		{
			get
			{
			    bool isVS = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName.Equals("devenv.exe"); 
				AttributeTableBuilder tableBuilder = new AttributeTableBuilder();
				Type t = typeof(Infragistics.Calculations.XamCalculationManager);
				Assembly controlAssembly = t.Assembly;

				#region Matcher Properties
				t = controlAssembly.GetType("PerCederberg.Grammatica.Parser.RE.Matcher");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // Matcher Properties

				#region RegExp Properties
				t = controlAssembly.GetType("PerCederberg.Grammatica.Parser.RE.RegExp");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RegExp Properties

				#region RegExpException Properties
				t = controlAssembly.GetType("PerCederberg.Grammatica.Parser.RE.RegExpException");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Message",
					new DescriptionAttribute(SR.GetString("RegExpException_Message_Property")),
				    new DisplayNameAttribute("Message")				);

				#endregion // RegExpException Properties

				#region Analyzer Properties
				t = controlAssembly.GetType("PerCederberg.Grammatica.Parser.Analyzer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // Analyzer Properties

				#region Node Properties
				t = controlAssembly.GetType("PerCederberg.Grammatica.Parser.Node");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // Node Properties

				#region ParseException Properties
				t = controlAssembly.GetType("PerCederberg.Grammatica.Parser.ParseException");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Message",
					new DescriptionAttribute(SR.GetString("ParseException_Message_Property")),
				    new DisplayNameAttribute("Message")				);

				#endregion // ParseException Properties

				#region Parser Properties
				t = controlAssembly.GetType("PerCederberg.Grammatica.Parser.Parser");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // Parser Properties

				#region ParserCreationException Properties
				t = controlAssembly.GetType("PerCederberg.Grammatica.Parser.ParserCreationException");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Message",
					new DescriptionAttribute(SR.GetString("ParserCreationException_Message_Property")),
				    new DisplayNameAttribute("Message")				);

				#endregion // ParserCreationException Properties

				#region ParserLogException Properties
				t = controlAssembly.GetType("PerCederberg.Grammatica.Parser.ParserLogException");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Message",
					new DescriptionAttribute(SR.GetString("ParserLogException_Message_Property")),
				    new DisplayNameAttribute("Message")				);

				#endregion // ParserLogException Properties

				#region Production Properties
				t = controlAssembly.GetType("PerCederberg.Grammatica.Parser.Production");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // Production Properties

				#region ProductionPattern Properties
				t = controlAssembly.GetType("PerCederberg.Grammatica.Parser.ProductionPattern");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ProductionPattern Properties

				#region ProductionPatternAlternative Properties
				t = controlAssembly.GetType("PerCederberg.Grammatica.Parser.ProductionPatternAlternative");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ProductionPatternAlternative Properties

				#region ProductionPatternElement Properties
				t = controlAssembly.GetType("PerCederberg.Grammatica.Parser.ProductionPatternElement");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ProductionPatternElement Properties

				#region RecursiveDescentParser Properties
				t = controlAssembly.GetType("PerCederberg.Grammatica.Parser.RecursiveDescentParser");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RecursiveDescentParser Properties

				#region Token Properties
				t = controlAssembly.GetType("PerCederberg.Grammatica.Parser.Token");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // Token Properties

				#region Tokenizer Properties
				t = controlAssembly.GetType("PerCederberg.Grammatica.Parser.Tokenizer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // Tokenizer Properties

				#region TokenPattern Properties
				t = controlAssembly.GetType("PerCederberg.Grammatica.Parser.TokenPattern");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TokenPattern Properties

				#region OneBasedArgumentNumberingAttribute Properties
				t = controlAssembly.GetType("Infragistics.Calculations.Engine.OneBasedArgumentNumberingAttribute");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ArgumentIndex",
					new DescriptionAttribute(SR.GetString("OneBasedArgumentNumberingAttribute_ArgumentIndex_Property")),
				    new DisplayNameAttribute("ArgumentIndex")				);

				#endregion // OneBasedArgumentNumberingAttribute Properties

				#region CalculationFunction Properties
				t = controlAssembly.GetType("Infragistics.Calculations.Engine.CalculationFunction");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsAlwaysDirty",
					new DescriptionAttribute(SR.GetString("CalculationFunction_IsAlwaysDirty_Property")),
				    new DisplayNameAttribute("IsAlwaysDirty")				);


				tableBuilder.AddCustomAttributes(t, "Name",
					new DescriptionAttribute(SR.GetString("CalculationFunction_Name_Property")),
				    new DisplayNameAttribute("Name")				);


				tableBuilder.AddCustomAttributes(t, "MinArgs",
					new DescriptionAttribute(SR.GetString("CalculationFunction_MinArgs_Property")),
				    new DisplayNameAttribute("MinArgs")				);


				tableBuilder.AddCustomAttributes(t, "MaxArgs",
					new DescriptionAttribute(SR.GetString("CalculationFunction_MaxArgs_Property")),
				    new DisplayNameAttribute("MaxArgs")				);


				tableBuilder.AddCustomAttributes(t, "Description",
					new DescriptionAttribute(SR.GetString("CalculationFunction_Description_Property")),
				    new DisplayNameAttribute("Description")				);


				tableBuilder.AddCustomAttributes(t, "Category",
					new DescriptionAttribute(SR.GetString("CalculationFunction_Category_Property")),
				    new DisplayNameAttribute("Category")				);


				tableBuilder.AddCustomAttributes(t, "ArgList",
					new DescriptionAttribute(SR.GetString("CalculationFunction_ArgList_Property")),
				    new DisplayNameAttribute("ArgList")				);


				tableBuilder.AddCustomAttributes(t, "ArgDescriptors",
					new DescriptionAttribute(SR.GetString("CalculationFunction_ArgDescriptors_Property")),
				    new DisplayNameAttribute("ArgDescriptors")				);

				#endregion // CalculationFunction Properties

				#region CalculationNumberStack Properties
				t = controlAssembly.GetType("Infragistics.Calculations.Engine.CalculationNumberStack");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CalculationNumberStack Properties

				#region UltraCalcReferenceCollection Properties
				t = controlAssembly.GetType("Infragistics.Calculations.Engine.UltraCalcReferenceCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsSynchronized",
					new DescriptionAttribute(SR.GetString("UltraCalcReferenceCollection_IsSynchronized_Property")),
				    new DisplayNameAttribute("IsSynchronized")				);


				tableBuilder.AddCustomAttributes(t, "Count",
					new DescriptionAttribute(SR.GetString("UltraCalcReferenceCollection_Count_Property")),
				    new DisplayNameAttribute("Count")				);


				tableBuilder.AddCustomAttributes(t, "SyncRoot",
					new DescriptionAttribute(SR.GetString("UltraCalcReferenceCollection_SyncRoot_Property")),
				    new DisplayNameAttribute("SyncRoot")				);

				#endregion // UltraCalcReferenceCollection Properties

				#region CalculationException Properties
				t = controlAssembly.GetType("Infragistics.Calculations.Engine.CalculationException");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CalculationException Properties

				#region CalculationValueException Properties
				t = controlAssembly.GetType("Infragistics.Calculations.Engine.CalculationValueException");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CalculationValueException Properties

				#region CalculationErrorValue Properties
				t = controlAssembly.GetType("Infragistics.Calculations.Engine.CalculationErrorValue");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Code",
					new DescriptionAttribute(SR.GetString("CalculationErrorValue_Code_Property")),
				    new DisplayNameAttribute("Code")				);


				tableBuilder.AddCustomAttributes(t, "Message",
					new DescriptionAttribute(SR.GetString("CalculationErrorValue_Message_Property")),
				    new DisplayNameAttribute("Message")				);


				tableBuilder.AddCustomAttributes(t, "ErrorValue",
					new DescriptionAttribute(SR.GetString("CalculationErrorValue_ErrorValue_Property")),
				    new DisplayNameAttribute("ErrorValue")				);

				#endregion // CalculationErrorValue Properties

				#region CalculationReferenceError Properties
				t = controlAssembly.GetType("Infragistics.Calculations.Engine.CalculationReferenceError");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Message",
					new DescriptionAttribute(SR.GetString("CalculationReferenceError_Message_Property")),
				    new DisplayNameAttribute("Message")				);

				#endregion // CalculationReferenceError Properties

				#region CalculationValue Properties
				t = controlAssembly.GetType("Infragistics.Calculations.Engine.CalculationValue");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsReference",
					new DescriptionAttribute(SR.GetString("CalculationValue_IsReference_Property")),
				    new DisplayNameAttribute("IsReference")				);


				tableBuilder.AddCustomAttributes(t, "IsError",
					new DescriptionAttribute(SR.GetString("CalculationValue_IsError_Property")),
				    new DisplayNameAttribute("IsError")				);


				tableBuilder.AddCustomAttributes(t, "IsNull",
					new DescriptionAttribute(SR.GetString("CalculationValue_IsNull_Property")),
				    new DisplayNameAttribute("IsNull")				);


				tableBuilder.AddCustomAttributes(t, "IsDBNull",
					new DescriptionAttribute(SR.GetString("CalculationValue_IsDBNull_Property")),
				    new DisplayNameAttribute("IsDBNull")				);


				tableBuilder.AddCustomAttributes(t, "IsString",
					new DescriptionAttribute(SR.GetString("CalculationValue_IsString_Property")),
				    new DisplayNameAttribute("IsString")				);


				tableBuilder.AddCustomAttributes(t, "IsBoolean",
					new DescriptionAttribute(SR.GetString("CalculationValue_IsBoolean_Property")),
				    new DisplayNameAttribute("IsBoolean")				);


				tableBuilder.AddCustomAttributes(t, "IsDateTime",
					new DescriptionAttribute(SR.GetString("CalculationValue_IsDateTime_Property")),
				    new DisplayNameAttribute("IsDateTime")				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("CalculationValue_Value_Property")),
				    new DisplayNameAttribute("Value")				);

				#endregion // CalculationValue Properties

				#region RefBase Properties
				t = controlAssembly.GetType("Infragistics.Calculations.Engine.RefBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Context",
					new DescriptionAttribute(SR.GetString("RefBase_Context_Property")),
				    new DisplayNameAttribute("Context")				);


				tableBuilder.AddCustomAttributes(t, "BaseParent",
					new DescriptionAttribute(SR.GetString("RefBase_BaseParent_Property")),
				    new DisplayNameAttribute("BaseParent")				);


				tableBuilder.AddCustomAttributes(t, "IsAnchored",
					new DescriptionAttribute(SR.GetString("RefBase_IsAnchored_Property")),
				    new DisplayNameAttribute("IsAnchored")				);


				tableBuilder.AddCustomAttributes(t, "IsRange",
					new DescriptionAttribute(SR.GetString("RefBase_IsRange_Property")),
				    new DisplayNameAttribute("IsRange")				);


				tableBuilder.AddCustomAttributes(t, "WrappedReference",
					new DescriptionAttribute(SR.GetString("RefBase_WrappedReference_Property")),
				    new DisplayNameAttribute("WrappedReference")				);


				tableBuilder.AddCustomAttributes(t, "ParsedReference",
					new DescriptionAttribute(SR.GetString("RefBase_ParsedReference_Property")),
				    new DisplayNameAttribute("ParsedReference")				);


				tableBuilder.AddCustomAttributes(t, "RelativeReference",
					new DescriptionAttribute(SR.GetString("RefBase_RelativeReference_Property")),
				    new DisplayNameAttribute("RelativeReference")				);


				tableBuilder.AddCustomAttributes(t, "AbsoluteName",
					new DescriptionAttribute(SR.GetString("RefBase_AbsoluteName_Property")),
				    new DisplayNameAttribute("AbsoluteName")				);


				tableBuilder.AddCustomAttributes(t, "NormalizedAbsoluteName",
					new DescriptionAttribute(SR.GetString("RefBase_NormalizedAbsoluteName_Property")),
				    new DisplayNameAttribute("NormalizedAbsoluteName")				);


				tableBuilder.AddCustomAttributes(t, "ElementName",
					new DescriptionAttribute(SR.GetString("RefBase_ElementName_Property")),
				    new DisplayNameAttribute("ElementName")				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("RefBase_Value_Property")),
				    new DisplayNameAttribute("Value")				);


				tableBuilder.AddCustomAttributes(t, "Formula",
					new DescriptionAttribute(SR.GetString("RefBase_Formula_Property")),
				    new DisplayNameAttribute("Formula")				);


				tableBuilder.AddCustomAttributes(t, "RecalcDeferred",
					new DescriptionAttribute(SR.GetString("RefBase_RecalcDeferred_Property")),
				    new DisplayNameAttribute("RecalcDeferred")				);


				tableBuilder.AddCustomAttributes(t, "RecalcVisible",
					new DescriptionAttribute(SR.GetString("RefBase_RecalcVisible_Property")),
				    new DisplayNameAttribute("RecalcVisible")				);


				tableBuilder.AddCustomAttributes(t, "HasRelativeIndex",
					new DescriptionAttribute(SR.GetString("RefBase_HasRelativeIndex_Property")),
				    new DisplayNameAttribute("HasRelativeIndex")				);


				tableBuilder.AddCustomAttributes(t, "HasAbsoluteIndex",
					new DescriptionAttribute(SR.GetString("RefBase_HasAbsoluteIndex_Property")),
				    new DisplayNameAttribute("HasAbsoluteIndex")				);


				tableBuilder.AddCustomAttributes(t, "HasScopeAll",
					new DescriptionAttribute(SR.GetString("RefBase_HasScopeAll_Property")),
				    new DisplayNameAttribute("HasScopeAll")				);


				tableBuilder.AddCustomAttributes(t, "References",
					new DescriptionAttribute(SR.GetString("RefBase_References_Property")),
				    new DisplayNameAttribute("References")				);


				tableBuilder.AddCustomAttributes(t, "IsEnumerable",
					new DescriptionAttribute(SR.GetString("RefBase_IsEnumerable_Property")),
				    new DisplayNameAttribute("IsEnumerable")				);


				tableBuilder.AddCustomAttributes(t, "Parent",
					new DescriptionAttribute(SR.GetString("RefBase_Parent_Property")),
				    new DisplayNameAttribute("Parent")				);


				tableBuilder.AddCustomAttributes(t, "IsDataReference",
					new DescriptionAttribute(SR.GetString("RefBase_IsDataReference_Property")),
				    new DisplayNameAttribute("IsDataReference")				);


				tableBuilder.AddCustomAttributes(t, "IsDisposedReference",
					new DescriptionAttribute(SR.GetString("RefBase_IsDisposedReference_Property")),
				    new DisplayNameAttribute("IsDisposedReference")				);


				tableBuilder.AddCustomAttributes(t, "ShouldFormulaEditorIncludeIndex",
					new DescriptionAttribute(SR.GetString("RefBase_ShouldFormulaEditorIncludeIndex_Property")),
				    new DisplayNameAttribute("ShouldFormulaEditorIncludeIndex")				);

				#endregion // RefBase Properties

				#region RefRange Properties
				t = controlAssembly.GetType("Infragistics.Calculations.Engine.RefRange");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "FromBase",
					new DescriptionAttribute(SR.GetString("RefRange_FromBase_Property")),
				    new DisplayNameAttribute("FromBase")				);


				tableBuilder.AddCustomAttributes(t, "NormalizedFromBase",
					new DescriptionAttribute(SR.GetString("RefRange_NormalizedFromBase_Property")),
				    new DisplayNameAttribute("NormalizedFromBase")				);


				tableBuilder.AddCustomAttributes(t, "FromRef",
					new DescriptionAttribute(SR.GetString("RefRange_FromRef_Property")),
				    new DisplayNameAttribute("FromRef")				);


				tableBuilder.AddCustomAttributes(t, "NormalizedFromRef",
					new DescriptionAttribute(SR.GetString("RefRange_NormalizedFromRef_Property")),
				    new DisplayNameAttribute("NormalizedFromRef")				);


				tableBuilder.AddCustomAttributes(t, "ToBase",
					new DescriptionAttribute(SR.GetString("RefRange_ToBase_Property")),
				    new DisplayNameAttribute("ToBase")				);


				tableBuilder.AddCustomAttributes(t, "NormalizedToBase",
					new DescriptionAttribute(SR.GetString("RefRange_NormalizedToBase_Property")),
				    new DisplayNameAttribute("NormalizedToBase")				);


				tableBuilder.AddCustomAttributes(t, "ToRef",
					new DescriptionAttribute(SR.GetString("RefRange_ToRef_Property")),
				    new DisplayNameAttribute("ToRef")				);


				tableBuilder.AddCustomAttributes(t, "NormalizedToRef",
					new DescriptionAttribute(SR.GetString("RefRange_NormalizedToRef_Property")),
				    new DisplayNameAttribute("NormalizedToRef")				);


				tableBuilder.AddCustomAttributes(t, "IsRange",
					new DescriptionAttribute(SR.GetString("RefRange_IsRange_Property")),
				    new DisplayNameAttribute("IsRange")				);


				tableBuilder.AddCustomAttributes(t, "ElementName",
					new DescriptionAttribute(SR.GetString("RefRange_ElementName_Property")),
				    new DisplayNameAttribute("ElementName")				);


				tableBuilder.AddCustomAttributes(t, "BaseParent",
					new DescriptionAttribute(SR.GetString("RefRange_BaseParent_Property")),
				    new DisplayNameAttribute("BaseParent")				);


				tableBuilder.AddCustomAttributes(t, "IsDataReference",
					new DescriptionAttribute(SR.GetString("RefRange_IsDataReference_Property")),
				    new DisplayNameAttribute("IsDataReference")				);


				tableBuilder.AddCustomAttributes(t, "AbsoluteName",
					new DescriptionAttribute(SR.GetString("RefRange_AbsoluteName_Property")),
				    new DisplayNameAttribute("AbsoluteName")				);


				tableBuilder.AddCustomAttributes(t, "NormalizedAbsoluteName",
					new DescriptionAttribute(SR.GetString("RefRange_NormalizedAbsoluteName_Property")),
				    new DisplayNameAttribute("NormalizedAbsoluteName")				);


				tableBuilder.AddCustomAttributes(t, "HasRelativeIndex",
					new DescriptionAttribute(SR.GetString("RefRange_HasRelativeIndex_Property")),
				    new DisplayNameAttribute("HasRelativeIndex")				);


				tableBuilder.AddCustomAttributes(t, "References",
					new DescriptionAttribute(SR.GetString("RefRange_References_Property")),
				    new DisplayNameAttribute("References")				);


				tableBuilder.AddCustomAttributes(t, "IsEnumerable",
					new DescriptionAttribute(SR.GetString("RefRange_IsEnumerable_Property")),
				    new DisplayNameAttribute("IsEnumerable")				);

				#endregion // RefRange Properties

				#region RefTuple Properties
				t = controlAssembly.GetType("Infragistics.Calculations.Engine.RefTuple");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Type",
					new DescriptionAttribute(SR.GetString("RefTuple_Type_Property")),
				    new DisplayNameAttribute("Type")				);


				tableBuilder.AddCustomAttributes(t, "Scope",
					new DescriptionAttribute(SR.GetString("RefTuple_Scope_Property")),
				    new DisplayNameAttribute("Scope")				);


				tableBuilder.AddCustomAttributes(t, "Name",
					new DescriptionAttribute(SR.GetString("RefTuple_Name_Property")),
				    new DisplayNameAttribute("Name")				);


				tableBuilder.AddCustomAttributes(t, "Marked",
					new DescriptionAttribute(SR.GetString("RefTuple_Marked_Property")),
				    new DisplayNameAttribute("Marked")				);


				tableBuilder.AddCustomAttributes(t, "Last",
					new DescriptionAttribute(SR.GetString("RefTuple_Last_Property")),
				    new DisplayNameAttribute("Last")				);


				tableBuilder.AddCustomAttributes(t, "NextToLast",
					new DescriptionAttribute(SR.GetString("RefTuple_NextToLast_Property")),
				    new DisplayNameAttribute("NextToLast")				);


				tableBuilder.AddCustomAttributes(t, "ScopeID",
					new DescriptionAttribute(SR.GetString("RefTuple_ScopeID_Property")),
				    new DisplayNameAttribute("ScopeID")				);


				tableBuilder.AddCustomAttributes(t, "ScopeIndex",
					new DescriptionAttribute(SR.GetString("RefTuple_ScopeIndex_Property")),
				    new DisplayNameAttribute("ScopeIndex")				);


				tableBuilder.AddCustomAttributes(t, "IsAbsolute",
					new DescriptionAttribute(SR.GetString("RefTuple_IsAbsolute_Property")),
				    new DisplayNameAttribute("IsAbsolute")				);

				#endregion // RefTuple Properties

				#region RefParser Properties
				t = controlAssembly.GetType("Infragistics.Calculations.Engine.RefParser");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "TupleCount",
					new DescriptionAttribute(SR.GetString("RefParser_TupleCount_Property")),
				    new DisplayNameAttribute("TupleCount")				);


				tableBuilder.AddCustomAttributes(t, "LastTuple",
					new DescriptionAttribute(SR.GetString("RefParser_LastTuple_Property")),
				    new DisplayNameAttribute("LastTuple")				);


				tableBuilder.AddCustomAttributes(t, "NextToLastTuple",
					new DescriptionAttribute(SR.GetString("RefParser_NextToLastTuple_Property")),
				    new DisplayNameAttribute("NextToLastTuple")				);


				tableBuilder.AddCustomAttributes(t, "IsFullyQualified",
					new DescriptionAttribute(SR.GetString("RefParser_IsFullyQualified_Property")),
				    new DisplayNameAttribute("IsFullyQualified")				);


				tableBuilder.AddCustomAttributes(t, "IsRoot",
					new DescriptionAttribute(SR.GetString("RefParser_IsRoot_Property")),
				    new DisplayNameAttribute("IsRoot")				);


				tableBuilder.AddCustomAttributes(t, "IsRelative",
					new DescriptionAttribute(SR.GetString("RefParser_IsRelative_Property")),
				    new DisplayNameAttribute("IsRelative")				);


				tableBuilder.AddCustomAttributes(t, "HasRelativeIndex",
					new DescriptionAttribute(SR.GetString("RefParser_HasRelativeIndex_Property")),
				    new DisplayNameAttribute("HasRelativeIndex")				);


				tableBuilder.AddCustomAttributes(t, "HasScopeAll",
					new DescriptionAttribute(SR.GetString("RefParser_HasScopeAll_Property")),
				    new DisplayNameAttribute("HasScopeAll")				);


				tableBuilder.AddCustomAttributes(t, "HasAbsoluteIndex",
					new DescriptionAttribute(SR.GetString("RefParser_HasAbsoluteIndex_Property")),
				    new DisplayNameAttribute("HasAbsoluteIndex")				);


				tableBuilder.AddCustomAttributes(t, "HasSummaryScope",
					new DescriptionAttribute(SR.GetString("RefParser_HasSummaryScope_Property")),
				    new DisplayNameAttribute("HasSummaryScope")				);

				#endregion // RefParser Properties

				#region RefUnAnchored Properties
				t = controlAssembly.GetType("Infragistics.Calculations.Engine.RefUnAnchored");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsUnQualifiedReference",
					new DescriptionAttribute(SR.GetString("RefUnAnchored_IsUnQualifiedReference_Property")),
				    new DisplayNameAttribute("IsUnQualifiedReference")				);


				tableBuilder.AddCustomAttributes(t, "ElementName",
					new DescriptionAttribute(SR.GetString("RefUnAnchored_ElementName_Property")),
				    new DisplayNameAttribute("ElementName")				);


				tableBuilder.AddCustomAttributes(t, "BaseParent",
					new DescriptionAttribute(SR.GetString("RefUnAnchored_BaseParent_Property")),
				    new DisplayNameAttribute("BaseParent")				);


				tableBuilder.AddCustomAttributes(t, "IsRange",
					new DescriptionAttribute(SR.GetString("RefUnAnchored_IsRange_Property")),
				    new DisplayNameAttribute("IsRange")				);


				tableBuilder.AddCustomAttributes(t, "IsAnchored",
					new DescriptionAttribute(SR.GetString("RefUnAnchored_IsAnchored_Property")),
				    new DisplayNameAttribute("IsAnchored")				);


				tableBuilder.AddCustomAttributes(t, "IsDataReference",
					new DescriptionAttribute(SR.GetString("RefUnAnchored_IsDataReference_Property")),
				    new DisplayNameAttribute("IsDataReference")				);


				tableBuilder.AddCustomAttributes(t, "WrappedReference",
					new DescriptionAttribute(SR.GetString("RefUnAnchored_WrappedReference_Property")),
				    new DisplayNameAttribute("WrappedReference")				);


				tableBuilder.AddCustomAttributes(t, "ParsedReference",
					new DescriptionAttribute(SR.GetString("RefUnAnchored_ParsedReference_Property")),
				    new DisplayNameAttribute("ParsedReference")				);


				tableBuilder.AddCustomAttributes(t, "AbsoluteName",
					new DescriptionAttribute(SR.GetString("RefUnAnchored_AbsoluteName_Property")),
				    new DisplayNameAttribute("AbsoluteName")				);


				tableBuilder.AddCustomAttributes(t, "RelativeReference",
					new DescriptionAttribute(SR.GetString("RefUnAnchored_RelativeReference_Property")),
				    new DisplayNameAttribute("RelativeReference")				);


				tableBuilder.AddCustomAttributes(t, "Context",
					new DescriptionAttribute(SR.GetString("RefUnAnchored_Context_Property")),
				    new DisplayNameAttribute("Context")				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("RefUnAnchored_Value_Property")),
				    new DisplayNameAttribute("Value")				);


				tableBuilder.AddCustomAttributes(t, "Formula",
					new DescriptionAttribute(SR.GetString("RefUnAnchored_Formula_Property")),
				    new DisplayNameAttribute("Formula")				);


				tableBuilder.AddCustomAttributes(t, "RecalcVisible",
					new DescriptionAttribute(SR.GetString("RefUnAnchored_RecalcVisible_Property")),
				    new DisplayNameAttribute("RecalcVisible")				);


				tableBuilder.AddCustomAttributes(t, "RecalcDeferred",
					new DescriptionAttribute(SR.GetString("RefUnAnchored_RecalcDeferred_Property")),
				    new DisplayNameAttribute("RecalcDeferred")				);


				tableBuilder.AddCustomAttributes(t, "HasRelativeIndex",
					new DescriptionAttribute(SR.GetString("RefUnAnchored_HasRelativeIndex_Property")),
				    new DisplayNameAttribute("HasRelativeIndex")				);


				tableBuilder.AddCustomAttributes(t, "HasScopeAll",
					new DescriptionAttribute(SR.GetString("RefUnAnchored_HasScopeAll_Property")),
				    new DisplayNameAttribute("HasScopeAll")				);


				tableBuilder.AddCustomAttributes(t, "HasAbsoluteIndex",
					new DescriptionAttribute(SR.GetString("RefUnAnchored_HasAbsoluteIndex_Property")),
				    new DisplayNameAttribute("HasAbsoluteIndex")				);


				tableBuilder.AddCustomAttributes(t, "References",
					new DescriptionAttribute(SR.GetString("RefUnAnchored_References_Property")),
				    new DisplayNameAttribute("References")				);


				tableBuilder.AddCustomAttributes(t, "IsEnumerable",
					new DescriptionAttribute(SR.GetString("RefUnAnchored_IsEnumerable_Property")),
				    new DisplayNameAttribute("IsEnumerable")				);


				tableBuilder.AddCustomAttributes(t, "IsDisposedReference",
					new DescriptionAttribute(SR.GetString("RefUnAnchored_IsDisposedReference_Property")),
				    new DisplayNameAttribute("IsDisposedReference")				);


				tableBuilder.AddCustomAttributes(t, "ShouldFormulaEditorIncludeIndex",
					new DescriptionAttribute(SR.GetString("RefUnAnchored_ShouldFormulaEditorIncludeIndex_Property")),
				    new DisplayNameAttribute("ShouldFormulaEditorIncludeIndex")				);

				#endregion // RefUnAnchored Properties

				#region CalculationResult Properties
				t = controlAssembly.GetType("Infragistics.Calculations.CalculationResult");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CalculationValue",
					new DescriptionAttribute(SR.GetString("CalculationResult_CalculationValue_Property")),
				    new DisplayNameAttribute("CalculationValue")				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("CalculationResult_Value_Property")),
				    new DisplayNameAttribute("Value")				);


				tableBuilder.AddCustomAttributes(t, "ErrorText",
					new DescriptionAttribute(SR.GetString("CalculationResult_ErrorText_Property")),
				    new DisplayNameAttribute("ErrorText")				);


				tableBuilder.AddCustomAttributes(t, "IsError",
					new DescriptionAttribute(SR.GetString("CalculationResult_IsError_Property")),
				    new DisplayNameAttribute("IsError")				);

				#endregion // CalculationResult Properties

				#region FormulaErrorEventArgsBase Properties
				t = controlAssembly.GetType("Infragistics.Calculations.FormulaErrorEventArgsBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Context",
					new DescriptionAttribute(SR.GetString("FormulaErrorEventArgsBase_Context_Property")),
				    new DisplayNameAttribute("Context")				);


				tableBuilder.AddCustomAttributes(t, "ErrorDisplayText",
					new DescriptionAttribute(SR.GetString("FormulaErrorEventArgsBase_ErrorDisplayText_Property")),
				    new DisplayNameAttribute("ErrorDisplayText")				);

				#endregion // FormulaErrorEventArgsBase Properties

				#region FormulaCalculationErrorEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Calculations.FormulaCalculationErrorEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ErrorValue",
					new DescriptionAttribute(SR.GetString("FormulaCalculationErrorEventArgs_ErrorValue_Property")),
				    new DisplayNameAttribute("ErrorValue")				);


				tableBuilder.AddCustomAttributes(t, "ErrorInfo",
					new DescriptionAttribute(SR.GetString("FormulaCalculationErrorEventArgs_ErrorInfo_Property")),
				    new DisplayNameAttribute("ErrorInfo")				);

				#endregion // FormulaCalculationErrorEventArgs Properties

				#region FormulaReferenceErrorEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Calculations.FormulaReferenceErrorEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FormulaReferenceErrorEventArgs Properties

				#region ValueDirtiedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Calculations.ValueDirtiedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Context",
					new DescriptionAttribute(SR.GetString("ValueDirtiedEventArgs_Context_Property")),
				    new DisplayNameAttribute("Context")				);


				tableBuilder.AddCustomAttributes(t, "Action",
					new DescriptionAttribute(SR.GetString("ValueDirtiedEventArgs_Action_Property")),
				    new DisplayNameAttribute("Action")				);

				#endregion // ValueDirtiedEventArgs Properties

				#region FormulaSyntaxErrorEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Calculations.FormulaSyntaxErrorEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FormulaSyntaxErrorEventArgs Properties

				#region FormulaCircularityErrorEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Calculations.FormulaCircularityErrorEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DisplayErrorMessage",
					new DescriptionAttribute(SR.GetString("FormulaCircularityErrorEventArgs_DisplayErrorMessage_Property")),
				    new DisplayNameAttribute("DisplayErrorMessage")				);

				#endregion // FormulaCircularityErrorEventArgs Properties

				#region XamCalculationManager Properties
				t = controlAssembly.GetType("Infragistics.Calculations.XamCalculationManager");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamCalculationManagerAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamCalculationManagerAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "AsynchronousCalculationDuration",
					new DescriptionAttribute(SR.GetString("XamCalculationManager_AsynchronousCalculationDuration_Property")),
				    new DisplayNameAttribute("AsynchronousCalculationDuration"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AsynchronousCalculationInterval",
					new DescriptionAttribute(SR.GetString("XamCalculationManager_AsynchronousCalculationInterval_Property")),
				    new DisplayNameAttribute("AsynchronousCalculationInterval"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CalculationFrequency",
					new DescriptionAttribute(SR.GetString("XamCalculationManager_CalculationFrequency_Property")),
				    new DisplayNameAttribute("CalculationFrequency"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DeferredCalculationsEnabled",
					new DescriptionAttribute(SR.GetString("XamCalculationManager_DeferredCalculationsEnabled_Property")),
				    new DisplayNameAttribute("DeferredCalculationsEnabled"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "NamedReferences",
					new DescriptionAttribute(SR.GetString("XamCalculationManager_NamedReferences_Property")),
				    new DisplayNameAttribute("NamedReferences"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AreCalculationsSuspended",
					new DescriptionAttribute(SR.GetString("XamCalculationManager_AreCalculationsSuspended_Property")),
				    new DisplayNameAttribute("AreCalculationsSuspended"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);

				#endregion // XamCalculationManager Properties

				#region ItemCalculation Properties
				t = controlAssembly.GetType("Infragistics.Calculations.ItemCalculation");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "TargetProperty",
					new DescriptionAttribute(SR.GetString("ItemCalculation_TargetProperty_Property")),
				    new DisplayNameAttribute("TargetProperty"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ReferenceId",
					new DescriptionAttribute(SR.GetString("ItemCalculation_ReferenceId_Property")),
				    new DisplayNameAttribute("ReferenceId"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Formula",
					new DescriptionAttribute(SR.GetString("ItemCalculation_Formula_Property")),
				    new DisplayNameAttribute("Formula"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TreatAsType",
					new DescriptionAttribute(SR.GetString("ItemCalculation_TreatAsType_Property")),
				    new DisplayNameAttribute("TreatAsType"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TreatAsTypeName",
					new DescriptionAttribute(SR.GetString("ItemCalculation_TreatAsTypeName_Property")),
				    new DisplayNameAttribute("TreatAsTypeName"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TreatAsTypeResolved",
					new DescriptionAttribute(SR.GetString("ItemCalculation_TreatAsTypeResolved_Property")),
				    new DisplayNameAttribute("TreatAsTypeResolved")				);

				#endregion // ItemCalculation Properties

				#region ItemCalculator Properties
				t = controlAssembly.GetType("Infragistics.Calculations.ItemCalculator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Item",
					new DescriptionAttribute(SR.GetString("ItemCalculator_Item_Property")),
				    new DisplayNameAttribute("Item"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Calculations",
					new DescriptionAttribute(SR.GetString("ItemCalculator_Calculations_Property")),
				    new DisplayNameAttribute("Calculations"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CalculationManager",
					new DescriptionAttribute(SR.GetString("ItemCalculator_CalculationManager_Property")),
				    new DisplayNameAttribute("CalculationManager"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Results",
					new DescriptionAttribute(SR.GetString("ItemCalculator_Results_Property")),
				    new DisplayNameAttribute("Results"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);

				#endregion // ItemCalculator Properties

				#region ItemCalculatorElement Properties
				t = controlAssembly.GetType("Infragistics.Calculations.ItemCalculatorElement");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Item",
					new DescriptionAttribute(SR.GetString("ItemCalculatorElement_Item_Property")),
				    new DisplayNameAttribute("Item"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Calculator",
					new DescriptionAttribute(SR.GetString("ItemCalculatorElement_Calculator_Property")),
				    new DisplayNameAttribute("Calculator"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);

				#endregion // ItemCalculatorElement Properties

				#region ListCalculation Properties
				t = controlAssembly.GetType("Infragistics.Calculations.ListCalculation");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ReferenceId",
					new DescriptionAttribute(SR.GetString("ListCalculation_ReferenceId_Property")),
				    new DisplayNameAttribute("ReferenceId"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Formula",
					new DescriptionAttribute(SR.GetString("ListCalculation_Formula_Property")),
				    new DisplayNameAttribute("Formula"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);

				#endregion // ListCalculation Properties

				#region ListCalculator Properties
				t = controlAssembly.GetType("Infragistics.Calculations.ListCalculator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CalculationManager",
					new DescriptionAttribute(SR.GetString("ListCalculator_CalculationManager_Property")),
				    new DisplayNameAttribute("CalculationManager"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemCalculations",
					new DescriptionAttribute(SR.GetString("ListCalculator_ItemCalculations_Property")),
				    new DisplayNameAttribute("ItemCalculations"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ListCalculations",
					new DescriptionAttribute(SR.GetString("ListCalculator_ListCalculations_Property")),
				    new DisplayNameAttribute("ListCalculations"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ListResults",
					new DescriptionAttribute(SR.GetString("ListCalculator_ListResults_Property")),
				    new DisplayNameAttribute("ListResults"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Items",
					new DescriptionAttribute(SR.GetString("ListCalculator_Items_Property")),
				    new DisplayNameAttribute("Items"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemsSource",
					new DescriptionAttribute(SR.GetString("ListCalculator_ItemsSource_Property")),
				    new DisplayNameAttribute("ItemsSource"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);

				#endregion // ListCalculator Properties

				#region ListCalculatorElement Properties
				t = controlAssembly.GetType("Infragistics.Calculations.ListCalculatorElement");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Calculator",
					new DescriptionAttribute(SR.GetString("ListCalculatorElement_Calculator_Property")),
				    new DisplayNameAttribute("Calculator"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemsSource",
					new DescriptionAttribute(SR.GetString("ListCalculatorElement_ItemsSource_Property")),
				    new DisplayNameAttribute("ItemsSource"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);

				#endregion // ListCalculatorElement Properties

				#region CalculationReferenceNode Properties
				t = controlAssembly.GetType("Infragistics.Calculations.CalculationReferenceNode");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Reference",
					new DescriptionAttribute(SR.GetString("CalculationReferenceNode_Reference_Property")),
				    new DisplayNameAttribute("Reference")				);


				tableBuilder.AddCustomAttributes(t, "ChildReferences",
					new DescriptionAttribute(SR.GetString("CalculationReferenceNode_ChildReferences_Property")),
				    new DisplayNameAttribute("ChildReferences")				);


				tableBuilder.AddCustomAttributes(t, "IsDataReference",
					new DescriptionAttribute(SR.GetString("CalculationReferenceNode_IsDataReference_Property")),
				    new DisplayNameAttribute("IsDataReference")				);


				tableBuilder.AddCustomAttributes(t, "DisplayName",
					new DescriptionAttribute(SR.GetString("CalculationReferenceNode_DisplayName_Property")),
				    new DisplayNameAttribute("DisplayName")				);


				tableBuilder.AddCustomAttributes(t, "DisplayNameResolved",
					new DescriptionAttribute(SR.GetString("CalculationReferenceNode_DisplayNameResolved_Property")),
				    new DisplayNameAttribute("DisplayNameResolved")				);


				tableBuilder.AddCustomAttributes(t, "NodeType",
					new DescriptionAttribute(SR.GetString("CalculationReferenceNode_NodeType_Property")),
				    new DisplayNameAttribute("NodeType")				);


				tableBuilder.AddCustomAttributes(t, "IsExpanded",
					new DescriptionAttribute(SR.GetString("CalculationReferenceNode_IsExpanded_Property")),
				    new DisplayNameAttribute("IsExpanded")				);


				tableBuilder.AddCustomAttributes(t, "SortPriority",
					new DescriptionAttribute(SR.GetString("CalculationReferenceNode_SortPriority_Property")),
				    new DisplayNameAttribute("SortPriority")				);

				#endregion // CalculationReferenceNode Properties

				#region NamedReference Properties
				t = controlAssembly.GetType("Infragistics.Calculations.NamedReference");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Category",
					new DescriptionAttribute(SR.GetString("NamedReference_Category_Property")),
				    new DisplayNameAttribute("Category"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Formula",
					new DescriptionAttribute(SR.GetString("NamedReference_Formula_Property")),
				    new DisplayNameAttribute("Formula"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Result",
					new DescriptionAttribute(SR.GetString("NamedReference_Result_Property")),
				    new DisplayNameAttribute("Result"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("NamedReference_Value_Property")),
				    new DisplayNameAttribute("Value"),
				    new TypeConverterAttribute(typeof(StringConverter))
,
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ReferenceId",
					new DescriptionAttribute(SR.GetString("NamedReference_ReferenceId_Property")),
				    new DisplayNameAttribute("ReferenceId"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);

				#endregion // NamedReference Properties

				#region NamedReferenceCollection Properties
				t = controlAssembly.GetType("Infragistics.Calculations.NamedReferenceCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NamedReferenceCollection Properties

				#region CustomCalculationFunction Properties
				t = controlAssembly.GetType("Infragistics.Calculations.CustomCalculationFunction");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ArgDescriptors",
					new DescriptionAttribute(SR.GetString("CustomCalculationFunction_ArgDescriptors_Property")),
				    new DisplayNameAttribute("ArgDescriptors")				);


				tableBuilder.AddCustomAttributes(t, "ArgList",
					new DescriptionAttribute(SR.GetString("CustomCalculationFunction_ArgList_Property")),
				    new DisplayNameAttribute("ArgList")				);


				tableBuilder.AddCustomAttributes(t, "Category",
					new DescriptionAttribute(SR.GetString("CustomCalculationFunction_Category_Property")),
				    new DisplayNameAttribute("Category")				);


				tableBuilder.AddCustomAttributes(t, "Description",
					new DescriptionAttribute(SR.GetString("CustomCalculationFunction_Description_Property")),
				    new DisplayNameAttribute("Description")				);


				tableBuilder.AddCustomAttributes(t, "MaxArgs",
					new DescriptionAttribute(SR.GetString("CustomCalculationFunction_MaxArgs_Property")),
				    new DisplayNameAttribute("MaxArgs")				);


				tableBuilder.AddCustomAttributes(t, "MinArgs",
					new DescriptionAttribute(SR.GetString("CustomCalculationFunction_MinArgs_Property")),
				    new DisplayNameAttribute("MinArgs")				);


				tableBuilder.AddCustomAttributes(t, "Name",
					new DescriptionAttribute(SR.GetString("CustomCalculationFunction_Name_Property")),
				    new DisplayNameAttribute("Name")				);

				#endregion // CustomCalculationFunction Properties

				#region ControlCalculationSettings Properties
				t = controlAssembly.GetType("Infragistics.Calculations.ControlCalculationSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Binding",
					new DescriptionAttribute(SR.GetString("ControlCalculationSettings_Binding_Property")),
				    new DisplayNameAttribute("Binding"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Formula",
					new DescriptionAttribute(SR.GetString("ControlCalculationSettings_Formula_Property")),
				    new DisplayNameAttribute("Formula"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Property",
					new DescriptionAttribute(SR.GetString("ControlCalculationSettings_Property_Property")),
				    new DisplayNameAttribute("Property"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ReferenceId",
					new DescriptionAttribute(SR.GetString("ControlCalculationSettings_ReferenceId_Property")),
				    new DisplayNameAttribute("ReferenceId"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ValueConverter",
					new DescriptionAttribute(SR.GetString("ControlCalculationSettings_ValueConverter_Property")),
				    new DisplayNameAttribute("ValueConverter"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TreatAsTypeName",
					new DescriptionAttribute(SR.GetString("ControlCalculationSettings_TreatAsTypeName_Property")),
				    new DisplayNameAttribute("TreatAsTypeName"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TreatAsTypeResolved",
					new DescriptionAttribute(SR.GetString("ControlCalculationSettings_TreatAsTypeResolved_Property")),
				    new DisplayNameAttribute("TreatAsTypeResolved"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TreatAsType",
					new DescriptionAttribute(SR.GetString("ControlCalculationSettings_TreatAsType_Property")),
				    new DisplayNameAttribute("TreatAsType"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);

				#endregion // ControlCalculationSettings Properties

				#region ItemCalculationCollection Properties
				t = controlAssembly.GetType("Infragistics.Calculations.ItemCalculationCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ItemCalculationCollection Properties

				#region ListCalculationCollection Properties
				t = controlAssembly.GetType("Infragistics.Calculations.ListCalculationCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ListCalculationCollection Properties

				#region ItemCalculatorBase Properties
				t = controlAssembly.GetType("Infragistics.Calculations.ItemCalculatorBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CalculationManager",
					new DescriptionAttribute(SR.GetString("ItemCalculatorBase_CalculationManager_Property")),
				    new DisplayNameAttribute("CalculationManager")				);


				tableBuilder.AddCustomAttributes(t, "PropertiesToExclude",
					new DescriptionAttribute(SR.GetString("ItemCalculatorBase_PropertiesToExclude_Property")),
				    new DisplayNameAttribute("PropertiesToExclude")				);


				tableBuilder.AddCustomAttributes(t, "PropertiesToInclude",
					new DescriptionAttribute(SR.GetString("ItemCalculatorBase_PropertiesToInclude_Property")),
				    new DisplayNameAttribute("PropertiesToInclude")				);


				tableBuilder.AddCustomAttributes(t, "ReferenceId",
					new DescriptionAttribute(SR.GetString("ItemCalculatorBase_ReferenceId_Property")),
				    new DisplayNameAttribute("ReferenceId")				);


				tableBuilder.AddCustomAttributes(t, "ValueConverter",
					new DescriptionAttribute(SR.GetString("ItemCalculatorBase_ValueConverter_Property")),
				    new DisplayNameAttribute("ValueConverter")				);

				#endregion // ItemCalculatorBase Properties

				#region ItemCalculatorElementBase Properties
				t = controlAssembly.GetType("Infragistics.Calculations.Primitives.ItemCalculatorElementBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CalculationManager",
					new DescriptionAttribute(SR.GetString("ItemCalculatorElementBase_CalculationManager_Property")),
				    new DisplayNameAttribute("CalculationManager"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);

				#endregion // ItemCalculatorElementBase Properties

				#region ItemCalculationBase Properties
				t = controlAssembly.GetType("Infragistics.Calculations.ItemCalculationBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Formula",
					new DescriptionAttribute(SR.GetString("ItemCalculationBase_Formula_Property")),
				    new DisplayNameAttribute("Formula"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ReferenceId",
					new DescriptionAttribute(SR.GetString("ItemCalculationBase_ReferenceId_Property")),
				    new DisplayNameAttribute("ReferenceId"),
					new CategoryAttribute(SR.GetString("XamCalculationManager_Properties"))
				);

				#endregion // ItemCalculationBase Properties

				#region DataAccessErrorEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Calculations.DataAccessErrorEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Exception",
					new DescriptionAttribute(SR.GetString("DataAccessErrorEventArgs_Exception_Property")),
				    new DisplayNameAttribute("Exception")				);


				tableBuilder.AddCustomAttributes(t, "ErrorMessage",
					new DescriptionAttribute(SR.GetString("DataAccessErrorEventArgs_ErrorMessage_Property")),
				    new DisplayNameAttribute("ErrorMessage")				);


				tableBuilder.AddCustomAttributes(t, "Item",
					new DescriptionAttribute(SR.GetString("DataAccessErrorEventArgs_Item_Property")),
				    new DisplayNameAttribute("Item")				);


				tableBuilder.AddCustomAttributes(t, "Property",
					new DescriptionAttribute(SR.GetString("DataAccessErrorEventArgs_Property_Property")),
				    new DisplayNameAttribute("Property")				);


				tableBuilder.AddCustomAttributes(t, "ThrownDuringSet",
					new DescriptionAttribute(SR.GetString("DataAccessErrorEventArgs_ThrownDuringSet_Property")),
				    new DisplayNameAttribute("ThrownDuringSet")				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("DataAccessErrorEventArgs_Value_Property")),
				    new DisplayNameAttribute("Value")				);

				#endregion // DataAccessErrorEventArgs Properties

				#region CalculationResultsDictionary Properties
				t = controlAssembly.GetType("Infragistics.Calculations.CalculationResultsDictionary");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CalculationResultsDictionary Properties

				#region CustomCalculationFunctionBase Properties
				t = controlAssembly.GetType("Infragistics.Calculations.CustomCalculationFunctionBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ArgDescriptors",
					new DescriptionAttribute(SR.GetString("CustomCalculationFunctionBase_ArgDescriptors_Property")),
				    new DisplayNameAttribute("ArgDescriptors")				);


				tableBuilder.AddCustomAttributes(t, "ArgList",
					new DescriptionAttribute(SR.GetString("CustomCalculationFunctionBase_ArgList_Property")),
				    new DisplayNameAttribute("ArgList")				);


				tableBuilder.AddCustomAttributes(t, "Category",
					new DescriptionAttribute(SR.GetString("CustomCalculationFunctionBase_Category_Property")),
				    new DisplayNameAttribute("Category")				);


				tableBuilder.AddCustomAttributes(t, "Description",
					new DescriptionAttribute(SR.GetString("CustomCalculationFunctionBase_Description_Property")),
				    new DisplayNameAttribute("Description")				);


				tableBuilder.AddCustomAttributes(t, "Name",
					new DescriptionAttribute(SR.GetString("CustomCalculationFunctionBase_Name_Property")),
				    new DisplayNameAttribute("Name")				);

				#endregion // CustomCalculationFunctionBase Properties

				#region ItemCalculationBaseCollection`1 Properties
				t = controlAssembly.GetType("Infragistics.Calculations.ItemCalculationBaseCollection`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ItemCalculationBaseCollection`1 Properties
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