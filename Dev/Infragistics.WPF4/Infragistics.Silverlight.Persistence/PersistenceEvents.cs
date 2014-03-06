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

namespace Infragistics.Persistence
{
	/// <summary>
	/// An object that contains the events used for Saving and Loading the persistence of <see cref="DependencyObject"/>s.
	/// </summary>
	public class PersistenceEvents
	{
		#region SavePropertyPersistence

		/// <summary>
		/// Occurs before a property is saved. 
		/// </summary>
		public event EventHandler<SavePropertyPersistenceEventArgs> SavePropertyPersistence;

		internal void OnSavePropertyPersistence(SavePropertyPersistenceEventArgs args)
		{
			if (this.SavePropertyPersistence != null)
			{
				this.SavePropertyPersistence(this, args);
			}
		}

		#endregion // SavePropertyPersistence

		#region LoadPropertyPersistence

		/// <summary>
		/// Occurs before a property is loaded.
		/// </summary>
		public event EventHandler<LoadPropertyPersistenceEventArgs> LoadPropertyPersistence;

		internal void OnLoadPropertyPersistence(LoadPropertyPersistenceEventArgs args)
		{
			if (this.LoadPropertyPersistence != null)
			{
				this.LoadPropertyPersistence(this, args);
			}
		}

		#endregion // LoadPropertyPersistence

		#region PersistenceLoaded

		/// <summary>
		/// Occurs after all properties are loaded. 
		/// </summary>
		public event EventHandler<PersistenceLoadedEventArgs> PersistenceLoaded;

		internal void OnPersistenceLoaded(PersistenceLoadedEventArgs args)
		{
			if (this.PersistenceLoaded != null)
			{
				this.PersistenceLoaded(this, args);
			}
		}

		#endregion // PersistenceLoaded

		#region PersistenceSaved

		/// <summary>
		/// Ocurrs afert all properties are saved. 
		/// </summary>
		public event EventHandler<PersistenceSavedEventArgs> PersistenceSaved;

		internal void OnPersistenceSaved(PersistenceSavedEventArgs args)
		{
			if (this.PersistenceSaved != null)
			{
				this.PersistenceSaved(this, args);
			}
		}

		#endregion // PersistenceSaved
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