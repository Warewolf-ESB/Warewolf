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

namespace Infragistics
{
	/// <summary>
	/// Allows objects to to specify which properties should be saved when the PersistenceManager goes to save it.
	/// </summary>
	public interface IProvidePropertyPersistenceSettings
	{
		/// <summary>
		/// Gets a List of properties that shouldn't be saved when the PersistenceManager goes to save them.
		/// </summary>
		List<string> PropertiesToIgnore { get; }

        /// <summary>
        /// Gets a List of properties that should be applied, before even trying to look at any other property on the object.
        /// </summary>
        List<string> PriorityProperties { get; }

        /// <summary>
        /// Allows an object to perform an operation, after it's been loaded.
        /// </summary>
        void FinishedLoadingPersistence();
	}

    /// <summary>
    /// Allows an object to perform some sort of initialization, before it is loaded via the Control Persistence Framework.
    /// </summary>
    public interface INeedInitializationForPersistence
    {
        /// <summary>
        /// This method is used to setup the object that was just created. 
        /// </summary>
        /// <param name="owner">The object that owns the object being initialized. </param>
        void InitObject(object owner);
    }

    /// <summary>
    /// Allows an object to save and load itself, instead of having the Control Persistence Framework, decie how it should be saved and loaded.
    /// </summary>
    public interface IProvideCustomPersistence
    {
        /// <summary>
        /// Gets the string representation of the object, that can be later be passed into the Load method of this object, in order to rehydrate.
        /// </summary>
        /// <returns></returns>
        string Save();

        /// <summary>
        /// Takes the string that was created in the Save method, and rehydrates the object. 
        /// </summary>
        /// <param name="owner">This is the object who owns this object as a property.</param>
        /// <param name="value"></param>
        void Load(object owner, string value);
    }

    /// <summary>
    /// Some collections have specific keys that identify them. So, when saving and loading, it's important that these properties
    /// get set first, so that they can be rehydrated instead of destroyed.
    /// </summary>
    public interface IProvidePersistenceLookupKeys
    {
        /// <summary>
        /// Gets a list of keys that each object in the collection has. 
        /// </summary>
        /// <returns></returns>
        Collection<string> GetLookupKeys();

        /// <summary>
        /// Looks through the keys, and determines that all the keys are in the collection, and that the same about of objects are in the collection.
        /// If this isn't the case, false is returned, and the Control Persistence Framework, will not try to reuse the object that are already in the collection.
        /// </summary>
        /// <param name="lookupKeys"></param>
        /// <returns></returns>
        bool CanRehydrate(Collection<string> lookupKeys);
    }

    /// <summary>
    /// An interface that will interupt the normal saving of an object by the persistence framework, so that it can save itself.
    /// </summary>
    public interface IProvideCustomObjectPersistence
    {
        /// <summary>
        /// Saves the object in simplified terms for the persitence framework
        /// </summary>
        /// <returns>A serializable object.</returns>
        object SaveObject();

        /// <summary>
        /// Given the object that was originally stored, the object can now rehydrate itself. 
        /// </summary>
        /// <param name="data"></param>
        void LoadObject(object data);

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