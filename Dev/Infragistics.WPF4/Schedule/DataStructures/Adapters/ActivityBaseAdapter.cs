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
using System.Diagnostics;
using Infragistics.Controls.Primitives;
using System.Collections.Generic;

namespace Infragistics.Controls.Schedules.Primitives
{
	internal class ActivityBaseAdapter : RecyclingContainer<ActivityPresenter>
	{
		#region Member Variables

		private ActivityBase _activity;
		private IObjectFactory<ActivityPresenter> _factory;
		private static IObjectFactory<ActivityPresenter>[] _factories;

		#endregion // Member Variables

		#region Constructor
		static ActivityBaseAdapter()
		{
			List<ActivityType> types = ScheduleUtilities.GetEnumValues<ActivityType>();
			_factories = new IObjectFactory<ActivityPresenter>[types.Count];

			foreach (ActivityType type in types)
			{
				int index = (int)type;
				Debug.Assert(index >= 0 && index < types.Count, "Unexpected type value:" + index.ToString());

				_factories[index] = GetActivityFactory(type);
			}
		}

		internal ActivityBaseAdapter(ActivityBase activity)
		{
			this.Initialize(activity);
		}
		#endregion // Constructor

		#region Base class overrides
		protected override ActivityPresenter CreateInstanceOfRecyclingElement()
		{
			return _factory.Create();
		}

		protected override Type RecyclingElementType
		{
			get
			{
				return _factory.Type;
			}
		}

		protected override void OnElementAttached(ActivityPresenter element)
		{
			base.OnElementAttached(element);

			element.Activity = _activity;
			element.DataContext = _activity;
			element.IsSelected = _isSelected;
			element.IsHiddenDragSource = _isHiddenDragSource;
		}

		protected override void OnElementReleased(ActivityPresenter element)
		{
			base.OnElementReleased(element);

			element.Activity = null;
			element.DataContext = null;
		}

		protected override bool OnElementReleasing(ActivityPresenter element)
		{
			if (element.IsInEditMode)
				return false;

			return base.OnElementReleasing(element);
		}
		#endregion // Base class overrides

		#region Properties

		#region IsHiddenDragSource
		private bool _isHiddenDragSource;

		internal bool IsHiddenDragSource
		{
			get { return _isHiddenDragSource; }
			set
			{
				if (value != _isHiddenDragSource)
				{
					_isHiddenDragSource = value;
					var ap = this.AttachedElement as ActivityPresenter;

					if (null != ap)
						ap.IsHiddenDragSource = value;
				}
			}
		}
		#endregion // IsHiddenDragSource

		private bool _isSelected;

		internal bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				if (value != _isSelected)
				{
					_isSelected = value;

					var ap = this.AttachedElement as ActivityPresenter;

					if (null != ap)
						ap.IsSelected = value;
				}
			}
		}

		#endregion // Properties

		#region Methods

		#region GetActivityFactory
		private static IObjectFactory<ActivityPresenter> GetActivityFactory(ActivityType type)
		{
			switch (type)
			{
				case ActivityType.Appointment:
					return new ObjectFactory<AppointmentPresenter>();
				case ActivityType.Journal:
					return new ObjectFactory<JournalPresenter>();
				case ActivityType.Task:
					return new ObjectFactory<TaskPresenter>();
				default:
					Debug.Assert(false, "Unrecognized activity type:" + type.ToString());
					return new ObjectFactory<ActivityPresenter>();
			}
		}
		#endregion // GetActivityFactory

		#region Initialize
		internal void Initialize(ActivityBase activity)
		{
			Debug.Assert(this.AttachedElement == null, "The activity is being changed but its already associated with an element?");

			CoreUtilities.ValidateNotNull(activity, "activity");
			_activity = activity;
			_factory = _factories[(int)activity.ActivityType];
		} 
		#endregion // Initialize 

		#endregion // Methods
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