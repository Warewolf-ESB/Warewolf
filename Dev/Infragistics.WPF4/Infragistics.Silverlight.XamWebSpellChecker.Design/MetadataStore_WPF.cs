using System;
using System.ComponentModel;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.Reflection;

[assembly: ProvideMetadata(typeof(InfragisticsWPF4.Controls.Interactions.XamSpellChecker.Design.MetadataStore))]

namespace InfragisticsWPF4.Controls.Interactions.XamSpellChecker.Design
{
	internal partial class MetadataStore : IProvideAttributeTable
	{
		public AttributeTable AttributeTable
		{
			get
			{
			    bool isVS = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName.Equals("devenv.exe"); 
				AttributeTableBuilder tableBuilder = new AttributeTableBuilder();
				Type t = typeof(Infragistics.Controls.Interactions.XamSpellChecker);
				Assembly controlAssembly = t.Assembly;

				#region BadWord Properties
				t = controlAssembly.GetType("Infragistics.BadWord");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Suggestions",
					new DescriptionAttribute(SR.GetString("BadWord_Suggestions_Property")),
				    new DisplayNameAttribute("Suggestions")				);

				#endregion // BadWord Properties

				#region WordOccurrence Properties
				t = controlAssembly.GetType("Infragistics.WordOccurrence");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "StartPosition",
					new DescriptionAttribute(SR.GetString("WordOccurrence_StartPosition_Property")),
				    new DisplayNameAttribute("StartPosition")				);


				tableBuilder.AddCustomAttributes(t, "EndPosition",
					new DescriptionAttribute(SR.GetString("WordOccurrence_EndPosition_Property")),
				    new DisplayNameAttribute("EndPosition")				);


				tableBuilder.AddCustomAttributes(t, "Word",
					new DescriptionAttribute(SR.GetString("WordOccurrence_Word_Property")),
				    new DisplayNameAttribute("Word")				);

				#endregion // WordOccurrence Properties

				#region SpellCheckCompletedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.SpellCheckCompletedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SpellCheckCompletedEventArgs Properties

				#region SpellOptions Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.SpellOptions");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowAnyCase",
					new DescriptionAttribute(SR.GetString("SpellOptions_AllowAnyCase_Property")),
				    new DisplayNameAttribute("AllowAnyCase")				);


				tableBuilder.AddCustomAttributes(t, "AllowCaseInsensitiveSuggestions",
					new DescriptionAttribute(SR.GetString("SpellOptions_AllowCaseInsensitiveSuggestions_Property")),
				    new DisplayNameAttribute("AllowCaseInsensitiveSuggestions")				);


				tableBuilder.AddCustomAttributes(t, "AllowMixedCase",
					new DescriptionAttribute(SR.GetString("SpellOptions_AllowMixedCase_Property")),
				    new DisplayNameAttribute("AllowMixedCase")				);


				tableBuilder.AddCustomAttributes(t, "AllowWordsWithDigits",
					new DescriptionAttribute(SR.GetString("SpellOptions_AllowWordsWithDigits_Property")),
				    new DisplayNameAttribute("AllowWordsWithDigits")				);


				tableBuilder.AddCustomAttributes(t, "AllowXml",
					new DescriptionAttribute(SR.GetString("SpellOptions_AllowXml_Property")),
				    new DisplayNameAttribute("AllowXml")				);


				tableBuilder.AddCustomAttributes(t, "CheckHyphenatedText",
					new DescriptionAttribute(SR.GetString("SpellOptions_CheckHyphenatedText_Property")),
				    new DisplayNameAttribute("CheckHyphenatedText")				);


				tableBuilder.AddCustomAttributes(t, "IncludeUserDictionaryInSuggestions",
					new DescriptionAttribute(SR.GetString("SpellOptions_IncludeUserDictionaryInSuggestions_Property")),
				    new DisplayNameAttribute("IncludeUserDictionaryInSuggestions")				);


				tableBuilder.AddCustomAttributes(t, "LanguageParser",
					new DescriptionAttribute(SR.GetString("SpellOptions_LanguageParser_Property")),
				    new DisplayNameAttribute("LanguageParser")				);


				tableBuilder.AddCustomAttributes(t, "SeparateHyphenWords",
					new DescriptionAttribute(SR.GetString("SpellOptions_SeparateHyphenWords_Property")),
				    new DisplayNameAttribute("SeparateHyphenWords")				);


				tableBuilder.AddCustomAttributes(t, "SuggestionsMethod",
					new DescriptionAttribute(SR.GetString("SpellOptions_SuggestionsMethod_Property")),
				    new DisplayNameAttribute("SuggestionsMethod")				);

				#endregion // SpellOptions Properties

				#region NoCurrentBadWordException Properties
				t = controlAssembly.GetType("Infragistics.SpellChecker.NoCurrentBadWordException");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NoCurrentBadWordException Properties

				#region PerformanceOptions Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.PerformanceOptions");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowCapitalizedWords",
					new DescriptionAttribute(SR.GetString("PerformanceOptions_AllowCapitalizedWords_Property")),
				    new DisplayNameAttribute("AllowCapitalizedWords")				);


				tableBuilder.AddCustomAttributes(t, "CheckCompoundWords",
					new DescriptionAttribute(SR.GetString("PerformanceOptions_CheckCompoundWords_Property")),
				    new DisplayNameAttribute("CheckCompoundWords")				);


				tableBuilder.AddCustomAttributes(t, "ConsiderationRange",
					new DescriptionAttribute(SR.GetString("PerformanceOptions_ConsiderationRange_Property")),
				    new DisplayNameAttribute("ConsiderationRange")				);


				tableBuilder.AddCustomAttributes(t, "SplitWordThreshold",
					new DescriptionAttribute(SR.GetString("PerformanceOptions_SplitWordThreshold_Property")),
				    new DisplayNameAttribute("SplitWordThreshold")				);


				tableBuilder.AddCustomAttributes(t, "SuggestSplitWords",
					new DescriptionAttribute(SR.GetString("PerformanceOptions_SuggestSplitWords_Property")),
				    new DisplayNameAttribute("SuggestSplitWords")				);

				#endregion // PerformanceOptions Properties

				#region DialogStringResources Properties
				t = controlAssembly.GetType("Infragistics.DialogStringResources");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "NotInDictionary",
					new DescriptionAttribute(SR.GetString("DialogStringResources_NotInDictionary_Property")),
				    new DisplayNameAttribute("NotInDictionary")				);


				tableBuilder.AddCustomAttributes(t, "Context",
					new DescriptionAttribute(SR.GetString("DialogStringResources_Context_Property")),
				    new DisplayNameAttribute("Context")				);


				tableBuilder.AddCustomAttributes(t, "Loading",
					new DescriptionAttribute(SR.GetString("DialogStringResources_Loading_Property")),
				    new DisplayNameAttribute("Loading")				);


				tableBuilder.AddCustomAttributes(t, "UseThisInstead",
					new DescriptionAttribute(SR.GetString("DialogStringResources_UseThisInstead_Property")),
				    new DisplayNameAttribute("UseThisInstead")				);


				tableBuilder.AddCustomAttributes(t, "Change",
					new DescriptionAttribute(SR.GetString("DialogStringResources_Change_Property")),
				    new DisplayNameAttribute("Change")				);


				tableBuilder.AddCustomAttributes(t, "ChangeAll",
					new DescriptionAttribute(SR.GetString("DialogStringResources_ChangeAll_Property")),
				    new DisplayNameAttribute("ChangeAll")				);


				tableBuilder.AddCustomAttributes(t, "Ignore",
					new DescriptionAttribute(SR.GetString("DialogStringResources_Ignore_Property")),
				    new DisplayNameAttribute("Ignore")				);


				tableBuilder.AddCustomAttributes(t, "IgnoreAll",
					new DescriptionAttribute(SR.GetString("DialogStringResources_IgnoreAll_Property")),
				    new DisplayNameAttribute("IgnoreAll")				);


				tableBuilder.AddCustomAttributes(t, "Suggestions",
					new DescriptionAttribute(SR.GetString("DialogStringResources_Suggestions_Property")),
				    new DisplayNameAttribute("Suggestions")				);


				tableBuilder.AddCustomAttributes(t, "PreviousField",
					new DescriptionAttribute(SR.GetString("DialogStringResources_PreviousField_Property")),
				    new DisplayNameAttribute("PreviousField")				);


				tableBuilder.AddCustomAttributes(t, "NextField",
					new DescriptionAttribute(SR.GetString("DialogStringResources_NextField_Property")),
				    new DisplayNameAttribute("NextField")				);


				tableBuilder.AddCustomAttributes(t, "Ok",
					new DescriptionAttribute(SR.GetString("DialogStringResources_Ok_Property")),
				    new DisplayNameAttribute("Ok")				);


				tableBuilder.AddCustomAttributes(t, "AddToDictionary",
					new DescriptionAttribute(SR.GetString("DialogStringResources_AddToDictionary_Property")),
				    new DisplayNameAttribute("AddToDictionary")				);


				tableBuilder.AddCustomAttributes(t, "SpellCheckComplete",
					new DescriptionAttribute(SR.GetString("DialogStringResources_SpellCheckComplete_Property")),
				    new DisplayNameAttribute("SpellCheckComplete")				);

				#endregion // DialogStringResources Properties

				#region XamSpellCheckerDialogWindowAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.XamSpellCheckerDialogWindowAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamSpellCheckerDialogWindowAutomationPeer Properties

				#region XamSpellCheckerDialogSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.XamSpellCheckerDialogSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CurrentWordBrush",
					new DescriptionAttribute(SR.GetString("XamSpellCheckerDialogSettings_CurrentWordBrush_Property")),
				    new DisplayNameAttribute("CurrentWordBrush"),
					new CategoryAttribute(SR.GetString("XamSpellCheckerDialogSettings_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SpellCheckDialogStyle",
					new DescriptionAttribute(SR.GetString("XamSpellCheckerDialogSettings_SpellCheckDialogStyle_Property")),
				    new DisplayNameAttribute("SpellCheckDialogStyle"),
					new CategoryAttribute(SR.GetString("XamSpellCheckerDialogSettings_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Mode",
					new DescriptionAttribute(SR.GetString("XamSpellCheckerDialogSettings_Mode_Property")),
				    new DisplayNameAttribute("Mode"),
					new CategoryAttribute(SR.GetString("XamSpellCheckerDialogSettings_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DialogStringResources",
					new DescriptionAttribute(SR.GetString("XamSpellCheckerDialogSettings_DialogStringResources_Property")),
				    new DisplayNameAttribute("DialogStringResources"),
					new CategoryAttribute(SR.GetString("XamSpellCheckerDialogSettings_Properties"))
				);

				#endregion // XamSpellCheckerDialogSettings Properties

				#region XamSpellCheckerCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.XamSpellCheckerCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamSpellCheckerCommandBase Properties

				#region AddToDictionaryCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.AddToDictionaryCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AddToDictionaryCommand Properties

				#region CancelAsyncDictionaryCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.CancelAsyncDictionaryCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CancelAsyncDictionaryCommand Properties

				#region IgnoreCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.IgnoreCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // IgnoreCommand Properties

				#region IgnoreAllCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.IgnoreAllCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // IgnoreAllCommand Properties

				#region ChangeCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.ChangeCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ChangeCommand Properties

				#region ChangeAllCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.ChangeAllCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ChangeAllCommand Properties

				#region CloseDialogCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.CloseDialogCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CloseDialogCommand Properties

				#region CancelDialogCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.CancelDialogCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CancelDialogCommand Properties

				#region NextFieldCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.NextFieldCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NextFieldCommand Properties

				#region PreviousFieldCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.PreviousFieldCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PreviousFieldCommand Properties

				#region SpellCheckCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.SpellCheckCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SpellCheckCommand Properties

				#region XamSpellCheckerCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.XamSpellCheckerCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("XamSpellCheckerCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType"),
					new CategoryAttribute(SR.GetString("XamSpellCheckerCommandSource_Properties"))
				);

				#endregion // XamSpellCheckerCommandSource Properties

				#region XamSpellCheckerDialogCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.XamSpellCheckerDialogCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("XamSpellCheckerDialogCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType"),
					new CategoryAttribute(SR.GetString("XamSpellCheckerDialogCommandSource_Properties"))
				);

				#endregion // XamSpellCheckerDialogCommandSource Properties

				#region SpellCheckerDialogRootPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.SpellCheckerDialogRootPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SpellCheckerDialogRootPanel Properties

				#region XamSpellChecker Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.XamSpellChecker");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamSpellCheckerAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamSpellCheckerAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "CurrentBadWord",
					new DescriptionAttribute(SR.GetString("XamSpellChecker_CurrentBadWord_Property")),
				    new DisplayNameAttribute("CurrentBadWord"),
					new CategoryAttribute(SR.GetString("XamSpellChecker_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SpellCheckDialog",
					new DescriptionAttribute(SR.GetString("XamSpellChecker_SpellCheckDialog_Property")),
				    new DisplayNameAttribute("SpellCheckDialog"),
					new CategoryAttribute(SR.GetString("XamSpellChecker_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DictionaryUri",
					new DescriptionAttribute(SR.GetString("XamSpellChecker_DictionaryUri_Property")),
				    new DisplayNameAttribute("DictionaryUri"),
					new CategoryAttribute(SR.GetString("XamSpellChecker_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "UserDictionaryUri",
					new DescriptionAttribute(SR.GetString("XamSpellChecker_UserDictionaryUri_Property")),
				    new DisplayNameAttribute("UserDictionaryUri"),
					new CategoryAttribute(SR.GetString("XamSpellChecker_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DialogSettings",
					new DescriptionAttribute(SR.GetString("XamSpellChecker_DialogSettings_Property")),
				    new DisplayNameAttribute("DialogSettings"),
					new CategoryAttribute(SR.GetString("XamSpellChecker_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PerformanceOptions",
					new DescriptionAttribute(SR.GetString("XamSpellChecker_PerformanceOptions_Property")),
				    new DisplayNameAttribute("PerformanceOptions"),
					new CategoryAttribute(SR.GetString("XamSpellChecker_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DictionaryLoadProgressDialogStyle",
					new DescriptionAttribute(SR.GetString("XamSpellChecker_DictionaryLoadProgressDialogStyle_Property")),
				    new DisplayNameAttribute("DictionaryLoadProgressDialogStyle"),
					new CategoryAttribute(SR.GetString("XamSpellChecker_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SpellOptions",
					new DescriptionAttribute(SR.GetString("XamSpellChecker_SpellOptions_Property")),
				    new DisplayNameAttribute("SpellOptions"),
					new CategoryAttribute(SR.GetString("XamSpellChecker_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SpellCheckTargets",
					new DescriptionAttribute(SR.GetString("XamSpellChecker_SpellCheckTargets_Property")),
				    new DisplayNameAttribute("SpellCheckTargets"),
					new CategoryAttribute(SR.GetString("XamSpellChecker_Properties"))
				);

				#endregion // XamSpellChecker Properties

				#region DictionaryLoadProgressDialog Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.DictionaryLoadProgressDialog");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "SpellChecker",
					new DescriptionAttribute(SR.GetString("DictionaryLoadProgressDialog_SpellChecker_Property")),
				    new DisplayNameAttribute("SpellChecker")				);


				tableBuilder.AddCustomAttributes(t, "ProgressValue",
					new DescriptionAttribute(SR.GetString("DictionaryLoadProgressDialog_ProgressValue_Property")),
				    new DisplayNameAttribute("ProgressValue")				);

				#endregion // DictionaryLoadProgressDialog Properties

				#region XamSpellCheckerDialogWindow Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.XamSpellCheckerDialogWindow");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "SpellChecker",
					new DescriptionAttribute(SR.GetString("XamSpellCheckerDialogWindow_SpellChecker_Property")),
				    new DisplayNameAttribute("SpellChecker"),
					new CategoryAttribute(SR.GetString("XamSpellCheckerDialogWindow_Properties"))
				);

				#endregion // XamSpellCheckerDialogWindow Properties

				#region TargetElement Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.TargetElement");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("TargetElement_Value_Property")),
				    new DisplayNameAttribute("Value")				);

				#endregion // TargetElement Properties
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