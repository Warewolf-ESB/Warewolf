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
using System.ComponentModel;
using System.Collections.Generic;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A base class for settings that will be attached to a <see cref="Column"/> object.
	/// </summary>
	public class ColumnSettings : DependencyObjectNotifier, IProvidePropertyPersistenceSettings
	{
		#region Members

		Column _column;
		List<string> _propertiesThatShouldntBePersisted;

		#endregion // Members

		#region Properties

		#region Protected

		#region Column
		/// <summary>
		/// Gets the <see cref="Column"/> object that this settings object is applying to.
		/// </summary>
		public virtual Column Column
		{
			get
			{
				return this._column;
			}
			protected internal set
			{
				if (this._column != value)
				{
					this._column = value;
				}
			}
		}
		#endregion // Column

		#region PropertiesToIgnore

		/// <summary>
		/// Gets a List of properties that shouldn't be saved when the PersistenceManager goes to save them.
		/// </summary>
		protected virtual List<string> PropertiesToIgnore
		{
			get
			{
				if (this._propertiesThatShouldntBePersisted == null)
				{
					this._propertiesThatShouldntBePersisted = new List<string>()
					{
                        "Column"
					};
				}

				return this._propertiesThatShouldntBePersisted;
			}
		}
		#endregion // PropertiesToIgnore

		#region PriorityProperties

		/// <summary>
		/// Gets a List of properties that should be applied, before even trying to look at any other property on the object.
		/// </summary>
		protected virtual List<string> PriorityProperties
		{
			get { return null; }
		}

		#endregion // PriorityProperties

		#endregion // Protected

		#endregion // Properties

		#region Methods

		#region Protected

		#region OnLoadedCatchUp

		/// <summary>
		/// Designed to be called during the <see cref="XamGrid"/> OnLoaded to allow any processing 
		/// due to objects added in the XAML but not able to be processed until other objects populated.
		/// </summary>
		protected internal virtual void OnLoadedCatchUp()
		{
		}

		#endregion //OnLoadedCatchUp

		#region FinishedLoadingPersistence

		/// <summary>
		/// Allows an object to perform an operation, after it's been loaded.
		/// </summary>
		protected virtual void FinishedLoadingPersistence()
		{

		}

		#endregion // FinishedLoadingPersistence

        #region Reset

        /// <summary>
        /// When overridden in derived classes allows the <see cref="ColumnSettings"/> object a chance to clean itself.
        /// </summary>
        protected internal virtual void Reset()
        {
        }
        #endregion // Reset

		#endregion // Protected

		#endregion // Methods

		#region IProvidePropertyPersistenceSettings Members

		#region PropertiesToIgnore

		List<string> IProvidePropertyPersistenceSettings.PropertiesToIgnore
		{
			get
			{
				return this.PropertiesToIgnore;
			}
		}

		#endregion // PropertiesToIgnore

		#region PriorityProperties

		List<string> IProvidePropertyPersistenceSettings.PriorityProperties
		{
			get { return this.PriorityProperties; }
		}

		void IProvidePropertyPersistenceSettings.FinishedLoadingPersistence()
		{
			this.FinishedLoadingPersistence();
		}
		#endregion // PriorityProperties

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