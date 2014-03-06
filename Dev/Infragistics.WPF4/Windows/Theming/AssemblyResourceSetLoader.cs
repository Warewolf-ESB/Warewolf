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
using Infragistics.Shared;
using Infragistics.Windows;
using Infragistics.Windows.Themes;

namespace Infragistics.Windows.Themes
{
	#region AssemblyResourceSetLoader class

	/// <summary>
	/// Used to load and register resource sets.
	/// </summary>
	/// <remarks>There is only one of the classes in an assembly. It is used to trigger the loading of all resource sets included in the assembly.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	abstract public class AssemblyResourceSetLoader
	{
		#region Private members

		// AS 11/6/07 ThemeGroupingName
		// This can be called multiple times as a new assembly is loaded.
		//
		//private bool _resourceSetsLoaded;

		// JJD 7/23/07 - ResourceWasher support
		private static int g_resourceSetloadingCount = 0;

		#endregion //Private members	
  
		#region Properties

		// JJD 7/23/07 - ResourceWasher support
		internal static bool LoadingResourceSets { get { return g_resourceSetloadingCount > 0; } }

		#endregion //Properties	
     
		#region Methods

			#region RegisterResourceSets

		/// <summary>
		/// Ensures that all resource sets are loaded and registered
		/// </summary>
		public void RegisterResourceSets()
		{
			
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


			
#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

			this.RegisterResourceSets(null);
		}

		internal void RegisterResourceSets(List<string> groupingNames)
		{
			// JJD 7/23/07 - ResourceWasher support
			// Keep track of when we are loading resource sets so we can postpone the
			// washing of resources until after initial registration
			Interlocked.Increment(ref g_resourceSetloadingCount);

			try
			{
				// AS 11/6/07 ThemeGroupingName
				// Pass the name of the loaded groupings along to the derived class so it can selectively 
				// load the resources.
				//
				//// call the virtual method to load and register the resourceSets.
				//this.OnRegisterResourceSets();
				if (null != groupingNames)
				{
					foreach (string grouping in groupingNames)
						this.OnRegisterResourceSets(grouping);
				}
				else
					this.OnRegisterResourceSets(null);
			}
			finally
			{
				// JJD 7/23/07 - ResourceWasher support
				// Keep track of when we are loading resource sets so we can postpone the
				// washing of resources until after initial registration
				Interlocked.Decrement(ref g_resourceSetloadingCount);
			}
		}

			#endregion //RegisterResourceSets	
    
			#region OnRegisterResourceSets

		/// <summary>
		/// Invoked for each grouping that is registered to allow the derived class to selectively load specific resource dictionaries.
		/// </summary>
		/// <param name="groupingName">The name of the grouping whose resources should be loaded or null if all resource sets should be loaded.</param>
		abstract protected void OnRegisterResourceSets(string groupingName);

			#endregion //OnRegisterResourceSets	
    
			#region RegisterResourceSet

		/// <summary>
		/// Helper method called by derived classes to register a specific resource set
		/// </summary>
		/// <param name="resourceSet">The resource set to register</param>
		protected static void RegisterResourceSet(ResourceSet resourceSet)
		{
			Debug.Assert(Utilities.HasSamePublicKey(resourceSet.GetType()) == false, "We should not be using this overload since it preloads the resource set.");

			ThemeManager.Register(resourceSet.Theme, resourceSet.Grouping, resourceSet.Resources);
		}

		/// <summary>
		/// Helper method called by derived classes to register a specific resource set
		/// </summary>
		/// <param name="locator">The <see cref="ResourceSetLocator"/> that can be used to lazily load the theme resources.</param>
		protected static void RegisterResourceSet(ResourceSetLocator locator)
		{
			ThemeManager.Register(locator);
		}
			#endregion //RegisterResourceSet	
    
		#endregion //Methods
	}

	#endregion //AssemblyResourceSetLoader class

	#region AssemblyResourceSetLoaderAttribute class

	/// <summary>
	/// Attribute class used to identify the <see cref="AssemblyResourceSetLoader"/> derived class that loads and registers the resource sets for an assembly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
	public sealed class AssemblyResourceSetLoaderAttribute : Attribute
	{
		#region Member Variables

		private Type resourceSetLoaderType;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="AssemblyResourceSetLoaderAttribute"/>
		/// </summary>
		/// <param name="assemblyResourceSetLoaderType">Type derived from <see cref="Infragistics.Windows.Themes.AssemblyResourceSetLoader"/> that is used to load and register themes for an assembly.</param>
		public AssemblyResourceSetLoaderAttribute(Type assemblyResourceSetLoaderType)
		{
			if (null == assemblyResourceSetLoaderType)
				throw new ArgumentNullException("assemblyResourceSetLoaderType");

			if (false == typeof(AssemblyResourceSetLoader).IsAssignableFrom(assemblyResourceSetLoaderType))
				throw new ArgumentException(SR.GetString("LE_ArgumentException_5"));

			this.resourceSetLoaderType = assemblyResourceSetLoaderType;
		}
		#endregion //Constructor

		#region Properties
		/// <summary>
		/// Returns the type that loads the resource sets for the assembly.
		/// </summary>
		public Type AssemblyResourceSetLoaderType
		{
			get { return this.resourceSetLoaderType; }
		}
		#endregion //Properties

		#region Base class overrides

		#region GetHashCode
		/// <summary>
		/// Serves as a hash function for a particular type, suitable for use in hashing algorithms and data structures like a hash table.
		/// </summary>
		public override int GetHashCode()
		{
			return this.resourceSetLoaderType.GetHashCode();
		}
		#endregion //GetHashCode

		#region Equals
		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current Object.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with the current Object.</param>
		/// <returns>true if the specified see cref="System.Object"/> is equal to the current Object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			AssemblyResourceSetLoaderAttribute loader = obj as AssemblyResourceSetLoaderAttribute;
			if (null != loader)
				return loader.AssemblyResourceSetLoaderType == this.resourceSetLoaderType;

			return false;
		}
		#endregion //Equals

		#endregion //Base class overrides
	}

	#endregion //AssemblyResourceSetLoaderAttribute class	

	// AS 11/6/07 ThemeGroupingName
	#region AssemblyThemeGroupingNameAttribute
	/// <summary>
	/// Attribute used to identify the name of the assembly to be passed to the <see cref="AssemblyResourceSetLoader.RegisterResourceSets(List&lt;string&gt;)"/> method to allow a 
	/// <see cref="AssemblyResourceSetLoader"/> to only register themes for the assemblies that have been loaded.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
	public sealed class AssemblyThemeGroupingNameAttribute : Attribute
	{
		#region Member Variables

		private string _name;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="AssemblyThemeGroupingNameAttribute"/>
		/// </summary>
		/// <param name="name">The name used to identify the assembly. This name will be passed to <see cref="AssemblyResourceSetLoader"/> instances to allow them to 
		/// selectively load theme classes.</param>
		public AssemblyThemeGroupingNameAttribute(string name)
		{
			if (null == name)
				throw new ArgumentNullException("name");

			this._name = name;
		}
		#endregion //Constructor

		#region Properties
		/// <summary>
		/// Returns the name used to identify the assembly.
		/// </summary>
		public string Name
		{
			get { return this._name; }
		}
		#endregion //Properties

		#region Base class overrides

		#region GetHashCode
		/// <summary>
		/// Serves as a hash function for a particular type, suitable for use in hashing algorithms and data structures like a hash table.
		/// </summary>
		public override int GetHashCode()
		{
			return this._name.GetHashCode();
		}
		#endregion //GetHashCode

		#region Equals
		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current Object.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with the current Object.</param>
		/// <returns>true if the specified see cref="System.Object"/> is equal to the current Object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			AssemblyThemeGroupingNameAttribute attrib = obj as AssemblyThemeGroupingNameAttribute;
			if (null != attrib)
				return attrib._name == this._name;

			return false;
		}
		#endregion //Equals

		#endregion //Base class overrides
	} 
	#endregion //AssemblyThemeGroupingNameAttribute
   
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