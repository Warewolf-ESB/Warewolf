using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Markup;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Infragistics.Windows;
using System.Collections.ObjectModel;

namespace Infragistics.Windows.Themes
{
	#region ResourceSet class
	/// <summary>
	/// Abstract base class for resource sets
	/// </summary>
	public abstract class ResourceSet : ResourceDictionary
	{
		#region Private members
		
		// JJD 7/23/07 - ResourceWasher support
		private bool _resourcesWashed;
		private ResourceDictionary _cachedResourcesReference;

		#endregion //Private members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ResourceSet"/> class
		/// </summary>
		protected ResourceSet()
		{
			// Call ThemeManager's VerifyThemeInformationRegistered method
			// to ensure that this assemblies theme are registered
			ThemeManager.VerifyThemeInformationRegistered(this.GetType().Assembly);

			// JJD 7/23/07 - ResourceWasher support
			// cache the refernece to the resources so we don't have to ask for it again
			// inside the VerifyResourcesbelow. Otherwise, we will get into a stack
			// overflow because accessing the property will trigger the verify after
			// the initial get
			this._cachedResourcesReference = this.Resources;

			//this.MergedDictionaries.Add(this.Resources);
			this.MergedDictionaries.Add(this._cachedResourcesReference);

			// JJD 7/23/07 - ResourceWasher support
			// Verify that the resources have been washed
			this.VerifyResources();
		}

		#endregion //Constructor	
    
		#region Base class overrides

			#region ToString

		/// <summary>
		/// Returns a string reperentation of the object.
		/// </summary>
		public override string ToString()
		{
			return string.Format("{0} for {1}", new object[] { this.Theme, this.Grouping } );
		}

			#endregion //ToString

		#endregion //Base class overrides

		#region Properties

			#region Public Properties

				#region Theme

		/// <summary>
		/// Returns the name of the theme (read-only)
		/// </summary>
		public abstract string Theme
		{
			get;
		}

				#endregion //Theme

				#region Grouping

		/// <summary>
		/// Returns the category that the resources are applied to (read-only)
		/// </summary>
		/// <remarks>
		/// Examples: DataPresenter, Editors, Primitives, WPF etc.
		/// </remarks>
		public abstract string Grouping
		{
			get;
		}

				#endregion //Grouping

				#region Resources

		/// <summary>
		/// Returns the ResourceDictionary containing the associated styles (read-only)
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This member is queried within the constructor of the ResourceSet and therefore the derived class' implementation of this member should not rely upon any information provided in the constructor.</p>
		/// </remarks>
		public abstract ResourceDictionary Resources
		{
			get;
		}

				#endregion //Resources

			#endregion //Public Properties

		#endregion //Properties

		#region Methods

		// AS 1/17/08 BR29775
		#region GetSealedResource
		/// <summary>
		/// Obtains the resource stored within the ResourceDictionary with the specified key and if the resource is sealable will ensure that the object is sealed.
		/// </summary>
		/// <param name="dictionary">The dictionary whose sealed resource is to be returned.</param>
		/// <param name="key">Key of the resource within the dictionary to obtain</param>
		/// <returns>The object stored with the specified key</returns>
		public static object GetSealedResource(ResourceDictionary dictionary, object key)
		{
			if (null == dictionary)
				throw new ArgumentNullException("dictionary");

			object resource = dictionary[key];

			if (resource is Style)
				((Style)resource).Seal();
			else if (resource is FrameworkTemplate)
				((FrameworkTemplate)resource).Seal();
			else if (resource is Freezable)
				((Freezable)resource).Freeze();

			return resource;
		} 
		#endregion //GetSealedResource

		#region VerifyResources
		// JJD 7/23/07 - ResourceWasher support
		/// <summary>
		/// Called when the resources are being asked for.
		/// </summary>
		internal protected void VerifyResources()
		{
			// if the resources have already been washed then return
			if (this._resourcesWashed || this._cachedResourcesReference == null)
				return;

			// if we are loading resources then postpone the washing
			if (AssemblyResourceSetLoader.LoadingResourceSets)
				return;

			this._resourcesWashed = true;

			
#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

			ResourceWasher.ForceAutoWashResources(this._cachedResourcesReference);
		} 
		#endregion //VerifyResources

		#endregion //Methods	
	}

	#endregion //ResourceSet class
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