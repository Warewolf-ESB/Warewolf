using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

using Infragistics.Windows.Helpers;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.Helpers
{
    // JJD 
    /// <summary>
    /// Abstract base class that implements INotifyPropertyChanged on a FrameworkContentElement
    /// </summary>
    public abstract class FrameworkContentElementNotifier : FrameworkContentElement, INotifyPropertyChanged
	{
		#region Base Class Overrides

			#region OnPropertyChanged

		/// <summary>
		/// Called when a property changes.
		/// </summary>
		/// <param name="e">A DependencyPropertyChangedEventArgs instance that contains information about the property that changed.</param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			this.RaisePropertyChangedEvent(e.Property.Name);
		}

			#endregion //OnPropertyChanged

		#endregion //Base Class Overrides	

		#region Protected Methods

			#region RaisePropertyChangedEvent

		/// <summary>
		/// Raises the PropertyChanged for the specified property.
		/// </summary>
		/// <param name="propertyName">The name of the property for which the PropertyChanged event should be raised.</param>
		protected void RaisePropertyChangedEvent(string propertyName)
		{
			if (this.PropertyChanged != null)
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

			#endregion //RaisePropertyChangedEvent	
        
			#region Reset

		/// <summary>
		/// Resets all properties to their default values.
		/// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void Reset()
        {
            List<DependencyProperty> properties = new List<DependencyProperty>();
            LocalValueEnumerator lve = this.GetLocalValueEnumerator();
            while (lve.MoveNext())
            {
                DependencyProperty dp = lve.Current.Property;

                if ( dp.ReadOnly )
                    continue;

                properties.Add(dp);
            }

            foreach(DependencyProperty dp in properties )
                this.ClearValue(dp);
        }

			#endregion //Reset

			#region ShouldSerialize

		/// <summary>
		/// Returns whether this object should be serialized.
		/// </summary>
		/// <returns>Returns true if the value of any property on this object is set to a non-default value, otherwise returns false.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual bool ShouldSerialize()
        {
            LocalValueEnumerator enumerator = this.GetLocalValueEnumerator();

            while (enumerator.MoveNext())
            {
                LocalValueEntry entry = enumerator.Current;

                if (!Object.Equals(entry.Property.DefaultMetadata.DefaultValue, entry.Value))
                    return true;
            }

            return false;
        }

			#endregion //ShouldSerialize

		#endregion Protected Methods

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Fired when the value of a property changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
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