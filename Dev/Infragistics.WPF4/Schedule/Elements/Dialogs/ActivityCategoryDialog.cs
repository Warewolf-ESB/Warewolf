using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Displays a UI for managing <see cref="ActivityCategory"/>s.
	/// </summary>
	/// <seealso cref="ActivityCategory"/>
	/// <seealso cref="ActivityCategoryHelper"/>
	[TemplatePart(Name = PartCategoriesList, Type = typeof(ListBox))]
	public class ActivityCategoryDialog : Control,
										  ICommandTarget,
										  IDialogElementProxyHost
	{
		#region Member Variables

		// Template part names
		private const string PartCategoriesList = "CategoriesList";

		private ActivityCategoryHelper						_activityCategoryHelper;
		private DialogElementProxy							_dialogElementProxy;
		private bool										_initialized;
		private Dictionary<ActivityBase, string>			_activityCategoriesSnapshot;
		private bool										_undoUpdatesOnClose = true;
		private ListBox										_categoriesListBox;
		private Dictionary<string, string>					_localizedStrings;
		private ActivityCategoryCreationDialog.ChooserResult
															_activityCategoryCreationResult;
		private int											_oldSelectedIndex;

		#endregion //Member Variables

		#region Constructors
		static ActivityCategoryDialog()
		{

			ReminderDialog.DefaultStyleKeyProperty.OverrideMetadata(typeof(ActivityCategoryDialog), new FrameworkPropertyMetadata(typeof(ActivityCategoryDialog)));

		}

		/// <summary>
		/// Creates an instance of the ActivityCategoryDialog.
		/// </summary>
		/// <param name="activityCategoryHelper">An instance of the <see cref="ActivityCategoryHelper"/> class.</param>
		public ActivityCategoryDialog(ActivityCategoryHelper activityCategoryHelper)
		{




			CoreUtilities.ValidateNotNull(activityCategoryHelper, "activityCategoryHelper");

			this._activityCategoryHelper = activityCategoryHelper;

			// Establish whether the OwningResource assopciated with the activities is modifiable.  First check IsLocked on the
			// on the Resource, then do a BeginEdit on the OwningResource.  If there is an error, disable the functions that update the
			// owning resource.
			if (this.ActivityCategoryHelper.OwningResource.IsLocked)
			{
				this.IsOwningResourceModifiable = this.ActivityCategoryHelper.IsOwningResourceModifiable = false;
			}
			else
			{
				DataErrorInfo errorInfo;
				this.DataManager.BeginEdit(this.ActivityCategoryHelper.OwningResource, out errorInfo);
				if (null != errorInfo)
				{



					MessageBox.Show(ScheduleUtilities.GetString("MSG_TEXT_CantEditResource"), ScheduleUtilities.GetString("MSG_TITLE_CantEditResource"), MessageBoxButton.OK, MessageBoxImage.Information);

					this.IsOwningResourceModifiable = this.ActivityCategoryHelper.IsOwningResourceModifiable = false;
				}
				else
					this.IsOwningResourceModifiable = this.ActivityCategoryHelper.IsOwningResourceModifiable = true;
			}
		}

		#endregion //Constructor

		#region Base Class Overrides

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this.DialogElementProxy.Initialize();

			// Initialize.
			this.Dispatcher.BeginInvoke(new ScheduleUtilities.MethodInvoker(this.Initialize));
		}
		#endregion //OnApplyTemplate

		#region OnKeyUp
		/// <summary>
		/// Called before the System.Windows.UIElement.KeyUp event occurs
		/// </summary>
		/// <param name="e"></param>
		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);

			if (e.Key == Key.Escape)
			{
				if (this.SelectedActivityCategoryListItem != null &&
					this.SelectedActivityCategoryListItem.IsInEditMode)
					this.SelectedActivityCategoryListItem.IsInEditMode = false;
				else
					this.Close();
			}
			else
			if (e.Key == Key.F2)
			{
				if (this.SelectedActivityCategoryListItem				!= null &&
					this.SelectedActivityCategoryListItem.IsInEditMode	== false)
					this.SelectedActivityCategoryListItem.IsInEditMode = true;
			}
		}
		#endregion //OnKeyUp

		#endregion //Base Class Overrides

		#region Properties

		#region Public Properties

		#region ActivityCategoryListItems

		private static readonly DependencyPropertyKey ActivityCategoryListItemsPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ActivityCategoryListItems",
			typeof(ReadOnlyObservableCollection<ActivityCategoryListItem>), typeof(ActivityCategoryDialog), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="ActivityCategoryListItems"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ActivityCategoryListItemsProperty = ActivityCategoryListItemsPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a read only collection of <see cref="ActivityCategoryListItem"/>s that represent the <see cref="ActivityCategory"/>s assigned to the currently selected <see cref="ActivityBase"/>. (read only)
		/// </summary>
		/// <seealso cref="ActivityCategoryListItemsProperty"/>
		/// <seealso cref="ActivityCategory"/>
		/// <seealso cref="ActivityCategoryListItem"/>
		/// <seealso cref="SelectedActivityCategoryListItem"/>
		/// <seealso cref="ActivityCategoryHelper"/>
		public ReadOnlyObservableCollection<ActivityCategoryListItem> ActivityCategoryListItems
		{
			get
			{
				return (ReadOnlyObservableCollection<ActivityCategoryListItem>)this.GetValue(ActivityCategoryDialog.ActivityCategoryListItemsProperty);
			}
			internal set
			{
				this.SetValue(ActivityCategoryDialog.ActivityCategoryListItemsPropertyKey, value);
			}
		}

		#endregion //ActivityCategoryListItems

		#region AreCustomColorsAllowed

		private static readonly DependencyPropertyKey AreCustomColorsAllowedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("AreCustomColorsAllowed",
				typeof(bool), typeof(ActivityCategoryDialog), KnownBoxes.TrueBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="AreCustomColorsAllowed"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AreCustomColorsAllowedProperty = AreCustomColorsAllowedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the <see cref="ScheduleDataConnectorBase"/> allow custom colors to be specified on <see cref="ActivityCategory"/>s. (read only)
		/// </summary>
		/// <remarks>
		/// If this returns false the only colors from the <see cref="DefaultCategoryColors"/> array are allowed to be set on <see cref="ActivityCategory"/>s.
		/// </remarks>
		/// <seealso cref="AreCustomColorsAllowedProperty"/>
		/// <seealso cref="DefaultCategoryColors"/>
		public bool AreCustomColorsAllowed
		{
			get { return (bool)this.GetValue(ActivityCategoryDialog.AreCustomColorsAllowedProperty); }
			internal set { this.SetValue(ActivityCategoryDialog.AreCustomColorsAllowedPropertyKey, value); }
		}

		#endregion //AreCustomColorsAllowed

		#region DataManager
		/// <summary>
		/// Returns the <see cref="XamScheduleDataManager"/> associated with the dialog.
		/// </summary>
		public XamScheduleDataManager DataManager
		{
			get { return this.ActivityCategoryHelper.DataManager; }
		}
		#endregion //DataManager

		#region DefaultCategoryColors

		private static readonly DependencyPropertyKey DefaultCategoryColorsPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("DefaultCategoryColors",
				typeof(IList<Color>), typeof(ActivityCategoryDialog), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="DefaultCategoryColors"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DefaultCategoryColorsProperty = DefaultCategoryColorsPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns an array of the default category colors as provided by the <see cref="ScheduleDataConnectorBase"/>. (read only)
		/// </summary>
		/// <seealso cref="DefaultCategoryColorsProperty"/>
		/// <seealso cref="AreCustomColorsAllowed"/>
		public IList<Color>  DefaultCategoryColors
		{
			get{ return (IList<Color>)this.GetValue(ActivityCategoryDialog.DefaultCategoryColorsProperty); }
			internal set { this.SetValue(ActivityCategoryDialog.DefaultCategoryColorsPropertyKey, value); }
		}

		#endregion //DefaultCategoryColors

		// JM 04-07-11 TFS71671 Added.
		#region IsActivityCategorySelected

		private static readonly DependencyPropertyKey IsActivityCategorySelectedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IsActivityCategorySelected",
				typeof(bool), typeof(ActivityCategoryDialog), KnownBoxes.FalseBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="IsActivityCategorySelected"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsActivityCategorySelectedProperty = IsActivityCategorySelectedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if an <see cref="ActivityCategory"/> is selected in the dialog's list of available activity categories. (read only)
		/// </summary>
		/// <seealso cref="IsActivityCategorySelectedProperty"/>
		/// <seealso cref="ActivityCategory"/>
		public bool IsActivityCategorySelected
		{
			get
			{
				return (bool)this.GetValue(ActivityCategoryDialog.IsActivityCategorySelectedProperty);
			}
			internal set
			{
				this.SetValue(ActivityCategoryDialog.IsActivityCategorySelectedPropertyKey, value);
			}
		}

		#endregion //IsActivityCategorySelected

		#region IsOwningResourceModifiable

		private static readonly DependencyPropertyKey IsOwningResourceModifiablePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IsOwningResourceModifiable",
				typeof(bool), typeof(ActivityCategoryDialog), KnownBoxes.FalseBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="IsOwningResourceModifiable"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsOwningResourceModifiableProperty = IsOwningResourceModifiablePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the owning <see cref="Resource"/> for the selected <see cref="ActivityBase"/>s is modifiable.  If not, certain functions in the dialog will be disabled (e.g., New, Delete, Rename and color changes). (read only)
		/// </summary>
		/// <seealso cref="IsOwningResourceModifiableProperty"/>
		/// <seealso cref="Resource"/>
		/// <seealso cref="ActivityBase"/>
		public bool IsOwningResourceModifiable
		{
			get
			{
				return (bool)this.GetValue(ActivityCategoryDialog.IsOwningResourceModifiableProperty);
			}
			internal set
			{
				this.SetValue(ActivityCategoryDialog.IsOwningResourceModifiablePropertyKey, value);
			}
		}

		#endregion //IsOwningResourceModifiable

		// JM 04-29-11 TFS74011 Added.
		#region IsSelectedActivityCategoryListItemCustomizable

		private static readonly DependencyPropertyKey IsSelectedActivityCategoryListItemCustomizablePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IsSelectedActivityCategoryListItemCustomizable",
				typeof(bool), typeof(ActivityCategoryDialog), KnownBoxes.FalseBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="IsSelectedActivityCategoryListItemCustomizable"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsSelectedActivityCategoryListItemCustomizableProperty = IsSelectedActivityCategoryListItemCustomizablePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if an <see cref="ActivityCategory"/> is selected in the dialog's list of available activity categories. (read only)
		/// </summary>
		/// <seealso cref="IsSelectedActivityCategoryListItemCustomizableProperty"/>
		/// <seealso cref="ActivityCategory"/>
		public bool IsSelectedActivityCategoryListItemCustomizable
		{
			get
			{
				return (bool)this.GetValue(ActivityCategoryDialog.IsSelectedActivityCategoryListItemCustomizableProperty);
			}
			internal set
			{
				this.SetValue(ActivityCategoryDialog.IsSelectedActivityCategoryListItemCustomizablePropertyKey, value);
			}
		}

		#endregion //IsSelectedActivityCategoryListItemCustomizable

		#region LocalizedStrings
		/// <summary>
		/// Returns a dictionary of localized strings for use by the controls in the template.
		/// </summary>
		public Dictionary<string, string> LocalizedStrings
		{
			get
			{
				if (this._localizedStrings == null)
				{
					this._localizedStrings = new Dictionary<string, string>(10);

					this._localizedStrings.Add("DLG_ActivityCategory_HelpText", ScheduleUtilities.GetString("DLG_ActivityCategory_HelpText"));
					this._localizedStrings.Add("DLG_ActivityCategory_NameColumnHeader", ScheduleUtilities.GetString("DLG_ActivityCategory_NameColumnHeader"));
					this._localizedStrings.Add("DLG_ActivityCategory_Btn_New", ScheduleUtilities.GetString("DLG_ActivityCategory_Btn_New"));
					this._localizedStrings.Add("DLG_ActivityCategory_Btn_Rename", ScheduleUtilities.GetString("DLG_ActivityCategory_Btn_Rename"));
					this._localizedStrings.Add("DLG_ActivityCategory_Btn_Delete", ScheduleUtilities.GetString("DLG_ActivityCategory_Btn_Delete"));
					this._localizedStrings.Add("DLG_ScheduleDialog_Btn_Ok", ScheduleUtilities.GetString("DLG_ScheduleDialog_Btn_Ok"));
					this._localizedStrings.Add("DLG_ScheduleDialog_Btn_Cancel", ScheduleUtilities.GetString("DLG_ScheduleDialog_Btn_Cancel"));
					this._localizedStrings.Add("DLG_ActivityCategory_Literal_Color", ScheduleUtilities.GetString("DLG_ActivityCategory_Literal_Color"));
					this._localizedStrings.Add("DLG_ActivityCategoryCreation_Title", ScheduleUtilities.GetString("DLG_ActivityCategoryCreation_Title"));
				}

				return this._localizedStrings;
			}
		}
		#endregion //LocalizedStrings

		#region SelectedActivityCategoryListItem

		private static readonly DependencyPropertyKey SelectedActivityCategoryListItemPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("SelectedActivityCategoryListItem",
			typeof(ActivityCategoryListItem), typeof(ActivityCategoryDialog), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="SelectedActivityCategoryListItem"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedActivityCategoryListItemProperty = SelectedActivityCategoryListItemPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the currently selected <see cref="ActivityCategoryListItem"/>. (read only)
		/// </summary>
		/// <seealso cref="SelectedActivityCategoryListItemProperty"/>
		/// <seealso cref="ActivityCategory"/>
		/// <seealso cref="ActivityCategoryListItem"/>
		/// <seealso cref="ActivityCategoryListItems"/>
		/// <seealso cref="ActivityCategoryHelper"/>
		public ActivityCategoryListItem SelectedActivityCategoryListItem
		{
			get
			{
				return (ActivityCategoryListItem)this.GetValue(ActivityCategoryDialog.SelectedActivityCategoryListItemProperty);
			}
			internal set
			{
				this.SetValue(ActivityCategoryDialog.SelectedActivityCategoryListItemPropertyKey, value);
			}
		}

		#endregion //SelectedActivityCategoryListItem

		#endregion //Public Properties

		#region Internal Properties

		#region ActivityCategoryHelper
		internal ActivityCategoryHelper ActivityCategoryHelper
		{
			get	{ return this._activityCategoryHelper; }
		}
		#endregion //ActivityCategoryHelper

		#region DialogElementProxy
		internal DialogElementProxy DialogElementProxy
		{
			get
			{
				if (this._dialogElementProxy == null)
					this._dialogElementProxy = new DialogElementProxy(this);

				return this._dialogElementProxy;
			}
		}
		#endregion //DialogElementProxy

		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region Close
		internal void Close()
		{
			this.DialogElementProxy.Close();
		}
		#endregion //Close

		#region CreateNewCategory
		internal void CreateNewCategory()
		{
			this._activityCategoryCreationResult = new ActivityCategoryCreationDialog.ChooserResult(null);
															
			this.DataManager.DisplayActivityCategoryCreationDialog(this,
																   ScheduleUtilities.GetString("DLG_ActivityCategoryCreation_Title"),
																   this.ActivityCategoryHelper,
																   this._activityCategoryCreationResult,
																   false,
																   null,
																   this.OnActivityCategoryCreationDialogClosed);
		}
		#endregion //CreateNewCategory

		#region DeleteSelectedCategory
		internal void DeleteSelectedCategory()
		{
			if (null == this.SelectedActivityCategoryListItem ||
				null == this.SelectedActivityCategoryListItem.ActivityCategory)
				return;

			ActivityCategory selectedActivityCategory = this.SelectedActivityCategoryListItem.ActivityCategory;






			MessageBoxResult result = MessageBox.Show(ScheduleUtilities.GetString("MSG_TEXT_DeleteCategory", selectedActivityCategory.CategoryName.Trim()), ScheduleUtilities.GetString("MSG_TITLE_DeleteCategory"), MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (result == MessageBoxResult.No)
				return;


			this.ActivityCategoryHelper.DeleteActivityCategoryFromOwningResource(this.SelectedActivityCategoryListItem.ActivityCategory);

			if (null != this._categoriesListBox)
				this._categoriesListBox.Focus();
		}
		#endregion //DeleteSelectedCategory

		#region EditSelectedCategory
		internal void EditSelectedCategory()
		{
			if (null != this.SelectedActivityCategoryListItem)
				this.SelectedActivityCategoryListItem.IsInEditMode = true;
		}
		#endregion //EditSelectedCategory

		#region SaveAndClose
		internal void SaveAndClose()
		{
			this._undoUpdatesOnClose = false;
			this.DialogElementProxy.Close();
		}
		#endregion //SaveAndClose

		#endregion //Internal Methods

		#region Private Methods

		#region Initialize
		private void Initialize()
		{
			if (true == this._initialized)
			{
				if (null != this._categoriesListBox)
					this._categoriesListBox.SelectionChanged -= new SelectionChangedEventHandler(OnCategoriesListSelectionChanged);
			}

			this.ActivityCategoryListItems		= this.ActivityCategoryHelper.CategoryOnlyListItems;
			this._activityCategoriesSnapshot	= this.ActivityCategoryHelper.GetActivityCategoriesSnapshot();

			this._categoriesListBox				= this.GetTemplateChild(PartCategoriesList) as ListBox;
			if (null != this._categoriesListBox)
			{
				this._categoriesListBox.SelectionChanged += new SelectionChangedEventHandler(OnCategoriesListSelectionChanged);
				if (this._categoriesListBox.Items.Count > 0)
					this._categoriesListBox.SelectedIndex = 0;

				// JM 03-01-11 TFS66906
				this._categoriesListBox.Focus();
				this._categoriesListBox.KeyUp += new KeyEventHandler(OnCategoriesListBoxKeyUp);
			}

			// JM 03-01-11 TFS66906
			this.ActivityCategoryHelper.BeforeRefreshListItems += new EventHandler(OnActivityCategoryHelperBeforeRefreshListItems);
			this.ActivityCategoryHelper.AfterRefreshListItems	+= new EventHandler(OnActivityCategoryHelperAfterRefreshListItems);

			// Ask the connector for the default list of colors that can be assigned to an ActivityCategory, as well
			// as whether customer colors can be assigned.
			bool customColorsAllowed;
			this.DefaultCategoryColors		= this.DataManager.DataConnector.GetDefaultCategoryColorsResolved(out customColorsAllowed);
			this.AreCustomColorsAllowed		= customColorsAllowed;
				
			// Register our ActivityCategoryHelper instance as a CommandTarget.  
			// We will unregister when the dialog closes.
			CommandSourceManager.RegisterCommandTarget(this.ActivityCategoryHelper);

			// JM 04-07-11 TFS71671
			this.UpdateCommandsStatus();

			this._initialized = true;
		}
		#endregion //Initialize

		#region OnActivityCategoryCreationDialogClosed
		private void OnActivityCategoryCreationDialogClosed(bool? dialogResult)
		{
			if (null != this._activityCategoryCreationResult.Choice)
			{
				ActivityCategory activityCategory = this._activityCategoryCreationResult.Choice;
				this.ActivityCategoryHelper.AddActivityCategoryToOwningResource(activityCategory);

				this.SelectListItemByActivityCategory(activityCategory);

				if (null != this._categoriesListBox)
					this._categoriesListBox.Focus();
			}
		}
		#endregion //OnActivityCategoryCreationDialogClosed

		#region SelectListItemByActivityCategory
		private void SelectListItemByActivityCategory(ActivityCategory activityCategory)
		{
			for (int i = 0; i < this._categoriesListBox.Items.Count; i++)
			{
				if (((ActivityCategoryListItem)this._categoriesListBox.Items[i]).ActivityCategory == activityCategory)
				{
					this._categoriesListBox.SelectedIndex = i;
					return;
				}
			}

			if (this._categoriesListBox.Items.Count > 0)
				this._categoriesListBox.SelectedIndex = 0;
		}
		#endregion //SelectListItemByActivityCategory

		#region UpdateCommandsStatus
		private void UpdateCommandsStatus()
		{
			CommandSourceManager.NotifyCanExecuteChanged(typeof(ActivityCategoryDialogSaveAndCloseCommand));
			CommandSourceManager.NotifyCanExecuteChanged(typeof(ActivityCategoryDialogCreateNewCategoryCommand));
			CommandSourceManager.NotifyCanExecuteChanged(typeof(ActivityCategoryDialogDeleteSelectedCategoryCommand));
			CommandSourceManager.NotifyCanExecuteChanged(typeof(ActivityCategoryDialogEditSelectedCategoryCommand));
		}
		#endregion //UpdateCommandsStatus

		#endregion //Private Methods

		#endregion //Methods

		#region Event Handlers

		// JM 03-01-11 TFS66906 Added.
		#region OnActivityCategoryHelperAfterRefreshListItems
		void OnActivityCategoryHelperAfterRefreshListItems(object sender, EventArgs e)
		{
			if (null == this._categoriesListBox)
				return;

			if (this._oldSelectedIndex < this._categoriesListBox.Items.Count)
				this._categoriesListBox.SelectedIndex = this._oldSelectedIndex;
			else
			if (this._categoriesListBox.Items.Count > 0)
				this._categoriesListBox.SelectedIndex = Math.Max(0, Math.Min(this._categoriesListBox.Items.Count - 1, this._oldSelectedIndex - 1));
		}
		#endregion //OnActivityCategoryHelperAfterRefreshListItems

		// JM 03-01-11 TFS66906 Added.
		#region OnActivityCategoryHelperBeforeRefreshListItems
		void OnActivityCategoryHelperBeforeRefreshListItems(object sender, EventArgs e)
		{
			if (null == this._categoriesListBox)
				return;

			this._oldSelectedIndex = Math.Max(0, this._categoriesListBox.SelectedIndex);
		}
		#endregion //OnActivityCategoryHelperBeforeRefreshListItems

		// JM 03-01-11 TFS66906 Added.
		#region OnCategoriesListBoxKeyUp
		void OnCategoriesListBoxKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Space)
			{
				if (this.SelectedActivityCategoryListItem != null &&
					false == this.SelectedActivityCategoryListItem.IsInEditMode)
				{
					this.ActivityCategoryHelper.ToggleActivityCategorySelectedState(new ActivityCategoryCommandParameterInfo(this.ActivityCategoryHelper, this.SelectedActivityCategoryListItem));

					// Ensure that the ListBoxItem representing the selected item has focus so that arrow key navigation
					// works properly.
					if (this._categoriesListBox					!= null &&
						this._categoriesListBox.SelectedIndex	!= -1)
					{
						this._categoriesListBox.InvalidateMeasure();
						this._categoriesListBox.UpdateLayout();
						ListBoxItem container = this._categoriesListBox.ItemContainerGenerator.ContainerFromIndex(this._categoriesListBox.SelectedIndex) as ListBoxItem;
						if (null != container)
							container.Focus();
					}
				}
			}
		}
		#endregion //OnCategoriesListBoxKeyUp

		#region OnCategoriesListSelectionChanged
		void OnCategoriesListSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// If there was a previous selection and it is in editmode, take it out of edit mode.
			if (this.SelectedActivityCategoryListItem != null &&
				this.SelectedActivityCategoryListItem.IsInEditMode)
				this.SelectedActivityCategoryListItem.IsInEditMode = false;

			
			this.SelectedActivityCategoryListItem = e.AddedItems.Count > 0 ? e.AddedItems[0] as ActivityCategoryListItem : null;

			// JM 04-07-11 TFS71671
			this.IsActivityCategorySelected	= (null != this.SelectedActivityCategoryListItem);

			// JM 04-29-11 TFS74011
			if (this.IsActivityCategorySelected)
				this.IsSelectedActivityCategoryListItemCustomizable = this.IsActivityCategorySelected && this.SelectedActivityCategoryListItem.IsCustomizable;
			else
				this.IsSelectedActivityCategoryListItemCustomizable = false;

			this.UpdateCommandsStatus();
		}
		#endregion //OnCategoriesListSelectionChanged

		#endregion //Event Handlers

		#region ICommandTarget Members

		object ICommandTarget.GetParameter(CommandSource source)
		{
			if (source.Command is ActivityCategoryDialogCommandBase)
				return this;

			return null;
		}

		bool ICommandTarget.SupportsCommand(ICommand command)
		{
			// If a parameter has been specified in the CommandSource, make sure it is a reference to this instance 
			// of ActivityCategoryDialog.
			CommandBase commandBase = command as CommandBase;
			if (null != commandBase					&&
				null != commandBase.CommandSource	&&
				null != commandBase.CommandSource.Parameter)
				return commandBase.CommandSource.Parameter == this;

			return command is ActivityCategoryDialogCommandBase;
		}

		#endregion

		#region IDialogElementProxyHost Members

		bool IDialogElementProxyHost.OnClosing()
		{
			CommandSourceManager.UnregisterCommandTarget(this.ActivityCategoryHelper);

			if (this.SelectedActivityCategoryListItem != null &&
				this.SelectedActivityCategoryListItem.IsInEditMode)
				this.SelectedActivityCategoryListItem.IsInEditMode = false;

			if (this._undoUpdatesOnClose)
			{
				// Reset the categories on each Activity to their values when the dialog was first displayed.
				this._activityCategoryHelper.RevertToActivityCategoriesSnapshot(this._activityCategoriesSnapshot);

				DataErrorInfo errorInfo;
				this.DataManager.CancelEdit(this.ActivityCategoryHelper.OwningResource, out errorInfo);
				if (null != errorInfo)
					MessageBox.Show(errorInfo.ToString(), ScheduleUtilities.GetString("LE_ActivityCategoryCreationDialog_1"), MessageBoxButton.OK);

				// Now done below.
				//this.ActivityCategoryHelper.DirtyAndRefreshCollections();
			}
			else
			{
				if (this.IsOwningResourceModifiable)
				{
					ResourceOperationResult ror = this.DataManager.EndEdit(this.ActivityCategoryHelper.OwningResource, true);
					if (null != ror.Error)
						MessageBox.Show(ror.Error.ToString(), ScheduleUtilities.GetString("LE_ActivityCategoryCreationDialog_1"), MessageBoxButton.OK);
				}
			}


			this.ActivityCategoryHelper.BeforeRefreshListItems	-= new EventHandler(OnActivityCategoryHelperBeforeRefreshListItems);
			this.ActivityCategoryHelper.AfterRefreshListItems	-= new EventHandler(OnActivityCategoryHelperAfterRefreshListItems);

			// JM 05-20-11 TFS74019
			this.ActivityCategoryHelper.DirtyAndRefreshCollections(true);

			// Don't cancel the closing.
			return false;
		}

		#endregion
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