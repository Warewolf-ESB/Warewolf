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
using System.Diagnostics;
using System.ComponentModel;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Displays a UI for creating a new <see cref="ActivityCategory"/>.
	/// </summary>
	/// <remarks>
	/// The dialog will automatically add the new <see cref="ActivityCategory"/> to the specified <see cref="Resource"/>'s custom categories collection,
	/// ensuring that the category name does not conflict with any existing category.
	/// </remarks>
	public class ActivityCategoryCreationDialog : ScheduleDialogBase<ActivityCategory>
	{
		#region Member Variables

		private bool								_isCancelled = true;
		private bool								_shouldUpdateOwningResource;

		#endregion //Member Variables

		#region Constructor

		static ActivityCategoryCreationDialog()
		{

			ActivityCategoryCreationDialog.DefaultStyleKeyProperty.OverrideMetadata(typeof(ActivityCategoryCreationDialog), new FrameworkPropertyMetadata(typeof(ActivityCategoryCreationDialog)));

		}

		// AS 6/14/12 TFS113929
		/// <summary>
		/// Constructor used at design time to style the control.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public ActivityCategoryCreationDialog()
			: base()
		{



		}

		/// <summary>
		/// Creates an instance of the ActivityCategoryCreationDialog which lets the user create an <see cref="ActivityCategory"/>
		/// </summary>
		/// <remarks>
		/// The dialog will automatically add the new <see cref="ActivityCategory"/> to the specified <see cref="Resource"/>'s custom categories collection,
		/// ensuring that the category name does not conflict with any existing category.
		/// </remarks>
		/// <param name="activityCategoryHelper">A reference to an <see cref="ActivityCategoryHelper"/> instance.</param>
		/// <param name="creationResult">An instance of the <see cref="ScheduleDialogBase&lt;TChoice&gt;.ChooserResult"/> class that holds the <see cref="ActivityCategory"/> created by the dialog and (optionally) a piece of user data.</param>
		/// <param name="updateOwningResource">True to automatically update the owning <see cref="Resource"/> with the new <see cref="ActivityCategory"/>.</param>
		public ActivityCategoryCreationDialog(ActivityCategoryHelper activityCategoryHelper, ChooserResult creationResult, bool updateOwningResource) : base(creationResult)
		{




			CoreUtilities.ValidateNotNull(activityCategoryHelper, "activityCategoryHelper");
			CoreUtilities.ValidateNotNull(creationResult, "creationResult");

			this.ActivityCategoryHelper			= activityCategoryHelper;
			this._shouldUpdateOwningResource	= updateOwningResource;
		}
		#endregion //Constructor

		#region Base Class Overrides

		#region CanSaveAndClose
		internal override bool CanSaveAndClose
		{
			get
			{
				// JM 02-23-11 TFS66896 - Trim the Name string before checking for empty.
				return false == string.IsNullOrEmpty(this.CategoryName.Trim());
			}
		} 
		#endregion //CanSaveAndClose

		#region Initialize
		internal override void Initialize()
		{
			CommandSourceManager.NotifyCanExecuteChanged(typeof(ScheduleDialogSaveAndCloseCommand));

			XamScheduleDataManager dataManager = this.DataManager;
			ScheduleDataConnectorBase dataConnector = dataManager != null ? dataManager.DataConnector : null;
			if (dataConnector != null)
			{
				bool areCustomColorCategoriesAllowed;
				this.DefaultCategoryColors = dataConnector.GetDefaultCategoryColorsResolved(out areCustomColorCategoriesAllowed);
				this.AreCustomColorsAllowed = areCustomColorCategoriesAllowed;
			}

			base.Initialize(); // AS 4/11/11 TFS71618
		} 
		#endregion //Initialize

		
		#region InitializeLocalizedStrings
		internal override void InitializeLocalizedStrings(Dictionary<string, string> localizedStrings)
		{
			localizedStrings.Add("DLG_ActivityCategory_Literal_Color", ScheduleUtilities.GetString("DLG_ActivityCategory_Literal_Color"));
			localizedStrings.Add("DLG_ScheduleDialog_Btn_Ok", ScheduleUtilities.GetString("DLG_ScheduleDialog_Btn_Ok"));
			localizedStrings.Add("DLG_ScheduleDialog_Btn_Cancel", ScheduleUtilities.GetString("DLG_ScheduleDialog_Btn_Cancel"));

			// JM 04-08-11 TFS72027
			localizedStrings.Add("DLG_ActivityCategoryCreation_Name", ScheduleUtilities.GetString("DLG_ActivityCategoryCreation_Name"));
		} 
		#endregion //InitializeLocalizedStrings

		#region OnClosing

		internal override bool OnClosing()
		{
			if (this._isCancelled)
			{
				this.Result.Choice = null;
				return false;
			}

			var helper = this.ActivityCategoryHelper;

			// AS 6/14/12 TFS113929
			if (helper == null)
				return false;

			// Initialize an ActivityCategory instance with the selected Name and Color.
			ActivityCategory activityCategoryToAdd = new ActivityCategory { Color = this.Color, CategoryName = this.CategoryName.Trim() };


			// Edit the CategoryName for uniqueness
			if (false == helper.GetIsActivityCategoryNameUnique(activityCategoryToAdd))
			{



				MessageBox.Show(ScheduleUtilities.GetString("MSG_TEXT_DuplicateCategoryName", activityCategoryToAdd.CategoryName.Trim()), ScheduleUtilities.GetString("MSG_TITLE_DuplicateCategoryName"), MessageBoxButton.OK, MessageBoxImage.Exclamation);

				this._isCancelled = true;	// re-initialize the cancelled flag

				return true;				// cancel the dialog closing since there was an error.
			}


			if (this._shouldUpdateOwningResource && helper.IsOwningResourceModifiable)
			{
				DataErrorInfo errorInfo;
				helper.DataManager.BeginEdit(helper.OwningResource, out errorInfo);
				if (null != errorInfo)
				{
					MessageBox.Show(ScheduleUtilities.GetString("MSG_TEXT_CantEditResource"), ScheduleUtilities.GetString("MSG_TITLE_CantEditResource"), MessageBoxButton.OK);

					this._isCancelled = true;	// re-initialize the cancelled flag

					return true;				// cancel the dialog closing since there was an error.
				}

				helper.AddActivityCategoryToOwningResource(activityCategoryToAdd);

				ResourceOperationResult ror = helper.DataManager.EndEdit(helper.OwningResource, true);
				if (null != ror.Error)
				{
					MessageBox.Show(ror.Error.ToString(), ScheduleUtilities.GetString("LE_ActivityCategoryCreationDialog_1"), MessageBoxButton.OK);

					this._isCancelled = true;	// re-initialize the cancelled flag

					return true;				// cancel the dialog closing since there was an error.
				}
			}

			this.Result.Choice = activityCategoryToAdd;
			return false;	// don't cancel the dialog closing
		}

		#endregion //OnClosing

		#region Save
		internal override void Save()
		{
			_isCancelled = false;
		} 
		#endregion //Save

		#region SupportsCommand
		internal override bool SupportsCommand(ICommand command)
		{
			// If a parameter has been specified in the CommandSource, make sure it is a reference to this instance 
			// of ActivityCategoryCreationDialog.
			CommandBase commandBase = command as CommandBase;
			if (null != commandBase &&
				null != commandBase.CommandSource &&
				null != commandBase.CommandSource.Parameter)
				return commandBase.CommandSource.Parameter == this;

			return base.SupportsCommand(command);
		}
		#endregion //SupportsCommand

		#endregion //Base Class Overrides

		#region Properties

		#region Private Properties

		#region ActivityCategoryHelper
		internal ActivityCategoryHelper ActivityCategoryHelper
		{
			get; set;
		}
		#endregion //ActivityCategoryHelper

		#endregion //Private Properties

		#region Public Properties

		#region AreCustomColorsAllowed

		private static readonly DependencyPropertyKey AreCustomColorsAllowedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("AreCustomColorsAllowed",
				typeof(bool), typeof(ActivityCategoryCreationDialog), KnownBoxes.TrueBox, null);

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
			get { return (bool)this.GetValue(ActivityCategoryCreationDialog.AreCustomColorsAllowedProperty); }
			internal set { this.SetValue(ActivityCategoryCreationDialog.AreCustomColorsAllowedPropertyKey, value); }
		}

		#endregion //AreCustomColorsAllowed

		#region Color

		/// <summary>
		/// Identifies the <see cref="Color"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ColorProperty = DependencyPropertyUtilities.Register("Color",
			typeof(Color?), typeof(ActivityCategoryCreationDialog),
			DependencyPropertyUtilities.CreateMetadata(System.Windows.Media.Color.FromArgb(255, 191, 191, 191), new PropertyChangedCallback(OnColorChanged))
			);

		private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
		}

		/// <summary>
		/// Returns the current color.
		/// </summary>
		public Color? Color
		{
			get { return (Color?)this.GetValue(ActivityCategoryCreationDialog.ColorProperty); }
			set { this.SetValue(ActivityCategoryCreationDialog.ColorProperty, value); }
		}

		#endregion //Color

		#region DataManager
		/// <summary>
		/// Returns the <see cref="XamScheduleDataManager"/> associated with the dialog.
		/// </summary>
		public XamScheduleDataManager DataManager
		{
			get 
			{
				// AS 6/14/12 TFS113929
				//return this.ActivityCategoryHelper.DataManager; 
				var helper = this.ActivityCategoryHelper;
				return helper != null ? helper.DataManager : null;
			}
		}
		#endregion //DataManager

		#region DefaultCategoryColors

		private static readonly DependencyPropertyKey DefaultCategoryColorsPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("DefaultCategoryColors",
				typeof(IList<Color>), typeof(ActivityCategoryCreationDialog), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="DefaultCategoryColors"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DefaultCategoryColorsProperty = DefaultCategoryColorsPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns an array of the default category colors as provided by the <see cref="ScheduleDataConnectorBase"/>. (read only)
		/// </summary>
		/// <seealso cref="DefaultCategoryColorsProperty"/>
		/// <seealso cref="AreCustomColorsAllowed"/>
		public IList<Color> DefaultCategoryColors
		{
			get { return (IList<Color>)this.GetValue(ActivityCategoryCreationDialog.DefaultCategoryColorsProperty); }
			internal set { this.SetValue(ActivityCategoryCreationDialog.DefaultCategoryColorsPropertyKey, value); }
		}

		#endregion //DefaultCategoryColors

		#region CategoryName

		/// <summary>
		/// Identifies the <see cref="CategoryName"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CategoryNameProperty = DependencyPropertyUtilities.Register("CategoryName",
			typeof(string), typeof(ActivityCategoryCreationDialog),
			DependencyPropertyUtilities.CreateMetadata(string.Empty, new PropertyChangedCallback(OnNameChanged))
			);

		private static void OnNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CommandSourceManager.NotifyCanExecuteChanged(typeof(ScheduleDialogSaveAndCloseCommand));
		}

		/// <summary>
		/// Returns or sets the current name for the new <see cref="ActivityCategory"/>.
		/// </summary>
		public string CategoryName
		{
			get { return (string)this.GetValue(ActivityCategoryCreationDialog.CategoryNameProperty); }
			set { this.SetValue(ActivityCategoryCreationDialog.CategoryNameProperty, value); }
		}

		#endregion //CategoryName

		#endregion //Public Properties

		#endregion //Properties
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