using System;
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Infragistics;
using Infragistics.Controls;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using Infragistics.Controls.Primitives;
using Infragistics.Collections;

namespace Infragistics.Controls.Editors
{

	/// <summary>
	/// A class that exposes an indexer to retrieve resources used by elements within the visual tree of <see cref="CalendarBase"/> derived controls
	/// </summary>
	public class CalendarResourceProvider : ResourceProviderWithOverrides<CalendarResourceId>
	{
		#region Private Members

		ResourceDictionary _resources;
		CalendarResourceSet _resourceSet;

		private const string ResourceSetUriMask = @"Themes/ResourceSets/ResourceSet_{0}.xaml";
		#endregion //Private Members	
    
		#region Base class overrides

		#region GetResource

		/// <summary>
		/// Returns a value for a specific id
		/// </summary>
		/// <param name="id">The id of the resource</param>
		/// <returns>The resource identified by the id or null.</returns>
		protected override object GetResource(CalendarResourceId id)
		{
			this.VerifyResources();

			return base.GetResource(id);
		}

		#endregion //GetResource	
    
		#region IsResourceValid

		/// <summary>
		/// Returns true if this is a valid resource
		/// </summary>
		/// <param name="id">The id of the resource</param>
		/// <param name="resource">The resource that was found</param>
		/// <returns>True if the resource is valid, otherwise false</returns>
		/// <remarks>
		/// <para class="body">For example, if the id represents a Bursh and a Pen is fould then this method should return false.</para>
		/// </remarks>
		protected override bool IsResourceValid(CalendarResourceId id, object resource)
		{
			switch (id)
			{

				case CalendarResourceId.CalendarDayStyle:
				case CalendarResourceId.CalendarItemStyle:
					return resource == null || resource is Style;

				case CalendarResourceId.ItemCorderRadius:
					return resource is CornerRadius;

				case CalendarResourceId.ScrollNextRepeatButtonStyle:
				case CalendarResourceId.ScrollPreviousRepeatButtonStyle:
				case CalendarResourceId.TodayButtonStyle:
					return resource is Style;
			}

			// Most of the resources are brushes
			return resource is Brush;
		}

		#endregion //IsResourceValid	

		#endregion //Base class overrides	

		#region Properties

		#region Public Properties

		#region ResourceSet

		/// <summary>
		/// Returns or sets an enumeration that determines which resource set to use.
		/// </summary>
		public CalendarResourceSet ResourceSet
		{
			get
			{
				return _resourceSet;
			}
			set
			{
				if (value != _resourceSet)
				{
					_resourceSet = value;

					_resources = null;

					this.InvalidateResources();

					this.RaisePropertyChangedEvent("ResourceSet");
				}
			}
		}

		#endregion //ResourceSet

		#endregion //Public Properties	
    
		#endregion //Properties	
    
		#region Methods

		#region Private Methods

		#region LoadResourceSet

		private static ResourceDictionary LoadResourceSet(string resourceSetName)
		{
			Uri uri = CoreUtilities.BuildEmbeddedResourceUri(typeof(CalendarResourceProvider).Assembly, string.Format(ResourceSetUriMask, resourceSetName));
			ResourceDictionary rd = new ResourceDictionary();
			rd.Source = uri;

			if (resourceSetName != "Generic")
			{
				rd.MergedDictionaries.Add(LoadResourceSet("Generic"));
			}


			// JJD 07/19/12 - TFS108812
			// In WPF we need to set the resource dictionary as the
			// Resources property on a FrameworkElement to provent
			// rooting elements that are bound to its brushes.
			var fe = new FrameworkElement();

			fe.Resources = rd;


			return rd;
		}

		#endregion //LoadResourceSet	
    
		#region VerifyResources

		private void VerifyResources()
		{
			if (base.DefaultResourcesHighContrast == null)
				base.DefaultResourcesHighContrast = LoadResourceSet("HighContrast");

			if (_resources == null)
			{
				_resources = LoadResourceSet(this.ResourceSet.ToString());

				base.DefaultResources = _resources;
			}
		}

		#endregion //VerifyResources	
    
		#endregion //Private Methods	
    
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