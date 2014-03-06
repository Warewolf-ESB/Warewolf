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
using Infragistics.Persistence.Primitives;

namespace Infragistics.Persistence
{
	/// <summary>
	/// An object that contains the settings for how an object should save and load properties. 
	/// </summary>
	public class PersistenceSettings : DependencyObject
	{
		#region Members

		PropertyPersistenceInfoBaseCollection _propertySettings;
		PropertyPersistenceInfoBaseCollection _ignorePropertySettings;
		PersistenceEvents _events;

		#endregion // Members

		#region Properties

		#region SavePersistenceOptions

		/// <summary>
		/// Gets/sets which properties should be saved. 
		/// </summary>
		public PersistenceOption SavePersistenceOptions
		{
			get;
			set;
		}

		#endregion // SavePersistenceOptions

		#region LoadPersistenceOptions

		/// <summary>
		/// Gets/sets which properties should be loaded. 
		/// </summary>
		public PersistenceOption LoadPersistenceOptions
		{
			get;
			set;
		}

		#endregion // LoadPersistenceOptions

		#region PropertySettings

		/// <summary>
		/// Gets a collection of PropertyPersistenceInfoBase objects to be saved/loaded.
		/// </summary>
		/// <remarks>
		/// When <see cref="SavePersistenceOptions"/> or <see cref="LoadPersistenceOptions"/> are set to OnlySpecified, just properties 
		/// that are identified here will be saved/loaded. 
		/// Otherwise this collection will be used to identify how a property should be saved/loaded.
		/// </remarks>
		public PropertyPersistenceInfoBaseCollection PropertySettings
		{
			get
			{
				if (this._propertySettings == null)
					this._propertySettings = new PropertyPersistenceInfoBaseCollection();

				return this._propertySettings;
			}
		}

		#endregion // PropertySettings 

		#region IgnorePropertySettings

		/// <summary>
		/// Gets a collection of PropertyPersistenceInfoBase objects that shouldn't be saved/loaded. 
		/// </summary>
		/// <remarks>
		/// Properties that are identified here will not be saved/loaded. 
		/// </remarks>
		public PropertyPersistenceInfoBaseCollection IgnorePropertySettings
		{
			get
			{
				if (this._ignorePropertySettings == null)
					this._ignorePropertySettings = new PropertyPersistenceInfoBaseCollection();

				return this._ignorePropertySettings;
			}
		}

		#endregion // IgnorePropertySettings

		#region Events

		/// <summary>
		/// Gets/Sets the events that will be raised while saving and loading this <see cref="PersistenceSettings"/>
		/// </summary>
		public PersistenceEvents Events
		{
			get
			{
				if (this._events == null)
				{
					this._events = new PersistenceEvents();
				}

				return this._events; 
			}
			set
			{
				this._events = value;
			}

		}

		#endregion // Events 

		#endregion // Properties

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