using System;
using System.ComponentModel;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.Reflection;

[assembly: ProvideMetadata(typeof(InfragisticsWPF4.Controls.Interactions.XamDialogWindow.Design.MetadataStore))]

namespace InfragisticsWPF4.Controls.Interactions.XamDialogWindow.Design
{
	internal partial class MetadataStore : IProvideAttributeTable
	{
		public AttributeTable AttributeTable
		{
			get
			{
			    bool isVS = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName.Equals("devenv.exe"); 
				AttributeTableBuilder tableBuilder = new AttributeTableBuilder();
				Type t = typeof(Infragistics.Controls.Interactions.WindowState);
				Assembly controlAssembly = t.Assembly;

				#region MovedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.MovedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Left",
					new DescriptionAttribute(SR.GetString("MovedEventArgs_Left_Property")),
				    new DisplayNameAttribute("Left")				);


				tableBuilder.AddCustomAttributes(t, "Top",
					new DescriptionAttribute(SR.GetString("MovedEventArgs_Top_Property")),
				    new DisplayNameAttribute("Top")				);

				#endregion // MovedEventArgs Properties

				#region MovingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.MovingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Left",
					new DescriptionAttribute(SR.GetString("MovingEventArgs_Left_Property")),
				    new DisplayNameAttribute("Left")				);


				tableBuilder.AddCustomAttributes(t, "Top",
					new DescriptionAttribute(SR.GetString("MovingEventArgs_Top_Property")),
				    new DisplayNameAttribute("Top")				);

				#endregion // MovingEventArgs Properties

				#region WindowStateChangingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.WindowStateChangingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "NewWindowState",
					new DescriptionAttribute(SR.GetString("WindowStateChangingEventArgs_NewWindowState_Property")),
				    new DisplayNameAttribute("NewWindowState")				);


				tableBuilder.AddCustomAttributes(t, "CurrentWindowState",
					new DescriptionAttribute(SR.GetString("WindowStateChangingEventArgs_CurrentWindowState_Property")),
				    new DisplayNameAttribute("CurrentWindowState")				);

				#endregion // WindowStateChangingEventArgs Properties

				#region WindowStateChangedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.WindowStateChangedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "PreviousWindowState",
					new DescriptionAttribute(SR.GetString("WindowStateChangedEventArgs_PreviousWindowState_Property")),
				    new DisplayNameAttribute("PreviousWindowState")				);


				tableBuilder.AddCustomAttributes(t, "NewWindowState",
					new DescriptionAttribute(SR.GetString("WindowStateChangedEventArgs_NewWindowState_Property")),
				    new DisplayNameAttribute("NewWindowState")				);

				#endregion // WindowStateChangedEventArgs Properties

				#region Clip Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Clip");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // Clip Properties

				#region CustomCursors Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.CustomCursors");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DiagonalResizeCursor",
					new DescriptionAttribute(SR.GetString("CustomCursors_DiagonalResizeCursor_Property")),
				    new DisplayNameAttribute("DiagonalResizeCursor")				);


				tableBuilder.AddCustomAttributes(t, "HorizontalResizeCursor",
					new DescriptionAttribute(SR.GetString("CustomCursors_HorizontalResizeCursor_Property")),
				    new DisplayNameAttribute("HorizontalResizeCursor")				);


				tableBuilder.AddCustomAttributes(t, "RightDiagonalResizeCursor",
					new DescriptionAttribute(SR.GetString("CustomCursors_RightDiagonalResizeCursor_Property")),
				    new DisplayNameAttribute("RightDiagonalResizeCursor")				);


				tableBuilder.AddCustomAttributes(t, "VerticalResizeCursor",
					new DescriptionAttribute(SR.GetString("CustomCursors_VerticalResizeCursor_Property")),
				    new DisplayNameAttribute("VerticalResizeCursor")				);

				#endregion // CustomCursors Properties

				#region KeyboardSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.KeyboardSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalStep",
					new DescriptionAttribute(SR.GetString("KeyboardSettings_HorizontalStep_Property")),
				    new DisplayNameAttribute("HorizontalStep")				);


				tableBuilder.AddCustomAttributes(t, "AllowKeyboardNavigation",
					new DescriptionAttribute(SR.GetString("KeyboardSettings_AllowKeyboardNavigation_Property")),
				    new DisplayNameAttribute("AllowKeyboardNavigation")				);


				tableBuilder.AddCustomAttributes(t, "VerticalStep",
					new DescriptionAttribute(SR.GetString("KeyboardSettings_VerticalStep_Property")),
				    new DisplayNameAttribute("VerticalStep")				);

				#endregion // KeyboardSettings Properties

				#region DialogRootPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.DialogRootPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DialogWindow",
					new DescriptionAttribute(SR.GetString("DialogRootPanel_DialogWindow_Property")),
				    new DisplayNameAttribute("DialogWindow")				);

				#endregion // DialogRootPanel Properties

				#region CloseCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.CloseCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CloseCommand Properties

				#region MaximizeCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.MaximizeCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MaximizeCommand Properties

				#region MinimizeCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.MinimizeCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MinimizeCommand Properties

				#region RestoreCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.RestoreCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RestoreCommand Properties

				#region ToggleMaximizeCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.ToggleMaximizeCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ToggleMaximizeCommand Properties

				#region ToggleMinimizeCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.ToggleMinimizeCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ToggleMinimizeCommand Properties

				#region XamDialogWindowCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.XamDialogWindowCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("XamDialogWindowCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType"),
					new CategoryAttribute(SR.GetString("XamDialogWindowCommandSource_Properties"))
				);

				#endregion // XamDialogWindowCommandSource Properties

				#region XamDialogWindow Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.XamDialogWindow");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamWebDialogWindowAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamWebDialogWindowAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "CloseButtonVisibility",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_CloseButtonVisibility_Property")),
				    new DisplayNameAttribute("CloseButtonVisibility"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaximizeButtonVisibility",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_MaximizeButtonVisibility_Property")),
				    new DisplayNameAttribute("MaximizeButtonVisibility"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinimizeButtonVisibility",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_MinimizeButtonVisibility_Property")),
				    new DisplayNameAttribute("MinimizeButtonVisibility"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CustomCursors",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_CustomCursors_Property")),
				    new DisplayNameAttribute("CustomCursors"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsActive",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_IsActive_Property")),
				    new DisplayNameAttribute("IsActive"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsModal",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_IsModal_Property")),
				    new DisplayNameAttribute("IsModal"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ModalBackground",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_ModalBackground_Property")),
				    new DisplayNameAttribute("ModalBackground"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ModalBackgroundEffect",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_ModalBackgroundEffect_Property")),
				    new DisplayNameAttribute("ModalBackgroundEffect"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ModalBackgroundOpacity",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_ModalBackgroundOpacity_Property")),
				    new DisplayNameAttribute("ModalBackgroundOpacity"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsMoveable",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_IsMoveable_Property")),
				    new DisplayNameAttribute("IsMoveable"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsResizable",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_IsResizable_Property")),
				    new DisplayNameAttribute("IsResizable"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "KeyboardSettings",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_KeyboardSettings_Property")),
				    new DisplayNameAttribute("KeyboardSettings"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Left",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_Left_Property")),
				    new DisplayNameAttribute("Left"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Top",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_Top_Property")),
				    new DisplayNameAttribute("Top"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinimizedHeaderTemplate",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_MinimizedHeaderTemplate_Property")),
				    new DisplayNameAttribute("MinimizedHeaderTemplate"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinimizedHeight",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_MinimizedHeight_Property")),
				    new DisplayNameAttribute("MinimizedHeight"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinimizedPanel",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_MinimizedPanel_Property")),
				    new DisplayNameAttribute("MinimizedPanel"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinimizedWidth",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_MinimizedWidth_Property")),
				    new DisplayNameAttribute("MinimizedWidth"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "StartupPosition",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_StartupPosition_Property")),
				    new DisplayNameAttribute("StartupPosition"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "WindowState",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_WindowState_Property")),
				    new DisplayNameAttribute("WindowState"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Header",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_Header_Property")),
				    new DisplayNameAttribute("Header"),
				    new TypeConverterAttribute(typeof(StringConverter))
,
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HeaderTemplate",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_HeaderTemplate_Property")),
				    new DisplayNameAttribute("HeaderTemplate"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HeaderBackground",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_HeaderBackground_Property")),
				    new DisplayNameAttribute("HeaderBackground"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HeaderForeground",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_HeaderForeground_Property")),
				    new DisplayNameAttribute("HeaderForeground"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HeaderHorizontalContentAlignment",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_HeaderHorizontalContentAlignment_Property")),
				    new DisplayNameAttribute("HeaderHorizontalContentAlignment"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HeaderVerticalContentAlignment",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_HeaderVerticalContentAlignment_Property")),
				    new DisplayNameAttribute("HeaderVerticalContentAlignment"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HeaderIconTemplate",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_HeaderIconTemplate_Property")),
				    new DisplayNameAttribute("HeaderIconTemplate"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HeaderIconVisibility",
					new DescriptionAttribute(SR.GetString("XamDialogWindow_HeaderIconVisibility_Property")),
				    new DisplayNameAttribute("HeaderIconVisibility"),
					new CategoryAttribute(SR.GetString("XamDialogWindow_Properties"))
				);

				#endregion // XamDialogWindow Properties

				#region XamDialogWindowAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.XamDialogWindowAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CanMove",
					new DescriptionAttribute(SR.GetString("XamDialogWindowAutomationPeer_CanMove_Property")),
				    new DisplayNameAttribute("CanMove"),
					new CategoryAttribute(SR.GetString("XamDialogWindowAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CanResize",
					new DescriptionAttribute(SR.GetString("XamDialogWindowAutomationPeer_CanResize_Property")),
				    new DisplayNameAttribute("CanResize"),
					new CategoryAttribute(SR.GetString("XamDialogWindowAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CanRotate",
					new DescriptionAttribute(SR.GetString("XamDialogWindowAutomationPeer_CanRotate_Property")),
				    new DisplayNameAttribute("CanRotate"),
					new CategoryAttribute(SR.GetString("XamDialogWindowAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "InteractionState",
					new DescriptionAttribute(SR.GetString("XamDialogWindowAutomationPeer_InteractionState_Property")),
				    new DisplayNameAttribute("InteractionState"),
					new CategoryAttribute(SR.GetString("XamDialogWindowAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsModal",
					new DescriptionAttribute(SR.GetString("XamDialogWindowAutomationPeer_IsModal_Property")),
				    new DisplayNameAttribute("IsModal"),
					new CategoryAttribute(SR.GetString("XamDialogWindowAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsTopmost",
					new DescriptionAttribute(SR.GetString("XamDialogWindowAutomationPeer_IsTopmost_Property")),
				    new DisplayNameAttribute("IsTopmost"),
					new CategoryAttribute(SR.GetString("XamDialogWindowAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Maximizable",
					new DescriptionAttribute(SR.GetString("XamDialogWindowAutomationPeer_Maximizable_Property")),
				    new DisplayNameAttribute("Maximizable"),
					new CategoryAttribute(SR.GetString("XamDialogWindowAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Minimizable",
					new DescriptionAttribute(SR.GetString("XamDialogWindowAutomationPeer_Minimizable_Property")),
				    new DisplayNameAttribute("Minimizable"),
					new CategoryAttribute(SR.GetString("XamDialogWindowAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "VisualState",
					new DescriptionAttribute(SR.GetString("XamDialogWindowAutomationPeer_VisualState_Property")),
				    new DisplayNameAttribute("VisualState"),
					new CategoryAttribute(SR.GetString("XamDialogWindowAutomationPeer_Properties"))
				);

				#endregion // XamDialogWindowAutomationPeer Properties

				#region XamDialogWindowCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Interactions.Primitives.XamDialogWindowCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamDialogWindowCommandBase Properties
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