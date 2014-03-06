using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

using Infragistics.Collections;

namespace Infragistics.Controls.Primitives

{

	/// <summary>
	/// Abstract base class that represents a string resource
	/// </summary>
	public abstract class ResourceStringBase : PropertyChangeNotifier
	{
		#region Private Members

		private bool _isLoaded;

		private string _resourceName;

		private string _resourceString;

		// JJD 9/16/11 - TFS87912 
		// Keep a thread static cache of all the resource strings
		[ThreadStatic()]
		private static WeakList<ResourceStringBase> _ResourceStrings;

		#endregion //Private Members

		#region Constructor

		/// <summary>
		/// Instantiates a new instance of a <see cref="ResourceStringBase"/> derived class
		/// </summary>
		protected ResourceStringBase()
		{
			// JJD 9/16/11 - TFS87912 
			// initialize the cache
			if (_ResourceStrings == null )
				_ResourceStrings = new WeakList<ResourceStringBase>();

			// JJD 9/16/11 - TFS87912 
			// Keeps track of every instance created so we can notify them when the resources are invalidated
			_ResourceStrings.Add(this);
		}

		#endregion //Constructor	
	
		#region Base class overrides

		#region ToString

		/// <summary>
		/// Returns the string representation of this object
		/// </summary>
		/// <returns>The <see cref="Value"/>  property</returns>
		public override string ToString()
		{
			return this.Value;
		}

		#endregion //ToString

		#endregion //Base class overrides	
		
		#region Properties

		#region ResourceName

		/// <summary>
		/// Gets/sets the resource idenifier
		/// </summary>
		public string ResourceName
		{
			get { return this._resourceName; }

			set
			{
				if (value != this._resourceName)
				{
					this._resourceName = value;
					this._isLoaded = false;
					this.RaisePropertyChangedEvent("Value");
				}
			}
		}

		#endregion //ResourceName

		#region Value

		/// <summary>
		/// Returns the loaded string (read-only)
		/// </summary>
		public string Value
		{
			get
			{
				if (!this._isLoaded)
				{
					this._isLoaded = true;
					this._resourceString = ResourceCache.GetResource(this) as string;
				}

				return this._resourceString; 
			}
		}

		#endregion //Value	
	
		#endregion //Properties	

		#region Methods

		#region Internal Methods

		#region InvalidateCachedResources

		/// <summary>
		/// Clears any cached resource strings and notifies all derived class instances that their values may have changed.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> this method is called automatically when registering custom resources.</para>
		/// </remarks>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void InvalidateCachedResources()
		{
			// JJD 9/16/11 - TFS87912
			// Clear the cached resources
			ResourceCache.ClearCache();

			if (_ResourceStrings != null)
			{
				foreach (ResourceStringBase rs in _ResourceStrings)
					rs.InvalidateResource();
			}
		}

		#endregion //InvalidateCachedResources

		#region InvalidateResource

		private void InvalidateResource()
		{
			// JJD 9/16/11 - TFS87912
			//clear the cached values and raise a change notification
			_isLoaded = false;
			_resourceString = null;
			this.RaisePropertyChangedEvent("Value");
		}

		#endregion //InvalidateCachedResources


		#region LoadResourceString

		internal abstract string LoadResourceString();

		#endregion //LoadResourceString

		#endregion //Internal Methods	
		
		#endregion //Methods	
	
		#region ResourceCache private class

		private static class ResourceCache
		{
			#region Members

			[ThreadStatic]
			private static Dictionary<string, object> s_resourceMap;

			#endregion //Members	
	
			#region Methods

			#region ClearCache

			internal static void ClearCache()
			{
				// JJD 9/16/11 - TFS87912
				// Clear any cached resource strings
				if (s_resourceMap != null)
					s_resourceMap.Clear();
			}

			#endregion //ClearCache	
    

			#region GetResource

			internal static object GetResource(ResourceStringBase rs)
			{
				object resource = null;
				if (s_resourceMap == null)
					s_resourceMap = new Dictionary<string, object>();
				else
					s_resourceMap.TryGetValue(rs.ResourceName, out resource);

				if (resource == null)
				{
					resource = rs.LoadResourceString();

					if ( resource == null )
						resource = "";

					s_resourceMap[rs.ResourceName] = resource;
				}

				return resource;
			}

			#endregion //GetResource

			#endregion //Methods	
		
		}

		#endregion //ResourceCache private class
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