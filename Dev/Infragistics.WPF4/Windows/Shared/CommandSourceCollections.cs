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
using Infragistics.Controls;
using Infragistics.Collections;

namespace Infragistics
{
	/// <summary>
	/// A collection of <see cref="CommandSource" /> objects which support a command.
	/// </summary>
	public class CommandSourceCollection : CollectionBase<CommandSource>
    {
        #region Members

        FrameworkElement _element;

        #endregion // Members

        #region Properties

        #region Public
        /// <summary>
		/// Gets / sets the framework element which the command is attached to.
		/// </summary>
		public FrameworkElement Element
		{
            get { return this._element; }
            set
            {
                if (this._element != value)
                {
                    FrameworkElement prevVal = this._element;
                    this._element = value;

                    if (prevVal == null)
                    {
                        foreach (CommandSource source in this)
                        {
                            CommandSourceManager.RegisterCommandSource(source, this.Element);
                        }
                    }
                }
            }
		}

		#endregion // Public

		#endregion // Properties

		#region Overrides
		/// <summary>
		/// Registers the <see cref="CommandSource"/> object with the <see cref="CommandSourceManager"/> and adds the object to the collection.
		/// </summary>
		/// <param name="index">The index location to add the object to.</param>
		/// <param name="item">The object to be added.</param>
		protected override void AddItem(int index, CommandSource item)
		{
            if(this.Element != null)
			    CommandSourceManager.RegisterCommandSource(item, this.Element);
			base.AddItem(index, item);
		}

		/// <summary>
		/// Unregisters the <see cref="CommandSource"/> object with the <see cref="CommandSourceManager"/> and removes the object to the collection.
		/// </summary>
		/// <param name="index">The index location to add the object to.</param>
		/// <returns>True if the object is removed.</returns>
		protected override bool RemoveItem(int index)
		{
			CommandSourceManager.UnregisterCommandSource(this.Items[index]);
			return base.RemoveItem(index);
		}

		/// <summary>
		/// Unregisters all <see cref="CommandSource"/> objects from the <see cref="CommandSourceManager"/>
		/// </summary>
		protected override void ResetItems()
		{
			foreach (CommandSource target in this.Items)
				CommandSourceManager.UnregisterCommandSource(target);
			base.ResetItems();
		}

		#endregion // Overrides

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