using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Windows.Ribbon;
using System.Windows;
using Infragistics.Controls.Schedules.Primitives;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace Infragistics.Controls.Schedules
{
	internal class ActivityDialogHelperWpf
	{
		[ThreadStatic]
		internal static bool		_ignoreReminderComboSelectionChanged;

		#region CreateActivityDialog

		internal static FrameworkElement CreateActivityDialog(FrameworkElement container, XamScheduleDataManager dataManager, ActivityBase activity, bool allowModifications, bool allowRemove)
		{
			FrameworkElement		element;
			XamRibbon				ribbon;
			ActivityDialogCore		adc;
			switch (activity.ActivityType)
			{
				case ActivityType.Appointment:
					{
						adc = new AppointmentDialogCore(container, dataManager, activity as Appointment);
						break;
					}
				case ActivityType.Task:
					{
						adc = new TaskDialogCore(container, dataManager, activity as Task);
						break;
					}
				case ActivityType.Journal:
					{
						adc = new JournalDialogCore(container, dataManager, activity as Journal);
						break;
					}
				default:	// return null for unsupported activity types.
					return null;
			}

			adc.IsActivityModifiable = allowModifications;
			adc.IsActivityRemoveable	= allowRemove;
			ribbon						= ActivityDialogHelperWpf.GetInitializedRibbon(adc);

			if (System.Windows.Interop.BrowserInteropHelper.IsBrowserHosted)
			{
				adc.NavigationControlSiteContent = ribbon;
				element = adc;
			}
			else
			{
				XamRibbonWindow			xrw		= new XamRibbonWindow();
				RibbonWindowContentHost rwch	= new RibbonWindowContentHost();
				xrw.Content		= rwch;

				if (null != activity.Subject)
					xrw.Title	= activity.Subject;

				rwch.Ribbon		= ribbon;
				rwch.Content	= adc;

				// Register the AppointmentDialogControl as a CommandTarget since it is not in the ancestor tree of the Ribbon.
				CommandSourceManager.RegisterCommandTarget(adc);

				element = xrw;
			}

			// Bind the DataContext of the Ribbon to the AppointmentDialogCore
			Binding binding = new Binding();
			binding.Mode	= BindingMode.OneWay;
			binding.Source	= adc;
			ribbon.SetBinding(XamRibbon.DataContextProperty, binding);

			return element;
		}

		#endregion //CreateActivityDialog

		#region GetInitializedRibbon
		private static XamRibbon GetInitializedRibbon(ActivityDialogCore adc)
		{
			bool isRecurrenceToolRequired	= true;
			bool isReminderToolRequired		= false;
			bool isTimeZoneToolRequired		= false;
			
#pragma warning disable 436
			XamRibbon ribbon			= new XamRibbon();
			ribbon.TabIndex				= 99999;
			ribbon.SetValue(XamRibbon.NameProperty, "Ribbon");


			// RIBBONTABITEM - Appointment
			RibbonTabItem tabItem		= new RibbonTabItem();

			if (adc is TaskDialogCore)
				tabItem.Header	= SR.GetString("DLG_Appointment_RibbonTabItem_Task");
			else 
			if (adc is JournalDialogCore)
				tabItem.Header	= SR.GetString("DLG_Appointment_RibbonTabItem_Journal");
			else
			if (adc is AppointmentDialogCore)
			{
				tabItem.Header = SR.GetString("DLG_Appointment_RibbonTabItem_Appointment");

				isReminderToolRequired = true;
				isTimeZoneToolRequired = true;
			}
			else
				tabItem.Header = SR.GetString("DLG_Appointment_RibbonTabItem_Activity");

			ribbon.Tabs.Add(tabItem);

			// RIBBONGROUP - Actions
			RibbonGroup ribbonGroup		= new RibbonGroup();
			ribbonGroup.Caption			= SR.GetString("DLG_Appointment_RibbonGroup_Actions");
			tabItem.RibbonGroups.Add(ribbonGroup);

			// BUTTONTOOL - Save and Close
			ButtonTool buttonTool		= new ButtonTool();
			buttonTool.Caption			= SR.GetString("DLG_Appointment_ButtonTool_SaveClose");
			buttonTool.LargeImage		= GetImageSource("/Images/SaveAndCloseAppointment_32x32.png");
			buttonTool.SetValue(RibbonGroup.MinimumSizeProperty, RibbonToolSizingMode.ImageAndTextLarge);
			buttonTool.SetValue(RibbonGroup.MaximumSizeProperty, RibbonToolSizingMode.ImageAndTextLarge);
			ActivityDialogCoreCommandSource commandSource = new ActivityDialogCoreCommandSource();
			commandSource.EventName		= "Click";
			commandSource.CommandType	= ActivityDialogCoreCommand.SaveAndClose;
			commandSource.Parameter		= adc;
			buttonTool.SetValue(Commanding.CommandProperty, commandSource);
			ribbonGroup.Items.Add(buttonTool);

			// BUTTONTOOL - Delete
			buttonTool = new ButtonTool();
			buttonTool.Caption			= SR.GetString("DLG_Appointment_ButtonTool_Delete");
			buttonTool.LargeImage		= GetImageSource("/Images/DeleteAppointment_32x32.png");
			buttonTool.SetValue(RibbonGroup.MinimumSizeProperty, RibbonToolSizingMode.ImageAndTextLarge);
			buttonTool.SetValue(RibbonGroup.MaximumSizeProperty, RibbonToolSizingMode.ImageAndTextLarge);
			commandSource				= new ActivityDialogCoreCommandSource();
			commandSource.EventName		= "Click";
			commandSource.CommandType	= ActivityDialogCoreCommand.Delete;
			commandSource.Parameter		= adc;
			buttonTool.SetValue(Commanding.CommandProperty, commandSource);
			ribbonGroup.Items.Add(buttonTool);

			// RIBBONGROUP - Options
			ribbonGroup					= new RibbonGroup();
			ribbonGroup.Caption			= SR.GetString("DLG_Appointment_RibbonGroup_Options");

			// COMBOTOOL - Reminder
			if (true == isReminderToolRequired &&
				true == adc.DataManager.IsActivityReminderAllowed(adc.Activity))
			{
				ComboEditorTool comboTool		= new ComboEditorTool();
				comboTool.Caption				= SR.GetString("DLG_Appointment_ComboTool_Reminder");
				comboTool.LargeImage			= GetImageSource("/Images/Reminder_16x16.png");
				comboTool.SetValue(RibbonGroup.MinimumSizeProperty, RibbonToolSizingMode.ImageAndTextNormal);
				comboTool.ItemsSource			= AppointmentDialogUtilities.GetReminderListItems();
				comboTool.IsEditable			= adc.IsActivityModifiable;
				comboTool.IsAlwaysInEditMode	= adc.IsActivityModifiable;
				comboTool.KeyUp					+= new System.Windows.Input.KeyEventHandler(reminderComboTool_KeyUp);
				comboTool.SelectedItemChanged	+= new RoutedPropertyChangedEventHandler<object>(reminderComboTool_SelectedItemChanged);
				comboTool.LostFocus				+= new RoutedEventHandler(reminderComboTool_LostFocus);
				comboTool.IsEnabled				= adc.IsActivityModifiable;

				TimeSpan? ts = null;
				if (true == adc.Activity.ReminderEnabled)
					ts = adc.Activity.ReminderInterval;
				comboTool.Text					= DurationListItem.DurationStringFromTimeSpan(ts);
				ribbonGroup.Items.Add(comboTool);
			}

			// BUTTONTOOL - Recurrence
			if (true == isRecurrenceToolRequired &&
				true == adc.DataManager.IsRecurringActivityAllowed(adc.Activity))
			{
				if (false == adc.IsOccurrence)
				{
					buttonTool					= new ButtonTool();
					buttonTool.Caption			= SR.GetString("DLG_Appointment_ButtonTool_Recurrence");
					buttonTool.LargeImage		= GetImageSource("/Images/Recurrence_32x32.png");
					buttonTool.SetValue(RibbonGroup.MinimumSizeProperty, RibbonToolSizingMode.ImageAndTextLarge);
					buttonTool.SetValue(RibbonGroup.MaximumSizeProperty, RibbonToolSizingMode.ImageAndTextLarge);
					commandSource				= new ActivityDialogCoreCommandSource();
					commandSource.EventName		= "Click";
					commandSource.CommandType	= ActivityDialogCoreCommand.DisplayRecurrenceDialog;
					commandSource.Parameter		= adc;
					buttonTool.SetValue(Commanding.CommandProperty, commandSource);

					ribbonGroup.Items.Add(buttonTool);
				}
			}

			// TOGGLEBUTTONTOOL - TimeZones
			if (true	== isTimeZoneToolRequired &&
				false	== adc.IsOccurrence)
			{
				ToggleButtonTool toggleButtonTool	= new ToggleButtonTool();
				toggleButtonTool.Caption			= SR.GetString("DLG_Appointment_ButtonTool_TimeZones");
				toggleButtonTool.LargeImage			= GetImageSource("/Images/TimeZones_32x32.png");
				toggleButtonTool.SetValue(RibbonGroup.MinimumSizeProperty, RibbonToolSizingMode.ImageAndTextLarge);
				toggleButtonTool.SetValue(RibbonGroup.MaximumSizeProperty, RibbonToolSizingMode.ImageAndTextLarge);
				toggleButtonTool.IsChecked			= adc.TimeZonePickerVisibility == Visibility.Visible;

				CommandSourceCollection commands	= new CommandSourceCollection();
				commandSource						= new ActivityDialogCoreCommandSource();
				commandSource.EventName				= "Checked";
				commandSource.CommandType			= ActivityDialogCoreCommand.ShowTimeZonePickers;
				commandSource.Parameter				= adc;
				commands.Add(commandSource);
				commandSource						= new ActivityDialogCoreCommandSource();
				commandSource.EventName				= "Unchecked";
				commandSource.CommandType			= ActivityDialogCoreCommand.HideTimeZonePickers;
				commandSource.Parameter				= adc;
				commands.Add(commandSource);
				toggleButtonTool.SetValue(Commanding.CommandsProperty, commands);
				ribbonGroup.Items.Add(toggleButtonTool);
			}

			// Only show the Options RibbonGroup if it contains at least 1 tool.
			if (ribbonGroup.Items.Count > 0)
				tabItem.RibbonGroups.Add(ribbonGroup);

			// RIBBONGROUP - Tags
			if (false == adc.IsOccurrence && false == adc.ShouldHideCategoriesButton)
			{
				ribbonGroup			= new RibbonGroup();
				ribbonGroup.Caption = SR.GetString("DLG_Appointment_RibbonGroup_Tags");
				tabItem.RibbonGroups.Add(ribbonGroup);

				// MENUTOOL - Categorize
				MenuTool menuTool	= new MenuTool();
				menuTool.Caption	= SR.GetString("DLG_Appointment_ButtonTool_Categorize");
				menuTool.LargeImage = GetImageSource("/Images/Categorize_32x32.png");
				menuTool.SetValue(RibbonGroup.MinimumSizeProperty, RibbonToolSizingMode.ImageAndTextLarge);
				menuTool.SetValue(RibbonGroup.MaximumSizeProperty, RibbonToolSizingMode.ImageAndTextLarge);

				// Add dummy tool so the menu appears enabled.  We will delay the loading of the actual tools until
				// the menu is about to open.
				menuTool.Items.Add(new ButtonTool());

				// Hook the opened/closed events of the MenuTool so we can add tools and register/unregister the
				// ActivityCategoryHelper class as the Command target.
				menuTool.Opening	+= new EventHandler<Windows.Ribbon.Events.ToolOpeningEventArgs>(OnCategorizeMenuOpened);
				menuTool.Closed		+= new RoutedEventHandler(OnCategorizeMenuClosed);

				// JM 05-08-12 TFS104446
				if (adc.IsActivityModifiable == false)
					menuTool.IsEnabled= false;

				ribbonGroup.Items.Add(menuTool);
			}

#pragma warning restore 436

			return ribbon;
		}

		#endregion //GetInitializedRibbon

		#region GetImageSource
		private static BitmapImage GetImageSource(string uriString)
		{
			 BitmapImage bmpImage = new BitmapImage();

			 bmpImage.BeginInit();
			 bmpImage.UriSource = new Uri("/InfragisticsWPF4.Controls.SchedulesDialogs.v" + AssemblyVersion.MajorMinor + ";component" + uriString, UriKind.Relative);
			 bmpImage.EndInit();

			 return bmpImage;
		}
		#endregion //GetImageSource

		#region OnCategorizeMenuClosed
		static void OnCategorizeMenuClosed(object sender, RoutedEventArgs e)
		{
			if (sender is FrameworkElement)
			{
				ActivityDialogCore adc = ((FrameworkElement)sender).DataContext as ActivityDialogCore;
				if (adc != null)
					// Delay the Unregistering of the ActivityCategoryHelper as a CommandTarget until after the MenuItem Click (if any)
					// that caused the closeup is processed by the Commanding infrastructure.  If we don't delay this, we will not be
					// registered/found as a command target when the command is invoked.
					((FrameworkElement)sender).Dispatcher.BeginInvoke(new Action<ActivityCategoryHelper>(OnCategorizeMenuClosedAsync), adc.ActivityCategoryHelper);
			}
		}
		#endregion //OnCategorizeMenuClosed

		#region OnCategorizeMenuClosedAsync
		static void OnCategorizeMenuClosedAsync(ActivityCategoryHelper ach)
		{
			CommandSourceManager.UnregisterCommandTarget(ach);
		}
		#endregion //OnCategorizeMenuClosedAsync

		#region OnCategorizeMenuOpened
		static void OnCategorizeMenuOpened(object sender, Windows.Ribbon.Events.ToolOpeningEventArgs e)
		{
			if (sender is MenuTool)
			{
				ActivityDialogCore adc = ((FrameworkElement)sender).DataContext as ActivityDialogCore;
				if (adc != null)
				{
					MenuTool menuTool = sender as MenuTool;

					// Remove the dummy element we added in GetInitializedRibbon
					menuTool.Items.Clear();

					// Create menu items
					ReadOnlyObservableCollection<ActivityCategoryListItem> listItems = adc.ActivityCategoryHelper.CategoryListItems;
					bool? lastItemWasToggleCommand = null;
					foreach (ActivityCategoryListItem listItem in listItems)
					{
						// Add a separator menuitem if we are between two menuitems, one of which supports the 
						// ToggleActivityCategorySelectedState command and the other which does not.
						if (lastItemWasToggleCommand.HasValue)
					    {
					        if (lastItemWasToggleCommand.Value == true && listItem.Command != ActivityCategoryCommand.ToggleActivityCategorySelectedState)
					            menuTool.Items.Add(new SeparatorTool());
					        else
					        if (lastItemWasToggleCommand.Value == false && listItem.Command == ActivityCategoryCommand.ToggleActivityCategorySelectedState)
					            menuTool.Items.Add(new SeparatorTool());
					    }

					    ButtonTool buttonTool	= new ButtonTool();
					    buttonTool.Caption		= listItem.Name;
					    buttonTool.SmallImage	= listItem.IconImageSource != null ? listItem.IconImageSource : adc.ActivityCategoryHelper.GetIconContentFromCategoryColorsAsImageSource(listItem);
					    buttonTool.DataContext	= listItem;

					    ActivityCategoryCommandSource source = new ActivityCategoryCommandSource();
					    source.EventName		= "Click";
					    source.CommandType		= listItem.Command;
					    source.Parameter		= listItem.CommandParameter;

					    buttonTool.SetValue(Commanding.CommandProperty, source);

					    menuTool.Items.Add(buttonTool);

					    lastItemWasToggleCommand = listItem.Command == ActivityCategoryCommand.ToggleActivityCategorySelectedState;
					}

					// JM 03-30-11 TFS68660 - Delay this processing.
					//// Register the ActivityCategoryHelper as the CommandTarget
					//CommandSourceManager.RegisterCommandTarget(adc.ActivityCategoryHelper);

					//// Refresh the CanExecute status of all ActivityCategory commands.
					//adc.ActivityCategoryHelper.RefreshCommandsCanExecuteStatus();
					Action<MenuTool> a = new Action<MenuTool>(ProcessOpen);
					((MenuTool)sender).Dispatcher.BeginInvoke(a, sender as MenuTool);
				}
			}
		}
		#endregion //OnCategorizeMenuOpened

		// JM 03-30-11 TFS68660 Added.
		#region ProcessOpen
		static void ProcessOpen(MenuTool menuTool)
		{
			if (null != menuTool)
			{
				ActivityDialogCore adc = menuTool.DataContext as ActivityDialogCore;
				if (adc != null)
				{
					// Register the ActivityCategoryHelper as the CommandTarget
					CommandSourceManager.RegisterCommandTarget(adc.ActivityCategoryHelper);

					// Refresh the CanExecute status of all ActivityCategory commands.
					adc.ActivityCategoryHelper.RefreshCommandsCanExecuteStatus();
				}
			}
		}
		#endregion //ProcessOpen

		#region reminderComboTool_KeyUp
		static void reminderComboTool_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				ComboEditorTool comboEditorTool = sender as ComboEditorTool;
				if (comboEditorTool != null)
					AppointmentDialogUtilities.ParseReminderText(comboEditorTool, Infragistics.Windows.Editors.ValueEditor.TextProperty, comboEditorTool.Text, false);
			}
			else
				AppointmentDialogUtilities.UpdateDirtyStatus(sender);
		}
		#endregion //reminderComboTool_KeyUp

		#region reminderComboTool_LostFocus
		static void reminderComboTool_LostFocus(object sender, RoutedEventArgs e)
		{
			ComboEditorTool comboEditorTool = sender as ComboEditorTool;
			if (comboEditorTool != null)
				AppointmentDialogUtilities.ParseReminderText(comboEditorTool, Infragistics.Windows.Editors.ValueEditor.TextProperty, comboEditorTool.Text, false);
		}
		#endregion //reminderComboTool_LostFocus

		#region reminderComboTool_SelectedItemChanged
		static void reminderComboTool_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			if (_ignoreReminderComboSelectionChanged)
				return;

			ComboEditorTool comboEditorTool = sender as ComboEditorTool;
			if (comboEditorTool != null && comboEditorTool.SelectedIndex != -1)
			{
				_ignoreReminderComboSelectionChanged = true;
				AppointmentDialogUtilities.ParseReminderText(comboEditorTool, Infragistics.Windows.Editors.ValueEditor.TextProperty, comboEditorTool.Text, true);
				_ignoreReminderComboSelectionChanged = false;
			}
		}
		#endregion //reminderComboTool_SelectedItemChanged
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