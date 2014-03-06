using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Element used to represent an <see cref="Appointment"/> instance
	/// </summary>
	public class AppointmentPresenter : ActivityPresenter
	{
		#region Constructor
		static AppointmentPresenter()
		{

			AppointmentPresenter.DefaultStyleKeyProperty.OverrideMetadata(typeof(AppointmentPresenter), new FrameworkPropertyMetadata(typeof(AppointmentPresenter)));

		}

		/// <summary>
		/// Initializes a new <see cref="AppointmentPresenter"/>
		/// </summary>
		public AppointmentPresenter()
		{



		}
		#endregion //Constructor

		#region Base Class Overrides

		#region OnActivityChanged

		/// <summary>
		/// Invoked when the associated activity has changed.
		/// </summary>
		/// <param name="oldValue">Old activity</param>
		/// <param name="newValue">New activity</param>
		protected override void OnActivityChanged(ActivityBase oldValue, ActivityBase newValue)
		{
			base.OnActivityChanged(oldValue, newValue);

			this.SetAdditionalText();
		}

		#endregion //OnActivityChanged	

		#region OnActivityPropertyChanged
		internal override void OnActivityPropertyChanged(string property, object extraInfo)
		{
			bool isAllProps = string.IsNullOrEmpty(property);
			if (isAllProps || property == "Location")
			{
				this.SetAdditionalText();

				this.SetProviderBrushes();
			}

			base.OnActivityPropertyChanged(property, extraInfo);
		}

		#endregion // OnActivityPropertyChanged
    
		#region SetProviderBrushes
		internal override void SetProviderBrushes()
		{
			base.SetProviderBrushes();

			// AS 1/5/11 NA 11.1 Activity Categories
			if ( this.IsInitializing )
				return;

			Appointment appt = this.Activity as Appointment;

			if (appt == null)
				return;

			string location = appt.Location;

			if ( CoreUtilities.IsValueEmpty(appt.Location) )
			{
				this.ClearValue(AdditionalTextVisibilityPropertyKey);
				this.ClearValue(SeparatorVisibilityPropertyKey);
				return;
			}

			if (this.IsInEditMode)
			{
				this.ClearValue(SeparatorVisibilityPropertyKey);

				if (this.IsSingleLineDisplay)
					this.ClearValue(AdditionalTextVisibilityPropertyKey);
				else
					this.SetValue(AdditionalTextVisibilityPropertyKey, KnownBoxes.VisibilityVisibleBox);
			}
			else
			{
				this.SetValue(AdditionalTextVisibilityPropertyKey, KnownBoxes.VisibilityVisibleBox);

				if (this.IsSingleLineDisplay)
					this.SetValue(SeparatorVisibilityPropertyKey, KnownBoxes.VisibilityVisibleBox);
				else
					this.ClearValue(SeparatorVisibilityPropertyKey);
			}

		}

		#endregion // SetProviderBrushes

		#endregion //Base Class Overrides

		#region Properties

		#region Public Properties

		#endregion //Public Properties	
    
		#endregion //Properties	
    
		#region Methods

		#region Private Methods

		#region SetAdditionalText

		private void SetAdditionalText()
		{
			Appointment appt = this.Activity as Appointment;

			string location = appt != null ? appt.Location : null;

			if (string.IsNullOrEmpty(location))
				this.ClearValue(AdditionalTextPropertyKey);
			else
				this.AdditionalText = location;
		}

		#endregion //SetAdditionalText	

		#endregion //PrivateMethods

		#endregion //Methods
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