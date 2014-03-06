using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;

namespace Infragistics.Undo
{
	partial class UndoManager
	{
		#region Methods

		#region Public Methods

		#region AddPropertyChange

		/// <summary>
		/// Adds an <see cref="PropertyChangeUndoUnitBase"/> for the specified property value change to the undo history.
		/// </summary>
		/// <param name="owner">The instance whose property was changed</param>
		/// <param name="propertyChangedArgs">The DependencyPropertyChangedEventArgs containing the old and new value.</param>
		/// <param name="preventMerge">Used to determine if the property change should be prevented from being merged with the top entry on the undo stack when merging is allowed.</param>
		/// <param name="propertyDisplayName">The name of the property as it should be displayed to the end user. If this is not specified the actual name of the property will be used.</param>
		/// <param name="typeDisplayName">The name of the object whose property is being changed as it should be displayed to the end user.</param>
		/// <remarks>
		/// <p class="note"><b>Note:</b> In Silverlight a default description may not be able to be properly provided if the Name of the DependencyProperty cannot be obtained.</p>
		/// </remarks>
		/// <seealso cref="Infragistics.Undo.UndoUnitFactory"/>
		/// <seealso cref="UndoUnitFactory"/>
		/// <returns>Returns the UndoUnit that was added or null if one was not added</returns>
		public UndoUnit AddPropertyChange(DependencyObject owner, DependencyPropertyChangedEventArgs propertyChangedArgs, bool? preventMerge = null, string propertyDisplayName = null, string typeDisplayName = null)
		{
			// don't bother creating the undo entity if we're suspended
			if (this.IsSuspended)
				return null;

			CoreUtilities.ValidateNotNull(propertyChangedArgs, "propertyChangedArgs");

			var unit = this.UndoUnitFactoryResolved.CreatePropertyChange(owner, propertyChangedArgs, propertyDisplayName, typeDisplayName);

			if (unit == null)
				return null;

			if (preventMerge != false)
			{
				Type propertyType;




				propertyType = propertyChangedArgs.Property.PropertyType;


				this.PreventPropertyMerge(preventMerge, propertyType);
			}

			return this.AddChangeHelper(unit);
		}
		#endregion //AddToUndoHistory

		#region RegisterResources

		/// <summary>
		/// Adds an additonal Resx file in which the control will pull its resources from.
		/// </summary>
		/// <param name="name">The name of the embedded resx file that contains the resources to be used.</param>
		/// <param name="assembly">The assembly in which the resx file is embedded.</param>
		/// <remarks>Don't include the extension of the file, but prefix it with the default Namespace of the assembly.</remarks>
		public static void RegisterResources(string name, System.Reflection.Assembly assembly)
		{
#pragma warning disable 436
			SR.AddResource(name, assembly);
#pragma warning restore 436
		}

		#endregion // RegisterResources

		#region UnregisterResources

		/// <summary>
		/// Removes a previously registered resx file.
		/// </summary>
		/// <param name="name">The name of the embedded resx file that was used for registration.</param>
		/// <remarks>
		/// Note: this won't have any effect on controls that are already in view and are already displaying strings.
		/// It will only affect any new controls created.
		/// </remarks>
		public static void UnregisterResources(string name)
		{
#pragma warning disable 436
			SR.RemoveResource(name);
#pragma warning restore 436
		}
		#endregion // UnregisterResources

		#endregion //Public Methods

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