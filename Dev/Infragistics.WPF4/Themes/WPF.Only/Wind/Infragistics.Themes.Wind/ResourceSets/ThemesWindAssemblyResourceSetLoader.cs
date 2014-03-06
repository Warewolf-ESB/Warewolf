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
using Infragistics.Windows.Themes;
using System.Runtime.CompilerServices;

namespace Infragistics.Windows.Themes.Internal
{
	/// <summary>
	/// Loads the themes contained in this assembly.
	/// </summary>
	public class ThemesWindAssemblyResourceSetLoader : AssemblyResourceSetLoader
	{
		/// <summary>
		/// Loads and registers the resource sets
		/// </summary>
		/// <param name="groupingName">The name of the grouping whose resources should be loaded or null if all resource sets should be loaded.</param>
		// AS 11/6/07 ThemeGroupingName
		//protected override void OnRegisterResourceSets()
		protected override void OnRegisterResourceSets(string groupingName)
		{
			
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

			if (groupingName == null)
				this.RegisterAll();
			else
			{
				switch (groupingName)
				{
					case "Primitives":
						this.RegisterPrimitives();
						break;
					case "DataPresenter":
						this.RegisterDataPresenter();
						break;
					case "Editors":
						this.RegisterEditors();
						break;
					case "Ribbon":
						this.RegisterRibbon();
						break;
					case "DockManager":
						this.RegisterDockManager();
						break;
					case "OutlookBar":
						this.RegisterOutlookBar();
						break;
					case "MonthCalendar":
						this.RegisterMonthCalendar();
						break;
					case "Tiles":
						this.RegisterTiles();
						break;
				}
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void RegisterAll()
		{
			this.RegisterPrimitives();
			this.RegisterEditors();
			this.RegisterDataPresenter();
			this.RegisterRibbon();
			this.RegisterDockManager();
			this.RegisterMonthCalendar();
			this.RegisterTiles();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void RegisterPrimitives()
		{
			ThemesWindAssemblyResourceSetLoader.RegisterResourceSet(Infragistics.Windows.Themes.Wind.Primitives.Location);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void RegisterEditors()
		{
			ThemesWindAssemblyResourceSetLoader.RegisterResourceSet(Infragistics.Windows.Themes.Wind.Editors.Location);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void RegisterDataPresenter()
		{
			ThemesWindAssemblyResourceSetLoader.RegisterResourceSet(Infragistics.Windows.Themes.Wind.DataPresenter.Location);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void RegisterRibbon()
		{
			ThemesWindAssemblyResourceSetLoader.RegisterResourceSet(Infragistics.Windows.Themes.Wind.Ribbon.Location);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void RegisterDockManager()
		{
			ThemesWindAssemblyResourceSetLoader.RegisterResourceSet(Infragistics.Windows.Themes.Wind.DockManager.Location);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void RegisterOutlookBar()
		{
			ThemesWindAssemblyResourceSetLoader.RegisterResourceSet(Infragistics.Windows.Themes.Wind.OutlookBar.Location);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void RegisterMonthCalendar()
		{
			ThemesWindAssemblyResourceSetLoader.RegisterResourceSet(Infragistics.Windows.Themes.Wind.MonthCalendar.Location);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void RegisterTiles()
		{
			ThemesWindAssemblyResourceSetLoader.RegisterResourceSet(Infragistics.Windows.Themes.Wind.Tiles.Location);
		}
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