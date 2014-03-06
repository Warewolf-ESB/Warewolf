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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Infragistics.Collections;
using Infragistics.Controls.Menus;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Text;
using Infragistics.Windows.Internal;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Helper class for creating and managing <see cref="ActivityCategoryListItem"/>s for all <see cref="ActivityCategory"/>s that apply to a set of <see cref="ActivityBase"/> instances.
	/// </summary>
	/// <seealso cref="ActivityCategory"/>
	/// <seealso cref="ActivityCategoryListItem"/>
	/// <seealso cref="ActivityCategoryListItemPresenter"/>

	[InfragisticsFeature(FeatureName = "ActivityCategories", Version = "11.1")]

	public class ActivityCategoryHelper : DependencyObject, 
										  ICommandTarget
	{
		#region Member Variables

		private static Color				SelectedBackgroundColor		= Color.FromArgb(255, 252, 241, 194);
		private static Color				SelectedBorderColor			= Color.FromArgb(255, 242, 149, 54);

		private ReadOnlyObservableCollection<ActivityCategoryListItem>	_categoryCollectionReadOnly;
		private ObservableCollectionExtended<ActivityCategoryListItem>	_categoryCollection;
		private ReadOnlyObservableCollection<ActivityCategoryListItem>	_selectedCategoryCollectionReadOnly;
		private ObservableCollectionExtended<ActivityCategoryListItem>	_selectedCategoryCollection;
		private ReadOnlyObservableCollection<ActivityCategoryListItem>	_categoryOnlyCollectionReadOnly;
		private ObservableCollectionExtended<ActivityCategoryListItem>	_categoryOnlyCollection;

		private bool													_categoriesDirty = true;
		private bool													_selectedCategoriesDirty = true;

		private Resource												_owningResource;

		private FrameworkElement										_dialogOwner; // AS 6/8/11 TFS73965

		// JM 03-30-11 TFS68665
		private ActivityCategoryPresenter								_activityCategoryPresenter;
		private XamContextMenu											_activityCategoryPresenterContextMenu;




		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Creates an instance of an ActivityCategoryHelper class
		/// </summary>
		/// <param name="dataManager">A <see cref="XamScheduleDataManager"/> instance</param>
		/// <param name="activities">A list of one or more <see cref="ActivityBase"/> intances whose categories are to be managed.</param>
		/// <param name="dialogOwner">The control that should be used as the owner of the dialog when calling the <see cref="XamScheduleDataManager.DisplayActivityCategoryDialog"/> method</param>
		public ActivityCategoryHelper(XamScheduleDataManager dataManager, IList<ActivityBase> activities, FrameworkElement dialogOwner = null )
		{
			CoreUtilities.ValidateNotNull(dataManager, "dataManager");
			CoreUtilities.ValidateNotNull(activities, "activities");
			if (activities.Count < 1)
				throw new ArgumentOutOfRangeException("activities");

			if (false == this.GetDoAllActivitiesHaveSameOwningResource(activities))
				throw new InvalidOperationException(ScheduleUtilities.GetString("LE_ActivityCategoryHelper_1"));
			else
			{
				// JM 09-23-11 TFS82908 - Make sure that the owning resources are initialized.
				foreach (ActivityBase activity in activities)
					dataManager.EnsureOwningCalendarInitialized(activity);

				if (null == activities[0].OwningResource)
					throw new InvalidOperationException(ScheduleUtilities.GetString("LE_ActivityCategoryHelper_2"));

				this._owningResource = activities[0].OwningResource;
			}

			_dialogOwner		= dialogOwner; // AS 6/8/11 TFS73965

			this.DataManager	= dataManager;
			this.Activities		= activities;
		}
		#endregion //Constructor

		#region Properties

		#region Public Properties

		#region ActivityCategoryHelper (attached)

		/// <summary>
		/// Identifies the ActivityCategoryHelper attached dependency property
		/// </summary>
		public static readonly DependencyProperty ActivityCategoryHelperProperty = DependencyProperty.RegisterAttached("ActivityCategoryHelper",
			typeof(ActivityCategoryHelper), typeof(ActivityCategoryHelper),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnActivityCategoryHelperChanged))
			);

		private static void OnActivityCategoryHelperChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// JM 03-30-11 TFS68665 - Save a reference to the ActivityCategoryPresenter on the ActivityCategoryHelper.
			if (e.NewValue is ActivityCategoryHelper && d is ActivityCategoryPresenter)
			{
				((ActivityCategoryHelper)e.NewValue)._activityCategoryPresenter = d as ActivityCategoryPresenter;

				((ActivityCategoryPresenter)d).MouseRightButtonDown += new MouseButtonEventHandler(OnActivityCategoryPresenterMouseRightButtonDown);
				((ActivityCategoryPresenter)d).MouseRightButtonUp	+= new MouseButtonEventHandler(OnActivityCategoryPresenterMouseRightButtonUp);
			}
		}

		/// <summary>
		/// Gets the value of the attached ActivityCategoryHelper DependencyProperty.
		/// </summary>
		/// <param name="d">The object whose value is to be returned</param>
		/// <seealso cref="ActivityCategoryHelperProperty"/>
		/// <seealso cref="SetActivityCategoryHelper"/>
		public static ActivityCategoryHelper GetActivityCategoryHelper(DependencyObject d)
		{
			return (ActivityCategoryHelper)d.GetValue(ActivityCategoryHelper.ActivityCategoryHelperProperty);
		}

		/// <summary>
		/// Sets the value of the attached ActivityCategoryHelper DependencyProperty.
		/// </summary>
		/// <param name="d">The object whose value is to be modified</param>
		/// <param name="value">The new value</param>
		/// <seealso cref="ActivityCategoryHelperProperty"/>
		/// <seealso cref="GetActivityCategoryHelper"/>
		public static void SetActivityCategoryHelper(DependencyObject d, bool value)
		{
			d.SetValue(ActivityCategoryHelper.ActivityCategoryHelperProperty, value);
		}

		static void OnActivityCategoryPresenterMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
		}

		static void OnActivityCategoryPresenterMouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			DependencyObject d = sender as DependencyObject;
			if (null != d)
			{
				ActivityCategoryHelper ach = d.GetValue(ActivityCategoryHelperProperty) as ActivityCategoryHelper;
				if (null != ach)
				{
					if (ach.Activities.Count > 0 &&
						false == ach.Activities[0].IsOccurrence)
					{
						e.Handled			= true;

						// Find the ActivityCategory associated with the ActivityCategoryPresenter and pass that
						// to the GetContextMenu method.
						ActivityCategory activityCategoryToClear = null;
						if (d is ActivityCategoryPresenter)
						{
							if (((ActivityCategoryPresenter)d).DataContext is ActivityCategoryListItem)
								activityCategoryToClear = ((ActivityCategoryListItem)((ActivityCategoryPresenter)d).DataContext).ActivityCategory;
						}

						// JM 03-30-11 TFS68665
						if (null != ach._activityCategoryPresenterContextMenu)
						{
							ach._activityCategoryPresenterContextMenu.IsOpen	= false;
							ach._activityCategoryPresenterContextMenu			= null;
						}

						ach._activityCategoryPresenterContextMenu = ach.GetContextMenu(d, 
																						 PlacementMode.Manual, 
																						 e.GetPosition(d as UIElement).X, 
																						 e.GetPosition(d as UIElement).Y, 
																						 activityCategoryToClear);
						ach._activityCategoryPresenterContextMenu.IsOpen		= true;

						// JM 03-30-11 TFS68665
						ach._activityCategoryPresenterContextMenu.Closed		+= new EventHandler<EventArgs>(ach.OnActivityCategoryPresenterContextMenuClosed);





					}
				}
			}
		}

		#endregion //ActivityCategoryHelper (attached)

		#endregion //Public Properties

		#region Internal Properties

		#region Activities
		internal IList<ActivityBase> Activities { get; private set; }
		#endregion //Activities

		#region AreCustomActivityCategoriesAllowed
		internal bool AreCustomActivityCategoriesAllowed
		{
			get{ return this.DataManager.AreCustomActivityCategoriesAllowed(this.OwningResource); }
		}
		#endregion //AreCustomActivityCategoriesAllowed

		#region CategoryListItems





		internal ReadOnlyObservableCollection<ActivityCategoryListItem> CategoryListItems
		{
			get
			{
				this.RefreshCollections();
				if (this._categoryCollectionReadOnly == null)
					this._categoryCollectionReadOnly = new ReadOnlyObservableCollection<ActivityCategoryListItem>(this._categoryCollection);

				return this._categoryCollectionReadOnly;
			}
		}
		#endregion //CategoryListItems

		#region CategoryOnlyListItems





		internal ReadOnlyObservableCollection<ActivityCategoryListItem> CategoryOnlyListItems
		{
			get
			{
				this.RefreshCollections();
				if (this._categoryOnlyCollectionReadOnly == null)
					this._categoryOnlyCollectionReadOnly = new ReadOnlyObservableCollection<ActivityCategoryListItem>(this._categoryOnlyCollection);

				return this._categoryOnlyCollectionReadOnly;
			}
		}
		#endregion //CategoryOnlyListItems

		#region DataManager
		internal XamScheduleDataManager DataManager { get; private set; }
		#endregion //DataManager

		#region IsOwningResourceModifiable
		internal bool IsOwningResourceModifiable
		{
			get; set;
		}
		#endregion //IsOwningResourceModifiable

		#region OwningResource
		internal Resource OwningResource
		{
			get { return this._owningResource; }
		}
		#endregion //OwningResource

		#region SelectedCategoryListItems





		internal ReadOnlyObservableCollection<ActivityCategoryListItem> SelectedCategoryListItems
		{
			get
			{
				this.RefreshCollections();
				if (this._selectedCategoryCollectionReadOnly == null)
					this._selectedCategoryCollectionReadOnly = new ReadOnlyObservableCollection<ActivityCategoryListItem>(this._selectedCategoryCollection);

				return this._selectedCategoryCollectionReadOnly;
			}
		}
		#endregion //SelectedCategoryListItems

		#endregion //Internal Properties

		#region Private Properties

		#region ActivityCategoryDialogImageSource
		internal BitmapImage ActivityCategoryDialogImageSource
		{
			get
			{



				return this.GetBitmapImage("Images/Categorize_16x16.png");

			}
		}
		#endregion //ActivityCategoryDialogImageSource

		#endregion //Private Properties

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region AddActivityCategoryToOwningResource
		internal bool AddActivityCategoryToOwningResource(ActivityCategory activityCategory)
		{
			Debug.Assert(true == this.IsOwningResourceModifiable, "Trying to modify locked OwningResource!");
			if (false == this.IsOwningResourceModifiable)
				return false;

			if (null == this.OwningResource.CustomActivityCategories)
				this.OwningResource.CustomActivityCategories = new ActivityCategoryCollection();

			if (null == this.OwningResource.CustomActivityCategories.FindMatchingItem(activityCategory.CategoryName))
			{
				this.OwningResource.CustomActivityCategories.Add(activityCategory);
				this._categoriesDirty			= true;
				this._selectedCategoriesDirty	= true;

				this.UpdateCategoryOnAllActivities(activityCategory, false);

				this.RefreshCollections();

				return true;
			}

			return false;
		}
		#endregion //AddActivityCategoryToOwningResource

		#region ClearAllActivityCategories
		internal void ClearAllActivityCategories(ActivityCategoryCommandParameterInfo parameterInfo)
		{
			foreach (ActivityBase activity in this.Activities)
			{
				if (string.IsNullOrEmpty(activity.Categories) ||
					string.IsNullOrEmpty(activity.Categories.Trim()))
					continue;

				if (activity.IsInEdit || activity.IsEditCopy)
				{
					activity.Categories				= string.Empty;

					// JM 04-08-11 TFS71616
					//this._selectedCategoriesDirty	= true;
					this._categoriesDirty = true;
					continue;
				}

				DataErrorInfo	errorInfo;
				bool			retval	= this.DataManager.BeginEdit(activity, out errorInfo);
				if (retval == false || errorInfo != null)
				{



					MessageBox.Show(errorInfo.Exception.Message, ScheduleUtilities.GetString("MSG_TITLE_ErrorEditingCategories"), MessageBoxButton.OK, MessageBoxImage.Exclamation);

				}
				else
				{
					activity.Categories		= string.Empty;
					this.DataManager.EndEdit(activity);

					// JM 04-08-11 TFS71616
					//this._selectedCategoriesDirty = true;
					this._categoriesDirty = true;
				}
			}

			this.RefreshCollections();
		}
		#endregion //ClearAllActivityCategories

		#region ClearActivityCategorySelectedState
		internal void ClearActivityCategorySelectedState(ActivityCategoryCommandParameterInfo parameterInfo)
		{
			Debug.Assert(parameterInfo != null, "parameterInfo = null!");
			if (null == parameterInfo || null == parameterInfo.ActivityCategoryListItem)
				return;

			// JM 03-29-11 TFS68245
			if (parameterInfo.ActivityCategoryListItem.IsInEditMode)
				parameterInfo.ActivityCategoryListItem.IsInEditMode = false;

			ActivityCategory activityCategory = parameterInfo.ActivityCategoryListItem.ActivityCategory;
			this.UpdateCategoryOnAllActivities(activityCategory, true);
		}
		#endregion //ClearActivityCategorySelectedState

		#region DeleteActivityCategoryFromOwningResource
		internal bool DeleteActivityCategoryFromOwningResource(ActivityCategory activityCategory)
		{
			Debug.Assert(true == this.IsOwningResourceModifiable, "Trying to modify locked OwningResource!");
			if (false == this.IsOwningResourceModifiable)
				return false;

			if (null != this.OwningResource.CustomActivityCategories &&
				this.OwningResource.CustomActivityCategories.Contains(activityCategory))
			{
				this.OwningResource.CustomActivityCategories.Remove(activityCategory);
				this._categoriesDirty			= true;
				this._selectedCategoriesDirty	= true;

				this.UpdateCategoryOnAllActivities(activityCategory, true);

				this.RefreshCollections();

				return true;
			}

			return false;
		}
		#endregion //DeleteActivityCategoryFromOwningResource

		#region DirtyAndRefreshCollections
		// JM 05-20-11 TFS74019
		//internal void DirtyAndRefreshCollections()
		internal void DirtyAndRefreshCollections(bool force)
		{
			this._categoriesDirty			= true;
			this._selectedCategoriesDirty	= true;
			this.RefreshCollections(force);
		}
		#endregion //DirtyAndRefreshCollections

		#region DisplayActivityCategoriesDialog
		internal void DisplayActivityCategoriesDialog(ActivityCategoryCommandParameterInfo parameterInfo)
		{
			this.DataManager.DisplayActivityCategoryDialog(_dialogOwner, // null, /* AS 6/8/11 TFS73965 */
														   ScheduleUtilities.GetString("DLG_ActivityCategory_Caption"), 
														   this,
														   null,
														   null);
		}
		#endregion //DisplayActivityCategoriesDialog

		#region GetActivityCategoriesSnapshot
		internal Dictionary<ActivityBase, string> GetActivityCategoriesSnapshot()
		{
			this.RefreshCollections();

			Dictionary<ActivityBase, string> snapshot = new Dictionary<ActivityBase,string>(this.Activities.Count);
			foreach (ActivityBase activity in this.Activities)
			{
				snapshot.Add(activity, activity.Categories);
			}

			return snapshot;
		}
		#endregion //GetActivityCategoriesSnapshot

		#region GetContextMenu
		internal XamContextMenu GetContextMenu(DependencyObject forElement, PlacementMode placementMode, double horizontalOffset, double verticalOffset, ActivityCategory categoryToClear)
		{
			CoreUtilities.ValidateNotNull(forElement, "forElement");

			// Create the context menu but don't populate it now - wait until the Opening event is raised.
			XamContextMenu	cm	= new XamContextMenu();

			cm.Placement		= placementMode;
			cm.PlacementTarget	= forElement as UIElement;
			cm.VerticalOffset	= verticalOffset;
			cm.HorizontalOffset	= horizontalOffset;
			cm.Tag				= categoryToClear;
			
			ContextMenuManager cmMgr = Infragistics.Controls.Menus.ContextMenuService.GetManager(forElement);
			if (cmMgr == null)
				cmMgr = new ContextMenuManager();

			cmMgr.OpenMode		= OpenMode.None;
			Infragistics.Controls.Menus.ContextMenuService.SetManager(forElement, cmMgr);
			cmMgr.ContextMenu	= cm;

			cm.Opening			+= new EventHandler<OpeningEventArgs>(OnContextMenuOpening);
			cm.Closed			+=new EventHandler<EventArgs>(OnContextMenuClosed);

			return cm;
		}

		#endregion //GetContextMenu

		#region GetDoAllActivitiesContainCategory
		internal bool GetDoAllActivitiesContainCategory(IEnumerable<ActivityBase> activities, string categoryId)
		{
			bool allHaveCategory = true;

			foreach (ActivityBase activity in activities)
			{
				if (false == this.GetDoesActivityContainCategory(activity, categoryId))
				{
					allHaveCategory = false;
					break;
				}
			}

			return allHaveCategory;
		}
		#endregion //GetDoAllActivitiesContainCategory

		#region GetDoesActivityContainCategory
		internal bool GetDoesActivityContainCategory(ActivityBase activity, string categoryId)
		{
			if (activity == null || string.IsNullOrEmpty(activity.Categories))
				return false;

			return this.GetCategoriesFromStringAsList(activity.Categories).Contains(categoryId.Trim());
		}
		#endregion //GetDoesActivityContainCategory

		#region GetDoAllActivitiesHaveSameOwningResource
		internal bool GetDoAllActivitiesHaveSameOwningResource(IEnumerable<ActivityBase> activities)
		{
			bool		allHaveSameOwningResource	= true;
			Resource	lastOwningResource			= null;

			foreach (ActivityBase activity in activities)
			{
				if (null == lastOwningResource)
					lastOwningResource = activity.OwningResource;

				if (activity.OwningResource != lastOwningResource)
				{
					allHaveSameOwningResource = false;
					break;
				}
			}

			return allHaveSameOwningResource;
		}
		#endregion //GetDoAllActivitiesHaveSameOwningResource

		#region GetIconContentFromCategoryColorsAsImageSource
		internal ImageSource GetIconContentFromCategoryColorsAsImageSource(ActivityCategoryListItem listItem)
		{


#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)

			DrawingVisual		dv				= new DrawingVisual();
			DrawingContext		dc				= dv.RenderOpen();

			Brush	brushBorder = Brushes.Transparent, brushBackground = Brushes.Transparent;
			if (listItem.IsSelected)
			{
				brushBackground	= ScheduleUtilities.GetBrush(SelectedBackgroundColor);
				brushBorder		= ScheduleUtilities.GetBrush(SelectedBorderColor);
			}

			dc.DrawRectangle(brushBorder, null, new Rect(0, 0, 16, 16));
			dc.DrawRectangle(brushBackground, null, new Rect(1, 1, 14, 14));

			dc.DrawRectangle(listItem.BorderBrush, null, new Rect(2, 2, 12, 12));
			dc.DrawRectangle(listItem.BackgroundBrush, null, new Rect(3, 3, 10, 10));

			dc.Close();
			
			RenderTargetBitmap renderBitmap = new RenderTargetBitmap(16, 16, 96, 96, PixelFormats.Pbgra32);
			renderBitmap.Render(dv);

			return renderBitmap;


		}
		#endregion //GetIconContentFromCategoryColorsAsImageSource

		#region GetIsActivityCategoryCustomizable
		internal bool GetIsActivityCategoryCustomizable(ActivityCategory activityCategory)
		{
			bool isCustomizable = false;

			// JM 02-25-11 TFS67015 Also check whether custom activity categories are allowed.
			if (this.OwningResource.CustomActivityCategories != null)
				isCustomizable = this.OwningResource.CustomActivityCategories.Contains(activityCategory) &&
								 this.IsOwningResourceModifiable &&
								 this.AreCustomActivityCategoriesAllowed;

			return isCustomizable;
		}
		#endregion //GetIsActivityCategoryCustomizable

		#region GetIsActivityCategoryNameUnique
		internal bool GetIsActivityCategoryNameUnique(ActivityCategory activityCategory)
		{
			// Validate that the provided Name is unique (case-insensitive).
			foreach (ActivityCategoryListItem listitem in this.CategoryOnlyListItems)
			{
				// If the listitem does not represent a phantom ActivityCategory, check to see if
				// it's name conflicts with the provided name.
				if (false == listitem.IsNotInMasterList)
				{
					if (listitem.HasActivityCategory &&
						listitem.ActivityCategory.CategoryName.Trim().ToLower() == activityCategory.CategoryName.Trim().ToLower())
						return false;
				}
			}

			return true;
		}
		#endregion //GetIsActivityCategoryNameUnique

		#region RefreshCommandsCanExecuteStatus
		internal void RefreshCommandsCanExecuteStatus()
		{
			// Refresh the CanExecute status of all ActivityCategory commands
			CommandSourceManager.NotifyCanExecuteChanged(typeof(ActivityCategoryClearActivityCategorySelectedStateCommand));
			CommandSourceManager.NotifyCanExecuteChanged(typeof(ActivityCategoryDisplayActivityCategoriesDialogCommand));
			CommandSourceManager.NotifyCanExecuteChanged(typeof(ActivityCategoryToggleActivityCategorySelectedStateCommand));
			CommandSourceManager.NotifyCanExecuteChanged(typeof(ActivityCategoryClearAllActivityCategoriesCommand));
		}
		#endregion //RefreshCommandsCanExecuteStatus

		#region RevertToActivityCategoriesSnapshot
		internal void RevertToActivityCategoriesSnapshot(Dictionary<ActivityBase, string> snapshot)
		{
			foreach (ActivityBase activity in this.Activities)
			{
				if (snapshot.ContainsKey(activity))
					activity.Categories = snapshot[activity];
				else
					activity.Categories = string.Empty;

				this._selectedCategoriesDirty = true;
			}

			this.RefreshCollections();
		}
		#endregion //RevertToActivityCategoriesSnapshot

		#region SetActivityCategorySelectedState
		internal void SetActivityCategorySelectedState(ActivityCategoryCommandParameterInfo parameterInfo)
		{
			Debug.Assert(parameterInfo != null, "parameterInfo = null!");
			if (null == parameterInfo || null == parameterInfo.ActivityCategoryListItem)
				return;

			// JM 03-29-11 TFS68245
			if (parameterInfo.ActivityCategoryListItem.IsInEditMode)
				parameterInfo.ActivityCategoryListItem.IsInEditMode = false;

			ActivityCategory activityCategory = parameterInfo.ActivityCategoryListItem.ActivityCategory;
			this.UpdateCategoryOnAllActivities(activityCategory, false);
		}
		#endregion //SetActivityCategorySelectedState

		#region ToggleActivityCategorySelectedState
		internal void ToggleActivityCategorySelectedState(ActivityCategoryCommandParameterInfo parameterInfo)
		{
			Debug.Assert(parameterInfo != null, "parameterInfo = null!");
			if (null == parameterInfo || null == parameterInfo.ActivityCategoryListItem)
				return;

			// JM 03-29-11 TFS68245
			if (parameterInfo.ActivityCategoryListItem.IsInEditMode)
				parameterInfo.ActivityCategoryListItem.IsInEditMode = false;

			bool				shouldRemove		= parameterInfo.ActivityCategoryListItem.IsSelected;
			ActivityCategory	activityCategory	= parameterInfo.ActivityCategoryListItem.ActivityCategory;
			this.UpdateCategoryOnAllActivities(activityCategory, shouldRemove);
		}
		#endregion //ToggleActivityCategorySelectedState

		#region UpdateCategoryOnAllActivities
		internal void UpdateCategoryOnAllActivities(ActivityCategory activityCategory, bool shouldRemove)
		{
			foreach (ActivityBase activity in this.Activities)
			{
				if (shouldRemove && (string.IsNullOrEmpty(activity.Categories) || string.IsNullOrEmpty(activity.Categories.Trim())))
					continue;

				if (true == shouldRemove && false == this.GetDoesActivityContainCategory(activity, activityCategory.CategoryName))
					continue;

				if (false == shouldRemove && true == this.GetDoesActivityContainCategory(activity, activityCategory.CategoryName))
					continue;

				if (shouldRemove)
					activity.Categories = this.RemoveCategoryFromString(activity.Categories, activityCategory);
				else
					activity.Categories = this.AddCategoryToString(activity.Categories, activityCategory);

				// JM 02-23-11 TFS66901.  Be more agressice here and dirty the base collection instead of the just selected collection.
				//this._selectedCategoriesDirty = true;
				this._categoriesDirty = true;
			}

			this.RefreshCollections();
		}
		#endregion //UpdateCategoryOnAllActivities

		#region UpdateCategoryNameOnAllActivities
		internal void UpdateCategoryNameOnAllActivities(string oldName, string newName)
		{
			foreach (ActivityBase activity in this.Activities)
			{
				// JM 02-24-11 Check for null categories.
				//if (activity.Categories.Contains(oldName))
				if (false == string.IsNullOrEmpty(activity.Categories) && activity.Categories.Contains(oldName))
				{
					activity.Categories = activity.Categories.Replace(oldName, newName);
				}
			}
		}
		#endregion //UpdateCategoryNameOnAllActivities

		#endregion Internal Methods

		#region Private Methods

		#region AddCategoryToString
		private string AddCategoryToString(string categories, ActivityCategory activityCategory)
		{
			// Return the categories string 'as-is' if it already contains the category.
			List<string> categoryList = this.GetCategoriesFromStringAsList(categories);
			if (categoryList.Contains(activityCategory.CategoryName))
				return categories;

			// Add the category to the head of the list.
			if (string.IsNullOrEmpty(categories))
				return activityCategory.CategoryName;
			else
				return activityCategory.CategoryName + "," + categories;
		}
		#endregion //AddCategoryToString



#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)


		#region CreateActivityCategoryListItemFromActivityCategory
		private ActivityCategoryListItem CreateActivityCategoryListItemFromActivityCategory(ActivityCategory activityCategory)
		{
			ActivityCategoryListItem acListItem =
				new ActivityCategoryListItem(this,
											 activityCategory,
											 ActivityCategoryCommand.ToggleActivityCategorySelectedState,
											 activityCategory.CategoryName,
											 null);

			return acListItem;
		}
		#endregion //CreateActivityCategoryListItemFromActivityCategory

		#region CreateCategoriesStringFromList
		private string CreateCategoriesStringFromList(List<string> categoriesList)
		{
			Debug.Assert(categoriesList != null, "categoriesList is null!!");

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < categoriesList.Count; i++)
			{
				sb.Append(categoriesList[i]);

				if (i < categoriesList.Count - 1)
					sb.Append(",");
			}

			return sb.ToString();
		}
		#endregion //CreateCategoriesStringFromList


		#region GetBitmapImage
		private BitmapImage GetBitmapImage(string resourceName)
		{
			BitmapImage bi = new BitmapImage();
			bi.BeginInit();

			Uri					uri		= CoreUtilities.BuildEmbeddedResourceUri(typeof(ReminderDialog).Assembly, resourceName);
			StreamResourceInfo	info	= Application.GetResourceStream(uri);
			if (info != null)
				bi.StreamSource = info.Stream;
			else
				Debug.Assert(false, "Could not get StreamResourceInfo for resource Uri!!");

			bi.EndInit();

			return bi;
		}
		#endregion //GetBitmapImage


		#region GetActivityCategoryListItemFromCollectionById
		private ActivityCategoryListItem GetActivityCategoryListItemFromCollectionById(ObservableCollectionExtended<ActivityCategoryListItem> collection, string id)
		{
			foreach (ActivityCategoryListItem listItem in collection)
			{
				if (listItem.ActivityCategory			!= null &&
					listItem.ActivityCategory.CategoryName.Trim() == id.Trim())
					return listItem;
			}

			return null;
		}
		#endregion //GetActivityCategoryListItemFromCollectionById

		#region GetActivityCategoryFromListById
		private ActivityCategory GetActivityCategoryFromListById(IEnumerable<ActivityCategory> activityCategories, string id)
		{
			foreach (ActivityCategory activityCategory in activityCategories)
			{
				if (activityCategory.CategoryName.Trim() == id.Trim())
					return activityCategory;
			}

			return null;
		}
		#endregion //GetActivityCategoryFromListById

		#region GetCategoriesFromStringAsList
		private List<String> GetCategoriesFromStringAsList(string categories)
		{
			if (string.IsNullOrEmpty(categories) ||
				string.IsNullOrEmpty(categories.Trim()))
				return new List<string>();

			// JM 03-02-11 TFS67026
 			categories = categories.Trim().Trim(',');

			// Get the comma delimited categories out of the string and trim them before returning as a list.
			List<string> list1 = new List<string>(categories.Split(','));
			List<string> list2 = new List<string>(list1.Count);
			foreach (string s in list1)
				list2.Add(s.Trim());

			return list2;
		}
		#endregion //GetCategoriesFromStringAsList

		#region GetIconContentFromCategoryColorsAsElement
		private object GetIconContentFromCategoryColorsAsElement(ActivityCategoryListItem listItem)
		{
			Border outerBorder = new Border
			{
				BorderBrush		= ScheduleUtilities.GetBrush(Colors.Transparent),
				BorderThickness = new Thickness(1),
				CornerRadius	= new CornerRadius(2),
				Background		= ScheduleUtilities.GetBrush(Colors.Transparent),
				Padding			= new Thickness(1)
			};
			if (listItem.IsSelected)
			{
				outerBorder.BorderBrush = ScheduleUtilities.GetBrush(SelectedBorderColor);
				outerBorder.Background	= ScheduleUtilities.GetBrush(SelectedBackgroundColor);
			}

			Border innerBorder = new Border
			{
				Width			= 12,
				Height			= 12,
				BorderBrush		= listItem.BorderBrush,
				BorderThickness = new Thickness(1),
				Background		= listItem.BackgroundBrush
			};

			outerBorder.Child	= innerBorder;

			return outerBorder;
		}
		#endregion //GetIconContentFromCategoryColorsAsElement

		#region GetMenuItems
		private IList<Control> GetMenuItems(ReadOnlyObservableCollection<ActivityCategoryListItem> categoryListItems)
		{
			Debug.Assert(categoryListItems != null, "categoryListitems is null!");
			if (categoryListItems == null)
				return null;

			List<Control>	menuItems					= new List<Control>(categoryListItems.Count);
			bool?			lastItemWasToggleCommand	= null;

			foreach (ActivityCategoryListItem listItem in categoryListItems)
			{
				// Add a separator menuitem if we are between two menuitems, one of which supports the 
				// ToggleActivityCategorySelectedState command and the other which does not.
				if (lastItemWasToggleCommand.HasValue)
				{
					if (lastItemWasToggleCommand.Value == true && listItem.Command != ActivityCategoryCommand.ToggleActivityCategorySelectedState)
						menuItems.Add(new XamMenuSeparator());
					else
					if (lastItemWasToggleCommand.Value == false && listItem.Command == ActivityCategoryCommand.ToggleActivityCategorySelectedState)
						menuItems.Add(new XamMenuSeparator());
				}

				XamMenuItem mi	= new XamMenuItem();
				mi.Header		= listItem.Name;
				mi.Icon			= listItem.IconImageSource != null ? new Image { Source = listItem.IconImageSource } : this.GetIconContentFromCategoryColorsAsElement(listItem);
				mi.DataContext	= listItem;

				ActivityCategoryCommandSource commandSource = new ActivityCategoryCommandSource();
				commandSource.EventName		= "Click";
				commandSource.CommandType	= listItem.Command;
				commandSource.Parameter		= listItem.CommandParameter;

				mi.SetValue(Commanding.CommandProperty, commandSource);

				menuItems.Add(mi);

				lastItemWasToggleCommand = listItem.Command == ActivityCategoryCommand.ToggleActivityCategorySelectedState;
			}

			return menuItems;
		}
		#endregion //GetMenuItems

		// JM 03-30-11 TFS68665 Added.
		#region ListenForMouseLeftButtonDown


#region Infragistics Source Cleanup (Region)
























#endregion // Infragistics Source Cleanup (Region)

		#endregion //ListenForMouseLeftButtonDown

		// JM 05-19-11 TFS74019 Added.
		#region ListsContainSameCategories
		private bool ListsContainSameCategories(IList<ActivityCategoryListItem> list1, IList<ActivityCategoryListItem> list2)
		{
			CoreUtilities.ValidateNotNull(list1, "list1");
			CoreUtilities.ValidateNotNull(list2, "list2");

			if (list1.Count != list2.Count)
				return false;

			for (int i = 0; i < list1.Count; i++)
			{
				if (list1[i].ActivityCategory	!= list2[i].ActivityCategory	||
					list1[i].Command			!= list2[i].Command				||
					list1[i].Name				!= list2[i].Name				||
					list1[i].BackgroundBrush	!= list2[i].BackgroundBrush		||
					list1[i].BorderBrush		!= list2[i].BorderBrush			||
					list1[i].ForegroundBrush	!= list2[i].ForegroundBrush		||
					list1[i].IsCustomizable		!= list2[i].IsCustomizable		||
					list1[i].IsNotInMasterList	!= list2[i].IsNotInMasterList)
					return false;
			}

			return true;
		}
		#endregion //ListsContainSameCategories

		// JM 03-30-11 TFS68665 Added.
		#region OnActivityCategoryPresenterContextMenuClosed
		private void OnActivityCategoryPresenterContextMenuClosed(object sender, EventArgs e)
		{
			this._activityCategoryPresenterContextMenu = null;



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		}
		#endregion //OnActivityCategoryPresenterContextMenuClosed

		// JM 03-30-11 TFS68665 Added.
		#region OnMouseLeftButtonDown
		private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (null != this._activityCategoryPresenterContextMenu)
				this._activityCategoryPresenterContextMenu.IsOpen = false;
		}
		#endregion //OnMouseLeftButtonDown

		// JM 10-4-11 TFS90245 - Added.
		#region OnUnloaded
		void OnUnloaded(object sender, RoutedEventArgs e)
		{
			if (null != this._activityCategoryPresenterContextMenu)
				this._activityCategoryPresenterContextMenu.IsOpen = false;
		}
		#endregion //OnUnloaded

		#region RefreshCollections
		// JM 05-20-11 TFS74019 - Add 'force' parameter
		private void RefreshCollections()
		{
			this.RefreshCollections(false);
		}
		private void RefreshCollections(bool force)
		{
			this.RaiseBeforeRefreshListItems(EventArgs.Empty);

			this.RefreshBaseCollections(force);		// JM 05-20-11 TFS74019 - Add 'force' parameter
			this.RefreshSelectedCollection();

			this.RaiseAfterRefreshListItems(EventArgs.Empty);
		}
		#endregion //RefreshCollections

		#region RefreshBaseCollections
		// JM 05-20-11 TFS74019 - Add 'force' parameter
		private void RefreshBaseCollections(bool force)
		{
			if (false == force)
			{
				if (null != this._categoryCollection && false == this._categoriesDirty)
					return;
			}

			// Create the collection if it doesn't exist
			if (null == this._categoryCollection)
				this._categoryCollection = new ObservableCollectionExtended<ActivityCategoryListItem>();

			// JM 05-19-11 TFS74019 - Make initial pass on a temp list 
			//// Bracket all updates in Begin/End Update
			//this._categoryCollection.BeginUpdate();

			//// Clear the collection and re-populate.
			//this._categoryCollection.Clear();

			// Create a temporary List and add the items there.  Once we are done adding, we will check to see if the
			// new list differs from the current list - if so, we will clear the existing items and copy over the new ones.
			List<ActivityCategoryListItem> tempList = new List<ActivityCategoryListItem>();


			// Add the 'ClearAllCategories' command entry to the collection.
			ActivityCategoryListItem acListItem = new ActivityCategoryListItem(this,
																				null,
																				ActivityCategoryCommand.ClearAllActivityCategories,
																				ScheduleUtilities.GetString("DLG_Activity_Core_ClearAllCategories"),
																				null);
			// JM 05-19-11 TFS74019 - Use the temp list 
			//this._categoryCollection.Add(acListItem);
			tempList.Add(acListItem);

			// Add an entry for each ActivityCategory that is valid for the current list of Activities.
			Collection<string> categoriesUsed = new Collection<string>();
			foreach (ActivityBase activity in this.Activities)
			{
				IEnumerable<ActivityCategory> supportedActivityCategories = this.DataManager.GetSupportedActivityCategories(activity);

				if (supportedActivityCategories != null)
				{
					foreach (ActivityCategory activityCategory in supportedActivityCategories)
					{
						if (false == categoriesUsed.Contains(activityCategory.CategoryName))
						{
							acListItem = this.CreateActivityCategoryListItemFromActivityCategory(activityCategory);

							// JM 05-19-11 TFS74019 - Use the temp list 
							//this._categoryCollection.Add(acListItem);
							tempList.Add(acListItem);

							categoriesUsed.Add(activityCategory.CategoryName);
						}
					}
				}
			}

			// Add the 'DisplayCategoriesDialog' command entry to the collection.
			acListItem = new ActivityCategoryListItem(this,
													  null,
													  ActivityCategoryCommand.DisplayActivityCategoriesDialog,
													  ScheduleUtilities.GetString("DLG_Activity_Core_DisplayCategoriesDialog"),
													  this.ActivityCategoryDialogImageSource);
			// JM 05-19-11 TFS74019 - Use the temp list 
			//this._categoryCollection.Add(acListItem);

			//this._categoryCollection.EndUpdate();
			tempList.Add(acListItem);

			// Check to see if the temp list we just created is the same as the original list.  If it is not,
			// update the original list with the new entries.
			if (force || false == this.ListsContainSameCategories(tempList, this._categoryCollection))
			{
				this._categoryCollection.BeginUpdate();

				this._categoryCollection.Clear();

				foreach (ActivityCategoryListItem item in tempList)
					this._categoryCollection.Add(item);

				this._categoryCollection.EndUpdate();
			}
			tempList.Clear();
			tempList = null;

			// Refresh the selected status of each list item.
			this.RefreshListItemsSelectedStatus();

			// JM 02-23-11 TFS66901.  Force the selected categories collection to be dirty since we are recreating the base collection.
			this._selectedCategoriesDirty = true;


			// -----------------------------------------------------------------
			// Refresh the collection of 'categories only' (i.e., just items whose
			// command = ActivityCategoryCommand.ToggleActivityCategorySelectedState.
			if (null == this._categoryOnlyCollection)
				this._categoryOnlyCollection = new ObservableCollectionExtended<ActivityCategoryListItem>();

			// JM 05-19-11 TFS74019 - Make initial pass on a temp list 
			//// Bracket all updates in Begin/End Update
			//this._categoryOnlyCollection.BeginUpdate();

			//// Clear the collection and re-populate.
			//this._categoryOnlyCollection.Clear();

			// Create a temporary List and add the items there.  Once we are done adding, we will check to see if the
			// new list differs from the current list - if so, we will clear the existing items and copy over the new ones.
			tempList = new List<ActivityCategoryListItem>();

			// Add appropriate items from the base collection. 
			foreach (ActivityCategoryListItem item in this._categoryCollection)
			{
				if (item.Command == ActivityCategoryCommand.ToggleActivityCategorySelectedState)
					// JM 05-19-11 TFS74019 - Use the temp list 
					//this._categoryOnlyCollection.Add(item);
					tempList.Add(item);
			}

			// Create and add entries for categories that exist on an activity but were not returned by XamDataManager
			// as 'supported activities' and are therefore not in the base collection.  
			foreach (ActivityBase activity in this.Activities)
			{
				IList<string> categoriesOnActivity = this.GetCategoriesFromStringAsList(activity.Categories);

				foreach (string categoryName in categoriesOnActivity)
				{
					if (false == categoriesUsed.Contains(categoryName))
					{
						ActivityCategory dummyCategory = new ActivityCategory { CategoryName = categoryName, Color = null };

						acListItem = new ActivityCategoryListItem(this,
																  dummyCategory,
																  ActivityCategoryCommand.ToggleActivityCategorySelectedState,
																  ScheduleUtilities.GetString("DLG_ActivityCategory_Literal_NotInMasterList", categoryName),
																  null);

						acListItem.IsNotInMasterList	= true;
						acListItem.IsSelected			= this.GetDoAllActivitiesContainCategory(this.Activities, categoryName);

						// JM 05-19-11 TFS74019 - Use the temp list 
						//this._categoryOnlyCollection.Add(acListItem);
						tempList.Add(acListItem);

						categoriesUsed.Add(categoryName);
					}
				}
			}

			// JM 05-19-11 TFS74019 - Use the temp list 
			//this._categoryOnlyCollection.EndUpdate();
			// Check to see if the temp list we just created is the same as the original list.  If it is not,
			// update the original list with the new entries.
			if (force || false == this.ListsContainSameCategories(tempList, this._categoryOnlyCollection))
			{
				this._categoryOnlyCollection.BeginUpdate();

				this._categoryOnlyCollection.Clear();

				foreach (ActivityCategoryListItem item in tempList)
					this._categoryOnlyCollection.Add(item);

				this._categoryOnlyCollection.EndUpdate();
			}

			this._categoriesDirty = false;
		}
		#endregion //RefreshBaseCollections

		#region RefreshListItemsSelectedStatus
		private void RefreshListItemsSelectedStatus()
		{
			// Set the IsSelected property on each ActivityCategoryListItem.
			foreach (ActivityCategoryListItem item in this._categoryCollection)
			{
				if (item.HasActivityCategory && item.Command == ActivityCategoryCommand.ToggleActivityCategorySelectedState)
				{
					item.IsSelected = this.GetDoAllActivitiesContainCategory(this.Activities, item.ActivityCategory.CategoryName);
				}
			}
		}
		#endregion //RefreshListItemsSelectedStatus

		#region RefreshSelectedCollection
		private void RefreshSelectedCollection()
		{
			if (null != this._selectedCategoryCollection && false == this._selectedCategoriesDirty)
				return;

			this.RefreshListItemsSelectedStatus();

			// -----------------------------------------------------------------
			// Refresh the collection of selected categories.
			if (null == this._selectedCategoryCollection)
				this._selectedCategoryCollection = new ObservableCollectionExtended<ActivityCategoryListItem>();

			// Bracket all updates in Begin/End Update
			this._selectedCategoryCollection.BeginUpdate();

			// Clear the collection and re-populate.
			this._selectedCategoryCollection.Clear();

			// First add items from the collection that correspond to the categories in the
			// first activity.  This will ensure that the selected items are in the same order
			// as they are in the first activity.  This preserves the order of categories in the 
			// activity for scenarios where there is only 1 activity (our only 
			// scenario right now).  Not sure what we will do to preserve the order in each activity when 
			// there is more than 1.
			List<string> categoriesInFirstActivity = this.GetCategoriesFromStringAsList(this.Activities[0].Categories);
			foreach (string categoryId in categoriesInFirstActivity)
			{
				ActivityCategoryListItem acli = this.GetActivityCategoryListItemFromCollectionById(this._categoryOnlyCollection, categoryId);
				if (null != acli && acli.IsSelected)
					this._selectedCategoryCollection.Add(acli);
			}

			// The add the remaining items that were not added above.
			foreach (ActivityCategoryListItem item in this._categoryOnlyCollection)
			{
				if (false == this._selectedCategoryCollection.Contains(item) &&
					item.IsSelected &&
					item.Command == ActivityCategoryCommand.ToggleActivityCategorySelectedState)
					this._selectedCategoryCollection.Add(item);
			}

			this._selectedCategoryCollection.EndUpdate();

			this._selectedCategoriesDirty = false;
		}
		#endregion //RefreshSelectedCollection

		#region RemoveCategoryFromString
		private string RemoveCategoryFromString(string categories, ActivityCategory activityCategory)
		{
			List<string> categoryList = this.GetCategoriesFromStringAsList(categories);
			if (false == categoryList.Contains(activityCategory.CategoryName))
				return categories;

			categoryList.Remove(activityCategory.CategoryName);

			return this.CreateCategoriesStringFromList(categoryList);
		}
		#endregion //RemoveCategoryFromString

		#endregion //Private Methods

		#endregion //Methods

		#region Events

		#region AfterRefreshListItems
		internal event EventHandler AfterRefreshListItems;
		private void RaiseAfterRefreshListItems(EventArgs e)
		{
			if (AfterRefreshListItems != null)
				AfterRefreshListItems(this, e);
		}
		#endregion //AfterRefreshListItems

		#region BeforeRefreshListItems
		internal event EventHandler BeforeRefreshListItems;
		private void RaiseBeforeRefreshListItems(EventArgs e)
		{
			if (BeforeRefreshListItems != null)
				BeforeRefreshListItems(this, e);
		}
		#endregion //BeforeRefreshListItems

		#endregion //Events

		#region Event Handlers

		#region OnContextMenuClosed
		void OnContextMenuClosed(object sender, EventArgs e)
		{
			// Delay the Unregistering of the ActivityCategoryHelper as a CommandTarget until after the MenuItem Click (if any)
			// that caused the closeup is processed by the Commanding infrastructure.  If we don't delay this, we will not be
			// registered/found as a command target when the command is invoked.
			this.Dispatcher.BeginInvoke(new ScheduleUtilities.MethodInvoker(OnContextMenuClosedAsync), null);
		}
		#endregion //OnContextMenuClosed

		#region OnContextMenuClosedAsync
		private void OnContextMenuClosedAsync()
		{
			CommandSourceManager.UnregisterCommandTarget(this);
		}
		#endregion //OnContextMenuClosedAsync

		#region OnContextMenuOpening
		void OnContextMenuOpening(object sender, OpeningEventArgs e)
		{
			// Refresh the menuitems in the menu in case they have changed since the context menu was
			// requested via GetContextMenu
			IList<Control> menuItems	= this.GetMenuItems(this.CategoryListItems);
			XamContextMenu cm			= sender as  XamContextMenu;

			if (null != cm)
			{
				cm.Items.Clear();

				// If the Tag property of the ContextMenu is set to an ActivityCategory instance,
				// then include a menuitem with a command to clear that category.
				ActivityCategory activityCategory = cm.Tag as ActivityCategory;
				if (null != activityCategory)
				{
					ActivityCategoryListItem acListItem = new ActivityCategoryListItem(this, 
																						activityCategory,
																						ActivityCategoryCommand.ClearActivityCategorySelectedState,
																						ScheduleUtilities.GetString("DLG_Activity_Core_ClearCategory", activityCategory.CategoryName),
																						null);

					XamMenuItem mi	= new XamMenuItem();
					mi.Header		= acListItem.Name;
					mi.DataContext	= acListItem;

					ActivityCategoryCommandSource commandSource = new ActivityCategoryCommandSource();
					commandSource.EventName		= "Click";
					commandSource.CommandType	= acListItem.Command;
					commandSource.Parameter		= acListItem.CommandParameter;

					mi.SetValue(Commanding.CommandProperty, commandSource);

					cm.Items.Add(mi);
				}

				// Add the menuitems that represent the entries in this.CategoryListItems
				foreach (Control menuItem in menuItems)
				{
					cm.Items.Add(menuItem);
				}
			}

			// Register our instance as a CommandTarget - we will unregister when the context
			// menu closes.
			CommandSourceManager.RegisterCommandTarget(this);

			// Refresh the CanExecute status of all ActivityCategory commands.
			this.RefreshCommandsCanExecuteStatus();
		}
		#endregion //OnContextMenuOpening

		#endregion //Event Handlers

		#region ICommandTarget Members

		object ICommandTarget.GetParameter(CommandSource source)
		{
			if (source.Command is ActivityCategoryCommandBase)
				return this;

			return null;
		}

		bool ICommandTarget.SupportsCommand(ICommand command)
		{
			// If a parameter has been specified in the CommandSource, make sure it is a reference to this instance 
			// of ActivityDialogCore.
			CommandBase commandBase = command as CommandBase;
			if (null != commandBase					&&
				null != commandBase.CommandSource	&&
				commandBase.CommandSource.Parameter is ActivityCategoryCommandParameterInfo)
				return ((ActivityCategoryCommandParameterInfo)commandBase.CommandSource.Parameter).ActivityCategoryHelper == this;

			return command is ActivityCategoryCommandBase;
		}

		#endregion
	}

	#region ActivityCategoryListItem Class
	/// <summary>
	/// A wrapper class for <see cref="ActivityCategory"/> that is used to display lists of <see cref="ActivityCategory"/>s.
	/// </summary>
	/// <seealso cref="ActivityCategory"/>
	/// <seealso cref="ActivityCategoryHelper"/>
	/// <seealso cref="ActivityCategoryListItemPresenter"/>

	[InfragisticsFeature(FeatureName = "ActivityCategories", Version = "11.1")]

	public class ActivityCategoryListItem : DependencyObject
	{
		#region Member Variables

		private static bool						s_ignoreNameChange;

		#endregion //Member Variables

		#region Constructor
		internal ActivityCategoryListItem(ActivityCategoryHelper activityCategoryHelper, ActivityCategory activityCategory, ActivityCategoryCommand command, string name, ImageSource iconImageSource)
		{
			CoreUtilities.ValidateNotNull(activityCategoryHelper, "activityCategoryHelper");
			CoreUtilities.ValidateNotEmpty(name, "name");

			this.ActivityCategoryHelper	= activityCategoryHelper;
			this.ActivityCategory		= activityCategory;
			this.Name					= name;
			this.Command				= command;
			this.IconImageSource		= iconImageSource;
			this.CommandParameter		= new ActivityCategoryCommandParameterInfo(activityCategoryHelper, this);

			this.UpdateBrushesBasedOnActivityColor();
			if (this.HasActivityCategory)
				this.ActivityCategory.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(OnActivityCategoryPropertyChanged);
		}
		#endregion //Constructor

		#region Properties

		#region ActivityCategory
		/// <summary>
		/// Returns the <see cref="ActivityCategory"/> associated with the list item or null if there is no associated <see cref="ActivityCategory"/>.  (read only)
		/// </summary>
		/// <seealso cref="HasActivityCategory"/>
		public ActivityCategory ActivityCategory { get; private set; }
		#endregion //ActivityCategory

		#region ActivityCategoryHelper
		/// <summary>
		/// Returns the <see cref="ActivityCategoryHelper"/> that created the <see cref="ActivityCategoryListItem"/>.  (read only)
		/// </summary>
		/// <seealso cref="HasActivityCategory"/>
		public ActivityCategoryHelper ActivityCategoryHelper { get; private set; }
		#endregion //ActivityCategoryHelper

		#region BorderBrush

		private static readonly DependencyPropertyKey BorderBrushPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("BorderBrush",
				typeof(Brush), typeof(ActivityCategoryListItem), (Brush)null, null);

		/// <summary>
		/// Identifies the read-only <see cref="BorderBrush"/> dependency property
		/// </summary>
		public static readonly DependencyProperty BorderBrushProperty = BorderBrushPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the Brush to be used for the category color border.
		/// </summary>
		public Brush BorderBrush
		{
			get { return (Brush)this.GetValue(ActivityCategoryListItem.BorderBrushProperty); }
			internal set { this.SetValue(ActivityCategoryListItem.BorderBrushPropertyKey, value); }
		}

		#endregion //BorderBrush

		#region BackgroundBrush

		private static readonly DependencyPropertyKey BackgroundBrushPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("BackgroundBrush",
				typeof(Brush), typeof(ActivityCategoryListItem), (Brush)null, null);

		/// <summary>
		/// Identifies the read-only <see cref="BackgroundBrush"/> dependency property
		/// </summary>
		public static readonly DependencyProperty BackgroundBrushProperty = BackgroundBrushPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the Brush to be used for the category color background.
		/// </summary>
		public Brush BackgroundBrush
		{
			get { return (Brush)this.GetValue(ActivityCategoryListItem.BackgroundBrushProperty); }
			internal set { this.SetValue(ActivityCategoryListItem.BackgroundBrushPropertyKey, value); }
		}

		#endregion //BackgroundBrush

		#region Command
		/// <summary>
		/// Returns the <see cref="ActivityCategoryCommand"/> associated with the list item.  (read only)
		/// </summary>
		public ActivityCategoryCommand Command { get; private set; }
		#endregion //Command

		#region CommandParameter
		/// <summary>
		/// Returns the <see cref="ActivityCategoryCommandParameterInfo"/> associated with the list item.  (read only)
		/// </summary>
		public ActivityCategoryCommandParameterInfo CommandParameter { get; private set; }
		#endregion //CommandParameter

		// JM 07-20-11 Change ForegroundBrush to a DependencyProperty
		#region ForegroundBrush

		private static readonly DependencyPropertyKey ForegroundBrushPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ForegroundBrush",
				typeof(Brush), typeof(ActivityCategoryListItem), (Brush)null, null);

		/// <summary>
		/// Identifies the read-only <see cref="ForegroundBrush"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ForegroundBrushProperty = ForegroundBrushPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the Brush to be used for the category color foreground.
		/// </summary>
		public Brush ForegroundBrush
		{
			get { return (Brush)this.GetValue(ActivityCategoryListItem.ForegroundBrushProperty); }
			internal set { this.SetValue(ActivityCategoryListItem.ForegroundBrushPropertyKey, value); }
		}

		#endregion //ForegroundBrush

		#region HasActivityCategory
		/// <summary>
		/// Returns true if the list item has an associated <see cref="ActivityCategory"/>, otherwise returns false.  (read only) 
		/// </summary>
		/// <seealso cref="ActivityCategory"/>
		public bool HasActivityCategory
		{
			get { return null != this.ActivityCategory; }
		}
		#endregion //HasActivityCategory

		// JM 07-20-11 Change IconImageSource to a DependencyProperty
		#region IconImageSource

		private static readonly DependencyPropertyKey IconImageSourcePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IconImageSource",
				typeof(ImageSource), typeof(ActivityCategoryListItem), (ImageSource)null, null);

		/// <summary>
		/// Identifies the read-only <see cref="IconImageSource"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IconImageSourceProperty = IconImageSourcePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the ImageSource for Icon associated with item.  (read only)
		/// </summary>
		public ImageSource IconImageSource
		{
			get { return (ImageSource)this.GetValue(ActivityCategoryListItem.IconImageSourceProperty); }
			internal set { this.SetValue(ActivityCategoryListItem.IconImageSourcePropertyKey, value); }
		}

		#endregion //IconImageSource

		#region IsCustomizable
		/// <summary>
		/// Returns true if the list item refers to an <see cref="ActivityCategory"/> that is customizable - i.e., one that
		/// is contained in the owning <see cref="Resource"/>'s <see cref="Resource.CustomActivityCategories"/> collection.
		/// </summary>
		/// <seealso cref="ActivityCategory"/>
		public bool IsCustomizable
		{
			get
			{
				if (null == this.ActivityCategory)
					return false;

				return this.ActivityCategoryHelper.GetIsActivityCategoryCustomizable(this.ActivityCategory);
			}
		}
		#endregion //IsCustomizable

		#region IsInEditMode

		/// <summary>
		/// Identifies the <see cref="IsInEditMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsInEditModeProperty = DependencyPropertyUtilities.Register("IsInEditMode",
				typeof(bool), typeof(ActivityCategoryListItem), 
				DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, null));

		/// <summary>
		/// Returns whether the list item is in edit mode.
		/// </summary>
		public bool IsInEditMode
		{
			get { return (bool)this.GetValue(ActivityCategoryListItem.IsInEditModeProperty); }
			// JM 07-20-11 TFS81257 - Make this public to support 2-way binding from our templates
			//internal set { this.SetValue(ActivityCategoryListItem.IsInEditModeProperty, value); }
			set { this.SetValue(ActivityCategoryListItem.IsInEditModeProperty, value); }
		}

		#endregion //IsInEditMode

		#region IsNotInMasterList
		/// <summary>
		/// Returns true if the list item refers to an <see cref="ActivityCategory"/> that is not defined as a default category
		/// or as a custom category on the <see cref="ActivityBase"/>'s owning <see cref="Resource"/>
		/// </summary>
		/// <seealso cref="ActivityCategory"/>
		/// <seealso cref="Resource"/>
		public bool IsNotInMasterList { get; internal set; }
		#endregion //IsNotInMasterList

		// JM 07-20-11 Change IsSelected to a DependencyProperty
		#region IsSelected

		private static readonly DependencyPropertyKey IsSelectedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IsSelected",
				typeof(bool), typeof(ActivityCategoryListItem), KnownBoxes.FalseBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="IsSelected"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsSelectedProperty = IsSelectedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the list item is selected, otherwise returns false.
		/// </summary>
		public bool IsSelected
		{
			get { return (bool)this.GetValue(ActivityCategoryListItem.IsSelectedProperty); }
			internal set { this.SetValue(ActivityCategoryListItem.IsSelectedPropertyKey, value); }
		}

		#endregion //IsSelected

		#region Name

		private static readonly DependencyProperty NameProperty = DependencyPropertyUtilities.Register("Name",
			typeof(string), typeof(ActivityCategoryListItem),
			DependencyPropertyUtilities.CreateMetadata(string.Empty, new PropertyChangedCallback(OnNameChanged)));

		private static void OnNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (ActivityCategoryListItem.s_ignoreNameChange)
				return;

			ActivityCategoryListItem listItem = d as ActivityCategoryListItem;
			if (null != listItem && listItem.HasActivityCategory)
			{
				if (e.NewValue is string &&
					false == string.IsNullOrEmpty((string)e.NewValue) &&
					e.OldValue is string &&
					false == string.IsNullOrEmpty((string)e.OldValue))
				{
					// JM 03-07-11 TFS 67887.  Display a confirmation message before accepting the name change.


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

					MessageBoxResult result = MessageBox.Show(ScheduleUtilities.GetString("MSG_TEXT_RenameCategory", new object[] { (string)e.OldValue, (string)e.NewValue }), ScheduleUtilities.GetString("MSG_TITLE_RenameCategory"), MessageBoxButton.YesNo, MessageBoxImage.Question);
					if (result == MessageBoxResult.No)
					{
						ActivityCategoryListItem.s_ignoreNameChange = true;
						listItem.Name = (string)e.OldValue;
						ActivityCategoryListItem.s_ignoreNameChange = false;

						return;
					}

					listItem.ActivityCategoryHelper.UpdateCategoryNameOnAllActivities((string)e.OldValue, (string)e.NewValue);

					listItem.ActivityCategory.CategoryName = (string)e.NewValue;
				}
			}
		}

		/// <summary>
		/// Returns the Name of the <see cref="ActivityCategory"/>.  (read only)
		/// </summary>
		/// <seealso cref="ActivityCategory"/>
		public string Name
		{
			get { return (string)this.GetValue(ActivityCategoryListItem.NameProperty); }
			private set { this.SetValue(ActivityCategoryListItem.NameProperty, value); }
		}

		#endregion //Name

		#endregion //Properties

		#region Methods

		#region OnActivityCategoryPropertyChanged
		void OnActivityCategoryPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Color")
				this.UpdateBrushesBasedOnActivityColor();
		}
		#endregion //OnActivityCategoryPropertyChanged

		#region UpdateBrushesBasedOnActivityColor
		private void UpdateBrushesBasedOnActivityColor()
		{
			if (false == this.HasActivityCategory)
				return;

			if (false == this.ActivityCategory.Color.HasValue)
			{
				this.BackgroundBrush	= ScheduleUtilities.GetBrush(ActivityCategoryPresenter.NullColorBackground);
				this.BorderBrush		= ScheduleUtilities.GetBrush(ActivityCategoryPresenter.NullColorBorder);
				this.ForegroundBrush	= ScheduleUtilities.GetBrush(ActivityCategoryPresenter.DarkForegroundColor);
			}
			else
			{
				this.BackgroundBrush	= ScheduleUtilities.GetActivityCategoryBrush(this.ActivityCategory.Color.Value, ActivityCategoryBrushId.Background);
				this.ForegroundBrush	= ScheduleUtilities.GetActivityCategoryBrush(this.ActivityCategory.Color.Value, ActivityCategoryBrushId.Foreground);
				this.BorderBrush		= ScheduleUtilities.GetActivityCategoryBrush(this.ActivityCategory.Color.Value, ActivityCategoryBrushId.Border);
			}
		}
		#endregion //UpdateBrushesBasedOnActivityColor

		#endregion //Methods
	}
	#endregion //ActivityCategoryListItem Class

	// JM 03-01-11 TFS66906 Added.
	#region ActivityCategoryListItemPresenter
	/// <summary>
	/// Custom element that represents an <see cref="ActivityCategoryListItem"/>
	/// </summary>
	/// <seealso cref="ActivityCategory"/>
	/// <seealso cref="ActivityCategoryHelper"/>
	/// <seealso cref="ActivityCategoryListItem"/>
	[TemplatePart(Name = PartSelectedCheckBox, Type = typeof(CheckBox))]
	[TemplatePart(Name = PartSelectedCheckBoxOverlay, Type = typeof(Rectangle))]	//JM 05-19-11 TFS74019

	[InfragisticsFeature(FeatureName = "ActivityCategories", Version = "11.1")]

	public class ActivityCategoryListItemPresenter : Control
	{
		#region Member Variables

		// Template part names
		private const string PartSelectedCheckBox = "SelectedCheckBox";
		private const string PartSelectedCheckBoxOverlay = "SelectedCheckBoxOverlay";	//JM 05-19-11 TFS74019

		private bool						_initialized;
		private CheckBox					_selectedCheckBox;

		// JM 05-19-11 TFS74019
		private Rectangle					_selectedCheckBoxOverlay;

		// JM 08-02-12 TFS115339
		private PropertyValueTracker		_isSelectedValueTracker;

		#endregion //Member Variables

		#region Constructor
		static ActivityCategoryListItemPresenter()
		{

			ActivityCategoryListItemPresenter.DefaultStyleKeyProperty.OverrideMetadata(typeof(ActivityCategoryListItemPresenter), new FrameworkPropertyMetadata(typeof(ActivityCategoryListItemPresenter)));

		}

		/// <summary>
		/// Initializes a new <see cref="ActivityCategoryListItemPresenter"/>
		/// </summary>
		public ActivityCategoryListItemPresenter()
		{



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

			// Initialize.
			this.Dispatcher.BeginInvoke(new ScheduleUtilities.MethodInvoker(this.Initialize));
		}
		#endregion //OnApplyTemplate

		// JM 05-19-11 TFS74019 Added.
		#region OnMouseLeftButtonDown
		/// <summary>
		/// Invoked when the left mouse button is pressed down on the element.
		/// </summary>
		/// <param name="e">Provides information about the mouse operation</param>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			if (e.OriginalSource == this._selectedCheckBoxOverlay)
			{
				this.Dispatcher.BeginInvoke(new Action(this.ProcessMouseLeftButtonDown));
			}

			base.OnMouseLeftButtonDown(e);
		}
		#endregion //OnMouseLeftButtonDown

		#endregion //Base Class Overrides

		#region Methods

		#region Initialize
		private void Initialize()
		{
			if (true == this._initialized)
			{
			}


			this._selectedCheckBox = this.GetTemplateChild(PartSelectedCheckBox) as CheckBox;
			if (null != this._selectedCheckBox)
			{



				this._selectedCheckBox.Focusable = false;

			}

			// JM 05-19-11 TFS74019
			this._selectedCheckBoxOverlay = this.GetTemplateChild(PartSelectedCheckBoxOverlay) as Rectangle;

			// JM 08-02-12 TFS115339
			ActivityCategoryListItem listItem = this.DataContext as ActivityCategoryListItem;
			if (null != listItem && this._isSelectedValueTracker == null)
				this._isSelectedValueTracker = new PropertyValueTracker(listItem, "IsSelected", this.OnActivityCategoryListItemIsSelectedChanged);

			this._initialized = true;
		}
		#endregion //Initialize

		// JM 08-02-12 TFS115339 Added.
		#region OnActivityCategoryListItemIsSelectedChanged
		private void OnActivityCategoryListItemIsSelectedChanged()
		{
			ActivityCategoryListItem listItem = this.DataContext as ActivityCategoryListItem;
			if (null != listItem)
				this._selectedCheckBox.IsChecked = listItem.IsSelected;
		}
		#endregion //OnActivityCategoryListItemIsSelectedChanged

		// JM 05-19-11 TFS74019 Added.
		#region ProcessMouseLeftButtonDown
		private void ProcessMouseLeftButtonDown()
		{
			if (this._selectedCheckBox.IsChecked.HasValue && this._selectedCheckBox.IsChecked.Value == true)
				this._selectedCheckBox.IsChecked = false;
			else
				this._selectedCheckBox.IsChecked = true;
		}
		#endregion //ProcessMouseLeftButtonDown

		#endregion //Methods
	}

	#endregion //ActivityCategoryListItemPresenter
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