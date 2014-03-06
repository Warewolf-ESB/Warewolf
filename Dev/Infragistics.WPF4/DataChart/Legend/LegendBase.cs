using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Runtime.CompilerServices;


namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents the base class for a legend in XamDataChart.
    /// </summary>
    [Widget]
    public abstract class LegendBase : ContentControl, INotifyPropertyChanged
    {
        internal virtual LegendBaseView CreateView()
        {
            return new LegendBaseView(this);
        }
        internal virtual void OnViewCreated(LegendBaseView view)
        {
            View = view;
        }
        internal LegendBaseView View { get; set; }

        /// <summary>
        /// Creates a new legend instance.
        /// </summary>
        public LegendBase()
        {
            LegendBaseView view = CreateView();
            OnViewCreated(view);
            view.OnInit();

            Children = new ObservableCollection<UIElement>();
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            View.DetachContent();
            ContentPresenter = GetTemplateChild(ContentPresenterName) as ContentPresenter;
            View.OnTemplateProvided();
        }


        /// <summary>
        /// Adds a child, in order, to the legend.
        /// </summary>
        /// <param name="legendItem">The item to add.</param>
        /// <param name="series">The series with which the item is associated.</param>
        protected internal virtual void AddChildInOrder(UIElement legendItem, Series series)
        {}

        /// <summary>
        /// Creates legend items for the legend.
        /// </summary>
        /// <param name="legendItems">The legend items to add.</param>
        /// <param name="itemsHost">The host for the items.</param>
        protected internal virtual void CreateLegendItems(List<UIElement> legendItems, Control itemsHost)
        {}

        #region Children
        /// <summary>
        /// Gets the current Legend object's collection of child UIElements.
        /// <para>
        /// The Children collection is matained automatically by the chart and should not
        /// be modified by the user.
        /// </para>
        /// </summary>
        public ObservableCollection<UIElement> Children { get; private set; }
        #endregion

        #region Owner
        internal object SeriesOwner { get; set; }
        internal object ChartOwner { get; set; }

        /// <summary>
        /// Gets the owner object, if known.
        /// </summary>
        internal object Owner
        {
            get
            {
                if (SeriesOwner != null)
                {
                    return SeriesOwner;
                }
                else
                {
                    return ChartOwner;
                }
            }
        }
        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        [SuppressWidgetMember]
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Occurs when a property value is updated.
        /// </summary>
        [SuppressWidgetMember]
        public event PropertyUpdatedEventHandler PropertyUpdated;
        /// <summary>
        /// Raises the property changed and updated events.
        /// </summary>
        /// <param name="name">The name of the property being changed.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected void RaisePropertyChanged(string name, object oldValue, object newValue)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }

            if (PropertyUpdated != null)
            {
                PropertyUpdated(this, new PropertyUpdatedEventArgs(name, oldValue, newValue));
            }
        }
        #endregion


        /// <summary>
        /// Gets the cureent Legend object's root ContentPresenter.
        /// </summary>
        public ContentPresenter ContentPresenter
        {
            get { return contentPresenter; }
            internal set
            {
                contentPresenter = value;
            }
        }

        internal const string ContentPresenterName = "ContentPresenter";
        private ContentPresenter contentPresenter;


        /// <summary>
        /// Occurs when the left mouse button is pressed while the mouse pointer is over a legend item
        /// in this Legend.
        /// </summary>
        public event DataChartLegendMouseButtonEventHandler LegendItemMouseLeftButtonDown;

        internal void OnLegendItemMouseLeftButtonDown(DataChartLegendMouseButtonEventArgs args)
        {
            if (LegendItemMouseLeftButtonDown != null)
            {
                LegendItemMouseLeftButtonDown(this, args);
            }
        }

        /// <summary>
        /// Occurs when the left mouse button is released while the mouse pointer is over a legend item
        /// in this Legend.
        /// </summary>
        public event DataChartLegendMouseButtonEventHandler LegendItemMouseLeftButtonUp;

        internal void OnLegendItemMouseLeftButtonUp(DataChartLegendMouseButtonEventArgs args)
        {
            if (LegendItemMouseLeftButtonUp != null)
            {
                LegendItemMouseLeftButtonUp(this, args);
            }
        }

        
        /// <summary>
        /// Occurs when the mouse pointer enters a legend item of this legend.
        /// </summary>
        public event DataChartLegendMouseEventHandler LegendItemMouseEnter;

        internal void OnLegendItemMouseEnter(ChartLegendMouseEventArgs args)
        {
            if (LegendItemMouseEnter != null)
            {
                LegendItemMouseEnter(this, args);
            }
        }

        /// <summary>
        /// Occurs when the mouse pointer leaves a legend item of this legend.
        /// </summary>
        public event DataChartLegendMouseEventHandler LegendItemMouseLeave;

        internal void OnLegendItemMouseLeave(ChartLegendMouseEventArgs args)
        {
            if (LegendItemMouseLeave != null)
            {
                LegendItemMouseLeave(this, args);
            }
        }


        /// <summary>
        /// Occurs when the mouse pointer moves over a legend item of this legend.
        /// </summary>
        public event DataChartLegendMouseEventHandler LegendItemMouseMove;

        internal void OnLegendItemMouseMove(ChartLegendMouseEventArgs args)
        {
            if (LegendItemMouseMove != null)
            {
                LegendItemMouseMove(this, args);
            }
        }
        



#region Infragistics Source Cleanup (Region)
















































#endregion // Infragistics Source Cleanup (Region)

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